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
using System.Text.Json.Nodes;
using System.Collections.Generic;
using MangaLightNovelWebScrape.Websites;

namespace Tsundoku.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private static readonly string USER_DATA_FILEPATH = @"UserData.json";
        private const double SCHEMA_VERSION = 2.0;
        private static bool newUserFlag = false;
        private static bool ShouldFilter = true;
        public static ObservableCollection<Series> SearchedCollection { get; set; } = [];
        public static List<Series> UserCollection { get; set; } = [];
        private static IEnumerable<Series> FilteredCollection { get; set; } = [];
        [Reactive] public string SearchText { get; set; }
        [Reactive] public bool LanguageChanged { get; set; } = false;
        [Reactive] public bool SearchIsBusy { get; set; } = false;
        [Reactive] public string CurDisplay { get; set; }
        [Reactive] public string CurFilter { get; set; } = "None";
        [Reactive] public Bitmap UserIcon { get; set; }
        [Reactive] public string CurLanguage { get; set; }
        [Reactive] public int LanguageIndex { get; set; }
        [Reactive] public int FilterIndex { get; set; }
        //private static SolidColorBrush AdvancedSearchBackgroundColor = new SolidColorBrush(Colors.Black);

        public AddNewSeriesWindow newSeriesWindow;
        public ReactiveCommand<Unit, Unit> OpenAddNewSeriesWindow { get; set; }

        public SettingsWindow settingsWindow;
        public ReactiveCommand<Unit, Unit> OpenSettingsWindow { get; set; }

        public CollectionThemeWindow themeSettingsWindow;
        public ReactiveCommand<Unit, Unit> OpenThemeSettingsWindow { get; set; }

        public PriceAnalysisWindow priceAnalysisWindow;
        public ReactiveCommand<Unit, Unit> OpenPriceAnalysisWindow { get; set; }

        public CollectionStatsWindow collectionStatsWindow;
        public ReactiveCommand<Unit, Unit> OpenCollectionStatsWindow { get; set; }

        // TODO Add wish list selection for series?
        // TODO Add genres?
        // TODO Manual Entry Option?
        // TODO Add Search Filters ex Series that have a rating of 5
        // TODO Add barcode functiionality?
        // TODO Add export/import for themes
        public MainWindowViewModel()
        {
            // Helpers.ExtensionMethods.PrintCultures();
            // Helpers.ExtensionMethods.PrintCurrencySymbols();
            LOGGER.Info("Starting TsundOku");
            LoadUserData();
            ConfigureWindows();

            this.WhenAnyValue(x => x.CurLanguage).Subscribe(x => MainUser.CurLanguage = x);
            this.WhenAnyValue(x => x.CurLanguage).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => LanguageIndex = AvailableLanguages.IndexOf(x));
            this.WhenAnyValue(x => x.CurFilter).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => AvailableCollectionFilters.IndexOf(x));
            this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(600)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(SearchCollection);
            this.WhenAnyValue(x => x.CurrentTheme).Subscribe(x => MainUser.MainTheme = x.ThemeName);
            this.WhenAnyValue(x => x.CurDisplay).Subscribe(x => MainUser.Display = x);
            this.WhenAnyValue(x => x.UserName).Subscribe(x => MainUser.UserName = x);
            this.WhenAnyValue(x => x.UserIcon).Subscribe(x => MainUser.UserIcon = User.ImageToByteArray(x));
        }

        private void LanguageChange(string lang)
        {
            MainUser.CurLanguage = lang;
            LanguageIndex = AvailableLanguages.IndexOf(lang);
        }

        /// <summary>
        /// Configures the various windows in the app
        /// </summary>
        private void ConfigureWindows()
        {
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

            priceAnalysisWindow = new PriceAnalysisWindow();
            OpenPriceAnalysisWindow = ReactiveCommand.CreateFromTask(() =>
            {
                if (priceAnalysisWindow.WindowState == WindowState.Minimized) 
                {
                    priceAnalysisWindow.WindowState = WindowState.Normal;
                }
                else if(!priceAnalysisWindow.IsActive && priceAnalysisWindow.IsOpen)
                {
                    priceAnalysisWindow.Activate();
                }
                else
                {
                    priceAnalysisWindow.Show();
                }
                return Task.CompletedTask;
            });

            collectionStatsWindow = new CollectionStatsWindow();
            OpenCollectionStatsWindow = ReactiveCommand.CreateFromTask(() =>
            {
                if (collectionStatsWindow.WindowState == WindowState.Minimized) 
                {
                    collectionStatsWindow.WindowState = WindowState.Normal;
                }
                else if(!collectionStatsWindow.IsActive && collectionStatsWindow.IsOpen)
                {
                    collectionStatsWindow.Activate();
                }
                else
                {
                    collectionStatsWindow.Show();
                }
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Sorts the users collection based on language
        /// </summary>
        public static async void SortCollection()
        {
            SearchedCollection.Clear();
            await Task.Run(() => 
            {
                UserCollection.Sort(new SeriesComparer(MainUser.CurLanguage));
            });
            SearchedCollection.AddRange(UserCollection);
        }

        /// <summary>
        /// Filters the users collection based on the selected preset filter
        /// </summary>
        /// <param name="filter">The filter preset chosen</param>
        public async void FilterCollection(string filter)
        {
            if (ShouldFilter)
            {
                if (!string.IsNullOrWhiteSpace(SearchText)) // Checks if the user is filtering after a search
                {
                    SearchIsBusy = false;
                    SearchText = string.Empty;
                }
                await Task.Run(() => {
                    switch (filter)
                    {
                        case "Ongoing":
                            FilteredCollection = UserCollection.Where(series => series.Status.Equals("Ongoing"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Finished":
                            FilteredCollection = UserCollection.Where(series => series.Status.Equals("Finished"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Hiatus":
                            FilteredCollection = UserCollection.Where(series => series.Status.Equals("Hiatus"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Cancelled":
                            FilteredCollection = UserCollection.Where(series => series.Status.Equals("Cancelled"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Complete":
                            FilteredCollection = UserCollection.Where(series => series.CurVolumeCount == series.MaxVolumeCount);
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Incomplete":
                            FilteredCollection = UserCollection.Where(series => series.CurVolumeCount != series.MaxVolumeCount);
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Favorites":
                            FilteredCollection = UserCollection.Where(series => series.IsFavorite);
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Manga":
                            FilteredCollection = UserCollection.Where(series => series.Format.Equals("Manga") || series.Format.Equals("Manhwa") || series.Format.Equals("Manhua") || series.Format.Equals("Manfra") || series.Format.Equals("Comic"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Novel":
                            FilteredCollection = UserCollection.Where(series => series.Format.Equals("Novel"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Shounen":
                            FilteredCollection = UserCollection.Where(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Shounen"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Shoujo":
                            FilteredCollection = UserCollection.Where(series => !string.IsNullOrWhiteSpace(series.Demographic) &&series.Demographic.Equals("Shoujo"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Seinen":
                            FilteredCollection = UserCollection.Where(series => !string.IsNullOrWhiteSpace(series.Demographic) &&series.Demographic.Equals("Seinen"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Josei":
                            FilteredCollection = UserCollection.Where(series => !string.IsNullOrWhiteSpace(series.Demographic) &&series.Demographic.Equals("Josei"));
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Read":
                            FilteredCollection = UserCollection.Where(series => series.VolumesRead != 0);
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Unread":
                            FilteredCollection = UserCollection.Where(series => series.VolumesRead == 0);
                            LOGGER.Info($"Filtered Collection by {filter}");
                            break;
                        case "Rating":
                            FilteredCollection = UserCollection.OrderByDescending(series => series.Rating);
                            LOGGER.Info($"Sorted Collection by {filter}");
                            break;
                        case "Cost":
                            FilteredCollection = UserCollection.OrderByDescending(series => series.Cost);
                            LOGGER.Info($"Sorted Collection by {filter}");
                            break;
                        case "None":
                        default:
                            FilteredCollection = UserCollection;
                            LOGGER.Info($"Removing Sort/Filter");
                            break;
                    }
                });
                SearchedCollection.Clear();
                SearchedCollection.AddRange(FilteredCollection);
            }
        }

        /// <summary>
        /// Searches the users collection by title and/or staff
        /// </summary>
        /// <param name="searchText">The text to search for</param>
        private async void SearchCollection(string searchText)
        {
            // Advanced Search Filter '[<filter><type><value>&<filter><type><value>&...]'
            // ex '[rating>=5&status=finished&cost>700&format=manga&staff=Tsuyoshi&notes=text&favorite&demographic=shounen&read=0&curvolumes>5&maxvolumes>=8&desc=text&complete]'
            // Regex = @"(?<filter>\w+)(?<math>=|<=|>=)(?<value>\d+|\w+)(?: & )?"
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (!CurFilter.Equals("None"))
                {
                    ShouldFilter = false;
                    CurFilter = "None";
                }

                await Task.Run(() => {
                    FilteredCollection = UserCollection.Where(x => x.Titles.Values.AsParallel().Any(title => title.Contains(searchText, StringComparison.OrdinalIgnoreCase)) || x.Staff.Values.AsParallel().Any(staff => staff.Contains(searchText, StringComparison.OrdinalIgnoreCase))); 
                });

                SearchedCollection.Clear();
                SearchedCollection.AddRange(FilteredCollection);
                ShouldFilter = true;
            }
            else if (SearchIsBusy)
            {
                SearchIsBusy = false;
                SearchedCollection.Clear();
                SearchedCollection.AddRange(UserCollection);
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before deserialization")]
        /// <summary>
        /// Gets the users data from the UserData.Json file in current working directory where the executable is
        /// </summary>
        private void LoadUserData()
        {
            if (!File.Exists(USER_DATA_FILEPATH))
            {
                LOGGER.Info("Creating New User");
                ThemeSettingsViewModel.UserThemes = [TsundokuTheme.DEFAULT_THEME];
                MainUser = new User(
                                    "UserName", 
                                    "Romaji", 
                                    "Default", 
                                    "Card", 
                                    SCHEMA_VERSION, 
                                    "$", 
                                    "$0.00", 
                                    new Dictionary<string, bool>{
                                        { RightStufAnime.WEBSITE_TITLE, false },
                                        { BarnesAndNoble.WEBSITE_TITLE , false },
                                        { BooksAMillion.WEBSITE_TITLE, false },
                                        { KinokuniyaUSA.WEBSITE_TITLE , false }
                                    }, 
                                    ThemeSettingsViewModel.UserThemes, 
                                    UserCollection
                                );
                UserName = MainUser.UserName;
                UserCollection = MainUser.UserCollection;
                CurLanguage = MainUser.CurLanguage;
                CurDisplay = MainUser.Display;
                newUserFlag = true;
                SaveUsersData();
            }
            
            bool save = VersionUpdate();

            FileStream fRead = new(USER_DATA_FILEPATH, FileMode.Open, FileAccess.Read, FileShare.Read);
            MainUser = JsonSerializer.Deserialize<User>(fRead, options);
            fRead.Flush();
            fRead.Close();

            LOGGER.Info($"Loading {MainUser.UserName}'s Data");
            UserName = MainUser.UserName;
            UserCollection = MainUser.UserCollection;
            ThemeSettingsViewModel.UserThemes = new ObservableCollection<TsundokuTheme>(MainUser.SavedThemes.OrderBy(theme => theme.ThemeName).ToList());
            CurLanguage = MainUser.CurLanguage;
            CurDisplay = MainUser.Display;
            CurCurrency = MainUser.Currency;
            LanguageIndex = Array.IndexOf(AvailableLanguages, CurLanguage);
            CurrentTheme = MainUser.SavedThemes.Single(x => x.ThemeName == MainUser.MainTheme).Cloning();
            if (MainUser.UserIcon.Length > 0 && MainUser.UserIcon != null)
            {
                UserIcon = new Bitmap(new MemoryStream(MainUser.UserIcon)).CreateScaledBitmap(new PixelSize(USER_ICON_WIDTH, USER_ICON_HEIGHT), BitmapInterpolationMode.HighQuality);
            }

            // Pre allocate the bitmaps for every image so it is not remade every pass.
            foreach (Series x in UserCollection)
            {
                // If the image does not exist in the covers folder then don't create a bitmap for it
                if(File.Exists(x.Cover))
                {
                    x.CoverBitMap = new Bitmap(x.Cover);
                }
                SearchedCollection.Add(x);
            }

            if (save) { SaveUsersData(); UpdateDefaultTheme(); updatedVersion = false; }
        }

        /// <summary>
        /// Updates the default TsundokuTheme
        /// </summary>
        private static void UpdateDefaultTheme()
        {
            for(int x = 0; x < MainUser.SavedThemes.Count; x++)
            {
                if (MainUser.SavedThemes[x].ThemeName.Equals("Default"))
                {
                    MainUser.SavedThemes[x] = TsundokuTheme.DEFAULT_THEME;
                    LOGGER.Info("Updated Default Theme");
                    break;
                }
            }
        }

        // TODO: Need to update Currency as well prob?
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
        private static bool VersionUpdate()
        {
            string json = File.ReadAllText(USER_DATA_FILEPATH);
            JsonNode userData = JsonNode.Parse(json);
            JsonNode series;
            JsonArray collectionJsonArray = userData["UserCollection"].AsArray();
            
            // For users who did not get the older update
            if (!userData.AsObject().ContainsKey("CurDataVersion"))
            {
                LOGGER.Debug("Check #4");
                userData.AsObject().Add("CurDataVersion", "1.0");
                LOGGER.Info("Added CurDataVersion Json Object");
            }
            double curVersion = double.Parse(userData["CurDataVersion"].ToString());

            if (curVersion < 1.5)
            {
                userData.AsObject().Add("Currency", "$");
                userData.AsObject().Add("MeanRating", 0);
                userData.AsObject().Add("VolumesRead", 0);
                userData.AsObject().Add("CollectionPrice", "");

                if (userData["CurLanguage"].ToString().Equals("Native"))
                {
                    userData["CurLanguage"] = "Japanese";
                }

                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x);
                    series["Titles"] = new JsonObject
					{
						["Romaji"] = series["Titles"][0].ToString(),
                        ["English"] = series["Titles"][1].ToString(),
                        [series["Format"].ToString() switch
                        {
                            "Manhwa" => "Korean",
                            "Manhua" => "Chinese",
                            _ => "Japanese"            
                        }] = series["Titles"][2].ToString()
                    };

                    series["Staff"] = new JsonObject
					{
						["Romaji"] = series["Staff"][0].ToString(),
                        [series["Format"].ToString() switch
                        {
                            "Manhwa" => "Korean",
                            "Manhua" => "Chinese",
                            _ => "Japanese"            
                        }] = series["Staff"][1].ToString()
                    };
                    series.AsObject().Add("Score", 0);
                    
                    if (series["Titles"]["Romaji"].ToString().Equals(series["Titles"]["English"].ToString()))
                    {
                        series["Titles"].AsObject().Remove("English");
                    }

                    if (curVersion < 1.4 && series["Status"].ToString().Equals("Complete"))
                    {
                        series["Status"] = "Finished";
                    }
                }
                userData["CurDataVersion"] = 1.5;
                LOGGER.Info("Updated Users Data to v1.5");
                updatedVersion = true;        
            }

            if (curVersion < 1.6)
            {
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x);
                    if (double.Parse(series["Score"].ToString()) == 0)
                    {
                        series["Score"] = -1;
                    }
                }

                userData["CurDataVersion"] = 1.6;
                LOGGER.Info("Updated Users Data to v1.6");
                updatedVersion = true;  
            }

            if (curVersion < 1.7 || newUserFlag)
            {
                string defaultFileByte = "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";

                if (newUserFlag)
                {
                    userData["UserIcon"] = defaultFileByte;
                    updatedVersion = false;
                }
                else
                {
                    userData.AsObject().Add(nameof(UserIcon), defaultFileByte);
                    userData["CurDataVersion"] = 1.7;
                    LOGGER.Info("Updated Users Data to v1.7");
                    updatedVersion = true;
                }
            }

            if (curVersion < 1.8)
            {
                JsonArray themeJsonArray = userData["SavedThemes"].AsArray();
                JsonObject theme;
                
                for (int x = 0; x < themeJsonArray.Count; x++)
                {
                    theme = themeJsonArray.ElementAt(x).AsObject();
                    theme["SeriesButtonBGColor"] = (uint)theme["SeriesSwitchPaneButtonBGColor"];
                    theme["SeriesButtonBGHoverColor"] = (uint)theme["SeriesSwitchPaneButtonBGHoverColor"];
                    theme["SeriesButtonIconColor"] = (uint)theme["SeriesSwitchPaneButtonIconColor"];
                    theme["SeriesButtonIconHoverColor"] = (uint)theme["SeriesSwitchPaneButtonIconHoverColor"];
                }

                userData["CurDataVersion"] = 1.8;
                LOGGER.Info("Updated Users Data to v1.8");
                updatedVersion = true;  
            }

            if (curVersion < 1.9)
            {
                string coverPath;
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    coverPath = collectionJsonArray.ElementAt(x)["Cover"].ToString();
                    if (File.Exists(coverPath))
                    {
                        Bitmap resizedBitMap = new Bitmap(coverPath).CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                        resizedBitMap.Save(coverPath, 100);
                    }
                }
                userData["CurDataVersion"] = 1.9;
                LOGGER.Info("Updated Users Data to v1.9");
                updatedVersion = true;
            }

            if (curVersion < 2.0)
            {
                userData["Memberships"] = new JsonObject
                {
                    [RightStufAnime.WEBSITE_TITLE] = false,
                    [BarnesAndNoble.WEBSITE_TITLE] = false,
                    [BooksAMillion.WEBSITE_TITLE] = false,
                    [KinokuniyaUSA.WEBSITE_TITLE] = false
                };

                userData.AsObject()["MeanRating"] = (decimal)userData["MeanScore"];
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    series["Rating"] = (decimal)series["Score"];
                }

                userData["CurDataVersion"] = 2.0;
                LOGGER.Info("Updated Users Data to v2.0");
                updatedVersion = true;
            }

            File.WriteAllText(USER_DATA_FILEPATH, JsonSerializer.Serialize(userData, options));
            return updatedVersion;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before serialization")]
        public static void SaveUsersData()
        {
            LOGGER.Info($"Saving {MainUser.UserName}'s Data");
            MainUser.UserCollection = UserCollection;
            MainUser.SavedThemes = ThemeSettingsViewModel.UserThemes;

            File.WriteAllText(USER_DATA_FILEPATH, JsonSerializer.Serialize(MainUser, options));
        }
    }
}