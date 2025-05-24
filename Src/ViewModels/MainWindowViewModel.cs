using ReactiveUI;
using Tsundoku.Models;
using Tsundoku.Views;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
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
using Tsundoku.Helpers;

namespace Tsundoku.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private static bool CanFilter = true;
        private static bool UpdatedCovers = false;
        public static AvaloniaList<Series> SearchedCollection { get; set; } = [];
        public static List<Series> UserCollection { get; set; } = [];
        private static IEnumerable<Series> FilteredCollection { get; set; }
        [Reactive] public string SearchText { get; set; }
        [Reactive] public string NotificationText { get; set; }

        [Reactive] public string AdvancedSearchText { get; set; } = string.Empty;
        [Reactive] public bool LanguageChanged { get; set; } = false;
        [Reactive] public Bitmap? UserIcon { get; set; }
        [Reactive] public int LanguageIndex { get; set; }
        [Reactive] public int FilterIndex { get; set; }
        public static bool SearchIsBusy { get; set; } = false;
        public static string CurSearchText;
        [Reactive] public string AdvancedSearchQueryErrorMessage { get; set; }

        public static AddNewSeriesWindow newSeriesWindow;
        public static SettingsWindow settingsWindow;
        public static CollectionThemeWindow themeSettingsWindow;
        public static PriceAnalysisWindow priceAnalysisWindow;
        public static CollectionStatsWindow collectionStatsWindow;
        public static UserNotesWindow userNotesWindow;

        public ICommand StartAdvancedSearch { get; }
        public Interaction<EditSeriesInfoViewModel, MainWindowViewModel?> EditSeriesInfoDialog { get; }

        public static readonly HttpClient AddCoverHttpClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        })
        {
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        [GeneratedRegex(@"(\w+)(==|<=|>=)(\d+|\w+|(?:'|"")(?:.*?)(?:'|""))")] public static partial Regex AdvancedQueryRegex();
        
        public MainWindowViewModel()
        {
            LoadUserData();
            ConfigureWindows();
            AddCoverHttpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

            this.WhenAnyValue(x => x.CurLanguage).ObserveOn(RxApp.MainThreadScheduler).Subscribe(LanguageChangedUpdate);
            this.WhenAnyValue(x => x.CurFilter).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => FilterIndex = AVAILABLE_COLLECTION_FILTERS.IndexOf(x.GetStringValue()));

            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(600))
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(SearchCollection);

            this.WhenAnyValue(x => x.SearchText).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => CurSearchText = x);
            
            this.WhenAnyValue(x => x.UserName).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.UserName = x);
            this.WhenAnyValue(x => x.UserIcon).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.UserIcon = User.ImageToByteArray(x));

            StartAdvancedSearch = ReactiveCommand.Create(() => AdvancedSearchCollection(AdvancedSearchText));
            
            EditSeriesInfoDialog = new Interaction<EditSeriesInfoViewModel, MainWindowViewModel?>();
        }

        private void LanguageChangedUpdate(string lang)
        {
            MainUser.CurLanguage = lang;
            LanguageIndex = AVAILABLE_LANGUAGES.IndexOf(lang);
        }

        /// <summary>
        /// Configures the various windows in the app
        /// </summary>
        private void ConfigureWindows()
        {
            LOGGER.Info("Configuring Windows...");
            newSeriesWindow = new AddNewSeriesWindow();
            settingsWindow = new SettingsWindow();
            themeSettingsWindow = new CollectionThemeWindow(); ;
            priceAnalysisWindow = new PriceAnalysisWindow();
            priceAnalysisWindow.ViewModel.CurRegion = MainUser.Region;
            collectionStatsWindow = new CollectionStatsWindow();
            userNotesWindow = new UserNotesWindow();
            LOGGER.Info("Finished Configuring Windows!");
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

        public void UpdateCurFilter(ComboBoxItem filterBoxItem, bool shouldFilter = true)
        {
            CurFilter = TsundokuFilterExtensions.GetFilterFromString(filterBoxItem.Content.ToString());
            if (shouldFilter)
            {
                FilterCollection(CurFilter);
            }
        }

        public static void UserIsSearching(bool value)
        {
            SearchIsBusy = value;
        }

        /// <summary>
        /// Filters the users collection based on the selected preset filter
        /// </summary>
        /// <param name="filter">The filter preset chosen</param>
        public void FilterCollection(TsundokuFilter filter)
        {
            if (CanFilter)
            {
                if (!string.IsNullOrWhiteSpace(SearchText)) // Checks if the user is filtering after a search
                {
                    SearchIsBusy = false;
                    SearchText = string.Empty;
                }
                LOGGER.Info(filter);
                switch (filter)
                {
                    case TsundokuFilter.Ongoing:
                        FilteredCollection = UserCollection.Where(series => series.Status == Status.Ongoing);
                        break;
                    case TsundokuFilter.Finished:
                        FilteredCollection = UserCollection.Where(series => series.Status == Status.Finished);
                        break;
                    case TsundokuFilter.Hiatus:
                        FilteredCollection = UserCollection.Where(series => series.Status == Status.Hiatus);
                        break;
                    case TsundokuFilter.Cancelled:
                        FilteredCollection = UserCollection.Where(series => series.Status == Status.Cancelled);
                        break;
                    case TsundokuFilter.Complete:
                        FilteredCollection = UserCollection.Where(series => series.CurVolumeCount == series.MaxVolumeCount);
                        break;
                    case TsundokuFilter.Incomplete:
                        FilteredCollection = UserCollection.Where(series => series.CurVolumeCount != series.MaxVolumeCount);
                        break;
                    case TsundokuFilter.Favorites:
                        FilteredCollection = UserCollection.Where(series => series.IsFavorite);
                        break;
                    case TsundokuFilter.Manga:
                        FilteredCollection = UserCollection.Where(series => series.Format != Format.Novel);
                        break;
                    case TsundokuFilter.Novel:
                        FilteredCollection = UserCollection.Where(series => series.Format == Format.Novel);
                        break;
                    case TsundokuFilter.Shounen:
                        FilteredCollection = UserCollection.Where(series => series.Demographic == Demographic.Shounen);
                        break;
                    case TsundokuFilter.Shoujo:
                        FilteredCollection = UserCollection.Where(series => series.Demographic == Demographic.Shoujo);
                        break;
                    case TsundokuFilter.Seinen:
                        FilteredCollection = UserCollection.Where(series => series.Demographic == Demographic.Seinen);
                        break;
                    case TsundokuFilter.Josei:
                        FilteredCollection = UserCollection.Where(series => series.Demographic == Demographic.Josei);
                        break;
                    case TsundokuFilter.Publisher:
                        FilteredCollection = UserCollection.OrderBy(series => series.Publisher);
                        break;
                    case TsundokuFilter.Read:
                        FilteredCollection = UserCollection.Where(series => series.VolumesRead != 0);
                        break;
                    case TsundokuFilter.Unread:
                        FilteredCollection = UserCollection.Where(series => series.VolumesRead == 0);
                        break;
                    case TsundokuFilter.Rating:
                        FilteredCollection = UserCollection.OrderByDescending(series => series.Rating);
                        break;
                    case TsundokuFilter.Value:
                        FilteredCollection = UserCollection.OrderByDescending(series => series.Value);
                        break;
                    case TsundokuFilter.Action:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Action));
                        break;
                    case TsundokuFilter.Adventure:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Adventure));
                        break;
                    case TsundokuFilter.Comedy:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Comedy));
                        break;
                    case TsundokuFilter.Drama:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Drama));
                        break;
                    case TsundokuFilter.Ecchi:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Ecchi));
                        break;
                    case TsundokuFilter.Fantasy:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Fantasy));
                        break;
                    case TsundokuFilter.Horror:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Horror));
                        break; 
                    case TsundokuFilter.MahouShoujo:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.MahouShoujo));
                        break; 
                    case TsundokuFilter.Mecha:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Mecha));
                        break; 
                    case TsundokuFilter.Music:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Music));
                        break; 
                    case TsundokuFilter.Mystery:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Mystery));
                        break; 
                    case TsundokuFilter.Psychological:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Psychological));
                        break; 
                    case TsundokuFilter.Romance:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Romance));
                        break; 
                    case TsundokuFilter.SciFi:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.SciFi));
                        break; 
                    case TsundokuFilter.SliceOfLife:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.SliceOfLife));
                        break;
                    case TsundokuFilter.Sports:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Sports));
                        break; 
                    case TsundokuFilter.Supernatural:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Supernatural));
                        break; 
                    case TsundokuFilter.Thriller:
                        FilteredCollection = UserCollection.Where(series => series.Genres != null && series.Genres.Contains(Genre.Thriller));
                        break; 
                    case TsundokuFilter.Query:
                        return;
                    case TsundokuFilter.None:
                    default:
                        FilteredCollection = UserCollection;
                        break;
                }
                LOGGER.Info($"Sorted Collection by \"{filter}\"");
                SearchedCollection.Clear();
                SearchedCollection.AddRange(FilteredCollection);
                FilteredCollection = [];
            }
        }

        /// <summary>
        /// Searches the users collection by title and/or staff
        /// </summary>
        /// <param name="searchText">The text to search for</param>
        public void SearchCollection(string searchText)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (CurFilter != TsundokuFilter.None)
                {
                    CanFilter = false;
                    CurFilter = TsundokuFilter.None;
                }

                FilteredCollection = UserCollection.Where(x =>
                    x.Publisher.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    || x.Titles.Values.Any(title => title.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    || x.Staff.Values.Any(staff => staff.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                ); 
                
                SearchedCollection.Clear();
                SearchedCollection.AddRange(FilteredCollection);
                CanFilter = true;
            }
            else if (SearchIsBusy)
            {
                SearchIsBusy = false;
                SearchedCollection.Clear();
                SearchedCollection.AddRange(UserCollection);
                CanFilter = true;
            }
        }

        // Basic Test Query Demographic==Seinen Format==Manga Series==Complete Favorite==True
        public async Task AdvancedSearchCollection(string AdvancedSearchQuery)
        {
            if (!string.IsNullOrWhiteSpace(AdvancedSearchQuery) && AdvancedQueryRegex().IsMatch(AdvancedSearchQuery))
            {
                AdvancedSearchQuery = AdvancedSearchQuery.Trim();
                if (!string.IsNullOrWhiteSpace(SearchText)) // Checks if the user searching and remove the text
                {
                    SearchIsBusy = false;
                    SearchText = string.Empty;
                }
                if (CurFilter != TsundokuFilter.Query)
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
                        AdvancedFilterExpression.AppendFormat("({0}) && ", ParseAdvancedFilter(advFilter[1].Value, advFilter[2].Value, advFilter[3].Value));
                    }
                    AdvancedFilterExpression.Remove(AdvancedFilterExpression.Length - 4, 4);
                    LOGGER.Info($"Initial Query = \"{AdvancedSearchQuery}\" -> \"{AdvancedFilterExpression}\"");
                    try
                    {
                        FilteredCollection = UserCollection.AsQueryable().Where(AdvancedFilterExpression.ToString());
                    }
                    catch (Exception ex)
                    {
                        AdvancedSearchQueryErrorMessage = "Incorrectly Formatted Advanced Search Query!";
                        LOGGER.Warn($"User Inputted Incorrectly Formatted Advanced Search Query => \"{ex.Message}\"");
                    }
                    finally
                    {
                        AdvancedFilterExpression.Clear();
                    }
                });

                if (FilteredCollection.Any())
                {
                    if (CurFilter != TsundokuFilter.Query)
                    {
                        CurFilter = TsundokuFilter.Query;
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
                "Rating" or "Value" => $"series.{filterName} {logicType} {filterValue}M",
                "Read" => $"series.VolumesRead {logicType} {filterValue}",
                "CurVolumes" => $"series.CurVolumeCount {logicType} {filterValue}",
                "MaxVolumes" => $"series.MaxVolumeCount {logicType} {filterValue}",
                "Format" or "Status" or "Demographic" => $"series.{filterName} == {filterName}.{filterValue}",
                "Series" => $"series.MaxVolumeCount {(filterValue.Equals("Complete") ? "==" : "<")} series.CurVolumeCount",
                "Favorite" => $"{(filterValue.Equals("True") ? '!' : "")}series.IsFavorite",
                "Notes" => $"!string.IsNullOrWhiteSpace(series.SeriesNotes) && series.SeriesNotes.Contains(\"{filterValue[1..^1]}\")",
                "Publisher" => $"series.Publisher.Contains({(!filterValue.StartsWith('"') ? "\"" : string.Empty)}{filterValue}{(!filterValue.EndsWith('"') ? "\"" : string.Empty)}, StringComparison.OrdinalIgnoreCase)",
                "Genre" => $"series.Genres != null && series.Genres.Contains(Tsundoku.Models.Genre.{filterValue})",
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
            if (!File.Exists(AppFileHelper.GetUserDataJsonPath()))
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
                MainUser.SavedThemes.Add(TsundokuTheme.DEFAULT_THEME);
                MainUser.UserIcon = Convert.FromBase64String("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");
                MainUser.SaveUserData();
            }

            // TODO - Check to see if Json is empty/formatted correct and throw error or make new one 
            JsonNode userData = JsonNode.Parse(File.ReadAllText(AppFileHelper.GetUserDataJsonPath()));

            // userData["CurDataVersion"].GetValue<double>() == 0 ||
            if (userData["CurDataVersion"] == null || userData["CurDataVersion"].GetValue<double>() < SCHEMA_VERSION)
            {
                User.UpdateSchemaVersion(userData, false);
                updatedVersion = false;
            }

            MainUser = JsonSerializer.Deserialize(userData, typeof(User), User.UserJsonModel) as User;
            MainUser.SavedThemes = [.. MainUser.SavedThemes.OrderBy(theme => theme.ThemeName)];

            LOGGER.Info($"Loading \"{MainUser.UserName}'s\" Data");
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

                if (coverCheck || secondCoverCheck)
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
            LOGGER.Info($"Finished Loading \"{MainUser.UserName}'s\" Data!");
        }

        /// <summary>
        /// Changes the cover for series
        /// </summary>
        /// <param name="series">The series to change the cover for</param>
        /// <param name="newBitmapPath">The path to the new cover</param>
        public static void ChangeCover(Series series, string newBitmapPath) {
            // Update Current Viewed Collection
            series.UpdateCover(newBitmapPath);
            int index = SearchedCollection.IndexOf(series);
            SearchedCollection.Remove(series);
            SearchedCollection.Insert(index, series);
        }

        /// <summary>
        /// Changes the cover for series
        /// </summary>
        /// <param name="series">The series to change the cover for</param>
        /// <param name="newBitmapPath">The path to the new cover</param>
        public static void ChangeCover(Series series, Bitmap newCover) {
            // Update Current Viewed Collection
            series.UpdateCover(newCover);
            int index = SearchedCollection.IndexOf(series);
            SearchedCollection.Remove(series);
            SearchedCollection.Insert(index, series);
        }

        public static void DeleteSeries(Series series)
        {
            SearchedCollection.Remove(series);
            UserCollection.Remove(series);
            collectionStatsWindow.ViewModel.UpdateAllStats(series.CurVolumeCount, (uint)(series.MaxVolumeCount - series.CurVolumeCount), true);
            LOGGER.Info("Removed {} From Collection", series.Titles["Romaji"]);
            series.Dispose();
        }

        public static void UpdateChartStats()
        {
            collectionStatsWindow.ViewModel.UpdateStatusChartValues();
            collectionStatsWindow.ViewModel.UpdateStatusPercentages();
        }

        public static void UpdateSeriesCard(Series series)
        {
            for(int x = 0; x < SearchedCollection.Count; x++)
            {
                if (SearchedCollection[x].Titles["Romaji"].Equals(series.Titles["Romaji"]))
                {
                    SearchedCollection.RemoveAt(x);
                    SearchedCollection.Insert(x, series);
                    break;
                }
            }

            int curIndex = UserCollection.FindIndex(curSeries => curSeries.Titles["Romaji"].Equals(series.Titles["Romaji"]));
            UserCollection.RemoveAt(curIndex);
            UserCollection.Insert(curIndex, series);
        }

        public async Task RefreshSeries(Series series)
        {
            newCoverCheck = true;
            Series? newSeries = await Series.CreateNewSeriesCardAsync(series.Link.Segments.Last(), series.Format, series.MaxVolumeCount, series.CurVolumeCount, series.SeriesContainsAdditionalLanagues(), series.Publisher, series.Demographic, series.VolumesRead, series.Rating, series.Value, string.Empty, true);

            if (newSeries != null)
            {
                LOGGER.Info($"\nRefreshing Series {series.Titles["Romaji"]}-> \n{newSeries}");
                bool titleChanged = false, staffChanged = false, statusChanged = false, genresChanged = false;

                int searchIndex = SearchedCollection.ToList().BinarySearch(series, new SeriesComparer(MainUser.CurLanguage));
                searchIndex = searchIndex < 0 ? ~searchIndex : searchIndex;
                if (searchIndex > -1)
                {
                    SearchedCollection.RemoveAt(searchIndex);
                    SearchedCollection.Insert(searchIndex, series);
                }

                int mainIndex = UserCollection.BinarySearch(series, new SeriesComparer(MainUser.CurLanguage));
                mainIndex = mainIndex < 0 ? ~mainIndex : mainIndex;
                IEnumerable<Genre> addedGenres = null, removedGenres = null;

                if (!series.Titles.Equals(newSeries.Titles))
                {
                    series.Titles = newSeries.Titles;
                    UserCollection[mainIndex].Titles = newSeries.Titles;
                    titleChanged = true;
                }
                if (!series.Staff.Equals(newSeries.Staff))
                {
                    series.Staff = newSeries.Staff;
                    UserCollection[mainIndex].Staff = newSeries.Staff;
                    staffChanged = true;
                }
                if (!series.Description.Equals(newSeries.Description))
                {
                    series.Description = newSeries.Description;
                    UserCollection[mainIndex].Description = newSeries.Description;
                }
                if (series.Status != newSeries.Status)
                {
                    series.Status = newSeries.Status;
                    UserCollection[mainIndex].Status = newSeries.Status;
                    UpdateChartStats();
                    statusChanged = true;
                }
                if (series.Genres != newSeries.Genres)
                {
                    if (series.Genres == null)
                    {
                        addedGenres = newSeries.Genres;
                    }
                    else
                    {
                        addedGenres = series.Genres.Except(newSeries.Genres);
                        removedGenres = newSeries.Genres.Except(series.Genres);
                    }
                    series.Genres = newSeries.Genres;
                    UserCollection[mainIndex].Genres = newSeries.Genres;
                    genresChanged = true;
                }
                
                // If there is a change and the user is searching or filtering apply the filter
                if (titleChanged || staffChanged)
                {
                    await RefreshCollection();
                }
                else if (
                    (titleChanged && !CurFilter.Equals("None")) 
                    || (statusChanged && (CurFilter.Equals("Ongoing") || CurFilter.Equals("Finished") || CurFilter.Equals("Hiatus") || CurFilter.Equals("Cancelled")))
                    || (genresChanged && ((addedGenres != null && addedGenres.Any(genre => CurFilter.Equals(genre.GetStringValue()))) || (removedGenres != null && removedGenres.Any(genre => CurFilter.Equals(genre.GetStringValue())))))
                )
                {
                    FilterCollection(CurFilter);
                }
            }
            else
            {
                LOGGER.Warn($"Refresh Returned Null Series Data for \"{series.Titles["Romaji"]}\"");
            }
        }

        public async Task RefreshCollection()
        {
            if (CurFilter == TsundokuFilter.Query)
            {
                await AdvancedSearchCollection(AdvancedSearchText);
            }
            else if (CurFilter != TsundokuFilter.None)
            {
                FilterCollection(CurFilter);
            }
            else if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchCollection(SearchText);
            }
        }
    }
}