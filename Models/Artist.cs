using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace localMusicPlayerTest.Models
{
    public class Artist : ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private List<Album> _albums;
        public List<Album> Albums
        {
            get => _albums;
            set
            {
                _albums = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage _picture;
        public BitmapImage Picture
        {
            get => _picture;
            set
            {
                _picture = value;
                OnPropertyChanged();
            }
        }

        public Album? GetAlbum(string name)
        {
            foreach (Album album in this.Albums)
            {
                if (album.Title == name)
                {
                    return album;
                }
            }
            return null;
        }
    }
}