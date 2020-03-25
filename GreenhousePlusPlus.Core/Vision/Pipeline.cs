using GreenhousePlusPlus.Core.Models;

namespace GreenhousePlusPlus.Core.Vision
{
  public class Pipeline
  {
    private ImageProcessor _imageProcessor;
    public readonly ImageManager ImageManager;
    public readonly FilterValues FilterValues = new FilterValues();

    public Pipeline(string basePath)
    {
      ImageManager = new ImageManager(basePath);
    }

    public void Create(string filepath)
    {
      ImageManager.Create(filepath);
    }

    public void Open(string filepath)
    {
      ImageManager.Open(filepath);
    }

    public ImageProcessResult Process(string file = null)
    {
      if (file != null)
      {
        ImageManager.Open(file);
      }
      _imageProcessor = new ImageProcessor(ImageManager);
      return _imageProcessor.Start(FilterValues);
    }
  }
}
