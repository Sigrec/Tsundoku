namespace Tsundoku.Models;

public sealed record AniListPickerSuggestion(string Id, string Display, string Format);

public sealed class AniListPickerSuggestionComparer : IComparer<AniListPickerSuggestion>
{
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