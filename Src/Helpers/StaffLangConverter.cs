using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tsundoku.Helpers
{
    public class StaffLangConverter : IMultiValueConverter
    {
        public static readonly StaffLangConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Any(x => x is Avalonia.UnsetValueType)) return false;
            var staff = (values[0] as Dictionary<string, string>);
            if (staff.ContainsKey((values[1] as string)))
            {
                return staff[(values[1] as string)];
            }
            return staff["Romaji"];
        }
    }
}
