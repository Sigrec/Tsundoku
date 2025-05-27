using Avalonia.Data.Converters;
using System.Globalization;
using static Tsundoku.Models.TsundokuLanguageModel;

namespace Tsundoku.Converters
{
    public class TitleLangConverter : IMultiValueConverter
    {
        public static readonly TitleLangConverter Instance = new();

        public object? Convert(IList<object?> values, Type type, object? parameter, CultureInfo culture)
        {
            // Input validation and initial setup
            if (values[0] is not Dictionary<TsundokuLanguage, string> titles)
            {
                return "ERROR";
            }

            string langStringValue = values[1].ToString();
            
            // Determine the language to use for title lookup.
            // Prioritize the language from the string value, falling back to Romaji.
            TsundokuLanguage effectiveLanguage = TsundokuLanguage.Romaji; // Default fallback

            if (TsundokuLanguageStringValueToLanguageMap.TryGetValue(langStringValue, out TsundokuLanguage mappedLanguage) &&
                titles.ContainsKey(mappedLanguage))
            {
                effectiveLanguage = mappedLanguage;
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
}