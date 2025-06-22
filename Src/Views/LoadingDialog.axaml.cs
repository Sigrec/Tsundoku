using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class LoadingDialog : ReactiveWindow<LoadingDialogViewModel>
{
    private readonly DispatcherTimer _dotTimer;
    private int _dotCount;

    public LoadingDialog()
    {
        InitializeComponent();

        _dotTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };

        _dotTimer.Tick += (_, _) =>
        {
            _dotCount = (_dotCount + 1) % 4;
            StatusTextBlock.Text = ViewModel!.StatusText + new string('.', _dotCount);
        };

        _dotTimer.Start();

        Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _dotTimer.Stop();
        _dotTimer.Tick -= null!;
    }
}