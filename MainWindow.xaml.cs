using Greenhouse.Models;
using Greenhouse.ViewModels;
using Greenhouse.Vision;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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
    private bool PrimaryMouseDown = false;
    private int GridY = 40;
    private int GridX = 40;
    private bool ImageLoaded = false;
    private bool[,] GridSelection;
    private int GridSizeX;
    private int GridSizeY;
    private Stack<UIElement> RedoStack = new Stack<UIElement>();
    private Size WindowStartSize;

    public MainWindow()
    {
      InitializeComponent();
      WindowStartSize = new Size(ActualHeight, ActualWidth);

      if (Directory.Exists(ImageManager.BasePath))
      {
        var images = Directory.EnumerateFiles(ImageManager.ThumbsPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png"));
        foreach (var image in images)
        {
          ImageListView.AddImage(image);
        }
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
        CurrentFile = new ImageManager(randomFilename);
        var targetFile = ImageManager.ImagePath + randomFilename;
        File.Copy(filePath, targetFile);

        // Create Thumb
        var thumb = ImageHelper.Resize(CurrentFile.Original.Path, 300, 200);
        thumb.Save(CurrentFile.Thumb.Path);
        ImageListView.AddImage(CurrentFile.Thumb.Path);

        ProcessFile();
      }
    }

    [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    public static extern long StrFormatByteSize(
        long fileSize
        , [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)] System.Text.StringBuilder buffer
        , int bufferSize);


    /// <summary>
    /// Converts a numeric value into a string that represents the number expressed as a size value in bytes, kilobytes, megabytes, or gigabytes, depending on the size.
    /// </summary>
    /// <param name="filelength">The numeric value to be converted.</param>
    /// <returns>the converted string</returns>
    public static string StrFormatByteSize(long filesize)
    {
      var sb = new System.Text.StringBuilder(11);
      StrFormatByteSize(filesize, sb, sb.Capacity);
      return sb.ToString();
    }

    private void ProcessFile()
    {
      canvasGreen.Children.Clear();
      imgInput.Source = CurrentFile.Original.BitmapImage.Value;
      ImageProcessor = new ImageProcessor(CurrentFile);

      // Color distribution of the original image
      var filterResult = ImageProcessor.Start(new FilterThesholds(green: GreenThreshold.Value, red: RedThreshold.Value));

      imgHistR.Source = CurrentFile.HistR.BitmapImage.Value;
      imgHistG.Source = CurrentFile.HistG.BitmapImage.Value;
      imgHistB.Source = CurrentFile.HistB.BitmapImage.Value;

      filterRed.Source = CurrentFile.FilteredRed.BitmapImage.Value;
      filterGreen.ImageSource = CurrentFile.FilteredGreen.BitmapImage.Value;
      segmentedEarth.Source = CurrentFile.Earth.BitmapImage.Value;
      segmentedLeaf.Source = CurrentFile.Leaf.BitmapImage.Value;

      ImageEdge.Source = CurrentFile.Edge.BitmapImage.Value;

      var imageSize = GreenImageSize();
      GridSizeX = (int)(imageSize.DisplayWidth / GridX);
      GridSizeY = (int)(imageSize.DisplayedHeight / GridY);

      OriginalImageLabel.Content = $"Original Image: {(int)CurrentFile.Original.BitmapImage.Value.Width}x{(int)CurrentFile.Original.BitmapImage.Value.Height} ({StrFormatByteSize(new FileInfo(CurrentFile.Original.Path).Length)})";

      // Is there some edge left at the right?
      if ((imageSize.DisplayWidth % GridX) > 0)
      {
        GridSizeX += 1;
      }
      if ((imageSize.DisplayedHeight % GridY) > 0)
      {
        GridSizeY += 1;
      }

      GridSelection = new bool[GridSizeX, GridSizeY];
      for (int x = 0; x < GridSizeX; x++)
      {
        for (int y = 0; y < GridSizeY; y++)
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

    private static void Rect(GridPos pos, int width, int height, System.Windows.Controls.Canvas cv)
    {
      if (pos.PixelX < 0 || pos.PixelY < 0)
      {
        return;
      }
      var b = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 255, 0, 255));
      var rect = new System.Windows.Shapes.Rectangle()
      {
        Width = width,
        Height = height,
        Stroke = b,
        StrokeThickness = width / 2
      };

      rect.Tag = pos;
      cv.Children.Add(rect);

      rect.SetValue(System.Windows.Controls.Canvas.LeftProperty, (double)pos.PixelX);
      rect.SetValue(System.Windows.Controls.Canvas.TopProperty, (double)pos.PixelY);
    }

    private GridPos MouseGridPos(Point e)
    {
      return new GridPos
      {
        GridX = (int)Math.Floor(e.X / GridX),
        GridY = (int)Math.Floor(e.Y / GridY),
        PixelX = (int)Math.Floor(e.X / GridX) * GridX,
        PixelY = (int)Math.Floor(e.Y / GridY) * GridY
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
      if (pos.GridX >= GridSizeX || pos.GridY >= GridSizeY)
      {
        return;
      }

      if (GridSelection[pos.GridX, pos.GridY])
      {
        return;
      }
      GridSelection[pos.GridX, pos.GridY] = true;

      Debug.WriteLine(pos);
      Rect(pos, GridX, GridY, canvasGreen);
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
      if (RedoStack.Count > 0)
      {
        canvasGreen.Children.Add(RedoStack.Pop());
      }
    }

    /// <summary>
    /// Calculates the actual image in the cell (which can change size depending on the window size).
    /// </summary>
    /// <returns></returns>
    private SizeInfo GreenImageSize()
    {
      var actualH = GridGreen.ActualHeight;
      var h = CurrentFile.Original.BitmapImage.Value.Height;
      var r = actualH / h;
      var w2 = filterGreen.ImageSource.Width * r;

      return new SizeInfo
      {
        DisplayedHeight = actualH,
        DisplayWidth = w2,
        Height = h,
        Width = CurrentFile.Original.BitmapImage.Value.Width
      };
    }

    private void ButtonClearSelection_Click(object sender, RoutedEventArgs e)
    {
      canvasGreen.Children.Clear();
      for (int x = 0; x < GridSizeX; x++)
      {
        for (int y = 0; y < GridSizeY; y++)
        {
          GridSelection[x, y] = false;
        }
      }
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
      /*
      foreach (var child in canvasGreen.Children)
      {
        var shape = child as System.Windows.Shapes.Rectangle;
        var pos = shape.Tag as GridPos;
        Debug.WriteLine($"{pos}");
      }
      */
      var size = GreenImageSize();
      var ratioX = size.DisplayWidth / size.Width;
      var ratioY = size.DisplayedHeight / size.Height;
      var source = new System.Drawing.Bitmap(CurrentFile.FilteredGreen.Path);
      var destination = new System.Drawing.Bitmap((int)source.Width, (int)source.Height);
      var gridXScaled = GridSizeX * ratioX; 
      var gridYScaled = GridSizeY * ratioY;

      for (int x = 0; x < GridSizeX; x++)
      {
        for (int y = 0; y < GridSizeY; y++)
        {
          if (GridSelection[x, y])
          {
            var scaledX = x * gridXScaled;
            var scaledY = y * gridYScaled;
            var rect = new System.Drawing.Rectangle((int)scaledX, (int)scaledY, (int)gridXScaled, (int)gridYScaled);
            CopyRegionIntoImage(source, rect, ref destination, rect);
          }
        }
      }
      var image = ImageSourceFromBitmap(destination);
      destination.Save(CurrentFile.Selection.Path);
      SelectedImageArea.Source = image;
    }

    public static void CopyRegionIntoImage(System.Drawing.Bitmap srcBitmap, System.Drawing.Rectangle srcRegion, ref System.Drawing.Bitmap destBitmap, System.Drawing.Rectangle destRegion)
    {
      using (System.Drawing.Graphics grD = System.Drawing.Graphics.FromImage(destBitmap))
      {
        grD.DrawImage(srcBitmap, destRegion, srcRegion, System.Drawing.GraphicsUnit.Pixel);
      }
    }

    //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
    [System.Runtime.InteropServices.DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    public static extern bool DeleteObject([System.Runtime.InteropServices.In] IntPtr hObject);

    public System.Windows.Media.ImageSource ImageSourceFromBitmap(System.Drawing.Bitmap bmp)
    {
      var handle = bmp.GetHbitmap();
      try
      {
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
      }
      finally { DeleteObject(handle); }
    }
  }
}
