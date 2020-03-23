using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace GreenhousePlusPlusCore.ViewModels
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private BitmapImage DefaultImage = new BitmapImage(new Uri("pack://application:,,,/Resources/leaf.png"));

    #region FilterValues
    private FilterValues _FilterValues;
    public FilterValues FilterValues
    {
      set
      {
        _FilterValues = value;
        OnPropertyChange("FilterValBlurRounds");
        OnPropertyChange("FilterValTheta");
        OnPropertyChange("FitlerValPassfilter");
        OnPropertyChange("FilterValScanline");
      }
    }

    public int FilterValBlurRounds
    {
      get => _FilterValues.BlurRounds;
      set
      {
        _FilterValues.BlurRounds = value;
        OnPropertyChange("FilterValBlurRounds");
      }
    }

    public double FilterValTheta
    {
      get => _FilterValues.ThetaTheshold;
      set
      {
        _FilterValues.ThetaTheshold = value;
        OnPropertyChange("FilterValTheta");
      }
    }

    public byte FitlerValPassfilter
    {
      get => _FilterValues.WhiteThreshold;
      set
      {
        _FilterValues.WhiteThreshold = value;
        OnPropertyChange("FitlerValPassfilter");
      }
    }

    public int FilterValScanline
    {
      get => _FilterValues.ScanlineInterpolationWidth;
      set
      {
        _FilterValues.ScanlineInterpolationWidth = value;
        OnPropertyChange("FilterValScanline");
      }
    }
    #endregion

    #region ImageManger
    public ImageManager _File = null;
    private bool _NotLoading = true;
    public DesignerSerializationVisibility LoaderVisibility
    {
      get => _NotLoading ? DesignerSerializationVisibility.Hidden : DesignerSerializationVisibility.Visible;
    }
    public DesignerSerializationVisibility MainWindowVisibility
    {
      get => _NotLoading ? DesignerSerializationVisibility.Visible : DesignerSerializationVisibility.Hidden;
    }
    public bool NotLoading
    {
      get => _NotLoading;
      set
      {
        _NotLoading = value;
        OnPropertyChange("NotLoading");
      }
    }

    private BitmapImage GetOrDefaultImage(Func<BitmapImage> build) => _File == null ? DefaultImage : build();

    public ImageManager File
    {
      set
      {
        _File = value;
        OnPropertyChange("HistogramRed");
        OnPropertyChange("HistogramGreen");
        OnPropertyChange("HistogramBlue");

        OnPropertyChange("FilteredRed");
        OnPropertyChange("FilteredGreen");

        OnPropertyChange("Earth");
        OnPropertyChange("Leaf");

        OnPropertyChange("EdgeImage");
        OnPropertyChange("EdgeOverlayImage");

        OnPropertyChange("BlurImage");
        OnPropertyChange("PassImage");

        OnPropertyChange("ImageLabel");
        OnPropertyChange("OriginalImageFile");

        OnPropertyChange("HistogramRedFile");
        OnPropertyChange("HistogramGreenFile");
        OnPropertyChange("HistogramBlueFile");

        OnPropertyChange("FilteredRedFile");
        OnPropertyChange("FilteredGreenFile");
        OnPropertyChange("EarthFile");
        OnPropertyChange("LeafFile");
        OnPropertyChange("EdgeFile");
        OnPropertyChange("EdgeOverlayFile");
        OnPropertyChange("BlurFile");
        OnPropertyChange("PassFile");
      }
    }

    public BitmapImage OriginalImage
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.Original.Path));
    }
    public string OriginalImageFile { get => _File?.Original.Path; }

    public BitmapImage HistogramRed
    {
      get  => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.HistR.Path));
    }
    public string HistogramRedFile { get => _File?.HistR.Path; }
    
    public BitmapImage HistogramGreen
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.HistG.Path));
    }
    public string HistogramGreenFile { get => _File?.HistG.Path; }

    public BitmapImage HistogramBlue
    {
      get  => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.HistB.Path));
    }
    public string HistogramBlueFile { get => _File?.HistB.Path; }

    public BitmapImage FilteredRed
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.FilteredRed.Path));
    }
    public string FilteredRedFile { get => _File?.FilteredRed.Path; }

    public BitmapImage FilteredGreen
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.FilteredGreen.Path));
    }
    public string FilteredGreenFile { get => _File?.FilteredGreen.Path; }

    public BitmapImage Earth
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.Earth.Path));
    }
    public string EarthFile { get => _File?.Earth.Path; }

    public BitmapImage Leaf
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.Leaf.Path));
    }
    public string LeafFile { get => _File?.Leaf.Path; }

    public BitmapImage EdgeImage
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.Edge.Path));
    }
    public string EdgeFile { get => _File?.Edge.Path; }

    public BitmapImage EdgeOverlayImage
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.PlantTip.Path));
    }
    public string EdgeOverlayFile { get => _File?.PlantTip.Path; }

    public BitmapImage BlurImage
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.Blur.Path));
    }
    public string BlurFile { get => _File?.Blur.Path; }

    public BitmapImage PassImage
    {
      get => GetOrDefaultImage(() => Greenhouse.Vision.ImageHelper.LoadBitmap(_File.Pass.Path));
    }
    public string PassFile { get => _File?.Pass.Path; }

    public string ImageLabel
    {
      get
      {
        if (_File != null)
        {
          return $"Original Image: {_File.Original.Info.Value.Width}x{_File.Original.Info.Value.Height} ({StrFormatByteSize(new System.IO.FileInfo(_File.Original.Path).Length)})";
        }
        return "No file loaded";
      }
    }
    #endregion

    #region Property Handler
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChange(string propertyName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }
    #endregion

    #region Helpers
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
    #endregion
  }
}
