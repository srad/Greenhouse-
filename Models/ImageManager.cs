using System;
using System.IO;

namespace Greenhouse.Models
{

  public class ImageManager
  {
    public static string BasePath = AppDomain.CurrentDomain.BaseDirectory + @"Images\";
    public static string ImagePath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Original\";
    public static string ThumbsPath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Thumbs\";
    public static string FilteredPath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Filtered\";
    public static string SegmentedPath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Segmented\";
    public static string HistPath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Hist\";
    public static string SelectionPath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Selection\";
    public static string KernelPath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Kernels\";

    public readonly string Filename;
    public ImageFile Original;
    public ImageFile Thumb;
    public ImageFile FilteredGreen;
    public ImageFile FilteredRed;

    public ImageFile HistR;
    public ImageFile HistG;
    public ImageFile HistB;

    public ImageFile Leaf;
    public ImageFile Earth;
    public ImageFile Edge;
    public ImageFile EdgeOverlay;
    public ImageFile Blur;
    public ImageFile Highpass;

    public ImageFile Selection;

    public ImageManager(string file)
    {
      Filename = file;

      Original = new ImageFile(ImagePath + file);
      Thumb = new ImageFile(ThumbsPath + file);

      HistR = new ImageFile(HistPath + "r_" + file);
      HistG = new ImageFile(HistPath + "g_" + file);
      HistB = new ImageFile(HistPath + "b_" + file);

      Selection = new ImageFile(SelectionPath + file);

      var pngFilename = Path.GetFileNameWithoutExtension(file) + ".png";

      FilteredGreen = new ImageFile(FilteredPath + "green_" + pngFilename);
      FilteredRed = new ImageFile(FilteredPath + "red_" + pngFilename);
      Earth = new ImageFile(SegmentedPath + "earth_" + pngFilename);
      Leaf = new ImageFile(SegmentedPath + "leaf_" + pngFilename);

      Edge = new ImageFile(KernelPath + "edge_" + pngFilename);
      EdgeOverlay = new ImageFile(KernelPath + "edge_overlay" + pngFilename);
      Blur = new ImageFile(KernelPath + "blur_" + pngFilename);
      Highpass = new ImageFile(KernelPath + "highpass_" + pngFilename);

      Directory.CreateDirectory(BasePath);
      Directory.CreateDirectory(ImagePath);
      Directory.CreateDirectory(ThumbsPath);
      Directory.CreateDirectory(FilteredPath);
      Directory.CreateDirectory(HistPath);
      Directory.CreateDirectory(SelectionPath);
      Directory.CreateDirectory(SegmentedPath);
      Directory.CreateDirectory(KernelPath);
    }

    public void Delete()
    {
      Original.Delete();
      Thumb.Delete();
      FilteredGreen.Delete();
      FilteredRed.Delete();
      HistR.Delete();
      HistG.Delete();
      HistB.Delete();
      Selection.Delete();
      Earth.Delete();
      Leaf.Delete();
      Edge.Delete();
      EdgeOverlay.Delete();
      Blur.Delete();
      Highpass.Delete();
    }
  }
}
