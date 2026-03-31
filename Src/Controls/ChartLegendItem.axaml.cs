using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Tsundoku.Controls;

public sealed class ChartLegendItem : TemplatedControl
{
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<ChartLegendItem, string>(nameof(Label), string.Empty);

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly StyledProperty<decimal> PercentageProperty =
        AvaloniaProperty.Register<ChartLegendItem, decimal>(nameof(Percentage));

    public decimal Percentage
    {
        get => GetValue(PercentageProperty);
        set => SetValue(PercentageProperty, value);
    }

    public static readonly StyledProperty<IBrush> ColorProperty =
        AvaloniaProperty.Register<ChartLegendItem, IBrush>(nameof(Color));

    public IBrush Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
}
