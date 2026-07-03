using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using Tsundoku.Models.Enums;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class RandomPickerWindow : ReactiveWindow<RandomPickerViewModel>
{
    public string? RequestedPriceAnalysisTitle { get; private set; }
    public bool RequestedIsNovel { get; private set; }

    public RandomPickerWindow(RandomPickerViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
            case Key.Space:
            case Key.R:
                if (ViewModel is { HasPick: true })
                {
                    ViewModel.Roll();
                    e.Handled = true;
                }
                break;
        }
    }

    public void RollOnOpen()
    {
        ViewModel!.Roll();
    }

    private void RollAgain(object? sender, RoutedEventArgs e)
    {
        ViewModel!.Roll();
    }

    private void CheckPrices(object? sender, RoutedEventArgs e)
    {
        if (ViewModel?.PickedSeries is null) return;

        Models.Series picked = ViewModel.PickedSeries;
        RequestedPriceAnalysisTitle = !string.IsNullOrWhiteSpace(ViewModel.MainTitle)
            ? ViewModel.MainTitle
            : picked.Titles.Values.FirstOrDefault() ?? string.Empty;
        RequestedIsNovel = picked.Format == SeriesFormatModel.SeriesFormat.Novel;
        Close();
    }

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
