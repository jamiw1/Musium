using localMusicPlayerTest.Models;
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

namespace localMusicPlayerTest.Services
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

        public async Task<Song> AddSongFromFile(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new FileNotFoundException("path '" + path + "' is invalid");
            }
            var tfile = TagLib.File.Create(path);
            if (!tfile.Properties.MediaTypes.Equals(MediaTypes.Audio))
            {
                return null;
            }
            Song song = new Song();
            song.Title = tfile.Tag.Title;
            var artistName = tfile.Tag.FirstPerformer;
            var albumName = tfile.Tag.Album;
            var artist = GetArtist(artistName);
            if (artist == null)
            {
                artist = new Artist();
                artist.Name = artistName;
                artist.Albums = new List<Album>();
                Database.Add(artist);
            }
            var album = artist.GetAlbum(albumName);
            if (album == null)
            {
                album = new Album();
                album.Title = albumName;
                album.Artist = artist;
                album.Songs = new List<Song>();
                artist.Albums.Add(album);
            }

            song.Album = album;
            song.FilePath = path;
            song.Genre = tfile.Tag.FirstGenre;
            song.Duration = tfile.Properties.Duration;

            album.Songs.Add(song);

            var pic = tfile.Tag.Pictures.ElementAtOrDefault(0);

            if (pic != null)
            {
                byte[] imageData = pic.Data.ToArray();
                album.CoverArtData = imageData;
            }

            return song;
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
