using Microsoft.UI.Xaml.Media.Imaging;
using Musium.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Radios;
using Windows.Media.Core;

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

        private string _artistName;
        public string ArtistName
        {
            get => _artistName;
            set
            {
                _artistName = value;
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

        private string? _lyrics;
        public string? Lyrics
        {
            get => _lyrics;
            set
            {
                _lyrics = value;
                OnPropertyChanged();
            }
        }
        public IOException? AttemptApplyLyricsToFile(string lyrics)
        {
            try
            {
                using (var file = TagLib.File.Create(FilePath))
                {
                    file.Tag.Lyrics = lyrics;
                    file.Save();
                    Lyrics = lyrics;
                }

                Debug.WriteLine("lyrics saved successfully.");
                return null;
            }
            catch (IOException ex)
            {
                return ex;
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
        private bool? _favorited;
        public bool Favorited
        {
            get => _favorited ?? false;
            set
            {
                bool isInitialSet = !_favorited.HasValue;
                if (_favorited == value) return;

                _favorited = value;
                OnPropertyChanged();

                if (isInitialSet) return;
                ApplyFavorited();
            }
        }
        public void ApplyFavorited()
        {
            using var file = TagLib.File.Create(FilePath);
            var loved = Favorited ? "L" : "O";

            var id3v2tag = file.GetTag(TagLib.TagTypes.Id3v2) as TagLib.Id3v2.Tag;
            if (id3v2tag != null)
            {
                var frames = id3v2tag.GetFrames<TagLib.Id3v2.UserTextInformationFrame>();
                var id3v2Loved = frames.FirstOrDefault(frame => frame.Description == "LOVE RATING");

                if (id3v2Loved == null)
                {
                    var newFrame = new TagLib.Id3v2.UserTextInformationFrame("LOVE RATING");
                    newFrame.Text = [loved];
                    id3v2tag.AddFrame(newFrame);
                }
                else
                {
                    id3v2Loved.Text = [loved];
                }
            }

            var xiphcommenttag = file.GetTag(TagLib.TagTypes.Xiph) as TagLib.Ogg.XiphComment;
            if (xiphcommenttag != null)
            {
                xiphcommenttag.SetField("LOVE RATING", loved);
            }

            var mp4tag = file.GetTag(TagLib.TagTypes.Apple) as TagLib.Mpeg4.AppleTag;
            if (mp4tag != null)
            {
                mp4tag.SetDashBox("com.apple.iTunes", "LOVERATING", loved);
            }

            file.Save();
            //Debug.WriteLine("Applied favorite (" + Favorited.ToString() + ") to song " + Title);
        }
        public bool RetrieveFavorited()
        {
            using (var file = TagLib.File.Create(FilePath))
            {
                var id3v2tag = file.GetTag(TagLib.TagTypes.Id3v2);
                var xiphcommenttag = file.GetTag(TagLib.TagTypes.Xiph);
                var mp4tag = file.GetTag(TagLib.TagTypes.Apple);

                if (id3v2tag is TagLib.Id3v2.Tag id3v2Tag)
                {
                    var frames = id3v2Tag.GetFrames<TagLib.Id3v2.UserTextInformationFrame>("TXXX");
                    var id3v2Loved = frames.FirstOrDefault(frame => frame.Description == "LOVE RATING");
                    return id3v2Loved != null;
                }
                if (xiphcommenttag is TagLib.Ogg.XiphComment xiphcommentTag)
                {
                    return xiphcommentTag.GetField("LOVE RATING").FirstOrDefault() == "L";
                }
                if (mp4tag is TagLib.Mpeg4.AppleTag mp4Tag)
                {
                    return mp4Tag.GetDashBox("com.apple.iTunes", "LOVERATING") == "L";
                }
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