using System;
using System.IO;
using System.Net.Mime;
using GreenhousePlusPlus.WebAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenhousePlusPlus.WebAPI
{
  public class Startup
  {
    public const string StaticFolder = "Static";
    public static string StaticPath => Path.Combine(StartupFolder, StaticFolder);
    public static string StartupFolder => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCors();
      services.AddControllers();
      services.AddControllers(options =>
        options.Filters.Add(new HttpResponseExceptionFilter()))
        .ConfigureApiBehaviorOptions(options =>
        {
          options.InvalidModelStateResponseFactory = context =>
          {
            var result = new BadRequestObjectResult(context.ModelState);

            // TODO: add `using using System.Net.Mime;` to resolve MediaTypeNames
            result.ContentTypes.Add(MediaTypeNames.Application.Json);
            result.ContentTypes.Add(MediaTypeNames.Application.Xml);

            return result;
          };
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
      app.UseExceptionHandler(env.IsDevelopment() ? "/error-local-development" : "/error");

      if (env.IsDevelopment())
      {
        var srcFolder = Path.Combine(Directory.GetCurrentDirectory(), StaticFolder);
        Directory.CreateDirectory(StaticPath);
        foreach (var srcPath in Directory.GetFiles(srcFolder))
        {
          File.Copy(srcPath, srcPath.Replace(srcPath, srcPath.Replace(srcFolder, StaticPath)), true);
        }
      }
      app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

      app.UseStaticFiles(new StaticFileOptions
      {
        FileProvider = new PhysicalFileProvider(StaticPath),
        RequestPath = "/static"
      });

      app.UseRouting();

      //app.UseAuthorization();

      app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
  }
}