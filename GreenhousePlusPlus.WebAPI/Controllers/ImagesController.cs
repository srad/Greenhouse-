using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GreenhousePlusPlusCore.Models;
using GreenhousePlusPlusCore.Vision;
using Microsoft.AspNetCore.Hosting;
using GreenhousePlusPlus.WebAPI.Models;

namespace GreenhousePlusPlus.WebAPI
{
  [Route("api/[controller]")]
  public class ImagesController : Controller
  {
    private readonly Pipeline _pipeline;
    private readonly IWebHostEnvironment _env;

    public ImagesController(IWebHostEnvironment env)
    {
      _pipeline = new Pipeline("Static");
      _env = env;
    }

    [HttpGet]
    public IEnumerable<FileData> Get()
    {
      var files = _pipeline.ImageManager
        .GetFiles()
        .Select(file => new FileData { Path = @"/" + file.Replace("\\", "/"), Name = Path.GetFileName(file) })
        .ToList();

      return files;
    }

    [HttpPost]
    public async Task<IEnumerable<FilterFileInfo>> Post([FromForm]SingleFileUpload upload)
    {
      // full path to file in temp location
      var tmpFile = Path.GetTempFileName();

      if (upload.File.Length > 0)
      {
        using (var stream = new FileStream(tmpFile, FileMode.Create))
        {
          await upload.File.CopyToAsync(stream);
        }
        _pipeline.Create(tmpFile);
      }

      var result = _pipeline.Process();

      return result
        .Select(file => new FilterFileInfo { Element = file.Element, Path = @"/" + file.Path.Replace("\\", "/"), Name = Path.GetFileName(file.Path) })
        .ToList();
    }

    [HttpPut("{file}")]
    public IEnumerable<FilterFileInfo> Open(string file)
    {
      _pipeline.Open(file);
      var result = _pipeline.Process();

      return result
        .Select(file => new FilterFileInfo { Element = file.Element, Path = @"/" + file.Path.Replace("\\", "/"), Name = Path.GetFileName(file.Path) })
        .ToList();
    }

    [HttpDelete("{file}")]
    public void Delete(string file)
    {
      var m = new ImageManager("Static");
      m.Open(file);
      m.Delete();
    }
  }
}
