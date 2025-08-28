namespace CozyPoC.SimplePoCBase.ViewModels
{
    using CommunityToolkit.Mvvm.Input;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a view model for testing and demonstrating exception handling scenarios in various threading
    /// contexts, including UI thread, background tasks, and background threads.
    /// </summary>
    /// <remarks>This class provides commands to simulate and observe different types of exceptions that can
    /// occur in a .NET application. It is intended for debugging, testing, and understanding exception propagation and
    /// handling in asynchronous and multithreaded environments.  The commands in this view model are designed to throw
    /// exceptions in specific contexts: <list type="bullet"> <item><description>UI thread
    /// exceptions.</description></item> <item><description>Unobserved task exceptions in asynchronous
    /// operations.</description></item> <item><description>Exceptions in background threads.</description></item>
    /// </list> Use these commands to test exception handling mechanisms such as <see
    /// cref="System.Windows.Threading.DispatcherUnhandledException"/>, <see
    /// cref="System.Threading.Tasks.TaskScheduler.UnobservedTaskException"/>, and <see
    /// cref="System.AppDomain.UnhandledException"/>.</remarks>
    public partial class DevToolViewModel
    {

#pragma warning disable IDE0079 // 不要な抑制を削除します
#pragma warning disable CA1822 // メソッドを static にできます
#pragma warning restore IDE0079 // 不要な抑制を削除します

        /// <summary>
        /// Throws a test exception on the UI thread to simulate an unhandled exception scenario.
        /// </summary>
        /// <remarks>This method is intended for testing purposes and will throw an <see
        /// cref="InvalidOperationException"/>  to simulate an unhandled exception on the UI thread. It should only be
        /// used in controlled environments  where such behavior is expected and can be safely handled.</remarks>
        /// <exception cref="InvalidOperationException">Always thrown when this method is invoked.</exception>
        // ① UIスレッド内で未処理例外 → DispatcherUnhandledException を確実に飛ばす：同期voidでthrow
        [RelayCommand]
        private void ThrowUIException()
        {
            throw new InvalidOperationException("UIスレッドでのテスト例外");
        }

        /// <summary>
        /// Executes an asynchronous operation that triggers an unobserved exception in a background task.
        /// </summary>
        /// <remarks>This method demonstrates the behavior of unobserved exceptions in tasks. It creates a
        /// background task  that throws an exception, waits for the task to complete without observing the exception,
        /// and then  forces garbage collection to finalize the task. This is intended for testing or demonstration
        /// purposes  and should not be used in production code.</remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ApplicationException"></exception>
        // ② 未観測Task例外 → 非同期
        [RelayCommand]
        private async Task ThrowTaskExceptionAsync()
        {
            SemaphoreSlim sem = new(0, 1);

            _ = Task.Run(() =>
            {
                try
                {
                    throw new ApplicationException("バックグラウンドTaskでのテスト例外");
                }
                finally
                {
                    sem.Release();
                }
            });

            await sem.WaitAsync();        // 例外を投げ終わるまで待つ（観測はしない）
            await Task.Yield();           // 1ティック譲ってからGC（安定度↑）

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Throws a test exception from a background thread.
        /// </summary>
        /// <remarks>This method starts a background thread that throws an exception after a short delay. 
        /// It is intended for testing scenarios where exceptions in background threads need to be handled or
        /// observed.</remarks>
        /// <exception cref="Exception"></exception>
        // ③ バックグラウンドThread例外 → 同期でもOK
        [RelayCommand]
        private void ThrowBackgroundTaskException()
        {
            var t = new Thread(() =>
            {
                Thread.Sleep(50);
                throw new Exception("バックグラウンドThreadでのテスト例外");
            })
            { IsBackground = true };
            t.Start();
        }

#pragma warning disable IDE0079 // 不要な抑制を削除します
#pragma warning restore CA1822 // メソッドを static にできます
#pragma warning restore IDE0079 // 不要な抑制を削除します

    }
}
