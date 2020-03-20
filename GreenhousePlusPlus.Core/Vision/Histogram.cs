using GreenhousePlusPlusCore.Models;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;

namespace GreenhousePlusPlusCore.Vision
{
  public class Histogram
  {
    public RGBArray RGBArray = new RGBArray();

    public Image<Rgba32> HistogramR;
    public Image<Rgba32> HistogramG;
    public Image<Rgba32> HistogramB;

    public Histogram Add(Histogram other)
    {
      if (other == null)
      {
        return this;
      }
      for (int i = 0; i < RGBArray.MAX; i++)
      {
        RGBArray.R[i] += other.RGBArray.R[i];
        RGBArray.G[i] += other.RGBArray.G[i];
        RGBArray.B[i] += other.RGBArray.B[i];
      }
      return this;
    }

    public RGB Max()
    {
      var max = new RGB();

      for (int i = 0; i < RGBArray.MAX; i++)
      {
        max.R = Math.Max(max.R, RGBArray.R[i]);
        max.G = Math.Max(max.G, RGBArray.G[i]);
        max.B = Math.Max(max.B, RGBArray.B[i]);
      }

      return max;
    }

    public void Draw(FilterResult filterResult, bool DrawPointHistogram, int width, int height)
    {
      var max = filterResult.Histogram.Max();
      var maxAll = Math.Max(Math.Max(max.B, max.G), max.R);
      var colorBandHeight = 5;
      HistogramR = new Image<Rgba32>(Configuration.Default, width, height + colorBandHeight + 1, new Rgba32(255, 255, 255, 0));
      HistogramG = new Image<Rgba32>(Configuration.Default, width, height + colorBandHeight + 1, new Rgba32(255, 255, 255, 0));
      HistogramB = new Image<Rgba32>(Configuration.Default, width, height + colorBandHeight + 1, new Rgba32(255, 255, 255, 0));


      for (int i = 0; i < RGBArray.MAX; i++)
      {
        int r = (int)(((double)filterResult.Histogram.RGBArray.R[i] / (double)(maxAll + 1)) * height);
        int g = (int)(((double)filterResult.Histogram.RGBArray.G[i] / (double)(maxAll + 1)) * height);
        int b = (int)(((double)filterResult.Histogram.RGBArray.B[i] / (double)(maxAll + 1)) * height);

        if (DrawPointHistogram)
        {
          HistogramR[i, HistogramR.Height - r - 1] = Rgba32.Red;
          HistogramG[i, HistogramG.Height - g - 1] = Rgba32.Green;
          HistogramB[i, HistogramB.Height - b - 1] = Rgba32.Blue;
        }
        else
        {
          for (int yR = HistogramR.Height - r - 1; yR < HistogramR.Height - 1; yR++)
          {
            HistogramR[i, yR] = Rgba32.Red;
          }
          for (int yG = HistogramG.Height - g - 1; yG < HistogramG.Height - 1; yG++)
          {
            HistogramG[i, yG] = Rgba32.Green;
          }
          for (int yB = HistogramB.Height - b - 1; yB < HistogramB.Height - 1; yB++)
          {
            HistogramB[i, yB] = Rgba32.Blue;
          }
        }

        // Color band
        for (int y = 0; y < colorBandHeight; y++)
        {
          var redSpan = HistogramR.GetPixelRowSpan(y + height + 1);
          var greenSpan = HistogramG.GetPixelRowSpan(y + height + 1);
          var blueSpan = HistogramB.GetPixelRowSpan(y + height + 1);

          for (int x = 0; x < width; x++)
          {
            var ratio = (float)x / RGBArray.MAX;
            redSpan[x] = new Rgba32(ratio, 0, 0);
            greenSpan[x] = new Rgba32(0, ratio, 0);
            blueSpan[x] = new Rgba32(0, 0, ratio);
          }
        }
      }
    }
  }
}
