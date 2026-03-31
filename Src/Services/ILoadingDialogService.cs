using Avalonia.Controls;
using Tsundoku.ViewModels;

namespace Tsundoku.Services;

/// <summary>
/// Defines a service for displaying loading dialogs during long-running operations.
/// </summary>
public interface ILoadingDialogService
{
    /// <summary>
    /// Displays an asynchronous loading dialog while the specified work executes.
    /// </summary>
    /// <param name="message">The status message displayed to the user.</param>
    /// <param name="work">The asynchronous work to execute while the dialog is shown.</param>
    /// <param name="owner">The parent window that owns the dialog.</param>
    Task ShowAsync(string message, Func<LoadingDialogViewModel, Task> work, Window owner);

    /// <summary>
    /// Displays a cancellable asynchronous loading dialog while the specified work executes.
    /// </summary>
    /// <param name="message">The status message displayed to the user.</param>
    /// <param name="work">The asynchronous work to execute, with a cancellation token for cooperative cancellation.</param>
    /// <param name="owner">The parent window that owns the dialog.</param>
    Task ShowCancellableAsync(string message, Func<LoadingDialogViewModel, CancellationToken, Task> work, Window owner);

    /// <summary>
    /// Displays a synchronous loading dialog while the specified work executes.
    /// </summary>
    /// <param name="message">The status message displayed to the user.</param>
    /// <param name="work">The synchronous work to execute while the dialog is shown.</param>
    /// <param name="owner">The parent window that owns the dialog.</param>
    void Show(string message, Action<LoadingDialogViewModel> work, Window owner);
}
