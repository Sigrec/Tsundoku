using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Tsundoku
{
    public partial class App : Application
    {
        private static Mutex _mutex;
        public static Mutex Mutex { get => _mutex; set => _mutex = value; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            const string appName = "Tsundoku";
            _mutex = new Mutex(true, appName, out bool createdNew);
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!createdNew)
                {
                    desktop.TryShutdown();
                }
                else
                {
                    Helpers.DiscordRP.Initialize();
                    desktop.MainWindow = new Views.MainWindow
                    {
                        DataContext = new ViewModels.MainWindowViewModel(),
                    };
                }
            }
            base.OnFrameworkInitializationCompleted();
        }

        public static void DisposeMutex()
        {
            _mutex.Dispose();
        }
    }
}
