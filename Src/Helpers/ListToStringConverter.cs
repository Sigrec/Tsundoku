using System.Globalization;
using Avalonia.Data.Converters;

namespace Tsundoku.Helpers
{
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<string> filters)
            {
                StringBuilder listAsString = new StringBuilder();
                foreach (string filter in filters)
                {
                    listAsString.AppendLine(filter);
                }
                return listAsString.ToString().Trim();
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
