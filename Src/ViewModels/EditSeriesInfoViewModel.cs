using System.Collections.Specialized;
using System.Globalization;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;
using Tsundoku.Models.Enums;
using static Tsundoku.Models.Enums.SeriesDemographicEnum;
using static Tsundoku.Models.Enums.SeriesGenreEnum;

namespace Tsundoku.ViewModels;

public sealed class EditSeriesInfoViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public Series Series { get; }
    [Reactive] public int DemographicIndex { get; set; }
    [Reactive] public string CoverImageUrl { get; set; }
    [Reactive] public string GenresToolTipText { get; set; }
    [Reactive] public string SeriesValueText { get; set; }
    [Reactive] public string SeriesValueMaskedText { get; set; }
    public AvaloniaList<ListBoxItem> SelectedGenres { get; set; } = [];

    public EditSeriesInfoViewModel(Series series, IUserService userService) : base(userService)
    {
        Series = series;
        this.WhenAnyValue(x => x.Series.Demographic)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Subscribe(x => DemographicIndex = SERIES_DEMOGRAPHICS[x]);

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

        this.WhenAnyValue(x => x.CurrentUser.Currency, x => x.Series.Value)
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Subscribe(tuple =>
            {
                var (currency, value) = tuple;
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(AVAILABLE_CURRENCY_WITH_CULTURE[currency].Culture);
                if (cultureInfo.NumberFormat.CurrencyPositivePattern is 0 or 2) // 0 = "$n", 2 = "$ n"
                {
                    SeriesValueText = $"{currency}{value}";
                }
                else
                {
                    SeriesValueText = $"{value}{currency}";
                }
            });

        SelectedGenres.CollectionChanged += SeriesGenresChanged;
    }

    private void SeriesGenresChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Reset:
                if (SelectedGenres is { Count: > 0 })
                {
                    StringBuilder builder = new();
                    foreach (ListBoxItem genre in SelectedGenres.OrderBy(g => g.Content?.ToString()))
                    {
                        builder.AppendLine(genre.Content?.ToString());
                    }
                    GenresToolTipText = builder.ToString().Trim();
                }
                else
                {
                    GenresToolTipText = string.Empty;
                }
                return;

            default:
                LOGGER.Error("{Action} is not supported for genre changes", e.Action);
                throw new ArgumentOutOfRangeException(nameof(e.Action), e.Action, "Unsupported collection change action.");
        }
    }

    public HashSet<SeriesGenre> GetCurrentGenresSelected()
    {
        HashSet<SeriesGenre> newGenres = [];
        foreach (ListBoxItem genreItem in SelectedGenres)
        {
            if (SeriesGenreEnum.TryParse(genreItem.Content.ToString(), out SeriesGenre genre))
            {
                newGenres.Add(genre);
            }
        }
        return newGenres;
    }
}
