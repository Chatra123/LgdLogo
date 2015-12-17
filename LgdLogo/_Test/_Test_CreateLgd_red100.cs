using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LgdLogo;



namespace LgdLogo.Test
{
  using LgdLogo.ExtensionMethod;


  public class _Test_CreateLgd
  {

    /// <summary>
    /// lgdファイル作成、出力
    /// </summary>
    public static void Test()
    {
      var manager = new LogoFileManager();

      var logofile = CreaetSample_red100();
      manager.Save("red100x100.lgd2", logofile);

      var readfile = manager.Load("red100x100.lgd2");

      string info = GetLogoInfo(logofile);
      Console.WriteLine(info);
      Console.WriteLine();
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


    /// <summary>
    /// サンプルLogoFile作成　100 x 100
    /// </summary>
    private static LogoFile CreaetSample_red100()
    {
      //LogoFile
      var logofile = new LogoFile();

      //Header.Str     はファイル拡張子から
      //Header.LogoNum はLogoFile.LogoData.Count()から
      //自動的に決定するので、設定する必要はない。
      logofile.Header.Str = FileHeader.Str_v2;
      logofile.Header.LogoNum = 1;

      //DataHeader_v2
      var dataHeader_v2 = new DataHeader_v2()
      {
        Name = "calender.clock",
        X = 0x1234,
        Y = 0x7654,
        Height = 100,
        Width = 100,
        FadeIn = 0x0DEF,
        FadeOut = 0x0123,
        Start = 0x4567,
        End = 0x3698,
      };

      DataHeader dataHeader = dataHeader_v2.ToLatestVer();

      //PIXEL
      var listPixels = new List<LOGO_PIXEL>();

      foreach (var i in Enumerable.Range(1, 100 * 100))
      {
        listPixels.Add(Preset_LOGO_PIXEL.Red);
      }

      var data = new LogoData()
      {
        Header = dataHeader,
        Pixels = listPixels,
      };

      logofile.LogoData.Add(data);

      return logofile;
    }








  }

}
