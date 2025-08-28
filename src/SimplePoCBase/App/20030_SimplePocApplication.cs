namespace CozyPoC.SimplePoCBase.App
{
    using CozyPoC.SimplePoCBase.Infrastructure;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    // ==============================================
    // SimplePocApplication（PoC用の簡易実装）
    // ----------------------------------------------
    // ■ 目的
    //   - WPF/MVVM アプリの「グローバル例外ハンドリング」を最小構成で示す。
    //   - 3つの入口をカバー：
    //       (1) UIスレッド:   Application.DispatcherUnhandledException
    //       (2) CLR全体:      AppDomain.CurrentDomain.UnhandledException
    //       (3) 未観測Task:   TaskScheduler.UnobservedTaskException
    //
    // ■ 設計の基本方針（現代の OOP / MVVM 観点）
    //   - 各 ViewModel / Model は自分の責務範囲で例外をできる限り処理する（翻訳して通知、ログ、必要に応じて再throw）。
    //   - 本クラスは「最後の砦」として漏れてきた未処理例外を集約し、通知/終了を判断する。
    //   - .NET GUI 特有の前提：UI は UI スレッドでのみ操作可能。待機や重い処理は UI 外で実行。
    //     → ダイアログ表示は Dispatcher 経由（CheckAccess/InvokeAsync）で必ず UI に戻す。
    //
    // ■ セキュリティ/プライバシ（PII/機微情報）
    //   - 原則として Exception メッセージに PII を含めない。含む場合も表示・保存時はマスキング/要約する。
    //   - 本 PoC では UI 通知に MessageBox を用いるため、PII を表示しない設計・文言にすること。
    //
    // ■ PoC から実運用へ（TODO）
    //   - 機微情報・PII ポリシの明確化（可能であれば Exception に含めない／表示・保存時にマスク）
    //   - MessageBox の代わりに専用の Dialog（MVVM）を構築（IDialogService など）
    //   - InnerException / AggregateException のより詳細な整形と出力ポリシ
    //   - 多言語対応（リソース化）
    //   - ログ出力（Serilog/NLog/MEL）またはログ出力 Action の DI 注入
    //   - コンテナ/フレームワーク（Prism/Toolkit 等）との連携
    //   - SRP を考慮したリファクタ（各例外ハンドラの分離、ポリシ駆動の判定など）
    //   - UnhandledException 通知にタイムアウト（本実装はプロパティで 15s）を設けデッドロック回避
    //
    // ■ 改善提案メモ（1～7）
    //   1) タイムアウト結果(shown)を活用し、UIが出せなかった場合のログやフォールバックを追加する
    //   2) 各ハンドラを別クラスに切り出し（SRP）、IDialogService/IErrorPolicy/ILogger などをDIできるようにする
    //   3) 例外メッセージの整形・PIIマスク処理を専用ヘルパで実施（MaskPII(ex) など）
    //   4) 多重発火対策（_fatalEntered）は UnhandledException 専用だが、必要なら他の経路にも簡易ガードを追加する
    //   5) ハンドラ登録場所は OnStartup 以外にも Composition Root に寄せる等、フレームワーク連携に応じて調整する
    //   6) FailFast/Shutdown 一辺倒ではなく、安全終了ルート（保存・退避）を shutdownAsync に差し替え可能にする
    //   7) 取りこぼし対策として FirstChanceException ログも併用し、未観測Taskや非同期例外の監視精度を上げる
    // ==============================================

    /// <summary>
    /// 最小構成のグローバル例外ハンドリングを示すための WPF アプリ実装（PoC）。
    /// </summary>
    /// <remarks>
    /// 本クラスは未処理例外の代表的な入口をカバーします：
    /// <list type="bullet">
    /// <item><description><see cref="Application.DispatcherUnhandledException"/>（UIスレッド）</description></item>
    /// <item><description><see cref="AppDomain.UnhandledException"/>（CLR全体）</description></item>
    /// <item><description><see cref="TaskScheduler.UnobservedTaskException"/>（未観測 Task）</description></item>
    /// </list>
    /// 役割は「最後の砦」であり、漏れてきた未処理例外を集約して通知し、継続可否（終了）を判断します。
    /// 実運用ではロギング、PII マスキング、多言語対応などの拡張を前提としてください。
    /// </remarks>
    public class SimplePocApplication : Application
    {
        /// <summary>
        /// UnhandledException 通知（UIダイアログ）を待つ最大時間。
        /// UI が取得できない／固まっている場合のデッドロック回避に使用します。
        /// </summary>
        protected virtual TimeSpan TimeoutForUnhandledExceptionNotification { get; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// UnhandledException 発生時の終了コード。
        /// </summary>
        protected virtual int ExitCodeForUnhandledException { get; } = 1;

        /// <summary>
        /// UnobservedTaskException 発生時の終了コード。
        /// </summary>
        protected virtual int ExitCodeForUnobservedTaskException { get; } = 2;

        /// <summary>
        /// UI スレッドに関連付けられた <see cref="Dispatcher"/>。
        /// UI 操作（ダイアログ表示など）を行う際に UI スレッドへマーシャリングするために使用します。
        /// </summary>
        private readonly Dispatcher _ui;

        protected IUIThreadSimpleDispatcher UIDispatcher { get; }

        /// <summary>
        /// 致命エラー通知がすでに行われたかどうか（0: 未 / 1: 済）。
        /// UnhandledException の多重発火を防ぐガードに使用します。
        /// </summary>
        private int _fatalEntered = 0;

        /// <summary>
        /// コンストラクタ。UI Dispatcher を捕捉します。
        /// </summary>
        public SimplePocApplication()
        {
            _ui = Dispatcher;

            UIDispatcher = CreateUIDispatcher(_ui);
        }

        /// <summary>
        /// UIThreadSimpleDispatcher
        /// </summary>
        /// <remarks>
        /// オブジェクト生成戦略の変更等により、派生クラスでオーバーライドされる想定
        /// (Factory Method パターン)
        /// </remarks>
        protected virtual IUIThreadSimpleDispatcher CreateUIDispatcher(Dispatcher dispatcher)
        {
            return new UIThreadSimpleDispatcher(dispatcher);
        }

        /// <summary>
        /// アプリ起動時の処理。グローバル例外の各種ハンドラを登録します。
        /// </summary>
        /// <param name="_">未使用。</param>
        protected override void OnStartup(StartupEventArgs _)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;   // (1) UIスレッド
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException; // (3) 未観測Task
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;  // (2) CLR全体
        }

        /// <summary>
        /// UI スレッド上の未処理例外をハンドルします。
        /// </summary>
        /// <remarks>
        /// ここでは例外を <see cref="DispatcherUnhandledExceptionEventArgs.Handled"/> = true とし、
        /// 最終的な継続/終了判断はダイアログ（ConfirmOrShutdownAsync）に委ねます。
        /// </remarks>
        protected virtual void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            var msg = $"""
                【DispatcherUnhandledException】

                UI スレッドで未処理例外が発生しました。

                {ex.GetType().Name}
                {ex.Message}

                続行しますか？
                """;

            // 非同期的に確認ダイアログを表示し、結果に応じて終了処理を実行
            // 本ハンドラ自体が同期メソッドであるため await できず、
            // また他の方法で待機するとUIスレッドをブロックしてしまうため Fire するだけにとどめる
            _ = ConfirmOrShutdownAsync(msg, "UI例外", shutdownAsync: () =>
            {
                // ユーザーが「続行しない」選択をした場合、まずは WPF の安全終了を試みる
                Current?.Shutdown(ExitCodeForUnhandledException);
                return Task.CompletedTask;
            });

            e.Handled = true; // ここで即クラッシュさせず、継続可能性を残す
        }

        /// <summary>
        /// 未観測 Task 例外をハンドルします（GC 最終化タイミング等で表面化）。
        /// </summary>
        /// <remarks>
        /// <see cref="UnobservedTaskExceptionEventArgs.SetObserved"/> を呼び、プロセス落ちを抑止します。
        /// その上で UI 通知 → 継続/終了判断を行います。
        /// </remarks>
        protected virtual void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            AggregateException agg = e.Exception;
            var flat = agg.Flatten();
            var inners = string.Join(Environment.NewLine,
                flat.InnerExceptions.Select((ex, i) =>
                    $"  [{i + 1}] {ex.GetType().Name}: {ex.Message}"));

            var msg = $"""
                【UnobservedTaskException】

                バックグラウンド Task で未観測例外が発生しました。

                {agg.GetType().Name}
                {agg.Message}

                ---- Inner Exceptions ----
                {inners}

                続行しますか？
                """;

            // 非同期的に確認ダイアログを表示し、結果に応じて終了処理を実行
            // 本ハンドラ自体が同期メソッドであるため await できず、
            // また他の方法で待機すると GC やファイナライザの動作を不安定にしかねないため、
            // Fire するだけにとどめる。(本メソッドは finalizer/GC 系から呼ばれる)
            _ = ConfirmOrShutdownAsync(msg, "非同期例外", shutdownAsync: () =>
            {
                // ユーザーが「続行しない」選択をした場合、まずは WPF の安全終了を試みる
                Current?.Shutdown(ExitCodeForUnobservedTaskException);
                return Task.CompletedTask;
            });

            e.SetObserved(); // 観測済みにすることで即終了を回避
        }

        /// <summary>
        /// CLR 全体の未処理例外をハンドルします（原則として回復不可能）。
        /// </summary>
        /// <remarks>
        /// UI 告知を試みつつ（最大 <see cref="TimeoutForUnhandledExceptionNotification"/> まで待機）、
        /// 最後は <see cref="Environment.FailFast(string)"/> により即時終了します。
        /// 例外ループを避けるため、多重発火は <c>_fatalEntered</c> でガードします。
        /// </remarks>
        protected virtual void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            // 多重発火防止（すでに致命ダイアログ表示中なら即終了）
            if (Interlocked.Exchange(ref _fatalEntered, 1) == 1)
            {
                Environment.FailFast(ex?.Message ?? "UnhandledException (reentered)");
                return;
            }

            var msg = $"""
                【UnhandledException】

                CLR 全体で未処理例外が発生しました。

                {ex?.GetType().Name}
                {ex?.Message}

                この例外は続行不可能です。
                アプリケーションを終了します。
                """;

            var sem = new Semaphore(0, 1);
            ShutdownWithNotification(msg, "続行できない例外", sem);

            // UIでの告知完了／タイムアウトのどちらかまで待機（true=告知できた/false=できなかった）
            var shown = sem.WaitOne(TimeoutForUnhandledExceptionNotification);

            // 無条件で即時終了（finally/dispose も走らない点に注意）
            // 終了コードの設定もできないため、ログ等は事前に済ませておくこと
            // (この場合でも Current.Shutdown() を利用するという設計上の選択肢はある）
            Environment.FailFast((ex?.Message ?? "UnhandledException") + $"- MessageBox Shown: {shown}");

            // 待機のためのヘルパメソッド
            async void ShutdownWithNotification(string msg, string title, Semaphore semaphore)
            {
                await ConfirmOrShutdownAsync(msg, title, isFatal: true);
                semaphore.Release();
            }
        }

        /// <summary>
        /// UI スレッド上で確認ダイアログを表示し、結果に応じて任意の終了処理を実行します。
        /// </summary>
        /// <param name="msg">表示するメッセージ。</param>
        /// <param name="title">ダイアログのタイトル。</param>
        /// <param name="isFatal">致命的エラーの場合は OK/エラーアイコン、通常は Yes/No/警告アイコン。</param>
        /// <param name="shutdownAsync">終了が必要な場合に呼ぶ任意のコールバック（null なら何もしない）。</param>
        protected async Task ConfirmOrShutdownAsync(
            string msg, string title, bool isFatal = false, Func<Task>? shutdownAsync = null)
        {
            MessageBoxResult res;
            await UIDispatcher.InvokeAsync(async () => {
                res = SafeShow(msg, title, isFatal);
                await DecideAsync(res).ConfigureAwait(false);
            }).ConfigureAwait(false);

            // ---- helpers ----
            static MessageBoxResult SafeShow(string msg, string title, bool isFatal)
            {
                try
                {
                    var owner = Current.MainWindow;
                    var button = isFatal ? MessageBoxButton.OK : MessageBoxButton.YesNo;
                    var icon = isFatal ? MessageBoxImage.Error : MessageBoxImage.Warning;
                    Func<MessageBoxResult> showBox =
                        owner is not null
                            ? () => MessageBox.Show(owner, msg, title, button, icon)
                            : () => MessageBox.Show(msg, title, button, icon);
                    return showBox();
                }
                catch { return MessageBoxResult.Yes; } // 表示不能時は継続扱い（フェイルセーフ）
            }

            Task DecideAsync(MessageBoxResult result)
            {
                // isFatal: 常に終了 / 非致命: No を選んだ場合に終了。Yes は継続。
                if (isFatal || result == MessageBoxResult.No)
                {
                    if (shutdownAsync != null) return shutdownAsync();
                }
                return Task.CompletedTask;
            }
        }
    }
}