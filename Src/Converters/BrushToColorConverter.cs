using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Tsundoku.Converters;

public sealed class BrushToColorConverter : IValueConverter
{
    public static readonly BrushToColorConverter Instance = new BrushToColorConverter();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ISolidColorBrush solid) return solid.Color;
        if (value is Color color) return color;
        return Colors.Transparent; // keep type-correct fallback
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush) return brush;

        if (value is Color color)
        {
            return new SolidColorBrush(color);
        }

        return new SolidColorBrush(Colors.Transparent);
    }
}