using Tsundoku.Models;
using ReactiveUI.SourceGenerators;
using System.Collections.Specialized;
using Tsundoku.Helpers;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;
using Avalonia.Collections;
using Tsundoku.Clients;
using Tsundoku.Services;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using ReactiveUI;
using System.Globalization;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using DynamicData;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace Tsundoku.ViewModels;

/// <summary>
/// View model for the Add New Series dialog, handling title lookup, suggestions, and series creation.
/// </summary>
public sealed partial class AddNewSeriesViewModel : ViewModelBase, IDisposable
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly BitmapHelper _bitmapHelper;
    private readonly MangaDex _mangaDex;
    private readonly AniList _aniList;

    [Reactive] public partial string TitleText { get; set; }
    [Reactive] public partial string PublisherText { get; set; }
    [Reactive] public partial string CoverImageUrl { get; set; }
    [Reactive] public partial string MaxVolumeCount { get; set; }
    [Reactive] public partial string CurVolumeCount { get; set; }
    [Reactive] public partial bool AllowDuplicate { get; set; }
    [Reactive] public partial string AdditionalLanguagesToolTipText { get; set; }
    [Reactive] public partial bool IsAddSeriesButtonEnabled { get; set; } = false;
    [Reactive] public partial string SeriesValueMaskedText { get; set; }
    [Reactive] public partial AvaloniaList<TsundokuLanguage> SelectedAdditionalLanguages { get; set; } = [];

    private readonly SourceList<AniListPickerSuggestion> _suggestionsSource = new();
    public ReadOnlyObservableCollection<AniListPickerSuggestion> Suggestions { get; private set; }
    [Reactive] public partial AniListPickerSuggestion? SelectedSuggestion { get; set; }
    [Reactive] public partial bool IsSuggestionsOpen { get; set; }

    private readonly StringBuilder _curLanguages = new();

    private readonly IApiHealthCheckService _apiHealthCheckService;

    public AddNewSeriesViewModel(IUserService userService, BitmapHelper bitmapHelper, MangaDex mangaDex, AniList aniList, IApiHealthCheckService apiHealthCheckService) : base(userService)
    {
        _bitmapHelper = bitmapHelper;
        _mangaDex = mangaDex;
        _aniList = aniList;
        _apiHealthCheckService = apiHealthCheckService;

        User? user = userService.GetCurrentUserSnapshot();
        if (user?.AdditionalLanguages is { Count: > 0 })
        {
            SelectedAdditionalLanguages.AddRange(user.AdditionalLanguages);
        }

        SelectedAdditionalLanguages.CollectionChanged += AdditionalLanguagesCollectionChanged;

        this.WhenAnyValue(x => x.CurrentUser.Currency)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(currency =>
            {
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(AVAILABLE_CURRENCY_WITH_CULTURE[currency].Culture);
                int decimalDigits = cultureInfo.NumberFormat.CurrencyDecimalDigits;
                string mask = decimalDigits > 0
                    ? "000000000000000000." + new string('0', decimalDigits)
                    : "000000000000000000";
                if (cultureInfo.NumberFormat.CurrencyPositivePattern is 0 or 2) // 0 = "$n", 2 = "$ n"
                {
                    SeriesValueMaskedText = $"{currency}{mask}";
                }
                else
                {
                    SeriesValueMaskedText = $"{mask}{currency}";
                }
            })
            .DisposeWith(_disposables);

        SetupTitleSuggestions();
    }

    private void SetupTitleSuggestions()
    {
        _suggestionsSource.Connect()
            .Sort(new AniListPickerSuggestionComparer())
            .Bind(out ReadOnlyObservableCollection<AniListPickerSuggestion> _suggestions)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe()
            .DisposeWith(_disposables);

        Suggestions = _suggestions;

        _suggestionsSource
            .CountChanged
            .Select(count => count > 0)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(x => IsSuggestionsOpen = x)
            .DisposeWith(_disposables);


        this.WhenAnyValue(x => x.TitleText)
            .Select(x => x is null ? string.Empty : x.Trim())
            .Throttle(TimeSpan.FromMilliseconds(300), RxSchedulers.TaskpoolScheduler)
            .DistinctUntilChanged()
            .Select(x =>
            {
                if (string.IsNullOrWhiteSpace(x) || x.Length < 2 || !_apiHealthCheckService.IsAniListUp)
                    return Observable.Return(Array.Empty<AniListPickerSuggestion>());

                return Observable.FromAsync(async ct =>
                {
                    return await _aniList.GetPickerSuggestionsAsync(x, ct);
                });
            })
            .Switch()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(items =>
            {
                _suggestionsSource.Edit(list =>
                {
                    list.Clear();
                    list.AddRange(items);
                });
            })
            .DisposeWith(_disposables);
    }

    /// <summary>
    /// Clears the current suggestions list and resets the selected suggestion.
    /// </summary>
    public void ClearSuggestions()
    {
        SelectedSuggestion = null;
        _suggestionsSource.Clear();
    }

    private void AdditionalLanguagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (SelectedAdditionalLanguages is not null && SelectedAdditionalLanguages.Count != 0)
        {
            _curLanguages.Clear();
            foreach (TsundokuLanguage lang in SelectedAdditionalLanguages.AsValueEnumerable().OrderBy(lang => lang))
            {
                _curLanguages.AppendLine(lang.ToString());
            }
            AdditionalLanguagesToolTipText = _curLanguages.ToString().Trim();
        }
        else
        {
            AdditionalLanguagesToolTipText = string.Empty;
        }

        _userService.UpdateUser(user =>
        {
            user.AdditionalLanguages = [.. SelectedAdditionalLanguages];
        });
    }
    
    /// <summary>
    /// Recieves the call from the AddNewSeriesWindow Button "Add" press which sends the data to this method which calls for the method to get the series data and create a new series object. Then it checks to see if it's a valid series and whether the series already exists in the users collection if not then it adds.
    /// </summary>
    /// <param name="title">The titles of the new series to add</param>
    /// <param name="bookType">The book type or format of the series to add either a Manga(Comic) or Novel</param>
    /// <param name="curVolCount">The current # of volumes the user has collected for this series</param>
    /// <param name="maxVolCount">The max # of volumes this series currently has</param>
    /// <param name="additionalLanguages">Additional languages to get more info for from Mangadex</param>
    /// <returns>Whether the series can be added to the users collection or not</returns>
    public async Task<(bool Success, string Message, Series? AddedSeries)> GetSeriesDataAsync(string input, SeriesFormat bookType, uint curVolCount = 0, uint maxVolCount = 1, TsundokuLanguage[]? additionalLanguages = null, string customImageUrl = "", string publisher = "Unknown", SeriesDemographic demographic = SeriesDemographic.Unknown, uint volumesRead = 0, decimal rating = -1, decimal value = 0, bool allowDuplicate = false)
    {
        string returnMsg = string.Empty;
        Series? newSeries = await Series.CreateNewSeriesCardAsync(
            bitmapHelper: _bitmapHelper,
            mangaDex: _mangaDex,
            aniList: _aniList,
            input: input,
            bookType: bookType,
            maxVolCount: maxVolCount,
            minVolCount: curVolCount, // Assuming curVolCount maps to minVolCount
            additionalLanguages: additionalLanguages,
            publisher: publisher,
            demographic: demographic,
            volumesRead: volumesRead,
            rating: rating,
            value: value,
            customImageUrl: customImageUrl,
            allowDuplicate: allowDuplicate
        );

        bool successfulAdd = false;
        if (newSeries is not null)
        {
            try
            {
                LOGGER.Debug("Adding New Series (Allow Dupe ?= {AllowDuplicate}) -> \"{Input}\" | \"{BookType}\" | {CurVolCount} | {MaxVolCount}\n{NewSeries}", allowDuplicate, input, bookType, curVolCount, maxVolCount, newSeries);
                successfulAdd = _userService.AddSeries(newSeries, allowDuplicate);
                if (!successfulAdd)
                {
                    returnMsg = "Duplicate Series";
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Error adding new Series to Collection");
                newSeries?.Dispose();
                returnMsg = ex.Message;
                newSeries = null;
            }
        }
        return (successfulAdd, returnMsg, successfulAdd ? newSeries : null);
    }

    /// <summary>
    /// Converts the selected additional languages list to an array.
    /// </summary>
    /// <returns>An array of the selected additional languages.</returns>
    public TsundokuLanguage[] ConvertSelectedLangList()
    {
        return SelectedAdditionalLanguages
            .AsValueEnumerable()
            .ToArray();
    }

    /// <summary>
    /// Releases resources and unsubscribes from collection change events.
    /// </summary>
    public void Dispose()
    {
        SelectedAdditionalLanguages.CollectionChanged -= AdditionalLanguagesCollectionChanged;
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}