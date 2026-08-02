using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using Tsundoku.Helpers;
using Tsundoku.Services;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class TrashWindow : ReactiveWindow<TrashViewModel>, IManagedWindow
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly IPopupDialogService _popupDialogService;
    public bool IsOpen { get; set; }

    public TrashWindow(TrashViewModel viewModel, IPopupDialogService popupDialogService)
    {
        ViewModel = viewModel;
        _popupDialogService = popupDialogService;
        InitializeComponent();
        this.ConfigureHideOnClose();
    }

    private void RestoreClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Guid id })
        {
            ViewModel?.Restore(id);
        }
    }

    private async void PurgeClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Guid id }) return;

        bool confirmed = await _popupDialogService.ConfirmAsync(
            "Delete Forever",
            "fa7-solid fa7-triangle-exclamation",
            "Permanently delete this series? This cannot be undone.",
            this);
        if (confirmed)
        {
            ViewModel?.Purge(id);
        }
    }

    private async void EmptyTrashClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null || ViewModel.TrashCount == 0) return;

        bool confirmed = await _popupDialogService.ConfirmAsync(
            "Empty Trash",
            "fa7-solid fa7-fire",
            $"Permanently delete all {ViewModel.TrashCount} series in the trash? This cannot be undone.",
            this);
        if (confirmed)
        {
            ViewModel.EmptyTrash();
        }
    }
}
