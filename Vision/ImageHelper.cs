using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
  }
}
