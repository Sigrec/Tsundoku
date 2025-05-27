using ReactiveUI;
using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;
using Avalonia.Media.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia.Collections;
using Tsundoku.Helpers;
using Tsundoku.Views;
using static Tsundoku.Models.TsundokuFilterModel;
using System.Reactive.Disposables;
using static Tsundoku.Models.TsundokuLanguageModel;
using System.Reactive.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.DependencyInjection;
using DynamicData;
using Tsundoku.Services;
using System.Collections.ObjectModel;

namespace Tsundoku.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        // --- Injected Dependencies (Readonly Fields) ---
        private readonly ISharedSeriesCollectionProvider _sharedSeriesProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly BitmapHelper _bitmapHelper;
        private readonly MangaDex _mangaDex;
        private readonly AniList _aniList;

        // --- Window Instances (Private Fields & Public Readonly Properties) ---
        private AddNewSeriesWindow _newSeriesWindow;
        private SettingsWindow _settingsWindow;
        private CollectionThemeWindow _themeSettingsWindow;
        private PriceAnalysisWindow _priceAnalysisWindow;
        private CollectionStatsWindow _collectionStatsWindow;
        private UserNotesWindow _userNotesWindow;

        public AddNewSeriesWindow NewSeriesWindow => _newSeriesWindow;
        public SettingsWindow SettingsWindow => _settingsWindow;
        public CollectionThemeWindow ThemeSettingsWindow => _themeSettingsWindow;
        public PriceAnalysisWindow PriceAnalysisWindow => _priceAnalysisWindow;
        public CollectionStatsWindow CollectionStatsWindow => _collectionStatsWindow;
        public UserNotesWindow UserNotesWindow => _userNotesWindow;

        // --- Reactive Properties (Public) ---
        [Reactive] public string AdvancedSeriesFilterText { get; set; } = string.Empty;
        [Reactive] public string SeriesFilterText { get; set; }
        [Reactive] public TsundokuFilter SelectedFilter { get; set; } = TsundokuFilter.None;
        [Reactive] public ushort SelectedFilterIndex { get; set; } = 0;
        [Reactive] public ushort SelectedLangIndex { get; set; }
        [Reactive] public string NotificationText { get; set; }
        [Reactive] public string AdvancedSearchQueryErrorMessage { get; set; }

        // --- Collections (Public Readonly / Static) ---
        public ReadOnlyObservableCollection<Series> UserCollection { get; }
        public static readonly List<Series> CoverChangedSeriesList = [];

        // --- Commands and Interactions (Public Readonly) ---
        public ICommand StartAdvancedSearch { get; }
        public Interaction<EditSeriesInfoViewModel, MainWindowViewModel?> EditSeriesInfoDialog { get; } = new Interaction<EditSeriesInfoViewModel, MainWindowViewModel?>();

        // --- Helper Properties / Methods (Regex, etc.) ---
        [GeneratedRegex(@"(\w+)(==|<=|>=)(\d+|\w+|(?:'|"")(?:.*?)(?:'|""))")] public static partial Regex AdvancedQueryRegex();

        public MainWindowViewModel(IUserService userService, ISharedSeriesCollectionProvider sharedSeriesProvider, BitmapHelper bitmapHelper, MangaDex mangaDex, AniList aniList, IServiceProvider serviceProvider) : base(userService)
        {
            _sharedSeriesProvider = sharedSeriesProvider ?? throw new ArgumentNullException(nameof(sharedSeriesProvider));
            _bitmapHelper = bitmapHelper;
            _mangaDex = mangaDex;
            _aniList = aniList;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // 1. Bind the UI-facing collection to the one provided by the shared service.
            UserCollection = _sharedSeriesProvider.DynamicUserCollection;

            // 2. Link the ViewModel's filter properties to the shared provider's properties.
            this.WhenAnyValue(x => x.SeriesFilterText)
                .Subscribe(text => sharedSeriesProvider.SeriesFilterText = text)
                .DisposeWith(_disposables);

            // When SelectedFilter changes in this ViewModel, update the shared provider's SelectedFilter.
            this.WhenAnyValue(x => x.SelectedFilter)
                .Subscribe(filter => sharedSeriesProvider.SelectedFilter = filter)
                .DisposeWith(_disposables);

            ConfigureWindows();

            this.WhenAnyValue(x => x.CurrentUser.Language)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(lang => SelectedLangIndex = (ushort)INDEXED_LANGUAGES[lang])
                .DisposeWith(_disposables);

            this.WhenAnyValue(x => x.SelectedFilter)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(filter => SelectedFilterIndex = (ushort)FILTERS[filter])
                .DisposeWith(_disposables);

            StartAdvancedSearch = ReactiveCommand.Create(() => AdvancedSearchCollection(AdvancedSeriesFilterText));
        }

        public void SaveUserData()
        {
            _userService.SaveUserData();
        }

        public async Task CreateEditSeriesDialog(Series series)
        {
            await EditSeriesInfoDialog.Handle(new EditSeriesInfoViewModel(series, _userService));
        }

        public void UpdateUserIcon(string filePath)
        {
            _userService.UpdateUserIcon(filePath);
        }

        public void AddUserVolumeCount()
        {
            CollectionStatsWindow.ViewModel.UsersNumVolumesCollected++;
            CollectionStatsWindow.ViewModel.UsersNumVolumesToBeCollected--;
        }

        public void SubstractUserVolumeCount()
        {
            CollectionStatsWindow.ViewModel.UsersNumVolumesCollected--;
            CollectionStatsWindow.ViewModel.UsersNumVolumesToBeCollected++;
        }

        private void ConfigureWindows()
        {
            LOGGER.Info("Configuring Windows...");

            _newSeriesWindow = _serviceProvider.GetRequiredService<AddNewSeriesWindow>();
            _settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
            _themeSettingsWindow = _serviceProvider.GetRequiredService<CollectionThemeWindow>();
            _priceAnalysisWindow = _serviceProvider.GetRequiredService<PriceAnalysisWindow>();
            // _priceAnalysisWindow.ViewModel.CurRegion = CurrentUser.Region;
            _collectionStatsWindow = _serviceProvider.GetRequiredService<CollectionStatsWindow>();
            _userNotesWindow = _serviceProvider.GetRequiredService<UserNotesWindow>();

            LOGGER.Info("Finished Configuring Windows!");
        }

        public void UpdateUserLanguage(string newLang)
        {
            _userService.UpdateUser(user => user.Language = newLang.GetEnumValueFromMemberValue(TsundokuLanguage.Romaji));
        }

        // Basic Test Query Demographic==Seinen Format==Manga Series==Complete Favorite==True
        // TODO - Fix the Advanced Search since others are done
        public async Task AdvancedSearchCollection(string AdvancedSearchQuery)
        {
            if (!string.IsNullOrWhiteSpace(AdvancedSearchQuery) && AdvancedQueryRegex().IsMatch(AdvancedSearchQuery))
            {
                AdvancedSearchQuery = AdvancedSearchQuery.Trim();
                if (!string.IsNullOrWhiteSpace(SeriesFilterText)) // Checks if the user searching and remove the text
                {
                    // SearchIsBusy = false;
                    SeriesFilterText = string.Empty;
                }
                if (SelectedFilter != TsundokuFilter.None)
                {
                    SelectedFilter = TsundokuFilter.None;
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
                        // FilteredCollection = UserCollection.AsQueryable().Where(AdvancedFilterExpression.ToString());
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

                // if (FilteredCollection.Any())
                // {
                //     if (SelectedFilter != TsundokuFilter.Query)
                //     {
                //         SelectedFilter = TsundokuFilter.Query;
                //     }
                //     SearchedCollection.Clear();
                //     SearchedCollection.AddRange(FilteredCollection);
                //     LOGGER.Info($"Valid Advanced Search Query");
                // }
                // else
                // {
                //     AdvancedSearchQueryErrorMessage = "Advanced Search Query Returned No Series!";
                //     LOGGER.Info("Advanced Search Query Returned No Series");
                // }
            }
            else
            {
                AdvancedSearchQueryErrorMessage = "Inputted Incorrectly Formatted Advanced Search Query!";
                LOGGER.Warn("User Inputted Incorrectly Formatted Advanced Search Query");
            }
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

        /// <summary>
        /// Changes the cover for series
        /// </summary>
        /// <param name="series">The series to change the cover for</param>
        /// <param name="newBitmapPath">The path to the new cover</param>
        public void ChangeCover(Series series, Bitmap newCover)
        {
            // Update Current Viewed Collection
            _userService.UpdateSeriesCoverBitmap(series.Id, newCover);
        }

        public void DeleteSeries(Series series)
        {
            _userService.RemoveSeries(series);
            CollectionStatsWindow.ViewModel.UpdateAllStats(series.CurVolumeCount, (uint)(series.MaxVolumeCount - series.CurVolumeCount), true);
            LOGGER.Info("Removed {} From Collection", series.Titles[TsundokuLanguage.Romaji]);
        }

        public void UpdateSeriesCard(Series series)
        {
            _userService.AddSeries(series);
        }

        // TODO - Ensure that if people change files directly in the Covers folder it updates properly
        public void CoverFolderWatcherLogic(string path)
        {
            LOGGER.Debug("{} | {} | {} | {}", path, !newCoverCheck, path.EndsWith("jpg", StringComparison.OrdinalIgnoreCase), path.EndsWith("png", StringComparison.OrdinalIgnoreCase));
            if (!newCoverCheck && (path.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) || path.EndsWith("png", StringComparison.OrdinalIgnoreCase)))
            {
                string pathFileExtension = path[^3..];
                Series series = _userService.GetSeriesByCoverPath(path) ?? _userService.GetSeriesByCoverPath(path[..^3] + (pathFileExtension.Equals("jpg") ? "png" : "jpg"));

                if (!series.Cover[^3..].Equals(pathFileExtension))
                {
                    series.Cover = series.Cover[..^3] + pathFileExtension;
                    File.Delete(series.Cover[..^3] + (pathFileExtension.Equals("jpg") ? "png" : "jpg"));
                    LOGGER.Info("Changed File Extention for {} to {}", series.Titles[TsundokuLanguage.Romaji], pathFileExtension);
                }

                if (!CoverChangedSeriesList.Contains(series))
                {
                    CoverChangedSeriesList.Add(series);
                    LOGGER.Debug($"Added \"{series.Titles[TsundokuLanguage.Romaji]}\" to Cover Change List");
                }
                else
                {
                    LOGGER.Info($"\"{series.Titles[TsundokuLanguage.Romaji]}\" Cover Changed Again");
                }
            }
            newCoverCheck = false;
        }

        public async Task RefreshSeries(Series series)
        {
            newCoverCheck = true;
            Series? refreshedSeries = await Series.CreateNewSeriesCardAsync(
                _bitmapHelper,
                _mangaDex,
                _aniList,
                series.Link.Segments.Last(),
                series.Format,
                series.MaxVolumeCount,
                series.CurVolumeCount,
                series.SeriesContainsAdditionalLanagues(),
                series.Publisher,
                series.Demographic,
                series.VolumesRead,
                series.Rating,
                series.Value,
                string.Empty,
                true);

            _userService.AddSeries(refreshedSeries);
            CollectionStatsWindow.ViewModel.UpdateChartStats();
        }

        public void SaveOnClose()
        {
            LOGGER.Info("Closing Tsundoku");
            if (!isReloading) { _userService.SaveUserData(); }
            DiscordRP.Deinitialize();

            if (NewSeriesWindow != null)
            {
                NewSeriesWindow.Closing += (s, e) => { e.Cancel = false; };
                NewSeriesWindow.Close();
            }

            if (SettingsWindow != null)
            {
                SettingsWindow.Closing += (s, e) => { e.Cancel = false; };
                SettingsWindow.Close();
            }

            if (ThemeSettingsWindow != null)
            {
                ThemeSettingsWindow.Closing += (s, e) => { e.Cancel = false; };
                ThemeSettingsWindow.Close();
            }

            if (PriceAnalysisWindow != null)
            {
                PriceAnalysisWindow.Closing += (s, e) => { e.Cancel = false; };
                PriceAnalysisWindow.Close();
            }

            if (CollectionStatsWindow != null)
            {
                CollectionStatsWindow.Closing += (s, e) => { e.Cancel = false; };
                CollectionStatsWindow.Close();
            }

            if (UserNotesWindow != null)
            {
                UserNotesWindow.Closing += (s, e) => { e.Cancel = false; };
                UserNotesWindow.Close();
            }

            LogManager.Shutdown();
            App.DisposeMutex();
        }
        
        /// <summary>
        /// Disposes of all managed resources, particularly Rx subscriptions.
        /// </summary>
        public override void Dispose()
        {
            _disposables.Dispose();
        }
    }
}