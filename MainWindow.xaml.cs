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
    private ImageProcessor ImageProcessor;
    private ImageManager CurrentFile;
    private readonly ObservableCollection<ImageListView> ImageFileList = new ObservableCollection<ImageListView>();
    private bool drawPoints = false;

    public MainWindow()
    {
      InitializeComponent();

      var images = Directory.GetFiles(ImageManager.ThumbsPath, "*.jpg");
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
          ImageFileList[i].ImageData = null;
          GC.Collect();
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
        var targetFile = ImageManager.ImagePath + randomFilename;
        File.Copy(filePath, targetFile);
        CurrentFile = new ImageManager(randomFilename);

        // Create Thumb
        var thumb = ImageHelper.Resize(CurrentFile.Original.Path, 300, 200);
        thumb.Save(CurrentFile.Thumb.Path);
        AddImageToListView(CurrentFile.Thumb.Path);

        ProcessFile();
      }
    }

    private void ProcessFile()
    {
      working.Visibility = Visibility.Visible;
      imgInput.Source = CurrentFile.Original.BitmapImage.Value;
      ImageProcessor = new ImageProcessor(CurrentFile, (progress) => {});

      // Color distribution of the original image
      var histogram = ImageProcessor.Start(new FilterThesholds(green: GreenThreshold.Value, red: RedThreshold.Value));

      var hR = new System.Drawing.Bitmap(256, 200);
      var hG = new System.Drawing.Bitmap(256, 200);
      var hB = new System.Drawing.Bitmap(256, 200);
      var max = histogram.Max();
      var maxAll = Math.Max(Math.Max(max.B, max.G), max.R);
      var colorBandHeight = 4;

      for (int i = 0; i < Histogram.MAX; i++)
      {
        int r = (int)(((double)histogram.R[i] / (double)(maxAll + 1)) * hR.Height);
        int g = (int)(((double)histogram.G[i] / (double)(maxAll + 1)) * hG.Height);
        int b = (int)(((double)histogram.B[i] / (double)(maxAll + 1)) * hB.Height);

        var green = System.Drawing.Color.FromArgb(0, i, 0);
        var red = System.Drawing.Color.FromArgb(i, 0, 0);
        var blue = System.Drawing.Color.FromArgb(0, 0, i);

        if (drawPoints)
        {
          hR.SetPixel(i, hR.Height - r - 1, System.Drawing.Color.FromArgb(125, 255, 0, 0));
          hG.SetPixel(i, hG.Height - g - 1, System.Drawing.Color.FromArgb(125, 0, 255, 0));
          hB.SetPixel(i, hB.Height - b - 1, System.Drawing.Color.FromArgb(125, 0, 0, 255));
        }
        else
        {
          for (int yR = hR.Height - r - 1; yR < hR.Height - 1; yR++)
          {
            hR.SetPixel(i, yR, System.Drawing.Color.FromArgb(125, 255, 0, 0));
          }
          for (int yG = hG.Height - g - 1; yG < hG.Height - 1; yG++)
          {
            hG.SetPixel(i, yG, System.Drawing.Color.FromArgb(125, 0, 255, 0));
          }
          for (int yB = hB.Height - b - 1; yB < hB.Height - 1; yB++)
          {
            hB.SetPixel(i, yB, System.Drawing.Color.FromArgb(125, 0, 0, 255));
          }
        }
        //histImage.SetPixel(i, histImage.Height-r-1, System.Drawing.Color.FromArgb(255, 0, 0));
        //histImage.SetPixel(i, histImage.Height-b-1, System.Drawing.Color.FromArgb(0, 0, 255));

        for (int j = 1; j < colorBandHeight; j++)
        {
          hR.SetPixel(i, hR.Height - j, System.Drawing.Color.FromArgb(i, i, i));
          hG.SetPixel(i, hG.Height - j, System.Drawing.Color.FromArgb(i, i, i));
          hB.SetPixel(i, hB.Height - j, System.Drawing.Color.FromArgb(i, i, i));
        }
      }

      hR.Save(CurrentFile.HistR.Path);
      hG.Save(CurrentFile.HistG.Path);
      hB.Save(CurrentFile.HistB.Path);
      imgHistR.Source = CurrentFile.HistR.BitmapImage.Value;
      imgHistG.Source = CurrentFile.HistG.BitmapImage.Value;
      imgHistB.Source = CurrentFile.HistB.BitmapImage.Value;

      working.Visibility = Visibility.Hidden;
      filterRed.Source = CurrentFile.FilteredRed.BitmapImage.Value;
      filterGreen.Source = CurrentFile.FilteredGreen.BitmapImage.Value;
    }
    
    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
      var image = sender as System.Windows.Controls.Image;
      var file = (string)image.Tag;
      CurrentFile = new ImageManager(file);
      ProcessFile();
    }

    void OpenFile(string file)
    {
      Process photoViewer = new Process();
      photoViewer.StartInfo.FileName = file;
      photoViewer.Start();
    }

    private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(ImageManager.BasePath);
    }

    private void btnDeleteImage_Click(object sender, RoutedEventArgs e)
    {
      var messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
      if (messageBoxResult == MessageBoxResult.Yes)
      {
        var image = sender as System.Windows.Controls.Button;
        var file = (string)image.Tag;
        var paths = new ImageManager(file);
        RemoveItem(file);
        paths.Delete();
      }
    }

    private void FilterGreen_MouseDown(object sender, RoutedEventArgs e)
    {
      OpenFile(CurrentFile.FilteredGreen.Path);
    }

    private void FilterRed_MouseDown(object sender, RoutedEventArgs e)
    {
      OpenFile(CurrentFile.FilteredRed.Path);
    }
  }
}
