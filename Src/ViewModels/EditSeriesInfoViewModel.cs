using System.Collections.Specialized;
using System.Globalization;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Collections;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.Services;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;

namespace Tsundoku.ViewModels;

/// <summary>
/// View model for the Edit Series Info dialog, managing demographic, genre, and value editing for a single series.
/// </summary>
public sealed partial class EditSeriesInfoViewModel : ViewModelBase, IDisposable
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly ISharedSeriesCollectionProvider? _sharedSeriesProvider;
    [Reactive] public partial Series Series { get; set; }
    [Reactive] public partial int DemographicIndex { get; set; }
    [Reactive] public partial string CoverImageUrl { get; set; }
    [Reactive] public partial string GenresToolTipText { get; set; }
    [Reactive] public partial string SeriesValueText { get; set; }
    [Reactive] public partial string SeriesValueMaskedText { get; set; }
    [Reactive] public partial string VolumesReadText { get; set; }
    [Reactive] public partial bool HasPrevious { get; set; }
    [Reactive] public partial bool HasNext { get; set; }
    public AvaloniaList<string> SelectedGenres { get; set; } = [];

    public EditSeriesInfoViewModel(Series series, IUserService userService, ISharedSeriesCollectionProvider? sharedSeriesProvider = null) : base(userService)
    {
        Series = series;
        _sharedSeriesProvider = sharedSeriesProvider;
        RecomputeNavigationFlags();
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

    /// <summary>Swaps to the previous series in the active filtered/sorted collection, if any.</summary>
    public bool NavigatePrevious()
    {
        if (_sharedSeriesProvider is null) return false;
        IReadOnlyList<Series> list = _sharedSeriesProvider.DynamicUserCollection;
        int index = IndexOfCurrent(list);
        if (index <= 0) return false;
        Series = list[index - 1];
        RecomputeNavigationFlags();
        return true;
    }

    /// <summary>Swaps to the next series in the active filtered/sorted collection, if any.</summary>
    public bool NavigateNext()
    {
        if (_sharedSeriesProvider is null) return false;
        IReadOnlyList<Series> list = _sharedSeriesProvider.DynamicUserCollection;
        int index = IndexOfCurrent(list);
        if (index < 0 || index >= list.Count - 1) return false;
        Series = list[index + 1];
        RecomputeNavigationFlags();
        return true;
    }

    private int IndexOfCurrent(IReadOnlyList<Series> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], Series)) return i;
        }
        return -1;
    }

    private void RecomputeNavigationFlags()
    {
        if (_sharedSeriesProvider is null)
        {
            HasPrevious = false;
            HasNext = false;
            return;
        }
        IReadOnlyList<Series> list = _sharedSeriesProvider.DynamicUserCollection;
        int index = IndexOfCurrent(list);
        HasPrevious = index > 0;
        HasNext = index >= 0 && index < list.Count - 1;
    }

    public void Dispose()
    {
        SelectedGenres.CollectionChanged -= SeriesGenresChanged;
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}
