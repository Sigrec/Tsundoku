using Tsundoku.Models;

namespace Tsundoku.Helpers;

/// <summary>
/// One row in the series picker. Carries the series itself so selection is tracked by
/// <see cref="Series.Id"/>, not by title, which two series can share.
/// </summary>
public sealed class SeriesPickerItem(Series series, string displayText)
{
    public Series Series { get; } = series;

    public string DisplayText { get; } = displayText;

    public override string ToString() => DisplayText;
}
