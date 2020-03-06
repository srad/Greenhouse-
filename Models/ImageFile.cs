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
  public struct ColCounter
  {
    public int X;
    public int Y;
    public int Counter;
  }

  public partial class ImageFile
  {
    public string Path;
    public Lazy<BitmapImage> BitmapImage;
    private readonly RGB Transparent = new RGB { R = 251, G = 1, B = 154 };

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static int Offset(int x, int y, int width, int depth) => ((y * width) + x) * depth;

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
      var count = 8;
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

      ScanlineVerticalLines(
        buffers[BufferIdx.EdgeFilter].AsSpan(buffers[BufferIdx.EdgeFilter].GetLowerBound(0), buffers[BufferIdx.EdgeFilter].Length),
        buffers[BufferIdx.LongestEdgeOverlay].AsSpan(buffers[BufferIdx.LongestEdgeOverlay].GetLowerBound(0), buffers[BufferIdx.LongestEdgeOverlay].Length),
        w,
        h,
        depth);

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
        RedBitmap = bitmaps[BufferIdx.Red],
        GreenBitmap = bitmaps[BufferIdx.Green],
        LeafBitmap = bitmaps[BufferIdx.Leaf],
        EarthBitmap = bitmaps[BufferIdx.Earth],
        EdgeBitmap = bitmaps[BufferIdx.EdgeFilter],
        HighpassBitmap = bitmaps[BufferIdx.Highpass],
        BlurBitmap = bitmaps[BufferIdx.Blur],
        EdgeOverlayBitmap = bitmaps[BufferIdx.LongestEdgeOverlay]
      };
    }

    private static Histogram Process(RGB transparentColor, FilterThesholds thresholds, byte[][] buffers, int x, int y, int endx, int endy, int width, int depth)
    {
      var h = new Histogram();
      var epsilon = 1;
      var copyOriginalBuffer = new byte[buffers[BufferIdx.EdgeFilter].Length];
      buffers[BufferIdx.EdgeFilter].CopyTo(copyOriginalBuffer, 0);

      double idx(int pos_x, int pos_y)
      {
        var index = Offset(pos_x, pos_y, width, depth);
        return (copyOriginalBuffer[index] + copyOriginalBuffer[index + 1] + copyOriginalBuffer[index + 2]) / 3.0;
      };

      for (int i = x; i < endx; i++)
      {
        for (int j = y; j < endy; j++)
        {
          var offset = ((j * width) + i) * depth;

          Byte b = buffers[0][offset];
          Byte g = buffers[0][offset + 1];
          Byte r = buffers[0][offset + 2];
          // Byte a = buffers[0][offset + 3];

          // Sobel operator
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

          // Theta is 0 for vertical edges, but real world numbers...fine tune.
          var theta = Math.Atan2(pixel_y, pixel_x);
          if (theta > -0.19 && theta < 0.19)
          {
            var mag = Math.Ceiling(Math.Sqrt((pixel_x * pixel_x) + (pixel_y * pixel_y)));
            byte val = 0;
            if (mag < 60)
            {
              val = (byte)(255 - mag);
            }
            buffers[BufferIdx.EdgeFilter][offset] = val;
            buffers[BufferIdx.EdgeFilter][offset + 1] = val;
            buffers[BufferIdx.EdgeFilter][offset + 2] = val;
          }
          else
          {
            buffers[BufferIdx.EdgeFilter][offset] = (byte)255;
            buffers[BufferIdx.EdgeFilter][offset + 1] = (byte)255;
            buffers[BufferIdx.EdgeFilter][offset + 2] = (byte)255;
          }

          h.RGBArray.R[r]++;
          h.RGBArray.G[g]++;
          h.RGBArray.B[b]++;

          var redRatio = ((r + epsilon) / ((Math.Max(g, b) + epsilon)));
          var greenRatio = ((g + epsilon) / ((Math.Max(r, b) + epsilon)));
          var redDominant = redRatio > thresholds.RedMinRatio;
          var greenDominant = greenRatio > thresholds.GreenMinRatio;

          // Set other color only blue
          if (!redDominant)
          {
            buffers[BufferIdx.Red][offset] = (byte)transparentColor.B;
            buffers[BufferIdx.Red][offset + 1] = (byte)transparentColor.G;
            buffers[BufferIdx.Red][offset + 2] = (byte)transparentColor.R;
          }
          else // Red is dominant
          {
            buffers[BufferIdx.Leaf][offset] = (byte)transparentColor.B;
            buffers[BufferIdx.Leaf][offset + 1] = (byte)transparentColor.G;
            buffers[BufferIdx.Leaf][offset + 2] = (byte)transparentColor.R;

            buffers[BufferIdx.Red][offset] = (byte)10;
            buffers[BufferIdx.Red][offset + 1] = (byte)69;
            buffers[BufferIdx.Red][offset + 2] = (byte)130;
          }
          if (!greenDominant)
          {
            buffers[BufferIdx.Green][offset] = (byte)transparentColor.B;
            buffers[BufferIdx.Green][offset + 1] = (byte)transparentColor.G;
            buffers[BufferIdx.Green][offset + 2] = (byte)transparentColor.R;

            buffers[BufferIdx.Earth][offset] = b;
            buffers[BufferIdx.Earth][offset + 1] = g;
            buffers[BufferIdx.Earth][offset + 2] = r;
          }
          else // Green is dominant
          {
            buffers[BufferIdx.Earth][offset] = (byte)transparentColor.B;
            buffers[BufferIdx.Earth][offset + 1] = (byte)transparentColor.G;
            buffers[BufferIdx.Earth][offset + 2] = (byte)transparentColor.R;

            buffers[BufferIdx.Green][offset] = (byte)0;
            buffers[BufferIdx.Green][offset + 1] = (byte)255;
            buffers[BufferIdx.Green][offset + 2] = (byte)0;
          }
        }
      }

      copyOriginalBuffer = null;

      // Retain edge for display purposes, duplicate
      var startOffset = ((y * width) + x) * depth;
      var chunkLength = ((((endy - y) * width) + (endx - x + 1)) * depth);

      // Don't overwrite, copy: Edges -> Blur
      Array.Copy(buffers[BufferIdx.EdgeFilter], startOffset, buffers[BufferIdx.Blur], startOffset, chunkLength);
      VerticalBlur(buffers[BufferIdx.Blur], width, depth, x, (y == 0 ? 20 : y), endx, endy);

      // Don't overwrite blur filter, copy: Blur -> Highpass
      Array.Copy(buffers[BufferIdx.Blur], startOffset, buffers[BufferIdx.Highpass], startOffset, chunkLength);

      HighpassFilter(buffers[BufferIdx.Highpass], width, depth, x, y, endx, endy);

      return h;
    }

    #region filters
    private static void VerticalBlur(byte[] blurBuffer, int width, int depth, int x, int y, int endx, int endy)
    {
      for (int z = 0; z < 70; z++)
      {
        // You cannot overwrite the same image while you're filtering it, create copy
        var edgeCopy = new byte[blurBuffer.Length];
        blurBuffer.CopyTo(edgeCopy, 0);

        double idx(int pos_x, int pos_y)
        {
          var index = Offset(pos_x, pos_y, width, depth);
          return edgeCopy[index];
        };

        // Skip edges
        for (int i = (x == 0 ? 2 : 0); i < endx; i++)
        {
          for (int j = y; j < endy; j++)
          {
            var offset = ((j * width) + i) * depth;

            byte blur = (byte)((idx(i, j - 1) + idx(i, j) + idx(i, j + 1)) / 3.0);
            blurBuffer[offset] = blur;
            blurBuffer[offset + 1] = blur;
            blurBuffer[offset + 2] = blur;
          }
        }
      }
    }

    private static void HighpassFilter(byte[] highpassBuffer, int width, int depth, int x, int y, int endx, int endy)
    {
      // Highpass filter
      for (int i = x; i < endx; i++)
      {
        for (int j = y; j < endy; j++)
        {
          var offset = ((j * width) + i) * depth;
          byte col = highpassBuffer[offset] < 150 ? (byte)0 : (byte)255;
          highpassBuffer[offset] = col;
          highpassBuffer[offset + 1] = col;
          highpassBuffer[offset + 2] = col;
        }
      }
    }

    /// <summary>
    /// Scanline algorithm: Count vertical intersecting with horizontal line
    /// Interpolate horizontally for robustness.
    /// </summary>
    /// <param name="edgeBuffer"></param>
    /// <param name="overlayBuffer"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="depth"></param>
    private static void ScanlineVerticalLines(Span<byte> edgeBuffer, Span<byte> overlayBuffer, int w, int h, int depth)
    {
      var vCount = new int[w, h];
      for (int y = 5; y < h - 5; y++)
      {
        for (int x = 5; x < w - 5; x++)
        {
          int offset = ((y * w) + x) * depth;
          double sum = 0d;

          // Horizontal average / interpolation,accepts slight slopes
          for (int k = -3; k <= 3; k++)
          {
            sum += edgeBuffer[offset + (k * depth)];
          }
          double avg = sum / 7.0d;

          // Lowpass filter: If not really white then count it
          vCount[x, y] = 0;
          if (avg < 120)
          {
            vCount[x, y] = 1;
          }
        }
      }

      var maxCol = new ColCounter { X = 0, Y = 0, Counter = 0 };
      // Verticall sum of continous vertical column
      for (int x = 0; x < w; x++)
      {
        int colSum = 0;
        for (int y = 5; y < h - 5; y++)
        {
          int offset = ((y * w) + x) * depth;
          if (vCount[x, y] == 1)
          {
            colSum++;
          }
          else
          {
            if (maxCol.Counter < colSum)
            {
              maxCol.X = x;
              maxCol.Y = y;
              maxCol.Counter = colSum;
            }
          }
        }
      }

      // Vertical line
      for (int y = maxCol.Y - maxCol.Counter; y < maxCol.Y; y++)
      {
        // Like thickness
        for (int x = maxCol.X - 2; x < maxCol.X + 2; x++)
        {
          var offset = ((y * w) + x) * depth;
          overlayBuffer[offset] = (byte)(edgeBuffer[offset] / 2);
          overlayBuffer[offset + 1] = (byte)(edgeBuffer[offset + 1] / 2);
          overlayBuffer[offset + 2] = 255;
          overlayBuffer[offset + 3] = 40;
        }
      }
    }
    #endregion
  }
}
