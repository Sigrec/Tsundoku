using Avalonia.Data.Converters;
using System.Globalization;
using static Tsundoku.Models.Enums.TsundokuLanguageEnums;

namespace Tsundoku.Converters;

public sealed class StaffLangConverter : IMultiValueConverter
{
    public static readonly StaffLangConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not Dictionary<TsundokuLanguage, string> staff)
        {
            return "ERROR";
        }

        string? languageString = values[1]?.ToString(); // Use nullable string for safety

        // Use the pre-built, optimized dictionary for lookup
        if (languageString is not null && TsundokuLanguageStringValueToLanguageMap.TryGetValue(languageString, out TsundokuLanguage langEnum))
        {
            if (staff.TryGetValue(langEnum, out string? result))
            {
                return result;
            }
        }

        // Fallback to Romaji if the language string was invalid, or not found in staff
        return staff[TsundokuLanguage.Romaji];
    }

    // IMultiValueConverter also requires a ConvertBack method.
    // If you don't need two-way conversion, you can implement it to throw a NotImplementedException.
    public object? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}