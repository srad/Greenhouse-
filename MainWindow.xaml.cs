using Greenhouse.Models;
using Greenhouse.ViewModels;
using Greenhouse.Vision;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    private bool DrawPointHistogram = false;
    private bool PrimaryMouseDown = false;
    private int GridY = 40;
    private int GridX = 40;
    private bool ImageLoaded = false;
    private bool[,] GridSelection;
    private Stack<UIElement> RedoStack = new Stack<UIElement>();

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

      histogram.HistogramR.Save(CurrentFile.HistR.Path);
      histogram.HistogramG.Save(CurrentFile.HistG.Path);
      histogram.HistogramB.Save(CurrentFile.HistB.Path);

      imgHistR.Source = CurrentFile.HistR.BitmapImage.Value;
      imgHistG.Source = CurrentFile.HistG.BitmapImage.Value;
      imgHistB.Source = CurrentFile.HistB.BitmapImage.Value;

      filterRed.Source = CurrentFile.FilteredRed.BitmapImage.Value;
      filterGreen.ImageSource = CurrentFile.FilteredGreen.BitmapImage.Value;

      int sizeX = (int)(CurrentFile.FilteredGreen.BitmapImage.Value.Width /  GridX) + 1;
      int sizeY = (int)(CurrentFile.FilteredGreen.BitmapImage.Value.Height / GridY) + 1;
      GridSelection = new bool[sizeX, sizeY];
      for (int x = 0; x < sizeX; x++)
      {
        for (int y = 0; y < sizeY; y++)
        {
          GridSelection[x, y] = false;
        }
      }
      ImageLoaded = true;
      RedoStack.Clear();
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
      if (!ImageLoaded)
      {
        return;
      }
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

    public static void Rect(int x, int y, int width, int height, System.Windows.Controls.Canvas cv)
    {
      if (x < 0 || y < 0)
      {
        return;
      }
      var b = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 255, 0, 255));
      var rect = new System.Windows.Shapes.Rectangle()
      {
        Width = width,
        Height = height,
        Stroke = b,
        StrokeThickness = width/2
      };

      cv.Children.Add(rect);

      rect.SetValue(System.Windows.Controls.Canvas.LeftProperty, (double)x);
      rect.SetValue(System.Windows.Controls.Canvas.TopProperty, (double)y);
    }

    private class GridV2
    {
      public int X;
      public int Y;
      public int GridX;
      public int GridY;
    }

    private GridV2 MouseGridPos(Point e)
    {
      return new GridV2
      {
        X = (int)Math.Floor(e.X / GridX),
        Y = (int)Math.Floor(e.Y / GridY),
        GridX = (int)Math.Floor(e.X / GridX) * GridX,
        GridY = (int)Math.Floor(e.Y / GridY) * GridY
      };
    }

    private void CanvasGreen_MouseDown(object sender, MouseButtonEventArgs e)
    {
      PrimaryMouseDown = true;
      DrawGrid(e.GetPosition(canvasGreen));
    }

    private void DrawGrid(Point e)
    {
      if (!ImageLoaded)
      {
        return;
      }
      if (!PrimaryMouseDown)
      {
        return;
      }
      PrimaryMouseDown = true;

      var pos = MouseGridPos(e);
      if (GridSelection[pos.X, pos.Y])
      {
        return;
      }
      GridSelection[pos.X, pos.Y] = true;

      Debug.WriteLine(pos);
      Rect(pos.GridX, pos.GridY, GridX, GridY, canvasGreen);
    }

    private void CanvasGreen_MouseMove(object sender, MouseEventArgs e)
    {
      DrawGrid(e.GetPosition(canvasGreen));
    }

    private void CanvasGreen_MouseUp(object sender, MouseButtonEventArgs e)
    {
      PrimaryMouseDown = false;
    }

    private void ButtonUndoGreen_Click(object sender, RoutedEventArgs e)
    {
      if (canvasGreen.Children.Count > 0)
      {
        var lastItem = canvasGreen.Children[canvasGreen.Children.Count - 1];
        RedoStack.Push(lastItem);
        canvasGreen.Children.Remove(canvasGreen.Children[canvasGreen.Children.Count - 1]);
      }
    }

    private void RedoGreenButton_Click(object sender, RoutedEventArgs e)
    {
      if(RedoStack.Count > 0)
      {
        canvasGreen.Children.Add(RedoStack.Pop());
      }
    }
  }
}
