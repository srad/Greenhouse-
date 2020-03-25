using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GreenhousePlusPlus.Core.Models;
using GreenhousePlusPlus.Core.Vision;
using GreenhousePlusPlus.WebAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace GreenhousePlusPlus.WebAPI.Controllers
{
  [Route("api/[controller]")]
  public class ImagesController : Controller
  {
    private readonly Pipeline _pipeline;
    private readonly IWebHostEnvironment _env;
    private const double MaxUploadSizeMb = 5.0;

    public ImagesController(IWebHostEnvironment env)
    {
      _pipeline = new Pipeline(Startup.StaticPath);
      _env = env;
    }

    [HttpGet]
    public IEnumerable<FileData> Get()
    {
      var files = _pipeline.ImageManager
        .GetRelativeFilePaths()
        .Select(file => new FileData { Path = @"/" + Startup.StaticFolder + file.Replace("\\", "/"), Name = Path.GetFileName(file) })
        .ToList();

      return files;
    }

    /// <summary>
    /// Upload a new file.
    /// </summary>
    /// <param name="upload"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IEnumerable<FilterFileInfo>> Post([FromForm]SingleFileUpload upload)
    {
      var mb = ((double)upload.File.Length / 1024) / 1024;
      if (mb > MaxUploadSizeMb)
      {
        throw new NotSupportedException($"The upload exceeds the maximum size of 3MB");
      }
      
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
        .Select(file => new FilterFileInfo { Element = file.Element, Path = @"/" + Startup.StaticFolder + file.Path.Replace("\\", "/"), Name = Path.GetFileName(file.Path) })
        .ToList();
    }

    /// <summary>
    /// Load and process an existing file.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPut("{file}")]
    public IEnumerable<FilterFileInfo> Open(string file)
    {
      _pipeline.Open(file);
      var result = _pipeline.Process();

      return result
        .Select(f => new FilterFileInfo { Element = f.Element, Path = @"/" + Startup.StaticFolder + f.Path.Replace("\\", "/"), Name = Path.GetFileName(f.Path) })
        .ToList();
    }

    [HttpDelete("{file}")]
    public void Delete(string file)
    {
      var m = new ImageManager(Startup.StaticPath);
      m.Open(file);
      m.Delete();
    }
  }
}
