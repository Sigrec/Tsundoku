using ReactiveUI;
using Tsundoku.Models;
using System.Collections.ObjectModel;
using System.IO;
using Tsundoku.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Collections.Specialized;
using Avalonia.Media;

namespace Tsundoku.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static string filePath = @"UserData.dat";
        public static ObservableCollection<Series> SearchedCollection { get; set; } = new();
        public static ObservableCollection<Series> Collection { get; set; } = new();
        public static User MainUser { get; set; }
        public AddNewSeriesWindow newSeriesWindow = new AddNewSeriesWindow();
        public SettingsWindow settingsWindow;
        public CollectionThemeWindow themeSettingsWindow;
        public static string _curDisplay;
        public static uint _curVolumesCollected, _curVolumesToBeCollected;
        public string[] AvailableLanguages { get; } = new string[] { "Romaji", "English", "Native" };
        public string[] AvailableDisplays { get; } = new string[] { "Card", "Mini-Card" };

        [Reactive]
        public string SearchText { get; set; }

        [Reactive]
        public uint UsersNumVolumesCollected { get; set; }
        private ReactiveCommand<Unit, Unit> IncrementVolumeCount { get; }

        [Reactive]
        public uint UsersNumVolumesToBeCollected { get; set; }
        private ReactiveCommand<Unit, Unit> DecrementVolumeCount { get; }
        
        [Reactive]
        public bool SeriesEditPaneIsOpen { get; set; } = false;
        private ReactiveCommand<Unit, bool> SeriesEditPaneButtonPressed { get; }
        
        [Reactive]
        public string CurLanguage { get; set; }

        [Reactive]
        public bool LanguageChanged { get; set; } = false;

        [Reactive]
        public bool SearchIsBusy { get; set; } = false;

        [Reactive]
        public string UserName { get; set; }

        [Reactive]
        public uint TestVal1 { get; set; } = 99999;

        [Reactive]
        public uint TestVal2 { get; set; } = 9999;

        [Reactive]
        public TsundokuTheme CurrentTheme { get; set; }

        public ReactiveCommand<Unit, Unit> OpenAddNewSeriesWindow { get; }
        public ReactiveCommand<Unit, Unit> OpenSettingsWindow { get; }
        public ReactiveCommand<Unit, Unit> OpenThemeSettingsWindow { get; }

        public string CurDisplay
        {
            get => _curDisplay;
            set => this.RaiseAndSetIfChanged(ref _curDisplay, value);
        }

        public MainWindowViewModel()
        {
            GetUserData();
            this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(400)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(SearchCollection!);

            this.WhenAnyValue(x => x.CurrentTheme).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.MainTheme = x);

            OpenAddNewSeriesWindow = ReactiveCommand.CreateFromTask(() =>
            {
                newSeriesWindow.Show();
                return Task.CompletedTask;
            });

            settingsWindow = new SettingsWindow();
            OpenSettingsWindow = ReactiveCommand.CreateFromTask(() =>
            {
                settingsWindow.Show();
                return Task.CompletedTask;
            });

            themeSettingsWindow = new CollectionThemeWindow();
            OpenThemeSettingsWindow = ReactiveCommand.CreateFromTask(() =>
            {
                themeSettingsWindow.Show();
                return Task.CompletedTask;
            });

            IncrementVolumeCount = ReactiveCommand.Create(IncrementSeriesVolumeCount);
            this.WhenAnyValue(x => x.UsersNumVolumesCollected).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.NumVolumesCollected = x);

            DecrementVolumeCount = ReactiveCommand.Create(DecrementSeriesVolumeCount);
            this.WhenAnyValue(x => x.UsersNumVolumesToBeCollected).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.NumVolumesToBeCollected = x);

            this.WhenAnyValue(x => x.CurLanguage).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.CurLanguage = x);
            this.WhenAnyValue(x => x.UserName).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.UserName = x);

            SeriesEditPaneButtonPressed = ReactiveCommand.Create(() => SeriesEditPaneIsOpen ^= true);

            Collection.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    UsersNumVolumesCollected += Collection[Collection.Count - 1].CurVolumeCount;
                    UsersNumVolumesToBeCollected += (uint)(Collection[Collection.Count - 1].MaxVolumeCount - Collection[Collection.Count - 1].CurVolumeCount);
                }
            };
        }

        private void IncrementSeriesVolumeCount()
        {
            UsersNumVolumesCollected += 1;
            UsersNumVolumesToBeCollected -= 1;
        }

        private void DecrementSeriesVolumeCount()
        {
            UsersNumVolumesCollected -= 1;
            UsersNumVolumesToBeCollected += 1;
        }

        public static void SortCollection()
        {
            SearchedCollection.Clear();
            switch (MainUser.CurLanguage)
            {
                case "Native":
                    foreach (Series x in Collection.OrderBy(x => x.Titles[2], StringComparer.CurrentCulture))
                    {
                        SearchedCollection.Add(x);
                    }
                    break;
                case "English":
                    foreach (Series x in Collection.OrderBy(x => x.Titles[1], StringComparer.CurrentCulture))
                    {
                        SearchedCollection.Add(x);
                    }
                    break;
                default:
                    foreach (Series x in Collection.OrderBy(x => x.Titles[0], StringComparer.CurrentCulture))
                    {
                        SearchedCollection.Add(x);
                    }
                    break;
            }
            MainUser.UserCollection = new ObservableCollection<Series>(SearchedCollection);
            Logger.Info($"Sorting {MainUser.CurLanguage}");
        }

        private void SearchCollection(string searchText)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                SearchedCollection.Clear();
                Logger.Debug($"Searching For {searchText}");
                foreach (Series series in MainUser.UserCollection.Where(x => x.Titles[0].Contains(searchText, StringComparison.InvariantCultureIgnoreCase) | x.Titles[1].Contains(searchText, StringComparison.InvariantCultureIgnoreCase) | x.Titles[2].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) | x.Staff[0].Contains(searchText, StringComparison.InvariantCultureIgnoreCase) | x.Staff[1].Contains(searchText, StringComparison.CurrentCultureIgnoreCase)))
                {
                    SearchedCollection.Add(series);
                }
            }
            else if (SearchIsBusy)
            {
                SearchIsBusy = false;
                SortCollection();
            }
        }

        private void GetUserData()
        {
            Logger.Info("Starting TsundOku");
            if (!File.Exists(filePath))
            {
                Logger.Info("Creating New User");
                TsundokuTheme DefaultTheme = new TsundokuTheme("Default");
                MainUser = new User("UserName", "Native", DefaultTheme, "Card", new ObservableCollection<TsundokuTheme>(), Collection);
                UserName = MainUser.UserName;
                Collection = MainUser.UserCollection;
                CurLanguage = MainUser.CurLanguage;
                CurDisplay = MainUser.Display;
                MainUser.SavedThemes.Add(DefaultTheme);
                ThemeSettingsViewModel.UserThemes = MainUser.SavedThemes;
                SaveUsersData();
            }

            using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                MainUser = (User)binaryFormatter.Deserialize(stream);
            }
            Logger.Info($"Loading {MainUser.UserName}'s Data");
            UserName = MainUser.UserName;
            Collection = MainUser.UserCollection;
            ThemeSettingsViewModel.UserThemes = MainUser.SavedThemes;
            CurLanguage = MainUser.CurLanguage;
            CurDisplay = MainUser.Display;
            CurrentTheme = MainUser.MainTheme;
            UsersNumVolumesCollected = MainUser.NumVolumesCollected;
            UsersNumVolumesToBeCollected = MainUser.NumVolumesToBeCollected;
        }

        public static async void SaveUsersData(){
            Logger.Info($"Saving {MainUser.UserName}'s Data");
            MainUser.UserCollection = Collection;
            MainUser.Display = _curDisplay;
            MainUser.SavedThemes = ThemeSettingsViewModel.UserThemes;
            using (Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, MainUser);
            }
        }
    }
}
