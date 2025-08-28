namespace CozyPoC.SimplePoCBase.Infrastructure
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// UI スレッド（Dispatcher）上で安全に処理を実行するためのインターフェース。
    /// </summary>
    /// <remarks>
    /// - UI スレッド以外から呼んでも自動的に Dispatcher にディスパッチされる。<br/>
    /// - 非同期処理（Task 戻り値あり/なし）にも対応。<br/>
    /// - ConfigureAwait(false) を内部で付与しており、呼び出し元は同期コンテキスト復帰を制御できる。
    /// </remarks>
    public interface IUIThreadSimpleDispatcher
    {
        /// <summary>
        /// 現在のスレッドが UI スレッドにアクセス可能かどうか。
        /// </summary>
        bool CheckAccess { get; }

        /// <summary>
        /// UI スレッド上で処理を実行する（戻り値なし）。
        /// </summary>
        Task InvokeAsync(Action action);

        /// <summary>
        /// UI スレッド上で処理を実行する（戻り値あり）。
        /// </summary>
        Task<T> InvokeAsync<T>(Func<T> func);

        /// <summary>
        /// UI スレッド上で非同期処理を実行する（戻り値なし）。
        /// </summary>
        Task InvokeAsync(Func<Task> actionAsync);

        /// <summary>
        /// UI スレッド上で非同期処理を実行する（戻り値あり）。
        /// </summary>
        Task<T> InvokeAsync<T>(Func<Task<T>> funcAsync);
    }
}