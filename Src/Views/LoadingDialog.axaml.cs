using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI.Avalonia;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class LoadingDialog : ReactiveWindow<LoadingDialogViewModel>
{
    private readonly DispatcherTimer _dotTimer;
    private int _dotCount;
    private CancellationTokenSource? _cts;

    public CancellationToken CancellationToken => _cts?.Token ?? CancellationToken.None;

    public LoadingDialog()
    {
        InitializeComponent();

        _dotTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };

        _dotTimer.Tick += OnDotTimerTick;
        _dotTimer.Start();

        Closed += OnClosed;
    }

    public void EnableCancellation()
    {
        _cts = new CancellationTokenSource();
        CancelButton.IsVisible = true;
    }

    private void OnDotTimerTick(object? sender, EventArgs e)
    {
        _dotCount = (_dotCount + 1) % 4;
        StatusTextBlock.Text = ViewModel!.StatusText + new string('.', _dotCount);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        CancelButton.IsEnabled = false;
        CancelButton.Content = "Cancelling...";
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _dotTimer.Stop();
        _dotTimer.Tick -= OnDotTimerTick;
        _cts?.Dispose();
    }
}
