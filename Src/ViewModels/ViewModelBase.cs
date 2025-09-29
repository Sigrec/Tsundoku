using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    public static string Filter { get; set; }
    public bool isReloading = false;
    public const string CUR_TSUNDOKU_VERSION = "1.4.2";
    public const double SCHEMA_VERSION = 6.1;
    public const string USER_DATA_FILEPATH = @"UserData.json";
    
    [Reactive] public TsundokuTheme CurrentTheme { get; protected set; }
    [Reactive] public User CurrentUser { get; protected set; }
    
    protected readonly IUserService _userService;
    protected readonly CompositeDisposable _disposables = [];

    protected ViewModelBase(IUserService userService)
    {
        _userService = userService;
        _userService.CurrentTheme
            .Where(theme => theme is not null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(theme => CurrentTheme = theme!)
            .DisposeWith(_disposables);

        _userService.CurrentUser
            .Where(user => user is not null) // Filters out the initial null from BehaviorSubject
            .Subscribe(user => CurrentUser = user);
    }

    public static async Task OpenSiteLink(string link)
    {
        await Task.Run(() =>
        {
            LOGGER.Info($"Opening Link {link}");
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