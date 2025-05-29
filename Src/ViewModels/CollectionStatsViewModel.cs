﻿using System.Collections.ObjectModel;
using LiveChartsCore;
using ReactiveUI;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Tsundoku.Models;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Tsundoku.Helpers;
using Tsundoku.Services;

namespace Tsundoku.ViewModels
{
    public partial class CollectionStatsViewModel : ViewModelBase, IDisposable
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public ObservableCollection<ISeries> Demographics { get; set; } = new ObservableCollection<ISeries>();
        public ObservableValue ShounenCount { get; set; }
        public ObservableValue SeinenCount { get; set; }
        public ObservableValue ShoujoCount { get; set; }
        public ObservableValue JoseiCount { get; set; }
        public ObservableValue UnknownCount { get; set; }

        public ObservableCollection<ISeries> StatusDistribution { get; set; }
        public ObservableValue OngoingCount { get; set; }
        public ObservableValue FinishedCount { get; set; }
        public ObservableValue CancelledCount { get; set; }
        public ObservableValue HiatusCount { get; set; }

        public ObservableCollection<ISeries> Formats { get; set; }
        public ObservableValue MangaCount { get; set; }
        public ObservableValue ManhwaCount { get; set; }
        public ObservableValue ManhuaCount { get; set; }
        public ObservableValue ManfraCount { get; set; }
        public ObservableValue ComicCount { get; set; }
        public ObservableValue NovelCount { get; set; }

        public ObservableCollection<ISeries> RatingDistribution { get; set; }
        private List<double?> RatingArray; // No longer initialized here
        public ObservableCollection<Axis> RatingXAxes { get; set; }
        public ObservableCollection<Axis> RatingYAxes { get; set; }
        public ObservableValue ZeroRatingCount { get; set; }
        public ObservableValue OneRatingCount { get; set; }
        public ObservableValue TwoRatingCount { get; set; }
        public ObservableValue ThreeRatingCount { get; set; }
        public ObservableValue FourRatingCount { get; set; }
        public ObservableValue FiveRatingCount { get; set; }
        public ObservableValue SixRatingCount { get; set; }
        public ObservableValue SevenRatingCount { get; set; }
        public ObservableValue EightRatingCount { get; set; }
        public ObservableValue NineRatingCount { get; set; }
        public ObservableValue TenRatingCount { get; set; }
        public ObservableValue MaxRatingCount { get; set; }
        public ObservableValue MaxRatingCount1 { get; set; }
        public ObservableValue MaxRatingCount2 { get; set; }
        public ObservableValue MaxRatingCount3 { get; set; }
        public ObservableValue MaxRatingCount4 { get; set; }
        public ObservableValue MaxRatingCount5 { get; set; }
        public ObservableValue MaxRatingCount6 { get; set; }
        public ObservableValue MaxRatingCount7 { get; set; }
        public ObservableValue MaxRatingCount8 { get; set; }
        public ObservableValue MaxRatingCount9 { get; set; }
        public ObservableValue MaxRatingCount10 { get; set; }

        public ObservableCollection<ISeries> VolumeCountDistribution { get; set; }
        private List<double?> VolumeCountArray; // No longer initialized here
        public ObservableCollection<Axis> VolumeCountXAxes { get; set; }
        public ObservableCollection<Axis> VolumeCountYAxes { get; set; }
        public ObservableValue ZeroVolumeCount { get; set; }
        public ObservableValue OneVolumeCount { get; set; }
        public ObservableValue TwoVolumeCount { get; set; }
        public ObservableValue ThreeVolumeCount { get; set; }
        public ObservableValue FourVolumeCount { get; set; }
        public ObservableValue FiveVolumeCount { get; set; }
        public ObservableValue SixVolumeCount { get; set; }
        public ObservableValue SevenVolumeCount { get; set; }
        public ObservableValue EightVolumeCount { get; set; }
        public ObservableValue NineVolumeCount { get; set; }
        public ObservableValue TenVolumeCount { get; set; }
        public ObservableValue MaxVolumeCount { get; set; }
        public ObservableValue MaxVolumeCount1 { get; set; }
        public ObservableValue MaxVolumeCount2 { get; set; }
        public ObservableValue MaxVolumeCount3 { get; set; }
        public ObservableValue MaxVolumeCount4 { get; set; }
        public ObservableValue MaxVolumeCount5 { get; set; }
        public ObservableValue MaxVolumeCount6 { get; set; }
        public ObservableValue MaxVolumeCount7 { get; set; }
        public ObservableValue MaxVolumeCount8 { get; set; }
        public ObservableValue MaxVolumeCount9 { get; set; }
        public ObservableValue MaxVolumeCount10 { get; set; }

        public ObservableCollection<ISeries<KeyValuePair<string, int>>> GenreDistribution { get; set; }
        private Dictionary<string, int> GenreData; // No longer initialized here
        private bool disposedValue;

        public Axis[] GenreXAxes { get; set; }
        public Axis[] GenreYAxes { get; set; }

        [Reactive] public decimal MeanRating { get; set; }
        [Reactive] public uint VolumesRead { get; set; }
        [Reactive] public string CollectionPrice { get; set; }
        [Reactive] public uint SeriesCount { get; set; }
        [Reactive] public uint FavoriteCount { get; set; }
        [Reactive] public decimal FinishedPercentage { get; set; }
        [Reactive] public decimal OngoingPercentage { get; set; }
        [Reactive] public decimal CancelledPercentage { get; set; }
        [Reactive] public decimal HiatusPercentage { get; set; }
        [Reactive] public decimal ShounenPercentage { get; set; }
        [Reactive] public decimal SeinenPercentage { get; set; }
        [Reactive] public decimal ShoujoPercentage { get; set; }
        [Reactive] public decimal JoseiPercentage { get; set; }
        [Reactive] public decimal UnknownPercentage { get; set; }
        [Reactive] public decimal MangaPercentage { get; set; }
        [Reactive] public decimal ManhwaPercentage { get; set; }
        [Reactive] public decimal ManhuaPercentage { get; set; }
        [Reactive] public decimal ManfraPercentage { get; set; }
        [Reactive] public decimal ComicPercentage { get; set; }
        [Reactive] public decimal NovelPercentage { get; set; }
        [Reactive] public uint UsersNumVolumesCollected { get; set; }
        [Reactive] public uint UsersNumVolumesToBeCollected { get; set; }

        public ReadOnlyObservableCollection<Series> UserCollection { get; }
        private readonly ISharedSeriesCollectionProvider _sharedSeriesProvider;

        public CollectionStatsViewModel(IUserService userService, ISharedSeriesCollectionProvider sharedSeriesProvider) : base(userService)
        {
            _sharedSeriesProvider = sharedSeriesProvider ?? throw new ArgumentNullException(nameof(sharedSeriesProvider));
            UserCollection = _sharedSeriesProvider.DynamicUserCollection;
            // this.WhenDeactivated().Subscribe(_ => _disposables.Dispose());
            
            ShounenCount = new ObservableValue(UserCollection.Count(series => series.Demographic == Demographic.Shounen));
            SeinenCount = new ObservableValue(UserCollection.Count(series => series.Demographic == Demographic.Seinen));
            ShoujoCount = new ObservableValue(UserCollection.Count(series => series.Demographic == Demographic.Shoujo));
            JoseiCount = new ObservableValue(UserCollection.Count(series => series.Demographic == Demographic.Josei));
            UnknownCount = new ObservableValue(UserCollection.Count(series => series.Demographic == Demographic.Unknown));

            StatusDistribution = new ObservableCollection<ISeries>();
            OngoingCount = new ObservableValue(UserCollection.Count(series => series.Status == Status.Ongoing));
            FinishedCount = new ObservableValue(UserCollection.Count(series => series.Status == Status.Finished));
            CancelledCount = new ObservableValue(UserCollection.Count(series => series.Status == Status.Cancelled));
            HiatusCount = new ObservableValue(UserCollection.Count(series => series.Status == Status.Hiatus));

            Formats = new ObservableCollection<ISeries>();
            MangaCount = new ObservableValue(UserCollection.Count(series => series.Format == Format.Manga));
            ManhwaCount = new ObservableValue(UserCollection.Count(series => series.Format == Format.Manhwa));
            ManhuaCount = new ObservableValue(UserCollection.Count(series => series.Format == Format.Manhua));
            ManfraCount = new ObservableValue(UserCollection.Count(series => series.Format == Format.Manfra));
            ComicCount = new ObservableValue(UserCollection.Count(series => series.Format == Format.Comic));
            NovelCount = new ObservableValue(UserCollection.Count(series => series.Format == Format.Novel));

            RatingDistribution = new ObservableCollection<ISeries>();
            RatingArray = new List<double?>(10);
            RatingXAxes = new ObservableCollection<Axis>();
            RatingYAxes = new ObservableCollection<Axis>();
            ZeroRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating > 0 && series.Rating < 1));
            OneRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating >= 1 && series.Rating < 2));
            TwoRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating >= 2 && series.Rating < 3));
            ThreeRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating >= 3 && series.Rating < 4));
            FourRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating >= 4 && series.Rating < 5));
            FiveRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating >= 5 && series.Rating < 6));
            SixRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating >= 7 && series.Rating < 8));
            SevenRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating >= 7 && series.Rating < 8));
            EightRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating >= 8 && series.Rating < 9));
            NineRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating >= 9 && series.Rating < 10));
            TenRatingCount = new ObservableValue(UserCollection.Count(series => series.Rating == 10));
            MaxRatingCount = new ObservableValue(0);
            MaxRatingCount1 = new ObservableValue(0);
            MaxRatingCount2 = new ObservableValue(0);
            MaxRatingCount3 = new ObservableValue(0);
            MaxRatingCount4 = new ObservableValue(0);
            MaxRatingCount5 = new ObservableValue(0);
            MaxRatingCount6 = new ObservableValue(0);
            MaxRatingCount7 = new ObservableValue(0);
            MaxRatingCount8 = new ObservableValue(0);
            MaxRatingCount9 = new ObservableValue(0);
            MaxRatingCount10 = new ObservableValue(0);

            VolumeCountDistribution = new ObservableCollection<ISeries>();
            VolumeCountArray = new List<double?>(10);
            VolumeCountXAxes = new ObservableCollection<Axis>();
            VolumeCountYAxes = new ObservableCollection<Axis>();
            ZeroVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 1 && series.MaxVolumeCount < 10));
            OneVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 10 && series.MaxVolumeCount < 20));
            TwoVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 20 && series.MaxVolumeCount < 30));
            ThreeVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 30 && series.MaxVolumeCount < 40));
            FourVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 40 && series.MaxVolumeCount < 50));
            FiveVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 50 && series.MaxVolumeCount < 60));
            SixVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 60 && series.MaxVolumeCount < 70));
            SevenVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 70 && series.MaxVolumeCount < 80));
            EightVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 80 && series.MaxVolumeCount < 90));
            NineVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 90 && series.MaxVolumeCount < 100));
            TenVolumeCount = new ObservableValue(UserCollection.Count(series => series.MaxVolumeCount >= 100));
            MaxVolumeCount = new ObservableValue(0);
            MaxVolumeCount1 = new ObservableValue(0);
            MaxVolumeCount2 = new ObservableValue(0);
            MaxVolumeCount3 = new ObservableValue(0);
            MaxVolumeCount4 = new ObservableValue(0);
            MaxVolumeCount5 = new ObservableValue(0);
            MaxVolumeCount6 = new ObservableValue(0);
            MaxVolumeCount7 = new ObservableValue(0);
            MaxVolumeCount8 = new ObservableValue(0);
            MaxVolumeCount9 = new ObservableValue(0);
            MaxVolumeCount10 = new ObservableValue(0);

            GenreDistribution = new ObservableCollection<ISeries<KeyValuePair<string, int>>>();
            GenreData = new Dictionary<string, int>();
            GenreXAxes = [];
            GenreYAxes = [];

            MeanRating = 0; // Initialize reactive properties
            VolumesRead = 0;
            CollectionPrice = string.Empty;
            SeriesCount = (uint)UserCollection.Count;
            FavoriteCount = (uint)UserCollection.Count(series => series.IsFavorite);
            // GenerateStats();

            // this.WhenAnyValue(x => x.MeanRating).Subscribe(x => MainUser.MeanRating = x);
            // this.WhenAnyValue(x => x.VolumesRead).Subscribe(x => MainUser.VolumesRead = x);
            // this.WhenAnyValue(x => x.CollectionPrice).Subscribe(x => MainUser.CollectionPrice = x);
            // this.WhenAnyValue(x => x.UsersNumVolumesCollected).Subscribe(x => MainUser.NumVolumesCollected = x);
            // this.WhenAnyValue(x => x.UsersNumVolumesToBeCollected).Subscribe(x => MainUser.NumVolumesToBeCollected = x);
        }

        public void UpdateChartStats()
        {
            UpdateStatusChartValues();
            UpdateStatusPercentages();
        }

        /// <summary>
        /// Updates the values in the demographic pie chart
        /// </summary>
        public void UpdateDemographicChartValues()
        {
            ShounenCount.Value = UserCollection.Count(series => series.Demographic == Demographic.Shounen);
            SeinenCount.Value = UserCollection.Count(series => series.Demographic == Demographic.Seinen);
            ShoujoCount.Value = UserCollection.Count(series => series.Demographic == Demographic.Shoujo);
            JoseiCount.Value = UserCollection.Count(series => series.Demographic == Demographic.Josei);
            UnknownCount.Value = UserCollection.Count(series => series.Demographic == Demographic.Unknown);
        }

        /// <summary>
        /// Initial Generation of the charts used in the stats window
        /// </summary>
        public void GenerateCharts()
        {
            Demographics.Add(new PieSeries<ObservableValue> 
            { 
                Values = new ObservableCollection<ObservableValue> { ShounenCount },
                Name = "Shounen"
            });
            Demographics.Add(new PieSeries<ObservableValue>
            { 
                Values = new ObservableCollection<ObservableValue> { SeinenCount },
                Name = "Seinen"
            });
            Demographics.Add(new PieSeries<ObservableValue>
            { 
                Values = new ObservableCollection<ObservableValue> { ShoujoCount },
                Name = "Shoujo"
            });
            Demographics.Add(new PieSeries<ObservableValue>
            { 
                Values = new ObservableCollection<ObservableValue> { JoseiCount },
                Name = "Josei"
            });
            Demographics.Add(new PieSeries<ObservableValue>
            { 
                Values = new ObservableCollection<ObservableValue> { UnknownCount },
                Name = "Unknown"
            });

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

            RatingDistribution.Add(new ColumnSeries<ObservableValue> 
            { 
                IsHoverable = false,
                Values = new ObservableCollection<ObservableValue> 
                { 
                    MaxRatingCount, MaxRatingCount1, MaxRatingCount2, MaxRatingCount3, MaxRatingCount4, MaxRatingCount5, MaxRatingCount6, MaxRatingCount7, MaxRatingCount8, MaxRatingCount9, MaxRatingCount10
                },
                Stroke = null,
                IgnoresBarPosition = true
            });
            RatingDistribution.Add(new ColumnSeries<ObservableValue> 
            { 
                IsHoverable = false,
                Values = new ObservableCollection<ObservableValue>
                {
                    ZeroRatingCount, OneRatingCount, TwoRatingCount, ThreeRatingCount, FourRatingCount, FiveRatingCount, SixRatingCount, SevenRatingCount, EightRatingCount, NineRatingCount, TenRatingCount
                },
                DataLabelsSize = 15,
                IgnoresBarPosition = true
            });
            RatingXAxes.Add(new Axis
            {
                LabelsRotation = 0,
                TextSize = 14,
                MinStep = 1,
                ForceStepToMin = true
            });
            RatingYAxes.Add(new Axis
            {
                Labels = Array.Empty<string>(),
                MinLimit = 0
            });

            VolumeCountDistribution.Add(new ColumnSeries<ObservableValue> 
            { 
                IsHoverable = false,
                Values = new ObservableCollection<ObservableValue> 
                { 
                    MaxVolumeCount, MaxVolumeCount1, MaxVolumeCount2, MaxVolumeCount3, MaxVolumeCount4, MaxVolumeCount5, MaxVolumeCount6, MaxVolumeCount7, MaxVolumeCount8, MaxVolumeCount9, MaxVolumeCount10
                },
                Stroke = null,
                IgnoresBarPosition = true
            });
            VolumeCountDistribution.Add(new ColumnSeries<ObservableValue> 
            { 
                IsHoverable = false,
                Values = new ObservableCollection<ObservableValue>
                {
                    ZeroVolumeCount, OneVolumeCount, TwoVolumeCount, ThreeVolumeCount, FourVolumeCount, FiveVolumeCount, SixVolumeCount, SevenVolumeCount, EightVolumeCount, NineVolumeCount, TenVolumeCount
                },
                DataLabelsSize = 15,
                IgnoresBarPosition = true
            });
            VolumeCountXAxes.Add(new Axis
            {
                LabelsRotation = 0,
                TextSize = 14,
                Labels = [ "1s", "10s", "20s", "30s", "40s", "50s", "60s", "70s", "80s", "90s", "100s", ] ,
                ForceStepToMin = true
            });
            VolumeCountYAxes.Add(new Axis
            {
                Labels = Array.Empty<string>(),
                MinLimit = 0
            });

            // Genre chart setup
            foreach(Series series in UserCollection)
            {
                if (series.Genres != null)
                {
                    foreach (Genre genre in series.Genres)
                    {
                        string curGenre = genre.GetStringValue();
                        if (!GenreData.TryAdd(curGenre, 1))
                        {
                            GenreData[curGenre] += 1;
                        }
                    }
                }
            }
            GenreData = GenreData.OrderBy(x => x.Value).ToDictionary();

            GenreDistribution.Add(
                new RowSeries<KeyValuePair<string, int>>
                {
                    Values = GenreData.ToArray(),
                    IsHoverable = false,
                    Mapping = (dataPoint, index) => new(index, dataPoint.Value)
                }
            );

            GenreXAxes = [ 
                new Axis 
                { 
                    SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)),
                    MinLimit = 0,
                    MinStep = 1,
                    ForceStepToMin = false,
                } 
            ];

            GenreYAxes = [ 
                new Axis 
                {
                    Labels = [.. GenreData.Keys],
                    ShowSeparatorLines = false,
                    ForceStepToMin = true
                } 
            ];
        }

        public void UpdateGenreChart(IEnumerable<Genre> addedGenres, IEnumerable<Genre> removedGenres)
        {
            foreach (Genre genre in addedGenres)
            {
                string curGenre = genre.GetStringValue();
                if (!GenreData.TryAdd(curGenre, 1))
                {
                    GenreData[curGenre] += 1;
                }
            }

            foreach (Genre genre in removedGenres)
            {
                string curGenre = genre.GetStringValue();
                if (GenreData[curGenre] > 1)
                {
                    GenreData[curGenre] -= 1;
                }
                else
                {
                    GenreData.Remove(curGenre);
                }
            }
            GenreData = GenreData.OrderBy(x => x.Value).ToDictionary();
            GenreDistribution[0].Values = GenreData.ToArray();
            GenreYAxes[0].Labels = [.. GenreData.Keys];
        }

        public void UpdateGenreChart()
        {
            GenreData.Clear();
            foreach(Series series in UserCollection)
            {
                if (series.Genres != null)
                {
                    foreach (Genre genre in series.Genres)
                    {
                        string curGenre = genre.GetStringValue();
                        if (!GenreData.TryAdd(curGenre, 1))
                        {
                            GenreData[curGenre] += 1;
                        }
                    }
                }
            }
            GenreData = GenreData.OrderBy(x => x.Value).ToDictionary();
            GenreDistribution[0].Values = GenreData.ToArray();
            GenreYAxes[0].Labels = [.. GenreData.Keys];
        }

        /// <summary>
        /// Updates the values in the status pie chart
        /// </summary>
        public void UpdateStatusChartValues()
        {
            OngoingCount.Value = UserCollection.Count(series => series.Status == Status.Ongoing);
            FinishedCount.Value = UserCollection.Count(series => series.Status == Status.Finished);
            CancelledCount.Value = UserCollection.Count(series => series.Status == Status.Cancelled);
            HiatusCount.Value = UserCollection.Count(series => series.Status == Status.Hiatus);
        }

        /// <summary>
        /// Updates the values in the format pie chart
        /// </summary>
        public void UpdateFormatChartValues()
        {
            MangaCount.Value = UserCollection.Count(series => series.Format == Format.Manga);
            ManhwaCount.Value = UserCollection.Count(series => series.Format == Format.Manhwa);
            ManhuaCount.Value = UserCollection.Count(series => series.Format == Format.Manhua);
            ManfraCount.Value = UserCollection.Count(series => series.Format == Format.Manfra);
            ComicCount.Value = UserCollection.Count(series => series.Format == Format.Comic);
            NovelCount.Value = UserCollection.Count(series => series.Format == Format.Novel);
        }

        /// <summary>
        /// Updates the percentages for the demographic pie chart legend
        /// </summary>
        public void UpdateFormatChartPercentages()
        {
            MangaPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && MangaCount.Value != double.NaN ? MangaCount.Value / UserCollection.Count * 100 : 0), 2);
            ManhwaPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && ManhwaCount.Value != double.NaN ? ManhwaCount.Value / UserCollection.Count * 100 : 0), 2);
            ManhuaPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && ManhuaCount.Value != double.NaN ? ManhuaCount.Value / UserCollection.Count * 100 : 0), 2);
            ManfraPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && ManfraCount.Value != double.NaN ? ManfraCount.Value / UserCollection.Count * 100 : 0), 2);
            ComicPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && ComicCount.Value != double.NaN ? ComicCount.Value / UserCollection.Count * 100 : 0), 2);
            NovelPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && NovelCount.Value != double.NaN ? NovelCount.Value / UserCollection.Count * 100 : 0), 2);
        }

        /// <summary>
        /// Updates the values in the Rating bar/cartersian chart
        /// </summary>
        public void UpdateRatingChartValues()
        {
            RatingArray.Clear();
            ZeroRatingCount.Value = UserCollection.Count(series => series.Rating >= 0 && series.Rating < 1);
            OneRatingCount.Value = UserCollection.Count(series => series.Rating >= 1 && series.Rating < 2);
            TwoRatingCount.Value = UserCollection.Count(series => series.Rating >= 2 && series.Rating < 3);
            ThreeRatingCount.Value = UserCollection.Count(series => series.Rating >= 3 && series.Rating < 4);
            FourRatingCount.Value = UserCollection.Count(series => series.Rating >= 4 && series.Rating < 5);
            FiveRatingCount.Value = UserCollection.Count(series => series.Rating >= 5 && series.Rating < 6);
            SixRatingCount.Value = UserCollection.Count(series => series.Rating >= 6 && series.Rating < 7);
            SevenRatingCount.Value = UserCollection.Count(series => series.Rating >= 7 && series.Rating < 8);
            EightRatingCount.Value = UserCollection.Count(series => series.Rating >= 8 && series.Rating < 9);
            NineRatingCount.Value = UserCollection.Count(series => series.Rating >= 9 && series.Rating < 10);
            TenRatingCount.Value = UserCollection.Count(series => series.Rating == 10);

            RatingArray.Add(ZeroRatingCount.Value);
            RatingArray.Add(OneRatingCount.Value);
            RatingArray.Add(TwoRatingCount.Value);
            RatingArray.Add(ThreeRatingCount.Value);
            RatingArray.Add(FourRatingCount.Value);
            RatingArray.Add(FiveRatingCount.Value);
            RatingArray.Add(SixRatingCount.Value);
            RatingArray.Add(SevenRatingCount.Value);
            RatingArray.Add(EightRatingCount.Value);
            RatingArray.Add(NineRatingCount.Value);
            RatingArray.Add(TenRatingCount.Value);

            MaxRatingCount.Value = RatingArray.Max();
            // if (MaxRatingCount.Value % 2 != 0) { MaxRatingCount.Value = MaxRatingCount.Value + 1; }
            MaxRatingCount1.Value = MaxRatingCount.Value;
            MaxRatingCount2.Value = MaxRatingCount.Value;
            MaxRatingCount3.Value = MaxRatingCount.Value;
            MaxRatingCount4.Value = MaxRatingCount.Value;
            MaxRatingCount5.Value = MaxRatingCount.Value;
            MaxRatingCount6.Value = MaxRatingCount.Value;
            MaxRatingCount7.Value = MaxRatingCount.Value;
            MaxRatingCount8.Value = MaxRatingCount.Value;
            MaxRatingCount9.Value = MaxRatingCount.Value;
            MaxRatingCount10.Value = MaxRatingCount.Value;
        }

        /// <summary>
        /// Updates the values in the Volume Count bar/cartersian chart
        /// </summary>
        public void UpdateVolumeCountChartValues()
        {
            VolumeCountArray.Clear();
            ZeroVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 1 && series.MaxVolumeCount < 10);
            OneVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 10 && series.MaxVolumeCount < 20);
            TwoVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 20 && series.MaxVolumeCount < 30);
            ThreeVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 30 && series.MaxVolumeCount < 40);
            FourVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 40 && series.MaxVolumeCount < 50);
            FiveVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 50 && series.MaxVolumeCount < 60);
            SixVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 60 && series.MaxVolumeCount < 70);
            SevenVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 70 && series.MaxVolumeCount < 80);
            EightVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 80 && series.MaxVolumeCount < 90);
            NineVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 90 && series.MaxVolumeCount < 100);
            TenVolumeCount.Value = UserCollection.Count(series => series.MaxVolumeCount >= 100);

            VolumeCountArray.Add(ZeroVolumeCount.Value);
            VolumeCountArray.Add(OneVolumeCount.Value);
            VolumeCountArray.Add(TwoVolumeCount.Value);
            VolumeCountArray.Add(ThreeVolumeCount.Value);
            VolumeCountArray.Add(FourVolumeCount.Value);
            VolumeCountArray.Add(FiveVolumeCount.Value);
            VolumeCountArray.Add(SixVolumeCount.Value);
            VolumeCountArray.Add(SevenVolumeCount.Value);
            VolumeCountArray.Add(EightVolumeCount.Value);
            VolumeCountArray.Add(NineVolumeCount.Value);
            VolumeCountArray.Add(TenVolumeCount.Value);

            MaxVolumeCount.Value = VolumeCountArray.Max();
            MaxVolumeCount1.Value = MaxVolumeCount.Value;
            MaxVolumeCount2.Value = MaxVolumeCount.Value;
            MaxVolumeCount3.Value = MaxVolumeCount.Value;
            MaxVolumeCount4.Value = MaxVolumeCount.Value;
            MaxVolumeCount5.Value = MaxVolumeCount.Value;
            MaxVolumeCount6.Value = MaxVolumeCount.Value;
            MaxVolumeCount7.Value = MaxVolumeCount.Value;
            MaxVolumeCount8.Value = MaxVolumeCount.Value;
            MaxVolumeCount9.Value = MaxVolumeCount.Value;
            MaxVolumeCount10.Value = MaxVolumeCount.Value;
        }

        /// <summary>
        /// Updates the percentages for the status pie chart legend
        /// </summary>
        public void UpdateStatusPercentages()
        {
            FinishedPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && FinishedCount.Value != double.NaN ? FinishedCount.Value / UserCollection.Count * 100 : 0), 2);
            OngoingPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && OngoingCount.Value != double.NaN ? OngoingCount.Value / UserCollection.Count * 100 : 0), 2);
            CancelledPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && CancelledCount.Value != double.NaN ? CancelledCount.Value / UserCollection.Count * 100 : 0), 2);
            HiatusPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && HiatusCount.Value != double.NaN ? HiatusCount.Value / UserCollection.Count * 100 : 0), 2);
        }

        /// <summary>
        /// Updates the percentages for the demographic pie chart legend
        /// </summary>
        public void UpdateDemographicPercentages()
        {
            ShounenPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && ShounenCount.Value != double.NaN ? ShounenCount.Value / UserCollection.Count * 100 : 0), 2);
            SeinenPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && SeinenCount.Value != double.NaN ? SeinenCount.Value / UserCollection.Count * 100 : 0), 2);
            ShoujoPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && ShoujoCount.Value != double.NaN ? ShoujoCount.Value / UserCollection.Count * 100 : 0), 2);
            JoseiPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && JoseiCount.Value != double.NaN ? JoseiCount.Value / UserCollection.Count * 100 : 0), 2);
            UnknownPercentage = Math.Round(Convert.ToDecimal(UserCollection.Count != 0 && UnknownCount.Value != double.NaN ? UnknownCount.Value / UserCollection.Count * 100 : 0), 2);
        }

        public void UpdateCollectionPrice()
        {
            decimal valueVal = decimal.Zero;
            foreach (Series x in UserCollection)
            {
                valueVal = decimal.Add(valueVal, x.Value);
            }
            CollectionPrice = $"{CurrentUser.Currency}{decimal.Round(valueVal, 2)}";
        }

        public void UpdateCollectionVolumesRead()
        {
            uint volumesRead = 0;
            foreach (Series x in UserCollection)
            {
                volumesRead += x.VolumesRead;
            }
            VolumesRead = volumesRead;
        }

        public void UpdateCollectionRating()
        {
            // MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateRatingChartValues();
            decimal rating = decimal.Zero;
            uint countRating = 0;
            foreach (Series x in UserCollection.ToList())
            {
                if (x.Rating >= 0)
                {
                    rating = decimal.Add(rating, x.Rating);
                    countRating++;
                }
            }
            MeanRating = countRating != 0 ? decimal.Round(decimal.Divide(rating, countRating), 1) : 0;
        }

        public void UpdateVolumeCounts(Series series, uint newCurVols, uint newMaxVols)
        {
            LOGGER.Debug($"Old UsersNumVolumesCollected = {UsersNumVolumesCollected} | Old UsersNumVolumesToBeCollected = {UsersNumVolumesToBeCollected}");
            UsersNumVolumesCollected = UsersNumVolumesCollected - series.CurVolumeCount + newCurVols;
            UsersNumVolumesToBeCollected = UsersNumVolumesToBeCollected - (uint)(series.MaxVolumeCount - series.CurVolumeCount) + (newMaxVols - newCurVols);
            LOGGER.Debug($"New UsersNumVolumesCollected = {UsersNumVolumesCollected} | New UsersNumVolumesToBeCollected = {UsersNumVolumesToBeCollected}");
        }

        public void UpdateVolumeCounts(uint additionalCurVols, uint additionalVolToBeCol, bool isRemoval = false)
        {
            LOGGER.Debug($"Old UsersNumVolumesCollected = {UsersNumVolumesCollected} | Old UsersNumVolumesToBeCollected = {UsersNumVolumesToBeCollected}");
            if (isRemoval)
            {
                UsersNumVolumesCollected -= additionalCurVols;
                UsersNumVolumesToBeCollected -= additionalVolToBeCol;
            }
            else
            {
                UsersNumVolumesCollected += additionalCurVols;
                UsersNumVolumesToBeCollected += additionalVolToBeCol;
            }
            LOGGER.Debug($"New UsersNumVolumesCollected = {UsersNumVolumesCollected} | New UsersNumVolumesToBeCollected = {UsersNumVolumesToBeCollected}");
        }

        public void UpdateSeriesCount()
        {
            SeriesCount = (uint)UserCollection.Count;
        }

        public void UpdateAllStats(uint additionalCurVols, uint additionalVolToBeCol, bool isRemoval = false)
        {
            UpdateVolumeCounts(additionalCurVols, additionalVolToBeCol, isRemoval);
            UpdateSeriesCount();
            
            UpdateCollectionPrice();
            UpdateCollectionVolumesRead();
            UpdateCollectionRating();

            UpdateStatusChartValues();
            UpdateStatusPercentages();

            UpdateDemographicChartValues();
            UpdateDemographicPercentages();

            UpdateFormatChartValues();
            UpdateFormatChartPercentages();
            
            UpdateRatingChartValues();

            UpdateVolumeCountChartValues();

            UpdateGenreChart();
        }

        /// <summary>
        /// Generates all of the values for the users stats
        /// </summary>
        public void GenerateStats()
        {
            UpdateStatusPercentages();
            UpdateDemographicPercentages();
            UpdateFormatChartPercentages();
            
            UpdateRatingChartValues();
            UpdateVolumeCountChartValues();

            uint testVolumesRead = 0, testUsersNumVolumesCollected = 0, testUsersNumVolumesToBeCollected = 0;
            decimal testCollectionPrice = 0, testMeanRating = 0, countMeanRating = 0;
            foreach (Series x in UserCollection)
            {
                testVolumesRead += x.VolumesRead;
                testCollectionPrice += x.Value;
                testUsersNumVolumesCollected += x.CurVolumeCount;
                testUsersNumVolumesToBeCollected += (uint)(x.MaxVolumeCount - x.CurVolumeCount);

                if (x.Rating >= 0)
                {
                    testMeanRating += x.Rating;
                    countMeanRating++;
                }
            }
            
            testMeanRating = countMeanRating == 0 ? 0 : decimal.Round(testMeanRating / countMeanRating, 1);
            string testCollectionPriceString = $"{CurrentUser.Currency}{decimal.Round(testCollectionPrice, 2)}";

            // Crash protection for aggregate values
            // if (MainUser.VolumesRead == testVolumesRead && MainUser.MeanRating == testMeanRating && MainUser.CollectionPrice.Equals(testCollectionPriceString) && testUsersNumVolumesCollected == MainUser.NumVolumesCollected && testUsersNumVolumesToBeCollected == MainUser.NumVolumesToBeCollected)
            // {
            //     MeanRating = MainUser.MeanRating;
            //     VolumesRead = MainUser.VolumesRead;
            //     CollectionPrice = MainUser.CollectionPrice;
            //     UsersNumVolumesCollected = MainUser.NumVolumesCollected;
            //     UsersNumVolumesToBeCollected = MainUser.NumVolumesToBeCollected;
            // }
            // else
            // {
            //     MeanRating = testMeanRating;
            //     VolumesRead = testVolumesRead;
            //     CollectionPrice = testCollectionPriceString;
            //     UsersNumVolumesCollected = testUsersNumVolumesCollected;
            //     UsersNumVolumesToBeCollected = testUsersNumVolumesToBeCollected;
            // }

            GenerateCharts();
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