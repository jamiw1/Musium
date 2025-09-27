using Musium.Controls;
using Musium.Models;
using Musium.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Devices.Radios;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using System.Threading.Tasks;

namespace Musium.Pages;

public sealed partial class NowPlaying : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public AudioService Audio { get; } = AudioService.Instance;
    private bool _mouseDown;

    public float CurrentTimeStamp { get; set; }
    private BitmapImage? _displayedCoverArt;
    public BitmapImage? DisplayedCoverArt
    {
        get => _displayedCoverArt;
        private set
        {
            _displayedCoverArt = value;
            OnPropertyChanged();
        }
    }

    private double _songProgressValue = 0;
    public double SongProgressValue
    {
        get => _songProgressValue;
        private set
        {
            _songProgressValue = value;
            OnPropertyChanged();
        }
    }

    public NowPlaying()
    {
        InitializeComponent();

        Audio.PositionChanged += Audio_PositionChanged;

        this.Unloaded += (s, e) =>
        {
            Audio.PositionChanged -= Audio_PositionChanged;
        };
    }
    private async Task LoadCoverArt()
    {
        if (Audio.CurrentSongPlaying is Song currentSong)
        {
            if (currentSong.Album?.CoverArtData is byte[] imageData)
            {
                var stream = new InMemoryRandomAccessStream();
                await stream.WriteAsync(imageData.AsBuffer());
                stream.Seek(0);

                var bitmapImage = new BitmapImage();
                bitmapImage.DecodePixelWidth = 320;
                await bitmapImage.SetSourceAsync(stream);

                DisplayedCoverArt = bitmapImage;
            } else
            {
                DisplayedCoverArt = null;
            }
        }
    }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Audio.PropertyChanged += Audio_PropertyChanged;
        await LoadCoverArt();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        Audio.PropertyChanged -= Audio_PropertyChanged;
    }

    private void Audio_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Audio.CurrentSongPlaying))
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await LoadCoverArt();
            });
        }
    }
    private void Audio_PositionChanged(object sender, TimeSpan newPos)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            TimeElapsed.Text = $"{newPos:m\\:ss}";
            SongProgressValue = newPos.TotalSeconds;
        });
    }
    private void ShuffleButton_Click(object sender, RoutedEventArgs e)
    {
        Audio.ToggleShuffle();
    }
    private void RewindButton_Click(object sender, RoutedEventArgs e)
    {
        Audio.PreviousSong();
    }


    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        switch (Audio.CurrentState)
        {
            case MediaPlayerState.Closed:
                break;
            case MediaPlayerState.Opening:
                break;
            case MediaPlayerState.Buffering:
                Audio.Pause();
                break;
            case MediaPlayerState.Playing:
                Audio.Pause();
                break;
            case MediaPlayerState.Paused:
                Audio.Resume();
                break;
            case MediaPlayerState.Stopped:
                break;
            default:
                break;
        }
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
        Audio.NextSong();
    }
    private void RepeatButton_Click(object sender, RoutedEventArgs e)
    {
        Audio.CycleRepeat();
    }

    private void ProgressSlider_Loaded(object sender, RoutedEventArgs e)
    {
        ProgressSlider.AddHandler(PointerPressedEvent, new PointerEventHandler(ProgressSlider_PointerPressed), true);
        ProgressSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(ProgressSlider_PointerReleased), true);
    }

    private bool _isUserDragging = false;

    private void ProgressSlider_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isUserDragging = true;
        Audio.Pause();
    }

    private void ProgressSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_isUserDragging)
        {
            Audio.ScrubTo((int)Math.Ceiling(e.NewValue));
        }
    }

    private void ProgressSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isUserDragging = false;
        var slider = sender as Slider;
        Audio.ScrubTo((int)Math.Ceiling(ProgressSlider.Value));
        Audio.Resume();
    }
}
