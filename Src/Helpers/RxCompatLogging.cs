using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tsundoku.Helpers;

internal static partial class RxCompatLogging
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Scheduled UI action dropped: target was disposed before dispatch")]
    public static partial void ScheduledActionTargetDisposed(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Scheduled UI action faulted")]
    public static partial void ScheduledActionFaulted(this ILogger logger, Exception ex);
}
