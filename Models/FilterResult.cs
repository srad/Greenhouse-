using Greenhouse.Vision;
using System.Drawing;

namespace Greenhouse.Models
{
  public class FilterResult
  {
    public Bitmap RedBitmap;
    public Bitmap GreenBitmap;
    public Bitmap LeafBitmap;
    public Bitmap EarthBitmap;
    public Histogram Histogram;
    public Histogram LeafHistogram;
    public Histogram EarthHistogram;
  }
}
