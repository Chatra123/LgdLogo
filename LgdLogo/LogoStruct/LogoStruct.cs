using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.ObjectModel;

#region title
#endregion

namespace LgdLogo
{
  using LgdLogo.ExtensionMethod;


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public class LogoFile
  {
    public FileHeader Header = new FileHeader();
    public List<LogoData> LogoData = new List<LogoData>();
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public class FileHeader
  {
    //constはlgdファイルに保存されない。
    public const string Str_v1 = "<logo data file ver0.1>";
    public const string Str_v2 = "<logo data file ver0.2>";
    public const int Str_FixSize = 28;


    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Str_FixSize)]
    public string Str;

    //LogoNumはビッグエンディアンで格納されている。
    private int _logonum;
    public int LogoNum
    {
      //  get  little
      get { return System.Net.IPAddress.NetworkToHostOrder(_logonum); }

      //  set  big
      set { _logonum = System.Net.IPAddress.HostToNetworkOrder(value); }
    }
  }



  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public class LogoData
  {
    public DataHeader Header;
    public List<LOGO_PIXEL> Pixels = new List<LOGO_PIXEL>();

    //Imageはファイルに保存しない
    private Bitmap _image;
    public Bitmap Image
    {
      get
      {
        _image = _image ?? this.GetBitmap();
        return _image;
      }
    }


  }



  public interface IDataHeader
  {
    void SetFromLatestVer(DataHeader header_latest);
    DataHeader ToLatestVer();
    void Validate();
  }

  public class DataHeader : DataHeader_v2
  {
    //  implement nothing
    //  inherit latest DataHeader
    //  publish to external class
  }


  #region DataHeader_v1
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public class DataHeader_v1 : IDataHeader
  {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string Name;              //終端の\0も含めて32byte
    public Int16 X, Y;               /* ロゴの位置             */
    public Int16 Height, Width;      /* ロゴの高さ・幅         */
    public Int16 FadeIn, FadeOut;    /* デフォルトのFadeIn/Out */
    public Int16 Start, End;         /* デフォルトの開始･終了  */


    public DataHeader_v1() { }
    public DataHeader_v1(DataHeader header_latest)
    {
      SetFromLatestVer(header_latest);
    }

    /// <summary>
    ///  Latest  -->  v1
    /// </summary>
    public void SetFromLatestVer(DataHeader header_latest)
    {
      this.Name = header_latest.Name;
      this.X = header_latest.X;
      this.Y = header_latest.Y;
      this.Height = header_latest.Height;
      this.Width = header_latest.Width;

      this.FadeIn = header_latest.FadeIn;
      this.FadeOut = header_latest.FadeOut;
      this.Start = header_latest.Start;
      this.End = header_latest.End;
      Validate();
    }

    /// <summary>
    /// v1  -->  Latest
    /// </summary>
    public DataHeader ToLatestVer()
    {
      var header_latest = new DataHeader();
      header_latest.Name = this.Name;
      header_latest.X = this.X;
      header_latest.Y = this.Y;
      header_latest.Height = this.Height;
      header_latest.Width = this.Width;

      header_latest.FadeIn = this.FadeIn;
      header_latest.FadeOut = this.FadeOut;
      header_latest.Start = this.Start;
      header_latest.End = this.End;
      header_latest.Reserved = null;
      header_latest.Validate();
      return header_latest;
    }

    /// <summary>
    /// Nameのサイズを32byte以下に
    /// </summary>
    public void Validate()
    {
      Name = StrUtility.TrimSize(Name, 32);
    }

  }
  #endregion



  #region DataHeader_v2
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public class DataHeader_v2 : IDataHeader
  {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Name;              //終端の\0も含めて256byte
    public Int16 X, Y;               /* ロゴの位置             */
    public Int16 Height, Width;      /* ロゴの高さ・幅         */
    public Int16 FadeIn, FadeOut;    /* デフォルトのFadeIn/Out */
    public Int16 Start, End;         /* デフォルトの開始･終了  */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 240)]
    public Byte[] Reserved;


    public DataHeader_v2() { }
    public DataHeader_v2(DataHeader header_latest)
    {
      SetFromLatestVer(header_latest);
    }

    /// <summary>
    ///  Latest  -->  v2
    /// </summary>
    public void SetFromLatestVer(DataHeader header_latest)
    {
      this.Name = header_latest.Name;
      this.X = header_latest.X;
      this.Y = header_latest.Y;
      this.Height = header_latest.Height;
      this.Width = header_latest.Width;

      this.FadeIn = header_latest.FadeIn;
      this.FadeOut = header_latest.FadeOut;
      this.Start = header_latest.Start;
      this.End = header_latest.End;
      this.Reserved = header_latest.Reserved;
      Validate();
    }

    /// <summary>
    /// v2  -->  Latest
    /// </summary>
    public DataHeader ToLatestVer()
    {
      var header_latest = new DataHeader();
      header_latest.Name = this.Name;
      header_latest.X = this.X;
      header_latest.Y = this.Y;
      header_latest.Height = this.Height;
      header_latest.Width = this.Width;

      header_latest.FadeIn = this.FadeIn;
      header_latest.FadeOut = this.FadeOut;
      header_latest.Start = this.Start;
      header_latest.End = this.End;
      header_latest.Reserved = this.Reserved;
      header_latest.Validate();
      return header_latest;
    }

    /// <summary>
    /// Nameのサイズを256byte以下に
    /// </summary>
    public void Validate()
    {
      Name = StrUtility.TrimSize(Name, 256);
    }
  }
  #endregion




  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public class LOGO_PIXEL
  {
    public const int LOGO_MAX_OPACITY = 1000;

    public Int16 Op__Y_;        /* 不透明度（輝度）            */
    public Int16 Clr_Y_;        /* 輝度              0～4096   */
    public Int16 Op__Cb;        /* 不透明度（青）              */
    public Int16 Clr_Cb;        /* 色差（青）    -2048～2048   */
    public Int16 Op__Cr;        /* 不透明度（赤）              */
    public Int16 Clr_Cr;        /* 色差（赤）    -2048～2048   */
  }



  /// <summary>
  /// Preset Color
  /// </summary>
  public static class Preset_LOGO_PIXEL
  {
    public static LOGO_PIXEL Red { get { return Rgb255_to_LogoPixel(new Byte[] { 255, 0, 0 }); } }
    public static LOGO_PIXEL Green { get { return Rgb255_to_LogoPixel(new Byte[] { 0, 255, 0 }); } }
    public static LOGO_PIXEL Blue { get { return Rgb255_to_LogoPixel(new Byte[] { 0, 0, 255 }); } }
    public static LOGO_PIXEL Gray { get { return Rgb255_to_LogoPixel(new Byte[] { 200, 200, 200 }); } }
    public static LOGO_PIXEL Black { get { return Rgb255_to_LogoPixel(new Byte[] { 0, 0, 0 }); } }

    private static LOGO_PIXEL Rgb255_to_LogoPixel(Byte[] rgb)
    {
      Byte[] bgr = ColorConv.Swap_RGB_to_BGR(rgb);
      Int16[] yc48 = YC48Conv.BGR_to_YC48(new double[] { bgr[0], bgr[1], bgr[2] });

      var logo_pixel = new LOGO_PIXEL()
      {
        Op__Y_ = LOGO_PIXEL.LOGO_MAX_OPACITY,
        Clr_Y_ = yc48[0],
        Op__Cb = LOGO_PIXEL.LOGO_MAX_OPACITY,
        Clr_Cb = yc48[1],
        Op__Cr = LOGO_PIXEL.LOGO_MAX_OPACITY,
        Clr_Cr = yc48[2],
      };

      return logo_pixel;
    }
  }




  static class StrUtility
  {
    /// <summary>
    /// 文字列をbyteSize以内に切り詰める。　Shift_JIS
    /// </summary>
    /// <param name="byteSize">文字列＋'\0'　のバイトサイズ</param>
    public static string TrimSize(string str, int byteSize)
    {
      if (byteSize <= 1) throw new ArgumentException("byteSizeは２以上にしてください。");

      var enc = Encoding.GetEncoding("Shift_JIS");
      string trimmed = str;

      while (true)
      {
        int curSize = enc.GetByteCount(trimmed + '\0');
        int trimSize = curSize - byteSize;

        if (0 < trimSize)
        {
          //後ろの１文字カット
          //　カットする文字が全角、半角かはわからない。
          trimmed = trimmed.Substring(0, trimmed.Length - 1);
        }
        else
          break;
      }

      return trimmed;
    }
  }








}
