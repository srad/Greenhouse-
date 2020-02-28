using System;

namespace Greenhouse.Models
{
  public class ImageFile
  {
    public static string BasePath = AppDomain.CurrentDomain.BaseDirectory + @"Images\";
    public static string ImagePath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Original\";
    public static string ThumbsPath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Thumbs\";
    public static string FilteredPath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Filtered\";
    public static string HistPath = AppDomain.CurrentDomain.BaseDirectory + @"Images\Hist\";

    public readonly string Filename;
    public string Original;
    public string Thumb;
    public string Filtered;
    public string Hist;

    public ImageFile(string filename)
    {
      Filename = filename;
      Original = ImagePath + Filename;
      Thumb = ThumbsPath + Filename;
      Filtered = FilteredPath + Filename;
      Hist = HistPath + Filename;
    }
  }
}
