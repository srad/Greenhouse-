using System;

namespace GreenhousePlusPlusCore.Models
{
  public class FilterValues
  {
    public double Red { get; set; }
    public double Green { get; set; }
    public double RedMinRatio { get; set; }
    public double GreenMinRatio { get; set; }
    public double ThetaTheshold { get; set; } = 0.83d;
    public byte WhiteThreshold { get; set; } = 25;
    public int BlurRounds { get; set; } = 40;
    public int ScanlineInterpolationWidth { get; set; } = 0;

    public FilterValues(double red = 10, double green = 10)
    {
      if (red > 100.0 || green > 100.0 || red < 0.0 || green < 0.0)
      {
        throw new ArgumentException($"Invalid arguments for FilterThresholds({green}, {red})");
      }
      this.Red = red;
      this.Green = green;
      this.RedMinRatio = Red / 100;
      this.GreenMinRatio = Green / 100;
    }
  }
}
