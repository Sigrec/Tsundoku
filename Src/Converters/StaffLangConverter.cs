using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Data.Converters;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Converters;

public sealed class StaffLangConverter : IMultiValueConverter
{
    public static readonly StaffLangConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count < 2)
            return "ERROR";

        if (!TryGetMap(values[0], out IReadOnlyDictionary<TsundokuLanguage, string> staff))
            return "ERROR";

        TsundokuLanguage effective = TsundokuLanguage.Romaji;

        // Handle either enum or string for values[1]
        if (values[1] is TsundokuLanguage langEnum)
        {
            if (staff.ContainsKey(langEnum))
                effective = langEnum;
        }
        else if (values[1] is string langString)
        {
            if (TsundokuLanguageStringValueToLanguageMap.TryGetValue(langString, out TsundokuLanguage mapped) && staff.ContainsKey(mapped))
                effective = mapped;
        }
        else if (values[1] is not null)
        {
            return "ERROR";
        }

        if (!staff.TryGetValue(effective, out string result) || string.IsNullOrWhiteSpace(result))
        {
            if (effective != TsundokuLanguage.Romaji &&
                (!staff.TryGetValue(TsundokuLanguage.Romaji, out result) || string.IsNullOrWhiteSpace(result)))
            {
                return "ERROR";
            }

            if (effective == TsundokuLanguage.Romaji)
                return "ERROR";
        }

        return result;
    }

    public object? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetMap(object? source, out IReadOnlyDictionary<TsundokuLanguage, string> dict)
    {
        if (source is IReadOnlyDictionary<TsundokuLanguage, string> ro)
        {
            dict = ro;
            return true;
        }

        if (source is IDictionary<TsundokuLanguage, string> d)
        {
            IReadOnlyDictionary<TsundokuLanguage, string>? ro2 = d as IReadOnlyDictionary<TsundokuLanguage, string>;
            dict = ro2 ?? new Dictionary<TsundokuLanguage, string>(d);
            return true;
        }

        dict = default!;
        return false;
    }
}