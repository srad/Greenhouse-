using Greenhouse.Models;
using Greenhouse.ViewModels;
using Greenhouse.Vision;
using Microsoft.Win32;
using System;
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
    private bool ImageLoaded = false;
    private readonly FitlerValues FilterValues = new FitlerValues();

    public MainWindow()
    {
      InitializeComponent();

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

      FilterValBlurRounds.Value = FilterValues.BlurRounds;
      FilterValTheta.Value = FilterValues.ThetaTheshold;
      FitlerValPassfilter.Value = FilterValues.WhiteThreshold;
      FilterValScanline.Value = FilterValues.ScanlineInterpolationWidth;
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
        var randomFilename = Guid.NewGuid() + ".jpg";
        CurrentFile = new ImageManager(randomFilename);
        var targetFile = ImageManager.ImagePath + randomFilename;
        var image = ImageHelper.Resize(filePath, 1024, 786);
        image.Save(targetFile);

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
      imgInput.Source = CurrentFile.Original.BitmapImage.Value;
      ImageProcessor = new ImageProcessor(CurrentFile);

      // Pass filter values
      FilterValues.Green = GreenThreshold.Value;
      FilterValues.Red = RedThreshold.Value;
      FilterValues.BlurRounds = (int)FilterValBlurRounds.Value;
      FilterValues.ThetaTheshold = FilterValTheta.Value;
      FilterValues.WhiteThreshold = (byte)FitlerValPassfilter.Value;
      FilterValues.ScanlineInterpolationWidth = (int)FilterValScanline.Value;

      ImageProcessor.Start(FilterValues);

      imgHistR.Source = CurrentFile.HistR.BitmapImage.Value;
      imgHistG.Source = CurrentFile.HistG.BitmapImage.Value;
      imgHistB.Source = CurrentFile.HistB.BitmapImage.Value;

      filterRed.Source = CurrentFile.FilteredRed.BitmapImage.Value;
      filterGreen.Source = CurrentFile.FilteredGreen.BitmapImage.Value;
      segmentedEarth.Source = CurrentFile.Earth.BitmapImage.Value;
      segmentedLeaf.Source = CurrentFile.Leaf.BitmapImage.Value;

      ImageEdge.Source = CurrentFile.Edge.BitmapImage.Value;
      ImageEdgeOverlay.Source = CurrentFile.EdgeOverlay.BitmapImage.Value;
      ImageBlur.Source = CurrentFile.Blur.BitmapImage.Value;
      ImageHighpass.Source = CurrentFile.Highpass.BitmapImage.Value;

      OriginalImageLabel.Content = $"Original Image: {(int)CurrentFile.Original.BitmapImage.Value.Width}x{(int)CurrentFile.Original.BitmapImage.Value.Height} ({StrFormatByteSize(new FileInfo(CurrentFile.Original.Path).Length)})";
      ImageLoaded = true;
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

    private void ImageEdge_MouseDown(object sender, MouseButtonEventArgs e)
    {
      OpenFile(CurrentFile.Edge.Path);
    }

    private void ImageEdgeOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
      OpenFile(CurrentFile.EdgeOverlay.Path);
    }

    private void ImageBlur_MouseDown(object sender, MouseButtonEventArgs e)
    {
      OpenFile(CurrentFile.Blur.Path);
    }

    private void ImageHighpass_MouseDown(object sender, MouseButtonEventArgs e)
    {
      OpenFile(CurrentFile.Highpass.Path);
    }
  }
}
