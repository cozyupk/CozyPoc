namespace CozyPoC.SimplePoCBase.App
{
    using CozyPoC.SimplePoCBase.Infrastructure;
    using System;
    using System.Threading.Tasks;
    using System.Windows.Threading;

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
        /// <summary>
        /// Represents the dispatcher used to manage the execution of work on a specific thread or context.
        /// </summary>
        /// <remarks>This field is initialized with the provided <see cref="Dispatcher"/> instance and
        /// cannot be null. It is intended to ensure that operations are dispatched to the appropriate thread or
        /// synchronization context.</remarks>
        private readonly Dispatcher _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        /// <summary>
        /// Gets a value indicating whether the calling thread has access to this dispatcher.
        /// </summary>
        /// <remarks>This property is typically used to determine if a thread can interact with objects
        /// associated with the dispatcher. If the property returns <see langword="false"/>, use the dispatcher to
        /// invoke the required operation on the correct thread.</remarks>
        public bool CheckAccess => _dispatcher.CheckAccess();

        /// <summary>
        /// Executes the specified action on the dispatcher thread asynchronously.
        /// </summary>
        /// <remarks>If the current thread has access to the dispatcher, the action is executed
        /// immediately.  Otherwise, the action is dispatched to the dispatcher thread and executed
        /// asynchronously.</remarks>
        /// <param name="action">The action to execute. This parameter cannot be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Executes the specified function either synchronously or asynchronously, depending on the current access
        /// context.
        /// </summary>
        /// <remarks>If the current context allows direct access, the function is executed synchronously. 
        /// Otherwise, the function is dispatched for asynchronous execution.</remarks>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <param name="func">The function to execute. This function must not be null.</param>
        /// <returns>The result of the executed function.</returns>
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

        /// <summary>
        /// Executes the specified asynchronous action on the dispatcher thread if required.
        /// </summary>
        /// <remarks>If the current thread has access to the dispatcher, the action is executed directly. 
        /// Otherwise, the action is dispatched to the dispatcher thread for execution.</remarks>
        /// <param name="actionAsync">The asynchronous action to execute. This parameter cannot be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Executes an asynchronous function, ensuring it is invoked on the appropriate thread or context.
        /// </summary>
        /// <remarks>If the current thread has access to the required context, the function is executed
        /// directly.  Otherwise, the function is dispatched to the appropriate context for execution.</remarks>
        /// <typeparam name="T">The type of the result returned by the asynchronous function.</typeparam>
        /// <param name="funcAsync">The asynchronous function to execute. Cannot be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task's result contains the value returned by
        /// <paramref name="funcAsync"/>.</returns>
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
