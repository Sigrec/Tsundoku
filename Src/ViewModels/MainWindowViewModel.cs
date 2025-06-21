using ReactiveUI;
using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;
using Avalonia.Media.Imaging;
using System.Text.RegularExpressions;
using Tsundoku.Helpers;
using Tsundoku.Views;
using static Tsundoku.Models.TsundokuFilterModel;
using System.Reactive.Disposables;
using static Tsundoku.Models.TsundokuLanguageModel;
using System.Reactive.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using Tsundoku.Clients;

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
    private SettingsWindow _settingsWindow;
    private CollectionThemeWindow _themeSettingsWindow;
    private PriceAnalysisWindow _priceAnalysisWindow;
    private CollectionStatsWindow _collectionStatsWindow;
    private UserNotesWindow _userNotesWindow;
    private bool disposedValue;

    public AddNewSeriesWindow NewSeriesWindow => _newSeriesWindow;
    public SettingsWindow SettingsWindow => _settingsWindow;
    public CollectionThemeWindow ThemeSettingsWindow => _themeSettingsWindow;
    public PriceAnalysisWindow PriceAnalysisWindow => _priceAnalysisWindow;
    public CollectionStatsWindow CollectionStatsWindow => _collectionStatsWindow;
    public UserNotesWindow UserNotesWindow => _userNotesWindow;

    // --- Reactive Properties (Public) ---
    [Reactive] public string AdvancedSearchQuery { get; set; } = string.Empty;
    [Reactive] public string SeriesFilterText { get; set; }
    [Reactive] public TsundokuFilter SelectedFilter { get; set; } = TsundokuFilter.None;
    [Reactive] public ushort SelectedFilterIndex { get; set; } = 0;
    [Reactive] public int SelectedLangIndex { get; set; }
    [Reactive] public string NotificationText { get; set; }
    [Reactive] public string AdvancedSearchQueryErrorMessage { get; set; }

    // --- Collections (Public Readonly / Static) ---
    public ReadOnlyObservableCollection<Series> UserCollection { get; }
    public static readonly List<Series> CoverChangedSeriesList = [];

    // --- Commands and Interactions (Public Readonly) ---
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

        this.WhenAnyValue(x => x.AdvancedSearchQuery)
            .Subscribe(query => sharedSeriesProvider.AdvancedSearchQuery = query)
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.AdvancedSearchQueryErrorMessage)
            .Subscribe(queryErrMsg => sharedSeriesProvider.AdvancedSearchQueryErrorMessage = queryErrMsg)
            .DisposeWith(_disposables);

        ConfigureWindows();

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

        Observable.Merge(
            this.WhenAnyValue(x => x.SelectedFilter),
            this.WhenAnyValue(x => x._sharedSeriesProvider.SelectedFilter)
        )
        .DistinctUntilChanged()
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(filter => SelectedFilterIndex = (ushort)FILTERS[filter])
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
        _settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
        _themeSettingsWindow = _serviceProvider.GetRequiredService<CollectionThemeWindow>();
        _priceAnalysisWindow = _serviceProvider.GetRequiredService<PriceAnalysisWindow>();
        _collectionStatsWindow = _serviceProvider.GetRequiredService<CollectionStatsWindow>();
        _userNotesWindow = _serviceProvider.GetRequiredService<UserNotesWindow>();

        LOGGER.Info("Finished Configuring Windows!");
    }

    public void UpdateUserLanguage(string newLang)
    {
        _userService.UpdateUser(user => user.Language = newLang.GetEnumValueFromMemberValue(TsundokuLanguage.Romaji));
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
        newCoverCheck = false;
        _userService.RemoveSeries(series);
    }

    public void UpdateSeriesCard(Series series)
    {
        _userService.AddSeries(series);
    }

    public async Task RefreshSeries(Series originalSeries)
    {
        LOGGER.Info("Refreshing {series} ({id})", originalSeries.Titles[TsundokuLanguage.Romaji] + (originalSeries.DuplicateIndex == 0 ? string.Empty : $" ({originalSeries.DuplicateIndex})"), originalSeries.Id);

        newCoverCheck = true;
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
            isRefresh: true);

        refreshedSeries.Cover = originalSeries.Cover;
        _userService.UpdateSeries(originalSeries, refreshedSeries);
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