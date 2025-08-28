namespace CozyPoC.SimplePoCBase.App
{
    using CozyPoC.SevensMCP.Views;
    using CozyPoC.SimplePoCBase.ViewModels;
    using System;
    using System.Windows;

    internal static class Program
    {
        [STAThread]
        public static void Main(string[] _)
        {
            var app = new SimplePocApplication()
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };

            // ViewModel の生成
            var vm = new DevToolViewModel();

            // MainWindow の生成
            var mainWindow = new TestWindow()
            {
                DataContext = vm
            };

            // アプリ開始
            app.Run(mainWindow);
        }
    }
}
