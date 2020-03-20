using SixLabors.ImageSharp;
using System;
using System.IO;

namespace GreenhousePlusPlusCore.Models
{
  struct ColCounter
  {
    public int X;
    public int Y;
    public int Length;
  }

  struct FilterRect
  {
    public int x;
    public int y;
    public int endx;
    public int endy;
  }

  public class ImageFile
  {
    public string Path;
    public Lazy<IImageInfo> Info;

    public ImageFile(string path)
    {
      this.Path = path;
      Info = new Lazy<IImageInfo>(() =>
      {
        using (var file = new FileStream(Path, FileMode.Open))
        {
          return Image.Identify(file);
        }
      });
    }

    public void Delete()
    {
      File.Delete(Path);
    }
  }
}
