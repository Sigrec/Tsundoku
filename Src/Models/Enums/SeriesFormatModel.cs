namespace Tsundoku.Models.Enums;

public static class SeriesFormatModel
{
    public enum SeriesFormat
    {
        Manga,    // Japanese comics
        Manhwa,   // Korean comics
        Manhua,   // Chinese comics
        Manfra,   // French comics (often inspired by manga)
        Comic,    // Western comics
        Novel     // Text-based novel (often adapted into manga/manhwa)
    }

    /// <summary>
    /// Determines the SeriesFormat based on the country of origin string from a JSON source.
    /// Defaults to SeriesFormat.Manga for any unlisted country codes, including Japanese.
    /// </summary>
    /// <param name="jsonCountryOfOrigin">The country code or language code string (e.g., "KR", "ko", "JP", "ja").</param>
    /// <returns>The corresponding SeriesFormat enum value.</returns>
    public static SeriesFormat Parse(string jsonCountryOfOrigin)
    {
        // The switch expression effectively maps various country/language codes to SeriesFormat.
        // The '_' (discard) pattern acts as the default case.
        return jsonCountryOfOrigin switch
        {
            // Korean formats
            "KR" or "ko" or "ko-ro" => SeriesFormat.Manhwa,

            // Chinese / Taiwanese formats
            "CN" or "TW" or "zh" or "zh-hk" or "zh-ro" => SeriesFormat.Manhua,

            // French formats (Manfra)
            "FR" or "fr" => SeriesFormat.Manfra,

            // English / Western comics
            "EN" or "en" => SeriesFormat.Comic,

            // Default case: Anything not explicitly matched above (including "JP", "ja", or genuinely unknown)
            // will default to SeriesFormat.Manga.
            _ => SeriesFormat.Manga
        };
    }
}
