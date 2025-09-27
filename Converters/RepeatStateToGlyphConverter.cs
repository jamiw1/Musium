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
    public class RepeatStateToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is RepeatState state)
            {
                switch (state)
                {
                    case RepeatState.Off:
                        return "\uF5E7";
                        break;
                    case RepeatState.Repeat:
                        return "\uE8EE";
                        break;
                    case RepeatState.RepeatOne:
                        return "\uE8ED";
                        break;
                    default:
                        return "\uF5E7";
                        break;
                }
            }
            return "\uF5E7";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
