using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using LgdLogo;


namespace LgdViewer
{
  #region ViewModel
  public class ViewModel : ViewModelBase
  {
    Model model;

    //リストボックスで選択中のアイテム
    public LogoDataVM SelectedItem { get; set; }

    //リストボックスに表示するリスト
    ObservableCollection<LogoDataVM> _logoList;
    public ObservableCollection<LogoDataVM> LogoList
    {
      get
      {
        return _logoList;
      }
      set
      {
        if (value != null)
        {
          _logoList = value;
          OnPropertyChanged("LogoList");
        }
      }
    }

    /// <summary>
    /// constructor
    /// </summary>
    public ViewModel()
    {
      this.model = new Model();

      //起動時に読み込むファイル名　自動読込み　デバッグ用
      const string first_readfile = "logodata.ldp2";
      OpenFile(first_readfile);
    }

    /// <summary>
    /// Logoファイルを開いて追加
    /// </summary>
    public void OpenFile(string path)
    {
      var srclist = this.model.OpenFile(path);
      var obslist = ListToObservable(srclist);

      if (obslist != null)
      {
        LogoList = LogoList ?? new ObservableCollection<LogoDataVM>();

        foreach (var one in obslist)
          LogoList.Add(one);
      }
    }

    //
    //   LogoData    ⇔    LogoDataVM
    //     ( List<> )        ( ObservableCollection<> )
    //
    //  Listは直接バインドできなのでObservableCollectionに相互変換
    //
    private ObservableCollection<LogoDataVM> ListToObservable(List<LogoData> list)
    {
      if (list == null) return null;
      var list_LogoDataVM = list.Select((one) => new LogoDataVM(one));
      return new ObservableCollection<LogoDataVM>(list_LogoDataVM);
    }

    private List<LogoData> ObservableToList(ObservableCollection<LogoDataVM> obslist)
    {
      if (obslist == null) return null;
      var list_LogoData = obslist.Select((one) => one.LogoData);
      return new List<LogoData>(list_LogoData);
    }


    //
    //  ICommand
    //
    //move
    private ICommand moveCommand;
    public ICommand MoveCommand
    {
      get
      {
        moveCommand = moveCommand ?? new DelegateCommand(
          (parameter) =>
          {
            if (parameter == null) return;
            if (SelectedItem == null) return;
            if (LogoList == null) return;

            int moveStep;
            var canparse = int.TryParse(parameter.ToString(), out moveStep);
            if (canparse == false) return;

            //move
            int oldIndex = LogoList.IndexOf(SelectedItem);
            int newIndex = oldIndex + moveStep;
            newIndex = 0 < newIndex ? newIndex : 0;
            newIndex = newIndex < LogoList.Count ? newIndex : LogoList.Count - 1;
            LogoList.Move(oldIndex, newIndex);

          });
        return moveCommand;
      }
    }

    //remove
    private ICommand removeCommand;
    public ICommand RemoveCommand
    {
      get
      {
        removeCommand = removeCommand ?? new DelegateCommand(
          (parameter) =>
          {
            if (SelectedItem == null) return;
            if (LogoList == null) return;

            //remove
            LogoList.Remove(SelectedItem);

          });
        return removeCommand;
      }
    }

    //clear
    private ICommand clearCommand;
    public ICommand ClearCommand
    {
      get
      {
        clearCommand = clearCommand ?? new DelegateCommand(
          (parameter) =>
          {
            if (LogoList == null) return;

            //clear
            LogoList.Clear();

          });
        return clearCommand;
      }
    }


    //save
    private ICommand saveCommand;
    public ICommand SaveCommand
    {
      get
      {
        saveCommand = saveCommand ?? new DelegateCommand(
          (parameter) =>
          {
            if (LogoList == null) return;
            if (LogoList.Count == 0) return;

            var list = ObservableToList(LogoList);
            //save
            Task.Run(() =>
            {
              model.SaveFile(list);    // データ10コで100msかかるのでTask経由
            });

          });
        return saveCommand;
      }
    }


    /// <summary>
    /// Split file
    /// </summary>
    public void SplitSave(string path)
    {
      Task.Run(() =>
      {
        model.SplitSave(path);
      });
    }

    /// <summary>
    /// Splitterページのテキスト
    /// </summary>
    public string SplitterPageText
    {
      get
      {
        const string text =
@"
Drop the file
  
  split and save to desktop as *.lgd files.
  not as *.lgd2.

    [ .lgd  .ldp  .lgd2 .ldp2 ]    →    [ .lgd ] 
";
        return text;
      }
    }




  }
  #endregion






  #region LogoDataVM
  /// <summary>
  /// LogoDataVM  画面表示する情報のみをプロパティとして公開する
  /// </summary>
  public class LogoDataVM : ViewModelBase
  {
    public LogoData LogoData { get; private set; }

    public LogoDataVM(LogoData logodata)
    {
      LogoData = logodata;
    }

    public override string ToString()
    {
      return Name;
    }

    public string Name
    {
      get
      {
        var header = LogoData.Header;
        return header.Name;
      }
      set
      {
        var header = LogoData.Header;
        header.Name = value;
        OnPropertyChanged("Name");
        OnPropertyChanged("Info");
      }
    }


    public BitmapImage Image
    {
      get
      {
        return LogoData.Image;
      }
    }


    public string Info
    {
      get
      {
        var header = LogoData.Header;
        var info = new StringBuilder();
        info.AppendLine("Name   :  " + header.Name);
        info.AppendLine("Width  :  " + header.Width);
        info.AppendLine("Height :  " + header.Height);
        info.AppendLine("X      :  " + header.X);
        info.AppendLine("Y      :  " + header.Y);
        return info.ToString();
      }
    }
  }
  #endregion





  #region Model
  /// <summary>
  /// Model
  /// </summary>
  class Model
  {
    readonly object sync = new object();

    private LogoFileManager manager;


    public Model()
    {
      manager = new LogoFileManager();
    }

    /// <summary>
    /// Open
    /// </summary>
    public List<LogoData> OpenFile(string path)
    {
      lock (sync)
      {
        //check path
        if (File.Exists(path) == false)
          return new List<LogoData>();

        //check extension
        var ext = Path.GetExtension(path).ToLower();

        if (ext != ".lgd" && ext != ".ldp"
          && ext != ".lgd2" && ext != ".ldp2")
          return new List<LogoData>();

        try
        {
          var logofile = manager.Load(path);
          return logofile.LogoData;
        }
        catch
        {
          return new List<LogoData>();
        }
      }
    }

    /// <summary>
    /// Save
    /// </summary>
    public bool SaveFile(IList<LogoData> savelist)
    {
      lock (sync)
      {
        //デスクトップに作成
        var savepath = new Func<string>(() =>
        {
          string desktop = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
          string timecode = DateTime.Now.ToString("HHmmss");
          string name = "LgdViewer.ldp2";
          string path = Path.Combine(desktop, timecode + " - " + name);
          return path;
        })();

        //同名のファイルがある？
        if (File.Exists(savepath)) 
          return false;

        if (savelist != null)
        {
          manager.Save(savepath, savelist);    // データ10コで100msかかる
          return true;
        }
        else
          return false;
      }
    }



    /// <summary>
    /// パッケージファイルを複数のlgdに分割
    /// </summary>
    public void SplitSave(string srcpath)
    {
      lock (sync)
      {
        //読
        var logolist = OpenFile(srcpath);
        if (logolist.Count == 0) return;


        //データが１つならデスクトップに保存
        //　　　　２つ以上ならフォルダを作成してから保存
        var desktop = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        string saveDir = "";
        {
          if (logolist.Count == 1)
          {
            saveDir = desktop;
          }
          else
          {
            var dirName = Path.GetFileName(srcpath);
            dirName = ReplaceInvalidChar(dirName);
            saveDir = Path.Combine(desktop, dirName);

            //同名のファイル or フォルダがある？
            if (File.Exists(saveDir))
            {
              var timecode = DateTime.Now.ToString("HHmmss.fff");
              saveDir = saveDir + " - " + timecode;
            }

            //フォルダ作成
            Directory.CreateDirectory(saveDir);
          }
        }


        //書
        //  lgdで保存、lgd2ではない
        foreach (var logodata in logolist)
        {
          var name = logodata.Header.Name;
          name = ReplaceInvalidChar(name);
          var savepath = Path.Combine(saveDir, name + ".lgd");

          manager.Save(savepath, new List<LogoData> { logodata });
        }
      }
    }


    /// <summary>
    /// ファイル名に使えない文字を _ に置換
    /// </summary>
    static string ReplaceInvalidChar(string filename)
    {  
      foreach (char c in Path.GetInvalidFileNameChars())
        filename = filename.Replace(c, '_');

      return filename;
    }

  }
  #endregion



}
