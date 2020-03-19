using Greenhouse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greenhouse.Vision
{
  class Pipeline
  {
    public ImageProcessor ImageProcessor;
    public ImageManager ImageFile;
    public readonly FilterValues FilterValues = new FilterValues();

    public Pipeline()
    {}

    public Pipeline(string filepath)
    {
      // Create a copy with random filename and resize
      var randomFilename = Guid.NewGuid() + ".jpg";
      ImageFile = new ImageManager(randomFilename);
      var targetFile = ImageManager.ImagePath + randomFilename;
      var image = ImageHelper.Resize(filepath, 1024, 786);
      image.Save(targetFile);

      // Create Thumb
      var thumb = ImageHelper.Resize(ImageFile.Original.Path, 300, 200);
      thumb.Save(ImageFile.Thumb.Path);
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
