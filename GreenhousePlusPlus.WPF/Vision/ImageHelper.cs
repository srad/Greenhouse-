using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace Greenhouse.Vision
{
  public static class ImageHelper
  {
    /// <summary>
    /// Loading images with this function has the benefit that it doesn't lock
    /// files in the disk bit only reads the file into memory, so that can be
    /// deleted from disk without an error.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
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
