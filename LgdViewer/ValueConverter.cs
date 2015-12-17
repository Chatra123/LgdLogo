using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LgdViewer
{

  /// <summary>
  /// 背景が縞模様のリストボックス
  /// </summary>
  public class ListBoxStyleSelector : StyleSelector
  {
    /// <summary>偶数スタイルを取得・設定します。</summary>
    public Style EvenStyle { get; set; }

    /// <summary>奇数スタイルを取得・設定します。</summary>
    public Style OddStyle { get; set; }

    public override Style SelectStyle(object item, DependencyObject container)
    {
      var control = ItemsControl.ItemsControlFromItemContainer(container);
      if ((control.Items.IndexOf(item) % 2) == 0)
      {
        // 計算上は偶数だが、デザイン上は奇数行になるので奇数スタイルを返却
        return OddStyle;
      }
      else
      {
        // 計算上は奇数だが、デザイン上は偶数行になるので偶数スタイルを返却
        return EvenStyle;
      }
    }
  }




  /// <summary>
  /// PropertyChangedのみを実装
  /// </summary>
  public class ViewModelBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

    public void OnPropertyChanged(string propertyname)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
      }
    }
  }




  /// <summary>
  /// 実装を委譲できるコマンドを表現します。
  /// </summary>
  public class DelegateCommand : ICommand
  {
    /// <summary>
    /// 現在の状態でこのコマンドを実行できるかどうかを判断するメソッドを定義します。
    /// </summary>
    private Func<object, bool> canExecute;


    /// <summary>
    /// 現在の状態でこのコマンドを実行できるかどうかを判断するメソッドを定義します。
    /// </summary>
    private Action<object> execute;


    /// <summary>
    /// コマンドの起動時に実行するメソッドを指定してインスタンスを生成、初期化します。
    /// </summary>
    /// <param name="execute">コマンドの起動時に実行するメソッド</param>
    public DelegateCommand(Action<object> execute)
      : this(execute, o => true)
    {
    }


    /// <summary>
    /// コマンドの起動時に実行するメソッドとコマンドを実行するかどうかを返すメソッドを指定してインスタンスを生成、初期化します。
    /// </summary>
    /// <param name="execute">コマンドの起動時に実行するメソッド</param>
    /// <param name="canExecute">コマンドを実行するかどうかを返すメソッド</param>
    public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
    {
      this.execute = execute;
      this.canExecute = canExecute;
    }


    /// <summary>
    /// コマンドを実行するかどうかに影響するような変更があった場合に発生します。
    /// </summary>
#pragma warning disable 0067                //警告：　イベント''は使用されませんでした
    public event EventHandler CanExecuteChanged;
#pragma warning restore 0067


    /// <summary>
    /// 現在の状態でこのコマンドを実行できるかどうかを返します。
    /// </summary>
    /// <param name="parameter">コマンドで使用されたデータ。 コマンドにデータを渡す必要がない場合は、このオブジェクトを null に設定されます。 </param>
    /// <returns>このコマンドを実行できる場合は true。それ以外の場合は false。</returns>
    public bool CanExecute(object parameter)
    {
      return this.canExecute(parameter);
    }


    /// <summary>
    /// コマンドの起動時に呼び出されます。
    /// </summary>
    /// <param name="parameter">コマンドで使用されたデータ。 コマンドにデータを渡す必要がない場合は、このオブジェクトを null に設定されます。</param>
    public void Execute(object parameter)
    {
      this.execute(parameter);
    }


  }
}
