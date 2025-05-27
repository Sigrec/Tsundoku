using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;
using static Tsundoku.Models.TsundokuLanguageModel;

namespace Tsundoku.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private static string _curCurrency;
        public static string CurCurrency
        {
            get => _curCurrency;
            set
            {
                if (_curCurrency != value)
                {
                    _curCurrency = value;
                    CurCurrencyChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static event EventHandler CurCurrencyChanged;
        public string CurCurrencyInstance => CurCurrency;
        [Reactive] public string UserName { get; set; }
        [Reactive] public string CurDisplay { get; set; }
        [Reactive] public Region CurRegion { get; set; }
        public static string Filter { get; set; }
        public static User? MainUser { get; set; }
        public static bool updatedVersion = false;
        public static bool newCoverCheck = false;
        public bool isReloading = false;
        public const string CUR_TSUNDOKU_VERSION = "1.0.0";
        public const double SCHEMA_VERSION = 5.2;
        public const string USER_DATA_FILEPATH = @"UserData.json";
        
        [Reactive] public TsundokuTheme CurrentTheme { get; private set; }
        [Reactive] public User CurrentUser { get; private set; }
        
        protected readonly IUserService _userService;
        protected readonly CompositeDisposable _disposables = new CompositeDisposable();

        protected ViewModelBase(IUserService userService)
        {
            _userService = userService;
            _userService.CurrentTheme
                .Where(theme => theme != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(theme => CurrentTheme = theme!)
                .DisposeWith(_disposables);

            _userService.CurrentUser
                .Where(user => user != null) // Filters out the initial null from BehaviorSubject
                .Subscribe(user => CurrentUser = user);

            CurCurrencyChanged += OnCurCurrencyChanged;
        }

        private void OnCurCurrencyChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(CurCurrencyInstance));
        }

        public virtual void Dispose()
        {
            CurCurrencyChanged -= OnCurCurrencyChanged;
        }

        public static async Task OpenSiteLink(string link)
        {
            await Task.Run(() =>
            {
                LOGGER.Debug($"Opening Link {link}");
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
}