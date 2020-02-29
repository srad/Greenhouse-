using Greenhouse.Models;
using System;
using System.Drawing;

namespace Greenhouse.Vision
{

  public class Histogram
  {
    public const int MAX = 256;

    public int[] R = new int[MAX];
    public int[] G = new int[MAX];
    public int[] B = new int[MAX];

    public Bitmap HistogramR;
    public Bitmap HistogramG;
    public Bitmap HistogramB;

    public Histogram Add(Histogram other)
    {
      for (int i = 0; i < MAX; i++)
      {
        R[i] += other.R[i];
        G[i] += other.G[i];
        B[i] += other.B[i];
      }
      return this;
    }

    public RGB Max()
    {
      var max = new RGB();

      for (int i = 0; i < MAX; i++)
      {
        max.R = Math.Max(max.R, R[i]);
        max.G = Math.Max(max.G, G[i]);
        max.B = Math.Max(max.B, B[i]);
      }

      return max;
    }
    
    public static Histogram CreateHist(FilterThesholds thresholds, byte[] redBffer, byte[] greenBuffer, int x, int y, int endx, int endy, int width, int depth)
    {
      var h = new Histogram();
      var eps = 1;

      for (int i = x; i < endx; i++)
      {
        for (int j = y; j < endy; j++)
        {
          //System.Threading.Interlocked.Increment(ref progress);
          var offset = ((j * width) + i) * depth;
          Byte b = redBffer[offset];
          Byte g = redBffer[offset + 1];
          Byte r = redBffer[offset + 2];
          // Byte a = buffer[offset + 3];
          h.R[r]++;
          h.G[g]++;
          h.B[b]++;

          var redRatio = ((r + eps) / ((Math.Max(g, b) + eps)));
          var greenRatio = ((g + eps) / ((Math.Max(r, b) + eps)));
          // Set other color only blue
          if (!(redRatio > thresholds.RedNormalized))
          {
            redBffer[offset] = (byte)255;
            redBffer[offset + 1] = (byte)0;
            redBffer[offset + 2] = (byte)0;
          }
          if (!(greenRatio > thresholds.GreenNormalized))
          {
            greenBuffer[offset] = (byte)255;
            greenBuffer[offset + 1] = (byte)0;
            greenBuffer[offset + 2] = (byte)0;
          }
        }
      }
      return h;
    }

    public void Draw(FilterResult filterResult, bool DrawPointHistogram, int width, int height)
    {
      HistogramR = new Bitmap(width, height);
      HistogramG = new Bitmap(width, height);
      HistogramB = new Bitmap(width, height);

      var max = filterResult.Histogram.Max();
      var maxAll = Math.Max(Math.Max(max.B, max.G), max.R);
      var colorBandHeight = 4;

      for (int i = 0; i < Histogram.MAX; i++)
      {
        int r = (int)(((double)filterResult.Histogram.R[i] / (double)(maxAll + 1)) * height);
        int g = (int)(((double)filterResult.Histogram.G[i] / (double)(maxAll + 1)) * height);
        int b = (int)(((double)filterResult.Histogram.B[i] / (double)(maxAll + 1)) * height);

        if (DrawPointHistogram)
        {
          HistogramR.SetPixel(i, HistogramR.Height - r - 1, Color.FromArgb(125, 255, 0, 0));
          HistogramG.SetPixel(i, HistogramG.Height - g - 1, Color.FromArgb(125, 0, 255, 0));
          HistogramB.SetPixel(i, HistogramB.Height - b - 1, Color.FromArgb(125, 0, 0, 255));
        }
        else
        {
          for (int yR = HistogramR.Height - r - 1; yR < HistogramR.Height - 1; yR++)
          {
            HistogramR.SetPixel(i, yR, Color.FromArgb(125, 255, 0, 0));
          }
          for (int yG = HistogramG.Height - g - 1; yG < HistogramG.Height - 1; yG++)
          {
            HistogramG.SetPixel(i, yG, Color.FromArgb(125, 0, 255, 0));
          }
          for (int yB = HistogramB.Height - b - 1; yB < HistogramB.Height - 1; yB++)
          {
            HistogramB.SetPixel(i, yB, Color.FromArgb(125, 0, 0, 255));
          }
        }

        for (int j = 1; j < colorBandHeight; j++)
        {
          HistogramR.SetPixel(i, HistogramR.Height - j, Color.FromArgb(i, i, i));
          HistogramG.SetPixel(i, HistogramG.Height - j, Color.FromArgb(i, i, i));
          HistogramB.SetPixel(i, HistogramB.Height - j, Color.FromArgb(i, i, i));
        }
      }
    }
  }
}
