using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Tsundoku.Controls
{
    public class ValueStat : TemplatedControl
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
    }
}
