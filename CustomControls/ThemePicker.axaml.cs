using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Tsundoku.CustomControls
{
    public class ThemePicker : TemplatedControl
    {
        public static readonly StyledProperty<string> ColorCategoryProperty = AvaloniaProperty.Register<ThemePicker, string>(nameof(ColorCategory), "Error");

        public string ColorCategory
        {
            get => GetValue(ColorCategoryProperty);
            set => SetValue(ColorCategoryProperty, value);
        }
    }
}
