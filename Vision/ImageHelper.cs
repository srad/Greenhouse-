using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Greenhouse.Vision
{
  public static class ImageHelper
  {
    public static BitmapImage Resize(string path, int width, int height)
    {
      BitmapImage source = new BitmapImage();

      source.BeginInit();
      source.UriSource = new Uri(path);
      source.DecodePixelHeight = height;
      source.DecodePixelWidth = width;
      source.EndInit();

      return source;
    }

    public static void Save(this BitmapImage image, string filePath)
    {
      BitmapEncoder encoder = new PngBitmapEncoder();
      encoder.Frames.Add(BitmapFrame.Create(image));

      using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
      {
        encoder.Save(fileStream);
      }
    }

    public static BitmapImage LoadBitmap(string file)
    {
      var bitmap = new BitmapImage();
      var stream = File.OpenRead(file);

      bitmap.BeginInit();
      bitmap.CacheOption = BitmapCacheOption.OnLoad;
      bitmap.StreamSource = stream;
      bitmap.EndInit();
      stream.Close();
      stream.Dispose();
      bitmap.Freeze();

      return bitmap;
    }

    public static RGBArray HistogramFromBitmap(Bitmap bmp)
    {
      var rgb = new RGBArray();
      // Lock the bitmap's bits.  
      Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
      BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

      // Get the address of the first line.
      IntPtr ptr = bmpData.Scan0;

      // Declare an array to hold the bytes of the bitmap.
      int bytes = bmpData.Stride * bmp.Height;
      byte[] rgbValues = new byte[bytes];

      // Copy the RGB values into the array.
      Marshal.Copy(ptr, rgbValues, 0, bytes);

      int stride = bmpData.Stride;

      for (int column = 0; column < bmpData.Height; column++)
      {
        for (int row = 0; row < bmpData.Width; row++)
        {
          var blue = (byte)(rgbValues[(column * stride) + (row * 3)]);
          var green = (byte)(rgbValues[(column * stride) + (row * 3) + 1]);
          var red = (byte)(rgbValues[(column * stride) + (row * 3) + 2]);
          rgb.B[blue]++;
          rgb.G[green]++;
          rgb.R[red]++;
        }
      }

      return rgb;
    }
  }
}
