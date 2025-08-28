namespace CozyPoC.SimplePoCBase.App
{
    using CozyPoC.SevensMCP.Views;
    using CozyPoC.SimplePoCBase.ViewModels;
    using System;
    using System.Windows;

    /// <summary>
    /// Serves as the entry point for the application.
    /// </summary>
    /// <remarks>This method initializes the application, sets the shutdown mode, creates the main window and
    /// its associated view model, and starts the application's message loop. The application shuts down when the main
    /// window is closed.</remarks>
    internal static class Program
    {
        /// <summary>
        /// The entry point of the application. Initializes the application, sets up the main window,  and starts the
        /// application's message loop.
        /// </summary>
        /// <remarks>This method configures the application to shut down when the main window is closed. 
        /// It creates and assigns a <see cref="DevToolViewModel"/> as the data context for the main window  and starts
        /// the application with the specified main window.</remarks>
        /// <param name="_"></param>
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
