using System.Collections.Specialized;
using System.Globalization;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Collections;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tsundoku.Helpers;
using Tsundoku.Models;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;

namespace Tsundoku.ViewModels;

/// <summary>
/// View model for the Edit Series Info dialog, managing demographic, genre, and value editing for a single series.
/// </summary>
public sealed partial class EditSeriesInfoViewModel : ViewModelBase, IDisposable
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public Series Series { get; }
    [Reactive] public partial int DemographicIndex { get; set; }
    [Reactive] public partial string CoverImageUrl { get; set; }
    [Reactive] public partial string GenresToolTipText { get; set; }
    [Reactive] public partial string SeriesValueText { get; set; }
    [Reactive] public partial string SeriesValueMaskedText { get; set; }
    [Reactive] public partial string VolumesReadText { get; set; }
    public AvaloniaList<string> SelectedGenres { get; set; } = [];

    public EditSeriesInfoViewModel(Series series, IUserService userService) : base(userService)
    {
        Series = series;
        this.WhenAnyValue(x => x.Series.Demographic)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Subscribe(x => DemographicIndex = SERIES_DEMOGRAPHICS_DICT[x])
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.CurrentUser.Currency)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
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

        this.WhenAnyValue(x => x.CurrentUser.Currency, x => x.Series.Value)
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Subscribe(tuple =>
            {
                var (currency, value) = tuple;
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(AVAILABLE_CURRENCY_WITH_CULTURE[currency].Culture);
                string formatted = value.ToString($"N{cultureInfo.NumberFormat.CurrencyDecimalDigits}", cultureInfo);
                if (cultureInfo.NumberFormat.CurrencyPositivePattern is 0 or 2) // 0 = "$n", 2 = "$ n"
                {
                    SeriesValueText = $"{currency}{formatted}";
                }
                else
                {
                    SeriesValueText = $"{formatted}{currency}";
                }
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.Series.VolumesRead)
            .ObserveOn(RxSchedulers.TaskpoolScheduler)
            .Subscribe(volumesRead => VolumesReadText = $"VOLS READ {volumesRead}")
            .DisposeWith(_disposables);

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
                    foreach (string genre in SelectedGenres.OrderBy(g => g.ToString()))
                    {
                        builder.AppendLine(genre);
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
        foreach (string genreItem in SelectedGenres)
        {
            if (TryParse(genreItem, out SeriesGenre genre))
            {
                newGenres.Add(genre);
            }
        }
        return newGenres;
    }

    public void Dispose()
    {
        SelectedGenres.CollectionChanged -= SeriesGenresChanged;
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}
