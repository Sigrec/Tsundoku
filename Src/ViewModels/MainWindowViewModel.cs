﻿using ReactiveUI;
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

/*
Issues
❌ LineHeight doesn't work with lower LineHeight values
❌ ComboBoxItem text color not applying in non main windows
❌ ListBoxItem theme/styling not applying in AddNewSeriesWindow
❌ Users can run multiple instances of the application
*/

namespace Tsundoku.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private static readonly string filePath = @"UserData.json";
        private const double SCHEMA_VERSION = 1.6;
        public static ObservableCollection<Series> SearchedCollection { get; set; } = new();
        public static ObservableCollection<Series> Collection { get; set; } = new();

        [Reactive] public string SearchText { get; set; }
        [Reactive] public bool LanguageChanged { get; set; } = false;
        [Reactive] public bool SearchIsBusy { get; set; } = false;
        [Reactive] public string UserName { get; set; }
        [Reactive] public string CurDisplay { get; set; }
        [Reactive] public string CurFilter { get; set; } = "None";
        [Reactive] public Bitmap UserIcon { get; set; }
        [Reactive] public string CurLanguage { get; set; }
        [Reactive] public int LanguageIndex { get; set; }
        [Reactive] public int FilterIndex { get; set; }
        // [Reactive] public uint TestVal { get; set; } = 88888;

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

        public MainWindowViewModel()
        {
            // Helpers.ExtensionMethods.PrintCultures();
            Constants.Logger.Info("Starting TsundOku");
            GetUserData();
            ConfigureWindows();

            this.WhenAnyValue(x => x.CurLanguage).Subscribe(x => MainUser.CurLanguage = x);
            this.WhenAnyValue(x => x.CurLanguage).Subscribe(x => LanguageIndex = Constants.AvailableLanguages.IndexOf(x));
            this.WhenAnyValue(x => x.CurFilter).Subscribe(x => FilterIndex = Constants.AvailableCollectionFilters.IndexOf(x));
            this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(600)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(SearchCollection);
            this.WhenAnyValue(x => x.CurrentTheme).Subscribe(x => MainUser.MainTheme = x.ThemeName);
            this.WhenAnyValue(x => x.CurDisplay).Subscribe(x => MainUser.Display = x);
            this.WhenAnyValue(x => x.UserName).Subscribe(x => MainUser.UserName = x);
            this.WhenAnyValue(x => x.UserIcon).Subscribe(x => MainUser.UserIcon = User.ImageToByteArray(x));
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
        public static void SortCollection()
        {
            SearchedCollection.Clear();
            SearchedCollection.AddRange(Collection.OrderBy(x => x.Titles.ContainsKey(MainUser.CurLanguage) ? x.Titles[MainUser.CurLanguage] : x.Titles["Romaji"], StringComparer.Create(new System.Globalization.CultureInfo(Constants.CULTURE_LANG_CODES[MainUser.CurLanguage]), false)));
            Collection = new ObservableCollection<Series>(SearchedCollection);
        }

        /// <summary>
        /// Filters the users collection based on the selected preset filter
        /// </summary>
        /// <param name="filter">The filter preset chosen</param>
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
                case "Favorites":
                    SearchedCollection.AddRange(Collection.Where(series => series.IsFavorite));
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
        }

        /// <summary>
        /// Searches the users collection by title and/or staff
        /// </summary>
        /// <param name="searchText">The text to search for</param>
        private void SearchCollection(string searchText)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                SearchedCollection.Clear();
                SearchedCollection.AddRange(Collection.AsParallel().Where(x => x.Titles.AsParallel().Any(text => text.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase)) || x.Staff.AsParallel().Any(text => text.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))));
            }
            else if (SearchIsBusy)
            {
                SearchedCollection.Clear();
                SearchIsBusy = false;
                SearchedCollection.AddRange(Collection);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before deserialization")]
        private void GetUserData()
        {
            if (!File.Exists(filePath))
            {
                Constants.Logger.Info("Creating New User");
                ThemeSettingsViewModel.UserThemes = new ObservableCollection<TsundokuTheme>() { TsundokuTheme.DEFAULT_THEME };
                MainUser = new User("UserName", "Romaji", "Default", "Card", "$", "$0.00", ThemeSettingsViewModel.UserThemes, Collection)
                {
                    CurDataVersion = SCHEMA_VERSION
                };
                UserName = MainUser.UserName;
                Collection = MainUser.UserCollection;
                CurLanguage = MainUser.CurLanguage;
                CurDisplay = MainUser.Display;
                SaveUsersData();
            }
            
            bool save = VersionUpdate();

            FileStream fRead = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            MainUser = JsonSerializer.Deserialize<User>(fRead, options);
            fRead.Flush();
            fRead.Close();

            CurrentTheme = MainUser.SavedThemes.Single(x => x.ThemeName == MainUser.MainTheme).Cloning();
            Constants.Logger.Info($"Loading {MainUser.UserName}'s Data");
            UserName = MainUser.UserName;
            Collection = MainUser.UserCollection;
            ThemeSettingsViewModel.UserThemes = new ObservableCollection<TsundokuTheme>(MainUser.SavedThemes.OrderBy(theme => theme.ThemeName).ToList());
            CurLanguage = MainUser.CurLanguage;
            CurDisplay = MainUser.Display;
            CurCurrency = MainUser.Currency;
            LanguageIndex = Array.IndexOf(Constants.AvailableLanguages, CurLanguage);
            UserIcon = new Bitmap(new MemoryStream(MainUser.UserIcon)).CreateScaledBitmap(new Avalonia.PixelSize(Constants.USER_ICON_WIDTH, Constants.USER_ICON_HEIGHT), BitmapInterpolationMode.HighQuality);

            // Pre allocate the bitmaps for every image so it is not remade every pass.
            foreach (Series x in Collection)
            {
                // If the image does not exist in the covers folder then don't create a bitmap for it
                if (File.Exists(x.Cover))
                {
                    x.CoverBitMap = new Bitmap(x.Cover).CreateScaledBitmap(new Avalonia.PixelSize(Constants.LEFT_SIDE_CARD_WIDTH, Constants.IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                }
                SearchedCollection.Add(x);
            }

            if (save) { SaveUsersData(); UpdateDefaultTheme(); }
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
                    Constants.Logger.Info("Updated Default Theme");
                    break;
                }
            }
        }

        // TODO: Need to update Currency as well prob?
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
        private static bool VersionUpdate()
        {
            string json = File.ReadAllText(filePath);
            JsonNode userData = JsonNode.Parse(json);
            JsonArray collectionJsonArray = userData["UserCollection"].AsArray();
            JsonNode series;
            bool updatedVersion = false;
            
            // For users who did not get the older update
            if (!userData.AsObject().ContainsKey("CurDataVersion"))
            {
                userData.AsObject().Add("CurDataVersion", "1.0");
                Constants.Logger.Info("Added CurDataVersion Data");
            }

            // 1.5 Data Update
            double curVersion = double.Parse(userData["CurDataVersion"].ToString());
            if (curVersion < 1.5)
            {
                userData.AsObject().Add("Currency", "$");
                userData.AsObject().Add("MeanScore", 0);
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
                Constants.Logger.Info("Updated Users Data to v1.5");
                updatedVersion = true;        
            }

            // Update to 1.6 schema
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
                Constants.Logger.Info("Updated Users Data to v1.6");
                updatedVersion = true;  
            }

            // if (curVersion < 1.7)
            // {
            //     // userData.AsObject().Add("Icon", "");
            //     // userData["CurDataVersion"] = 1.6;
            //     updatedVersion = true;
            //     Constants.Logger.Info("Updated Users Data to v1.7");
            // }

            File.WriteAllText(filePath, JsonSerializer.Serialize(userData, options));
            return updatedVersion;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before serialization")]
        public static void SaveUsersData()
        {
            Constants.Logger.Info($"Saving {MainUser.UserName}'s Data");
            MainUser.UserCollection = Collection;
            MainUser.SavedThemes = ThemeSettingsViewModel.UserThemes;

            File.WriteAllText(filePath, JsonSerializer.Serialize(MainUser, options));
        }
    }
}