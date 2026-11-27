namespace Tsundoku.Helpers;

/// <summary>
/// One applied bulk edit on the undo stack. When <see cref="WasTrash"/> is set, undoing restores
/// the series from the trash instead of applying snapshots.
/// </summary>
public sealed record BulkEditOperation(string Description, IReadOnlyList<BulkEditUndoEntry> Entries, bool WasTrash);
