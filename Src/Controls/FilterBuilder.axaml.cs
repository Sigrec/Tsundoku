using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;

namespace Tsundoku.Controls;

public sealed partial class FilterBuilder : UserControl
{
    /// <summary>
    /// Converts bool (IsConnectorOr) to "OR"/"AND" text for the connector pill.
    /// </summary>
    public static readonly FuncValueConverter<bool, string> ConnectorTextConverter =
        new(isOr => isOr ? "OR" : "AND");

    public FilterBuilder()
    {
        Resources["ConnectorTextConverter"] = ConnectorTextConverter;
        InitializeComponent();
    }

    private FilterBuilderViewModel? ViewModel => DataContext as FilterBuilderViewModel;

    private void OnAddChip(object? sender, RoutedEventArgs e)
    {
        ViewModel?.AddChip();
    }

    private void OnClearAll(object? sender, RoutedEventArgs e)
    {
        ViewModel?.ClearAll();
    }

    private void OnRemoveChip(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: FilterChipViewModel chip })
        {
            ViewModel?.RemoveChip(chip);
        }
    }

    private void OnToggleConnector(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: FilterChipViewModel chip })
        {
            chip.ToggleConnector();
        }
    }
}
