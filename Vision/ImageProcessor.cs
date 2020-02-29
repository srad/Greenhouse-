using Greenhouse.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
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
    public RGBHistogram Start(double greenThreshold)
    {
      progress = 0;
      greenThreshold = (double)greenThreshold / 100;

      var rect = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
      var data = Bitmap.LockBits(rect, ImageLockMode.ReadWrite, Bitmap.PixelFormat);
      var depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8;

      var buffer = new byte[data.Width * data.Height * depth];
      var maxProgress = data.Width * data.Height;

      //copy pixels to buffer
      Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

      var w = data.Width;
      var h = data.Height;
      var results = new List<RGBHistogram>();

      try
      {
        // Run on 4 threads
        Parallel.Invoke(
          () => results.Add(RGBHistogram.CreateHist(greenThreshold, buffer, 0, 0, w / 2, h / 2, w, depth)),
          () => results.Add(RGBHistogram.CreateHist(greenThreshold, buffer, w / 2, 0, w, h / 2, w, depth)),
          () => results.Add(RGBHistogram.CreateHist(greenThreshold, buffer, 0, h / 2, w / 2, h, w, depth)),
          () => results.Add(RGBHistogram.CreateHist(greenThreshold, buffer, w / 2, h / 2, w, h, w, depth))
        );
        // These lines can generate an arbritrary number of threads but they raise random exceptions
        // var threads = Environment.ProcessorCount;
        // var actions = new List<Action>();
        // var chunks = h / threads;
        // Parallel.For(0, threads, i => actions.Add(() => results.Add(CreateHist(buffer, 0, i * chunks, w, i * chunks + chunks, w, depth))));
        // Parallel.Invoke(actions.ToArray());
      }
      catch (Exception e)
      {
        System.Windows.MessageBox.Show("Error :" + e.Message);
      }
      // Copy the buffer back to image
      Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);

      Bitmap.UnlockBits(data);

      try
      {
        Bitmap.Save(ImageFile.Filtered, ImageFormat.Jpeg);
      }
      catch (Exception e)
      {
        System.Windows.MessageBox.Show("Error :" + e.Message);
      }

      return results.Aggregate(new RGBHistogram(), (a, b) => a.Add(b));
    }
  }
}
