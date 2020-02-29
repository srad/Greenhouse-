using System;

namespace Greenhouse.Vision
{
  public class RGB
  {
    public int r = 0;
    public int b = 0;
    public int g = 0;
  }

  public class RGBHistogram
  {
    public const int MAX = 256;

    public int[] r = new int[MAX];
    public int[] g = new int[MAX];
    public int[] b = new int[MAX];

    public RGBHistogram Add(RGBHistogram other)
    {
      for (int i = 0; i < MAX; i++)
      {
        r[i] += other.r[i];
        g[i] += other.g[i];
        b[i] += other.b[i];
      }
      return this;
    }

    public RGB Max()
    {
      var max = new RGB();

      for (int i = 0; i < MAX; i++)
      {
        max.r = System.Math.Max(max.r, r[i]);
        max.g = System.Math.Max(max.g, g[i]);
        max.b = System.Math.Max(max.b, b[i]);
      }

      return max;
    }
    
    public static RGBHistogram CreateHist(double greenThreshold, byte[] buffer, int x, int y, int endx, int endy, int width, int depth)
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

          var greenMax = ((double)(g + eps) / (double)((Math.Max(r, b) + eps)));
          if (!(greenMax > greenThreshold))
          {
            buffer[offset] = (byte)255;
            buffer[offset + 1] = (byte)0;
            buffer[offset + 2] = (byte)0;
          }
        }
      }
      return rgb;
    }
  }
}
