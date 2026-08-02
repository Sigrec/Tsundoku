using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Avalonia.Threading;
using MsLogger = Microsoft.Extensions.Logging.ILogger;

namespace Tsundoku.Helpers;

/// <summary>
/// System.Reactive-compatible scheduler that dispatches work onto the Avalonia UI thread.
/// Replaces the removed <c>Avalonia.ReactiveUI.AvaloniaScheduler</c> and bridges around
/// ReactiveUI 24.x's shift of <c>RxSchedulers.MainThreadScheduler</c> to its new
/// <c>ISequencer</c> abstraction, which is no longer assignable to <c>IScheduler</c>.
/// </summary>
public sealed class TsundokuUIScheduler : LocalScheduler
{
    public static readonly TsundokuUIScheduler Instance = new();

    private TsundokuUIScheduler() { }

    private static readonly MsLogger _logger = AppLog.CreateLogger<TsundokuUIScheduler>();

    public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        SingleAssignmentDisposable subscription = new();

        void Run()
        {
            if (subscription.IsDisposed) return;

            try
            {
                subscription.Disposable = action(this, state);
            }
            catch (ObjectDisposedException ex)
            {
                _logger.ScheduledActionTargetDisposed(ex);
            }
            catch (Exception ex)
            {
                _logger.ScheduledActionFaulted(ex);
            }
        }

        if (dueTime <= TimeSpan.Zero)
        {
            // Run inline when already on the UI thread so subscriptions to
            // BehaviorSubjects observe their current value synchronously — matches
            // the old Avalonia.ReactiveUI AvaloniaScheduler semantics. Deferring
            // here would break WhenAnyValue/ObserveOn chains that other code
            // relies on to fire during construction (e.g. two-way ComboBox
            // bindings that must not race with the observable-driven writer).
            if (Dispatcher.UIThread.CheckAccess())
            {
                Run();
            }
            else
            {
                Dispatcher.UIThread.Post(Run);
            }
        }
        else
        {
            DispatcherTimer.RunOnce(Run, dueTime);
        }

        return subscription;
    }
}

/// <summary>
/// Static holder that exposes an <see cref="IScheduler"/> for the UI thread.
/// Use in place of the ReactiveUI 24.x <c>RxSchedulers.MainThreadScheduler</c>
/// (which is now typed as <c>ISequencer</c> and can no longer be passed to
/// System.Reactive's <c>ObserveOn</c>).
/// </summary>
public static class TsundokuSchedulers
{
    public static IScheduler MainThread => TsundokuUIScheduler.Instance;
    public static IScheduler TaskPool => System.Reactive.Concurrency.TaskPoolScheduler.Default;
}

/// <summary>
/// Bridges the <c>DisposeWith(this IDisposable, CompositeDisposable)</c> pattern
/// to ReactiveUI 24.x's new <c>WhenActivated(Action{Action{IDisposable}})</c>
/// callback, which hands the closure an <c>Action&lt;IDisposable&gt;</c>
/// composer instead of a <c>CompositeDisposable</c>.
/// </summary>
public static class DisposeWithExtensions
{
    public static T DisposeWith<T>(this T disposable, Action<IDisposable> composer) where T : IDisposable
    {
        ArgumentNullException.ThrowIfNull(composer);
        composer(disposable);
        return disposable;
    }
}
