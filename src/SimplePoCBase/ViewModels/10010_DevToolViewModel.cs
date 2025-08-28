namespace CozyPoC.SimplePoCBase.ViewModels
{
    using CommunityToolkit.Mvvm.Input;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class DevToolViewModel
    {

#pragma warning disable IDE0079 // 不要な抑制を削除します
#pragma warning disable CA1822 // メソッドを static にできます
#pragma warning restore IDE0079 // 不要な抑制を削除します

        // ① UIスレッド内で未処理例外 → DispatcherUnhandledException を確実に飛ばす：同期voidでthrow
        [RelayCommand]
        private void ThrowUIException()
        {
            throw new InvalidOperationException("UIスレッドでのテスト例外");
        }

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
