using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Musium.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Musium.Controls
{
    public sealed partial class AlbumItemControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public AlbumItemControl()
        {
            InitializeComponent();
        }
        private BitmapImage? _displayedCoverArt;
        public BitmapImage? DisplayedCoverArt
        {
            get => _displayedCoverArt;
            set
            {
                _displayedCoverArt = value;
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty AlbumProperty = DependencyProperty.Register(
            "Album",
            typeof(Album),
            typeof(AlbumItemControl),
            new PropertyMetadata(null, OnAlbumChanged));
        public Album Album
        {
            get => (Album)GetValue(AlbumProperty);
            set => SetValue(AlbumProperty, value);
        }

        private static async void OnAlbumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AlbumItemControl;
            if (control != null && control.Album?.CoverArtData is byte[] imageData)
            {
                var stream = new InMemoryRandomAccessStream();
                await stream.WriteAsync(imageData.AsBuffer());
                stream.Seek(0);

                var bitmapImage = new BitmapImage();
                bitmapImage.DecodePixelWidth = 160;
                await bitmapImage.SetSourceAsync(stream);

                control.DisplayedCoverArt = bitmapImage;
            }
            else if (control != null)
            {
                control.DisplayedCoverArt = null;
            }
        }
    }
}
