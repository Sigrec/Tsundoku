using Avalonia.Controls;
using Avalonia.Threading;

namespace Tsundoku.Views;

/// <summary>
/// Lightweight splash window shown during application startup.
/// </summary>
public sealed partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Updates the status text shown on the splash screen. Thread-safe.
    /// </summary>
    public void UpdateStatus(string status)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            StatusText.Text = status;
        }
        else
        {
            Dispatcher.UIThread.Post(() => StatusText.Text = status);
        }
    }
}
