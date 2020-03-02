using Greenhouse.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Greenhouse.Vision
{
  public class ImageProcessor
  {
    private readonly ImageManager ImageFile;
    private readonly Action<int> progressCallback;
    private static int progress = 0;

    public ImageProcessor(ImageManager imagefile, Action<int> progressCallback)
    {
      this.ImageFile = imagefile;
      this.progressCallback = progressCallback;
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
      progress = 0;
      var filterResult = ImageFile.Original.Filter(thresholds);

      try
      {
        filterResult.RedBitmap.Save(ImageFile.FilteredRed.Path, ImageFormat.Jpeg);
        filterResult.GreenBitmap.Save(ImageFile.FilteredGreen.Path, ImageFormat.Jpeg);
        filterResult.LeafBitmap.Save(ImageFile.Leaf.Path, ImageFormat.Jpeg);
        filterResult.EarthBitmap.Save(ImageFile.Earth.Path, ImageFormat.Jpeg);
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
