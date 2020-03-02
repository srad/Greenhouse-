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
      var results = new List<Histogram>();
      var count = 4;
      var bitmaps = new Bitmap[count];
      var bitmapData = new BitmapData[count];
      var buffers = new byte[count][];

      for(int i=0; i < count; i++)
      {
        bitmaps[i] = new Bitmap(Path);
      }

      var h = bitmaps[0].Height;
      var w = bitmaps[0].Width;
      var rect = new Rectangle(0, 0, w, h);

      for(int i=0; i < count; i++)
      {
        bitmapData[i] = bitmaps[i].LockBits(rect, ImageLockMode.ReadWrite, bitmaps[i].PixelFormat);
      }

      for (int i = 0; i < count; i++)
      {
        var bitMapDepth = Bitmap.GetPixelFormatSize(bitmapData[i].PixelFormat) / 8;
        buffers[i] = new byte[bitmapData[i].Width * bitmapData[i].Height * bitMapDepth];
        // Copy pixels to buffer
        Marshal.Copy(bitmapData[i].Scan0, buffers[i], 0, buffers[i].Length);
      }

      var depth = Bitmap.GetPixelFormatSize(bitmapData[0].PixelFormat) / 8;

      try
      {
        // Run on 4 threads
        Parallel.Invoke(
          () => results.Add(Histogram.CreateHist(thresholds, buffers, 0, 0, w / 2, h / 2, w, depth)),
          () => results.Add(Histogram.CreateHist(thresholds, buffers, w / 2, 0, w, h / 2, w, depth)),
          () => results.Add(Histogram.CreateHist(thresholds, buffers, 0, h / 2, w / 2, h, w, depth)),
          () => results.Add(Histogram.CreateHist(thresholds, buffers, w / 2, h / 2, w, h, w, depth))
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

      for (int i = 0; i < count; i++)
      {
        // Copy the buffer back to image
        Marshal.Copy(buffers[i], 0, bitmapData[i].Scan0, buffers[i].Length);
        bitmaps[i].UnlockBits(bitmapData[i]);
      }

      return new FilterResult
      {
        Histogram = results.Aggregate(new Histogram(), (a, b) => a.Add(b)),
        RedBitmap = bitmaps[0],
        GreenBitmap = bitmaps[1],
        LeafBitmap = bitmaps[2],
        EarthBitmap = bitmaps[3]
      };
    }
  }
}
