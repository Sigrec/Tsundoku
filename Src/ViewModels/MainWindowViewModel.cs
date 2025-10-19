using System.Collections.ObjectModel;
using System.Reactive.Disposables;

using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Clients;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.Views;
using static Tsundoku.Models.Enums.TsundokuFilterModel;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase, IDisposable
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
    private UserSettingsWindow _userSettingsWindow;
    private CollectionThemeWindow _themeSettingsWindow;
    private PriceAnalysisWindow _priceAnalysisWindow;
    private CollectionStatsWindow _collectionStatsWindow;
    private UserNotesWindow _userNotesWindow;
    private bool disposedValue;

    public AddNewSeriesWindow NewSeriesWindow => _newSeriesWindow;
    public UserSettingsWindow UserSettingsWindow => _userSettingsWindow;
    public CollectionThemeWindow ThemeSettingsWindow => _themeSettingsWindow;
    public PriceAnalysisWindow PriceAnalysisWindow => _priceAnalysisWindow;
    public CollectionStatsWindow CollectionStatsWindow => _collectionStatsWindow;
    public UserNotesWindow UserNotesWindow => _userNotesWindow;

    // --- Reactive Properties (Public) ---
    [Reactive] public string AdvancedSearchQuery { get; set; } = string.Empty;
    [Reactive] public string SeriesFilterText { get; set; }
    [Reactive] public TsundokuFilter SelectedFilter { get; set; } = TsundokuFilter.None;
    [Reactive] public int SelectedFilterIndex { get; set; } = 0;
    [Reactive] public int SelectedLangIndex { get; set; }
    [Reactive] public string NotificationText { get; set; }
    [Reactive] public string AdvancedSearchQueryErrorMessage { get; set; }
    [Reactive] public TsundokuLanguage SelectedLanguage { get; set; }

    public ReadOnlyObservableCollection<Series> UserCollection { get; }
    public static readonly List<Series> CoverChangedSeriesList = [];

    public Interaction<EditSeriesInfoViewModel, MainWindowViewModel?> EditSeriesInfoDialog { get; } = new Interaction<EditSeriesInfoViewModel, MainWindowViewModel?>();

    [GeneratedRegex(@"(\w+)(==|<=|>=)(\d+|\w+|(?:'|"")(?:.*?)(?:'|""))")] public static partial Regex AdvancedQueryRegex();

    public MainWindowViewModel(IUserService userService, ISharedSeriesCollectionProvider sharedSeriesProvider, BitmapHelper bitmapHelper, MangaDex mangaDex, AniList aniList, IServiceProvider serviceProvider) : base(userService)
    {
        _sharedSeriesProvider = sharedSeriesProvider ?? throw new ArgumentNullException(nameof(sharedSeriesProvider));
        _bitmapHelper = bitmapHelper ?? throw new ArgumentNullException(nameof(bitmapHelper));
        _mangaDex = mangaDex ?? throw new ArgumentNullException(nameof(mangaDex));
        _aniList = aniList ?? throw new ArgumentNullException(nameof(aniList));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        // 1. Bind the UI-facing collection to the one provided by the shared service.
        UserCollection = _sharedSeriesProvider.DynamicUserCollection;

        ConfigureWindows();

        // 2. Link the ViewModel's filter properties to the shared provider's properties.
        this.WhenAnyValue(x => x.SeriesFilterText)
            .Subscribe(text => sharedSeriesProvider.SeriesFilterText = text)
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.SelectedFilter)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(filter => LOGGER.Info("Applying filter: {Filter}", filter))
            .Subscribe(filter =>
            {
                // First action: update the local index
                SelectedFilterIndex = TSUNDOKU_FILTER_DICT[filter];

                // Second action: update the shared provider
                _sharedSeriesProvider.SelectedFilter = filter;
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.AdvancedSearchQuery)
            .Subscribe(query => _sharedSeriesProvider.AdvancedSearchQuery = query)
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.AdvancedSearchQueryErrorMessage)
            .Subscribe(queryErrMsg => _sharedSeriesProvider.AdvancedSearchQueryErrorMessage = queryErrMsg)
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.CurrentUser.Language)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
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

    public async Task CreateEditSeriesDialog(Series series)
    {
        await EditSeriesInfoDialog.Handle(new EditSeriesInfoViewModel(series, _userService));
    }

    public void UpdateUserIcon(string filePath)
    {
        _userService.UpdateUserIcon(filePath);
    }
    private void ConfigureWindows()
    {
        LOGGER.Info("Configuring Windows...");

        _newSeriesWindow = _serviceProvider.GetRequiredService<AddNewSeriesWindow>();
        _userSettingsWindow = _serviceProvider.GetRequiredService<UserSettingsWindow>();
        _themeSettingsWindow = _serviceProvider.GetRequiredService<CollectionThemeWindow>();
        _priceAnalysisWindow = _serviceProvider.GetRequiredService<PriceAnalysisWindow>();
        _collectionStatsWindow = _serviceProvider.GetRequiredService<CollectionStatsWindow>();
        _userNotesWindow = _serviceProvider.GetRequiredService<UserNotesWindow>();

        LOGGER.Info("Finished Configuring Windows!");
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
            isCoverImageRefresh: originalSeries.IsCoverImageEmpty(),
            coverPath: originalSeries.Cover);

        _userService.RefreshSeries(originalSeries, refreshedSeries);
    }

    public void SaveOnClose()
    {
        LOGGER.Info("Closing Tsundoku");
        if (!isReloading) _userService.SaveUserData();
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _disposables.Dispose();
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