using System.Security.Cryptography.X509Certificates;
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
using System.Text.Json;
using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Media.Imaging;
using DynamicData;

namespace Tsundoku.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static string filePath = @"UserData.json";
        public static ObservableCollection<Series> SearchedCollection { get; set; } = new();
        public static ObservableCollection<Series> Collection { get; set; } = new();
        public static User MainUser { get; set; }
        public AddNewSeriesWindow newSeriesWindow;
        public SettingsWindow settingsWindow;
        public CollectionThemeWindow themeSettingsWindow;
        public string[] AvailableLanguages { get; } = new string[] { "Romaji", "English", "Native" };
        public string[] AvailableCollectionFilters { get; } = new string[] { "None", "Ongoing", "Finished", "Hiatus", "Cancelled", "Complete", "Incomplete", "Manga", "Novel" };

        [Reactive]
        public string SearchText { get; set; }

        [Reactive]
        public uint UsersNumVolumesCollected { get; set; }

        [Reactive]
        public uint UsersNumVolumesToBeCollected { get; set; }
        
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
        public string CurDisplay { get; set; }

        // [Reactive]
        // public uint TestVal { get; set; } = 88888;

        public ReactiveCommand<Unit, Unit> OpenAddNewSeriesWindow { get; }
        public ReactiveCommand<Unit, Unit> OpenSettingsWindow { get; }
        public ReactiveCommand<Unit, Unit> OpenThemeSettingsWindow { get; }

        public MainWindowViewModel()
        {
            GetUserData();

            this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(600)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(SearchCollection);

            this.WhenAnyValue(x => x.CurrentTheme).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.MainTheme = x.ThemeName);
            this.WhenAnyValue(x => x.CurDisplay).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.Display = x);

            this.WhenAnyValue(x => x.UsersNumVolumesCollected).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.NumVolumesCollected = x);
            this.WhenAnyValue(x => x.UsersNumVolumesToBeCollected).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.NumVolumesToBeCollected = x);

            this.WhenAnyValue(x => x.CurLanguage).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.CurLanguage = x);
            this.WhenAnyValue(x => x.UserName).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.UserName = x);

            newSeriesWindow = new AddNewSeriesWindow();
            OpenAddNewSeriesWindow = ReactiveCommand.CreateFromTask(() =>
            {
                if (newSeriesWindow.WindowState == WindowState.Minimized) 
                {
                    newSeriesWindow.WindowState = WindowState.Normal;
                }
                else if(!newSeriesWindow.IsActive && newSeriesWindow.IsOpen)
                {
                    newSeriesWindow.Activate();
                }
                else
                {
                    newSeriesWindow.Show();
                }
                return Task.CompletedTask;
            });

            settingsWindow = new SettingsWindow();
            OpenSettingsWindow = ReactiveCommand.CreateFromTask(() =>
            {
                if (settingsWindow.WindowState == WindowState.Minimized) 
                {
                    settingsWindow.WindowState = WindowState.Normal;
                }
                else if(!settingsWindow.IsActive && settingsWindow.IsOpen)
                {
                    settingsWindow.Activate();
                }
                else
                {
                    settingsWindow.Show();
                }
                return Task.CompletedTask;
            });

            themeSettingsWindow = new CollectionThemeWindow();
            OpenThemeSettingsWindow = ReactiveCommand.CreateFromTask(() =>
            {
                if (themeSettingsWindow.WindowState == WindowState.Minimized) 
                {
                    themeSettingsWindow.WindowState = WindowState.Normal;
                }
                else if(!themeSettingsWindow.IsActive && themeSettingsWindow.IsOpen)
                {
                    themeSettingsWindow.Activate();
                }
                else
                {
                    themeSettingsWindow.Show();
                }
                return Task.CompletedTask;
            });

            SeriesEditPaneButtonPressed = ReactiveCommand.Create(() => SeriesEditPaneIsOpen ^= true);
        }

        public static int SearchForSort(Series series)
        {
            int index;
            switch(MainUser.CurLanguage)
            {
                case "Native":
                    index = Collection.BinarySearch<Series>(series, new NativeComparer());
                    break;
                case "English":
                    index = Collection.BinarySearch<Series>(series, new EnglishComparer());
                    break;
                default:
                    index = Collection.BinarySearch<Series>(series, new RomajiComparer());
                    break;
            }
            return index;
        }

        public static void SortCollection()
        {
            SearchedCollection.Clear();
            switch (MainUser.CurLanguage)
            {
                case "Native":
                    SearchedCollection.AddRange(Collection.AsParallel().OrderBy(x => x.Titles[2], StringComparer.Create(new System.Globalization.CultureInfo("ja-JP"), true)));
                    break;
                case "English": 
                    SearchedCollection.AddRange(Collection.AsParallel().OrderBy(x => x.Titles[1], StringComparer.OrdinalIgnoreCase));
                    break;
                default:
                    SearchedCollection.AddRange(Collection.AsParallel().OrderBy(x => x.Titles[0], StringComparer.OrdinalIgnoreCase));
                    break;
            }
            Collection = new ObservableCollection<Series>(SearchedCollection);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static void FilterCollection(string filter)
        {
            SearchedCollection.Clear();
            switch (filter)
            {
                case "Ongoing":
                    SearchedCollection.AddRange(Collection.Where(series => series.Status.Equals("Ongoing")));
                    break;
                case "Finished":
                    SearchedCollection.AddRange(Collection.Where(series => series.Status.Equals("Finished")));
                    break;
                case "Hiatus":
                    SearchedCollection.AddRange(Collection.Where(series => series.Status.Equals("Hiatus")));
                    break;
                case "Cancelled":
                    SearchedCollection.AddRange(Collection.Where(series => series.Status.Equals("Cancelled")));
                    break;
                case "Complete":
                    SearchedCollection.AddRange(Collection.Where(series => series.CurVolumeCount == series.MaxVolumeCount));
                    break;
                case "Incomplete":
                    SearchedCollection.AddRange(Collection.Where(series => series.CurVolumeCount != series.MaxVolumeCount));
                    break;
                case "Manga":
                    SearchedCollection.AddRange(Collection.Where(series => series.Format.Equals("Manga") || series.Format.Equals("Manhwa") || series.Format.Equals("Manhua") || series.Format.Equals("Manfra")));
                    break;
                case "Novel":
                    SearchedCollection.AddRange(Collection.Where(series => series.Format.Equals("Novel")));
                    break;
                case "None":
                default:
                    SearchedCollection.AddRange(Collection);
                    break;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void SearchCollection(string searchText)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                SearchedCollection.Clear();
                Logger.Info($"Searching For {searchText}");
                SearchedCollection.AddRange(Collection.AsParallel().Where(x => x.Titles[0].Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || x.Titles[1].Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || x.Titles[2].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) || x.Staff[0].Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || x.Staff[1].Contains(searchText, StringComparison.CurrentCultureIgnoreCase)));
            }
            else if (SearchIsBusy)
            {
                SearchedCollection.Clear();
                SearchIsBusy = false;
                Logger.Info($"No Longer Searching");
                SearchedCollection.AddRange(Collection);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before deserialization")]
        public void GetUserData()
        {
            Logger.Info("Starting TsundOku");
            if (!File.Exists(filePath))
            {
                Logger.Info("Creating New User");
                ThemeSettingsViewModel.UserThemes = new ObservableCollection<TsundokuTheme>() { TsundokuTheme.DEFAULT_THEME };
                MainUser = new User("UserName", "Romaji", "Default", "Card", ThemeSettingsViewModel.UserThemes, Collection);
                MainUser.CurDataVersion = 1.4;
                UserName = MainUser.UserName;
                Collection = MainUser.UserCollection;
                CurLanguage = MainUser.CurLanguage;
                CurDisplay = MainUser.Display;
                SaveUsersData();
            }

            FileStream fRead = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            MainUser = JsonSerializer.DeserializeAsync<User>(fRead, options).Result;
            fRead.FlushAsync();
            fRead.Close();

            Logger.Info($"Loading {MainUser.UserName}'s Data");
            UserName = MainUser.UserName;
            Collection = MainUser.UserCollection;
            ThemeSettingsViewModel.UserThemes = MainUser.SavedThemes;
            CurLanguage = MainUser.CurLanguage;
            CurDisplay = MainUser.Display;
            CurrentTheme = ThemeSettingsViewModel.UserThemes.Single(x => x.ThemeName == MainUser.MainTheme).Cloning();

            uint testUsersNumVolumesCollected = 0, testUsersNumVolumesToBeCollected = 0;

            // Pre allocate the bitmaps for every image so it is not remade every pass.
            foreach (Series x in Collection)
            {
                testUsersNumVolumesCollected += x.CurVolumeCount;
                testUsersNumVolumesToBeCollected += (uint)(x.MaxVolumeCount - x.CurVolumeCount);
                // If the image does not exist in the covers folder then don't create a bitmap for it
                if (File.Exists(x.Cover))
                {
                    x.CoverBitMap = new Bitmap(x.Cover).CreateScaledBitmap(new Avalonia.PixelSize(Constants.LEFT_SIDE_CARD_WIDTH, Constants.IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                }
                SearchedCollection.Add(x);
            }

            // Crash protection for series count
            if (testUsersNumVolumesCollected == MainUser.NumVolumesCollected && testUsersNumVolumesToBeCollected == MainUser.NumVolumesToBeCollected)
            {
                UsersNumVolumesCollected = MainUser.NumVolumesCollected;
                UsersNumVolumesToBeCollected = MainUser.NumVolumesToBeCollected;
            }
            else
            {
                UsersNumVolumesCollected = testUsersNumVolumesCollected;
                UsersNumVolumesToBeCollected = testUsersNumVolumesToBeCollected;
            }

            // Updating series data from for Version 1.4.0.0
            if (MainUser.CurDataVersion < 1.4)
            {
                MainUser.CurDataVersion = 1.4;
                foreach (Series x in Collection)
                {
                    if (x.Status.Equals("Complete"))
                    {
                        x.Status = "Finished";
                    }
                }
                Logger.Info("Updated Users Data for version 1.4.0.0");
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before serialization")]
        public static void SaveUsersData()
        {
            Logger.Info($"Saving {MainUser.UserName}'s Data");
            MainUser.UserCollection = Collection;
            MainUser.SavedThemes = ThemeSettingsViewModel.UserThemes;

            File.WriteAllText(filePath, JsonSerializer.Serialize(MainUser, options));
        }
    }
}