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
  public partial class MainWindow : Window
  {
    private ImageProcessor ImageProcessor;
    private ImageManager CurrentFile;

    private readonly ImageListView ImageListView = new ImageListView();
    private readonly MainWindowViewModel MainWindowViewModel = new MainWindowViewModel();

    private bool ImageLoaded = false;
    private readonly FilterValues FilterValues = new FilterValues();

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

      ThumbList.ItemsSource = ImageListView;
      DataContext = MainWindowViewModel;

      MainWindowViewModel.FilterValues = FilterValues;
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

    private void ProcessFile()
    {
      imgInput.Source = CurrentFile.Original.BitmapImage.Value;
      ImageProcessor = new ImageProcessor(CurrentFile);
      ImageProcessor.Start(FilterValues);
      MainWindowViewModel.File = CurrentFile;
      ImageLoaded = true;
    }

    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
      var image = sender as System.Windows.Controls.Image;
      var file = (string)image.Tag;
      CurrentFile = new ImageManager(file);
      ProcessFile();
    }

    private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(ImageManager.BasePath);
    }

    private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
    {
      var messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
      if (messageBoxResult == MessageBoxResult.Yes)
      {
        var image = sender as System.Windows.Controls.Button;
        var file = (string)image.Tag;
        var paths = new ImageManager(file);
        ImageListView.RemoveItem(file);
        paths.Delete();
      }
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

    public void ViewFile(object sender, EventArgs e)
    {
      if (!ImageLoaded)
      {
        return;
      }
      var element = sender as FrameworkElement;
      var file = (string)element.Tag;
      if (file != null)
      {
        OpenFile(file);
      }
    }
  }
}
