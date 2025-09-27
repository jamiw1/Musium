using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Musium.Converters
{
    public class BoolGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? new GridLength(1, GridUnitType.Star) : GridLength.Auto;
            }
            return GridLength.Auto;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}

