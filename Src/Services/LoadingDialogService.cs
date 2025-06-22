using Avalonia.Controls;
using Tsundoku.ViewModels;
using Tsundoku.Views;

namespace Tsundoku.Services;

public interface ILoadingDialogService
{
    Task ShowAsync(string message, Func<LoadingDialogViewModel, Task> work, Window owner);
    void Show(string message, Action<LoadingDialogViewModel> work, Window owner);
}

public class LoadingDialogService : ILoadingDialogService
{
    private readonly LoadingDialogViewModel _viewModel;

    public LoadingDialogService(LoadingDialogViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public async Task ShowAsync(
        string message,
        Func<LoadingDialogViewModel, Task> work,
        Window owner)
    {
        _viewModel.StatusText = message;

        LoadingDialog dialog = new LoadingDialog
        {
            ViewModel = _viewModel
        };

        dialog.Show(owner);

        try
        {
            await work(_viewModel);
        }
        finally
        {
            dialog.Close();
        }
    }

    public void Show(
        string message,
        Action<LoadingDialogViewModel> work,
        Window owner)
    {
        _viewModel.StatusText = message;

        LoadingDialog dialog = new LoadingDialog
        {
            ViewModel = _viewModel
        };

        dialog.Show(owner);

        try
        {
            work(_viewModel);
        }
        finally
        {
            dialog.Close();
        }
    }
}