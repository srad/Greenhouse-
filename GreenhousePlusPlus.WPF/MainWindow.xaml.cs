using GreenhousePlusPlus.Core.Models;
using GreenhousePlusPlus.Core.Vision;
using GreenhousePlusPlusCore.ViewModels;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace GreenhousePlusPlusCore
{
  public partial class MainWindow : Window
  {
    private readonly ImageListView ImageListView = new ImageListView();
    private readonly MainWindowViewModel MainWindowViewModel = new MainWindowViewModel();

    private readonly Pipeline Pipeline = new Pipeline();
    private bool ImageLoaded = false;

    public MainWindow()
    {
      InitializeComponent();

      var images = new ImageManager().GetRelativeFilePaths();
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
        Pipeline.Create(filePath);
        ImageListView.AddImage(Pipeline.ImageManager.Thumb.Path);

        ProcessFile();
        MainWindowViewModel.NotLoading = true;
      }
    }

    private void ProcessFile(string file = null)
    {
      MainWindowViewModel.NotLoading = false;
      Pipeline.Process(file);
      imgInput.Source = Greenhouse.Vision.ImageHelper.LoadBitmap(Pipeline.ImageManager.Original.Path);
      MainWindowViewModel.File = Pipeline.ImageManager;
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
      Process.Start(Pipeline.ImageManager.BasePath);
    }

    private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
    {
      var messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
      if (messageBoxResult == MessageBoxResult.Yes)
      {
        try
        {
          var image = sender as System.Windows.Controls.Button;
          var file = (string)image.Tag;
          var paths = new ImageManager();
          paths.Open(file);
          paths.Delete();
          ImageListView.RemoveItem(file);
        }
        catch(Exception ex)
        {
          MessageBox.Show($"Error: {ex.Message}");
        }
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
