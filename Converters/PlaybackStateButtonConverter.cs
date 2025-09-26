using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Media.Playback;

namespace Musium.Converters
{
    public class PlaybackStateButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MediaPlayerState state)
            {
                return state switch
                {
                    MediaPlayerState.Playing => Symbol.Pause,
                    _ => Symbol.Play,
                };
            }
            return Symbol.Play;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}