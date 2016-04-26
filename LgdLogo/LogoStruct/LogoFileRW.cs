using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;


#region title
#endregion

namespace LgdLogo
{
  using ExtensionMethod;


  /// <summary>
  /// ロゴ構造体のファイル読み書き
  /// </summary>
  public static class LogoFileRW
  {
    //読
    //File
    public static LogoFile Read_LogoFile(BinaryReader reader)
    {
      try
      {
        var logofile = new LogoFile();
        logofile.Header = StructRW.Read<FileHeader>(reader);

        int ver = logofile.Version();
        for (int i = 0; i < logofile.Header.LogoNum; i++)
        {
          var logodata = Read_LogoData(ver, reader);
          logofile.LogoData.Add(logodata);
        }

        return logofile;
      }
      catch
      {
        throw;
      }
    }

    //読
    //Data
    private static LogoData Read_LogoData(int ver, BinaryReader reader)
    {
      var logodata = new LogoData();

      //v1
      if (ver == 1)
      {
        var header_v1 = StructRW.Read<DataHeader_v1>(reader);
        logodata.Header = header_v1.ToLatestVer();

        //Pixels
        var size = logodata.LogoImageSize();
        var totalpix = size.Width * size.Height;
        for (int i = 0; i < totalpix; i++)
          logodata.Pixels.Add(StructRW.Read<LOGO_PIXEL>(reader));

      }
      //v2
      else if (ver == 2)
      {
        var header_v2 = StructRW.Read<DataHeader_v2>(reader);
        logodata.Header = header_v2.ToLatestVer();

        //Pixels
        var size = logodata.LogoImageSize();
        var totalpix = size.Width * size.Height;
        for (int i = 0; i < totalpix; i++)
          logodata.Pixels.Add(StructRW.Read<LOGO_PIXEL>(reader));

      }
      else
        throw new ArgumentException();

      return logodata;
    }



    //書
    //File
    public static void Write_LogoFile(LogoFile logofile, BinaryWriter writer)
    {
      try
      {
        StructRW.Write(logofile.Header, writer);

        int ver = logofile.Version();
        foreach (var logodata in logofile.LogoData)
        {
          Write_LogoData(ver, logodata, writer);
        }
      }
      catch
      {
        throw;
      }
    }

    //書
    //Data
    private static void Write_LogoData(int ver, LogoData logoData, BinaryWriter writer)
    {
      //v1
      if (ver == 1)
      {
        var header_v1 = new DataHeader_v1(logoData.Header);
        StructRW.Write(header_v1, writer);

        foreach (var pixel in logoData.Pixels)
          StructRW.Write(pixel, writer);
      }
      //v2
      else if (ver == 2)
      {
        var header_v2 = new DataHeader_v2(logoData.Header);
        StructRW.Write(header_v2, writer);

        foreach (var pixel in logoData.Pixels)
          StructRW.Write(pixel, writer);
      }
      else
        throw new ArgumentException();
    }

  }








}
