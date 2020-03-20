using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GreenhousePlusPlusCore.Models
{
  public class ImageManager
  {
    public const string ImageDir = "Images";

    private readonly string BaseDir;
    public string BasePath { get => Path.Combine(BaseDir, ImageDir); }
    public string ImagePath { get => Path.Combine(BaseDir, ImageDir, "Original"); }
    public string ThumbsPath { get => Path.Combine(BaseDir, ImageDir, "Thumbs"); }
    public string FilteredPath { get => Path.Combine(BaseDir, ImageDir, "Filtered"); }
    public string SegmentedPath { get => Path.Combine(BaseDir, ImageDir, "Segmented"); }
    public string HistPath { get => Path.Combine(BaseDir, ImageDir, "Hist"); }
    public string KernelPath { get => Path.Combine(BaseDir, ImageDir, "Kernels"); }

    public string Filename;
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

    private bool FileOpened = false;

    public ImageManager() : this(AppDomain.CurrentDomain.BaseDirectory)
    {}

    public ImageManager(string baseDir)
    {
      this.BaseDir = baseDir;
      Directory.CreateDirectory(BasePath);
      Directory.CreateDirectory(ImagePath);
      Directory.CreateDirectory(ThumbsPath);
      Directory.CreateDirectory(FilteredPath);
      Directory.CreateDirectory(HistPath);
      Directory.CreateDirectory(SegmentedPath);
      Directory.CreateDirectory(KernelPath);
    }

    public void Create(string srcFile)
    {
      var filename = Guid.NewGuid();
      Filename = filename + ".jpg";
      var destFile = Path.Combine(ImagePath, Filename);
      var thumbFile = Path.Combine(ThumbsPath, Filename);

      using (var image = Image.Load<Rgba32>(srcFile))
      {
        using (var target = image.Clone(x => x.Resize(0, 480)))
        {
          target.Save(destFile);
        }
        using (var thumb = image.Clone(x => x.Resize(0, 200)))
        {
          thumb.Save(thumbFile);
        }
      }

      Open(Filename);
    }

    public void Open(string filename)
    {
      Original = new ImageFile(Path.Combine(ImagePath, filename));
      Thumb = new ImageFile(Path.Combine(ThumbsPath, filename));

      HistR = new ImageFile(Path.Combine(HistPath, "r_" + filename));
      HistG = new ImageFile(Path.Combine(HistPath, "g_" + filename));
      HistB = new ImageFile(Path.Combine(HistPath, "b_" + filename));

      var pngFilename = Path.GetFileNameWithoutExtension(filename) + ".png";

      FilteredGreen = new ImageFile(Path.Combine(FilteredPath, "green_" + pngFilename));
      FilteredRed = new ImageFile(Path.Combine(FilteredPath, "red_" + pngFilename));
      Earth = new ImageFile(Path.Combine(SegmentedPath, "earth_" + pngFilename));
      Leaf = new ImageFile(Path.Combine(SegmentedPath, "leaf_" + pngFilename));

      Edge = new ImageFile(Path.Combine(KernelPath, "edge_" + pngFilename));
      PlantTip = new ImageFile(Path.Combine(KernelPath, "edge_overlay" + pngFilename));
      Blur = new ImageFile(Path.Combine(KernelPath, "blur_" + pngFilename));
      Pass = new ImageFile(Path.Combine(KernelPath, "pass_" + pngFilename));

      FileOpened = true;
    }

    public IEnumerable<string> GetFiles()
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
      if (!FileOpened || !File.Exists(Original.Path))
      {
        throw new FileNotFoundException("No file to delete");
      }
      Original.Delete();
      Thumb.Delete();
      FilteredGreen.Delete();
      FilteredRed.Delete();
      HistR.Delete();
      HistG.Delete();
      HistB.Delete();
      Earth.Delete();
      Leaf.Delete();
      Edge.Delete();
      PlantTip.Delete();
      Blur.Delete();
      Pass.Delete();
    }
  }
}
