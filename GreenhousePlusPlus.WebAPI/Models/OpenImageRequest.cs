using System.ComponentModel.DataAnnotations;
using GreenhousePlusPlus.Core.Models;

namespace GreenhousePlusPlus.WebAPI.Models
{
  public class OpenImageRequest
  {
    [Required] public string File { get; set; }
    [Required] public double ThetaTheshold { get; set; } = 0.83d;
    [Required] public byte WhiteThreshold { get; set; } = 25;
    [Required] public int BlurRounds { get; set; } = 40;
    [Required] public int ScanlineInterpolationWidth { get; set; } = 0;
  }
}