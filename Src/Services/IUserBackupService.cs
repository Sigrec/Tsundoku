namespace Tsundoku.Services;

/// <summary>
/// Keeps a rolling set of dated copies of UserData.json so an older version of the collection
/// can be recovered after a bad save, accidental edit, or lost file.
/// </summary>
public interface IUserBackupService
{
    /// <summary>Default number of dated backups kept before the oldest are removed.</summary>
    const int DefaultRetentionCount = 3;

    /// <summary>Retention counts offered in settings.</summary>
    static readonly IReadOnlyList<int> RetentionOptions = [3, 5, 10, 20, 50];

    /// <summary>
    /// Resolves the folder dated backups are written to: the user's custom folder when it is set and
    /// usable, otherwise the app's own Backups folder.
    /// </summary>
    /// <param name="customFolder">The user's chosen backup folder, or <c>null</c> for the default.</param>
    /// <returns>The full path of an existing, writable backup folder.</returns>
    string ResolveBackupFolder(string? customFolder);

    /// <summary>
    /// Copies the current UserData.json into the backup folder under a timestamped name, then removes
    /// the oldest dated backups beyond <paramref name="retentionCount"/>. Skipped when the newest
    /// existing backup is byte-for-byte identical, so repeated opens and closes don't push out older
    /// versions.
    /// </summary>
    /// <param name="customFolder">The user's chosen backup folder, or <c>null</c> for the default.</param>
    /// <param name="retentionCount">How many dated backups to keep.</param>
    /// <returns>The path of the new backup, or <c>null</c> if none was written.</returns>
    string? CreateBackup(string? customFolder, int retentionCount);
}
