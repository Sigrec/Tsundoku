using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tsundoku.Views;

internal static partial class EditSeriesInfoWindowLogging
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Marked {Title} complete ({VolumeCount} volumes)")]
    public static partial void SeriesMarkedComplete(this ILogger logger, string title, uint volumeCount);
}
