using Avalonia.Controls;
using Tsundoku.ViewModels;
using Tsundoku.Views;

namespace Tsundoku.Services;

public interface IPopupDialogService
{
    Task ShowAsync(string title, string icon, string info,  Window owner);
}

public sealed class PopupDialogService(PopupDialogViewModel viewModel) : IPopupDialogService
{
    private readonly PopupDialogViewModel _viewModel = viewModel;

    public async Task ShowAsync(
        string title,
        string icon,
        string info,
        Window owner)
    {
        _viewModel.SetPopupInfo(title, icon, info);
        PopupDialog dialog = new PopupDialog
        {
            ViewModel = _viewModel
        };

        try
        {
            await dialog.ShowDialog(owner);
        }
        finally
        {
            dialog.Close();
        }
    }
}