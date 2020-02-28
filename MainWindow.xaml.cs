using Greenhouse.Models;
using Greenhouse.Vision;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Greenhouse__
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private bool filtered = false;
    private ImageProcessor ImageProcessor;
    private ImageFile imagefile;
    private readonly ObservableCollection<ImageListView> ImageFileList = new ObservableCollection<ImageListView>();

    public MainWindow()
    {
      InitializeComponent();

      Directory.CreateDirectory(ImageFile.ImagePath);
      Directory.CreateDirectory(ImageFile.ThumbsPath);
      Directory.CreateDirectory(ImageFile.FilteredPath);
      Directory.CreateDirectory(ImageFile.HistPath);


      var images = Directory.GetFiles(ImageFile.ThumbsPath, "*.jpg");
      foreach (var image in images)
      {
        AddImageToListView(image);
      }

      TvBox.ItemsSource = ImageFileList;
    }

    private void AddImageToListView(string file)
    {
      var item = new ImageListView
      {
        File = Path.GetFileName(file),
        ImageData = ImageHelper.LoadBitmap(file),
        Title = "Image"
      };

      ImageFileList.Add(item);
    }

    private void RemoveItem(string filename)
    {
      for (int i = 0; i < ImageFileList.Count; i++)
      {
        if (ImageFileList[i].File == filename)
        {
          ImageFileList.Remove(ImageFileList[i]);
        }
      }
    }

    private void btnOpenFile_Click(object sender, RoutedEventArgs e)
    {
      var openFileDialog = new OpenFileDialog
      {
        RestoreDirectory = true,
        Filter = "JPG image|*.jpg|PNG image|*.png",
      };
      if (openFileDialog.ShowDialog() == true)
      {
        // Copy file to project
        var filePath = openFileDialog.FileName;
        var randomFilename = Guid.NewGuid() + Path.GetExtension(filePath);
        var targetFile = ImageFile.ImagePath + randomFilename;
        File.Copy(filePath, targetFile);
        imagefile = new ImageFile(randomFilename);

        // Create Thumb
        var thumb = ImageHelper.Resize(imagefile.Original, 300, 200);
        thumb.Save(imagefile.Thumb);
        AddImageToListView(imagefile.Thumb);

        ProcessFile();
      }
    }

    private void ProcessFile()
    {
      imgInput.Source = ImageHelper.LoadBitmap(imagefile.Original);
      ImageProcessor = new ImageProcessor(imagefile, (progress) =>
      {
      });

      var histogram = ImageProcessor.Start();
      var histImage = new System.Drawing.Bitmap(256, 200);
      var max = histogram.Max();
      var maxAll = Math.Max(Math.Max(max.b, max.g), max.r);
      var colorBandHeight = 4;

      for (int i = 0; i < RGBHistogram.MAX; i++)
      {
        int r = (int)(((double)histogram.r[i] / (double)(maxAll + 1)) * histImage.Height);
        int g = (int)(((double)histogram.g[i] / (double)(maxAll + 1)) * histImage.Height);
        int b = (int)(((double)histogram.b[i] / (double)(maxAll + 1)) * histImage.Height);

        var green = System.Drawing.Color.FromArgb(0, i, 0);
        var red = System.Drawing.Color.FromArgb(i, 0, 0);
        var blue = System.Drawing.Color.FromArgb(0, 0, i);

        histImage.SetPixel(i, histImage.Height - r - 1, System.Drawing.Color.FromArgb(125, 255, 0, 0));
        histImage.SetPixel(i, histImage.Height - g - 1, System.Drawing.Color.FromArgb(125, 0, 255, 0));
        histImage.SetPixel(i, histImage.Height - b - 1, System.Drawing.Color.FromArgb(125, 0, 0, 255));
        /*
        for (int yR = histImage.Height - r - 1; yR < histImage.Height - 1; yR++)
        {
          histImage.SetPixel(i, yR, System.Drawing.Color.FromArgb(125, 255, 0, 0));
        }
        for (int yG = histImage.Height - g - 1; yG < histImage.Height - 1; yG++)
        {
          histImage.SetPixel(i, yG, System.Drawing.Color.FromArgb(125, 0, 255, 0));
        }
        for (int yB = histImage.Height - b - 1; yB < histImage.Height - 1; yB++)
        {
          histImage.SetPixel(i, yB, System.Drawing.Color.FromArgb(125, 0, 0, 255));
        }
        */
        //histImage.SetPixel(i, histImage.Height-r-1, System.Drawing.Color.FromArgb(255, 0, 0));
        //histImage.SetPixel(i, histImage.Height-b-1, System.Drawing.Color.FromArgb(0, 0, 255));

        for (int j = 1; j < colorBandHeight; j++)
        {
          histImage.SetPixel(i, histImage.Height - j, System.Drawing.Color.FromArgb(i, i, i));
        }
      }

      histImage.Save(imagefile.Hist);
      imgHist.Source = ImageHelper.LoadBitmap(imagefile.Hist);

      filtered = true;
      imgClustered.Source = ImageHelper.LoadBitmap(imagefile.Filtered);
    }
    
    private void OpenImageClustered(object sender, MouseButtonEventArgs e)
    {
      if (!filtered)
      {
        return;
      }
      OpenFile(imagefile.Filtered);
    }

    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
      var image = sender as System.Windows.Controls.Image;
      var file = (string)image.Tag;
      imagefile = new ImageFile(file);
      ProcessFile();
    }

    void OpenFile(string file)
    {
      Process photoViewer = new Process();
      photoViewer.StartInfo.FileName = file;
      photoViewer.Start();
    }

    private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(ImageFile.BasePath);
    }

    private void btnDeleteImage_Click(object sender, RoutedEventArgs e)
    {
      var messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
      if (messageBoxResult == MessageBoxResult.Yes)
      {
        var image = sender as System.Windows.Controls.Button;
        var file = (string)image.Tag;
        var paths = new ImageFile(file);
        RemoveItem(file);
        File.Delete(paths.Hist);
        File.Delete(paths.Original);
        File.Delete(paths.Thumb);
        File.Delete(paths.Filtered);
      }
    }
  }
}
