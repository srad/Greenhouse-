using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greenhouse.Vision
{
  public static class Kernel
  {
    public static class Sobel
    {
      public static readonly double[,] X = new double[,]
      {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 },
      };

      public static readonly double[,] Y = new double[,]
      {
        { -1, -2, -1 },
        { -2,  0,  2 },
        { -1,  2,  1 },
      };
    }

    public static class Blur
    {
      private const double z = 1.0 / 9.0;
      public static readonly double[,] Simple = new double[,]
      {
        { z, z, z },
        { z, z, z },
        { z, z, z },
      };

      public static readonly double[,] Simple2 = new double[,]
      {
        { 1/16d, 1/8d, 1/16d },
        { 1/8d, 1/4d, 1/8d },
        { 1/16d, 1/8d, 1/16d }
      };

      public static double[,] Gaussian(int size, double sigma = 1.1)
      {
        var matrix = new double[size, size];
        double sigmaSq = sigma * sigma;
        double s = (double)size;
        double constant = (1d / (2d * Math.PI * sigmaSq));

        for (int y = 0; y < size; y++)
        {
          for (int x = 0; x < size; x++)
          {
            var dx = Math.Abs(s / 2d - (double)x);
            var dy = Math.Abs(s / 2d - (double)y);
            matrix[y, x] = constant * Math.Exp(-((dx * dx + dy * dy) / (2 * sigmaSq)));
          }
        }
        return matrix;
      }
    }

    public static void Fold(ReadOnlySpan<byte> originalBuffer, Span<byte> outputBuffer, double[,] kernel, int maxX, int maxY, int depth)
    {
      for (int y = 0; y < maxY; y++)
      {
        for (int x = 0; x < maxX; x++)
        {
          var i = depth * y + x;
          var b = (double)originalBuffer[i];
          var g = (double)originalBuffer[i + 1];
          var r = (double)originalBuffer[i + 2];

          double grey = (r + g + b) / 3.0;

          double sum = 0;
          int len = (int)Math.Sqrt(kernel.Length);
          for (int row = 0; row < maxX; row++)
          {
            for (int col = 0; col < maxY; col++)
            {
              sum += kernel[row, col] * grey;
            }
          }
          var result = (byte)sum;
          outputBuffer[i] = result;
          outputBuffer[i + 1] = result;
          outputBuffer[i + 2] = result;
        }
      }
    }
  }
}
