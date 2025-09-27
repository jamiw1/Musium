using Musium.Models;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TagLib;
using Windows.Media.Core;
using Windows.Media.Playback;
using System.Diagnostics;

namespace Musium.Services
{
    public class AudioService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DispatcherQueue dispatcherQueue;

        private MediaPlayer _mediaPlayer;
        public event EventHandler<TimeSpan> PositionChanged;

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
        public void ToggleShuffle()
        {
            CurrentShuffleState = CurrentShuffleState == ShuffleState.Off ? ShuffleState.Shuffle : ShuffleState.Off;
        }

        private AudioService()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.PlaybackSession.PositionChanged += OnPlaybackSessionChanged;
            _mediaPlayer.CurrentStateChanged += OnCurrentStateChanged;
        }

        private List<Artist> Database = new List<Artist>();
        private List<Song> Queue = new List<Song>();
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

        private Song _currentSongPlaying;
        public Song CurrentSongPlaying
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

        public void PlaySong(Song song)
        {
            _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("file:///" + song.FilePath));
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
                    Albums = new List<Album>()
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
                    Songs = new List<Song>()
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
            return song;
        }
        private bool IsLossless(string path)
        {
            var extension = System.IO.Path.GetExtension(path).ToLower();
            if (!audioExtensions.Contains(extension))
            {
                return false;
            }
            return losslessAudioExtensions.Contains(extension);
        }

        public async void SetLibrary(string targetDirectory)
        {
            Database.Clear();
            await ScanDirectoryIntoLibrary(targetDirectory);
        }

        public async Task ScanDirectoryIntoLibrary(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                await AddSongFromFile(fileName);

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                await ScanDirectoryIntoLibrary(subdirectory);
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
