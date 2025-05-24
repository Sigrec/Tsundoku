using Avalonia.Data.Converters;
using System.Globalization;

namespace Tsundoku.Helpers
{
    public class TitleLangConverter : IMultiValueConverter
    {
        public static readonly TitleLangConverter Instance = new();

        public object? Convert(IList<object?> values, Type type, object? parameter, CultureInfo culture)
        {
            Dictionary<string, string>? titles = values[0] as Dictionary<string, string>;
            if (titles == null)
            {
                return "ERROR";
            }

            string lang = values[1].ToString();
            string title = titles.TryGetValue(lang, out string? value) ? value : titles["Romaji"];

            if (values.Count == 3)
            {
                uint dupeIndex = uint.Parse(values[2].ToString());
                if (dupeIndex != 0)
                {
                    title += $" ({dupeIndex})";
                }
            }
            return title;
        }
    }
}
