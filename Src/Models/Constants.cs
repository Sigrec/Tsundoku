using System.Collections.Frozen;
using System.Globalization;

namespace Tsundoku.Models;

public static class Constants
{
    #region Layout

    public const byte USER_ICON_WIDTH = 71;
    public const byte USER_ICON_HEIGHT = 71;

    public const ushort CARD_WIDTH = 525;
    public const ushort RIGHT_SIDE_CARD_WIDTH = 355;
    public const ushort LEFT_SIDE_CARD_WIDTH = CARD_WIDTH - RIGHT_SIDE_CARD_WIDTH;

    public const ushort CARD_HEIGHT = 290;
    public const byte BOTTOM_SECTION_CARD_HEIGHT = 40;
    public const ushort TOP_SECTION_CARD_HEIGHT = CARD_HEIGHT - BOTTOM_SECTION_CARD_HEIGHT - 40;

    public const ushort USER_NOTES_WIDTH = RIGHT_SIDE_CARD_WIDTH - 74;
    public const ushort USER_NOTES_HEIGHT = TOP_SECTION_CARD_HEIGHT - 16;

    public const ushort IMAGE_HEIGHT = CARD_HEIGHT - BOTTOM_SECTION_CARD_HEIGHT;
    public const ushort MENU_LENGTH = 400;

    #endregion

    #region Filters

    public static readonly FrozenSet<string> ADVANCED_SEARCH_FILTERS =
        new[]
        {
            "CurVolumes==", "CurVolumes>=", "CurVolumes<=",
            "Demographic==Josei", "Demographic==Seinen", "Demographic==Shoujo", "Demographic==Shounen", "Demographic==Unknown",
            "Favorite==False", "Favorite==True",
            "Format==Manga", "Format==Novel",
            "Genre==Action", "Genre==Adventure", "Genre==Comedy", "Genre==Drama", "Genre==Ecchi", "Genre==Fantasy",
            "Genre==Horror", "Genre==MahouShoujo", "Genre==Mecha", "Genre==Music", "Genre==Mystery",
            "Genre==Psychological", "Genre==Romance", "Genre==SciFi", "Genre==SliceOfLife", "Genre==Sports",
            "Genre==Supernatural", "Genre==Thriller",
            "MaxVolumes==", "MaxVolumes>=", "MaxVolumes<=",
            "Notes==", "Publisher==",
            "Read==", "Read>=", "Read<=",
            "Rating==", "Rating>=", "Rating<=",
            "Series==Complete", "Series==InComplete",
            "Status==Cancelled", "Status==Finished", "Status==Hiatus", "Status==Ongoing",
            "Value==", "Value>=", "Value<="
        }.ToFrozenSet();

    public static readonly FrozenSet<string> AVAILABLE_CURRENCIES = new[]
    {
        "$", "€", "£", "¥", "₹", "₱", "₩", "₽", "₺", "₫", "฿", "₸", "₼", "₾",
        "Rp", "RM", "R$", "₪", "₴", "zł", "Ft", "Kč", "kr", "lei", "৳", "₮",
        "KM", "Br", "L", "din", "ден", "ر.س", "د.إ", "د.ك", "Rs"
    }.ToFrozenSet();

    public static FrozenDictionary<string, (int Index, string Culture)> AVAILABLE_CURRENCY_WITH_CULTURE { get; } = AVAILABLE_CURRENCIES
        .AsValueEnumerable()
        .Select((symbol, index) =>
        {
            string? cultureName = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .AsValueEnumerable()
            .Where(c =>
            {
                try
                {
                    return new RegionInfo(c.Name).CurrencySymbol == symbol;
                }
                catch
                {
                    return false;
                }
            })
            .Select(c => c.Name)
            .FirstOrDefault();

            if (cultureName is null)
            {
                var fallback = symbol switch
                {
                    "din" => "sr-RS",  // Serbian Dinar
                    "ден" => "mk-MK",  // Macedonian Denar
                    "ر.س" => "ar-SA", // Saudi Riyal
                    "د.إ" => "ar-AE",   // UAE Dirham
                    "د.ك" => "ar-KW",  // Kuwaiti Dinar
                    "lei" => "ro-RO",  // Romanian Leu
                    _ => "unknown"
                };

                cultureName = fallback;
            }
            return new KeyValuePair<string, (int, string)>(symbol, (index, cultureName));
        })
        .ToFrozenDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    #endregion
}
