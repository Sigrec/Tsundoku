using Avalonia.Controls;
using Avalonia.Input;

namespace Tsundoku.Views;

/// <summary>Reference of every keyboard shortcut and click action in the app, opened from Settings.</summary>
public sealed partial class ControlsWindow : Window
{
    public ControlsWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
    }

    public static Task ShowForAsync(Window owner) => new ControlsWindow().ShowDialog(owner);

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}
