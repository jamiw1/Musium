using System;
using Microsoft.UI.Xaml.Data;

namespace localMusicPlayerTest.Converters
{
    public class GetLosslessGlyph : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool lossless)
            {
                return (lossless ? "\uf61f" : "\u0020"); 
            }
            return "\u0020";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}