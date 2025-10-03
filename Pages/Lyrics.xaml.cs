using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Musium.Models;
using Musium.Popups;
using Musium.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Musium.Pages
{
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
                case ContentDialogResult.Primary: // online
                    Song currentSong = Audio.CurrentSongPlaying;
                    var artist = WebUtility.UrlEncode(currentSong.ArtistName);
                    var track = WebUtility.UrlEncode(currentSong.Title);
                    var album = WebUtility.UrlEncode(currentSong.Album.Title);
                    var duration = currentSong.Duration.TotalSeconds;

                    var requestUrl = $"get?artist_name={artist}&track_name={track}&album_name={album}&duration={duration}";

                    LyricResult? response = null;
                    HttpStatusCode? code = null;
                    try
                    {
                        response = await App.LyricHttpClient.GetFromJsonAsync<LyricResult?>(requestUrl);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"api request for lyrics failed: {ex.StatusCode}");
                        code = ex.StatusCode;
                    }

                    if (response?.plainLyrics is string lyrics)
                    {
                        ContentDialog confirmDialog = new ContentDialog();

                        confirmDialog.XamlRoot = XamlRoot;
                        confirmDialog.Title = "Are these correct?";
                        confirmDialog.PrimaryButtonText = "Yes, use these";
                        confirmDialog.CloseButtonText = "No";
                        confirmDialog.DefaultButton = ContentDialogButton.Primary;
                        confirmDialog.Content = new ConfirmLyricsPopup();
                        ConfirmLyricsPopup content = (ConfirmLyricsPopup)confirmDialog.Content;
                        content.SetContent(lyrics);

                        var confirmResult = await confirmDialog.ShowAsync();
                        switch (confirmResult)
                        {
                            case ContentDialogResult.None:
                                break;
                            case ContentDialogResult.Primary:
                                if (Audio.CurrentSongPlaying == null) return;
                                if (dialog.Content is AddLyricsPopup popup)
                                {
                                    Audio.CurrentSongPlaying.Lyrics = lyrics;
                                    if (!popup.SessionChecked) Audio.CurrentSongPlaying.ApplyLyricsToFile();
                                }
                                break;
                            case ContentDialogResult.Secondary:
                                break;
                            default:
                                break;
                        }
                    } else
                    {
                        ContentDialog confirmDialog = new ContentDialog();

                        confirmDialog.XamlRoot = XamlRoot;
                        confirmDialog.Title = code switch
                        {
                            HttpStatusCode.NotFound => "Lyrics unavailable",

                            HttpStatusCode.RequestTimeout => "No connection",
                            HttpStatusCode.ServiceUnavailable => "No connection",
                            HttpStatusCode.GatewayTimeout => "No connection",

                            _ => "Lyrics unavailable"
                        };
                        confirmDialog.CloseButtonText = ":(";
                        confirmDialog.DefaultButton = ContentDialogButton.Close;
                        confirmDialog.Content = new ConfirmLyricsPopup();
                        ConfirmLyricsPopup content = (ConfirmLyricsPopup)confirmDialog.Content;

                        await confirmDialog.ShowAsync();
                    }

                    break;
                case ContentDialogResult.Secondary: // clipboard
                    var package = Clipboard.GetContent();
                    if (package.Contains(StandardDataFormats.Text))
                    {
                        if (Audio.CurrentSongPlaying == null) return;
                        var text = await package.GetTextAsync();
                        Audio.CurrentSongPlaying.Lyrics = text;
                        if (dialog.Content is AddLyricsPopup popup)
                        {
                            if (!popup.SessionChecked) Audio.CurrentSongPlaying.ApplyLyricsToFile();
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
