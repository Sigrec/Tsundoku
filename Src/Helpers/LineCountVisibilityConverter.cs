using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tsundoku.Helpers
{
    public class LineCountVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint v)
            {
                return v > 3;
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
