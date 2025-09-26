using System;
using Microsoft.UI.Xaml.Data;

namespace Musium.Converters
{
    public class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan duration)
            {
                return $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}";
            }
            return "0:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}