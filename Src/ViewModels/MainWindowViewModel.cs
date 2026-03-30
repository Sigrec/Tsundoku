using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tsundoku.Clients;
using Tsundoku.Helpers;
using Tsundoku.Models;
using static Tsundoku.Models.Enums.TsundokuFilterModel;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;
using static Tsundoku.Models.Enums.TsundokuSortModel;

namespace Tsundoku.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    // --- Injected Dependencies (Readonly Fields) ---
    private readonly ISharedSeriesCollectionProvider _sharedSeriesProvider;
    private readonly BitmapHelper _bitmapHelper;
    private readonly MangaDex _mangaDex;
    private readonly AniList _aniList;
    private bool disposedValue;

    // --- Reactive Properties (Public) ---
    [Reactive] public partial string AdvancedSearchQuery { get; set; } = string.Empty;
    [Reactive] public partial string SeriesFilterText { get; set; }
    [Reactive] public partial TsundokuFilter SelectedFilter { get; set; } = TsundokuFilter.None;
    [Reactive] public partial int SelectedFilterIndex { get; set; } = -1;
    [Reactive] public partial TsundokuSort SelectedSort { get; set; } = TsundokuSort.TitleAZ;
    [Reactive] public partial int SelectedSortIndex { get; set; } = 0;
    [Reactive] public partial int SelectedLangIndex { get; set; }
    [Reactive] public partial string NotificationText { get; set; }
    [Reactive] public partial string AdvancedSearchQueryErrorMessage { get; set; }
    [Reactive] public partial TsundokuLanguage SelectedLanguage { get; set; }
    [Reactive] public partial string SelectedPublisher { get; set; } = string.Empty;

    public ReadOnlyObservableCollection<Series> UserCollection { get; }
    public ReadOnlyObservableCollection<string> AvailablePublishers => _sharedSeriesProvider.AvailablePublishers;
    public ReadOnlyObservableCollection<TsundokuTheme> SavedThemes => _userService.SavedThemes;
    public FilterBuilderViewModel FilterBuilder { get; } = new();

    public Interaction<EditSeriesInfoViewModel, MainWindowViewModel?> EditSeriesInfoDialog { get; } = new Interaction<EditSeriesInfoViewModel, MainWindowViewModel?>();

    public MainWindowViewModel(IUserService userService, ISharedSeriesCollectionProvider sharedSeriesProvider, BitmapHelper bitmapHelper, MangaDex mangaDex, AniList aniList) : base(userService)
    {
        _sharedSeriesProvider = sharedSeriesProvider ?? throw new ArgumentNullException(nameof(sharedSeriesProvider));
        _bitmapHelper = bitmapHelper ?? throw new ArgumentNullException(nameof(bitmapHelper));
        _mangaDex = mangaDex ?? throw new ArgumentNullException(nameof(mangaDex));
        _aniList = aniList ?? throw new ArgumentNullException(nameof(aniList));

        // 1. Bind the UI-facing collection to the one provided by the shared service.
        UserCollection = _sharedSeriesProvider.DynamicUserCollection;

        // 2. Link the ViewModel's filter properties to the shared provider's properties.
        this.WhenAnyValue(x => x.SeriesFilterText)
            .Subscribe(text => sharedSeriesProvider.SeriesFilterText = text)
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.SelectedFilter)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Do(filter => LOGGER.Info("Applying filter: {Filter}", filter))
            .Subscribe(filter =>
            {
                // First action: update the local index
                SelectedFilterIndex = TSUNDOKU_FILTER_DICT.TryGetValue(filter, out int index) ? index : -1;

                // Second action: update the shared provider
                _sharedSeriesProvider.SelectedFilter = filter;
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.SelectedSort)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(sort =>
            {
                SelectedSortIndex = TSUNDOKU_SORT_DICT[sort];
                _sharedSeriesProvider.SelectedSort = sort;
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.SelectedPublisher)
            .DistinctUntilChanged()
            .Subscribe(publisher => _sharedSeriesProvider.SelectedPublisher = publisher)
            .DisposeWith(_disposables);

        // Wire FilterBuilder's synthesized query into the shared provider
        FilterBuilder.WhenAnyValue(x => x.SynthesizedQuery)
            .Subscribe(query =>
            {
                AdvancedSearchQuery = query;
                _sharedSeriesProvider.AdvancedSearchQuery = query;
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.AdvancedSearchQueryErrorMessage)
            .Subscribe(queryErrMsg => _sharedSeriesProvider.AdvancedSearchQueryErrorMessage = queryErrMsg)
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.CurrentUser.Language)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(lang =>
            {
                int newIndex = INDEXED_LANGUAGES[lang];
                if (SelectedLangIndex != newIndex)
                {
                    SelectedLangIndex = newIndex;
                }
            })
            .DisposeWith(_disposables);
    }

    public void SaveUserData()
    {
        _userService.SaveUserData();
    }

    public bool ShouldShowChangelog()
    {
        string lastSeen = _userService.GetCurrentUserSnapshot()?.LastSeenAppVersion ?? string.Empty;
        return Changelog.ShouldShow(CUR_TSUNDOKU_VERSION, lastSeen);
    }

    public void MarkChangelogSeen()
    {
        _userService.UpdateUser(user => user.LastSeenAppVersion = CUR_TSUNDOKU_VERSION);
    }

    public async Task CreateEditSeriesDialog(Series series)
    {
        await EditSeriesInfoDialog.Handle(new EditSeriesInfoViewModel(series, _userService));
    }

    public void UpdateUserIcon(string filePath)
    {
        _userService.UpdateUserIcon(filePath);
    }
    public void UpdateUserLanguage(TsundokuLanguage newLang)
    {
        _userService.UpdateUser(user => user.Language = newLang);
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
    }

    public void UpdateSeriesCard(Series series)
    {
        _userService.AddSeries(series);
    }

    public async Task RefreshSeries(Series originalSeries)
    {
        LOGGER.Info("Refreshing {series} ({id})", originalSeries.Titles[TsundokuLanguage.Romaji] + (originalSeries.DuplicateIndex == 0 ? string.Empty : $" ({originalSeries.DuplicateIndex})"), originalSeries.Id);

        Series? refreshedSeries = await Series.CreateNewSeriesCardAsync(
            _bitmapHelper,
            _mangaDex,
            _aniList,
            originalSeries.Link.Segments.Last(),
            originalSeries.Format,
            originalSeries.MaxVolumeCount,
            originalSeries.CurVolumeCount,
            originalSeries.SeriesContainsAdditionalLanagues(),
            originalSeries.Publisher,
            originalSeries.Demographic,
            originalSeries.VolumesRead,
            originalSeries.Rating,
            originalSeries.Value,
            string.Empty,
            allowDuplicate: false,
            isRefresh: true,
            isCoverImageRefresh: originalSeries.IsCoverImageEmpty() || (_userService.GetCurrentUserSnapshot()?.RefreshCovers ?? false),
            coverPath: originalSeries.Cover);

        _userService.RefreshSeries(originalSeries, refreshedSeries);
    }

    public void SaveOnClose()
    {
        LOGGER.Info("Closing Tsundoku");
        if (!isReloading)
        {
            _userService.SaveUserData();
            _userService.SaveBackupUserData();
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _disposables.Dispose();
                FilterBuilder.Dispose();
            }
            disposedValue = true;
        }
    }
    
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}