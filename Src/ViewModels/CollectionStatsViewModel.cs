using System.Collections.ObjectModel;
using LiveChartsCore;
using System.Reactive.Linq;
using ReactiveUI.SourceGenerators;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Tsundoku.Models;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Tsundoku.Helpers;
using DynamicData;
using Avalonia.Media;
using ReactiveUI;
using static Tsundoku.Models.Enums.SeriesStatusModel;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace Tsundoku.ViewModels;

/// <summary>
/// View model for the collection statistics window, computing demographic, status, format, rating, genre, and volume distributions.
/// </summary>
public sealed partial class CollectionStatsViewModel : ViewModelBase, IDisposable
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    public ObservableCollection<ISeries> Demographics { get; set; } = [];
    public ObservableValue ShounenCount { get; } = new ObservableValue(0);
    public ObservableValue SeinenCount { get; } = new ObservableValue(0);
    public ObservableValue ShoujoCount { get; } = new ObservableValue(0);
    public ObservableValue JoseiCount { get; } = new ObservableValue(0);
    public ObservableValue UnknownCount { get; } = new ObservableValue(0);
    [Reactive] public partial decimal ShounenPercentage { get; set; }
    [Reactive] public partial decimal SeinenPercentage { get; set; }
    [Reactive] public partial decimal ShoujoPercentage { get; set; }
    [Reactive] public partial decimal JoseiPercentage { get; set; }
    [Reactive] public partial decimal UnknownPercentage { get; set; }

    public ObservableCollection<ISeries> StatusDistribution { get; set; } = [];
    public ObservableValue OngoingCount { get; } = new ObservableValue(0);
    public ObservableValue FinishedCount { get; } = new ObservableValue(0);
    public ObservableValue CancelledCount { get; } = new ObservableValue(0);
    public ObservableValue HiatusCount { get; } = new ObservableValue(0);
    [Reactive] public partial decimal FinishedPercentage { get; set; }
    [Reactive] public partial decimal OngoingPercentage { get; set; }
    [Reactive] public partial decimal CancelledPercentage { get; set; }
    [Reactive] public partial decimal HiatusPercentage { get; set; }

    public ObservableCollection<ISeries> Formats { get; set; } = [];
    public ObservableValue MangaCount { get; } = new ObservableValue(0);
    public ObservableValue ManhwaCount { get; } = new ObservableValue(0);
    public ObservableValue ManhuaCount { get; } = new ObservableValue(0);
    public ObservableValue ManfraCount { get; } = new ObservableValue(0);
    public ObservableValue ComicCount { get; } = new ObservableValue(0);
    public ObservableValue NovelCount { get; } = new ObservableValue(0);
    [Reactive] public partial decimal MangaPercentage { get; set; }
    [Reactive] public partial decimal ManhwaPercentage { get; set; }
    [Reactive] public partial decimal ManhuaPercentage { get; set; }
    [Reactive] public partial decimal ManfraPercentage { get; set; }
    [Reactive] public partial decimal ComicPercentage { get; set; }
    [Reactive] public partial decimal NovelPercentage { get; set; }

    public ObservableCollection<ISeries> RatingDistribution { get; set; } = [];
    public ObservableCollection<Axis> RatingXAxes { get; } = [];
    public ObservableCollection<Axis> RatingYAxes { get; } = [];
    public ObservableValue ZeroRatingCount { get; } = new ObservableValue(0);
    public ObservableValue OneRatingCount { get; } = new ObservableValue(0);
    public ObservableValue TwoRatingCount { get; } = new ObservableValue(0);
    public ObservableValue ThreeRatingCount { get; } = new ObservableValue(0);
    public ObservableValue FourRatingCount { get; } = new ObservableValue(0);
    public ObservableValue FiveRatingCount { get; } = new ObservableValue(0);
    public ObservableValue SixRatingCount { get; } = new ObservableValue(0);
    public ObservableValue SevenRatingCount { get; } = new ObservableValue(0);
    public ObservableValue EightRatingCount { get; } = new ObservableValue(0);
    public ObservableValue NineRatingCount { get; } = new ObservableValue(0);
    public ObservableValue TenRatingCount { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount1 { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount2 { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount3 { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount4 { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount5 { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount6 { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount7 { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount8 { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount9 { get; } = new ObservableValue(0);
    public ObservableValue MaxRatingCount10 { get; } = new ObservableValue(0);

    public ObservableCollection<ISeries> VolumeCountDistribution { get; set; } = [];
    public ObservableCollection<Axis> VolumeCountXAxes { get; set; } = [];
    public ObservableCollection<Axis> VolumeCountYAxes { get; set; } = [];
    public ObservableValue ZeroVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue OneVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue TwoVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue ThreeVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue FourVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue FiveVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue SixVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue SevenVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue EightVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue NineVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue TenVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount1 { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount2 { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount3 { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount4 { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount5 { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount6 { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount7 { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount8 { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount9 { get; } = new ObservableValue(0);
    public ObservableValue MaxVolumeCount10 { get; } = new ObservableValue(0);

    public ObservableCollection<ISeries<KeyValuePair<string, int>>> GenreDistribution { get; set; } = [];
    private Dictionary<string, int> GenreData = [];
    public Axis[] GenreXAxes { get; set; } = [];
    public Axis[] GenreYAxes { get; set; } = [];

    public ObservableCollection<ISeries<KeyValuePair<string, int>>> GenreDistribution1 { get; set; } = [];
    public ObservableCollection<ISeries<KeyValuePair<string, int>>> GenreDistribution2 { get; set; } = [];
    public Axis[] GenreXAxes1 { get; set; } = [];
    public Axis[] GenreXAxes2 { get; set; } = [];
    public Axis[] GenreYAxes1 { get; set; } = [];
    public Axis[] GenreYAxes2 { get; set; } = [];

    [Reactive] public partial int SeriesCount { get; set; }
    [Reactive] public partial int FavoriteCount { get; set; }

    [Reactive] public partial SolidColorBrush PaneBackgroundColor { get; set; }
    [Reactive] public partial SolidColorBrush UnknownRectangleColor { get; set; }
    [Reactive] public partial SolidColorBrush ManhuaRectangleColor { get; set; }
    [Reactive] public partial SolidColorBrush ManfraRectangleColor { get; set; }
    [Reactive] public partial string CollectionValueText { get; set; }

    public ReadOnlyObservableCollection<Series> UserCollection { get; }
    private readonly ISharedSeriesCollectionProvider _sharedSeriesProvider;
    private bool disposedValue;

    public CollectionStatsViewModel(IUserService userService, ISharedSeriesCollectionProvider sharedSeriesProvider) : base(userService)
    {
        _sharedSeriesProvider = sharedSeriesProvider ?? throw new ArgumentNullException(nameof(sharedSeriesProvider));
        UserCollection = _sharedSeriesProvider.DynamicUserCollection;

        SetupStats();
        SetupPieCharts();
        SetupBarCharts();
        UpdateStatsTheme();
    }

    private void UpdateStatsTheme()
    {
        this.WhenAnyValue(
            x => x.CurrentTheme.MenuBGColor,
            x => x.CurrentTheme.MenuButtonBGColor,
            x => x.CurrentTheme.MenuTextColor,
            x => x.CurrentTheme.DividerColor)
        .DistinctUntilChanged()
        .Subscribe(_ =>
        {
            UpdateRatingBarChartColors();
            UpdateVolumeDistributionBarChartColors();
            UpdateGenreBarChartColors();
            UpdatePieChartColors();
            UpdateBackgroundColors();
            UpdateUnknownRectangleColor();
            UpdateManhuaRectangleColor();
        })
        .DisposeWith(_disposables);

        this.WhenAnyValue(
            x => x.CurrentTheme.SeriesCardDescColor,
            x => x.CurrentTheme.SeriesCardTitleColor)
        .DistinctUntilChanged()
        .Subscribe(_ =>
        {
            UpdatePieChartColors();
            UpdateManhuaRectangleColor();
            UpdateManfraRectangleColor();
        })
        .DisposeWith(_disposables);

        this.WhenAnyValue(
            x => x.CurrentTheme.SeriesCardBGColor)
        .DistinctUntilChanged()
        .Subscribe(_ =>
        {
            UpdateBackgroundColors();
            UpdateUnknownRectangleColor();
        })
        .DisposeWith(_disposables);
        
        this.WhenAnyValue(
            x => x.CurrentTheme.SeriesCardStaffColor,
            x => x.CurrentTheme.SeriesButtonIconColor)
        .DistinctUntilChanged()
        .Subscribe(_ =>
        {
            UpdateManfraRectangleColor();
        })
        .DisposeWith(_disposables);
    }

    private void UpdateBackgroundColors()
    {
        PaneBackgroundColor = CurrentTheme.SeriesCardBGColor.Equals(CurrentTheme.MenuBGColor)
            ? CurrentTheme.MenuButtonBGColor
            : CurrentTheme.SeriesCardBGColor;
    }

    private SolidColorBrush GetUnknownColor()
    {
        return  CurrentTheme.SeriesCardDescColor.Color == CurrentTheme.MenuTextColor.Color
            ? CurrentTheme.SeriesCardTitleColor
            : CurrentTheme.SeriesCardDescColor;
    }
    private void UpdateUnknownRectangleColor()
    {
        UnknownRectangleColor = GetUnknownColor();
    }

    private SolidColorBrush GetManhwaColor()
    {
        return CurrentTheme.SeriesCardDescColor.Color == CurrentTheme.MenuTextColor.Color
            ? CurrentTheme.SeriesCardTitleColor
            : CurrentTheme.SeriesCardDescColor;
    }

    private void UpdateManhuaRectangleColor()
    {
        ManhuaRectangleColor = GetManhwaColor();
    }

    private SolidColorBrush GetManfraColor()
    {
        return CurrentTheme.SeriesCardStaffColor.Color == CurrentTheme.SeriesCardTitleColor.Color
            ? CurrentTheme.SeriesButtonIconColor
            : CurrentTheme.SeriesCardStaffColor;
    }
    private void UpdateManfraRectangleColor()
    {
        ManfraRectangleColor = GetManfraColor();
    }

#region Stats
    private void SetupStats()
    {
        _userService.UserCollectionChanges
            .AutoRefresh(x => x.Rating)
            .DistinctUntilChanged()
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .ToCollection()
            .Select(list =>
            {
                decimal total = 0;
                int seriesCount = 0;
                foreach (Series series in list)
                {
                    if (series.Rating >= 0.00m)
                    {
                        total += series.Rating;
                        seriesCount++;
                    }
                }
                return seriesCount != 0 ? decimal.Round(decimal.Divide(total, seriesCount), 2) : 0.00m;
            })
            .Subscribe(rating => _userService.UpdateUser(user => user.MeanRating = rating))
            .DisposeWith(_disposables);

        _userService.UserCollectionChanges
            .AutoRefresh(x => x.VolumesRead)
            .DistinctUntilChanged()
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .ToCollection()
            .Select(seriesCollection => (uint)seriesCollection.Sum(item => item.VolumesRead))
            .Subscribe(volumesRead =>
            {
                _userService.UpdateUser(user => user.VolumesRead = volumesRead);
            })
            .DisposeWith(_disposables);

        Observable.CombineLatest(
            _userService.UserCollectionChanges
                .AutoRefresh(x => x.Value)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .ToCollection()
                .Select(seriesCollection => decimal.Round(seriesCollection.Sum(item => item.Value), 2)),
            this.WhenAnyValue(x => x.CurrentUser.Currency),
            (Value, Currency) => new { Value, Currency }
        )
        .ObserveOn(RxSchedulers.MainThreadScheduler)
        .Subscribe(result =>
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(AVAILABLE_CURRENCY_WITH_CULTURE[result.Currency].Culture);
            if (cultureInfo.NumberFormat.CurrencyPositivePattern is 0 or 2) // 0 = "$n", 2 = "$ n"
            {
                CollectionValueText = $"{result.Currency}{result.Value}";
            }
            else
            {
                CollectionValueText = $"{result.Value}{result.Currency}";
            }

            _userService.UpdateUser(user =>
                user.CollectionValue = CollectionValueText);
        })
        .DisposeWith(_disposables);

        _userService.UserCollectionChanges
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .ToCollection()
            .Subscribe(seriesList =>
            {
                SeriesCount = seriesList.Count;
            })
            .DisposeWith(_disposables);

        _userService.UserCollectionChanges
            .AutoRefresh(x => x.IsFavorite)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .ToCollection()
            .Select(seriesList => seriesList.Count(x => x.IsFavorite))
            .Subscribe(count =>
            {
                FavoriteCount = count;
            })
            .DisposeWith(_disposables);

        _userService.UserCollectionChanges
            .AutoRefresh(x => x.CurVolumeCount)
            .AutoRefresh(x => x.MaxVolumeCount)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ToCollection()
            .Select(seriesList =>
            {
                uint totalCurVolumes = 0;
                uint totalVolumesToBeCollected = 0;

                foreach (Series series in seriesList)
                {
                    totalCurVolumes += series.CurVolumeCount;
                    totalVolumesToBeCollected += series.MaxVolumeCount - series.CurVolumeCount;
                }
                return (totalCurVolumes, totalVolumesToBeCollected);
            })
            .Subscribe(tuple =>
            {
                _userService.UpdateUser(user =>
                {
                    user.NumVolumesCollected = tuple.totalCurVolumes;
                    user.NumVolumesToBeCollected = tuple.totalVolumesToBeCollected;
                });
            })
            .DisposeWith(_disposables);
    }
#endregion

#region BarCharts
    private void SetupBarCharts()
    {
        SolidColorPaint behindBarPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuButtonBGColor).WithAlpha(120));

        SetupRatingBarChart(behindBarPaint);
        SetupVolumeDistributionBarChart(behindBarPaint);
        SetupGenreBarChart();
    }

    private static ColumnSeries<ObservableValue> CreateColumnSeries( ObservableCollection<ObservableValue> values, bool isHoverable = false, double dataLabelsSize = 0, bool ignoresBarPosition = true)
    {
        return new ColumnSeries<ObservableValue>
        {
            Values = values,
            IsHoverable = isHoverable,
            DataLabelsSize = dataLabelsSize,
            IgnoresBarPosition = ignoresBarPosition
        };
    }

    private void SetupRatingBarChart(SolidColorPaint behindBarPaint)
    {
        // A `ColumnSeries<ObservableValue>` named `ratingBarBehindObject` is created for the max rating counts.
        ColumnSeries<ObservableValue> ratingBarBehindObject = CreateColumnSeries(
            new ObservableCollection<ObservableValue> {
                MaxRatingCount, MaxRatingCount1, MaxRatingCount2, MaxRatingCount3,
                MaxRatingCount4, MaxRatingCount5, MaxRatingCount6, MaxRatingCount7,
                MaxRatingCount8, MaxRatingCount9, MaxRatingCount10
            },
            isHoverable: false,
            ignoresBarPosition: true
        );
        ratingBarBehindObject.Fill = behindBarPaint;
        ratingBarBehindObject.Stroke = null;
        RatingDistribution.Add(ratingBarBehindObject);

        // The second series: Actual Rating Counts
        ColumnSeries<ObservableValue> ratingBarObject = CreateColumnSeries(
            new ObservableCollection<ObservableValue> {
                ZeroRatingCount, OneRatingCount, TwoRatingCount, ThreeRatingCount,
                FourRatingCount, FiveRatingCount, SixRatingCount, SevenRatingCount,
                EightRatingCount, NineRatingCount, TenRatingCount
            },
            isHoverable: false,
            dataLabelsSize: 15,
            ignoresBarPosition: true
        );
        ratingBarObject.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor));
        ratingBarObject.DataLabelsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
        ratingBarObject.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        RatingDistribution.Add(ratingBarObject);

        // --- Rating Chart Axes ---
        Axis ratingXAxisObject = new Axis
        {
            LabelsRotation = 0,
            TextSize = 14,
            MinStep = 1,
            ForceStepToMin = true,
            LabelsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor)),
            TicksPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor))
        };
        RatingXAxes.Add(ratingXAxisObject);

        // An `Axis` named `ratingYAxisObject` is added to `RatingYAxes` for the Y-axis.
        Axis ratingYAxisObject = new Axis
        {
            Labels = [],
            MinLimit = 0,
            SeparatorsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor))
        };
        RatingYAxes.Add(ratingYAxisObject);

        _ = _userService.UserCollectionChanges
                .AutoRefresh(x => x.Rating)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .QueryWhenChanged(query => query.Items)
                .Select(seriesList => // Perform all your counting logic on this snapshot
                {
                    int[] counts = new int[11]; // For ratings 0-10
                    foreach (Series series in seriesList)
                    {
                        int bucketIndex = (int)Math.Floor(series.Rating);
                        if (series.Rating == 10)
                        {
                            counts[10]++;
                        }
                        else if (bucketIndex >= 0 && bucketIndex < 10)
                        {
                            counts[bucketIndex]++;
                        }
                    }

                    int zero = counts[0];
                    int one = counts[1];
                    int two = counts[2];
                    int three = counts[3];
                    int four = counts[4];
                    int five = counts[5];
                    int six = counts[6];
                    int seven = counts[7];
                    int eight = counts[8];
                    int nine = counts[9];
                    int ten = counts[10];

                    // Return an anonymous object containing all calculated values
                    return new
                    {
                        Zero = zero,
                        One = one,
                        Two = two,
                        Three = three,
                        Four = four,
                        Five = five,
                        Six = six,
                        Seven = seven,
                        Eight = eight,
                        Nine = nine,
                        Ten = ten,
                        Max = counts.Max()
                    };
                })
                .Subscribe(calculatedValues =>
                {
                    // Update your ObservableValue properties with the new counts
                    ZeroRatingCount.Value = calculatedValues.Zero;
                    OneRatingCount.Value = calculatedValues.One;
                    TwoRatingCount.Value = calculatedValues.Two;
                    ThreeRatingCount.Value = calculatedValues.Three;
                    FourRatingCount.Value = calculatedValues.Four;
                    FiveRatingCount.Value = calculatedValues.Five;
                    SixRatingCount.Value = calculatedValues.Six;
                    SevenRatingCount.Value = calculatedValues.Seven;
                    EightRatingCount.Value = calculatedValues.Eight;
                    NineRatingCount.Value = calculatedValues.Nine;
                    TenRatingCount.Value = calculatedValues.Ten;

                    MaxRatingCount.Value = calculatedValues.Max;
                    MaxRatingCount1.Value = calculatedValues.Max;
                    MaxRatingCount2.Value = calculatedValues.Max;
                    MaxRatingCount3.Value = calculatedValues.Max;
                    MaxRatingCount4.Value = calculatedValues.Max;
                    MaxRatingCount5.Value = calculatedValues.Max;
                    MaxRatingCount6.Value = calculatedValues.Max;
                    MaxRatingCount7.Value = calculatedValues.Max;
                    MaxRatingCount8.Value = calculatedValues.Max;
                    MaxRatingCount9.Value = calculatedValues.Max;
                    MaxRatingCount10.Value = calculatedValues.Max;
                })
                .DisposeWith(_disposables);
    }
    
    private void UpdateRatingBarChartColors()
    {
        // First ColumnSeries: Behind Bar Color
        if (RatingDistribution.Count > 0 && RatingDistribution[0] is ColumnSeries<ObservableValue> ratingBarBehindObject)
        {
            SKColor baseBehindBarColor = ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuButtonBGColor);
            ratingBarBehindObject.Fill = new SolidColorPaint(baseBehindBarColor.WithAlpha(120));
        }

        // Second ColumnSeries: Actual Rating Counts
        if (RatingDistribution.Count > 1 && RatingDistribution[1] is ColumnSeries<ObservableValue> ratingBarObject)
        {
            ratingBarObject.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor));
            ratingBarObject.DataLabelsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
            ratingBarObject.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }

        // --- Update Rating Chart Axes Colors ---
        if (RatingXAxes.Count > 0 && RatingXAxes[0] is Axis ratingXAxisObject)
        {
            ratingXAxisObject.LabelsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
            ratingXAxisObject.TicksPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
        }

        if (RatingYAxes.Count > 0 && RatingYAxes[0] is Axis ratingYAxisObject)
        {
            ratingYAxisObject.SeparatorsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
    }

    private void SetupVolumeDistributionBarChart(SolidColorPaint behindBarPaint)
    {
        ColumnSeries<ObservableValue> volumeBarBehindObject = CreateColumnSeries(
            new ObservableCollection<ObservableValue> {
                MaxVolumeCount, MaxVolumeCount1, MaxVolumeCount2, MaxVolumeCount3,
                MaxVolumeCount4, MaxVolumeCount5, MaxVolumeCount6, MaxVolumeCount7,
                MaxVolumeCount8, MaxVolumeCount9, MaxVolumeCount10
            },
            isHoverable: false,
            ignoresBarPosition: true
        );
        volumeBarBehindObject.Fill = behindBarPaint;
        volumeBarBehindObject.Stroke = null;
        VolumeCountDistribution.Add(volumeBarBehindObject);

        // The second series: Actual Volume Counts
        ColumnSeries<ObservableValue> volumeBarObject = CreateColumnSeries(
            new ObservableCollection<ObservableValue> {
                ZeroVolumeCount, OneVolumeCount, TwoVolumeCount, ThreeVolumeCount,
                FourVolumeCount, FiveVolumeCount, SixVolumeCount, SevenVolumeCount,
                EightVolumeCount, NineVolumeCount, TenVolumeCount
            },
            isHoverable: false,
            dataLabelsSize: 15,
            ignoresBarPosition: true
        );
        volumeBarObject.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor));
        volumeBarObject.DataLabelsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
        volumeBarObject.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        VolumeCountDistribution.Add(volumeBarObject);

        // --- Volume Count Chart Axes ---
        Axis volumeXAxisObject = new Axis
        {
            LabelsRotation = 0,
            TextSize = 14,
            Labels = ["1s", "10s", "20s", "30s", "40s", "50s", "60s", "70s", "80s", "90s", "100+"],
            ForceStepToMin = true,
            MinStep = 1,
            LabelsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor)),
            TicksPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor))
        };
        VolumeCountXAxes.Add(volumeXAxisObject);

        Axis volumeYAxisObject = new Axis
        {
            Labels = Array.Empty<string>(),
            MinLimit = 0,
            SeparatorsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor))
        };
        VolumeCountYAxes.Add(volumeYAxisObject);

        _userService.UserCollectionChanges
            .AutoRefresh(x => x.MaxVolumeCount)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .QueryWhenChanged(query => query.Items)
            .Select(seriesList =>
            {
                int[] volumeCounts = new int[11];

                foreach (Series series in seriesList)
                {
                    uint volume = series.MaxVolumeCount;

                    if (volume >= 1 && volume < 10)
                    {
                        volumeCounts[0]++;
                    }
                    else if (volume >= 10 && volume < 20)
                    {
                        volumeCounts[1]++;
                    }
                    else if (volume >= 20 && volume < 30)
                    {
                        volumeCounts[2]++;
                    }
                    else if (volume >= 30 && volume < 40)
                    {
                        volumeCounts[3]++;
                    }
                    else if (volume >= 40 && volume < 50)
                    {
                        volumeCounts[4]++;
                    }
                    else if (volume >= 50 && volume < 60)
                    {
                        volumeCounts[5]++;
                    }
                    else if (volume >= 60 && volume < 70)
                    {
                        volumeCounts[6]++;
                    }
                    else if (volume >= 70 && volume < 80)
                    {
                        volumeCounts[7]++;
                    }
                    else if (volume >= 80 && volume < 90)
                    {
                        volumeCounts[8]++;
                    }
                    else if (volume >= 90 && volume < 100)
                    {
                        volumeCounts[9]++;
                    }
                    else if (volume >= 100)
                    {
                        volumeCounts[10]++;
                    }
                }

                int max = volumeCounts.Max();

                return new // Return an anonymous object with all calculated volume values
                {
                    Zero = volumeCounts[0],
                    One = volumeCounts[1],
                    Two = volumeCounts[2],
                    Three = volumeCounts[3],
                    Four = volumeCounts[4],
                    Five = volumeCounts[5],
                    Six = volumeCounts[6],
                    Seven = volumeCounts[7],
                    Eight = volumeCounts[8],
                    Nine = volumeCounts[9],
                    Ten = volumeCounts[10],
                    Max = max
                };
            })
            .Subscribe(calculatedValues =>
            {
                ZeroVolumeCount.Value = calculatedValues.Zero;
                OneVolumeCount.Value = calculatedValues.One;
                TwoVolumeCount.Value = calculatedValues.Two;
                ThreeVolumeCount.Value = calculatedValues.Three;
                FourVolumeCount.Value = calculatedValues.Four;
                FiveVolumeCount.Value = calculatedValues.Five;
                SixVolumeCount.Value = calculatedValues.Six;
                SevenVolumeCount.Value = calculatedValues.Seven;
                EightVolumeCount.Value = calculatedValues.Eight;
                NineVolumeCount.Value = calculatedValues.Nine;
                TenVolumeCount.Value = calculatedValues.Ten;

                MaxVolumeCount.Value = calculatedValues.Max;
                MaxVolumeCount1.Value = calculatedValues.Max;
                MaxVolumeCount2.Value = calculatedValues.Max;
                MaxVolumeCount3.Value = calculatedValues.Max;
                MaxVolumeCount4.Value = calculatedValues.Max;
                MaxVolumeCount5.Value = calculatedValues.Max;
                MaxVolumeCount6.Value = calculatedValues.Max;
                MaxVolumeCount7.Value = calculatedValues.Max;
                MaxVolumeCount8.Value = calculatedValues.Max;
                MaxVolumeCount9.Value = calculatedValues.Max;
                MaxVolumeCount10.Value = calculatedValues.Max;
            })
            .DisposeWith(_disposables);
    }

    private void UpdateVolumeDistributionBarChartColors()
    {
        // First ColumnSeries: Behind Bar Color
        if (VolumeCountDistribution.Count > 0 && VolumeCountDistribution[0] is ColumnSeries<ObservableValue> volumeBarBehindObject)
        {
            SKColor baseBehindBarColor = ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuButtonBGColor);
            volumeBarBehindObject.Fill = new SolidColorPaint(baseBehindBarColor.WithAlpha(120));
        }

        // Second ColumnSeries: Actual Volume Counts
        if (VolumeCountDistribution.Count > 1 && VolumeCountDistribution[1] is ColumnSeries<ObservableValue> volumeBarObject)
        {
            volumeBarObject.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor));
            volumeBarObject.DataLabelsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
            volumeBarObject.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }

        // --- Update Volume Count Chart Axes Colors ---
        if (VolumeCountXAxes.Count > 0 && VolumeCountXAxes[0] is Axis volumeXAxisObject)
        {
            volumeXAxisObject.LabelsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
            volumeXAxisObject.TicksPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
        }

        if (VolumeCountYAxes.Count > 0 && VolumeCountYAxes[0] is Axis volumeYAxisObject)
        {
            volumeYAxisObject.SeparatorsPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
    }

    private static RowSeries<KeyValuePair<string, int>> CreateGenreRowSeries()
    {
        return new RowSeries<KeyValuePair<string, int>>
        {
            Values = Array.Empty<KeyValuePair<string, int>>(),
            IsHoverable = false,
            Mapping = (dataPoint, index) => new(index, dataPoint.Value)
        };
    }

    private static Axis CreateGenreXAxis()
    {
        return new Axis
        {
            SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)),
            MinLimit = 0,
            MinStep = 1,
            ForceStepToMin = false,
        };
    }

    private static Axis CreateGenreYAxis()
    {
        return new Axis
        {
            Labels = Array.Empty<string>(),
            ShowSeparatorLines = false,
            ForceStepToMin = true,
        };
    }

    private void UpdateSplitGenreCharts(KeyValuePair<string, int>[] orderedGenreData)
    {
        int midpoint = (orderedGenreData.Length + 1) / 2;
        KeyValuePair<string, int>[] firstHalf = orderedGenreData[..midpoint];
        KeyValuePair<string, int>[] secondHalf = orderedGenreData[midpoint..];

        if (GenreDistribution1.Count > 0 && GenreDistribution1[0] is RowSeries<KeyValuePair<string, int>> series1)
        {
            series1.Values = firstHalf;
        }
        if (GenreYAxes1 is { Length: > 0 })
        {
            GenreYAxes1[0].Labels = firstHalf.AsValueEnumerable().Select(kvp => kvp.Key).ToArray();
        }

        if (GenreDistribution2.Count > 0 && GenreDistribution2[0] is RowSeries<KeyValuePair<string, int>> series2)
        {
            series2.Values = secondHalf;
        }
        if (GenreYAxes2 is { Length: > 0 })
        {
            GenreYAxes2[0].Labels = secondHalf.AsValueEnumerable().Select(kvp => kvp.Key).ToArray();
        }
    }

    private void SetupGenreBarChart()
    {
        if (GenreDistribution.Count == 0)
        {
            GenreDistribution.Add(CreateGenreRowSeries());
        }
        if (GenreDistribution1.Count == 0)
        {
            GenreDistribution1.Add(CreateGenreRowSeries());
        }
        if (GenreDistribution2.Count == 0)
        {
            GenreDistribution2.Add(CreateGenreRowSeries());
        }

        GenreXAxes = [CreateGenreXAxis()];
        GenreYAxes = [CreateGenreYAxis()];
        GenreXAxes1 = [CreateGenreXAxis()];
        GenreYAxes1 = [CreateGenreYAxis()];
        GenreXAxes2 = [CreateGenreXAxis()];
        GenreYAxes2 = [CreateGenreYAxis()];

        foreach (Series series in UserCollection)
        {
            if (series.Genres is not null)
            {
                foreach (SeriesGenre genre in series.Genres)
                {
                    string currentGenre = genre.GetEnumMemberValue();
                    if (!GenreData.TryAdd(currentGenre, 1))
                    {
                        GenreData[currentGenre]++;
                    }
                }
            }
        }

        GenreData = GenreData.AsValueEnumerable().OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        KeyValuePair<string, int>[] orderedData = GenreData.AsValueEnumerable().ToArray();

        if (GenreDistribution.Count > 0 && GenreDistribution[0] is RowSeries<KeyValuePair<string, int>> genreBarObject)
        {
            genreBarObject.Values = orderedData;
        }
        if (GenreYAxes is { Length: > 0 })
        {
            GenreYAxes[0].Labels = [.. GenreData.Keys];
        }

        UpdateSplitGenreCharts(orderedData);

        _userService.UserCollectionChanges
            .AutoRefresh(x => x.Genres)
            .DistinctUntilChanged()
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .QueryWhenChanged(query => query.Items)
            .Select(seriesList =>
            {
                GenreData.Clear();
                foreach (Series series in seriesList)
                {
                    if (series.Genres is not { Count: > 0 })
                        continue;

                    foreach (SeriesGenre genre in series.Genres)
                    {
                        string genreKey = genre.GetEnumMemberValue();

                        if (GenreData.TryGetValue(genreKey, out int count))
                        {
                            GenreData[genreKey] = count + 1;
                        }
                        else
                        {
                            GenreData[genreKey] = 1;
                        }
                    }
                }

                return GenreData.AsValueEnumerable().OrderBy(x => x.Value).ToArray();
            })
            .Subscribe(calculatedGenreData =>
            {
                if (GenreDistribution.Count > 0 && GenreDistribution[0] is RowSeries<KeyValuePair<string, int>> genreBarObject)
                {
                    genreBarObject.Values = calculatedGenreData;
                }

                if (GenreYAxes is { Length: > 0 })
                {
                    GenreYAxes[0].Labels = calculatedGenreData.AsValueEnumerable().Select(kvp => kvp.Key).ToArray();
                }

                UpdateSplitGenreCharts(calculatedGenreData);
            })
            .DisposeWith(_disposables);
    }

    private void ApplyGenreChartColorsToSeries(ObservableCollection<ISeries<KeyValuePair<string, int>>> distribution, Axis[] xAxes, Axis[] yAxes, SKColor fillColor, SKColor textColor, SKColor dividerColor)
    {
        if (distribution.Count > 0 && distribution[0] is RowSeries<KeyValuePair<string, int>> barObject)
        {
            barObject.Fill = new SolidColorPaint(fillColor);
            barObject.DataLabelsPaint = new SolidColorPaint(textColor);
            barObject.Stroke = new SolidColorPaint(dividerColor);
        }

        if (yAxes is { Length: > 0 })
        {
            yAxes[0].LabelsPaint = new SolidColorPaint(textColor);
            yAxes[0].TicksPaint = new SolidColorPaint(textColor);
        }

        if (xAxes is { Length: > 0 })
        {
            xAxes[0].TicksPaint = new SolidColorPaint(textColor);
            xAxes[0].LabelsPaint = new SolidColorPaint(textColor);
            xAxes[0].SeparatorsPaint = new SolidColorPaint(dividerColor);
        }
    }

    private void UpdateGenreBarChartColors()
    {
        SKColor menuBgColor = ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor);
        SKColor menuTextColor = ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor);
        SKColor dividerColor = ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor);

        ApplyGenreChartColorsToSeries(GenreDistribution, GenreXAxes, GenreYAxes, menuBgColor, menuTextColor, dividerColor);
        ApplyGenreChartColorsToSeries(GenreDistribution1, GenreXAxes1, GenreYAxes1, menuBgColor, menuTextColor, dividerColor);
        ApplyGenreChartColorsToSeries(GenreDistribution2, GenreXAxes2, GenreYAxes2, menuBgColor, menuTextColor, dividerColor);
    }
#endregion
    
#region PieCharts
    private void SetupPieCharts()
    {
        int initialUserCollectionCount = _userService.GetCurCollectionCount();
        SetupDemographicPieChart(initialUserCollectionCount);
        SetupStatusPieChart(initialUserCollectionCount);
        SetupFormatPieChart(initialUserCollectionCount);
    }

    private static PieSeries<ObservableValue> CreatePieSeries(ObservableValue countValue, string name)
    {
        return new PieSeries<ObservableValue>
        {
            Values = new ObservableCollection<ObservableValue> { countValue },
            Name = name,
        };
    }

    private void SetupDemographicPieChart(int initialUserCollectionCount)
    {
        Demographics.Add(CreatePieSeries(ShounenCount, "Shounen"));
        Demographics.Add(CreatePieSeries(SeinenCount, "Seinen"));
        Demographics.Add(CreatePieSeries(ShoujoCount, "Shoujo"));
        Demographics.Add(CreatePieSeries(JoseiCount, "Josei"));
        Demographics.Add(CreatePieSeries(UnknownCount, "Unknown"));

        _userService.UserCollectionChanges
            .Where(series => series is not null)
            .AutoRefresh(series => series.Demographic)
            .Group(user => user.Demographic)
            .Subscribe(groupChangeSet =>
            {
                foreach (Change<IGroup<Series, Guid, SeriesDemographic>, SeriesDemographic> change in groupChangeSet)
                {
                    IGroup<Series, Guid, SeriesDemographic> group = change.Current;

                    group?.Cache.CountChanged
                        .StartWith(group.Cache.Count)
                        .Subscribe(count =>
                        {
                            decimal percentage = SeriesCount != 0
                                ? decimal.Round(((decimal)count / SeriesCount) * 100, 2)
                                : 0m;

                            switch (group.Key)
                            {
                                case SeriesDemographic.Shounen:
                                    ShounenCount.Value = count;
                                    ShounenPercentage = percentage;
                                    break;
                                case SeriesDemographic.Seinen:
                                    SeinenCount.Value = count;
                                    SeinenPercentage = percentage;
                                    break;
                                case SeriesDemographic.Shoujo:
                                    ShoujoCount.Value = count;
                                    ShoujoPercentage = percentage;
                                    break;
                                case SeriesDemographic.Josei:
                                    JoseiCount.Value = count;
                                    JoseiPercentage = percentage;
                                    break;
                                case SeriesDemographic.Unknown:
                                    UnknownCount.Value = count;
                                    UnknownPercentage = percentage;
                                    break;
                            }
                        })
                        .DisposeWith(_disposables);
                }
            })
            .DisposeWith(_disposables);

        ShounenPercentage = CalculatePercentage(ShounenCount.Value, initialUserCollectionCount);
        SeinenPercentage = CalculatePercentage(SeinenCount.Value, initialUserCollectionCount);
        ShoujoPercentage = CalculatePercentage(ShoujoCount.Value, initialUserCollectionCount);
        JoseiPercentage = CalculatePercentage(JoseiCount.Value, initialUserCollectionCount);
        UnknownPercentage = CalculatePercentage(UnknownCount.Value, initialUserCollectionCount);
    }

    private void SetupStatusPieChart(int initialUserCollectionCount)
    {
        StatusDistribution.Add(CreatePieSeries(OngoingCount, "Ongoing"));
        StatusDistribution.Add(CreatePieSeries(FinishedCount, "Finished"));
        StatusDistribution.Add(CreatePieSeries(CancelledCount, "Cancelled"));
        StatusDistribution.Add(CreatePieSeries(HiatusCount, "Josei"));

        _userService.UserCollectionChanges
            .AutoRefresh(x => x.Status)
            .Group(user => user.Status)
            .Subscribe(groupChangeSet =>
            {
                foreach (Change<IGroup<Series, Guid, SeriesStatus>, SeriesStatus> change in groupChangeSet)
                {
                    IGroup<Series, Guid, SeriesStatus> group = change.Current;

                    group?.Cache.CountChanged
                        .StartWith(group.Cache.Count)
                        .Subscribe(count =>
                        {
                            decimal percentage = SeriesCount != 0
                                ? Math.Round((decimal)count / SeriesCount * 100, 2)
                                : 0;

                            switch (group.Key)
                            {
                                case SeriesStatus.Ongoing:
                                    OngoingCount.Value = count;
                                    OngoingPercentage = percentage;
                                    break;
                                case SeriesStatus.Finished:
                                    FinishedCount.Value = count;
                                    FinishedPercentage = percentage;
                                    break;
                                case SeriesStatus.Cancelled:
                                    CancelledCount.Value = count;
                                    CancelledPercentage = percentage;
                                    break;
                                case SeriesStatus.Hiatus:
                                    HiatusCount.Value = count;
                                    HiatusPercentage = percentage;
                                    break;
                            }
                        })
                        .DisposeWith(_disposables);
                }
            })
            .DisposeWith(_disposables);
            
        FinishedPercentage = CalculatePercentage(FinishedCount.Value, initialUserCollectionCount);
        OngoingPercentage = CalculatePercentage(OngoingCount.Value, initialUserCollectionCount);
        CancelledPercentage = CalculatePercentage(CancelledCount.Value, initialUserCollectionCount);
        HiatusPercentage = CalculatePercentage(HiatusCount.Value, initialUserCollectionCount);
    }
    private void SetupFormatPieChart(int initialUserCollectionCount)
    {
        Formats.Add(CreatePieSeries(MangaCount, "Manga"));
        Formats.Add(CreatePieSeries(ManhwaCount, "Manhwa"));
        Formats.Add(CreatePieSeries(NovelCount, "Novel"));
        Formats.Add(CreatePieSeries(ComicCount, "Comic"));
        Formats.Add(CreatePieSeries(ManhuaCount, "Manhua"));
        Formats.Add(CreatePieSeries(ManfraCount, "Manfra"));

        _userService.UserCollectionChanges
            .AutoRefresh(x => x.Format)
            .Group(user => user.Format)
            .Subscribe(groupChangeSet =>
            {
                foreach (Change<IGroup<Series, Guid, SeriesFormat>, SeriesFormat> change in groupChangeSet)
                {
                    IGroup<Series, Guid, SeriesFormat> group = change.Current;

                    group?.Cache.CountChanged
                        .StartWith(group.Cache.Count)
                        .Subscribe(count =>
                        {
                            decimal percentage = SeriesCount != 0
                                ? Math.Round((decimal)count / SeriesCount * 100, 2)
                                : 0;

                            switch (group.Key)
                            {
                                case SeriesFormat.Manga:
                                    MangaCount.Value = count;
                                    MangaPercentage = percentage;
                                    break;
                                case SeriesFormat.Manfra:
                                    ManfraCount.Value = count;
                                    ManfraPercentage = percentage;
                                    break;
                                case SeriesFormat.Manhwa:
                                    ManhwaCount.Value = count;
                                    ManhwaPercentage = percentage;
                                    break;
                                case SeriesFormat.Manhua:
                                    ManhuaCount.Value = count;
                                    ManhuaPercentage = percentage;
                                    break;
                                case SeriesFormat.Novel:
                                    NovelCount.Value = count;
                                    NovelPercentage = percentage;
                                    break;
                                case SeriesFormat.Comic:
                                    ComicCount.Value = count;
                                    ComicPercentage = percentage;
                                    break;
                            }
                        })
                        .DisposeWith(_disposables);
                }
            })
            .DisposeWith(_disposables);

        MangaPercentage = CalculatePercentage(MangaCount.Value, initialUserCollectionCount);
        ManhwaPercentage = CalculatePercentage(ManhwaCount.Value, initialUserCollectionCount);
        ManhuaPercentage = CalculatePercentage(ManhuaCount.Value, initialUserCollectionCount);
        ManfraPercentage = CalculatePercentage(ManfraCount.Value, initialUserCollectionCount);
        ComicPercentage = CalculatePercentage(ComicCount.Value, initialUserCollectionCount);
        NovelPercentage = CalculatePercentage(NovelCount.Value, initialUserCollectionCount);
    }

    private void UpdatePieChartColors()
    {
        // --- Update Demographic Pie Chart Series ---
        if (Demographics.Count > 0 && Demographics[0] is PieSeries<ObservableValue> shounen)
        {
            shounen.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor));
            shounen.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (Demographics.Count > 1 && Demographics[1] is PieSeries<ObservableValue> seinen)
        {
            seinen.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuButtonBGColor));
            seinen.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (Demographics.Count > 2 && Demographics[2] is PieSeries<ObservableValue> shoujo)
        {
            shoujo.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
            shoujo.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (Demographics.Count > 3 && Demographics[3] is PieSeries<ObservableValue> josei)
        {
            josei.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
            josei.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (Demographics.Count > 4 && Demographics[4] is PieSeries<ObservableValue> unknown)
        {
            unknown.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(GetUnknownColor())); // Use helper
            unknown.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }

        // --- Update Status Pie Chart Series ---
        if (StatusDistribution.Count > 0 && StatusDistribution[0] is PieSeries<ObservableValue> ongoing)
        {
            ongoing.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor));
            ongoing.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (StatusDistribution.Count > 1 && StatusDistribution[1] is PieSeries<ObservableValue> finished)
        {
            finished.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuButtonBGColor));
            finished.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (StatusDistribution.Count > 2 && StatusDistribution[2] is PieSeries<ObservableValue> cancelled)
        {
            cancelled.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
            cancelled.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (StatusDistribution.Count > 3 && StatusDistribution[3] is PieSeries<ObservableValue> hiatus)
        {
            hiatus.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
            hiatus.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }

        // --- Update Format Pie Chart Series ---
        if (Formats.Count > 0 && Formats[0] is PieSeries<ObservableValue> manga)
        {
            manga.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuButtonBGColor));
            manga.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (Formats.Count > 1 && Formats[1] is PieSeries<ObservableValue> manhwa)
        {
            manhwa.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor));
            manhwa.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (Formats.Count > 2 && Formats[2] is PieSeries<ObservableValue> novel)
        {
            novel.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
            novel.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (Formats.Count > 3 && Formats[3] is PieSeries<ObservableValue> comic)
        {
            comic.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
            comic.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (Formats.Count > 4 && Formats[4] is PieSeries<ObservableValue> manhua)
        {
            manhua.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(GetManhwaColor()));
            manhua.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
        if (Formats.Count > 5 && Formats[5] is PieSeries<ObservableValue> manfra)
        {
            manfra.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(GetManfraColor()));
            manfra.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
        }
    }
#endregion

    private static decimal CalculatePercentage(double? count, decimal total)
    {
        if (count is null || total == 0)
        {
            return 0m;
        }
        return decimal.Round((decimal)count / total * 100m, 2);
    }

    private static SKColor ConvertAvaloniaBrushToSKColor(SolidColorBrush brush)
    {
        return new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
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