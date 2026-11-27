namespace Tsundoku.Helpers;

/// <summary>
/// The state of one series captured just before a bulk edit, so the edit can be rolled back.
/// <see cref="IsFavorite"/> is stored alongside the snapshot because <see cref="SeriesSnapshot"/>
/// doesn't cover it.
/// </summary>
public sealed record BulkEditUndoEntry(Guid SeriesId, SeriesSnapshot Snapshot, bool IsFavorite);
