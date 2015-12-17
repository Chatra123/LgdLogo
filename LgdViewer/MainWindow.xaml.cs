using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LgdViewer
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {

    ViewModel viewmodel { get { return (this.DataContext as ViewModel); } }

    public MainWindow()
    {
      InitializeComponent();
      this.DataContext = new ViewModel();
    }

    /// <summary>
    ///  Drag Over
    /// </summary>
    private void Window_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
    {

      if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
      {
        e.Effects = System.Windows.DragDropEffects.Copy;
      }
      else
      {
        e.Effects = System.Windows.DragDropEffects.None;
      }
      e.Handled = true;
    }

    /// <summary>
    ///  Drop File
    /// </summary>
    private void Window_Drop(object sender, System.Windows.DragEventArgs e)
    {
      var dropFiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
      if (dropFiles == null) return;

      //Viewer
      if (tabControl.SelectedIndex == 0)
      {
        foreach (var one in dropFiles)
          viewmodel.OpenFile(one);
      }
      //Splitter
      else if (tabControl.SelectedIndex == 1)
      {
        viewmodel.SplitSave(dropFiles[0]);
      }
    }


    /// <summary>
    /// Deleteキーで削除
    /// </summary>
    private void listLogoData_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Delete)
        viewmodel.RemoveCommand.Execute(null);
    }


  }

}
