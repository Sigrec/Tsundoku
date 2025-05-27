using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Tsundoku.ViewModels;
using Tsundoku.Views;
using System.Net;
using System.Net.Http.Headers;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Avalonia.Media;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using Tsundoku.Services;

namespace Tsundoku
{
    public partial class App : Application
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static Mutex _mutex;
        public static Mutex Mutex { get => _mutex; set => _mutex = value; }
        public static IServiceProvider? ServiceProvider { get; private set; }
        private readonly CompositeDisposable _disposables = new CompositeDisposable(); // Add this line

        public App()
        {
            // This constructor is required by Avalonia's AppBuilder.Configure<App>().
            // Do NOT put any DI logic here, as the ServiceProvider is not fully ready yet.
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // BindingPlugins.DataValidators.RemoveAt(0);
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection); // Configure your services

            const string appName = "Tsundoku";
            _mutex = new Mutex(true, appName, out bool createdNew);

            // Declare 'desktop' ONCE here and check it.
            // If it's not a desktop application lifetime, handle it early.
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                LOGGER.Error("Application is not running in a classic desktop environment.");
                base.OnFrameworkInitializationCompleted();
                Environment.Exit(1);
                return; // Exit if not desktop mode
            }

            if (!createdNew)
            {
                LOGGER.Info("Application already running. Shutting down new instance.");
                desktop.TryShutdown(); // Use the 'desktop' variable declared above
            }
            else
            {
                LOGGER.Info("Application starting up.");
                DiscordRP.Initialize();
                ServiceProvider = serviceCollection.BuildServiceProvider();

                IUserService userService = ServiceProvider.GetRequiredService<IUserService>();

                try
                {
                    userService.LoadUserData();
                }
                catch (Exception ex)
                {
                    LOGGER.Fatal(ex, "FATAL ERROR: Failed to load essential user data on startup. Shutting down.");
                    desktop.Shutdown(1); // Shutdown on critical failure
                    return;
                }

                // Get the MainWindowViewModel from the DI container
                desktop.MainWindow = new MainWindow(ServiceProvider.GetRequiredService<MainWindowViewModel>());
            }
            base.OnFrameworkInitializationCompleted();
        }

        public static void DisposeMutex()
        {
            _mutex?.Dispose();
        }

        // Configure your application's services for Dependency Injection
        // TODO - If I change something about a series while filtering the filter doesn't reset basically after
        private static void ConfigureServices(IServiceCollection services)
        {
            // You can configure a specific named HttpClient if needed
            services.AddHttpClient("AddCoverClient", client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Tsundoku/1.0");
                client.DefaultRequestVersion = HttpVersion.Version20;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                client.Timeout = TimeSpan.FromSeconds(30);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddHttpClient("MangaDexClient", client =>
            {
                client.BaseAddress = new Uri("https://api.mangadex.org/");
                client.DefaultRequestVersion = HttpVersion.Version30;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                client.DefaultRequestHeaders.Add("User-Agent", "Tsundoku/1.0");
                client.Timeout = TimeSpan.FromSeconds(30);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

            // --- AniList GraphQL Client Setup ---
            services.AddHttpClient("AniListHttpClient", client =>
            {
                client.BaseAddress = new Uri("https://graphql.anilist.co");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestVersion = HttpVersion.Version30;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
                client.DefaultRequestHeaders.Add("User-Agent", "Tsundoku/1.0");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("RequestType", "POST");
                client.DefaultRequestHeaders.Add("ContentType", "application/json");
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5))
              .AddTypedClient<AniListGraphQLClient>();

            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ISharedSeriesCollectionProvider, SharedSeriesCollectionProvider>();

            services.AddSingleton<AddNewSeriesWindow>();
            services.AddSingleton<AddNewSeriesViewModel>();

            services.AddSingleton<SettingsWindow>();
            services.AddSingleton<UserSettingsViewModel>();

            services.AddSingleton<CollectionThemeWindow>();
            services.AddSingleton<ThemeSettingsViewModel>();

            services.AddSingleton<PriceAnalysisWindow>();
            services.AddSingleton<PriceAnalysisViewModel>();

            services.AddSingleton<CollectionStatsWindow>();
            services.AddSingleton<CollectionStatsViewModel>();

            services.AddSingleton<UserNotesWindow>();
            services.AddSingleton<UserNotesWindowViewModel>();

            services.AddTransient<EditSeriesInfoWindow>();
            // services.AddSingleton<EditSeriesInfoViewModel>();

            services.AddTransient<PopupWindow>();
            services.AddTransient<PopupWindowViewModel>();

            services.AddSingleton<BitmapHelper>();

            services.AddSingleton<MangaDex>();
            services.AddSingleton<AniList>();
            services.AddSingleton<MainWindowViewModel>();
        }
    }
}