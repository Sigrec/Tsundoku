using Avalonia.Controls;

namespace Tsundoku.Services;

/// <summary>
/// Defines a service for displaying popup dialogs to the user.
/// </summary>
public interface IPopupDialogService
{
    /// <summary>
    /// Displays a popup dialog with the specified title, icon, and information text.
    /// </summary>
    /// <param name="title">The title displayed at the top of the popup.</param>
    /// <param name="icon">The icon identifier shown in the popup.</param>
    /// <param name="info">The informational message body of the popup.</param>
    /// <param name="owner">The parent window that owns the dialog.</param>
    Task ShowAsync(string title, string icon, string info, Window owner);

    /// <summary>
    /// Displays a confirmation dialog and returns true if confirmed, false if cancelled.
    /// </summary>
    Task<bool> ConfirmAsync(string title, string icon, string info, Window owner);

    /// <summary>
    /// Displays an input dialog and returns the entered text, or null if cancelled.
    /// </summary>
    Task<string?> InputAsync(string title, string icon, string prompt, Window owner);
}
