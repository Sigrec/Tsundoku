﻿using System.Collections.ObjectModel;
using LiveChartsCore;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Tsundoku.Models;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Tsundoku.Helpers;
using Tsundoku.Services;
using DynamicData;
using Avalonia.Media;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Tsundoku.ViewModels
{
    public partial class CollectionStatsViewModel : ViewModelBase, IDisposable
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public ObservableCollection<ISeries> Demographics { get; set; } = [];
        public ObservableValue ShounenCount { get; } = new ObservableValue(0);
        public ObservableValue SeinenCount { get; } = new ObservableValue(0);
        public ObservableValue ShoujoCount { get; } = new ObservableValue(0);
        public ObservableValue JoseiCount { get; } = new ObservableValue(0);
        public ObservableValue UnknownCount { get; } = new ObservableValue(0);
        [Reactive] public decimal ShounenPercentage { get; set; }
        [Reactive] public decimal SeinenPercentage { get; set; }
        [Reactive] public decimal ShoujoPercentage { get; set; }
        [Reactive] public decimal JoseiPercentage { get; set; }
        [Reactive] public decimal UnknownPercentage { get; set; }

        public ObservableCollection<ISeries> StatusDistribution { get; set; } = [];
        public ObservableValue OngoingCount { get; } = new ObservableValue(0);
        public ObservableValue FinishedCount { get; } = new ObservableValue(0);
        public ObservableValue CancelledCount { get; } = new ObservableValue(0);
        public ObservableValue HiatusCount { get; } = new ObservableValue(0);
        [Reactive] public decimal FinishedPercentage { get; set; }
        [Reactive] public decimal OngoingPercentage { get; set; }
        [Reactive] public decimal CancelledPercentage { get; set; }
        [Reactive] public decimal HiatusPercentage { get; set; }

        public ObservableCollection<ISeries> Formats { get; set; } = [];
        public ObservableValue MangaCount { get; } = new ObservableValue(0);
        public ObservableValue ManhwaCount { get; } = new ObservableValue(0);
        public ObservableValue ManhuaCount { get; } = new ObservableValue(0);
        public ObservableValue ManfraCount { get; } = new ObservableValue(0);
        public ObservableValue ComicCount { get; } = new ObservableValue(0);
        public ObservableValue NovelCount { get; } = new ObservableValue(0);
        [Reactive] public decimal MangaPercentage { get; set; }
        [Reactive] public decimal ManhwaPercentage { get; set; }
        [Reactive] public decimal ManhuaPercentage { get; set; }
        [Reactive] public decimal ManfraPercentage { get; set; }
        [Reactive] public decimal ComicPercentage { get; set; }
        [Reactive] public decimal NovelPercentage { get; set; }

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

        [Reactive] public decimal MeanRating { get; set; }
        [Reactive] public uint VolumesRead { get; set; }
        [Reactive] public string CollectionPrice { get; set; }
        [Reactive] public uint SeriesCount { get; set; }
        [Reactive] public uint FavoriteCount { get; set; }
        [Reactive] public uint UsersNumVolumesCollected { get; set; }
        [Reactive] public uint UsersNumVolumesToBeCollected { get; set; }

        public ReadOnlyObservableCollection<Series> UserCollection { get; }
        private readonly ISharedSeriesCollectionProvider _sharedSeriesProvider;
        private bool disposedValue;

        public CollectionStatsViewModel(IUserService userService, ISharedSeriesCollectionProvider sharedSeriesProvider) : base(userService)
        {
            _sharedSeriesProvider = sharedSeriesProvider ?? throw new ArgumentNullException(nameof(sharedSeriesProvider));
            UserCollection = _sharedSeriesProvider.DynamicUserCollection;

            SetupPieCharts();
            SetupBarCharts();

            _userService.UserCollectionChanges
                .AutoRefresh(x => x.Rating) // react to Rating changes
                .ToCollection()
                .Subscribe(seriesList =>
                {
                    decimal total = 0;
                    int count = 0;

                    foreach (var series in seriesList)
                    {
                        if (series.Rating >= 0)
                        {
                            total += series.Rating;
                            count++;
                        }
                    }

                    MeanRating = count > 0 ? Math.Round(total / count, 1) : 0;
                });

            _userService.UserCollectionChanges
                .AutoRefresh(x => x.VolumesRead)
                .ToCollection()
                .Subscribe(seriesList =>
                {
                    VolumesRead = (uint)seriesList.Sum(x => x.VolumesRead);
                });

            _userService.UserCollectionChanges
                .AutoRefresh(x => x.Value)
                .ToCollection()
                .Subscribe(seriesList =>
                {
                    decimal totalValue = seriesList.Sum(x => x.Value);
                    CollectionPrice = $"{CurrentUser.Currency}{Math.Round(totalValue, 2)}";
                });

            _userService.UserCollectionChanges
                .ToCollection()
                .Subscribe(seriesList =>
                {
                    SeriesCount = (uint)seriesList.Count;
                });

            _userService.UserCollectionChanges
                .AutoRefresh(x => x.IsFavorite)
                .ToCollection()
                .Subscribe(seriesList =>
                {
                    FavoriteCount = (uint)seriesList.Count(x => x.IsFavorite);
                });

            _userService.UserCollectionChanges
                .AutoRefresh(x => x.CurVolumeCount)
                .AutoRefresh(x => x.MaxVolumeCount)
                .ToCollection()
                .Subscribe(seriesList =>
                {
                    uint totalCurVolumes = 0;
                    uint totalVolumesToBeCollected = 0;

                    foreach (var series in seriesList)
                    {
                        totalCurVolumes += series.CurVolumeCount;
                        totalVolumesToBeCollected += (uint)(series.MaxVolumeCount - series.CurVolumeCount);
                    }

                    UsersNumVolumesCollected = totalCurVolumes;
                    UsersNumVolumesToBeCollected = totalVolumesToBeCollected;
                });
        }

#region BarCharts
        private void SetupBarCharts()
        {
            SolidColorPaint behindBarPaint = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuButtonBGColor).WithAlpha(120));

            SetupRatingBarChart(behindBarPaint);
            SetupVolumeDistributionBarChart(behindBarPaint);
            SetupGenreBarChart();

            this.WhenAnyValue(
                x => x.CurrentTheme.MenuBGColor,
                x => x.CurrentTheme.MenuButtonBGColor,
                x => x.CurrentTheme.MenuTextColor,
                x => x.CurrentTheme.DividerColor)
            .Subscribe(_ =>
            {
                UpdateRatingBarChartColors();
                UpdateVolumeDistributionBarChartColors();
                UpdateGenreBarChartColors();
            })
            .DisposeWith(_disposables);
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
                    .AutoRefresh(x => x.Rating) // <--- This is key: it triggers re-evaluation if x.Rating changes
                    .QueryWhenChanged(query => query.Items.ToArray()) // Get the current snapshot of all items
                    .Select(seriesList => // Perform all your counting logic on this snapshot
                    {
                        int[] counts = new int[11]; // For ratings 0-10
                        foreach (var series in seriesList)
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
                    .ObserveOn(RxApp.MainThreadScheduler) // Ensure updates to ObservableValue properties are on the UI thread
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
                .QueryWhenChanged(query => query.Items.ToArray())
                .Select(seriesList =>
                {
                    int[] volumeCounts = new int[11];

                    foreach (var series in seriesList)
                    {
                        var volume = series.MaxVolumeCount;

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
                .ObserveOn(RxApp.MainThreadScheduler) // Ensure UI updates are on the main thread
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

        private void SetupGenreBarChart()
        {
            if (GenreDistribution.Count == 0) // Prevent adding duplicates if called multiple times
            {
                GenreDistribution.Add(
                    new RowSeries<KeyValuePair<string, int>> // Values will be KeyValuePair<string, int>
                    {
                        Values = Array.Empty<KeyValuePair<string, int>>(), // Initialize empty
                        IsHoverable = false,
                        // Mapping is crucial here to tell LiveCharts how to draw from KeyValuePair
                        Mapping = (dataPoint, index) => new(index, dataPoint.Value)
                        // Styling will be handled by ApplyGenreChartTheme
                    }
                );
            }

            // 2. Chart Axes Setup
            GenreXAxes =
            [
                new Axis
                {
                    SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)),
                    MinLimit = 0,
                    MinStep = 1,
                    ForceStepToMin = false,
                    // Styling will be handled by ApplyGenreChartTheme
                }
            ];

            GenreYAxes =
            [
                new Axis
                {
                    Labels = Array.Empty<string>(), // Initialize empty, will be updated in UpdateGenreChart
                    ShowSeparatorLines = false,
                    ForceStepToMin = true,
                    // Styling will be handled by ApplyGenreChartTheme
                }
            ];

            foreach (Series series in UserCollection)
            {
                if (series.Genres != null)
                {
                    foreach (Genre genre in series.Genres)
                    {
                        string currentGenre = genre.GetEnumMemberValue();
                        if (!GenreData.TryAdd(currentGenre, 1))
                        {
                            GenreData[currentGenre]++;
                        }
                    }
                }
            }

            // 2. Order genres by their count for consistent display
            // Convert to Dictionary after ordering to maintain order (though Dictionary itself doesn't guarantee order)
            // ToDictionary(x => x.Key, x => x.Value) ensures a new dictionary with the order.
            GenreData = GenreData.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // 3. Update the chart series values
            // Access the first (and only) RowSeries in your distribution collection.
            if (GenreDistribution.Count > 0 && GenreDistribution[0] is RowSeries<KeyValuePair<string, int>> genreBarObject)
            {
                genreBarObject.Values = GenreData.ToArray(); // Update the values of the existing series
            }

            // 4. Update the Y-axis labels
            if (GenreYAxes != null && GenreYAxes.Length > 0)
            {
                GenreYAxes[0].Labels = [.. GenreData.Keys]; // Update the labels of the existing axis
            }

            _userService.UserCollectionChanges
                .AutoRefresh(x => x.Genres)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(500))
                .QueryWhenChanged(query => query.Items.ToArray()) // Get the current snapshot of all Series
                .Select(seriesList => // Perform the genre counting logic
                {
                    GenreData.Clear();
                    foreach (var series in seriesList)
                    {
                        if (series.Genres is not { Count: > 0 })
                            continue;

                        foreach (var genre in series.Genres)
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

                    return GenreData.OrderBy(x => x.Value).ToArray();
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(calculatedGenreData =>
                {
                    if (GenreDistribution.Count > 0 && GenreDistribution[0] is RowSeries<KeyValuePair<string, int>> genreBarObject)
                    {
                        genreBarObject.Values = calculatedGenreData;
                    }

                    if (GenreYAxes != null && GenreYAxes.Length > 0)
                    {
                        GenreYAxes[0].Labels = calculatedGenreData.Select(kvp => kvp.Key).ToArray();
                    }
                })
                .DisposeWith(_disposables);
        }

        private void UpdateGenreBarChartColors()
        {
            // Get the relevant theme colors. Assuming CurrentTheme is accessible.
            SKColor menuBgColor = ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor);
            SKColor menuTextColor = ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor);
            SKColor dividerColor = ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor);

            // Apply colors to the bar series
            if (GenreDistribution.Count > 0 && GenreDistribution[0] is RowSeries<KeyValuePair<string, int>> genreBarObject)
            {
                genreBarObject.Fill = new SolidColorPaint(menuBgColor);
                genreBarObject.DataLabelsPaint = new SolidColorPaint(menuTextColor);
                genreBarObject.Stroke = new SolidColorPaint(dividerColor);
            }

            // Apply colors to the Y-axis (genre labels)
            if (GenreYAxes != null && GenreYAxes.Length > 0)
            {
                Axis genreYAxisObject = GenreYAxes[0];
                genreYAxisObject.LabelsPaint = new SolidColorPaint(menuTextColor);
                genreYAxisObject.TicksPaint = new SolidColorPaint(menuTextColor);
            }

            // Apply colors to the X-axis (counts)
            if (GenreXAxes != null && GenreXAxes.Length > 0)
            {
                Axis genreXAxisObject = GenreXAxes[0];
                genreXAxisObject.TicksPaint = new SolidColorPaint(menuTextColor);
                genreXAxisObject.LabelsPaint = new SolidColorPaint(menuTextColor);
                genreXAxisObject.SeparatorsPaint = new SolidColorPaint(dividerColor);
            }
        }
#endregion
        
#region PieCharts
        private void SetupPieCharts()
        {
            int initialUserCollectionCount = _userService.GetCurCollectionCount();
            SetupDemographicPieChart(initialUserCollectionCount);
            SetupStatusPieChart(initialUserCollectionCount);
            SetupFormatPieChart(initialUserCollectionCount);

            this.WhenAnyValue(
                x => x.CurrentTheme.MenuBGColor,
                x => x.CurrentTheme.MenuButtonBGColor,
                x => x.CurrentTheme.MenuTextColor,
                x => x.CurrentTheme.DividerColor,
                x => x.CurrentTheme.SeriesCardDescColor,
                x => x.CurrentTheme.SeriesCardTitleColor)
            .Subscribe(_ => UpdatePieChartColors())
            .DisposeWith(_disposables);
        }

        private static PieSeries<ObservableValue> CreatePieSeries(ObservableValue countValue, string name, SolidColorBrush fillBrush, SolidColorBrush strokeBrush)
        {
            // Convert Avalonia.Media.Color to SkiaSharp.SKColor for Fill
            SKColor skFillColor = new SKColor(
                fillBrush.Color.R,
                fillBrush.Color.G,
                fillBrush.Color.B,
                fillBrush.Color.A);

            SKColor skStrokeColor = new SKColor(
                strokeBrush.Color.R,
                strokeBrush.Color.G,
                strokeBrush.Color.B,
                strokeBrush.Color.A);

            return new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { countValue },
                Name = name,
                Fill = new SolidColorPaint(skFillColor),   // Uses SkiaSharp.SKColor
                Stroke = new SolidColorPaint(skStrokeColor) // Uses SkiaSharp.SKColor
            };
        }

        private SolidColorBrush GetConditionalFillBrush()
        {
            return CurrentTheme.SeriesCardDescColor.Color == CurrentTheme.MenuTextColor.Color ?
                CurrentTheme.SeriesCardTitleColor :
                CurrentTheme.SeriesCardDescColor;
        }

        private void SetupDemographicPieChart(int initialUserCollectionCount)
        {
            Demographics.Add(CreatePieSeries(ShounenCount, "Shounen", CurrentTheme.MenuBGColor, CurrentTheme.DividerColor));
            Demographics.Add(CreatePieSeries(SeinenCount, "Seinen", CurrentTheme.MenuButtonBGColor, CurrentTheme.DividerColor));
            Demographics.Add(CreatePieSeries(ShoujoCount, "Shoujo", CurrentTheme.MenuTextColor, CurrentTheme.DividerColor));
            Demographics.Add(CreatePieSeries(JoseiCount, "Josei", CurrentTheme.DividerColor, CurrentTheme.DividerColor));
            Demographics.Add(CreatePieSeries(UnknownCount, "Unknown", GetConditionalFillBrush(), CurrentTheme.DividerColor));

            _userService.UserCollectionChanges
                .AutoRefresh(x => x.Demographic) // detect property changes
                .Group(user => user.Demographic)
                .Subscribe(groupChangeSet =>
                {
                    int collectionCount = _userService.GetCurCollectionCount();
                    foreach (Change<IGroup<Series, Guid, Demographic>, Demographic> change in groupChangeSet)
                    {
                        IGroup<Series, Guid, Demographic> group = change.Current;

                        // Subscribe to count changes in this group
                        group?.Cache.CountChanged
                            .StartWith(group.Cache.Count)
                            .Subscribe(count =>
                            {
                                decimal percentage = collectionCount != 0
                                    ? Math.Round((decimal)count / collectionCount * 100, 2)
                                    : 0;

                                switch (group.Key)
                                {
                                    case Demographic.Shounen:
                                        ShounenCount.Value = count;
                                        ShounenPercentage = percentage;
                                        break;
                                    case Demographic.Seinen:
                                        SeinenCount.Value = count;
                                        SeinenPercentage = percentage;
                                        break;
                                    case Demographic.Shoujo:
                                        ShoujoCount.Value = count;
                                        ShoujoPercentage = percentage;
                                        break;
                                    case Demographic.Josei:
                                        JoseiCount.Value = count;
                                        JoseiPercentage = percentage;
                                        break;
                                    case Demographic.Unknown:
                                        UnknownCount.Value = count;
                                        UnknownPercentage = percentage;
                                        break;
                                }
                            });
                    }
                });

            ShounenPercentage = CalculatePercentage(ShounenCount.Value, initialUserCollectionCount);
            SeinenPercentage = CalculatePercentage(SeinenCount.Value, initialUserCollectionCount);
            ShoujoPercentage = CalculatePercentage(ShoujoCount.Value, initialUserCollectionCount);
            JoseiPercentage = CalculatePercentage(JoseiCount.Value, initialUserCollectionCount);
            UnknownPercentage = CalculatePercentage(UnknownCount.Value, initialUserCollectionCount);
        }

        private void SetupStatusPieChart(int initialUserCollectionCount)
        {
            StatusDistribution.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { OngoingCount },
                Name = "Ongoing"
            });
            StatusDistribution.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { FinishedCount },
                Name = "Finished"
            });
            StatusDistribution.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { CancelledCount },
                Name = "Cancelled"
            });
            StatusDistribution.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { HiatusCount },
                Name = "Hiatus"
            });

            _userService.UserCollectionChanges
                .AutoRefresh(x => x.Status) // detect property changes
                .Group(user => user.Status)
                .Subscribe(groupChangeSet =>
                {
                    int collectionCount = _userService.GetCurCollectionCount();
                    foreach (Change<IGroup<Series, Guid, Status>, Status> change in groupChangeSet)
                    {
                        IGroup<Series, Guid, Status> group = change.Current;

                        // Subscribe to count changes in this group
                        group?.Cache.CountChanged
                            .StartWith(group.Cache.Count)
                            .Subscribe(count =>
                            {
                                decimal percentage = collectionCount != 0
                                    ? Math.Round((decimal)count / collectionCount * 100, 2)
                                    : 0;

                                switch (group.Key)
                                {
                                    case Status.Ongoing:
                                        OngoingCount.Value = count;
                                        OngoingPercentage = percentage;
                                        break;
                                    case Status.Finished:
                                        FinishedCount.Value = count;
                                        FinishedPercentage = percentage;
                                        break;
                                    case Status.Cancelled:
                                        CancelledCount.Value = count;
                                        CancelledPercentage = percentage;
                                        break;
                                    case Status.Hiatus:
                                        HiatusCount.Value = count;
                                        HiatusPercentage = percentage;
                                        break;
                                }
                            });
                    }
                });
                
            FinishedPercentage = CalculatePercentage(FinishedCount.Value, initialUserCollectionCount);
            OngoingPercentage = CalculatePercentage(OngoingCount.Value, initialUserCollectionCount);
            CancelledPercentage = CalculatePercentage(CancelledCount.Value, initialUserCollectionCount);
            HiatusPercentage = CalculatePercentage(HiatusCount.Value, initialUserCollectionCount);
        }

        private void SetupFormatPieChart(int initialUserCollectionCount)
        {
            Formats.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { MangaCount },
                Name = "Manga"
            });
            Formats.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { ManhwaCount },
                Name = "Manhwa"
            });
            Formats.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { ManhuaCount },
                Name = "Manhua"
            });
            Formats.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { ManfraCount },
                Name = "Manfra"
            });
            Formats.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { ComicCount },
                Name = "Comic"
            });
            Formats.Add(new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { NovelCount },
                Name = "Novel"
            });

            _userService.UserCollectionChanges
                .AutoRefresh(x => x.Format) // detect property changes
                .Group(user => user.Format)
                .Subscribe(groupChangeSet =>
                {
                    int collectionCount = _userService.GetCurCollectionCount();
                    foreach (Change<IGroup<Series, Guid, Format>, Format> change in groupChangeSet)
                    {
                        IGroup<Series, Guid, Format> group = change.Current;

                        // Subscribe to count changes in this group
                        group?.Cache.CountChanged
                            .StartWith(group.Cache.Count)
                            .Subscribe(count =>
                            {
                                decimal percentage = collectionCount != 0
                                    ? Math.Round((decimal)count / collectionCount * 100, 2)
                                    : 0;

                                switch (group.Key)
                                {
                                    case Format.Manga:
                                        MangaCount.Value = count;
                                        MangaPercentage = percentage;
                                        break;
                                    case Format.Manfra:
                                        ManfraCount.Value = count;
                                        ManfraPercentage = percentage;
                                        break;
                                    case Format.Manhwa:
                                        ManhwaCount.Value = count;
                                        ManhwaPercentage = percentage;
                                        break;
                                    case Format.Manhua:
                                        ManhuaCount.Value = count;
                                        ManhuaPercentage = percentage;
                                        break;
                                    case Format.Novel:
                                        NovelCount.Value = count;
                                        NovelPercentage = percentage;
                                        break;
                                    case Format.Comic:
                                        ComicCount.Value = count;
                                        ComicPercentage = percentage;
                                        break;
                                }
                            });
                    }
                });
            
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
                unknown.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(GetConditionalFillBrush())); // Use helper
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
                manga.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuBGColor));
                manga.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
            }
            if (Formats.Count > 1 && Formats[1] is PieSeries<ObservableValue> manhwa)
            {
                manhwa.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuButtonBGColor));
                manhwa.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
            }
            if (Formats.Count > 2 && Formats[2] is PieSeries<ObservableValue> manhua)
            {
                manhua.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.MenuTextColor));
                manhua.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
            }
            if (Formats.Count > 3 && Formats[3] is PieSeries<ObservableValue> novel)
            {
                novel.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
                novel.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
            }
            if (Formats.Count > 4 && Formats[4] is PieSeries<ObservableValue> comic)
            {
                comic.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(GetConditionalFillBrush()));
                comic.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
            }
            if (Formats.Count > 5 && Formats[5] is PieSeries<ObservableValue> manfra)
            {
                manfra.Fill = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(GetConditionalFillBrush()));
                manfra.Stroke = new SolidColorPaint(ConvertAvaloniaBrushToSKColor(CurrentTheme.DividerColor));
            }
        }
#endregion

        private static decimal CalculatePercentage(double? count, decimal total)
        {
            if (count  == null || total == 0) return 0M;
            return Math.Round((decimal)count / total * 100M, 2);
        }

        private static SKColor ConvertAvaloniaBrushToSKColor(SolidColorBrush brush)
        {
            return new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        }

        protected virtual void Dispose(bool disposing)
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
}