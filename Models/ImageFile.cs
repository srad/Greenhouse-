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
    private readonly RGB Transparent = new RGB { R = 251, G = 1, B = 154 };
    public static class BitmapIndex { public const int Red = 0, Green = 1, Leaf = 2, Earth = 3, EdgeFilter = 4; }

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
      var count = 5;
      var bitmaps = new Bitmap[count];
      var bitmapData = new BitmapData[count];
      var buffers = new byte[count][];

      for (int i = 0; i < count; i++)
      {
        // Copy to a transparent bitmap
        var bmp = new Bitmap(Path);
        bitmaps[i] = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format32bppRgb);
      }

      var h = bitmaps[0].Height;
      var w = bitmaps[0].Width;
      var rect = new Rectangle(0, 0, w, h);

      for (int i = 0; i < count; i++)
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
        var threads = Environment.ProcessorCount;
        double chunks = (double)h / threads;
        // Leave a image edge of 1px for kernel
        Parallel.For(0, threads, i => results.Add(Process(Transparent, thresholds, buffers, 1, (int)(i * chunks) + 1, w - 1, (int)((i + 1) * chunks) + (i == (threads - 1) ? -1 : +1), w, depth)));
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
        bitmaps[i].MakeTransparent(Color.FromArgb(Transparent.R, Transparent.G, Transparent.B));
      }

      return new FilterResult
      {
        Histogram = results.Aggregate(new Histogram(), (a, b) => a.Add(b)),
        RedBitmap = bitmaps[BitmapIndex.Red],
        GreenBitmap = bitmaps[BitmapIndex.Green],
        LeafBitmap = bitmaps[BitmapIndex.Leaf],
        EarthBitmap = bitmaps[BitmapIndex.Earth],
        EdgeBitmap = bitmaps[BitmapIndex.EdgeFilter]
      };
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Offset(int x, int y, int width, int depth) => ((y * width) + x) * depth;

    // X
    private static readonly int[,] sobel_i = new int[,]
    {
      { -1, 0, 1 },
      { -2, 0, 2 },
      { -1, 0, 1 }
    };

    // Y
    private static readonly int[,] sobel_j = new int[,]
    {
      { -1,-2,-1 },
      {  0, 0, 0 },
      {  1, 2, 1 }
    };
    
    private static Histogram Process(RGB transparentColor, FilterThesholds thresholds, byte[][] buffers, int x, int y, int endx, int endy, int width, int depth)
    {
      var h = new Histogram();
      var eps = 1;
      var dX = endx - x;
      var dY = endy - y;
      var edgeBuffer = new double[dX * dY];

      double idx(int pos_x, int pos_y)
      {
        var index = Offset(pos_x, pos_y, width, depth);
        return (buffers[BitmapIndex.EdgeFilter][index] + buffers[BitmapIndex.EdgeFilter][index + 1] + buffers[BitmapIndex.EdgeFilter][index + 2]) / 3.0;
      };

      var counter = 0;
      for (int i = x; i < endx; i++)
      {
        for (int j = y; j < endy; j++)
        {
          var offset = ((j * width) + i) * depth;

          Byte b = buffers[0][offset];
          Byte g = buffers[0][offset + 1];
          Byte r = buffers[0][offset + 2];
          // Byte a = buffers[0][offset + 3];

          byte blur = (byte)(
              (idx(i - 1, j - 1)
            + idx(i, j - 1)
            + idx(i + 1, j - 1)
            + idx(i - 1, j)
            + idx(i, j)
            + idx(i + 1, j)
            + idx(i - 1, j + 1)
            + idx(i, j + 1)
            + idx(i + 1, j + 1)) / 9);
          buffers[BitmapIndex.EdgeFilter][offset] = blur;
          buffers[BitmapIndex.EdgeFilter][offset + 1] = blur;
          buffers[BitmapIndex.EdgeFilter][offset + 2] = blur;
          
          // Scobel operator
          var pixel_x =
              // Row above
              (idx(i + 1, j - 1) - idx(i - 1, j - 1))
            // Some row
            + (2 * idx(i + 1, j) - 2 * idx(i - 1, j))
            // Row below
            + (idx(i + 1, j + 1) - idx(i - 1, j + 1));

          var pixel_y =
              // Row above
              (idx(i - 1, j + 1) - idx(i - 1, j - 1))
            // Some row
            + (2 * idx(i, j + 1) - 2 * idx(i, j - 1))
            // Row below
            + (idx(i + 1, j + 1) - idx(i + 1, j - 1));

          var val = (Math.Ceiling(Math.Sqrt((pixel_x * pixel_x) + (pixel_y * pixel_y))));
          // Theta is 0 for vertical edges, but real world numbers...fine tune.
          var theta = Math.Atan2(pixel_y, pixel_x);
          if (theta > -0.1 && theta < 0.1)
          {
            edgeBuffer[counter] = val;
          }
          counter++;

          h.RGBArray.R[r]++;
          h.RGBArray.G[g]++;
          h.RGBArray.B[b]++;

          var redRatio = ((r + eps) / ((Math.Max(g, b) + eps)));
          var greenRatio = ((g + eps) / ((Math.Max(r, b) + eps)));
          var redDominant = redRatio > thresholds.RedMinRatio;
          var greenDominant = greenRatio > thresholds.GreenMinRatio;

          // Set other color only blue
          if (!redDominant)
          {
            buffers[BitmapIndex.Red][offset] = (byte)transparentColor.B;
            buffers[BitmapIndex.Red][offset + 1] = (byte)transparentColor.G;
            buffers[BitmapIndex.Red][offset + 2] = (byte)transparentColor.R;
          }
          else // Red is dominant
          {
            buffers[BitmapIndex.Leaf][offset] = (byte)transparentColor.B;
            buffers[BitmapIndex.Leaf][offset + 1] = (byte)transparentColor.G;
            buffers[BitmapIndex.Leaf][offset + 2] = (byte)transparentColor.R;

            buffers[BitmapIndex.Red][offset] = (byte)10;
            buffers[BitmapIndex.Red][offset + 1] = (byte)69;
            buffers[BitmapIndex.Red][offset + 2] = (byte)130;
          }
          if (!greenDominant)
          {
            buffers[BitmapIndex.Green][offset] = (byte)transparentColor.B;
            buffers[BitmapIndex.Green][offset + 1] = (byte)transparentColor.G;
            buffers[BitmapIndex.Green][offset + 2] = (byte)transparentColor.R;

            buffers[BitmapIndex.Earth][offset] = b;
            buffers[BitmapIndex.Earth][offset + 1] = g;
            buffers[BitmapIndex.Earth][offset + 2] = r;
          }
          else // Green is dominant
          {
            buffers[BitmapIndex.Earth][offset] = (byte)transparentColor.B;
            buffers[BitmapIndex.Earth][offset + 1] = (byte)transparentColor.G;
            buffers[BitmapIndex.Earth][offset + 2] = (byte)transparentColor.R;

            buffers[BitmapIndex.Green][offset] = (byte)0;
            buffers[BitmapIndex.Green][offset + 1] = (byte)255;
            buffers[BitmapIndex.Green][offset + 2] = (byte)0;
          }
        }
      }

      counter = 0;
      for (int i = x; i < endx; i++)
      {
        for (int j = y; j < endy; j++)
        {
          var offset = ((j * width) + i) * depth;
          var color = (byte)(255-edgeBuffer[counter]);
          buffers[BitmapIndex.EdgeFilter][offset] = color;
          buffers[BitmapIndex.EdgeFilter][offset + 1] = color;
          buffers[BitmapIndex.EdgeFilter][offset + 2] = color;
          counter++;
        }
      }

      return h;
    }
  }
}
