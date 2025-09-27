using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Musium.Models
{
    public class Song : ObservableObject
    {
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

        private Album _album;
        public Album Album
        {
            get => _album;
            set
            {
                _album = value;
                OnPropertyChanged();
            }
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        private string? _genre;
        public string? Genre
        {
            get => _genre;
            set
            {
                _genre = value;
                OnPropertyChanged();
            }
        }

        private int? _trackNumber;
        public int? TrackNumber
        {
            get => _trackNumber;
            set
            {
                _trackNumber = value;
                OnPropertyChanged();
            }
        }

        private bool _favorited;
        public bool Favorited
        {
            get => _favorited;
            set
            {
                _favorited = value;
                OnPropertyChanged();
            }
        }

        private bool _lossless;
        public bool Lossless
        {
            get => _lossless;
            set
            {
                _lossless = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }
    }
}