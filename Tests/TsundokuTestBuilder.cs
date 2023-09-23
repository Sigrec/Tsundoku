using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Tsundoku.Tests.TsundokuTestBuilder))]
namespace Tsundoku.Tests
{
    public class TsundokuTestBuilder
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<Tsundoku.App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}