using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Tsundoku.Src
{
    public class ColorConverter : IValueConverter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UInt32)
            {
                if (parameter != null && parameter.Equals("Hex"))
                {
                    return "0 5 10 1 " + Avalonia.Media.Color.FromUInt32((uint)value).ToString();
                }
                else
                {
                    return new Avalonia.Media.SolidColorBrush((uint)value);
                }
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
