using Avalonia.Controls;
using ReactiveUI.Avalonia;

namespace Tsundoku.Helpers;

public static class WindowHelper
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Opens or activates a managed ReactiveWindow in a non-blocking manner.
    /// </summary>
    /// <typeparam name="TWindow">The type of the ReactiveWindow to open/activate.</typeparam>
    /// <typeparam name="TViewModel">The ViewModel type of the ReactiveWindow.</typeparam>
    /// <param name="parentWindow">The parent window (e.g., MainWindow) for context (currently unused in this helper, but good practice).</param>
    /// <param name="windowInstance">The instance of the window to open, retrieved from the ViewModel.</param>
    /// <param name="windowNameForLogging">A friendly name for the window, used in log messages.</param>
    /// <returns>The opened/activated window instance if successful; otherwise, null.</returns>
    public static TWindow? OpenManagedWindow<TWindow, TViewModel>(
        this Window parentWindow,
        TWindow? windowInstance,
        string windowNameForLogging)
        where TWindow : ReactiveWindow<TViewModel>
        where TViewModel : class
    {
        if (windowInstance is null)
        {
            LOGGER.Error("{Window} instance is null. Cannot open window", windowNameForLogging);
            return null;
        }

        try
        {
            windowInstance.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            windowInstance.WindowState = WindowState.Normal;

            if (!windowInstance.IsVisible)
            {
                windowInstance.Show(parentWindow);
            }
            else
            {
                windowInstance.Activate();
                windowInstance.Focus();
            }
            return windowInstance;
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to open or activate {Window}", windowNameForLogging);
            return null;
        }
    }

    /// <summary>
    /// Asynchronously opens a managed ReactiveWindow as a blocking (modal) dialog and returns its result.
    /// This is used for dialogs that are meant to truly close and return a value.
    /// </summary>
    /// <typeparam name="TWindow">The type of the ReactiveWindow (dialog) to open.</typeparam>
    /// <typeparam name="TViewModel">The ViewModel type of the dialog.</typeparam>
    /// <typeparam name="TResult">The type of the result expected from the dialog (e.g., bool, string, a custom object).</typeparam>
    /// <param name="parentWindow">The parent window (e.g., MainWindow) that will be blocked.</param>
    /// <param name="dialogInstance">The instance of the dialog window to open.</param>
    /// <param name="windowNameForLogging">A friendly name for the dialog, used in log messages.</param>
    /// <returns>A Task representing the asynchronous operation, which resolves to the dialog's result (TResult) or default(TResult) if an error occurs.</returns>
    public static async Task<TResult?> OpenManagedDialogWithResultAsync<TWindow, TViewModel, TResult>(
        this Window parentWindow, // This is an extension method, callable on any Window instance
        TWindow? dialogInstance,
        string windowNameForLogging)
        where TWindow : ReactiveWindow<TViewModel>
        where TViewModel : class // Constraint for the ViewModel type
    {
        if (dialogInstance is null)
        {
            LOGGER.Error("{Window} instance is null. Cannot open dialog", windowNameForLogging);
            return default; // Return default if instance is null
        }

        try
        {
            // Ensure the dialog is in a normal state before showing
            dialogInstance.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialogInstance.WindowState = WindowState.Normal;
            LOGGER.Debug("Opening {Window} as a modal dialog", windowNameForLogging);
            
            // Await ShowDialog, passing the TResult type argument to get the dialog's return value
            TResult? result = await dialogInstance.ShowDialog<TResult?>(parentWindow);
            LOGGER.Debug($"{windowNameForLogging} dialog closed with result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to open {Window} as a dialog", windowNameForLogging);
            return default; // Return default on error
        }
    }
}