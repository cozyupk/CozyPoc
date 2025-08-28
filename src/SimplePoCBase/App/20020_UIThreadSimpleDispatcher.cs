using CozyPoC.SimplePoCBase.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CozyPoC.SimplePoCBase.App
{
    /// <summary>
    /// 実際の WPF Dispatcher をラップした実装。
    /// </summary>
    /// <remarks>
    /// - Dispatcher を直接公開せず、必ずこのクラス経由で呼ぶこと。<br/>
    /// - Dispatcher.InvokeAsync は Task<Task> を返すケースがあるため、Unwrap() して内側まで待機している点に注意。
    /// </remarks>
    /// <remarks>
    /// 指定された Dispatcher をラップするインスタンスを生成する。
    /// </remarks>
    public sealed class UIThreadSimpleDispatcher(Dispatcher dispatcher) : IUIThreadSimpleDispatcher
    {
        private readonly Dispatcher _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        public bool CheckAccess => _dispatcher.CheckAccess();

        public async Task InvokeAsync(Action action)
        {
            if (CheckAccess)
            {
                action();
            }
            else
            {
                await _dispatcher.InvokeAsync(action).Task.ConfigureAwait(false);
            }
        }

        public async Task<T> InvokeAsync<T>(Func<T> func)
        {
            if (CheckAccess)
            {
                return func();
            }
            else
            {
                return await _dispatcher.InvokeAsync(func).Task.ConfigureAwait(false);
            }
        }

        public async Task InvokeAsync(Func<Task> actionAsync)
        {
            if (CheckAccess)
            {
                await actionAsync().ConfigureAwait(false);
            }
            else
            {
                // DispatcherOperation<Task> → Task<Task> になるため Unwrap() が必須
                await _dispatcher.InvokeAsync(actionAsync).Task.Unwrap().ConfigureAwait(false);
            }
        }

        public async Task<T> InvokeAsync<T>(Func<Task<T>> funcAsync)
        {
            if (CheckAccess)
            {
                return await funcAsync().ConfigureAwait(false);
            }
            else
            {
                return await _dispatcher.InvokeAsync(funcAsync).Task.Unwrap().ConfigureAwait(false);
            }
        }
    }

}
