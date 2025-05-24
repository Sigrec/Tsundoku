using System.Diagnostics;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;
using Tsundoku.Views;

namespace Tsundoku.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        private static TsundokuTheme _currentTheme = new TsundokuTheme();
        public static TsundokuTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    CurrentThemeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        public static event EventHandler CurrentThemeChanged;
        public TsundokuTheme CurrentThemeInstance => CurrentTheme;

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
        [Reactive] public TsundokuFilter CurFilter { get; set; }
        [Reactive] public string CurLanguage { get; set; }
        [Reactive] public Region CurRegion { get; set; }
        public static string Filter { get; set; }
        public static User? MainUser { get; set; }
        public static bool updatedVersion = false;
        public static bool newCoverCheck = false;
        public static bool isReloading = false;
        public const string CUR_TSUNDOKU_VERSION = "1.0.0";
        public const double SCHEMA_VERSION = 5.2;
        public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.66 Safari/537.36";
        public const string USER_DATA_FILEPATH = @"UserData.json";
        public MainWindow CollectionWindow = (MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;

        protected ViewModelBase()
        {
            CurrentThemeChanged += OnCurrentThemeChanged;
            CurCurrencyChanged += OnCurCurrencyChanged;
            CurFilter = TsundokuFilter.None;
        }

        private void OnCurrentThemeChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(CurrentThemeInstance));
        }

        private void OnCurCurrencyChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(CurCurrencyInstance));
        }

        public virtual void Dispose()
        {
            CurrentThemeChanged -= OnCurrentThemeChanged;
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
