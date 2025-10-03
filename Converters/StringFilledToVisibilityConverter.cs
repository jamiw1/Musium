using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Diagnostics;

namespace Musium.Converters
{
    public class StringFilledToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string s)
            {
                bool isFilled = !string.IsNullOrEmpty(s) || !string.IsNullOrWhiteSpace(s);
                
                if (parameter is string param && param.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
                {
                    isFilled = !isFilled;
                }
                if (isFilled) Debug.WriteLine(s + " - should be lyrics before");

                return isFilled ? Visibility.Visible : Visibility.Collapsed;
            }

            if (parameter is string par && par.Equals("Inverse", StringComparison.OrdinalIgnoreCase)) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}

    