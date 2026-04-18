using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome7;
using ReactiveUI.Avalonia;
using static Tsundoku.Models.Constants;

namespace Tsundoku;

internal sealed class Program
{
    /// <summary>
    /// Estimated number of cover textures to keep in the GPU cache
    /// (visible cards + virtualization buffer above and below the viewport).
    /// </summary>
    private const int EstimatedCachedCovers = 200;

    /// <summary>Bytes per pixel for RGBA textures.</summary>
    private const int BytesPerPixel = 4;

    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesome7IconProvider>();

        // Calculate GPU cache size from actual cover dimensions
        long coverTextureBytes = (long)(LEFT_SIDE_CARD_WIDTH * BITMAP_SCALE)
                               * (IMAGE_HEIGHT * BITMAP_SCALE)
                               * BytesPerPixel;
        long gpuCacheBytes = coverTextureBytes * EstimatedCachedCovers;

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new SkiaOptions
            {
                MaxGpuResourceSizeBytes = gpuCacheBytes
            })
            .With(new CompositionOptions
            {
                UseRegionDirtyRectClipping = true
            })
#if DEBUG
            .LogToTrace()
#endif
            .UseReactiveUI(_ => { });
    }
}
