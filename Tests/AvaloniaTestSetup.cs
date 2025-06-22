using Avalonia;
using Avalonia.Headless;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Tsundoku.Tests;

[assembly: AvaloniaTestApplication(typeof(AvaloniaTestSetup))]
namespace Tsundoku.Tests;

public class AvaloniaTestSetup
{
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>();

        return AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}