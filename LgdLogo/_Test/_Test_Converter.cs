using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
//using LgdLogo;

//using System.Diagnostics;
// var watch = new Stopwatch();
// watch.Restart();
// watch.Stop();
// Console.WriteLine("{0}  {1}ms","case1", watch.ElapsedMilliseconds);
// Trace.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
// System.Threading.Thread.Sleep(0);

#region title
#endregion

namespace LgdLogo.Test
{

  public static class _Test_Converter
  {
    public static void Test()
    {
      Trace.WriteLine("");

      Tweak_Converter.BGR_to_YC48();
      Tweak_Converter.YC48_to_BGR();

      Tweak_Converter.Cycle__BGR_YC48_BGR__calc_int();
      Tweak_Converter.Cycle__BGR_YC48_BGR__calc_double();

      Tweak_Converter.Time__YC48_to_BGR();


      Tweak_Converter.Bitmap_BGR_Bitmap();

      Tweak_Converter.LogoPixel_to_YC48_to_Rgb();


      Tweak_Converter.test_Clamp();


      Trace.WriteLine("");
      Trace.WriteLine("");
      Thread.Sleep(0);
    }
  }




  /// <summary>
  /// 色変換のテスト
  /// </summary>
  static class Tweak_Converter
  {

    #region TestColor
    /// <summary>
    /// プリセットカラー  BGR  CbCr
    /// </summary>
    static class TestColor
    {
      public static double[] dblBGR, dblCbCr;
      public static int[] intBGR, intCbCr;
      public static Int16[] i16BGR, i16CbCr;
      public static Byte[] byteBGR, byteCbCr;

      public static readonly Bitmap image;

      static TestColor()
      {
        //preset color
        var BGR_red = new double[] { 0, 0, 255 };  // red
        var BGR_rgb_plus1 = new double[] {
                             0, 0, 255,            // blue
                             0, 255, 0,            // green
                             255,0,0 ,             // red
                             225, 91, 156 };       // plus1

        //choose one
        dblBGR = BGR_rgb_plus1;

        //convert type
        intBGR = dblBGR.Select(value => (int)value).ToArray();
        i16BGR = dblBGR.Select(value => (Int16)value).ToArray();
        byteBGR = dblBGR.Select(value => (Byte)value).ToArray();

        // BGR  -->  CrCb 
        i16CbCr = YC48Conv.BGR_to_YC48(TestColor.dblBGR);
        dblCbCr = i16CbCr.Select(value => (double)value).ToArray();
        intCbCr = i16CbCr.Select(value => (int)value).ToArray();
        byteCbCr = i16CbCr.Select(value => (Byte)value).ToArray();


        //image file
        image = CreateBMP();
        //image = LoadBMP("block_24BitPerPixel.bmp");
      }


      /// <summary>
      /// BMPファイル作成
      /// </summary>
      private static Bitmap CreateBMP()
      {
        Bitmap bitmap = new Bitmap(2, 2, PixelFormat.Format24bppRgb);
        bitmap.SetPixel(0, 0, Color.FromArgb(255, 0, 0));      //red
        bitmap.SetPixel(0, 1, Color.FromArgb(0, 255, 0));      //green
        bitmap.SetPixel(1, 0, Color.FromArgb(0, 0, 255));      //blue
        bitmap.SetPixel(1, 1, Color.FromArgb(255, 255, 255));  //white
        return bitmap;
      }

      /// <summary>
      /// BMPファイル読込み
      /// </summary>
      private static Bitmap LoadBMP(string filename)
      {
        if (File.Exists(filename) == false)
          throw new IOException();

        var filBbmp = new Bitmap(filename);

        //Format24bppRgb専用
        if (filBbmp.PixelFormat != PixelFormat.Format24bppRgb)
          throw new Exception();

        return filBbmp;
      }
    }
    #endregion






    /// <summary>
    ///  BGR  -->  YC48
    /// </summary>
    public static void BGR_to_YC48()
    {
      //double型で計算
      Int16[] yc48_A = YC48Conv.BGR_to_YC48(TestColor.dblBGR);

      //int型で計算
      Int16[] yc48_B = YC48Conv.BGR_to_YC48(TestColor.byteBGR);

      //diff
      var diff_A = tool.Check_diff(TestColor.dblBGR, yc48_A);
      var diff_B = tool.Check_diff(TestColor.byteBGR, yc48_B);
      var diff = Math.Abs(diff_A - diff_B);

      //show
      string result = (diff <= 1.0) ? "  ○" : "  ☆　☆　☆";
      Trace.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
      Trace.WriteLine(result);
      Trace.WriteLine("");
    }



    /// <summary>
    ///   YC48  -->  BGR
    /// </summary>
    public static void YC48_to_BGR()
    {
      //double型で計算
      Byte[] bgr_A = YC48Conv.YC48_to_BGR(TestColor.dblCbCr);

      //int型で計算
      Byte[] bgr_B = YC48Conv.YC48_to_BGR(TestColor.i16CbCr);

      //diff
      var diff_A = tool.Check_diff(TestColor.dblCbCr, bgr_A);
      var diff_B = tool.Check_diff(TestColor.i16CbCr, bgr_B);
      var diff = Math.Abs(diff_A - diff_B);

      //show
      string result = (diff <= 1.0) ? "  ○" : "  ☆　☆　☆";
      Trace.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
      Trace.WriteLine(result);
      Trace.WriteLine("");
    }



    /// <summary>
    ///  BGR  -->  YC48  --> BGR
    /// </summary>
    public static void Cycle__BGR_YC48_BGR__calc_int()
    {
      // BGR_to_YC48(Byte)は内部の計算でint型で計算する

      //BGR
      Byte[] byte_bgr = TestColor.byteBGR;

      //BGR  -->  YC48
      Int16[] yc48 = YC48Conv.BGR_to_YC48(byte_bgr);

      //          revert
      //          YCbCr  --> BGR
      Byte[] rvt_bgr = YC48Conv.YC48_to_BGR(yc48);

      //check result
      var diff = tool.Check_diff(byte_bgr, rvt_bgr);
      string result = (diff <= 1.0) ? "  ○" : "  ☆　☆　☆";

      //show
      Trace.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
      Trace.WriteLine(String.Format("   bgr    {0}  {1}  {2}", byte_bgr[0], byte_bgr[1], byte_bgr[2]));
      Trace.WriteLine(String.Format("   yc48   {0}  {1}  {2}", yc48[0], yc48[1], yc48[2]));
      Trace.WriteLine(String.Format("   bgr    {0}  {1}  {2}", rvt_bgr[0], rvt_bgr[1], rvt_bgr[2]));
      Trace.WriteLine(result);
      Trace.WriteLine("");
    }



    /// <summary>
    ///  BGR  -->  YC48  --> BGR
    /// </summary>
    public static void Cycle__BGR_YC48_BGR__calc_double()
    {
      // BGR_to_YC48(double)は内部の計算でdouble型で計算する

      //BGR
      double[] dbl_bgr = TestColor.dblBGR;

      //BGR  -->  YC48
      Int16[] i16_yc48 = YC48Conv.BGR_to_YC48(dbl_bgr);

      //double型で計算するYC48_to_BGR()は引数にdoubleしかとれない
      double[] dbl_yc48 = i16_yc48.Select(value => (double)value).ToArray();

      //          revert
      //          YCbCr  --> BGR
      Byte[] rvt_bgr = YC48Conv.YC48_to_BGR(dbl_yc48);

      //check result
      var diff = tool.Check_diff(dbl_bgr, rvt_bgr);
      string result = (diff <= 1.0) ? "  ○" : "  ☆　☆　☆";

      //show
      Trace.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
      Trace.WriteLine(String.Format("   bgr    {0}  {1}  {2}", dbl_bgr[0], dbl_bgr[1], dbl_bgr[2]));
      Trace.WriteLine(String.Format("   yc48   {0}  {1}  {2}", i16_yc48[0], i16_yc48[1], i16_yc48[2]));
      Trace.WriteLine(String.Format("   bgr    {0}  {1}  {2}", rvt_bgr[0], rvt_bgr[1], rvt_bgr[2]));
      Trace.WriteLine(result);
      Trace.WriteLine("");
    }



    /// <summary>
    ///  BGR  -->  YC48 の計測
    ///  double、 intでの計算時間比較
    /// </summary>
    public static void Time__YC48_to_BGR()
    {
      var watch = new Stopwatch();

      //double
      watch.Restart();
      for (int i = 0; i < 1000 * 1000; i++)
      {
        Int16[] yc48_A = YC48Conv.BGR_to_YC48(TestColor.dblBGR);
      }
      watch.Stop();
      Trace.WriteLine(String.Format("{0}  {1}ms", "calc double", watch.ElapsedMilliseconds));       //1127 ms

      //int
      watch.Restart();
      for (int i = 0; i < 1000 * 1000; i++)
      {
        Int16[] yc48_B = YC48Conv.BGR_to_YC48(TestColor.byteBGR);
      }
      watch.Stop();
      Trace.WriteLine(String.Format("{0}  {1}ms", "calc int   ", watch.ElapsedMilliseconds));       //116 ms

    }



    /// <summary>
    ///  Bitmap  -->  BGR  -->  Bitmap
    ///  Bitmap  -->  YC48  -->  Bitmap
    /// </summary>
    public static void Bitmap_BGR_Bitmap()
    {
      //File  -->  Bitmap
      Bitmap srcbmp = TestColor.image;
      Size size = srcbmp.Size;

      //Bitmap  --> BGR
      Byte[] src_bgr = BitmapConv.ToByte(srcbmp);

      //BGR  -->  YC48
      Int16[] src_yc48 = YC48Conv.BGR_to_YC48(src_bgr);


      //BGR  -->  Bitmap
      Bitmap bmp_bgr_1 = BitmapConv.BGR_to_Bitmap(src_bgr, size);
      Bitmap bmp_bgr_2 = BitmapConv.BGR_to_Bitmap__for(src_bgr, size);  //これだけFormat32bppArgb
      Bitmap bmp_bgr_3 = BitmapConv.BGR_to_Bitmap__SetPixel(src_bgr, size);

      //YC48  -->  Bitmap
      //内部でBGR_to_Bitmap(Byte[],size)を使用している
      //bmp_bgr_1の処理と同等
      Bitmap bmp_yc48_1 = YC48Conv.YC48_to_Bitmap(src_yc48, size);


      //check
      //Bitmap  -->  Byte[]
      Byte[] byte_bgr_1 = BitmapConv.ToByte(bmp_bgr_1);
      Byte[] byte_bgr_2 = BitmapConv.ToByte(bmp_bgr_2);    //ArgbなのでAlphaが増えている
      Byte[] byte_bgr_3 = BitmapConv.ToByte(bmp_bgr_3);
      Byte[] byte_yc48_1 = BitmapConv.ToByte(bmp_yc48_1);  //Bitmapから取り出すデータはBGR

      var diff_1 = tool.Check_diff(src_bgr, byte_bgr_1);
      var diff_2 = tool.Check_diff(src_bgr, byte_bgr_2);        //Alphaが追加されているので大きな値になる
      var diff_3 = tool.Check_diff(src_bgr, byte_bgr_3);
      var diff_4 = tool.Check_diff(src_bgr, byte_yc48_1);       //Bitmapから取り出すデータはBGR

      var diff = Math.Abs(diff_1) + Math.Abs(diff_3) + Math.Abs(diff_4);

      //show
      string result = (diff <= 1.0) ? "  ○" : "  ☆　☆　☆";
      Trace.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
      Trace.WriteLine(result);
      Trace.WriteLine("");

      //save file
      //bmp_bgr_1.Save("bmp_bgr_1.bmp", ImageFormat.Bmp);
      //bmp_bgr_2.Save("bmp_bgr_2.bmp", ImageFormat.Bmp);
      //bmp_bgr_3.Save("bmp_bgr_3.bmp", ImageFormat.Bmp);
      //bmp_yc48_1.Save("bmp_yc48_1.bmp", ImageFormat.Bmp);
      Thread.Sleep(0);
    }




    /// <summary>
    ///  LOGO_PIXEL  -->  YC48  -->  RGR
    /// </summary>
    public static void LogoPixel_to_YC48_to_Rgb()
    {
      List<LOGO_PIXEL> logo_pixel_list = new List<LOGO_PIXEL> {
          Preset_LOGO_PIXEL.Red, Preset_LOGO_PIXEL.Green,
          Preset_LOGO_PIXEL.Blue,  Preset_LOGO_PIXEL.Black,
        };

      Int16[] yc48 = YC48Conv.LogoPixel_to_YC48(logo_pixel_list);
      Byte[] bgr = YC48Conv.YC48_to_BGR(yc48);
      Byte[] rgb = ColorConv.Swap_RGB_to_BGR(bgr);

      //
      //check result vlaue yourself
      //

      Trace.WriteLine("");
    }



    /// <summary>
    ///  Clamp
    /// </summary>
    public static void test_Clamp()
    {
      int[] int_valueList = Enumerable.Range(-3000, 3000).ToArray();
      //int[] int_valueList = new int[] { -3000, -2048, -255, 0, 255, 2048, 3000 };
      int count_err = 0;

      // BGR
      foreach (int value in int_valueList)
      {
        const int Min = 0, Max = 255;
        Byte clamped = ColorConv.Clamp255(value);

        //out of range?
        if (clamped < Min || Max < clamped)
        {
          count_err++;
        }
      }


      // Y
      foreach (int value in int_valueList)
      {
        const int Min = 0, Max = 4096;
        Int16 clamped = ColorConv.Clamp4096(value);

        //out of range?
        if (clamped < Min || Max < clamped)
        {
          count_err++;
        }
      }


      // CbCr
      foreach (int value in int_valueList)
      {
        const int Min = -2048, Max = 2048;
        Int16 clamped = ColorConv.Clamp2048(value);

        //out of range?
        if (clamped < Min || Max < clamped)
        {
          count_err++;
        }
      }


      //Single
      //  -3000 to 3000    -->    -2.0 to 2.0 
      double[] dbl_valueList = int_valueList.Select(value => 1.0 * value / 1500).ToArray();

      foreach (int value in dbl_valueList)
      {
        const double Min = 0.0, Max = 1.0;
        Single clamped = ColorConv.ClampOne(value);

        //out of range?
        if (clamped < Min || Max < clamped)
        {
          count_err++;
        }
      }

      //show
      string result = (count_err == 0) ? "  ○" : "  ☆　☆　☆";
      Trace.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
      Trace.WriteLine(result);
      Trace.WriteLine("");
    }




    #region TestTool
    /// <summary>
    /// テスト用ツール
    /// </summary>
    static class tool
    {
      /// <summary>
      /// 誤差を計算
      /// </summary>
      public static double Check_diff<T1, T2>(T1[] A, T2[] B)
      {
        double[] dbl_A = A.Select(value => Convert.ToDouble(value)).ToArray(); ;
        double[] dbl_B = B.Select(value => Convert.ToDouble(value)).ToArray(); ;

        return Check_diff(dbl_A, dbl_B);
      }

      /// <summary>
      /// 誤差を計算
      /// </summary>
      public static double Check_diff(double[] A, double[] B)
      {
        if (A.Count() != B.Count()) return double.MaxValue;

        const double eps = 0.001;         //doubleによる誤差
        const double err_allowable = 1.0; //許容誤差

        double diff_sum = 0.0;
        int count = 0;

        for (int i = 0; i < A.Count(); i++)
        {
          double diff = Math.Abs(A[i] - B[i]);
          if (diff <= eps) continue;
          if (diff <= err_allowable) continue;         //許容できる誤差

          //誤差のあるピクセルのみ計算
          diff_sum += diff;
          count++;
        }

        if (count != 0)
          return diff_sum / count;
        else
          return 0;
      }




    }
    #endregion


  }
}
