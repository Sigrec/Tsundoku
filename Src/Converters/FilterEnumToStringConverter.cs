using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Avalonia.Data.Converters;
using static Tsundoku.Models.Enums.TsundokuFilterEnums;

namespace Tsundoku.Converters;

public sealed class FilterEnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TsundokuFilter filter)
        {
            string? enumMember = typeof(TsundokuFilter)
                .GetField(filter.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>()?.Value;

            return enumMember ?? filter.ToString();
        }
        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            foreach (TsundokuFilter val in Enum.GetValues(typeof(TsundokuFilter)))
            {
                string? enumMember = typeof(TsundokuFilter)
                    .GetField(val.ToString())
                    ?.GetCustomAttribute<EnumMemberAttribute>()?.Value;

                if (enumMember == s || val.ToString() == s)
                    return val;
            }
            return TsundokuFilter.None; // fallback
        }
        return AvaloniaProperty.UnsetValue;
    }
}