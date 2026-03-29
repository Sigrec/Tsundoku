using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Tsundoku.Controls;

public sealed partial class ColorPickerItem : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ColorPickerItem, string>(nameof(Title), string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<SolidColorBrush> ColorBrushProperty =
        AvaloniaProperty.Register<ColorPickerItem, SolidColorBrush>(nameof(ColorBrush), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public SolidColorBrush ColorBrush
    {
        get => GetValue(ColorBrushProperty);
        set => SetValue(ColorBrushProperty, value);
    }

    public ColorPickerItem()
    {
        InitializeComponent();
    }
}
