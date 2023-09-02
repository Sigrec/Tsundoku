using System.Threading;
using Avalonia;
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
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!createdNew)
                {
                    desktop.Shutdown();
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
    }
}
