namespace Tsundoku.Models.Enums;

/// <summary>
/// Specifies culture, AniList, and MangaDex codes for a TsundokuLanguage enum value.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class LanguageCodeAttribute : Attribute
{
    /// <summary>BCP-47 culture code (e.g., "ja-JP", "en-US").</summary>
    public string Culture { get; }

    /// <summary>MangaDex language codes that map to this language. Comma-separated for multiple (e.g., "ja" or "pt,pt-br").</summary>
    public string MangaDex { get; init; } = string.Empty;

    /// <summary>AniList country code (e.g., "JP", "KR"). Empty if AniList doesn't use this language.</summary>
    public string AniList { get; init; } = string.Empty;

    public LanguageCodeAttribute(string culture)
    {
        Culture = culture;
    }
}
