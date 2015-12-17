using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LgdViewer
{
  /// <summary>
  /// App.xaml の相互作用ロジック
  /// </summary>
  public partial class App : Application
  {
    public App()
      : base()
    {
      this.Startup += new StartupEventHandler(App_Startup);
    }

    public void App_Startup(object sender, System.Windows.StartupEventArgs e)
    {
      this.MainWindow = new MainWindow();
      this.MainWindow.Show();
    }
  }
}
