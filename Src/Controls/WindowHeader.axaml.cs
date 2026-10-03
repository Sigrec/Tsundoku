using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace Tsundoku.Controls;

public sealed class WindowHeader : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<WindowHeader, string>(nameof(Title), string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<WindowHeader, string>(nameof(Icon), string.Empty);

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<WindowHeader, object?>(nameof(RightContent));

    [Content]
    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }
}
