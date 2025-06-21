using Avalonia;
using Avalonia.Headless;
using Tsundoku.Tests;

[assembly: AvaloniaTestApplication(typeof(AvaloniaTestSetup))]
namespace Tsundoku.Tests;

public class AvaloniaTestSetup
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}