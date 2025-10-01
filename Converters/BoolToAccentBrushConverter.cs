using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Musium.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Musium.Converters
{
    public class BoolToAccentBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool state && state == true)
            {
                return Application.Current.Resources["AccentFillColorDefaultBrush"] as SolidColorBrush;
            }
            else
            {
                return Application.Current.Resources["TextFillColorPrimaryBrush"] as SolidColorBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
