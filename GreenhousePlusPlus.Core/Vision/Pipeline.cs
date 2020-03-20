using GreenhousePlusPlusCore.Models;
using System;

namespace GreenhousePlusPlusCore.Vision
{
  public class Pipeline
  {
    private ImageProcessor ImageProcessor;
    public readonly ImageManager ImageManager;
    public readonly FilterValues FilterValues = new FilterValues();

    public Pipeline()
    {
      ImageManager = new ImageManager();
    }

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
      ImageProcessor = new ImageProcessor(ImageManager);
      return ImageProcessor.Start(FilterValues);
    }
  }
}
