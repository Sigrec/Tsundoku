using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tsundoku.Services;

internal static partial class UserBackupServiceLogging
{
    #region Backup creation

    [LoggerMessage(Level = LogLevel.Information, Message = "Dated backup created at {BackupPath}")]
    public static partial void BackupCreated(this ILogger logger, string backupPath);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Skipped dated backup; {LatestBackupPath} already matches the current user data")]
    public static partial void BackupSkippedUnchanged(this ILogger logger, string latestBackupPath);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped dated backup; user data file not found at {UserDataPath}")]
    public static partial void BackupSkippedNoSource(this ILogger logger, string userDataPath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create dated backup in {BackupFolder}")]
    public static partial void BackupFailed(this ILogger logger, string backupFolder, Exception ex);

    #endregion

    #region Folder resolution

    [LoggerMessage(Level = LogLevel.Warning, Message = "Custom backup folder {CustomFolder} is not usable; falling back to {DefaultFolder}")]
    public static partial void CustomFolderUnavailable(this ILogger logger, string customFolder, string defaultFolder, Exception? ex);

    #endregion

    #region Pruning

    [LoggerMessage(Level = LogLevel.Debug, Message = "Removed old backup {BackupPath}")]
    public static partial void BackupPruned(this ILogger logger, string backupPath);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to remove old backup {BackupPath}")]
    public static partial void BackupPruneFailed(this ILogger logger, string backupPath, Exception ex);

    #endregion
}
