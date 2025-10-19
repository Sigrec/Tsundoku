using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;

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
        // Zero-alloc if already a brush
        if (value is ISolidColorBrush solid) return solid;

        if (value is Color color)
        {
            // New immutable brush; safe under theme changes (no stale cache)
            return new ImmutableSolidColorBrush(color);
        }

        return new ImmutableSolidColorBrush(Colors.Transparent);
    }
}