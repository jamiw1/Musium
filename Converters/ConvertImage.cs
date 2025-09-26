using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

public static class ConvertImage
{
    public static async Task<BitmapImage> ToBitmapImage(this TagLib.IPicture picture)
    {
        if (picture == null || picture.Data.Count == 0)
        {
            return null;
        }

        using (var stream = new InMemoryRandomAccessStream())
        {
            await stream.WriteAsync(picture.Data.ToArray().AsBuffer());

            stream.Seek(0);

            var bitmapImage = new BitmapImage();
            bitmapImage.DecodePixelWidth = 320;
            bitmapImage.DecodePixelHeight = 320;
            await bitmapImage.SetSourceAsync(stream);

            return bitmapImage;
        }
    }
}