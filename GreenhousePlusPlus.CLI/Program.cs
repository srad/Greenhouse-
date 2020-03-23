using System;
using System.IO;
using GreenhousePlusPlus.Core.Vision;
using Newtonsoft.Json;

namespace GreenhousePlusPlus.CLI
{
  class Program
  {
    static void Main(string[] args)
    {
      var p = new Pipeline("Output");
      p.Create(args[0]);
      var files = JsonConvert.SerializeObject(p.Process());
      var filename = Path.Combine("Output", Path.GetFileNameWithoutExtension(p.ImageManager.Filename) + ".json");
      File.WriteAllText(filename, files);
      Console.WriteLine($"The images has been written to: {p.ImageManager.BasePath}");
      Console.WriteLine($"The information about the files have been written to: {filename}");
    }
  }
}