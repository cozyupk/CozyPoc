using CozyPoC.SevensMCP.ViewModels;
using System.Windows;

namespace CozyPoC.SevensMCP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 必要ならここで初期化（ロガー/設定読み込み/ファイル列挙など）

            var vm = new MainViewModel();       // 引数が必要ならここで渡す
            var window = new MainWindow         // XAML側のビュー
            {
                DataContext = vm
            };

            // アプリの終了条件（デフォルトは MainWindow が Close されたら終了）
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow = window;
            window.Show();
        }
    }
}
