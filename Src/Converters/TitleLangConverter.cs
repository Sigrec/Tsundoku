using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Data.Converters;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Converters;

public sealed class TitleLangConverter : IMultiValueConverter
{
    public static readonly TitleLangConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count < 2)
            return "ERROR";

        if (!TryGetTitles(values[0], out IReadOnlyDictionary<TsundokuLanguage, string> titles))
            return "ERROR";

        TsundokuLanguage effective = TsundokuLanguage.Romaji;
        if (values[1] is TsundokuLanguage lang && titles.ContainsKey(lang))
            effective = lang;

        if (!titles.TryGetValue(effective, out string title) || string.IsNullOrEmpty(title))
        {
            // If we tried a non-Romaji key first, fallback to Romaji
            if (effective != TsundokuLanguage.Romaji &&
                (!titles.TryGetValue(TsundokuLanguage.Romaji, out title) || string.IsNullOrEmpty(title)))
            {
                return "ERROR";
            }

            // If effective already Romaji and it's missing/empty, it's an error
            if (effective == TsundokuLanguage.Romaji)
                return "ERROR";
        }

        if (values.Count >= 3 && values[2] is not null)
        {
            if (!TryReadUInt(values[2], out uint di))
                return "ERROR";

            if (di != 0)
                return AppendSuffix(title, di);
        }

        return title;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetTitles(object? source, out IReadOnlyDictionary<TsundokuLanguage, string> dict)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReadUInt(object? value, out uint result)
    {
        if (value is uint u) { result = u; return true; }
        if (value is int i && i >= 0) { result = (uint)i; return true; }
        if (value is byte b) { result = b; return true; }
        if (value is ushort us) { result = us; return true; }
        if (value is long l && l >= 0 && l <= uint.MaxValue) { result = (uint)l; return true; }
        if (value is string s)
            return uint.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out result);

        result = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string AppendSuffix(string title, uint n)
    {
        // total length = title + " (" + digits(n) + ")"
        int digits = CountDigits(n);
        int totalLength = title.Length + 3 + digits; // 3 => space + '(' + ')'

        return string.Create(totalLength, (title, n), static (span, state) =>
        {
            int idx = 0;

            // copy title
            state.title.AsSpan().CopyTo(span);
            idx += state.title.Length;

            // append " ("
            span[idx++] = ' ';
            span[idx++] = '(';

            // append number
            if (!state.n.TryFormat(span[idx..], out int written, provider: CultureInfo.InvariantCulture))
                written = 0;
            idx += written;

            // append ')'
            span[idx] = ')';
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CountDigits(uint n)
    {
        // Cheap digit count without loops for common ranges
        if (n < 10U) return 1;
        if (n < 100U) return 2;
        if (n < 1_000U) return 3;
        if (n < 10_000U) return 4;
        if (n < 100_000U) return 5;
        if (n < 1_000_000U) return 6;
        if (n < 10_000_000U) return 7;
        if (n < 100_000_000U) return 8;
        if (n < 1_000_000_000U) return 9;
        return 10;
    }
}