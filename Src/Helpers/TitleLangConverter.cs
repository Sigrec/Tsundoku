using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tsundoku.Helpers
{
    public class TitleLangConverter : IMultiValueConverter
    {
        public static readonly TitleLangConverter Instance = new();

        public object? Convert(IList<object?> values, Type type, object? parameter, CultureInfo culture)
        {
            var titles = values[0] as Dictionary<string, string>;
            if (titles.ContainsKey(values[1] as string))
            {
                return titles[values[1] as string];
            }
            return titles["Romaji"];
        }
    }
}
