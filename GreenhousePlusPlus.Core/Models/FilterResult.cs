using GreenhousePlusPlusCore.Vision;
using SixLabors.ImageSharp;

namespace GreenhousePlusPlusCore.Models
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
}
