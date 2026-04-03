using System.Collections.Frozen;
using Tsundoku.Models.Enums;

namespace Tsundoku.Models;

public static class Constants
{
    #region Layout

    /// <summary>
    /// Bitmap storage scale factor for high-DPI displays.
    /// Covers and icons are stored at this multiplier of logical size.
    /// </summary>
    public const byte BITMAP_SCALE = 2;

    public const byte USER_ICON_WIDTH = 71;
    public const byte USER_ICON_HEIGHT = 71;

    public const ushort CARD_WIDTH = 525;
    public const ushort LEFT_SIDE_CARD_WIDTH = 165;
    public const ushort RIGHT_SIDE_CARD_WIDTH = CARD_WIDTH - LEFT_SIDE_CARD_WIDTH;

    public const ushort CARD_HEIGHT = 250;
    public const byte BOTTOM_SECTION_CARD_HEIGHT = 38;
    public const ushort TOP_SECTION_CARD_HEIGHT = CARD_HEIGHT - BOTTOM_SECTION_CARD_HEIGHT;

    public const ushort USER_NOTES_WIDTH = RIGHT_SIDE_CARD_WIDTH - 74;
    public const ushort USER_NOTES_HEIGHT = TOP_SECTION_CARD_HEIGHT - 16;

    public const ushort IMAGE_HEIGHT = CARD_HEIGHT;

    public const byte CARD_MARGIN = 12;
    public const double CARD_CELL_WIDTH = CARD_WIDTH + (CARD_MARGIN * 2);
    public const double CARD_CELL_HEIGHT = CARD_HEIGHT + (CARD_MARGIN * 2);
    public const ushort MENU_LENGTH = 400;

    #endregion

    #region Filters

    /// <summary>Currency symbols — derived from CurrencyModel enum.</summary>
    public static readonly FrozenSet<string> AVAILABLE_CURRENCIES = CurrencyModel.AVAILABLE_CURRENCIES.ToFrozenSet();

    /// <summary>Currency symbol → (index, culture) mapping — derived from CurrencyModel enum.</summary>
    public static FrozenDictionary<string, (int Index, string Culture)> AVAILABLE_CURRENCY_WITH_CULTURE => CurrencyModel.AVAILABLE_CURRENCY_WITH_CULTURE;

    #endregion
}
