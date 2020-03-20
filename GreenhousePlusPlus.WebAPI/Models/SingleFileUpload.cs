using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenhousePlusPlus.WebAPI.Models
{
  public class SingleFileUpload
  {
    public IFormFile File { get; set; }
  }
}
