using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Media.Playback;

namespace Musium.Converters
{
    public class PlaybackStateIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var symbolToUse = Symbol.Play;

            if (value is MediaPlayerState state)
            {
                symbolToUse = state switch
                {
                    MediaPlayerState.Playing => Symbol.Pause,
                    _ => Symbol.Play,
                };
            }

            return new SymbolIcon(symbolToUse);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}