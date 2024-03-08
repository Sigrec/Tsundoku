using ReactiveUI;
using Tsundoku.Models;
using System.Collections.ObjectModel;
using Tsundoku.Views;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Media.Imaging;
using DynamicData;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Linq.Dynamic.Core;
using System.Windows.Input;
using MangaAndLightNovelWebScrape.Websites;
using Avalonia.Collections;
using System.Reflection;
using Src.Helpers;

namespace Tsundoku.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public const string USER_DATA_FILEPATH = @"UserData.json";
        private static bool CanFilter = true;
        private static bool UpdatedCovers = false;
        public static AvaloniaList<Series> SearchedCollection { get; set; } = [];
        public static List<Series> UserCollection { get; set; } = [];
        private static IEnumerable<Series> FilteredCollection { get; set; } = [];
        [Reactive] public string SearchText { get; set; }
        [Reactive] public string AdvancedSearchText { get; set; } = string.Empty;
        [Reactive] public bool LanguageChanged { get; set; } = false;
        [Reactive] public Bitmap? UserIcon { get; set; }
        [Reactive] public string CurLanguage { get; set; }
        [Reactive] public int LanguageIndex { get; set; }
        [Reactive] public int FilterIndex { get; set; }
        public static bool SearchIsBusy { get; set; } = false;
        public static string CurSearchText;
        [Reactive] public string AdvancedSearchQueryErrorMessage { get; set; }

        public static AddNewSeriesWindow newSeriesWindow;
        public ReactiveCommand<Unit, Unit> OpenAddNewSeriesWindow { get; set; }

        public static SettingsWindow settingsWindow;
        public ReactiveCommand<Unit, Unit> OpenSettingsWindow { get; set; }

        public static CollectionThemeWindow themeSettingsWindow;
        public ReactiveCommand<Unit, Unit> OpenThemeSettingsWindow { get; set; }

        public static PriceAnalysisWindow priceAnalysisWindow;
        public ReactiveCommand<Unit, Unit> OpenPriceAnalysisWindow { get; set; }

        public static CollectionStatsWindow collectionStatsWindow;
        public ReactiveCommand<Unit, Unit> OpenCollectionStatsWindow { get; set; }
        public ICommand StartAdvancedSearch { get; }

        public static readonly HttpClient AddCoverHttpClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        })
        {
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        [GeneratedRegex(@"(\w+)(==|<=|>=)(\d+|\w+|\'(?:.*?)\')")] public static partial Regex AdvancedQueryRegex();
        
        // TODO Add genres?
        // TODO Manual Entry Option?
        public MainWindowViewModel()
        {
            // Helpers.ExtensionMethods.PrintCultures();
            // Helpers.ExtensionMethods.PrintCurrencySymbols();
            LoadUserData();
            ConfigureWindows();
            AddCoverHttpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

            this.WhenAnyValue(x => x.CurLanguage).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.CurLanguage = x);
            this.WhenAnyValue(x => x.CurLanguage).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => LanguageIndex = AVAILABLE_LANGUAGES.IndexOf(x));
            this.WhenAnyValue(x => x.CurFilter).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => FilterIndex = AVAILABLE_COLLECTION_FILTERS.IndexOf(x));
            this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(600)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(SearchCollection);
            this.WhenAnyValue(x => x.SearchText).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => CurSearchText = x);
            this.WhenAnyValue(x => x.CurrentTheme).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.MainTheme = x.ThemeName);
            this.WhenAnyValue(x => x.CurDisplay).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.Display = x);
            this.WhenAnyValue(x => x.UserName).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.UserName = x);
            this.WhenAnyValue(x => x.UserIcon).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.UserIcon = User.ImageToByteArray(x));

            StartAdvancedSearch = ReactiveCommand.Create(() => AdvancedSearchCollection(AdvancedSearchText));
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
            priceAnalysisWindow.PriceAnalysisVM.CurRegion = MainUser.Region;
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

        public void UpdateCurFilter(ComboBoxItem filterBoxItem)
        {
            CurFilter = filterBoxItem.Content.ToString();
            FilterCollection(CurFilter);
        }

        public string GetFilter()
        {
            return CurFilter;
        }

        public static void UserIsSearching(bool value)
        {
            SearchIsBusy = value;
        }

        /// <summary>
        /// Filters the users collection based on the selected preset filter
        /// </summary>
        /// <param name="filter">The filter preset chosen</param>
        public void FilterCollection(string filter)
        {
            if (CanFilter)
            {
                if (!string.IsNullOrWhiteSpace(SearchText)) // Checks if the user is filtering after a search
                {
                    SearchIsBusy = false;
                    SearchText = string.Empty;
                }
                switch (filter)
                {
                    case "Ongoing":
                        FilteredCollection = UserCollection.Where(series => series.Status == Status.Ongoing);
                        LOGGER.Info($"Filtered Collection by {filter}");
                        break;
                    case "Finished":
                        FilteredCollection = UserCollection.Where(series => series.Status == Status.Finished);
                        LOGGER.Info($"Filtered Collection by {filter}");
                        break;
                    case "Hiatus":
                        FilteredCollection = UserCollection.Where(series => series.Status == Status.Hiatus);
                        LOGGER.Info($"Filtered Collection by {filter}");
                        break;
                    case "Cancelled":
                        FilteredCollection = UserCollection.Where(series => series.Status == Status.Cancelled);
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
                        FilteredCollection = UserCollection.Where(series => series.Format != Format.Novel);
                        LOGGER.Info($"Filtered Collection by {filter}");
                        break;
                    case "Novel":
                        FilteredCollection = UserCollection.Where(series => series.Format == Format.Novel);
                        LOGGER.Info($"Filtered Collection by {filter}");
                        break;
                    case "Shounen":
                        FilteredCollection = UserCollection.Where(series => series.Demographic == Demographic.Shounen);
                        LOGGER.Info($"Filtered Collection by {filter}");
                        break;
                    case "Shoujo":
                        FilteredCollection = UserCollection.Where(series => series.Demographic == Demographic.Shoujo);
                        LOGGER.Info($"Filtered Collection by {filter}");
                        break;
                    case "Seinen":
                        FilteredCollection = UserCollection.Where(series => series.Demographic == Demographic.Seinen);
                        LOGGER.Info($"Filtered Collection by {filter}");
                        break;
                    case "Josei":
                        FilteredCollection = UserCollection.Where(series => series.Demographic == Demographic.Josei);
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
                    case "Query":
                        return;
                    case "None":
                    default:
                        FilteredCollection = UserCollection;
                        LOGGER.Info($"Removing Sort/Filter");
                        break;
                }
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
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (!CurFilter.Equals("None"))
                {
                    CanFilter = false;
                    CurFilter = "None";
                }

                await Task.Run(() => {
                    FilteredCollection = UserCollection.Where(x => x.Titles.Values.AsParallel().Any(title => title.Contains(searchText, StringComparison.OrdinalIgnoreCase)) || x.Staff.Values.AsParallel().Any(staff => staff.Contains(searchText, StringComparison.OrdinalIgnoreCase))); 
                });

                SearchedCollection.Clear();
                SearchedCollection.AddRange(FilteredCollection);
                CanFilter = true;
            }
            else if (SearchIsBusy)
            {
                SearchIsBusy = false;
                SearchedCollection.Clear();
                SearchedCollection.AddRange(UserCollection);
            }
        }

        // Basic Test Query Demographic==Seinen Format==Manga Series==Complete Favorite==True
        public async void AdvancedSearchCollection(string AdvancedSearchQuery)
        {
            if (!string.IsNullOrWhiteSpace(AdvancedSearchQuery) && AdvancedQueryRegex().IsMatch(AdvancedSearchQuery))
            {
                AdvancedSearchQuery = AdvancedSearchQuery.Trim();
                if (!CurFilter.Equals("Query"))
                {
                    CanFilter = false;
                }

                await Task.Run(() => 
                {
                    var AdvancedSearchMatches = AdvancedQueryRegex().Matches(AdvancedSearchQuery.Trim()).OrderBy(x => x.Value);
                    GroupCollection advFilter;

                    StringBuilder AdvancedFilterExpression = new StringBuilder();
                    AdvancedFilterExpression.Append("series => ");
                    foreach (Match SearchFilter in AdvancedSearchMatches)
                    {
                        advFilter = SearchFilter.Groups;
                        AdvancedFilterExpression.AppendFormat("{0} && ", ParseAdvancedFilter(advFilter[1].Value, advFilter[2].Value, advFilter[3].Value));
                    }
                    AdvancedFilterExpression.Remove(AdvancedFilterExpression.Length - 4, 4);
                    LOGGER.Info($" Initial Query = \"{AdvancedSearchQuery}\" -> \"{AdvancedFilterExpression}\"");
                    try
                    {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                        FilteredCollection = UserCollection.AsQueryable().Where(AdvancedFilterExpression.ToString());
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    }
                    catch (Exception)
                    {
                        AdvancedSearchQueryErrorMessage = "Incorrectly Formatted Advanced Search Query!";
                        LOGGER.Warn("User Inputted Incorrectly Formatted Advanced Search Query");
                    }
                    finally
                    {
                        AdvancedFilterExpression.Clear();
                    }
                });

                if (FilteredCollection.Any())
                {
                    if (!CurFilter.Equals("Query"))
                    {
                        CurFilter = "Query";
                    }
                    SearchedCollection.Clear();
                    SearchedCollection.AddRange(FilteredCollection);
                    LOGGER.Info($"Valid Advanced Search Query");
                }
                else
                {
                    AdvancedSearchQueryErrorMessage = "Advanced Search Query Returned No Series!";
                    LOGGER.Info("Advanced Search Query Returned No Series");
                }
            }
            else
            {
                AdvancedSearchQueryErrorMessage = "Inputted Incorrectly Formatted Advanced Search Query!";
                LOGGER.Warn("User Inputted Incorrectly Formatted Advanced Search Query");
            }
            CanFilter = true;
        }

        private static string ParseAdvancedFilter(string filterName, string logicType, string filterValue)
        {
            logicType = logicType.Equals("=") ? "==" : logicType;
            return filterName switch
            {
                "Rating" or "Cost" => $"series.{filterName} {logicType} {filterValue}M",
                "Read" => $"series.VolumesRead {logicType} {filterValue}",
                "CurVolumes" => $"series.CurVolumeCount {logicType} {filterValue}",
                "MaxVolumes" => $"series.MaxVolumeCount {logicType} {filterValue}",
                "Format" or "Status" or "Demographic" => $"series.{filterName} == {filterName}.{filterValue}",
                "Series" => $"series.MaxVolumeCount {(filterValue.Equals("Complete") ? "==" : "<")} series.CurVolumeCount",
                "Favorite" => $"{(filterValue.Equals("True") ? '!' : "")}series.IsFavorite",
                "Notes" => $"(!string.IsNullOrWhiteSpace(series.SeriesNotes) && series.SeriesNotes.Contains(\"{filterValue[1..^1]}\"))",
                _ => string.Empty,
            };
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before deserialization")]
        /// <summary>
        /// Gets the users data from the UserData.Json file in current working directory where the executable is
        /// </summary>
        private void LoadUserData()
        {
            LOGGER.Info("Starting Tsundoku");
            if (!File.Exists(USER_DATA_FILEPATH))
            {
                LOGGER.Info("Creating New User");
                MainUser = new User(
                            "UserName", 
                            "Romaji", 
                            "Default", 
                            "Card", 
                            SCHEMA_VERSION, 
                            "$", 
                            "$0.00",
                            Region.America,
                            new Dictionary<string, bool>
                            {
                                { BarnesAndNoble.WEBSITE_TITLE , false },
                                { BooksAMillion.WEBSITE_TITLE, false },
                                { Indigo.WEBSITE_TITLE, false },
                                { KinokuniyaUSA.WEBSITE_TITLE , false }
                            }, 
                            [], 
                            UserCollection
                        );
                UserName = MainUser.UserName;
                UserCollection = MainUser.UserCollection;
                CurLanguage = MainUser.CurLanguage;
                CurDisplay = MainUser.Display;
                MainUser.UserIcon = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAEcAAABHCAYAAABVsFofAAAABHNCSVQICAgIfAhkiAAACERJREFUeJztm3uMXFUdxz/n7sx2d9t9tNttd9sCsqUPMFmkUBGolSJKNNFCMCrWxABqmv6j9g/1D2PRSAwKjRI1KpKCoaYk9REjEWNJqDG0JE2X0jaUhUJ3u7ul3Ue3s/O6z+Mfd+bO3Nk7e8+0dx9T55vczNx7fr/f+Z3f+Z3feUMNNdRQQw01zHcIBZpGYDuwBdAAWYbPBv4J3AKsDJF5HugFPgnEi76XynaAg8DvgKSCrrOOv+EqPZfP/hkvZQBiIekasAHg5ztuZ911bYUUKSFz3v0Fjp6e5LG970PHarh7R4FGCDANhJ4psB7eAxfP8vXO1Xy8rcOX4YSexc7JHDF1fjp0CqDnSgp5uQgzDrgGYnNPFxtvLCqIlMiUDdIBoHGB5n5vWgyr7/JL0DOITKrA+safAbh5URv3Lu70kX6QTnrGGTIyCEDmdJhthBlHJSbNBmLANSE0BjAKLAfqQmgngEmVTKeDDBMwS7gOGAihcYAzwPWEV+o48BXgX9MRVYnnCNBCnEHaGlJ2IwRoJWqXVrHjLAGeB1bgGjUQ6p4jmGIqQeGbKE4Twq+QCGAOFolAIHLMXlprJzz8vBffQIBloqUSHp/z2rPQ9writh60T/hjnpPKIG3bfTEt+OM+sJ2mQKWKoBKQAXjwB/+mob6k9hzT+5s2cooPHYffPuinkxJHFlVQagyAx/pP8OTgKR+pLZ18B4iNdM0kNGhoxWdxy3RHVl5J6nO/cWhqLNFTgpUjrjNRhbJxhkbT05rZU9vSERND5dPB6/7HLINxywiXqYRKqMWUP0FQjjl/ee4Jbr7pBr8y6WHP1Q8d7WPbzl+zat0G7v/2L326JjIWKb1QzQd//x1GzxznqR99i/s/s9mfY/ocSAuAgeExtmz78Qz0CmoSlWPOimXtfOiarqIUCSnTM857A+ddgfUNtHas8gtJmYhswTh1cbcJdLS3+WUCJC3POLYzt51ldIOredKvVYhpra8cc7Z/9wlaFi0sdEtSgqN78WMi4Y6AP3jvBC/segiZ+y4EmLb0ecH4YB8Aj/9iD3/Y+/eCPCHAyno6Z3TDC87RQq0mlWNO7/E+JYHZVIL+k4eVaE+908+pd/qVaOcCyjHnJ9/YyOoVzf4U/YLnOScGkjy+rx+WdsNdj/ilmAbCyBZYj/wJJgZ5eHk3G1vafaSXjCxOTua4ZbB7uG/eBmTPcz5168qAiafwAvLSY3HXOAuXwI33+qWUTjxPvgQTg2xoXszWpf6ln9KJ5+5hNY+tDGpdeVhAzq+nXGWQU/4EIcw4weP+2UbkGqgJVPGcuUfkWqgJVPGcqxBXk+coIfp6nJPlx4pRUbmjq8/qMM48jTkSSMZjGm3N9Ves0mVDyXPyBY6ueakY556DT3/u/TWrWiPLtGJE7jnRBGSA4Y/dtCwRTlZNiKZZVSGiczOVudVzt37zr+sP7P4si5sXRJZxRXAsGDuDb/3CtpDpoq0nPTd3S2eRo+M+dpnKgm27pTEtZfupGOee3r7RBe8OJdi4viOEfIaQOA/PfGnK56AyymMnkcdORpKtinFEWU1mCXWxuLv0Kgp6SCS2g3cuI5sYw8hMwqKFaK2LfPzSdgq7OkgYGUNlFa0qdjxb2rvY8atXvdVFAMN0uJAo7Fwc2ruLvv+8SOy+TdQ/+mUfvz4yiZPNbcnoOnLnD8GyQvOtkh1PcjtYsux7AQEqX+Zaq4rnzLn3JCdG2P+z7T5VHAd0q7CjMX72LQDs146inxvx8Tu6icyvYdu2+yhAPebMIUw9w9uvv6xEK89dwD53IZJ8qyLm0NAMG77gbx7SQRh64XXgCIyeRqzsQnRf62N3DMt1NQDbgd43IwnI8wONLbDZ36ywTETykvcqswkYPQ3XrkBs2eRjF8k0Mt8ETROOnVBqWtUxQq7If2dv4jnn8UYd0UcA9d4q5HxOQErJe7CdpzufoyY34H9olaoZUgCdwE7c88NBYjcBC25b30FLU717LknmMrCziNyI42LSovf0pBs8l68r0cXxt/HRd0FPsbaxmWX1DT5Sw/ZO5aBLhzdTlyC2AFb24A2Hka4SdtFAbmLQPffT2oJY0uaTKW2nEIAdB4bPgcQGXg0or8Q9I/0UwEvM3Pnhan7+IQC7Plan7drqRvgg15EVfs+nRSVvuu/l8lCVl9QNjFxPlneupw8cwbQdJwZodUKwtrPdN3f5f0EikyVjFI7CCQGaJsBGq46ufI5QM04A8k2uZpwA5INLzTjToGacaeCNkKWUVdNbCUSZNa3K9Zey/ARdAMMCujpamtxMqwR3rlkpH7r9ww7AaDIjdr/8ujaZLX/guxycohPz4Mab8WQG6dqFbcBvgKYy/DEArehyg5T5AT4O01ysmEnE6sThFx/99BcBdu4/fGf/+KUXmDpX1HKPT3+nYI1yC8kpYEeeox5YGEBUB5zWhGjZcXcPDXE371feGqD37AjAM8D3KitWZEgCxRcZFuG/LwrwCPDk8uYmvnrHeq9lPPvfk1xMZ21gDe7dq1KkACNvaSP3lCIGSAE0xOM0xN2LIXWFsaMBXKywUDOFoAuyaQChCRrjrt0EvhtHE0yjv9J6jjsTK135rwrk9tyKdVdHmHEcwHSkZDyV9YSPJr0zxfPFa8phHCBjWl6cSRsmSd0Et0nq5VnDIYADgGxprJef/0i3/Oj1nVK4q1EmcMeVCJ8FLMVtOnLNsjb5wC03yK7WhfklibeJYJzXhHu/snS94/tXKniWsJWpuieBtWGMqgObJcADwH3ABWAP8Ab+u3LzFQK3V/oasB44BOwDBudSqRpqqKGGGmqIBv8DdoKZCGn1FckAAAAASUVORK5CYII=");
                SaveUsersData();
            }

            JsonNode userData = JsonNode.Parse(File.ReadAllText(USER_DATA_FILEPATH));
            // JsonNode userData = JsonNode.Parse(File.ReadAllText(USER_DATA_FILEPATH));

            // userData["CurDataVersion"].GetValue<double>() == 0 ||
            if (userData["CurDataVersion"] == null || userData["CurDataVersion"].GetValue<double>() < SCHEMA_VERSION)
            {
                VersionUpdate(userData, false);
                updatedVersion = false;
            }
            
            MainUser = JsonSerializer.Deserialize(userData, typeof(User), User.UserJsonModel) as User;
            MainUser.SavedThemes.Add(TsundokuTheme.DEFAULT_THEME);
            MainUser.SavedThemes = new ObservableCollection<TsundokuTheme>(MainUser.SavedThemes.OrderBy(theme => theme.ThemeName));

            LOGGER.Info($"Loading {MainUser.UserName}'s Data");
            UserName = MainUser.UserName;
            UserCollection = MainUser.UserCollection;
            CurLanguage = MainUser.CurLanguage;
            CurDisplay = MainUser.Display;
            CurCurrency = MainUser.Currency;
            LanguageIndex = Array.IndexOf(AVAILABLE_LANGUAGES, CurLanguage);
            CurrentTheme = !MainUser.MainTheme.Equals("Default") ? MainUser.SavedThemes.Single(x => x.ThemeName == MainUser.MainTheme).Cloning() : TsundokuTheme.DEFAULT_THEME;
            
            if (MainUser.UserIcon != null && MainUser.UserIcon.Length > 0)
            {
                MemoryStream iconByteStream = new MemoryStream(MainUser.UserIcon);
                UserIcon = new Bitmap(iconByteStream).CreateScaledBitmap(new PixelSize(USER_ICON_WIDTH, USER_ICON_HEIGHT), BitmapInterpolationMode.HighQuality);
                iconByteStream.Flush();
                iconByteStream.Close();
            }

            // Pre allocate the bitmaps for every image so it is not remade every pass.
            foreach (Series x in UserCollection)
            {
                // If the image does not exist in the covers folder then don't create a bitmap for it
                string newCoverPath = x.Cover[..^3] + (x.Cover[^3..].Equals("jpg") ? "png" : "jpg");
                bool coverCheck = File.Exists(x.Cover);
                bool secondCoverCheck = File.Exists(newCoverPath);
                if (secondCoverCheck && coverCheck)
                {
                    File.Delete(newCoverPath);
                    LOGGER.Info("Removed {} File", newCoverPath);
                }

                if(coverCheck || secondCoverCheck)
                {
                    coverCheck = File.Exists(x.Cover);
                    Bitmap loadedBitmap = new Bitmap(coverCheck ? x.Cover : newCoverPath);
                    if (!UpdatedCovers && loadedBitmap.Size.Width != LEFT_SIDE_CARD_WIDTH && loadedBitmap.Size.Height != IMAGE_HEIGHT)
                    {
                        x.CoverBitMap = loadedBitmap.CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                        x.CoverBitMap.Save(coverCheck ? x.Cover : newCoverPath, 100);
                        LOGGER.Debug("Scaling {} Cover Image", x.Titles["Romaji"]);
                    }
                    else
                    {
                        x.CoverBitMap = loadedBitmap;
                    }
                    x.Cover = coverCheck ? x.Cover : newCoverPath;
                }
                SearchedCollection.Add(x);
            }
        }

        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
        public static bool VersionUpdate(JsonNode userData, bool isImport)
        {
            JsonNode series;
            JsonNode theme;
            JsonArray collectionJsonArray = userData[nameof(UserCollection)].AsArray();
            JsonArray themeJsonArray = userData["SavedThemes"].AsArray();
            
            // For users who did not get the older update
            if (!userData.AsObject().ContainsKey("CurDataVersion"))
            {
                userData.AsObject().Add("CurDataVersion", "1.0");
            }
            double curVersion = double.Parse(userData["CurDataVersion"].ToString());

            if (curVersion < 1.5) // 1.5 Version Upgrade changes Series Title, Staff. & Status to accomadte new changes & adds Stats related variables to Series and also changes Native to Japanese
            {
                userData.AsObject().Add("Currency", "$");
                userData.AsObject().Add("MeanRating", 0);
                userData.AsObject().Add("VolumesRead", 0);
                userData.AsObject().Add("CollectionPrice", "");

                if (userData[nameof(CurLanguage)].ToString().Equals("Native"))
                {
                    userData[nameof(CurLanguage)] = "Japanese";
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
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v1.5", !isImport);
                updatedVersion = true;        
            }

            if (curVersion < 1.6) // 1.6 Version Upgrade fixes scores defaulting to 0 to -1 instead so it doesn't show up i stats
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
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v1.6", !isImport);
                updatedVersion = true;  
            }

            if (curVersion < 1.7) // 1.7 Version Upgrade To accomdate adding user icon
            {
                string defaultFileByte = "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";

                userData.AsObject().Add(nameof(UserIcon), defaultFileByte);
                userData["CurDataVersion"] = 1.7;
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v1.7", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 1.8) // 1.8 Version Upgrade to rename button color identifiers for TsundokuTheme change
            {   
                for (int x = 0; x < themeJsonArray.Count; x++)
                {
                    theme = themeJsonArray.ElementAt(x).AsObject();
                    theme["SeriesButtonBGColor"] = (uint)theme["SeriesSwitchPaneButtonBGColor"];
                    theme["SeriesButtonBGHoverColor"] = (uint)theme["SeriesSwitchPaneButtonBGHoverColor"];
                    theme["SeriesButtonIconColor"] = (uint)theme["SeriesSwitchPaneButtonIconColor"];
                    theme["SeriesButtonIconHoverColor"] = (uint)theme["SeriesSwitchPaneButtonIconHoverColor"];
                }

                userData["CurDataVersion"] = 1.8;
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v1.8", !isImport);
                updatedVersion = true;  
            }

            if (curVersion < 1.9) // 1.9 Version Upgrade resizes all bitmaps in the users cover folder
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
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v1.9", !isImport);
                UpdatedCovers = true;
                updatedVersion = true;
            }

            if (curVersion < 2.0) // 2.0 Verion Upgrade adds Memberships for Price Analysis and changes "Score" to "Rating"
            {
                userData["Memberships"] = new JsonObject
                {
                    [BarnesAndNoble.WEBSITE_TITLE] = false,
                    [BooksAMillion.WEBSITE_TITLE] = false,
                    [KinokuniyaUSA.WEBSITE_TITLE] = false
                };

                userData.AsObject()["MeanRating"] = (decimal)userData["MeanScore"];
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    series["Rating"] = decimal.Parse(series["Score"].ToString());
                }
                userData.AsObject().Remove("MeanScore");

                userData["CurDataVersion"] = 2.0;
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v2.0", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 2.1) // 2.1 Version updates empty demographics from null or empty string to "Unknown" & removes Default thme from file
            {
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    if (string.IsNullOrWhiteSpace(series["Demographic"]?.ToString()))
                    {
                        series["Demographic"] = Demographic.Unknown.ToString();
                    }
                }
                JsonArray themes = userData["SavedThemes"].AsArray();
                for (int x = 0; x < themes.Count; x++)
                {
                    if (themes.ElementAt(x)["ThemeName"].ToString().Equals("Default"))
                    {   
                        themes.RemoveAt(x);
                    }
                }
                userData["CurDataVersion"] = 2.1;
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v2.1", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 2.2)
            {
                userData["Memberships"].AsObject().Remove("RightStufAnime");
                userData["Memberships"].AsObject().Add(new KeyValuePair<string, JsonNode?>(Indigo.WEBSITE_TITLE, false));

                userData["CurDataVersion"] = 2.2;
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v2.2", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 2.4) // Add Region property to User
            {
                userData.AsObject().Add(new KeyValuePair<string, JsonNode?>("Region", Region.America.ToString()));
                userData["CurDataVersion"] = 2.4;
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v2.4", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 2.5)
            {
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    string[] coverFileName = series["Cover"].ToString().Split('_');
                    string format = series["Format"].ToString().ToUpper();
                    string oldCoverFileName = series["Cover"].ToString();
                    if (!coverFileName[1].StartsWith(format) && File.Exists(oldCoverFileName))
                    {
                        series["Cover"] = $@"{coverFileName[0]}_{format}.{oldCoverFileName.Substring(oldCoverFileName.Length - 3)}";
                        File.Move(oldCoverFileName, series["Cover"].ToString());
                        LOGGER.Debug("Updated {} Cover File Name", series["Titles"]["Romaji"].ToString());
                    }
                }
                userData["CurDataVersion"] = 2.5;
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v2.5", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 3.0) // Update the colors in themes to hex string
            {
                for (int x = 0; x < themeJsonArray.Count; x++)
                {
                    theme = themeJsonArray.ElementAt(x).AsObject();
                    foreach (PropertyInfo property in typeof(TsundokuTheme).GetProperties().Skip(1))
                    {
                        theme[property.Name] = Avalonia.Media.Color.FromUInt32(uint.Parse(theme[property.Name].ToString())).ToString();
                    }
                }
                userData["CurDataVersion"] = 3.0;
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v3.0", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 4.0) // Remove "SeriesButtonBGColor" & SeriesButtonBGHoverColor" Colors from Themes
            {
                for (int x = 0; x < themeJsonArray.Count; x++)
                {
                    themeJsonArray.ElementAt(x).AsObject().Remove("SeriesButtonBGColor");
                    themeJsonArray.ElementAt(x).AsObject().Remove("SeriesButtonBGHoverColor");
                }
                userData["CurDataVersion"] = 4.0;
                TsundokuLogger.Info(LOGGER, "Updated Users Data to v4.0", !isImport);
                updatedVersion = true;
            }

            if (!isImport)
            {
                File.WriteAllText(USER_DATA_FILEPATH, JsonSerializer.Serialize(userData, new JsonSerializerOptions()
                { 
                    WriteIndented = true,
                    ReadCommentHandling = JsonCommentHandling.Disallow,
                    AllowTrailingCommas = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }));
            }
            return updatedVersion;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "A New file will always be created if it doesn't exist before serialization")]
        public static void SaveUsersData()
        {
            LOGGER.Info($"Saving \"{MainUser?.UserName}'s\" Collection Data");
            MainUser.UserCollection = UserCollection;
            MainUser.SavedThemes.Remove(TsundokuTheme.DEFAULT_THEME);
    
            File.WriteAllText(USER_DATA_FILEPATH, JsonSerializer.Serialize(MainUser, typeof(User), User.UserJsonModel));
        }

        /// <summary>
        /// Changes the cover for series
        /// </summary>
        /// <param name="series">The series to change the cover for</param>
        /// <param name="newBitmapPath">The path to the new cover</param>
        public static void ChangeCover(Series series, string newBitmapPath) {
            Bitmap newCover = new Bitmap(newBitmapPath).CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
            newCover.Save(series.Cover, 100);

            // Update Current Viewed Collection
            series.CoverBitMap = newCover;
            int index = SearchedCollection.IndexOf(series);
            SearchedCollection.Remove(series);
            SearchedCollection.Insert(index, series);
            LOGGER.Info("Updated Cover for {}", series.Titles["Romaji"]);
        }

        public static void DeleteCover(Series series)
        {
            if (File.Exists(series.Cover))
            {
                File.SetAttributes(series.Cover, FileAttributes.Normal);
                File.Delete(series.Cover);
                series.Dispose();
            }
        }

        public static void DeleteSeries(Series series)
        {
            DeleteCover(series);
            SearchedCollection.Remove(series);
            UserCollection.Remove(series);
            collectionStatsWindow.CollectionStatsVM.UpdateAllStats(series.CurVolumeCount, (uint)(series.MaxVolumeCount - series.CurVolumeCount));
            LOGGER.Info("Removed {} From Collection", series.Titles["Romaji"]);
        }
    }
}