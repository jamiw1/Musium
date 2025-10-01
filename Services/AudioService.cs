using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.VisualBasic;
using Musium.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using TagLib.Riff;
using Windows.Devices.Radios;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Musium.Services
{
    public enum RepeatState
    {
        Off,
        Repeat,
        RepeatOne
    }
    public enum ShuffleState
    {
        Off,
        Shuffle
    }
    public class AudioService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DispatcherQueue dispatcherQueue;

        private MediaPlayer _mediaPlayer;
        public event EventHandler<TimeSpan> PositionChanged;

        private readonly Random _rng = new Random();

        private RepeatState _currentRepeatState = RepeatState.Off;
        public RepeatState CurrentRepeatState
        {
            get => _currentRepeatState;
            private set
            {
                _currentRepeatState = value;
                OnPropertyChanged();
            }
        }

        private ShuffleState _currentShuffleState = ShuffleState.Off;
        public ShuffleState CurrentShuffleState
        {
            get => _currentShuffleState;
            private set
            {
                _currentShuffleState = value;
                OnPropertyChanged();
            }
        }
        public void CycleRepeat()
        {
            switch (CurrentRepeatState)
            {
                case RepeatState.Off:
                    CurrentRepeatState = RepeatState.Repeat;
                    break;
                case RepeatState.Repeat:
                    CurrentRepeatState = RepeatState.RepeatOne;
                    break;
                case RepeatState.RepeatOne:
                    CurrentRepeatState = RepeatState.Off;
                    break;
                default:
                    CurrentRepeatState = RepeatState.Repeat;
                    break;
            }
        }
        private AudioService()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.PlaybackSession.PositionChanged += OnPlaybackSessionChanged;
            _mediaPlayer.CurrentStateChanged += OnCurrentStateChanged;
            _mediaPlayer.MediaEnded += OnMediaEnded;
        }

        private List<Artist> Database = new List<Artist>();
        public ObservableCollection<Song> Queue = new ObservableCollection<Song>();
        private List<Song> _fullCurrentSongList = new List<Song>();
        public List<Song> History = new List<Song>();

        public void PlaySongList(List<Song> inputSongList, Song startingSong)
        {
            bool shuffled = CurrentShuffleState == ShuffleState.Shuffle;
            var songList = new List<Song>(inputSongList);
            songList.Remove(startingSong);

            _fullCurrentSongList = inputSongList;

            if (shuffled) shuffleSongList(songList);
            ReplaceQueueWithList(songList);
            PlaySong(startingSong);
        }
        private void shuffleSongList(List<Song> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int k = _rng.Next(i + 1);
                var temp = list[i];
                list[i] = list[k];
                list[k] = temp;
            }
        }
        private void ReplaceQueueWithList(List<Song> list)
        {
            Queue.Clear();
            foreach (Song song in list)
            {
                Queue.Add(song);
            }
        }
        public void ToggleShuffle()
        {
            CurrentShuffleState = CurrentShuffleState == ShuffleState.Off ? ShuffleState.Shuffle : ShuffleState.Off;
            ShuffleLogic();
        }

        public void SetShuffle(ShuffleState newState)
        {
            CurrentShuffleState = newState;
            ShuffleLogic();
        }
        private void ReplaceQueueWithCurrentUnshuffled()
        {
            int index = _fullCurrentSongList.FindIndex(s => s == CurrentSongPlaying);

            if (index == -1) return;

            int startIndex = index + 1;
            if (startIndex <= _fullCurrentSongList.Count)
            {
                int count = _fullCurrentSongList.Count - startIndex;
                ReplaceQueueWithList(_fullCurrentSongList.GetRange(startIndex, count));
            }
        }

        private void ShuffleLogic()
        {
            var songList = new List<Song>(_fullCurrentSongList);
            if (CurrentShuffleState == ShuffleState.Shuffle)
            {
                shuffleSongList(songList);
                songList.Remove(CurrentSongPlaying);
                ReplaceQueueWithList(songList);
            }
            else
            {
                ReplaceQueueWithCurrentUnshuffled();
            }
        }

        private Artist? GetArtist(string name)
        {
            foreach (Artist artist in Database)
            {
                if (artist.Name == name)
                {
                    return artist;
                }
            }
            return null;
        }

        public async Task<List<Song>> GetAllTracksAsync()
        {
            return await Task.Run(() =>
            {
                List<Song> songs = new List<Song>();
                foreach (Artist artist in Database)
                {
                    foreach (Album album in artist.Albums)
                    {
                        foreach (Song song in album.Songs)
                        {
                            songs.Add(song);
                        }
                    }
                }
                return songs;
            });
        }

        public async Task<List<Album>> GetAllAlbumsAsync()
        {
            return await Task.Run(() =>
            {
                List<Album> albums = new List<Album>();
                foreach (Artist artist in Database)
                {
                    foreach (Album album in artist.Albums)
                    {
                        albums.Add(album);
                    }
                }
                return albums;
            });
        }
        public void SetDispatcherQueue(DispatcherQueue newdq)
        {
            dispatcherQueue = newdq;
        }
        

        private void OnPlaybackSessionChanged(MediaPlaybackSession sender, object args)
        {
            PositionChanged?.Invoke(this, sender.Position);
        }
        private void OnCurrentStateChanged(MediaPlayer sender, object args)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                CurrentState = sender.CurrentState;
            });   
        }
        private void OnMediaEnded(MediaPlayer sender, object args)
        {
            NextSong();
        }

        public void TogglePlayback()
        {
            switch (CurrentState)
            {
                case MediaPlayerState.Closed:
                    break;
                case MediaPlayerState.Opening:
                    break;
                case MediaPlayerState.Buffering:
                    Pause();
                    break;
                case MediaPlayerState.Playing:
                    Pause();
                    break;
                case MediaPlayerState.Paused:
                    Resume();
                    break;
                case MediaPlayerState.Stopped:
                    break;
                default:
                    break;
            }
        }

        public void NextSong()
        {
            var currentSong = CurrentSongPlaying;

            if (CurrentRepeatState == RepeatState.RepeatOne && currentSong != null)
            {
                PlaySong(currentSong);
                return;
            }

            var songToPlay = Queue.FirstOrDefault();
            if (songToPlay != null) // there is a song to play next
            {
                PlaySong(songToPlay);

                dispatcherQueue.TryEnqueue(() =>
                {
                    if (currentSong != null)
                    {
                        History.Add(currentSong);
                        if (History.Count > 100) // capped history at 100, dunno if people actually use it past 100 songs so make PR/issue if u do
                        {
                            History.RemoveAt(0);
                        }
                    }
                    Queue.Remove(songToPlay);
                });
            }
            else if (CurrentRepeatState == RepeatState.Repeat) // there is no song to play next and repeat is enabled
            {
                dispatcherQueue.TryEnqueue(() =>
                {
                    var list = _fullCurrentSongList;
                    if (CurrentShuffleState == ShuffleState.Shuffle) shuffleSongList(list);
                    ReplaceQueueWithList(list);

                    var firstSongInNewQueue = Queue.FirstOrDefault();
                    if (firstSongInNewQueue != null)
                    {
                        PlaySong(firstSongInNewQueue);
                        Queue.Remove(firstSongInNewQueue);
                    }
                });
            }
        }
        public void PreviousSong()
        {
            if (_mediaPlayer.PlaybackSession.Position.TotalMilliseconds > 3000 || History.Count <= 0) // rewind goes to previous song if done in under 3 seconds
            {
                ScrubTo(0);
                return;
            }

            var currentSong = CurrentSongPlaying;

            var songToPlay = History.LastOrDefault();
            if (songToPlay != null)
            {
                PlaySong(songToPlay);

                dispatcherQueue.TryEnqueue(() =>
                {
                    History.Remove(songToPlay);
                    if (currentSong != null)
                    {
                        Queue.Insert(0, currentSong);
                    }
                });
            }
        }

        private static AudioService _instance;
        public static AudioService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AudioService();
                }
                return _instance;
            }
        }

        private Song? _currentSongPlaying;
        public Song? CurrentSongPlaying
        {
            get => _currentSongPlaying;
            private set
            {
                _currentSongPlaying = value;
                OnPropertyChanged();
            }
        }

        private MediaPlayerState _currentState;
        public MediaPlayerState CurrentState
        {
            get => _currentState;
            private set
            {
                _currentState = value;
                OnPropertyChanged();
            }
        }

        private Album? _currentViewedAlbum;
        public Album? CurrentViewedAlbum
        {
            get => _currentViewedAlbum;
            set
            {
                if (_currentViewedAlbum != value)
                {
                    _currentViewedAlbum = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Pause()
        {
            _mediaPlayer?.Pause();
        }
        public void Resume()
        {
            _mediaPlayer?.Play();
        }

        public void SetMediaPlayer(MediaPlayerElement element)
        {
            element.SetMediaPlayer(_mediaPlayer);
        }

        private async Task<MediaSource> CreateMediaSourceFromMemoryAsync(string filePath)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            IBuffer buffer = await FileIO.ReadBufferAsync(file);

            var memoryStream = new InMemoryRandomAccessStream();
            await memoryStream.WriteAsync(buffer);

            memoryStream.Seek(0);

            var tagFile = TagLib.File.Create(filePath);
            var mimeType = tagFile.MimeType.ToString();
            return MediaSource.CreateFromStream(memoryStream, mimeType);
        }
        public async void PlaySong(Song song)
        {
            var source = await CreateMediaSourceFromMemoryAsync(song.FilePath);
            _mediaPlayer.Source = source;
            _mediaPlayer.Play();

            CurrentSongPlaying = song;
        }
        public void ScrubTo(int seconds)
        {
            _mediaPlayer.Position = new TimeSpan(0, 0, seconds);
        }

        private static readonly List<string> audioExtensions = new List<string>
        {
            ".mp3",
            ".m4a",
            ".ogg",
            ".opus",
            ".wma",
            ".flac",
            ".alac",
            ".wav",
            ".aiff",
            ".dsd"
        };
        private static readonly List<string> losslessAudioExtensions = new List<string>
        {
            ".flac",
            ".alac",
            ".wav",
            ".aiff",
            ".dsd"
        };
        public async Task<Song> AddSongFromFile(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new FileNotFoundException($"path '{path}' is invalid");
            }

            var extension = System.IO.Path.GetExtension(path).ToLower();
            if (!audioExtensions.Contains(extension))
            {
                return null;
            }

            try
            {
                var tfile = TagLib.File.Create(path);

                if (!tfile.Properties.MediaTypes.Equals(MediaTypes.Audio))
                {
                    return null;
                }

                var artist = GetArtistOrCreate(tfile.Tag.FirstPerformer);
                var album = GetAlbumOrCreate(artist, tfile.Tag.Album);

                var song = ExtractSongData(tfile, path, album);

                return song;
            }
            catch (TagLib.CorruptFileException ex)
            {
                Console.WriteLine($"error with file {path}: {ex.Message}");
                return null;
            }
        }
        private Artist GetArtistOrCreate(string artistName)
        {
            var artist = GetArtist(artistName);
            if (artist == null)
            {
                artist = new Artist
                {
                    Name = artistName,
                    Albums = new ObservableCollection<Album>()
                };
                Database.Add(artist);
            }
            return artist;
        }

        private Album GetAlbumOrCreate(Artist artist, string albumName)
        {
            var album = artist.GetAlbum(albumName);
            if (album == null)
            {
                album = new Album
                {
                    Title = albumName,
                    Artist = artist,
                    Songs = new ObservableCollection<Song>()
                };
                artist.Albums.Add(album);
            }
            return album;
        }

        private Song ExtractSongData(TagLib.File tfile, string path, Album album)
        {
            var song = new Song
            {
                Title = tfile.Tag.Title,
                Album = album,
                FilePath = path,
                Genre = tfile.Tag.FirstGenre,
                Duration = tfile.Properties.Duration,
                TrackNumber = (int)tfile.Tag.Track,
                Lossless = IsLossless(path)
            };
            song.Favorited = song.RetrieveFavorited();

            album.Songs.Add(song);
            album.Songs.OrderBy(song => song.TrackNumber);

            var pic = tfile.Tag.Pictures.ElementAtOrDefault(0);
            if (pic != null)
            {
                album.CoverArtData = pic.Data.ToArray();
            } 
            else
            {
                var parent = Path.GetDirectoryName(path);
                var jpgPath = Path.Combine(parent, "cover.jpg");
                var pngPath = Path.Combine(parent, "cover.png");
                if (System.IO.File.Exists(pngPath))
                {
                    album.CoverArtData = System.IO.File.ReadAllBytes(pngPath);
                }
                else if (System.IO.File.Exists(jpgPath))
                {
                    album.CoverArtData = System.IO.File.ReadAllBytes(jpgPath);
                }
            }
            Task.Run(async () =>
            {
                if (album.CoverArtData is byte[] imageData)
                {
                    byte[] resizedImageData = await ResizeImageAsync(album.CoverArtData, 320);
                    album.CoverArtData = resizedImageData;
                }
            });
            return song;
        }
        private bool IsLossless(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            if (!audioExtensions.Contains(extension))
            {
                return false;
            }
            return losslessAudioExtensions.Contains(extension);
        }

        public async void SetLibrary(string targetDirectory)
        {
            Database.Clear();
            await Task.Run(async () =>
            {
                await ScanDirectoryIntoLibrary(targetDirectory);
            });
        }

        public async Task ScanDirectoryIntoLibrary(string targetDirectory)
        {
            if (!Directory.Exists(targetDirectory)) return;
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                await AddSongFromFile(fileName);

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                await ScanDirectoryIntoLibrary(subdirectory);
        }

        public void PlayAlbum(Song startingSong)
        {
            PlaySongList([.. startingSong.Album.Songs], startingSong);
            if (CurrentShuffleState == ShuffleState.Shuffle) return;
            ReplaceQueueWithCurrentUnshuffled();
        }

        public async Task PlayTrackAsync(Song startingSong, bool favoritesOnly = false)
        {
            var tracks = await GetAllTracksAsync();
            if (favoritesOnly)
            {
                tracks = tracks.Where(song => song.Favorited).ToList();
            }
            PlaySongList(tracks, startingSong);
            if (CurrentShuffleState == ShuffleState.Shuffle) return;
            ReplaceQueueWithCurrentUnshuffled();
        }

        public void InsertStartOfQueue(Song song)
        {
            Queue.Insert(0, song);
        }
        public void InsertEndOfQueue(Song song)
        {
            Queue.Add(song);
        }
        public void RemoveFromQueue(Song song)
        {
            Queue.Remove(song);
        }
        public static async Task<byte[]> ResizeImageAsync(byte[] imageData, uint newWidth)
        {
            var inputStream = new InMemoryRandomAccessStream();
            await inputStream.WriteAsync(imageData.AsBuffer());
            inputStream.Seek(0);

            var decoder = await BitmapDecoder.CreateAsync(inputStream);

            var transform = new BitmapTransform()
            {
                ScaledWidth = newWidth,
                ScaledHeight = (uint)((decoder.PixelHeight / (double)decoder.PixelWidth) * newWidth),
                InterpolationMode = BitmapInterpolationMode.Fant
            };

            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Rgba8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );

            var outputStream = new InMemoryRandomAccessStream();

            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);

            encoder.SetPixelData(
                BitmapPixelFormat.Rgba8,
                BitmapAlphaMode.Straight,
                transform.ScaledWidth,
                transform.ScaledHeight,
                decoder.DpiX,
                decoder.DpiY,
                pixelData.DetachPixelData()
            );

            await encoder.FlushAsync();

            var reader = new DataReader(outputStream.GetInputStreamAt(0));
            var bytes = new byte[outputStream.Size];
            await reader.LoadAsync((uint)outputStream.Size);
            reader.ReadBytes(bytes);

            return bytes;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (dispatcherQueue != null && !dispatcherQueue.HasThreadAccess)
            {
                dispatcherQueue.TryEnqueue(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
