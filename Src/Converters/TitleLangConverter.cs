using Avalonia.Data.Converters;
using System.Globalization;
using static Tsundoku.Models.Enums.TsundokuLanguageEnums;

namespace Tsundoku.Converters;

public sealed class TitleLangConverter : IMultiValueConverter
{
    public static readonly TitleLangConverter Instance = new();

    public object? Convert(IList<object?> values, Type type, object? parameter, CultureInfo culture)
    {
        // Input validation and initial setup
        if (values[0] is not Dictionary<TsundokuLanguage, string> titles)
        {
            return "ERROR";
        }

        TsundokuLanguage? lang = (TsundokuLanguage)values[1]!;
        
        // Determine the language to use for title lookup.
        // Prioritize the language from the string value, falling back to Romaji.
        TsundokuLanguage effectiveLanguage = TsundokuLanguage.Romaji; // Default fallback

        if (lang is not null && titles.ContainsKey(lang.Value))
        {
            effectiveLanguage = lang.Value;
        }

        // Get the title using the determined effective language
        string title = titles[effectiveLanguage];

        // Append duplicate index if present and not zero
        if (values.Count == 3 &&
            uint.TryParse(values[2]?.ToString(), out uint dupeIndex) &&
            dupeIndex != 0)
        {
            title += $" ({dupeIndex})";
        }

        return title;
    }
}