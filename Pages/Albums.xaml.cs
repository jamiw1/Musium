using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Musium.Controls;
using Musium.Models;
using Musium.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Radios;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Musium.Pages
{
    public sealed partial class Albums : Page
    {
        public readonly AudioService Audio = AudioService.Instance;
        public ObservableCollection<Album> AllAlbums { get; } = new ObservableCollection<Album>();

        public Albums()
        {
            InitializeComponent();
            Loaded += Albums_Loaded;
        }
        private async void Albums_Loaded(object sender, RoutedEventArgs e)
        {
            var allAlbumsData = await Audio.GetAllAlbumsAsync();

            AllAlbums.Clear();
            foreach (Album album in allAlbumsData)
            {
                AllAlbums.Add(album);
            }
        }
        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedAlbum = e.ClickedItem as Album;

            if (clickedAlbum != null)
            {
                Audio.CurrentViewedAlbum = clickedAlbum;
                Frame.Navigate(typeof(InnerAlbum));
                MainWindow.UpdateNavigationViewSelection(typeof(InnerAlbum));
            }
        }
    }
}
