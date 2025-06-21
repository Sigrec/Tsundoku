using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Tsundoku.Converters;

public sealed class BrushToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
            return brush.Color;
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color)
            return new SolidColorBrush(color);
        return new SolidColorBrush(Colors.Transparent);
    }
}