using Avalonia.Controls;
using Tsundoku.ViewModels;
using Tsundoku.Views;

namespace Tsundoku.Services;

/// <summary>
/// Displays loading dialogs during long-running operations using the <see cref="LoadingDialog"/> window.
/// </summary>
public sealed class LoadingDialogService(LoadingDialogViewModel viewModel) : ILoadingDialogService
{
    private readonly LoadingDialogViewModel _viewModel = viewModel;

    /// <inheritdoc />
    public async Task ShowAsync(
        string message,
        Func<LoadingDialogViewModel, Task> work,
        Window owner)
    {
        _viewModel.StatusText = message;

        LoadingDialog dialog = new()
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

    /// <inheritdoc />
    public async Task ShowCancellableAsync(
        string message,
        Func<LoadingDialogViewModel, CancellationToken, Task> work,
        Window owner)
    {
        _viewModel.StatusText = message;

        LoadingDialog dialog = new()
        {
            ViewModel = _viewModel
        };
        dialog.EnableCancellation();
        dialog.Show(owner);

        try
        {
            await work(_viewModel, dialog.CancellationToken);
        }
        finally
        {
            dialog.Close();
        }
    }

    /// <inheritdoc />
    public void Show(
        string message,
        Action<LoadingDialogViewModel> work,
        Window owner)
    {
        _viewModel.StatusText = message;

        LoadingDialog dialog = new()
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
