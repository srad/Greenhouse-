using System;
using System.IO;
using SixLabors.ImageSharp;

namespace GreenhousePlusPlus.Core.Models
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
    public readonly string Path;
    public string RelativePath => Path.Replace(_basePath, "");
    public Lazy<IImageInfo> Info;
    private readonly string _basePath;

    public ImageFile(string basePath, string path)
    {
      _basePath = basePath;
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
