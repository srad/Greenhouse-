using GreenhousePlusPlusCore.ViewModels;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using GreenhousePlusPlusCore.Vision;
using GreenhousePlusPlusCore.Models;

namespace GreenhousePlusPlusCore
{
  public partial class MainWindow : Window
  {
    private readonly ImageListView ImageListView = new ImageListView();
    private readonly MainWindowViewModel MainWindowViewModel = new MainWindowViewModel();

    private Pipeline Pipeline = new Pipeline();
    private bool ImageLoaded = false;

    public MainWindow()
    {
      InitializeComponent();

      var images = ImageManager.GetFiles();
      foreach (var image in images)
      {
        ImageListView.AddImage(image);
      }

      ThumbList.ItemsSource = ImageListView;
      DataContext = MainWindowViewModel;

      MainWindowViewModel.FilterValues = Pipeline.FilterValues;
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
        MainWindowViewModel.NotLoading = false;
        // Copy file to project
        var filePath = openFileDialog.FileName;
        Pipeline = new Pipeline(filePath);
        ImageListView.AddImage(Pipeline.ImageFile.Thumb.Path);

        ProcessFile();
        MainWindowViewModel.NotLoading = true;
      }
    }

    private void ProcessFile(string file = null)
    {
      MainWindowViewModel.NotLoading = false;
      Pipeline.Process(file);
      imgInput.Source = Greenhouse.Vision.ImageHelper.LoadBitmap(Pipeline.ImageFile.Original.Path);
      MainWindowViewModel.File = Pipeline.ImageFile;
      ImageLoaded = true;
      MainWindowViewModel.NotLoading = true;
    }

    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
      var image = sender as System.Windows.Controls.Image;
      var file = (string)image.Tag;
      ProcessFile(file);
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

    private void MenuItem_About(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Programmed by Saman Sedighi Rad\nWebsite: https://github.com/srad\nMIT License, 2020");
    }

    private void FilterValueChanged(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
      if (ImageLoaded)
      {
        ProcessFile();
      }
    }
  }
}
