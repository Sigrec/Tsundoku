namespace Tsundoku.Models;

/// <summary>
/// Compares <see cref="AniListPickerSuggestion"/> instances, prioritizing MANGA format
/// and then sorting alphabetically by display name.
/// </summary>
public sealed class AniListPickerSuggestionComparer : IComparer<AniListPickerSuggestion>
{
    /// <summary>
    /// Compares two suggestions, prioritizing MANGA format over NOVEL, then sorting by display name.
    /// </summary>
    /// <param name="x">The first suggestion to compare.</param>
    /// <param name="y">The second suggestion to compare.</param>
    /// <returns>A value indicating the relative order of the suggestions.</returns>
    public int Compare(AniListPickerSuggestion? x, AniListPickerSuggestion? y)
    {
        // Handle null cases
        if (x is null || y is null)
        {
            if (x is null && y is null) return 0;
            return x is null ? -1 : 1;
        }

        // Prioritize "MANGA"
        bool xIsManga = x.Format == "MANGA";
        bool yIsManga = y.Format == "MANGA";

        if (xIsManga && !yIsManga) return -1; // x comes first
        if (!xIsManga && yIsManga) return 1;  // y comes first

        // If both are "MANGA" or both are "NOVEL", sort by Display
        return string.Compare(x.Display, y.Display, StringComparison.OrdinalIgnoreCase);
    }
}
