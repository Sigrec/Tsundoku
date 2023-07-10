using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia.FontAwesome;
using System;
using Projektanker.Icons.Avalonia;

namespace Tsundoku
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            IconProvider.Current
                .Register<FontAwesomeIconProvider>();

            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI()
                .With(LoaderOptimization.MultiDomainHost)
                .With(LoadHint.Sometimes);
                // .With(new SkiaOptions 
                // {
                //     MaxGpuResourceSizeBytes = 1024000000
                // })
                // .With(new Win32PlatformOptions
                // {
                //     UseWgl = true,
                //     AllowEglInitialization = true
                // });
        }
    }
}
