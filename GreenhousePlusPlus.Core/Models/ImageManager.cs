using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GreenhousePlusPlusCore.Models
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
    public ImageFile PlantTip;
    public ImageFile Blur;
    public ImageFile Pass;

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
      PlantTip = new ImageFile(KernelPath + "edge_overlay" + pngFilename);
      Blur = new ImageFile(KernelPath + "blur_" + pngFilename);
      Pass = new ImageFile(KernelPath + "pass_" + pngFilename);

      Directory.CreateDirectory(BasePath);
      Directory.CreateDirectory(ImagePath);
      Directory.CreateDirectory(ThumbsPath);
      Directory.CreateDirectory(FilteredPath);
      Directory.CreateDirectory(HistPath);
      Directory.CreateDirectory(SelectionPath);
      Directory.CreateDirectory(SegmentedPath);
      Directory.CreateDirectory(KernelPath);
    }

    public static IEnumerable<string> GetFiles()
    {
      if (Directory.Exists(BasePath))
      {
        return Directory.EnumerateFiles(ThumbsPath, "*.*", SearchOption.TopDirectoryOnly)
              .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png"));
      }
      return new List<string>();
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
      PlantTip.Delete();
      Blur.Delete();
      Pass.Delete();
    }
  }
}
