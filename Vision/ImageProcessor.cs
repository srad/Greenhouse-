using Greenhouse.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;

namespace Greenhouse.Vision
{
  public class ImageProcessor
  {
    private readonly Bitmap Bitmap;
    private readonly ImageFile ImageFile;
    private readonly Action<int> progressCallback;
    private static int progress = 0;

    public ImageProcessor(ImageFile imagefile, Action<int> progressCallback)
    {
      this.ImageFile = imagefile;
      this.progressCallback = progressCallback;
      Bitmap = new Bitmap(imagefile.Original);
    }

    /// <summary>
    /// This implements a multi-threaded image proccessing, otherwise
    /// it takes probably 30-50x longer.
    /// </summary>
    /// <returns></returns>
    public RGBHistogram Start()
    {
      progress = 0;
      var hist = new RGBHistogram();

      var rect = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
      var data = Bitmap.LockBits(rect, ImageLockMode.ReadWrite, Bitmap.PixelFormat);
      var depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8;

      var buffer = new byte[data.Width * data.Height * depth];
      var maxProgress = data.Width * data.Height;

      //copy pixels to buffer
      Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

      RGBHistogram leftUp = new RGBHistogram(), rightUp = new RGBHistogram(), leftLow = new RGBHistogram(), rightLow = new RGBHistogram();

      var w = data.Width;
      var h = data.Height;

      // Run on 4 threads
      Parallel.Invoke(
        () => leftUp = CreateHist(buffer, 0, 0, w / 2, h / 2, w, depth),
        () => rightUp = CreateHist(buffer, w / 2, 0, w, h / 2, w, depth),
        () => leftLow = CreateHist(buffer, 0, h / 2, w / 2, h, w, depth),
        () => rightLow = CreateHist(buffer, w / 2, h / 2, w, h, w, depth)
      );

      //Copy the buffer back to image
      Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);

      Bitmap.UnlockBits(data);

      try
      {
        Bitmap.Save(ImageFile.Filtered, ImageFormat.Jpeg);
      }
      catch(Exception e)
      {
        System.Windows.MessageBox.Show("Error :" + e.Message);
      }

      hist.Add(leftUp);
      hist.Add(rightUp);
      hist.Add(leftLow);
      hist.Add(rightLow);

      return hist;
    }

    private static RGBHistogram CreateHist(byte[] buffer, int x, int y, int endx, int endy, int width, int depth)
    {
      var rgb = new RGBHistogram();
      var eps = 1;

      for (int i = x; i < endx; i++)
      {
        for (int j = y; j < endy; j++)
        {
          //System.Threading.Interlocked.Increment(ref progress);
          var offset = ((j * width) + i) * depth;
          Byte b = buffer[offset];
          Byte g = buffer[offset + 1];
          Byte r = buffer[offset + 2];
          // Byte a = buffer[offset + 3];
          rgb.r[r]++;
          rgb.g[g]++;
          rgb.b[b]++;
          // Green normalize
          buffer[offset + 2] = (byte)((g + eps) / ((Math.Max(r, b) + eps)));
        }
      }
      return rgb;
    }
  }
}
