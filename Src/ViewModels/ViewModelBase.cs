using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reflection;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tsundoku.Models;

namespace Tsundoku.ViewModels;

/// <summary>
/// Base class for all view models, providing shared user and theme state via reactive subscriptions.
/// </summary>
public partial class ViewModelBase : ReactiveObject
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    /// <summary>Gets or sets the current collection filter text.</summary>
    public static string Filter { get; set; }

    /// <summary>Indicates whether the collection is currently reloading.</summary>
    public bool isReloading = false;

    /// <summary>The current application version string derived from the assembly informational version.</summary>
    public static readonly string CUR_TSUNDOKU_VERSION = Assembly.GetEntryAssembly()!
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
        .InformationalVersion
        .Split('+')[0];

    /// <summary>The current user data schema version for migration logic.</summary>
    public const double SCHEMA_VERSION = 6.3;

    /// <summary>The file name used for persisting user data.</summary>
    public const string USER_DATA_FILEPATH = @"UserData.json";

    [Reactive] public partial TsundokuTheme CurrentTheme { get; protected set; }
    [Reactive] public partial User CurrentUser { get; protected set; }

    /// <summary>The user service providing access to user data and collection management.</summary>
    protected readonly IUserService _userService;

    /// <summary>Composite disposable for managing reactive subscription lifetimes.</summary>
    protected readonly CompositeDisposable _disposables = [];

    /// <summary>
    /// Initializes the view model with the specified user service and subscribes to user and theme changes.
    /// </summary>
    /// <param name="userService">The user service to bind to.</param>
    protected ViewModelBase(IUserService userService)
    {
        _userService = userService;
        _userService.CurrentTheme
            .Where(theme => theme is not null)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(theme => CurrentTheme = theme!)
            .DisposeWith(_disposables);

        _userService.CurrentUser
            .Where(user => user is not null) // Filters out the initial null from BehaviorSubject
            .Subscribe(user => CurrentUser = user)
            .DisposeWith(_disposables);
    }

    /// <summary>
    /// Opens the specified URL in the user's default browser.
    /// </summary>
    /// <param name="link">The URL to open.</param>
    public static async Task OpenSiteLink(string link)
    {
        await Task.Run(() =>
        {
            LOGGER.Info("Opening Link {Link}", link);
            try
            {
                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch (Exception other)
            {
                LOGGER.Error(other.Message);
            }
        });
    }
}