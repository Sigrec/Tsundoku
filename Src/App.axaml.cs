using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Net;
using System.Net.Http.Headers;
using Tsundoku.Clients;
using Tsundoku.Helpers;
using Tsundoku.ViewModels;
using Tsundoku.Views;

namespace Tsundoku;

public sealed partial class App : Application
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public static Mutex? Mutex { get; set; }
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
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted(); // Still required
            return;
        }

        const string appName = "Tsundoku";
        Mutex = new Mutex(true, appName, out bool createdNew);

        ConfigureNLog();

        if (!createdNew)
        {
            LOGGER.Info("Application already running. Shutting down new instance.");
            desktop.TryShutdown(); // Use the 'desktop' variable declared above
        }
        else
        {
            LOGGER.Info("Application starting up.");

            ServiceCollection services = new();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            DiscordRP.Initialize();

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

            desktop.Exit += (_, _) =>
            {
                LOGGER.Info("Disposing Application...");
                DiscordRP.Deinitialize();

                if (ServiceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                    ServiceProvider = null;
                }

                LogManager.Shutdown();

                Mutex?.Dispose();
                Mutex = null;
            };

            // Get the MainWindowViewModel from the DI container
            desktop.MainWindow = new MainWindow(ServiceProvider.GetRequiredService<MainWindowViewModel>());
        }
        base.OnFrameworkInitializationCompleted();
    }

    // Configure your application's services for Dependency Injection
    public static void ConfigureServices(IServiceCollection services)
    {
        // You can configure a specific named HttpClient if needed
        services.AddHttpClient("AddCoverClient", client =>
        {
            client.DefaultRequestVersion = HttpVersion.Version30;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku", ViewModelBase.CUR_TSUNDOKU_VERSION));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));

            client.Timeout = TimeSpan.FromSeconds(60);
        }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddHttpClient("MangaDexClient", client =>
        {
            client.BaseAddress = new Uri("https://api.mangadex.org/");
            client.DefaultRequestVersion = HttpVersion.Version30;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku", ViewModelBase.CUR_TSUNDOKU_VERSION));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));

            client.Timeout = TimeSpan.FromSeconds(90);
        }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // --- AniList GraphQL Client Setup ---
        services.AddHttpClient("AniListHttpClient", client =>
        {
            client.BaseAddress = new Uri("https://graphql.anilist.co");
            client.Timeout = TimeSpan.FromMinutes(2);
            client.DefaultRequestVersion = HttpVersion.Version30;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku", ViewModelBase.CUR_TSUNDOKU_VERSION));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddTypedClient<AniListGraphQLClient>();

        services.AddSingleton<BitmapHelper>();
        services.AddSingleton<MangaDex>();
        services.AddSingleton<AniList>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<ISharedSeriesCollectionProvider, SharedSeriesCollectionProvider>();

        services.AddTransient<LoadingDialogViewModel>();
        services.AddTransient<ILoadingDialogService, LoadingDialogService>();

        services.AddSingleton<AddNewSeriesWindow>();
        services.AddSingleton<AddNewSeriesViewModel>();

        services.AddSingleton<UserSettingsWindow>();
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

        services.AddTransient<PopupDialogViewModel>();
        services.AddTransient<IPopupDialogService, PopupDialogService>();
    }

    public static void ConfigureNLog()
    {
        // 1. Get the LocalCacheFolder path
        string localCachePath = AppFileHelper.GetFolderPath("Logs");

        // 2. Grab (or create) your existing NLog configuration
        LoggingConfiguration config = LogManager.Configuration;
        config ??= new LoggingConfiguration();

        // 3. Configure (or create) the file target
        FileTarget fileTarget = config.FindTargetByName<FileTarget>("TsundokuLogs");
        if (fileTarget is null)
        {
            fileTarget = new FileTarget("TsundokuLogs")
            {
                Layout = "[${longdate} | ${level:uppercase=true}] (${logger}) ${message} ${exception:format=ToString,StackTrace}",
                FileName = Path.Combine(localCachePath, "TsundokuLogs.log"),
                ArchiveFileName = Path.Combine(localCachePath, "TsundokuLogs.{#}.log"),
                ArchiveSuffixFormat = "_{1:dd_MM_yyyy}_{0:00}",
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 7,
                KeepFileOpen = false,
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
        if (consoleTarget is null)
        {
            consoleTarget = new ColoredConsoleTarget("TsundokuConsole")
            {
                Layout = "[${longdate} | ${level:uppercase=true}] (${logger}) ${message} ${exception:format=ToString,StackTrace}"
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