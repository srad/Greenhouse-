using Greenhouse.Models;
using Greenhouse.ViewModels;
using Greenhouse.Vision;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Greenhouse
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private ImageProcessor ImageProcessor;
    private ImageManager CurrentFile;
    private readonly ImageListView ImageListView = new ImageListView();
    private bool drawPoints = false;
    private bool PrimaryMouseDown = false;
    private int GridY = 30;
    private int GridX = 30;
    private bool[][] GridSelected;

    public MainWindow()
    {
      InitializeComponent();

      var images = Directory.GetFiles(ImageManager.ThumbsPath, "*.jpg");
      foreach (var image in images)
      {
        ImageListView.AddImage(image);
      }

      TvBox.ItemsSource = ImageListView;
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
        ImageListView.AddImage(CurrentFile.Thumb.Path);

        ProcessFile();
      }
    }

    private void ProcessFile()
    {
      canvasGreen.Children.Clear();
      imgInput.Source = CurrentFile.Original.BitmapImage.Value;
      ImageProcessor = new ImageProcessor(CurrentFile, (progress) => { });

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

      filterRed.Source = CurrentFile.FilteredRed.BitmapImage.Value;
      filterGreen.ImageSource = CurrentFile.FilteredGreen.BitmapImage.Value;
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

    private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
    {
      var messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
      if (messageBoxResult == MessageBoxResult.Yes)
      {
        var image = sender as System.Windows.Controls.Button;
        var file = (string)image.Tag;
        var paths = new ImageManager(file);
        ImageListView.RemoveItem(file);
        paths.Delete();
      }
    }

    private void OpenGreenFiltered(object sender, RoutedEventArgs e)
    {
      OpenFile(CurrentFile.FilteredGreen.Path);
    }

    private void OpenRedFiltered(object sender, RoutedEventArgs e)
    {
      OpenFile(CurrentFile.FilteredRed.Path);
    }

    public static void Square(int x, int y, int width, int height, System.Windows.Controls.Canvas cv)
    {
      if (x < 0 || y < 0)
      {
        return;
      }
      var b = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(25, 255, 100, 0));
      var rect = new System.Windows.Shapes.Rectangle()
      {
        Width = width,
        Height = height,
        Stroke = b,
        StrokeThickness = width / 2
      };

      cv.Children.Add(rect);

      rect.SetValue(System.Windows.Controls.Canvas.LeftProperty, (double)x);
      rect.SetValue(System.Windows.Controls.Canvas.TopProperty, (double)y);
    }

    private void CanvasGreen_MouseDown(object sender, MouseButtonEventArgs e)
    {
      PrimaryMouseDown = true;
      var info = e.GetPosition(canvasGreen);
      Square((int)info.X, (int)info.Y, 30, 30, canvasGreen);
    }

    private void CanvasGreen_MouseMove(object sender, MouseEventArgs e)
    {
      if (PrimaryMouseDown)
      {
        var info = e.GetPosition(canvasGreen);
        Debug.WriteLine(info.X + "," + info.Y);
        Square((int)info.X, (int)info.Y, 30, 30, canvasGreen);
      }
    }

    private void CanvasGreen_MouseUp(object sender, MouseButtonEventArgs e)
    {
      PrimaryMouseDown = false;
    }

    private void ButtonUndoGreen_Click(object sender, RoutedEventArgs e)
    {
      for (int i = 0; i < 30; i++)
      {
        if (canvasGreen.Children.Count == 0)
        {
          break;
        }
        canvasGreen.Children.Remove(canvasGreen.Children[canvasGreen.Children.Count - 1]);
      }
      Debug.WriteLine(canvasGreen.Children.Count);
    }
  }
}
