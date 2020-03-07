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
    public FilterResult Start(FilterValues thresholds)
    {
      var filterResult = ImageFile.Original.Filter(thresholds);

      try
      {
        filterResult.RedBitmap.Save(ImageFile.FilteredRed.Path, ImageFormat.Png);
        filterResult.GreenBitmap.Save(ImageFile.FilteredGreen.Path, ImageFormat.Png);
        filterResult.LeafBitmap.Save(ImageFile.Leaf.Path, ImageFormat.Png);
        filterResult.EarthBitmap.Save(ImageFile.Earth.Path, ImageFormat.Png);
        filterResult.EdgeBitmap.Save(ImageFile.Edge.Path, ImageFormat.Png);
        filterResult.EdgeOverlayBitmap.Save(ImageFile.EdgeOverlay.Path, ImageFormat.Png);
        filterResult.BlurBitmap.Save(ImageFile.Blur.Path, ImageFormat.Png);
        filterResult.HighpassBitmap.Save(ImageFile.Highpass.Path, ImageFormat.Png);
        filterResult.Histogram.Draw(filterResult, false, 256, 200);
        filterResult.Histogram.HistogramR.Save(ImageFile.HistR.Path);
        filterResult.Histogram.HistogramG.Save(ImageFile.HistG.Path);
        filterResult.Histogram.HistogramB.Save(ImageFile.HistB.Path);
      }
      catch (Exception e)
      {
        System.Windows.MessageBox.Show("Error :" + e.Message);
      }

      return filterResult;
    }
  }
}
