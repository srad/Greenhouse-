using Greenhouse.Vision;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Greenhouse.Models
{

  public class ImageFile
  {
    public string Path;
    public Lazy<BitmapImage> BitmapImage;

    public ImageFile(string path)
    {
      this.Path = path;
      BitmapImage = new Lazy<BitmapImage>(() => ImageHelper.LoadBitmap(path));
    }

    public void Delete()
    {
      File.Delete(Path);
    }

    public FilterResult Filter(FilterThesholds thresholds)
    {
      var redBitmap = new Bitmap(Path);
      var greenBitmap = new Bitmap(Path);

      var rect = new Rectangle(0, 0, redBitmap.Width, redBitmap.Height);
      var redData = redBitmap.LockBits(rect, ImageLockMode.ReadWrite, redBitmap.PixelFormat);
      var greenData = greenBitmap.LockBits(rect, ImageLockMode.ReadWrite, greenBitmap.PixelFormat);

      var depth = Bitmap.GetPixelFormatSize(redData.PixelFormat) / 8;

      var redBuffer = new byte[redData.Width * redData.Height * depth];
      var greenBuffer = new byte[greenData.Width * greenData.Height * depth];
      var maxProgress = redData.Width * redData.Height;

      //copy pixels to buffer
      Marshal.Copy(redData.Scan0, redBuffer, 0, redBuffer.Length);
      Marshal.Copy(greenData.Scan0, greenBuffer, 0, greenBuffer.Length);

      var w = redData.Width;
      var h = redData.Height;
      var results = new List<Histogram>();

      try
      {
        // Run on 4 threads
        Parallel.Invoke(
          () => results.Add(Histogram.CreateHist(thresholds, redBuffer, greenBuffer, 0, 0, w / 2, h / 2, w, depth)),
          () => results.Add(Histogram.CreateHist(thresholds, redBuffer, greenBuffer, w / 2, 0, w, h / 2, w, depth)),
          () => results.Add(Histogram.CreateHist(thresholds, redBuffer, greenBuffer, 0, h / 2, w / 2, h, w, depth)),
          () => results.Add(Histogram.CreateHist(thresholds, redBuffer, greenBuffer, w / 2, h / 2, w, h, w, depth))
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
      Marshal.Copy(redBuffer, 0, redData.Scan0, redBuffer.Length);
      Marshal.Copy(greenBuffer, 0, greenData.Scan0, greenBuffer.Length);

      redBitmap.UnlockBits(redData);
      greenBitmap.UnlockBits(redData);

      return new FilterResult
      {
        Histogram = results.Aggregate(new Histogram(), (a, b) => a.Add(b)),
        RedBitmap = redBitmap,
        GreenBitmap = greenBitmap
      };
    }
  }
}
