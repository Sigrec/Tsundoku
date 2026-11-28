using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tsundoku.ViewModels;

internal static partial class BulkEditLogging
{
    #region Edits

    [LoggerMessage(Level = LogLevel.Information, Message = "Bulk edit \"{Description}\" applied to {ChangedCount} of {SelectedCount} selected series")]
    public static partial void BulkEditApplied(this ILogger logger, string description, int changedCount, int selectedCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Bulk edit \"{Description}\" undone for {SeriesCount} series")]
    public static partial void BulkEditUndone(this ILogger logger, string description, int seriesCount);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Bulk edit \"{Description}\" skipped series {SeriesId}: {Reason}")]
    public static partial void BulkEditSeriesSkipped(this ILogger logger, string description, Guid seriesId, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Bulk edit \"{Description}\" failed on series {SeriesId}")]
    public static partial void BulkEditSeriesFailed(this ILogger logger, string description, Guid seriesId, Exception ex);

    #endregion

    #region Trash

    [LoggerMessage(Level = LogLevel.Information, Message = "Moved {SeriesCount} selected series to the trash")]
    public static partial void BulkTrashApplied(this ILogger logger, int seriesCount);

    #endregion

    #region Selection

    [LoggerMessage(Level = LogLevel.Debug, Message = "Bulk selection changed: {SelectedCount} selected, {HiddenCount} hidden by the current filter")]
    public static partial void SelectionChanged(this ILogger logger, int selectedCount, int hiddenCount);

    #endregion
}
