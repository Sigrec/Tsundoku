using Avalonia.Controls.Primitives;

namespace Tsundoku.Controls;

public sealed class ValueStat : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<ValueStat, string>(nameof(Text), "VSText Error");

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<ValueStat, string>(nameof(Title), "VSTitle Error");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> IconProperty = AvaloniaProperty.Register<ValueStat, string>(nameof(Title));

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> CopyTextProperty = AvaloniaProperty.Register<ValueStat, string>(nameof(CopyText), "VSCopyText Error");

    public string CopyText
    {
        get => GetValue(CopyTextProperty);
        set => SetValue(CopyTextProperty, value);
    }
}
