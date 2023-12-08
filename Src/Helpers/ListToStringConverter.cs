using System.Globalization;
using Avalonia.Data.Converters;

namespace Tsundoku.Helpers
{
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<string>)
            {
                StringBuilder listAsString = new StringBuilder();
                foreach (string filter in value as List<string>)
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
