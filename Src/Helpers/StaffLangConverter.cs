using Avalonia.Data.Converters;
using System.Globalization;

namespace Tsundoku.Helpers
{
    public class StaffLangConverter : IMultiValueConverter
    {
        public static readonly StaffLangConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var staff = values[0] as Dictionary<string, string>;
            if (staff == null)
            {
                return "ERROR";
            }
            else if (staff.ContainsKey(values[1] as string))
            {
                return staff[values[1] as string];
            }
            return staff["Romaji"];
        }
    }
}
