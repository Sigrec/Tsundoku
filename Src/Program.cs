using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System.Runtime.CompilerServices;

namespace Tsundoku
{
    internal class Program
    {
        // Add a static property to hold the ServiceProvider for dependency injection

        // This is your main entry point
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);

            // Optional: When the app shuts down, dispose of the service provider
            //(ServiceProvider as IDisposable)?.Dispose();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            IconProvider.Current
                .Register<FontAwesomeIconProvider>();

            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace() // This often logs basic Avalonia events, can be useful
                .UseReactiveUI()
                .With(LoaderOptimization.MultiDomainHost) // Using LoaderOptimization with explicit type
                .With(LoadHint.Always);
        }
    }
}
