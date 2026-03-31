namespace Tsundoku.Models;

/// <summary>
/// Represents a suggestion entry from AniList used in the series title picker.
/// </summary>
/// <param name="Id">The AniList media ID.</param>
/// <param name="Display">The formatted display string shown to the user.</param>
/// <param name="Format">The media format (e.g., "MANGA" or "NOVEL").</param>
public sealed record AniListPickerSuggestion(string Id, string Display, string Format);
