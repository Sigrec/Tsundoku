using System.Reflection;
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
using System.Text.Json;
using System.Reactive.Concurrency;
using System.Diagnostics;
using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Tsundoku.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
#if (!DEBUG)
        private static string filePath = Environment.CurrentDirectory + @"\UserData.json";
#else
        private static string filePath = @"\Tsundoku\UserData.json";
#endif
        public static ObservableCollection<Series> SearchedCollection { get; set; } = new();
        public static ObservableCollection<Series> Collection { get; set; } = new();
        public static User MainUser { get; set; }
        public AddNewSeriesWindow newSeriesWindow;
        public SettingsWindow settingsWindow;
        public CollectionThemeWindow themeSettingsWindow;
        public static string _curDisplay;
        public static uint _curVolumesCollected, _curVolumesToBeCollected;
        public string[] AvailableLanguages { get; } = new string[] { "Romaji", "English", "Native" };
        public string[] AvailableDisplays { get; } = new string[] { "Card" };

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

        public ReactiveCommand<Unit, Unit> OpenAddNewSeriesWindow { get; }
        public ReactiveCommand<Unit, Unit> OpenSettingsWindow { get; }
        public ReactiveCommand<Unit, Unit> OpenThemeSettingsWindow { get; }

        public MainWindowViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(GetUserData);
            this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(400)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(SearchCollection!);

            this.WhenAnyValue(x => x.CurrentTheme).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.MainTheme = x.ThemeName);
            this.WhenAnyValue(x => x.CurDisplay).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.Display = x);

            newSeriesWindow = new AddNewSeriesWindow();
            OpenAddNewSeriesWindow = ReactiveCommand.CreateFromTask(() =>
            {
                if (newSeriesWindow.WindowState == WindowState.Minimized) 
                {
                    newSeriesWindow.WindowState = WindowState.Normal;
                }
                else if(newSeriesWindow.Topmost == false && newSeriesWindow.IsOpen)
                {
                    newSeriesWindow.Topmost = true;
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
                else if(settingsWindow.Topmost == false && settingsWindow.IsOpen)
                {
                    settingsWindow.Topmost = true;
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
                else if(themeSettingsWindow.Topmost == false && themeSettingsWindow.IsOpen)
                {
                    themeSettingsWindow.Topmost = true;
                }
                else
                {
                    themeSettingsWindow.Show();
                }
                return Task.CompletedTask;
            });

            this.WhenAnyValue(x => x.UsersNumVolumesCollected).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.NumVolumesCollected = x);
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

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before deserialization")]
        public void GetUserData()
        {
            Logger.Info("Starting TsundOku");
            if (!File.Exists(filePath))
            {
                Logger.Info("Creating New User");
                ThemeSettingsViewModel.UserThemes = new ObservableCollection<TsundokuTheme>() { TsundokuTheme.DEFAULT_THEME };
                MainUser = new User("UserName", "Romaji", "Default", "Card", ThemeSettingsViewModel.UserThemes, Collection);
                UserName = MainUser.UserName;
                Collection = MainUser.UserCollection;
                CurLanguage = MainUser.CurLanguage;
                CurDisplay = MainUser.Display;
                SaveUsersData();
            }

            MainUser = JsonSerializer.Deserialize<User>(File.ReadAllText(filePath));

            Logger.Info($"Loading {MainUser.UserName}'s Data");
            UserName = MainUser.UserName;
            Collection = MainUser.UserCollection;
            ThemeSettingsViewModel.UserThemes = MainUser.SavedThemes;
            CurLanguage = MainUser.CurLanguage;
            CurDisplay = MainUser.Display;
            CurrentTheme = ThemeSettingsViewModel.UserThemes.Single(x => x.ThemeName == MainUser.MainTheme).Cloning();
            UsersNumVolumesCollected = MainUser.NumVolumesCollected;
            UsersNumVolumesToBeCollected = MainUser.NumVolumesToBeCollected;

            foreach (Series x in Collection)
            {
                SearchedCollection.Add(x);
            }
        }

        public static void CleanCoversFolder()
        {
             // Cleans the Covers asset folder of images for series that is not in the users collection on close/save
            bool removeSeriesCheck = true;
            if (Directory.Exists(@$"{Environment.CurrentDirectory}\Covers"))
            {
                foreach (string coverPath in Directory.GetFiles(@"\Tsundoku\Covers"))
                {
                    int underscoreIndex = coverPath.IndexOf("_");
                    int periodIndex = coverPath.IndexOf(".");
                    foreach (Series curSeries in MainWindowViewModel.Collection)
                    {
                        string curTitle = Regex.Replace(curSeries.Titles[0], @"[^A-Za-z\d]", "");
                        string coverPathTitleAndFormat = coverPath.Substring(17);
                        if (Slice(coverPathTitleAndFormat, 0, coverPathTitleAndFormat.IndexOf("_")).Equals(curTitle) && Slice(coverPathTitleAndFormat, coverPathTitleAndFormat.IndexOf("_") + 1, coverPathTitleAndFormat.IndexOf(".")).Equals(curSeries.Format.ToUpper()))
                        {
                            removeSeriesCheck = false;
                            break;
                        }
                    }

                    if (removeSeriesCheck)
                    {
                        Logger.Info($"Deleted Cover -> {coverPath}");
                        File.Delete(coverPath);
                    }
                    removeSeriesCheck = true;
                }
            }
        }

        public static string Slice(string source, int start, int end)
        {
            if (end < 0) // Keep this for negative end support
            {
                end = source.Length + end;
            }
            int len = end - start;               // Calculate length
            return source.Substring(start, len); // Return Substring of length
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before serialization")]
        public static void SaveUsersData()
        {
            Logger.Info($"Saving {MainUser.UserName}'s Data");
            MainUser.UserCollection = Collection;
            MainUser.SavedThemes = ThemeSettingsViewModel.UserThemes;

            var options = new JsonSerializerOptions { 
                WriteIndented = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            File.WriteAllText(filePath, JsonSerializer.Serialize(MainUser, options));
        }
    }
}
