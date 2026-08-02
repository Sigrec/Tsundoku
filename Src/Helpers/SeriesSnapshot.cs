using Tsundoku.Models;
using Tsundoku.Models.Enums;

namespace Tsundoku.Helpers;

/// <summary>
/// Immutable snapshot of the user-editable fields on a <see cref="Series"/>.
/// Used by the Edit Series window's Revert / Reapply buttons to roll unsaved
/// changes back to a known point (window open or last Save).
/// </summary>
public sealed record SeriesSnapshot(
    uint MaxVolumeCount,
    uint CurVolumeCount,
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
        s.MaxVolumeCount, s.CurVolumeCount, s.VolumesRead, s.Value, s.Rating,
        s.Publisher, s.Format, s.Status, s.Demographic, s.SeriesNotes,
        [.. s.Genres]);

    public void ApplyTo(Series s)
    {
        s.MaxVolumeCount = MaxVolumeCount;
        s.CurVolumeCount = CurVolumeCount;
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
