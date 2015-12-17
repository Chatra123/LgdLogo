using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using LgdLogo;



namespace LgdLogo.Test
{
  using LgdLogo.ExtensionMethod;

  public class _Test_LogoStruct
  {

    /// <summary>
    ///　ロゴ構造体を作成、保存、読込をして同一のデータであることを確認する。
    /// </summary>
    public static void Test()
    {
      var manager = new LogoFileManager();

      Console.WriteLine("v1");
      Console.WriteLine();
      var logoObj_v1 = CreaetSample_v1();
      Test_Core("file_v1.lgd", logoObj_v1);

      Console.WriteLine("------------------------------------------");
      Console.WriteLine("v2");
      Console.WriteLine();
      var logoObj_v2 = CreaetSample_v2();
      Test_Core("file_v2.lgd2", logoObj_v2);

    }

    /// <summary>
    ///　ロゴ構造体を作成、保存、読込をして同一のデータであることを確認する。
    /// </summary>
    private static void Test_Core(string filename, LogoFile srcLogoObj)
    {
      var manager = new LogoFileManager();
      //書
      manager.Save(filename, srcLogoObj);
      //読
      var readLogoObj = manager.Load(filename);

      Console.WriteLine("src LogoObj");
      Console.WriteLine(GetLogoInfo(srcLogoObj));
      Console.WriteLine("Read as LogoObj");
      Console.WriteLine(GetLogoInfo(readLogoObj));


      /// 比較
      //・bytes_srcLogoObj :  構造体から直接バイト配列に変換。
      //・bytes_ReadLogoObj:  ファイルを構造体として読み込み、バイト配列に変換。
      //・bytes_DirectFile :  ファイルから直接バイト配列として読み込む。
      // ３つを比較する。 
      var bytes_srcLogoObj = ToBytes_LogoFile(srcLogoObj).ToArray();
      var bytes_ReadLogoObj = ToBytes_LogoFile(readLogoObj).ToArray();
      var bytes_DirectFile = File.ReadAllBytes(filename).ToArray();

      bool same_ReadLogoObj = Compare(bytes_srcLogoObj, bytes_ReadLogoObj);
      bool same_DirectFile = Compare(bytes_srcLogoObj, bytes_DirectFile);
      bool same_All = same_ReadLogoObj && same_DirectFile;

      Console.WriteLine("same_ReadmLogoObj  = {0}", same_ReadLogoObj);
      Console.WriteLine("same_DirectFile    = {0}", same_DirectFile);
      Console.WriteLine();
      Console.WriteLine((same_All) ? "same　○  ○  ○" : "☆  ☆  ☆  diff");
      Console.WriteLine();
      Console.WriteLine();
    }



    /// <summary>
    /// バイナリを比較
    /// </summary>
   private static bool Compare(byte[] A, byte[] B)
    {
      if (A == null || B == null) return false;
      if (A.Length != B.Length) return false;

      for (int i = 0; i < A.Length; i++)
      {
        if (A[i] != B[i]) return false;
      }
      return true;
    }


    /// <summary>
    /// LogoFileの情報取得
    /// </summary>
   private static string GetLogoInfo(LogoFile logofile)
    {
      var fileHeader = logofile.Header;
      var dataHeader = logofile.LogoData[0].Header;

      var info = new StringBuilder();
      info.AppendLine(fileHeader.Str);
      info.AppendLine("" + fileHeader.LogoNum);
      info.AppendLine(dataHeader.Name);
      info.AppendLine("" + dataHeader.X);
      info.AppendLine("" + dataHeader.Y);
      info.AppendLine("" + dataHeader.Height);
      info.AppendLine("" + dataHeader.Width);
      info.AppendLine("" + dataHeader.FadeIn);
      info.AppendLine("" + dataHeader.FadeOut);
      info.AppendLine("" + dataHeader.Start);
      info.AppendLine("" + dataHeader.End);
      info.AppendLine("------------------------");
      return info.ToString();
    }





    #region テスト用ファイル作成
    //
    //CreaetSample_v1
    //
    public static LogoFile CreaetSample_v1()
    {
      //LogoFile
      var logofile = new LogoFile();
      logofile.Header.Str = FileHeader.Str_v1;
      logofile.Header.LogoNum = 1;

      //DataHeader_v1
      var dataHeader_v1 = new DataHeader_v1()
      {
        Name = "hello",
        X = 0x1234,
        Y = 0x7654,
        Height = 0x0002,
        Width = 0x0002,
        FadeIn = 0x0DEF,
        FadeOut = 0x0123,
        Start = 0x4567,
        End = 0x3698,
      };

      DataHeader dataHeader = dataHeader_v1.ToLatestVer();


      //PIXEL
      var listPixels = new List<LOGO_PIXEL>();
      listPixels.AddRange(new LOGO_PIXEL[]
      { 
        Preset_LOGO_PIXEL.Red,  Preset_LOGO_PIXEL.Blue, 
        Preset_LOGO_PIXEL.Green, Preset_LOGO_PIXEL.Gray,
      });

      var data = new LogoData()
      {
        Header = dataHeader,
        Pixels = listPixels,
      };

      logofile.LogoData.Add(data);

      return logofile;
    }



    //
    //CreaetSample_v2
    //
    public static LogoFile CreaetSample_v2()
    {
      //LogoFile
      var logofile = new LogoFile();
      logofile.Header.Str = FileHeader.Str_v2;
      logofile.Header.LogoNum = 1;

      //DataHeader_v2
      var dataHeader_v2 = new DataHeader_v2()
      {
        Name = "calender.clock",
        X = 0x1234,
        Y = 0x7654,
        Height = 0x0002,
        Width = 0x0002,
        FadeIn = 0x0DEF,
        FadeOut = 0x0123,
        Start = 0x4567,
        End = 0x3698,
      };

      DataHeader dataHeader = dataHeader_v2.ToLatestVer();


      //PIXEL
      var listPixels = new List<LOGO_PIXEL>();
      listPixels.AddRange(new LOGO_PIXEL[]
      { 
        Preset_LOGO_PIXEL.Red,  Preset_LOGO_PIXEL.Blue, 
        Preset_LOGO_PIXEL.Green, Preset_LOGO_PIXEL.Gray,
      });

      var data = new LogoData()
      {
        Header = dataHeader,
        Pixels = listPixels,
      };

      logofile.LogoData.Add(data);

      return logofile;
    }
    #endregion





    #region ToBytes
    /// <summary>
    /// 構造体をバイト配列に変換   LogoFile部
    /// </summary>
    private static List<byte> ToBytes_LogoFile(LogoFile logofile)
    {
      var buffer = new List<byte>();

      try
      {
        buffer.AddRange(StructRW.ToBytes(logofile.Header));

        foreach (var logodata in logofile.LogoData)
        {
          buffer.AddRange(
            ToBytes_LogoData(logodata, logofile.Version())
            );
        }
      }
      catch (Exception e)
      {
        throw e;
      }

      return buffer;
    }

    /// <summary>
    /// 構造体をバイト配列に変換   LogoData部
    /// </summary>
    private static List<byte> ToBytes_LogoData(LogoData logoData, int fileVer)
    {
      var buffer = new List<byte>();
      //v1
      if (fileVer == 1)
      {
        var header_v1 = new DataHeader_v1(logoData.Header);
        buffer.AddRange(StructRW.ToBytes(header_v1));

        foreach (var pixel in logoData.Pixels)
          buffer.AddRange(StructRW.ToBytes(pixel));
      }
      //v2
      else if (fileVer == 2)
      {
        var header_v2 = new DataHeader_v2(logoData.Header);
        buffer.AddRange(StructRW.ToBytes(header_v2));

        foreach (var pixel in logoData.Pixels)
          buffer.AddRange(StructRW.ToBytes(pixel));
      }
      else
        throw new ArgumentException();

      return buffer;
    }
    #endregion



  }

}
