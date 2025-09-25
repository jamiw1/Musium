using localMusicPlayerTest.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace localMusicPlayerTest.Services
{
    class AudioService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private MediaPlayer _mediaPlayer;

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

        public void PlaySong(Song song)
        {
            _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("file:///" + song.FilePath));
            _mediaPlayer.Play();
            
            CurrentSongPlaying = song;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
