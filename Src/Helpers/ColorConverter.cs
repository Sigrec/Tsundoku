using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tsundoku.Helpers
{
    public class ColorConverter : IValueConverter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UInt32)
            {
                return new Avalonia.Media.SolidColorBrush((uint)value);
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
