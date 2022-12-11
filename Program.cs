using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System;

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
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .WithIcons(container => container
                    .Register<FontAwesomeIconProvider>())
                .UseReactiveUI()
                .With(new SkiaOptions 
                {
                    MaxGpuResourceSizeBytes = 1024000000
                })
                .With(new Win32PlatformOptions
                {
                    UseCompositor = false,
                    UseWgl = true,
                    AllowEglInitialization = true
                });
    }
}
