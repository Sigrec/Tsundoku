using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Src
{
    public class TitleLangConverter : IMultiValueConverter
    {
        public static readonly TitleLangConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            switch (values[3])
            {
                case "Native":
                    return values[2];
                case "English":
                    return values[1];
                default:
                    return values[0];
            }
        }
    }
}
