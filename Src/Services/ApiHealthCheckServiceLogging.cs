using System.Net;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tsundoku.Services;

internal static partial class ApiHealthCheckServiceLogging
{
    [LoggerMessage(Level = LogLevel.Information, Message = "API health check service started (interval: {IntervalMinutes} minutes)")]
    public static partial void ServiceStarted(this ILogger logger, int intervalMinutes);

    [LoggerMessage(Level = LogLevel.Information, Message = "API health check service stopped")]
    public static partial void ServiceStopped(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception during API health check")]
    public static partial void UnhandledCheckException(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "AniList API health check failed: {Error}")]
    public static partial void AniListCheckFailedResponse(this ILogger logger, string error);

    [LoggerMessage(Level = LogLevel.Debug, Message = "AniList API health check passed")]
    public static partial void AniListCheckPassed(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "AniList API health check failed with exception")]
    public static partial void AniListCheckFailedException(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "MangaDex API health check failed: {StatusCode}")]
    public static partial void MangaDexCheckFailedResponse(this ILogger logger, HttpStatusCode statusCode);

    [LoggerMessage(Level = LogLevel.Debug, Message = "MangaDex API health check passed")]
    public static partial void MangaDexCheckPassed(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "MangaDex API health check failed with exception")]
    public static partial void MangaDexCheckFailedException(this ILogger logger, Exception ex);
}
