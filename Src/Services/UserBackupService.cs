using System.Globalization;
using Microsoft.Extensions.Logging;
using Tsundoku.Helpers;
using MsLogger = Microsoft.Extensions.Logging.ILogger;

namespace Tsundoku.Services;

/// <summary>
/// Writes timestamped copies of UserData.json to a backup folder and keeps only the newest N.
/// </summary>
public sealed class UserBackupService : IUserBackupService
{
    private const string BackupFilePrefix = "UserData_";
    private const string BackupTimestampFormat = "yyyy-MM-dd_HH-mm-ss";
    private const string BackupSearchPattern = "UserData_????-??-??_??-??-??*.json";

    private readonly MsLogger _logger;

    public UserBackupService(ILogger<UserBackupService> logger) => _logger = logger;

    public string ResolveBackupFolder(string? customFolder)
    {
        string defaultFolder = AppFileHelper.GetBackupsFolderPath();
        if (string.IsNullOrWhiteSpace(customFolder))
        {
            return defaultFolder;
        }

        try
        {
            Directory.CreateDirectory(customFolder);
            return customFolder;
        }
        catch (Exception ex)
        {
            _logger.CustomFolderUnavailable(customFolder, defaultFolder, ex);
            return defaultFolder;
        }
    }

    public string? CreateBackup(string? customFolder, int retentionCount)
    {
        string userDataPath = AppFileHelper.GetUserDataJsonPath();
        if (!File.Exists(userDataPath))
        {
            _logger.BackupSkippedNoSource(userDataPath);
            return null;
        }

        string backupFolder = ResolveBackupFolder(customFolder);
        try
        {
            List<FileInfo> existing = GetDatedBackups(backupFolder);
            if (existing.Count > 0 && FilesAreIdentical(userDataPath, existing[0].FullName))
            {
                _logger.BackupSkippedUnchanged(existing[0].FullName);
                return null;
            }

            string backupPath = BuildUniqueBackupPath(backupFolder, DateTime.Now);
            File.Copy(userDataPath, backupPath, overwrite: false);
            _logger.BackupCreated(backupPath);

            PruneOldBackups(backupFolder, Math.Max(1, retentionCount));
            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.BackupFailed(backupFolder, ex);
            return null;
        }
    }

    /// <summary>Dated backups in <paramref name="folder"/>, newest first.</summary>
    private static List<FileInfo> GetDatedBackups(string folder)
    {
        DirectoryInfo dir = new(folder);
        List<FileInfo> backups = [.. dir.EnumerateFiles(BackupSearchPattern, SearchOption.TopDirectoryOnly)];
        backups.Sort(static (a, b) => string.CompareOrdinal(b.Name, a.Name));
        return backups;
    }

    private static string BuildUniqueBackupPath(string folder, DateTime timestamp)
    {
        string stamp = timestamp.ToString(BackupTimestampFormat, CultureInfo.InvariantCulture);
        string path = Path.Combine(folder, $"{BackupFilePrefix}{stamp}.json");
        int suffix = 2;
        while (File.Exists(path))
        {
            path = Path.Combine(folder, $"{BackupFilePrefix}{stamp}_{suffix}.json");
            suffix++;
        }
        return path;
    }

    private void PruneOldBackups(string folder, int retentionCount)
    {
        List<FileInfo> backups = GetDatedBackups(folder);
        for (int i = retentionCount; i < backups.Count; i++)
        {
            try
            {
                backups[i].Delete();
                _logger.BackupPruned(backups[i].FullName);
            }
            catch (Exception ex)
            {
                _logger.BackupPruneFailed(backups[i].FullName, ex);
            }
        }
    }

    private static bool FilesAreIdentical(string pathA, string pathB)
    {
        FileInfo a = new(pathA);
        FileInfo b = new(pathB);
        if (a.Length != b.Length)
        {
            return false;
        }

        const int BufferSize = 81920;
        using FileStream streamA = a.OpenRead();
        using FileStream streamB = b.OpenRead();
        byte[] bufferA = new byte[BufferSize];
        byte[] bufferB = new byte[BufferSize];
        while (true)
        {
            int readA = streamA.ReadAtLeast(bufferA, BufferSize, throwOnEndOfStream: false);
            int readB = streamB.ReadAtLeast(bufferB, BufferSize, throwOnEndOfStream: false);
            if (readA != readB)
            {
                return false;
            }
            if (readA == 0)
            {
                return true;
            }
            if (!bufferA.AsSpan(0, readA).SequenceEqual(bufferB.AsSpan(0, readB)))
            {
                return false;
            }
        }
    }
}
