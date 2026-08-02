namespace Tsundoku.Models;

/// <summary>
/// Wraps a <see cref="Series"/> that has been soft-deleted. Retained for a bounded
/// window (see <c>UserService.TrashRetentionDays</c>) so users can restore accidental
/// deletes; auto-purged on startup and manually via the Trash window.
/// </summary>
public sealed class TrashedSeries
{
    /// <summary>The full Series data as it existed at the moment of deletion.</summary>
    public Series Series { get; set; } = null!;

    /// <summary>UTC timestamp when the series was moved to the trash.</summary>
    public DateTime DeletedAtUtc { get; set; }
}
