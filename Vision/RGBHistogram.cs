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

    public void Add(RGBHistogram other)
    {
      for (int i = 0; i < MAX; i++)
      {
        r[i] += other.r[i];
        g[i] += other.g[i];
        b[i] += other.b[i];
      }
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
  }
}
