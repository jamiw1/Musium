using System;
using Microsoft.UI.Xaml.Data;

namespace Musium.Converters
{
    public class BoolToFavoriteStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool favorited)
            {
                return (favorited ? "Unfavorite" : "Favorite"); 
            }
            return "Favorite";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}