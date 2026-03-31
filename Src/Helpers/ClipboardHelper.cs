namespace Tsundoku.Helpers;

public static class ClipboardHelper
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    public static async Task CopyToClipboardAsync(string text)
    {
        try
        {
            LOGGER.Info("Copying {Text} to Clipboard", text);
            await TextCopy.ClipboardService.SetTextAsync(text);
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to copy text to clipboard");
        }
    }
}
