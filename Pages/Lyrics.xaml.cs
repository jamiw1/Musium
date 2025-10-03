using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Musium.Popups;
using Musium.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Musium.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Lyrics : Page
    {
        public readonly AudioService Audio = AudioService.Instance;
        public Lyrics()
        {
            InitializeComponent();
        }

        private async void AddLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = XamlRoot;
            dialog.Title = "Add lyrics";
            dialog.PrimaryButtonText = "Online";
            dialog.SecondaryButtonText = "Clipboard";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new AddLyricsPopup();

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.None:
                    break;
                case ContentDialogResult.Primary:
                    break;
                case ContentDialogResult.Secondary:
                    var package = Clipboard.GetContent();
                    if (package.Contains(StandardDataFormats.Text))
                    {
                        if (Audio.CurrentSongPlaying == null) return;
                        var text = await package.GetTextAsync();
                        Audio.CurrentSongPlaying.Lyrics = text;
                        Audio.CurrentSongPlaying.ApplyLyricsToFile();
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
