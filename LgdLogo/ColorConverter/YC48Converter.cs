using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;  //BitmapImage  for WPF


namespace LgdLogo
{
  /// <summary>
  /// PixelYCから各形式に変換
  /// </summary>
  public static class YC48Converter
  {
    /*
     * AviUtl
     *               format       　　　　　　　    range
     *               
     *   LOGO_PIXEL  Op__Y   Op__Cb    Op__Cr       0 to 1000
     *               Clr_Y   Clr_Cb    Clr_Cr       0 to 4098    -2048 to 2048 
     * 
     * 
     *   YCbCr           Y       Cb        Cr       0 to 4098    -2048 to 2048 
     *   YC48
     *   PIXEL_YC
     * 
     *   BGR             B        G         R       0 to 255     
     *   PIXEL
     *   
     * 
     *　範囲を超えた値が許されているので、範囲チェックしておかないと
     *　処理によってはオーバーフローするかもしれないので注意。
     * 
     */

    /*       
     * OpenCV                                       U8C3 = unsinged int ch 3
     *   BGR             B        G         R       0 to 255
     *   
     *   YCrCb           Y        Cr       Cb       0 to 255
     *            
     *            ・                ・
     *   OpenCVは CrCb で、AviUtlは CbCr
     *   
     */


    /// <summary>
    /// BGR        -->  YC48           intで計算
    ///  (BGR255)        (YCbCr4096)
    /// </summary>
    public static Int16[] BGR_to_YC48(Byte[] bgr)
    {
      var YCbCr = new Int16[bgr.Length];

      for (int pix = 0; pix < bgr.Length / 3; pix++)
      {
        int B = bgr[pix * 3 + 0], G = bgr[pix * 3 + 1], R = bgr[pix * 3 + 2];

        int Y_ = ((4918 * R + 351) >> 10) + ((9655 * G + 585) >> 10) + ((1875 * B + 523) >> 10);
        int Cb = ((-2775 * R + 240) >> 10) + ((-5449 * G + 515) >> 10) + ((8224 * B + 256) >> 10);
        int Cr = ((8224 * R + 256) >> 10) + ((-6887 * G + 110) >> 10) + ((-1337 * B + 646) >> 10);

        YCbCr[pix * 3 + 0] = ColorConverter.Clamp4096(Y_);
        YCbCr[pix * 3 + 1] = ColorConverter.Clamp2048(Cb);
        YCbCr[pix * 3 + 2] = ColorConverter.Clamp2048(Cr);
      }

      return YCbCr;
    }


    /// <summary>
    /// BGR        -->  YC48           doubleで計算
    ///  (BGR255)        (YCbCr4096)
    /// </summary>
    public static Int16[] BGR_to_YC48(double[] bgr)
    {
      double[] ycbcr255 = ColorConverter.BGR_to_YCbCr(bgr);
      double[] ycbcr4096 = ColorConverter.Range255_to_4096(ycbcr255);
      Int16[] ycbcr_round = ColorConverter.ClampYC48(ycbcr4096);

      return ycbcr_round;
    }


    /// <summary>
    /// YC48        -->  BGR           intで計算
    ///  (YCbCr4096)      (BGR255)
    /// </summary>
    public static Byte[] YC48_to_BGR(Int16[] yc48)
    {
      var BGR = new Byte[yc48.Length];

      for (int pix = 0; pix < yc48.Length / 3; pix++)
      {
        Int16 Y_ = yc48[pix * 3 + 0], Cb = yc48[pix * 3 + 1], Cr = yc48[pix * 3 + 2];

        int R = (255 * Y_ + (((22881 * Cr >> 16) + 3) << 10)) >> 12;
        int G = (255 * Y_ + (((-5616 * Cb >> 16) + (-11655 * Cr >> 16) + 3) << 10)) >> 12;
        int B = (255 * Y_ + (((28919 * Cb >> 16) + 3) << 10)) >> 12;

        BGR[pix * 3 + 0] = ColorConverter.Clamp255(B);
        BGR[pix * 3 + 1] = ColorConverter.Clamp255(G);
        BGR[pix * 3 + 2] = ColorConverter.Clamp255(R);
      }

      return BGR;
    }


    /// <summary>
    /// YC48        -->  BGR           doubleで計算
    ///  (YCbCr4096)      (BGR255)
    /// </summary>
    public static Byte[] YC48_to_BGR(double[] yc48)
    {
      double[] Bgr4096 = ColorConverter.YCbCr_to_BGR(yc48);
      double[] Bgr255 = ColorConverter.Range4096_to_255(Bgr4096);
      Byte[] Bgr_round = ColorConverter.Clamp255(Bgr255);

      return Bgr_round;
    }


    /// <summary>
    /// YC48 -->  Bitmap
    /// </summary>
    public static Bitmap YC48_to_Bitmap(Int16[] yc48, Size size)
    {
      Byte[] Bgr = YC48_to_BGR(yc48);
      Bitmap Bmp = BitmapConverter.BGR_to_Bitmap(Bgr, size);

      return Bmp;
    }


    /// <summary>
    /// LogoPixel( YC48+Op )を黒背景と合成
    /// </summary>
    public static Int16[] LogoPixel_to_YC48(List<LOGO_PIXEL> logo_pixel_List)
    {
      var Black = Preset_LOGO_PIXEL.Black;                //黒背景
      var YCbCr = new Int16[logo_pixel_List.Count * 3];
      int pix_len = logo_pixel_List.Count;

      //不透明度　０．０～１．０なら
      //合成色　＝　前景×不透明度
      //　　　　　　　＋　背景×（１．０　－　不透明度）

      for (int pix = 0; pix < pix_len; pix++)
      {
        var logo_pixel = logo_pixel_List[pix];

        //color
        double clr_y_ = logo_pixel.Clr_Y_;
        double clr_cb = logo_pixel.Clr_Cb;
        double clr_cr = logo_pixel.Clr_Cr;

        //opacity
        //  set range
        //     0 to Max_Op  -->  0.0 to 1.0
        const int Max_Op = LOGO_PIXEL.LOGO_MAX_OPACITY;
        double op_y_ = 1.0 * logo_pixel.Op__Y_ / Max_Op;
        double op_cb = 1.0 * logo_pixel.Op__Cb / Max_Op;
        double op_cr = 1.0 * logo_pixel.Op__Cr / Max_Op;


        double Y_ = clr_y_ * op_y_
          + Black.Clr_Y_ * (1.0 - op_y_);

        double Cb = clr_cb * op_cb
          + Black.Clr_Cb * (1.0 - op_cb);

        double Cr = clr_cr * op_cr
          + Black.Clr_Cr * (1.0 - op_cr);

        YCbCr[pix * 3 + 0] = (Int16)Y_;
        YCbCr[pix * 3 + 1] = (Int16)Cb;
        YCbCr[pix * 3 + 2] = (Int16)Cr;
      }

      return YCbCr;
    }



    /// <summary>
    /// LOGO_PIXEL( YC48+Op ) からBitmap作成
    /// </summary>
    public static Bitmap LogoPixel_to_Bitmap(List<LOGO_PIXEL> logo_pixel_List, Size size)
    {
      var yc48 = YC48Converter.LogoPixel_to_YC48(logo_pixel_List);
      Byte[] bgr = YC48Converter.YC48_to_BGR(yc48);
      Bitmap bmp = BitmapConverter.BGR_to_Bitmap(bgr, size);
      return bmp;
    }


  }
}
