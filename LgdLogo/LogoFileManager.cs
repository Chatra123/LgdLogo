using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace LgdLogo
{
  using LgdLogo.ExtensionMethod;


  public class LogoFileManager
  {
    /// <summary>
    /// lgdファイル保存
    /// </summary>
    public bool Save(string filename, IList<LogoData> logodata)
    {
      return Save(filename, new LogoFile() { LogoData = logodata.ToList() });
    }

    /// <summary>
    /// lgdファイル保存
    /// </summary>
    public bool Save(string filename, LogoFile logofile)
    {

      //拡張子からversionを決定。デフォルト値　ver = 2
      {
        int ver;
        {
          var ext = Path.GetExtension(filename).ToLower();

          if (ext == ".lgd2" || ext == ".ldp2") ver = 2;
          else if (ext == ".lgd" || ext == ".ldp") ver = 1;
          else ver = 2;
        }

        //Header.str書換
        if (ver == 1)
          logofile.Header.Str = FileHeader.Str_v1;
        else if (ver == 2)
          logofile.Header.Str = FileHeader.Str_v2;
      }

      //LogoNum
      logofile.Header.LogoNum = logofile.LogoData.Count;


      //validate logodata size
      foreach (var logodata in logofile.LogoData)
      {
        int w = logodata.LogoImageSize().Width;
        int h = logodata.LogoImageSize().Height;
        int pix_count = logodata.Pixels.Count;

        //”ヘッダー内のイメージサイズ”と”ピクセル数”が違う？
        if (w * h != pix_count)
        {
          throw new Exception("invalid logo size");
        }
      }


      //書
      try
      {
        using (var mstream = new MemoryStream())
        {
          var writer = new BinaryWriter(mstream);

          //構造体をメモリーストリームに書込み
          LogoFileRW.Write_LogoFile(logofile, writer);
          //ファイル書込み
          using (var fstream = new FileStream(filename, FileMode.Create))
          {
            fstream.Write(mstream.ToArray(), 0, (int)mstream.Length);
          }
        }

        return true;
      }
      catch
      {
        return false;
      }

    }


    /// <summary>
    /// lgdファイル読込
    /// </summary>
    public LogoFile Load(string filename)
    {
      LogoFile logofile;

      //読
      try
      {
        using (var fstream = new FileStream(filename, FileMode.Open))
        using (var mstream = new MemoryStream())
        {
          //ファイルを読込みメモリーストリームにコピー
          fstream.CopyTo(mstream);
          mstream.Seek(0, SeekOrigin.Begin);

          //メモリーストリームからロゴ構造体に変換
          var reader = new BinaryReader(mstream);
          logofile = LogoFileRW.Read_LogoFile(reader);
          return logofile;
        }

      }
      catch
      {
        return null;
      }

    }




  }





  public static class LogoFileUtil
  {

    /// <summary>
    /// LogoDataの名前一覧取得
    /// </summary>
    public static List<string> GetNameList(IList<LogoData> logodata)
    {
      return GetNameList(new LogoFile() { LogoData = logodata.ToList() });
    }
    public static List<string> GetNameList(LogoFile logofile)
    {
      return logofile.LogoData.Select(one => one.Header.Name).ToList();
    }


    /// <summary>
    /// 名前からLogoData取得　　部分一致
    /// </summary>
    public static List<LogoData> GetLogoData(string namekey, IList<LogoData> logodata)
    {
      return GetLogoData(namekey, new LogoFile() { LogoData = logodata.ToList() });
    }
    public static List<LogoData> GetLogoData(string namekey, LogoFile logofile)
    {
      namekey = namekey.ToLower();
      return logofile.LogoData.Where(
                                      one => one.Header.Name.ToLower().Contains(namekey)
                                     ).ToList();
    }


  }













}
