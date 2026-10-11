using System.Reactive;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace Tsundoku.Helpers;

/// <summary>
/// Catches errors that nothing else handled once the app is running, logs them, and shows
/// them in an <see cref="ErrorDialog"/> instead of letting the app close without explanation.
/// </summary>
public static class UnhandledErrorReporter
{
    private const string DialogTitle = "Tsundoku Ran Into an Error";
    private const string DialogSummary = "Something went wrong. You can keep using Tsundoku, but restart it if anything looks off.";

    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private static IClassicDesktopStyleApplicationLifetime? _desktop;
    private static bool _isDialogOpen;

    /// <summary>Receives errors raised by ReactiveUI commands and bindings that had no subscriber.</summary>
    public static IObserver<Exception> ReactiveExceptionObserver { get; } = Observer.Create<Exception>(
        exception => Report(exception, "Unhandled ReactiveUI error"));

    public static void Register(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _desktop = desktop;

        Dispatcher.UIThread.UnhandledException += (sender, e) =>
        {
            e.Handled = true;
            Report(e.Exception, "Unhandled UI thread error");
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            e.SetObserved();
            LOGGER.Error(e.Exception, "Unobserved background task error");
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            LOGGER.Fatal(e.ExceptionObject as Exception, "Unhandled error on a background thread; the app is closing");
    }

    private static void Report(Exception exception, string logMessage)
    {
        LOGGER.Error(exception, logMessage);
        Dispatcher.UIThread.Post(() => _ = ShowAsync(exception));
    }

    private static async Task ShowAsync(Exception exception)
    {
        if (_isDialogOpen || App.IsShuttingDown || FindOwner() is not Window owner)
        {
            return;
        }

        _isDialogOpen = true;
        try
        {
            await ErrorDialog.ShowAsync(owner, DialogTitle, DialogSummary, exception);
        }
        finally
        {
            _isDialogOpen = false;
        }
    }

    private static Window? FindOwner()
    {
        if (_desktop is null)
        {
            return null;
        }

        foreach (Window window in _desktop.Windows)
        {
            if (window.IsActive && window.IsVisible)
            {
                return window;
            }
        }

        return _desktop.MainWindow is { IsVisible: true } mainWindow ? mainWindow : null;
    }
}
