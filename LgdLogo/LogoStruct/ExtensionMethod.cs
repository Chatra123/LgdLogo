using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;


namespace LgdLogo
{
  using ExtensionMethod;

  namespace ExtensionMethod
  {
    /// <summary>
    /// LogoData、LogoFile用の拡張メソッド
    /// </summary>
    public static class ExMethod
    {
      /// <summary>
      /// ロゴデータヘッダーのバイト数
      /// </summary>
      public static int LogoHeaderSize(this LogoData logoData)
      {
        var type = logoData.Header.GetType();
        var header = Convert.ChangeType(logoData.Header, type);

        return Marshal.SizeOf(header);
      }


      /// <summary>
      /// ロゴピクセルのバイト数
      /// </summary>
      public static int LogoPixelSize(this LogoData logoData)
      {
        var type = logoData.Header.GetType();
        var header = Convert.ChangeType(logoData.Header, type);

        return logoData.Pixels.Count * Marshal.SizeOf(typeof(LOGO_PIXEL));
      }


      /// <summary>
      /// ロゴデータ全体のバイト数　　　ヘッダー＋ピクセル
      /// </summary>
      public static int LogoDataSize(this LogoData logoData)
      {
        return logoData.LogoHeaderSize() + logoData.LogoPixelSize();
      }


      /// <summary>
      /// ロゴ画像の縦横サイズ
      /// </summary>
      public static Size LogoImageSize(this LogoData logoData)
      {
        return new Size(logoData.Header.Width, logoData.Header.Height);
      }


      /// <summary>
      /// ロゴファイルのバージョン取得
      /// </summary>
      public static int Version(this LogoFile logoFile)
      {
        switch (logoFile.Header.Str)
        {
          case FileHeader.Str_v1: return 1;
          case FileHeader.Str_v2: return 2;
          default: return -1;
        }
      }


      /// <summary>
      /// ToString
      /// </summary>
      public static string ToString(this LogoData logoData)
      {
        return logoData.Header.Name;
      }


      /// <summary>
      /// Pixel　→　Bitmap
      /// </summary>
      public static Bitmap GetBitmap(this LogoData logoData)
      {
        var size = logoData.LogoImageSize();
        var bmp = YC48Conv.LogoPixel_to_Bitmap(logoData.Pixels, size);
        return bmp;
      }

    }
  }
}
