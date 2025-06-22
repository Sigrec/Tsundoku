using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;
using Avalonia.Controls;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Tsundoku.Helpers;
using static Tsundoku.Models.Enums.TsundokuLanguageEnums;
using Avalonia.Collections;
using Tsundoku.Clients;
using static Tsundoku.Models.Enums.SeriesDemographicEnum;
using static Tsundoku.Models.Enums.SeriesFormatEnum;
using ReactiveUI;
using System.Globalization;

namespace Tsundoku.ViewModels;

public sealed class AddNewSeriesViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly BitmapHelper _bitmapHelper;
    private readonly MangaDex _mangaDex;
    private readonly AniList _aniList;
    [Reactive] public string TitleText { get; set; }
    [Reactive] public string PublisherText { get; set; }
    [Reactive] public string CoverImageUrl { get; set; }
    [Reactive] public string MaxVolumeCount { get; set; }
    [Reactive] public string CurVolumeCount { get; set; }
    [Reactive] public bool AllowDuplicate { get; set; }
    [Reactive] public string AdditionalLanguagesToolTipText { get; set; }
    [Reactive] public bool IsAddSeriesButtonEnabled { get; set; } = false;
    [Reactive] public string SeriesValueMaskedText { get; set; }
    public AvaloniaList<ListBoxItem> SelectedAdditionalLanguages { get; set; } = [];
    private static readonly StringBuilder CurLanguages = new StringBuilder();

    public AddNewSeriesViewModel(IUserService userService, BitmapHelper bitmapHelper, MangaDex mangaDex, AniList aniList) : base(userService)
    {
        _bitmapHelper = bitmapHelper;
        _mangaDex = mangaDex;
        _aniList = aniList;

        SelectedAdditionalLanguages.CollectionChanged += AdditionalLanguagesCollectionChanged;

        this.WhenAnyValue(x => x.CurrentUser.Currency)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Subscribe(currency =>
            {
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(AVAILABLE_CURRENCY_WITH_CULTURE[currency].Culture);
                if (cultureInfo.NumberFormat.CurrencyPositivePattern is 0 or 2) // 0 = "$n", 2 = "$ n"
                {
                    SeriesValueMaskedText = $"{currency}0000000000000000.00";
                }
                else
                {
                    SeriesValueMaskedText = $"0000000000000000.00{currency}";
                }
            });
    }

    // TODO - Additional Languages selected should persist across app startups? I can traverse the list and and check what additional langs are used or store it in json
    // TODO - When user is looking for a additional lang but MangaDex doesn't have it show a popup that lets them enter the title in?
    private void AdditionalLanguagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
                if (SelectedAdditionalLanguages is not null && SelectedAdditionalLanguages.Any())
                {
                    foreach (ListBoxItem lang in SelectedAdditionalLanguages.OrderBy(lang => lang.Content.ToString()))
                    {
                        CurLanguages.AppendLine(lang.Content.ToString());
                    }
                    AdditionalLanguagesToolTipText = CurLanguages.ToString().Trim();
                }
                else
                {
                    AdditionalLanguagesToolTipText = string.Empty;
                }
                CurLanguages.Clear();
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
    public async Task<KeyValuePair<bool, string>> GetSeriesDataAsync(string input, SeriesFormat bookType, uint curVolCount = 0, uint maxVolCount = 1, TsundokuLanguage[]? additionalLanguages = null, string customImageUrl = "", string publisher = "Unknown", SeriesDemographic demographic = SeriesDemographic.Unknown, uint volumesRead = 0, decimal rating = -1, decimal value = 0, bool allowDuplicate = false)
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
                LOGGER.Debug($"\nAdding New Series (Allow Dupe ?= {allowDuplicate}) -> \"{input}\" | \"{bookType}\" | {curVolCount} | {maxVolCount}\n{newSeries}");
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
            }
        }
        return new KeyValuePair<bool, string>(successfulAdd, returnMsg);
    }

    public TsundokuLanguage[] ConvertSelectedLangList()
    {
        return SelectedAdditionalLanguages
            .AsValueEnumerable()
            .Select(lang => lang.Content.ToString().GetEnumValueFromMemberValue<TsundokuLanguage>())
            .ToArray();
    }
}
