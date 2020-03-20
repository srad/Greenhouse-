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
    public const string ImageDir = @"Images\";

    private readonly string BaseDir;
    public string BasePath { get => BaseDir + ImageDir; }
    public string ImagePath { get => BaseDir + ImageDir + @"Original\"; }
    public string ThumbsPath { get => BaseDir + ImageDir + @"Thumbs\"; }
    public string FilteredPath { get => BaseDir + ImageDir + @"Filtered\"; }
    public string SegmentedPath { get => BaseDir + ImageDir + @"Segmented\"; }
    public string HistPath { get => BaseDir + ImageDir + @"Hist\"; }
    public string SelectionPath { get => BaseDir + ImageDir + @"Selection\"; }
    public string KernelPath { get => BaseDir + ImageDir + @"Kernels\"; }

    private string _filename;
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
      Directory.CreateDirectory(SelectionPath);
      Directory.CreateDirectory(SegmentedPath);
      Directory.CreateDirectory(KernelPath);
    }

    public void Create(string srcFile)
    {
      var filename = Guid.NewGuid();
      _filename = filename + ".jpg";
      var destFile = ImagePath + _filename;
      var thumbFile = ThumbsPath + _filename;

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

      Open(_filename);
    }

    public void Open(string filename)
    {
      Original = new ImageFile(ImagePath + filename);
      Thumb = new ImageFile(ThumbsPath + filename);

      HistR = new ImageFile(HistPath + "r_" + filename);
      HistG = new ImageFile(HistPath + "g_" + filename);
      HistB = new ImageFile(HistPath + "b_" + filename);

      Selection = new ImageFile(SelectionPath + filename);

      var pngFilename = Path.GetFileNameWithoutExtension(filename) + ".png";

      FilteredGreen = new ImageFile(FilteredPath + "green_" + pngFilename);
      FilteredRed = new ImageFile(FilteredPath + "red_" + pngFilename);
      Earth = new ImageFile(SegmentedPath + "earth_" + pngFilename);
      Leaf = new ImageFile(SegmentedPath + "leaf_" + pngFilename);

      Edge = new ImageFile(KernelPath + "edge_" + pngFilename);
      PlantTip = new ImageFile(KernelPath + "edge_overlay" + pngFilename);
      Blur = new ImageFile(KernelPath + "blur_" + pngFilename);
      Pass = new ImageFile(KernelPath + "pass_" + pngFilename);

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
