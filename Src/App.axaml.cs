using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Tsundoku.ViewModels;
using Tsundoku.Views;
using System.Net;
using System.Net.Http.Headers;
using Tsundoku.Helpers;
using Tsundoku.Controls;
using Windows.Storage;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Tsundoku
{
    public partial class App : Application
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static Mutex _mutex;
        public static Mutex Mutex { get => _mutex; set => _mutex = value; }
        public static IServiceProvider? ServiceProvider { get; private set; }

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

            ConfigureNLogToUseLocalCacheFolder();

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
        private static void ConfigureServices(IServiceCollection services)
        {
            // You can configure a specific named HttpClient if needed
            services.AddHttpClient("AddCoverClient", client =>
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku", ViewModelBase.CUR_TSUNDOKU_VERSION));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));
                client.DefaultRequestVersion = HttpVersion.Version20;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                client.Timeout = TimeSpan.FromSeconds(30);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddHttpClient("MangaDexClient", client =>
            {
                client.BaseAddress = new Uri("https://api.mangadex.org/");
                client.DefaultRequestVersion = HttpVersion.Version30;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku", ViewModelBase.CUR_TSUNDOKU_VERSION));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));
                client.Timeout = TimeSpan.FromSeconds(30);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

            // --- AniList GraphQL Client Setup ---
            services.AddHttpClient("AniListHttpClient", client =>
            {
                client.BaseAddress = new Uri("https://graphql.anilist.co");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestVersion = HttpVersion.Version30;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku", ViewModelBase.CUR_TSUNDOKU_VERSION));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));
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
            services.AddTransient<SeriesCardDisplay>();
            // services.AddSingleton<EditSeriesInfoViewModel>();

            services.AddTransient<PopupWindow>();
            services.AddTransient<PopupWindowViewModel>();

            services.AddSingleton<BitmapHelper>();

            services.AddSingleton<MangaDex>();
            services.AddSingleton<AniList>();
            services.AddSingleton<MainWindowViewModel>();
        }

        private static void ConfigureNLogToUseLocalCacheFolder()
        {
            // 1. Get the LocalCacheFolder path
            string localCachePath = AppFileHelper.GetFolderPath("Logs");

            // 2. Grab (or create) your existing NLog configuration
            LoggingConfiguration config = LogManager.Configuration;
            config ??= new LoggingConfiguration();

            // 3. Configure (or create) the file target
            FileTarget fileTarget = config.FindTargetByName<FileTarget>("TsundokuLogs");
            if (fileTarget == null)
            {
                fileTarget = new FileTarget("TsundokuLogs")
                {
                    Layout = "${longdate} | ${level:uppercase=true} | ${message} ${exception:format=ToString,StackTrace}",
                    FileName = Path.Combine(localCachePath, "TsundokuLogs.log"),
                    ArchiveFileName = Path.Combine(localCachePath, "TsundokuLogs.{#}.log"),
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveDateFormat = "yyyy-MM-dd",
                    MaxArchiveFiles = 7,
                    ConcurrentWrites = true,
                    CleanupFileName = true,
                    KeepFileOpen = false,
                    EnableArchiveFileCompression = true,
                    AutoFlush = false,
                    BufferSize = 65536,
                    OpenFileCacheSize = 10,
                    OpenFileCacheTimeout = 30,
                    OpenFileFlushTimeout = 5
                };
            }
            else
            {
                fileTarget.FileName = Path.Combine(localCachePath, "TsundokuLogs.log");
                fileTarget.ArchiveFileName = Path.Combine(localCachePath, "TsundokuLogs.{#}.log");
            }

            AsyncTargetWrapper asyncWrapper = new AsyncTargetWrapper(fileTarget)
            {
                Name = "TsundokuLogsAsync",
                OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
                QueueLimit = 10000,
                TimeToSleepBetweenBatches = 50
            };

            config.AddTarget(asyncWrapper);

        #if DEBUG
            // 4. Configure (or create) the console target (only in DEBUG)
            ColoredConsoleTarget consoleTarget = config.FindTargetByName<ColoredConsoleTarget>("TsundokuConsole");
            if (consoleTarget == null)
            {
                consoleTarget = new ColoredConsoleTarget("TsundokuConsole")
                {
                    Layout = "${longdate} | ${level:uppercase=true:padding=5:fixedLength=true} | ${message} ${exception:format=ToString,StackTrace}"
                };
                config.AddTarget(consoleTarget);
            }
        #endif

            // 5. Clear existing rules for these targets (to avoid duplicates)
            for (int i = config.LoggingRules.Count - 1; i >= 0; i--)
            {
                LoggingRule rule = config.LoggingRules[i];
                if (rule.Targets.Any(t => t == fileTarget
        #if DEBUG
                                            || t == consoleTarget
        #endif
                                            ))
                {
                    config.LoggingRules.RemoveAt(i);
                }
            }

            // 6. Create rule: File = Info+ only
            LoggingRule fileRule = new LoggingRule("*", LogLevel.Info, asyncWrapper);
            config.LoggingRules.Add(fileRule);

        #if DEBUG
            // 7. Create rule: Console = all levels (Debug+), only in DEBUG
            LoggingRule consoleRule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(consoleRule);
        #endif

            // 8. Apply the updated configuration
            LogManager.Configuration = config;
        }
    }
}