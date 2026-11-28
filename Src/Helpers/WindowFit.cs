using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Tsundoku.Helpers;

/// <summary>
/// Sizes every window to its content, or to the size it declares, unless that doesn't fit on the
/// screen. Then the window is limited to the screen's work area, less <see cref="ScreenMargin"/> on
/// each side, and its content scrolls. <see cref="Register"/> hooks this up for every window that
/// opens, so new windows get it without any code of their own.
/// </summary>
public static class WindowFit
{
    public const double ScreenMargin = 24;

    private static readonly ConditionalWeakTable<Window, WindowFitState> States = [];

    /// <summary>Fits every window that opens, except those <paramref name="exclude"/> returns true for.</summary>
    public static void Register(Func<Window, bool> exclude)
    {
        Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
        {
            if (!exclude(window))
            {
                Refit(window);
            }
        }, RoutingStrategies.Direct);
    }

    /// <summary>
    /// Lets <paramref name="window"/> be resized smaller than its content, by up to what
    /// <paramref name="shrinkableHeight"/> returns, instead of always matching its content height.
    /// </summary>
    public static void SetShrinkableHeight(Window window, Func<double> shrinkableHeight) =>
        GetState(window).ShrinkableHeight = shrinkableHeight;

    /// <summary>Sizes the window to its content again, then applies the screen limits. Call it when the content changes size.</summary>
    public static void Refit(Window window)
    {
        GetState(window).Restore(window);
        Dispatcher.UIThread.Post(() => FitToScreen(window), DispatcherPriority.Loaded);
    }

    private static WindowFitState GetState(Window window) => States.GetValue(window, static w => new WindowFitState(w));

    private static void FitToScreen(Window window)
    {
        if (!window.IsVisible) return;

        Screen? screen = window.Screens.ScreenFromWindow(window) ?? window.Screens.Primary;
        if (screen is null) return;

        double scaling = screen.Scaling;
        PixelRect workArea = screen.WorkingArea;
        double availableWidth = Math.Max(0, (workArea.Width / scaling) - (ScreenMargin * 2));
        double availableHeight = Math.Max(0, (workArea.Height / scaling) - (ScreenMargin * 2));

        WindowFitState state = GetState(window);
        double naturalWidth = window.Bounds.Width;
        double naturalHeight = window.Bounds.Height;
        double shrinkable = Math.Max(0, state.ShrinkableHeight?.Invoke() ?? 0);
        bool clamped = false;

        if (shrinkable > 0)
        {
            double minimumHeight = naturalHeight - shrinkable;
            window.SizeToContent = WithoutHeight(window.SizeToContent);
            window.MaxHeight = Math.Max(availableHeight, minimumHeight);
            window.MinHeight = Math.Min(minimumHeight, window.MaxHeight);
            window.Height = Math.Clamp(naturalHeight, window.MinHeight, window.MaxHeight);
            clamped = naturalHeight > window.MaxHeight;
        }
        else
        {
            window.MaxHeight = Math.Min(state.MaxHeight, availableHeight);
            window.MinHeight = Math.Min(state.MinHeight, window.MaxHeight);
            if (naturalHeight > window.MaxHeight)
            {
                window.SizeToContent = WithoutHeight(window.SizeToContent);
                window.Height = window.MaxHeight;
                clamped = true;
            }
        }

        window.MaxWidth = Math.Min(state.MaxWidth, availableWidth);
        window.MinWidth = Math.Min(state.MinWidth, window.MaxWidth);
        if (naturalWidth > window.MaxWidth)
        {
            window.SizeToContent = WithoutWidth(window.SizeToContent);
            window.Width = window.MaxWidth;
            clamped = true;
        }

        Dispatcher.UIThread.Post(() => KeepOnScreen(window, workArea, scaling, center: clamped), DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Moves the window inside the work area with a <see cref="ScreenMargin"/> gap. A window that had to
    /// be shrunk is centered instead, since its startup position was worked out for its larger size.
    /// </summary>
    private static void KeepOnScreen(Window window, PixelRect workArea, double scaling, bool center)
    {
        int margin = (int)(ScreenMargin * scaling);
        int width = (int)(window.Bounds.Width * scaling);
        int height = (int)(window.Bounds.Height * scaling);

        int minX = workArea.X + margin;
        int minY = workArea.Y + margin;
        int maxX = Math.Max(minX, workArea.Right - margin - width);
        int maxY = Math.Max(minY, workArea.Bottom - margin - height);

        PixelPoint position = center
            ? new PixelPoint(workArea.X + ((workArea.Width - width) / 2), workArea.Y + ((workArea.Height - height) / 2))
            : window.Position;

        window.Position = new PixelPoint(Math.Clamp(position.X, minX, maxX), Math.Clamp(position.Y, minY, maxY));
    }

    private static SizeToContent WithoutHeight(SizeToContent value) => value switch
    {
        SizeToContent.WidthAndHeight => SizeToContent.Width,
        SizeToContent.Height => SizeToContent.Manual,
        _ => value,
    };

    private static SizeToContent WithoutWidth(SizeToContent value) => value switch
    {
        SizeToContent.WidthAndHeight => SizeToContent.Height,
        SizeToContent.Width => SizeToContent.Manual,
        _ => value,
    };
}
