using GreenhousePlusPlusCore.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace GreenhousePlusPlusCore.Vision
{
  public class Pipeline
  {
    public ImageProcessor ImageProcessor;
    public ImageManager ImageFile;
    public readonly FilterValues FilterValues = new FilterValues();

    public Pipeline()
    { }

    public Pipeline(string filepath)
    {
      // Create a copy with random filename and resize
      var randomFilename = Guid.NewGuid() + ".jpg";
      ImageFile = new ImageManager(randomFilename);
      var targetFile = ImageManager.ImagePath + randomFilename;

      using (Image image = Image.Load<Rgba32>(filepath))
      {
        using (var target = image.Clone(x => x.Resize(0, 480)))
        {
          target.Save(targetFile);
        }
        using (var thumb = image.Clone(x => x.Resize(0, 200)))
        {
          thumb.Save(ImageFile.Thumb.Path);
        }
      }
    }

    public void Process(string file = null)
    {
      if (file != null)
      {
        ImageFile = new ImageManager(file);
      }
      ImageProcessor = new ImageProcessor(ImageFile);
      ImageProcessor.Start(FilterValues);
    }
  }
}
