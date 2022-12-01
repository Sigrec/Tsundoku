using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tsundoku.Src
{
    public class ColorConverter : IValueConverter
    {
        public static ColorConverter Instance = new ColorConverter();

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
