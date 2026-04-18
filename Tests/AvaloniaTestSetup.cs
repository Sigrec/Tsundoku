using Avalonia;
using Avalonia.Headless;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome7;
using ReactiveUI.Avalonia;
using Tsundoku.Tests;

[assembly: AvaloniaTestApplication(typeof(AvaloniaTestSetup))]
namespace Tsundoku.Tests;

public class AvaloniaTestSetup
{
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesome7IconProvider>();

        return AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions())
        .UseReactiveUI(_ => { });
    }
}