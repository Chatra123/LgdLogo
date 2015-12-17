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
//参照設定　→　参照の追加　→　フレームワーク
//  System.Drawing
//  System.Xaml
//  PresentationCore
//  WindowsBase
//


namespace LgdLogo
{

  public static class BitmapConverter
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



    /// <summary>
    /// Bitmap  -->  Byte[] 
    /// </summary>
    ///     Format24bppRgb、Format32bppRgb　両対応
    public static Byte[] ToByte(Bitmap bmp)
    {
      int w = bmp.Size.Width, h = bmp.Size.Height;

      //Byte per pixel
      //    PixelFormat.Format24bppRgb   -->  3 Byte
      //    PixelFormat.Format32bppArgb  -->  4 Byte
      int Bpp = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;

      var bmpdata = bmp.LockBits(
        new Rectangle(0, 0, w, h),
        ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

      byte[] buf = new byte[w * h * Bpp];

      //ラインごとに処理
      //メモリが不連続。各行の後ろに空のデータがある。
      for (int y = 0; y < h; y++)
      {
        IntPtr line = bmpdata.Scan0 + bmpdata.Stride * y;
        Marshal.Copy(line, buf, w * y * Bpp, w * Bpp);
      }
      bmp.UnlockBits(bmpdata);

      return buf;
    }



    /// <summary>
    /// Bitmap  -->  Byte[]    GetPixel
    /// </summary>
    ///     Format24bppRgb限定
    public static Byte[] ToByte__GetPixel(Bitmap bmp)
    {
      System.Diagnostics.Trace.Assert(
        bmp.PixelFormat == PixelFormat.Format24bppRgb);


      byte[] buf = new byte[bmp.Width * bmp.Height * 3];

      for (int y = 0; y < bmp.Height; y++)
      {
        for (int x = 0; x < bmp.Width; x++)
        {
          Color color = bmp.GetPixel(x, y);
          int pix = y * bmp.Width + x;
          buf[pix * 3 + 0] = color.R;
          buf[pix * 3 + 1] = color.G;
          buf[pix * 3 + 2] = color.B;
        }
      }
      return buf;
    }


    /// <summary>
    /// BGR  -->  Bitmap    Marshal.Copy
    /// </summary>
    ///     Marshal.Copyで１行ずつコピー
    /// 　  Format24bppRgbを出力
    /// 　
    /// ○bmpdata.Strideについて
    /// 　  PixelFormat.Format24bppRgbはWidthが４の倍数でないとラインの後ろに空のデータがいれられる。
    /// 　  PixelFormat.Format32bppArgbならWidthに関係なく常に４の倍数。
    /// 　　
    /// 　  一度に処理しても、ラインごとに処理しても処理速度は変わらずどちらも十分早い。
    /// 　  bmpdata.Strideが負になることは基本的に無いと思うので、考慮しない。
    /// 　  
    ///     Format24bppRgb限定
    public static Bitmap BGR_to_Bitmap(Byte[] bgr, Size size)
    {
      int w = size.Width, h = size.Height;
      var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);

      //lock
      var bmpdata = bmp.LockBits(
          new Rectangle(Point.Empty, size),
          ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

      //copy
      for (int y = 0; y < h; y++)
      {
        IntPtr line = bmpdata.Scan0 + bmpdata.Stride * y;
        Marshal.Copy(bgr, w * y * 3, line, w * 3);
      }

      bmp.UnlockBits(bmpdata);

      return bmp;
    }



    /// <summary>
    /// BGR  -->  Bitmap    for
    /// </summary>
    ///     データを取出　⇒　forでコピー　⇒　データを戻す
    ///     Strideの処理をはぶくためにFormat32bppArgbのみ
    ///     
    ///     Format32bppArgb限定
    public static Bitmap BGR_to_Bitmap__for(Byte[] bgr, Size size)
    {
      int w = size.Width, h = size.Height;
      var bmp = new Bitmap(w, h);
      var buf = new Byte[w * h * 4];

      //input
      //Bitmapをbufにコピー
      //ここでは空のBitmapなのでとばしてもいい。
      var bmpdata = bmp.LockBits(
        new Rectangle(Point.Empty, size),
        ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

      Marshal.Copy(bmpdata.Scan0, buf, 0, buf.Length);
      bmp.UnlockBits(bmpdata);


      //edit image
      //値の加工はしない。コピ－するだけ。
      for (int y = 0; y < h; y++)
      {
        for (int x = 0; x < w; x++)
        {
          int pix = y * w + x;

          buf[pix * 4 + 0] = bgr[pix * 3 + 0];  // B
          buf[pix * 4 + 1] = bgr[pix * 3 + 1];  // G
          buf[pix * 4 + 2] = bgr[pix * 3 + 2];  // R
          buf[pix * 4 + 3] = 0xFF;              // A
        }
      }

      //output
      //bufを出力用のBitmapにコピー
      var result = new Bitmap(w, h);
      bmpdata = result.LockBits(
        new Rectangle(Point.Empty, size),
        ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
      Marshal.Copy(buf, 0, bmpdata.Scan0, buf.Length);
      result.UnlockBits(bmpdata);

      return result;
    }



    /// <summary>
    /// BGR  -->  Bitmap    SetPixel
    /// </summary>
    ///     SetPixelでコピー
    ///     Format24bppRgb限定
    public static Bitmap BGR_to_Bitmap__SetPixel(Byte[] bgr, Size size)
    {
      int w = size.Width, h = size.Height;
      Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

      for (int y = 0; y < h; y++)
      {
        for (int x = 0; x < w; x++)
        {
          int pix = y * w + x;

          bitmap.SetPixel(
            x, y, Color.FromArgb(
            bgr[pix * 3 + 2],      //R
            bgr[pix * 3 + 1],      //G
            bgr[pix * 3 + 0]));    //B
        }
      }
      return bitmap;
    }


  }
}
