using Tsundoku.Models;
using Tsundoku.Models.Enums;

namespace Tsundoku.Helpers;

/// <summary>
/// Immutable snapshot of the user-editable fields on a <see cref="Series"/>.
/// Used by the Edit Series window's Revert / Reapply buttons and by bulk-edit undo to roll
/// changes back to a known point.
/// </summary>
public sealed record SeriesSnapshot(
    uint MaxVolumeCount,
    IReadOnlyList<decimal> OwnedVolumes,
    IReadOnlyList<decimal> SpecialVolumes,
    IReadOnlyList<ExtraVolume> ExtraVolumes,
    uint VolumesRead,
    decimal Value,
    decimal Rating,
    string Publisher,
    SeriesFormatModel.SeriesFormat Format,
    SeriesStatusModel.SeriesStatus Status,
    SeriesDemographicModel.SeriesDemographic Demographic,
    string SeriesNotes,
    HashSet<SeriesGenreModel.SeriesGenre> Genres)
{
    public static SeriesSnapshot Capture(Series s) => new(
        s.MaxVolumeCount, [.. s.OwnedVolumes], [.. s.SpecialVolumes], [.. s.ExtraVolumes], s.VolumesRead, s.Value, s.Rating,
        s.Publisher, s.Format, s.Status, s.Demographic, s.SeriesNotes,
        [.. s.Genres]);

    /// <summary>Owned-volume state equal to the series' current state.</summary>
    public bool VolumesMatch(Series s) =>
        MaxVolumeCount == s.MaxVolumeCount
        && OwnedVolumes.SequenceEqual(s.OwnedVolumes)
        && SpecialVolumes.SequenceEqual(s.SpecialVolumes)
        && ExtraVolumes.SequenceEqual(s.ExtraVolumes);

    public void ApplyTo(Series s)
    {
        s.MaxVolumeCount = MaxVolumeCount;
        s.SpecialVolumes = [.. SpecialVolumes];
        s.OwnedVolumes = [.. OwnedVolumes];
        s.ExtraVolumes = [.. ExtraVolumes];
        s.VolumesRead = VolumesRead;
        s.Value = Value;
        s.Rating = Rating;
        s.Publisher = Publisher;
        s.Format = Format;
        s.Status = Status;
        s.Demographic = Demographic;
        s.SeriesNotes = SeriesNotes;
        s.Genres = [.. Genres];
    }
}
