using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tsundoku.Helpers;

internal static partial class SingleLinePasteLogging
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to paste clipboard text into a text box")]
    public static partial void ClipboardPasteFailed(this ILogger logger, Exception exception);
}
