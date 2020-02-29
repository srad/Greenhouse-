using System;

namespace Greenhouse.Models
{
  public class FilterThesholds
  {
    public readonly double Red;
    public readonly double Green;
    public readonly double RedNormalized;
    public readonly double GreenNormalized;

    public FilterThesholds(double red, double green)
    {
      if (red > 100.0 || green > 100.0 || red < 0.0 || green < 0.0)
      {
        throw new ArgumentException($"Invalid arguments for FilterThresholds({green}, {red})");
      }
      this.Red = red;
      this.Green = green;
      this.RedNormalized = Red / 100;
      this.GreenNormalized = Green / 100;
    }
  }
}
