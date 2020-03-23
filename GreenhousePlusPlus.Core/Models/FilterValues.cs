using System;

namespace GreenhousePlusPlus.Core.Models
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

    public FilterValues(double redThreshold = 100, double greenThreshold = 100)
    {
      if (redThreshold > 100.0 || greenThreshold > 100.0 || redThreshold < 0.0 || greenThreshold < 0.0)
      {
        throw new ArgumentException($"Invalid arguments for FilterThresholds({greenThreshold}, {redThreshold})");
      }
      this.Red = redThreshold;
      this.Green = greenThreshold;
      // Percentage of the color
      this.RedMinRatio = redThreshold / 100;
      this.GreenMinRatio =greenThreshold / 100;
    }
  }
}
