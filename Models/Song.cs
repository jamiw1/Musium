using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TagLib.Id3v2;

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

        private bool _favorited = false;
        public bool Favorited
        {
            get => _favorited;
            set
            {
                _favorited = value;
                OnPropertyChanged();
            }
        }
        public bool RetrieveFavorited()
        {
            var file = TagLib.File.Create(FilePath);
            if (file == null) return false;

            var tag = file.Tag;

            switch (tag)
            {
                case TagLib.Id3v2.Tag id3v2tag:
                    var frames = id3v2tag.GetFrames<UserTextInformationFrame>();
                    var favoritedFrame = frames.FirstOrDefault(frame => frame.Description == "LOVE RATING L");
                    if (favoritedFrame == null) return false;
                    return true;
                default:
                    return false;
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