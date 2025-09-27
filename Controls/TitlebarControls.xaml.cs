using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Musium.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Musium.Controls
{
    public sealed partial class TitlebarControls : UserControl
    {
        public readonly AudioService Audio = AudioService.Instance;
        public TitlebarControls()
        {
            InitializeComponent();
        }

        private void Rewind_Click(object sender, RoutedEventArgs e)
        {
            Audio.PreviousSong();
        }
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Audio.TogglePlayback();
        }
        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            Audio.NextSong();
        }
    }
}
