using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Avalonia.ReactiveUI;
using System.Runtime.CompilerServices;

namespace Tsundoku;

internal sealed class Program
{
    // This is your main entry point
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .With(LoaderOptimization.MultiDomainHost)
            .With(LoadHint.Always)
#if DEBUG
            .LogToTrace()
#endif
            .UseReactiveUI();
    }
}
