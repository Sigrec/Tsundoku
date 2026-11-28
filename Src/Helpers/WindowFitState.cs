using Avalonia.Controls;

namespace Tsundoku.Helpers;

/// <summary>
/// A window's size limits and <see cref="SizeToContent"/> as declared in its XAML, captured the first time
/// <see cref="WindowFit"/> sees it so each fit starts from the declared values rather than the
/// screen-clamped ones from a previous fit.
/// </summary>
internal sealed class WindowFitState(Window window)
{
    public SizeToContent SizeToContent { get; } = window.SizeToContent;
    public double MinWidth { get; } = window.MinWidth;
    public double MaxWidth { get; } = window.MaxWidth;
    public double MinHeight { get; } = window.MinHeight;
    public double MaxHeight { get; } = window.MaxHeight;

    /// <summary>
    /// How far the window's content can shrink below its natural height, for windows that size to
    /// their content but can also be resized smaller. Zero for every other window.
    /// </summary>
    public Func<double>? ShrinkableHeight { get; set; }

    public void Restore(Window target)
    {
        target.MinWidth = MinWidth;
        target.MaxWidth = MaxWidth;
        target.MinHeight = MinHeight;
        target.MaxHeight = MaxHeight;
        target.SizeToContent = SizeToContent;
    }
}
