using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using MsLogger = Microsoft.Extensions.Logging.ILogger;

namespace Tsundoku.Helpers;

/// <summary>
/// Makes single-line text boxes accept multi-line clipboard text by flattening it to one line.
/// Without this a single-line <see cref="TextBox"/> keeps only the text before the first line break,
/// so clipboard text that starts with one (common when copying from a web page) pastes nothing.
/// </summary>
public static class SingleLinePasteHelper
{
    private static readonly char[] LineBreakChars = ['\r', '\n'];

    public static void Register()
    {
        TextBox.PastingFromClipboardEvent.AddClassHandler<TextBox>(OnPastingFromClipboard);
    }

    public static string ToSingleLine(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return string.Join(' ', text.Split(LineBreakChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static async void OnPastingFromClipboard(TextBox textBox, RoutedEventArgs e)
    {
        if (e.Handled || textBox.AcceptsReturn || textBox.IsReadOnly || textBox is MaskedTextBox)
        {
            return;
        }

        IClipboard? clipboard = TopLevel.GetTopLevel(textBox)?.Clipboard;
        if (clipboard is null)
        {
            return;
        }

        e.Handled = true;
        try
        {
            string singleLine = ToSingleLine(await clipboard.TryGetTextAsync());
            if (singleLine.Length != 0)
            {
                textBox.SelectedText = singleLine;
            }
        }
        catch (Exception ex)
        {
            MsLogger logger = AppLog.CreateLogger(nameof(SingleLinePasteHelper));
            logger.ClipboardPasteFailed(ex);
        }
    }
}
