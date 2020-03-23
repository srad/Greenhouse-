using System.Collections.Generic;
using GreenhousePlusPlus.Core.Vision;
using SixLabors.ImageSharp;

namespace GreenhousePlusPlus.Core.Models
{
  public class FilterResult
  {
    public Image RedImage;
    public Image GreenImage;
    public Image LeafImage;
    public Image EarthImage;
    public Image EdgeImage;
    public Image PlantTipImage;
    public Image BlurImage;
    public Image PassImage;
    public Histogram Histogram;
    public Histogram LeafHistogram;
    public Histogram EarthHistogram;
  }

  public class FilterFileInfo
  {
    public string Element { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
  }

  public class ImageProcessResult : List<FilterFileInfo>
  {
  }
}