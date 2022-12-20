using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tsundoku.Helpers
{
    public class StaffLangConverter : IMultiValueConverter
    {
        public static readonly StaffLangConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            switch (values[2])
            {
                case "Native":
                    return values[1];
                default:
                    return values[0];
            }
        }
    }
}
