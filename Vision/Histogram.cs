using Greenhouse.Models;
using System;

namespace Greenhouse.Vision
{

  public class Histogram
  {
    public const int MAX = 256;

    public int[] R = new int[MAX];
    public int[] G = new int[MAX];
    public int[] B = new int[MAX];

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
        max.R = System.Math.Max(max.R, R[i]);
        max.G = System.Math.Max(max.G, G[i]);
        max.B = System.Math.Max(max.B, B[i]);
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
  }
}
