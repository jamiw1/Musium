using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Musium.Controls
{
    public sealed partial class AlbumItemControl : UserControl
    {
        public AlbumItemControl()
        {
            InitializeComponent();
        }
        public string AlbumTitle
        {
            get { return (string)GetValue(AlbumTitleProperty); }
            set { SetValue(AlbumTitleProperty, value); }
        }

        public static readonly DependencyProperty AlbumTitleProperty =
            DependencyProperty.Register(
                "AlbumTitle",
                typeof(string),
                typeof(AlbumItemControl),
                new PropertyMetadata("Default Title", new PropertyChangedCallback(OnAlbumTitleChanged))
            );
        private static void OnAlbumTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AlbumItemControl control = d as AlbumItemControl;
            if (control != null)
            {
                control.albumTitleText.Text = e.NewValue as string;
            }
        }
        public string ArtistName
        {
            get { return (string)GetValue(ArtistNameProperty); }
            set { SetValue(ArtistNameProperty, value); }
        }

        public static readonly DependencyProperty ArtistNameProperty =
            DependencyProperty.Register(
                "ArtistName",
                typeof(string),
                typeof(AlbumItemControl),
                new PropertyMetadata("Default Artist", new PropertyChangedCallback(OnArtistNameChanged))
            );
        private static void OnArtistNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AlbumItemControl control = d as AlbumItemControl;
            if (control != null)
            {
                control.artistNameText.Text = e.NewValue as string;
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                "Source",
                typeof(ImageSource),
                typeof(AlbumItemControl),
                new PropertyMetadata("ms-appx:///Assets/Placeholder.png", new PropertyChangedCallback(OnSourceChanged))
            );
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AlbumItemControl control = d as AlbumItemControl;
            if (control != null)
            {
                control.albumArtImage.Source = e.NewValue as ImageSource;
            }
        }
        private void LayoutRoot_Tapped(object sender, TappedRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("album tapped!");
        }
    }
}
