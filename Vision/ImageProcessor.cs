using Greenhouse.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Greenhouse.Vision
{
  public class ImageProcessor
  {
    private readonly ImageManager ImageFile;

    public ImageProcessor(ImageManager imagefile)
    {
      this.ImageFile = imagefile;
    }

    /// <summary>
    /// This method does three things:
    ///   1. Create a histogram or the original image color distribution
    ///   2. Creates a new image with filtered green colors
    ///   2. Creates a new image with filtered red colors
    /// </summary>
    /// <returns></returns>
    public Histogram Start(FilterThesholds thresholds)
    {
      var filterResult = ImageFile.Original.Filter(thresholds);

      try
      {
        filterResult.RedBitmap.Save(ImageFile.FilteredRed.Path, ImageFormat.Png);
        filterResult.GreenBitmap.Save(ImageFile.FilteredGreen.Path, ImageFormat.Png);
        filterResult.LeafBitmap.Save(ImageFile.Leaf.Path, ImageFormat.Png);
        filterResult.EarthBitmap.Save(ImageFile.Earth.Path, ImageFormat.Png);
        filterResult.Histogram.Draw(filterResult, false, 256, 200);
      }
      catch (Exception e)
      {
        System.Windows.MessageBox.Show("Error :" + e.Message);
      }

      return filterResult.Histogram;
    }
  }
}
