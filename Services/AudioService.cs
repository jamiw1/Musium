using Musium.Models;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Devices.Radios;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace Musium.Services
{
    public class AudioService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DispatcherQueue dispatcherQueue;

        private MediaPlayer _mediaPlayer;
        public event EventHandler<TimeSpan> PositionChanged;

        private AudioService()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.PlaybackSession.PositionChanged += OnPlaybackSessionChanged;
            _mediaPlayer.CurrentStateChanged += OnCurrentStateChanged;
        }

        private List<Artist> Database = new List<Artist>();
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

        public List<Song> GetAllTracks()
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
                Lossless = IsLossless(path)
            };

            album.Songs.Add(song);

            var pic = tfile.Tag.Pictures.ElementAtOrDefault(0);
            if (pic != null)
            {
                album.CoverArtData = pic.Data.ToArray();
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
            ScanDirectoryIntoLibrary(targetDirectory);
        }

        public async void ScanDirectoryIntoLibrary(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                await AddSongFromFile(fileName);

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ScanDirectoryIntoLibrary(subdirectory);
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
