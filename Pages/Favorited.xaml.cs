using Musium.Controls;
using Musium.Models;
using Musium.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;

namespace Musium.Pages
{
    public sealed partial class Favorited : Page
    {
        public readonly AudioService Audio = AudioService.Instance;
        public ObservableCollection<Song> AllFavoritedTracks { get; } = new ObservableCollection<Song>();
        public Favorited()
        {
            InitializeComponent();
            Loaded += Favorited_Loaded;
        }

        private async void Favorited_Loaded(object sender, RoutedEventArgs e)
        {
            var allTracksData = await Audio.GetAllTracksAsync();

            AllFavoritedTracks.Clear();
            foreach (Song track in allTracksData)
            {
                if (track.Favorited) AllFavoritedTracks.Add(track);
            }
        }

        private async void TrackItemControl_Clicked(object sender, RoutedEventArgs e)
        {
            var clickedControl = sender as TrackItemControl;
            if (clickedControl != null)
            {
                await Audio.StartQueueFromSongAsync(clickedControl.Song, true);
                Frame.Navigate(typeof(NowPlaying));
                MainWindow.UpdateNavigationViewSelection(typeof(NowPlaying));
            }
        }
    }
}
