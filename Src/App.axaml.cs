using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Net;
using System.Net.Http.Headers;
using Tsundoku.Clients;
using System.Reactive.Linq;
using Tsundoku.Helpers;
using Tsundoku.Models;
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

            // Pre-load theme before splash so it uses correct colors.
            // FileStream + JsonDocument.Parse(stream) skips the intermediate UTF-16 string allocation.
            try
            {
                string userDataPath = AppFileHelper.GetFilePath("UserData.json");
                if (File.Exists(userDataPath))
                {
                    using FileStream stream = new(userDataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, options: FileOptions.SequentialScan);
                    using JsonDocument doc = JsonDocument.Parse(stream);
                    if (doc.RootElement.TryGetProperty("MainTheme", out JsonElement mainThemeEl))
                    {
                        string? mainTheme = mainThemeEl.GetString();
                        if (mainTheme is not null && doc.RootElement.TryGetProperty("SavedThemes", out JsonElement themesArray))
                        {
                            foreach (JsonElement themeEl in themesArray.EnumerateArray())
                            {
                                if (themeEl.TryGetProperty("ThemeName", out JsonElement nameEl) && nameEl.GetString() == mainTheme)
                                {
                                    TsundokuTheme? theme = JsonSerializer.Deserialize<TsundokuTheme>(themeEl.GetRawText());
                                    if (theme is not null)
                                    {
                                        ThemeResourceService svc = new();
                                        svc.ApplyTheme(theme);
                                        LOGGER.Info("Pre-loaded theme '{Theme}' for splash screen", mainTheme);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOGGER.Warn(ex, "Could not pre-load theme for splash screen, using defaults");
            }

            // Show splash screen
            SplashWindow splash = new();
            desktop.MainWindow = splash;

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

            // Load everything after the splash is visible
            splash.Opened += async (_, _) =>
            {
                try
                {
                    // Let the splash window fully render before starting heavy work
                    await Task.Delay(500);

                    splash.UpdateStatus("Initializing services");
                    ServiceCollection services = new();
                    ConfigureServices(services);
                    ServiceProvider = services.BuildServiceProvider();

                    splash.UpdateStatus("Loading user data");
                    IUserService userService = ServiceProvider.GetRequiredService<IUserService>();

                    // Discord connection and user-data load are independent — run them concurrently.
                    Task discordInitTask = Task.Run(DiscordRP.Initialize);
                    Task userDataTask = userService.LoadUserDataAsync();

                    try
                    {
                        await userDataTask;
                    }
                    catch (Exception ex)
                    {
                        LOGGER.Fatal(ex, "FATAL ERROR: Failed to load essential user data on startup. Shutting down.");
                        desktop.Shutdown(1);
                        return;
                    }

                    // Discord is non-critical — don't fail startup if it errors.
                    _ = discordInitTask.ContinueWith(
                        t => LOGGER.Warn(t.Exception, "Discord RPC initialization failed"),
                        TaskContinuationOptions.OnlyOnFaulted);

                    splash.UpdateStatus("Applying theme");
                    IThemeResourceService themeResourceService = ServiceProvider.GetRequiredService<IThemeResourceService>();
                    TsundokuTheme? initialTheme = userService.GetCurrentThemeSnapshot();
                    if (initialTheme is not null)
                    {
                        themeResourceService.ApplyTheme(initialTheme);
                    }

                    IDisposable themeSubscription = userService.CurrentTheme
                        .Where(t => t is not null)
                        .Subscribe(theme => themeResourceService.ApplyTheme(theme!));

                    desktop.Exit += (_, _) => themeSubscription.Dispose();

                    // Apply glassmorphism if user has it enabled
                    if (userService.GetCurrentUserSnapshot()?.GlassmorphismEnabled == true)
                    {
                        GlassmorphismService.Apply(true);
                    }

                    splash.UpdateStatus("Building UI");
                    MainWindowViewModel mainViewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
                    MainWindow mainWindow = new(
                        mainViewModel,
                        userService,
                        ServiceProvider.GetRequiredService<IApiHealthCheckService>(),
                        ServiceProvider.GetRequiredService<IPopupDialogService>(),
                        ServiceProvider.GetRequiredService<AddNewSeriesWindow>(),
                        ServiceProvider.GetRequiredService<UserSettingsWindow>(),
                        ServiceProvider.GetRequiredService<CollectionThemeWindow>(),
                        ServiceProvider.GetRequiredService<PriceAnalysisWindow>(),
                        ServiceProvider.GetRequiredService<CollectionStatsWindow>(),
                        ServiceProvider.GetRequiredService<UserNotesWindow>());

                    mainWindow.Opened += async (_, _) =>
                    {
                        if (mainViewModel.ShouldShowChangelog())
                        {
                            await Task.Delay(500);
                            mainWindow.Activate();

                            ChangelogWindow changelog = new() { DataContext = mainViewModel };
                            changelog.SetVersion(ViewModelBase.CUR_TSUNDOKU_VERSION);
                            await changelog.ShowDialog(mainWindow);
                            mainViewModel.MarkChangelogSeen();
                        }
                    };

                    // Swap splash for main window
                    desktop.MainWindow = mainWindow;
                    GlassmorphismService.ApplyToWindow(mainWindow, GlassmorphismService.IsEnabled);
                    mainWindow.Show();
                    mainWindow.Activate();
                    mainWindow.Focus();
                    splash.Close();
                }
                catch (Exception ex)
                {
                    LOGGER.Fatal(ex, "Failed during startup");
                    desktop.Shutdown(1);
                }
            };
            splash.Show();
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

        services.AddHttpClient("ColorApiClient", client =>
        {
            client.BaseAddress = new Uri("https://www.thecolorapi.com/");
            client.Timeout = TimeSpan.FromSeconds(15);
        }).SetHandlerLifetime(TimeSpan.FromMinutes(10));

        services.AddSingleton<ColorApi>();

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
            client.DefaultRequestVersion = HttpVersion.Version20;
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
        services.AddSingleton<IApiHealthCheckService, ApiHealthCheckService>();

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

        services.AddSingleton<IThemeResourceService, ThemeResourceService>();
    }

    public static void ConfigureNLog()
    {
        string localCachePath = AppFileHelper.GetFolderPath("Logs");

        // Grab (or create) existing NLog configuration
        LoggingConfiguration config = LogManager.Configuration ?? new LoggingConfiguration();

        // Configure (or create) the file target
        FileTarget fileTarget = config.FindTargetByName<FileTarget>("TsundokuLogs");
        if (fileTarget is null)
        {
            fileTarget = new FileTarget("TsundokuLogs")
            {
                Layout = "${longdate:universalTime=true} [${uppercase:${level:format=TriLetter}}] (${logger}) - ${message} ${exception:format=ToString,StackTrace}",
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

        var asyncWrapper = new AsyncTargetWrapper(fileTarget)
        {
            Name = "TsundokuLogsAsync",
            OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
            QueueLimit = 10000,
            TimeToSleepBetweenBatches = 50
        };
        config.AddTarget(asyncWrapper);

    #if DEBUG
        // Configure (or create) the console target (only in DEBUG)
        ColoredConsoleTarget consoleTarget = config.FindTargetByName<ColoredConsoleTarget>("TsundokuConsole");
        if (consoleTarget is null)
        {
            consoleTarget = new ColoredConsoleTarget("TsundokuConsole")
            {
                Layout = "${longdate:universalTime=true} [${uppercase:${level:format=TriLetter}}] (${logger}) - ${message} ${exception:format=ToString,StackTrace}"
            };
            config.AddTarget(consoleTarget);
        }
    #endif

        // Clear existing rules for these targets (avoid duplicates)
        for (int i = config.LoggingRules.Count - 1; i >= 0; i--)
        {
            var rule = config.LoggingRules[i];
            if (rule.Targets.Any(t =>
                t == fileTarget
                || t == asyncWrapper
    #if DEBUG
                || t == consoleTarget
    #endif
            ))
            {
                config.LoggingRules.RemoveAt(i);
            }
        }

        // --- Namespace-specific filtering for MangaAndLightNovelWebScrape.* ---

        // Null target to swallow low levels
        NLog.Targets.NullTarget? blackHole = config.FindTargetByName<NLog.Targets.NullTarget>("BlackHole");
        if (blackHole is null)
        {
            blackHole = new NLog.Targets.NullTarget("BlackHole");
            config.AddTarget(blackHole);
        }

        // 1) Drop Trace..Info for that namespace, and stop processing
        var dropLowLevels = new LoggingRule("MangaAndLightNovelWebScrape.*", LogLevel.Trace, LogLevel.Info, blackHole)
        {
            Final = true
        };
        config.LoggingRules.Add(dropLowLevels);

        // 2) Allow Warn..Fatal for that namespace, and stop processing (so it won't hit catch-alls)
        var allowWarnPlus = new LoggingRule("MangaAndLightNovelWebScrape.*", LogLevel.Warn, LogLevel.Fatal, asyncWrapper)
        {
            Final = true
        };
    #if DEBUG
        allowWarnPlus.Targets.Add(consoleTarget);
    #endif
        config.LoggingRules.Add(allowWarnPlus);

        // --- Default catch-alls ---
        config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, LogLevel.Fatal, asyncWrapper));
    #if DEBUG
        config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, LogLevel.Fatal, consoleTarget));
    #endif

        LogManager.Configuration = config;
        LogManager.ReconfigExistingLoggers();
    }
}