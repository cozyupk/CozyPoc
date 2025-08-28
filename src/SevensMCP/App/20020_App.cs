using CozyPoC.SevensMCP.Domain.Impl;
using CozyPoC.SevensMCP.Views;
using CozyPoC.SimplePoCBase.App;
using System;
using System.Windows;

namespace CozyPoC.SevensMCP.App
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] _)
        {
            var app = new SimplePocApplication()
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };

            // モデルファクトリの生成（そのうちDIコンテナに置き換える）
            var modelFactory = new ModelsFactory();

            // ViewModel の生成
            var vm = new ViewModels.MainViewModel(modelFactory);

            // MainWindow の生成
            var mainWindow = new MainWindow()
            {
                DataContext = vm
            };

            // アプリ開始
            app.Run(mainWindow);
        }

    }
}
