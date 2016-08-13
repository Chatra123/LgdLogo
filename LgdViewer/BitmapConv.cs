using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;



namespace LgdLogo
{

  public static class BitmapConv_WPF
  {
    /// <summary>
    /// Bitmap  -->  BitmapImage      WPF用
    /// </summary>
    public static BitmapImage ToBitmapImage(Bitmap bmp)
    {
      using (var memory = new MemoryStream())
      {
        bmp.Save(memory, ImageFormat.Bmp);
        memory.Position = 0;

        var bmpimg = new BitmapImage();
        bmpimg.BeginInit();
        bmpimg.StreamSource = memory;
        bmpimg.CacheOption = BitmapCacheOption.OnLoad;
        bmpimg.EndInit();
        return bmpimg;
      }
    }


    /// <summary>
    /// Bitmap  -->  BitmapSource      WPF用
    /// </summary>
    public static BitmapSource ToBitmapSource(Bitmap bmp)
    {
      var bmpsrc =
        System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        bmp.GetHbitmap(),
        IntPtr.Zero,
        System.Windows.Int32Rect.Empty,
        BitmapSizeOptions.FromWidthAndHeight(bmp.Size.Width, bmp.Size.Height));

      return bmpsrc;
    }




  }
}
