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
      float originalHeight = 0, originalWidth = 0;
      using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
      {
        using (Image tif = Image.FromStream(stream: file, useEmbeddedColorManagement: false, validateImageData: false))
        {
          originalHeight = tif.PhysicalDimension.Height;
          originalWidth = tif.PhysicalDimension.Width;
        }
      }
      var ratioX = (double)width / originalWidth;
      var ratioY = (double)height / originalHeight;
      var ratio = Math.Min(ratioX, ratioY);

      var newWidth = (int)(originalWidth * ratio);
      var newHeight = (int)(originalHeight * ratio);

      BitmapImage newBitmap = new BitmapImage();

      newBitmap.BeginInit();
      newBitmap.UriSource = new Uri(path);
      newBitmap.DecodePixelHeight = newHeight;
      newBitmap.DecodePixelWidth = newWidth;
      newBitmap.EndInit();

      return newBitmap;
    }

    public static void Save(this BitmapImage image, string filePath)
    {
      BitmapEncoder encoder = new JpegBitmapEncoder();
      encoder.Frames.Add(BitmapFrame.Create(image));

      using (var fileStream = new FileStream(filePath, FileMode.Create))
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
