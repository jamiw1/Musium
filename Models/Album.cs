using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Musium.Models
{
    public class Album : ObservableObject
    {
        private ObservableCollection<Song> _songs;
        public ObservableCollection<Song> Songs
        {
            get => _songs;
            set
            {
                _songs = value;
                OnPropertyChanged();
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private Artist _artist;
        public Artist Artist
        {
            get => _artist;
            set
            {
                _artist = value;
                OnPropertyChanged();
            }
        }

        private byte[]? _coverArtData;
        public byte[]? CoverArtData
        {
            get => _coverArtData;
            set
            {
                _coverArtData = value;
                OnPropertyChanged();
            }
        }

        public Song? GetSong(string name)
        {
            foreach (Song song in this.Songs)
            {
                if (song.Title == name)
                {
                    return song;
                }
            }
            return null;
        }
    }
}