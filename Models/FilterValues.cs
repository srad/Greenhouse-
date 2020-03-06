using System;

namespace Greenhouse.Models
{
  public class FitlerValues
  {
    public double Red { get; set; }
    public double Green { get; set; }
    public double RedMinRatio { get; set; }
    public double GreenMinRatio { get; set; }
    public double ThetaTheshold { get; set; } = 0.83d;
    public byte WhiteThreshold { get; set; } = 141;
    public int BlurRounds { get; set; } = 100;
    public int ScanlineInterpolationWidth { get; set; } = 6;

    public FitlerValues(double red = 10, double green = 10)
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
