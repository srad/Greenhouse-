using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenhousePlusPlusCore.ViewModels
{
  public class ImageListView : ObservableCollection<ImageListViewItem>
  {
    public void AddImage(string file)
    {
      var item = new ImageListViewItem
      {
        File = System.IO.Path.GetFileName(file),
        ImageData = Greenhouse.Vision.ImageHelper.LoadBitmap(file),
        Title = "Image"
      };

      Add(item);
    }

    public void RemoveItem(string filename)
    {
      for (int i = 0; i < Count; i++)
      {
        if (this[i].File == filename)
        {
          this[i].ImageData = null;
          GC.Collect();
          Remove(this[i]);
        }
      }
    }
  }
}
