using System.Globalization;

namespace Tsundoku.Helpers;

/// <summary>
/// Treats Series.Value as a 2-decimal baseline (USD-like) and converts to/from
/// the active currency's digit layout by shifting the decimal. No FX rate is
/// applied — the digits are preserved and only the decimal position moves
/// (e.g. 1000.55 at baseline ↔ 100055 in JPY which has 0 decimal digits).
/// </summary>
internal static class CurrencyValueHelper
{
    private const int BaselineDecimals = 2;

    /// <summary>Baseline stored value → value formatted for the given culture.</summary>
    public static decimal ToDisplay(decimal baselineValue, CultureInfo cultureInfo)
    {
        int shift = BaselineDecimals - cultureInfo.NumberFormat.CurrencyDecimalDigits;
        return Shift(baselineValue, shift);
    }

    /// <summary>Value entered in the given culture → baseline stored value.</summary>
    public static decimal ToBaseline(decimal displayValue, CultureInfo cultureInfo)
    {
        int shift = BaselineDecimals - cultureInfo.NumberFormat.CurrencyDecimalDigits;
        return Shift(displayValue, -shift);
    }

    private static decimal Shift(decimal value, int shift) => shift switch
    {
        > 0 => value * Pow10(shift),
        < 0 => value / Pow10(-shift),
        _ => value
    };

    private static decimal Pow10(int exponent)
    {
        decimal result = 1m;
        for (int i = 0; i < exponent; i++) result *= 10m;
        return result;
    }
}
