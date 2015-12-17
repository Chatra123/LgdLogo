using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


/*
 *               format       type      range
 *               
 * OpenCV                               U8C3 = unsinged int ch 3
 *               B  G  R      Byte      0 to 255
 *               Y  Cr Cb     Byte      0 to 255
 *                  
 * AviUtl
 *   PIXEL       B  G  R      Byte      0 to 255
 *   PIXEL_YC    Y  Cb Cr     Int16     0 to 4096   -2048 to 2048
 * 
 * 
 *   OpenCVは CrCb で、AviUtlは CbCr
 *   
 */

namespace LgdLogo
{
  /// <summary>
  /// 色変換
  /// 基本的に全てdoubleで計算する。
  /// </summary>
  public static class ColorConverter
  {
    /// <summary>
    /// RGBをYCrCbに変換
    /// </summary>
    /// <param name="bgr">変換元のbgr（bgr：0 to N）</param>
    /// <returns>変換後のYCrCb（Y：0 to N　　CrCb：-N/2 to N/2）</returns>
    public static double[] BGR_to_YCbCr(double[] bgr)
    {
      /*
       * RGB        0  to    N
       * Y          0  to    N 
       * CbCr    -N/2  to  N/2
       */
      var YCbCr = new double[bgr.Length];

      for (int pix = 0; pix < bgr.Length / 3; pix++)
      {
        double B = bgr[pix * 3 + 0], G = bgr[pix * 3 + 1], R = bgr[pix * 3 + 2];

        double Y_ = +0.299 * R + 0.587 * G + 0.114 * B;
        double Cb = -0.169 * R - 0.332 * G + 0.500 * B;
        double Cr = +0.500 * R - 0.419 * G - 0.081 * B;

        YCbCr[pix * 3 + 0] = Y_;
        YCbCr[pix * 3 + 1] = Cb;
        YCbCr[pix * 3 + 2] = Cr;
      }

      return YCbCr;
    }

    /// <summary>
    /// YCrCbをRGBに変換
    /// </summary>
    /// <param name="cbcr">変換元のcbcr（Y：0 to N　　CrCb：-N/2 to N/2）</param>
    /// <returns>変換後のRGB（RGB：0 to N）</returns>
    public static double[] YCbCr_to_BGR(double[] cbcr)
    {
      /*
       * RGB        0  to    N
       * Y          0  to    N 
       * CbCr    -N/2  to  N/2
       */
      var BGR = new double[cbcr.Length];

      for (int pix = 0; pix < cbcr.Length / 3; pix++)
      {
        double Y_ = cbcr[pix * 3 + 0], Cb = cbcr[pix * 3 + 1], Cr = cbcr[pix * 3 + 2];

        double R = Y_ + 1.402 * Cr;
        double G = Y_ - 0.344 * Cb - 0.714 * Cr;
        double B = Y_ + 1.772 * Cb;

        BGR[pix * 3 + 0] = B;
        BGR[pix * 3 + 1] = G;
        BGR[pix * 3 + 2] = R;
      }

      return BGR;
    }


    //---------------------------------------------------------------------------------
    /// <summary>
    /// RGB  -->  BGR　　入れ替え
    /// </summary>
    public static Byte[] Swap_RGB_to_BGR(Byte[] rgb)
    {
      var BGR = new Byte[rgb.Length];

      for (int pix = 0; pix < rgb.Length / 3; pix++)
      {
        Byte R = rgb[pix * 3 + 0], G = rgb[pix * 3 + 1], B = rgb[pix * 3 + 2];

        BGR[pix * 3 + 0] = B;
        BGR[pix * 3 + 1] = G;
        BGR[pix * 3 + 2] = R;
      }
      return BGR;
    }


    /// <summary>
    /// YCrCb  -->  YCbCr　　入れ替え
    /// </summary>
    public static Int16[] Swap_CrCb_to_CbCr(Int16[] crcb)
    {
      var CbCr = new Int16[crcb.Length];

      for (int pix = 0; pix < crcb.Length / 3; pix++)
      {
        Int16 Y = crcb[pix * 3 + 0], Cr = crcb[pix * 3 + 1], Cb = crcb[pix * 3 + 2];

        CbCr[pix * 3 + 0] = Y;
        CbCr[pix * 3 + 1] = Cb;
        CbCr[pix * 3 + 2] = Cr;
      }
      return CbCr;
    }





    //---------------------------------------------------------------------------------
    /// <summary>
    /// Clamp  generic
    /// </summary>
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
      if (value.CompareTo(min) < 0) return min;
      else if (value.CompareTo(max) > 0) return max;
      else return value;
    }


    /// <summary>
    /// 値を０から２５５に丸める。
    /// </summary>
    public static Byte[] Clamp255(double[] bgr)
    {
      Byte[] clamp = bgr.Select(value => Clamp255((int)Math.Round(value))).ToArray();
      return clamp;
    }
    public static Byte Clamp255(double value)
    {
      return (Byte)Clamp((int)Math.Round(value), 0, 255);
    }
    public static Byte Clamp255(int value)
    {
      return (Byte)Clamp(value, 0, 255);
    }



    /// <summary>
    /// 値を０から４０９６、－２０４８から２０４８に丸める。
    /// </summary>
    public static Int16[] ClampYC48(double[] yc48)
    {
      var clamp = new Int16[yc48.Length];

      for (int pix = 0; pix < yc48.Length / 3; pix++)
      {
        double y_ = yc48[pix * 3 + 0];
        double cb = yc48[pix * 3 + 1];
        double cr = yc48[pix * 3 + 2];

        clamp[pix * 3 + 0] = Clamp4096((int)Math.Round(y_));
        clamp[pix * 3 + 1] = Clamp2048((int)Math.Round(cb));
        clamp[pix * 3 + 2] = Clamp2048((int)Math.Round(cr));
      }

      return clamp;
    }

    public static Int16[] ClampYC48(Int16[] yc48)
    {
      var clamp = new Int16[yc48.Length];

      for (int pix = 0; pix < yc48.Length / 3; pix++)
      {
        Int16 y_ = yc48[pix * 3 + 0];
        Int16 cb = yc48[pix * 3 + 1];
        Int16 cr = yc48[pix * 3 + 2];

        clamp[pix * 3 + 0] = Clamp4096(y_);
        clamp[pix * 3 + 1] = Clamp2048(cb);
        clamp[pix * 3 + 2] = Clamp2048(cr);
      }

      return clamp;
    }

    //Y 4096
    public static Int16 Clamp4096(int value)
    {
      return (Int16)Clamp(value, 0, 4096);
    }
    //CbCr 2048
    public static Int16 Clamp2048(int value)
    {
      return (Int16)Clamp(value, -2048, 2048);
    }



    /// <summary>
    /// 値を０.０から１．０に丸める。
    /// </summary>
    public static Single[] ClampOne(double[] bgr)
    {
      Single[] clamp = bgr.Select(value => ClampOne(value)).ToArray();
      return clamp;
    }
    //Single    Math.Round(double)したら 0 or 1 になってしまう。
    public static Single ClampOne(double value)
    {
      return (Single)Clamp(value, 0.0, 1.0);
    }


    //---------------------------------------------------------------------------------
    /// <summary>
    /// 値の範囲を２５５から４０９６に変更する。
    /// </summary>
    /// <param name="bgr">変更元の配列</param>
    /// <returns>値の範囲が変更された配列</returns>
    ///	AviUtlプラグインでは、
    ///    Y  :     0 ～ 4096
    ///    Cr : -2048 ～ 2048
    ///    Cb : -2048 ～ 2048
    public static double[] Range255_to_4096(double[] bgr)
    {
      return RangeA_to_B(bgr, 255, 4096);
    }


    /// <summary>
    /// 値の範囲を４０９６から２５５に変更する。
    /// </summary>
    /// <param name="cbcr">変更元の配列</param>
    /// <returns>値の範囲が変更された配列</returns>
    public static double[] Range4096_to_255(double[] cbcr)
    {
      return RangeA_to_B(cbcr, 4096, 255);
    }


    /// <summary>
    /// 値の範囲を変更する。（全要素を同じ範囲に変更）
    /// </summary>
    /// <param name="ary">変更元の配列</param>
    /// <param name="fromA">変更前の上限　Ａ（２５５）</param>
    /// <param name="toB">変更後の上限　  Ｂ（４０９６）</param>
    /// <returns>値の範囲が変更された配列</returns>
    private static double[] RangeA_to_B(double[] ary, double fromA, double toB)
    {
      for (int i = 0; i < ary.Length; i++)
      {
        ary[i] = ary[i] / fromA * toB;
      }
      return ary;
    }


  }//class
}
