using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Tsundoku.Helpers
{
    public class HsvColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint v)
            {
                return new HsvColor(Color.FromUInt32(v));
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
