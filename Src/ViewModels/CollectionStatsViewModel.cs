using System.Collections.ObjectModel;
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

namespace Tsundoku.ViewModels
{
    public partial class CollectionStatsViewModel : ViewModelBase
    {
        public ObservableCollection<ISeries> Demographics { get; set; } = new ObservableCollection<ISeries>();
        public ObservableValue ShounenCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Shounen));
        public ObservableValue SeinenCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Seinen));
        public ObservableValue ShoujoCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Shoujo));
        public ObservableValue JoseiCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Josei));
        public ObservableValue UnknownCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Unknown));

        public ObservableCollection<ISeries> StatusDistribution { get; set; } = new ObservableCollection<ISeries>();
        public ObservableValue OngoingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Status == Status.Ongoing));
        public ObservableValue FinishedCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Status == Status.Finished));
        public ObservableValue CancelledCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Status == Status.Cancelled));
        public ObservableValue HiatusCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Status == Status.Hiatus));

        public ObservableCollection<ISeries> Formats { get; set; } = new ObservableCollection<ISeries>();
        public ObservableValue MangaCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Manga));
        public ObservableValue ManhwaCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Manhwa));
        public ObservableValue ManhuaCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Manhua));
        public ObservableValue ManfraCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Manfra));
        public ObservableValue ComicCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Comic));
        public ObservableValue NovelCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Novel));

        public ObservableCollection<ISeries> RatingDistribution { get; set; } = new ObservableCollection<ISeries>();
        private List<double?> RatingArray = new List<double?>(10);
        public ObservableCollection<Axis> RatingXAxes { get; set; } = new ObservableCollection<Axis>();
        public ObservableCollection<Axis> RatingYAxes { get; set; } = new ObservableCollection<Axis>();
        public ObservableValue ZeroRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating > 0 && series.Rating < 1));
        public ObservableValue OneRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating >= 1 && series.Rating < 2));
        public ObservableValue TwoRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating >= 2 && series.Rating < 3));
        public ObservableValue ThreeRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating >= 3 && series.Rating < 4));
        public ObservableValue FourRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating >= 4 && series.Rating < 5));
        public ObservableValue FiveRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating >= 5 && series.Rating < 6));
        public ObservableValue SixRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating >= 7 && series.Rating < 8));
        public ObservableValue SevenRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating >= 7 && series.Rating < 8));
        public ObservableValue EightRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating >= 8 && series.Rating < 9));
        public ObservableValue NineRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating >= 9 && series.Rating < 10));
        public ObservableValue TenRatingCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.Rating == 10));
        public ObservableValue MaxRatingCount { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount1 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount2 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount3 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount4 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount5 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount6 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount7 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount8 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount9 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxRatingCount10 { get; set; } = new ObservableValue(0);

        public ObservableCollection<ISeries> VolumeCountDistribution { get; set; } = new ObservableCollection<ISeries>();
        private List<double?> VolumeCountArray = new List<double?>(10);
        public ObservableCollection<Axis> VolumeCountXAxes { get; set; } = new ObservableCollection<Axis>();
        public ObservableCollection<Axis> VolumeCountYAxes { get; set; } = new ObservableCollection<Axis>();
        public ObservableValue ZeroVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 1 && series.MaxVolumeCount < 10));
        public ObservableValue OneVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 10 && series.MaxVolumeCount < 20));
        public ObservableValue TwoVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 20 && series.MaxVolumeCount < 30));
        public ObservableValue ThreeVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 30 && series.MaxVolumeCount < 40));
        public ObservableValue FourVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 40 && series.MaxVolumeCount < 50));
        public ObservableValue FiveVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 50 && series.MaxVolumeCount < 60));
        public ObservableValue SixVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 60 && series.MaxVolumeCount < 70));
        public ObservableValue SevenVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 70 && series.MaxVolumeCount < 80));
        public ObservableValue EightVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 80 && series.MaxVolumeCount < 90));
        public ObservableValue NineVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 90 && series.MaxVolumeCount < 100));
        public ObservableValue TenVolumeCount { get; set; } = new ObservableValue(MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 100));
        public ObservableValue MaxVolumeCount { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount1 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount2 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount3 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount4 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount5 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount6 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount7 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount8 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount9 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxVolumeCount10 { get; set; } = new ObservableValue(0);

        public ObservableCollection<ISeries<KeyValuePair<string, int>>> GenreDistribution { get; set; } = [];
        private Dictionary<string, int> GenreData { get; set; } = [];
        public Axis[] GenreXAxes { get; set; }
        public Axis[] GenreYAxes  { get; set; }

        [Reactive] public decimal MeanRating { get; set; }
        [Reactive] public uint VolumesRead { get; set; }
        [Reactive] public string CollectionPrice { get; set; }
        [Reactive] public uint SeriesCount { get; set; } = (uint)MainUser.UserCollection.Count;
        [Reactive] public uint FavoriteCount { get; set; } = (uint)MainUser.UserCollection.Count(series => series.IsFavorite);
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

        public CollectionStatsViewModel()
        {
            GenerateStats();

            this.WhenAnyValue(x => x.MeanRating).Subscribe(x => MainUser.MeanRating = x);
            this.WhenAnyValue(x => x.VolumesRead).Subscribe(x => MainUser.VolumesRead = x);
            this.WhenAnyValue(x => x.CollectionPrice).Subscribe(x => MainUser.CollectionPrice = x);
            this.WhenAnyValue(x => x.UsersNumVolumesCollected).Subscribe(x => MainUser.NumVolumesCollected = x);
            this.WhenAnyValue(x => x.UsersNumVolumesToBeCollected).Subscribe(x => MainUser.NumVolumesToBeCollected = x);
        }

        /// <summary>
        /// Updates the values in the demographic pie chart
        /// </summary>
        public void UpdateDemographicChartValues()
        {
            ShounenCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Shounen);
            SeinenCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Seinen);
            ShoujoCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Shoujo);
            JoseiCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Josei);
            UnknownCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Demographic == Demographic.Unknown);
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
            foreach(Series series in MainWindowViewModel.UserCollection)
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
            foreach(Series series in MainWindowViewModel.UserCollection)
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
            OngoingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Status == Status.Ongoing);
            FinishedCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Status == Status.Finished);
            CancelledCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Status == Status.Cancelled);
            HiatusCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Status == Status.Hiatus);
        }

        /// <summary>
        /// Updates the values in the format pie chart
        /// </summary>
        public void UpdateFormatChartValues()
        {
            MangaCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Manga);
            ManhwaCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Manhwa);
            ManhuaCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Manhua);
            ManfraCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Manfra);
            ComicCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Comic);
            NovelCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Format == Format.Novel);
        }

        /// <summary>
        /// Updates the percentages for the demographic pie chart legend
        /// </summary>
        public void UpdateFormatChartPercentages()
        {
            MangaPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && MangaCount.Value != double.NaN ? MangaCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            ManhwaPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && ManhwaCount.Value != double.NaN ? ManhwaCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            ManhuaPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && ManhuaCount.Value != double.NaN ? ManhuaCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            ManfraPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && ManfraCount.Value != double.NaN ? ManfraCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            ComicPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && ComicCount.Value != double.NaN ? ComicCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            NovelPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && NovelCount.Value != double.NaN ? NovelCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
        }

        /// <summary>
        /// Updates the values in the Rating bar/cartersian chart
        /// </summary>
        public void UpdateRatingChartValues()
        {
            RatingArray.Clear();
            ZeroRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 0 && series.Rating < 1);
            OneRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 1 && series.Rating < 2);
            TwoRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 2 && series.Rating < 3);
            ThreeRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 3 && series.Rating < 4);
            FourRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 4 && series.Rating < 5);
            FiveRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 5 && series.Rating < 6);
            SixRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 6 && series.Rating < 7);
            SevenRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 7 && series.Rating < 8);
            EightRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 8 && series.Rating < 9);
            NineRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating >= 9 && series.Rating < 10);
            TenRatingCount.Value = MainWindowViewModel.UserCollection.Count(series => series.Rating == 10);

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
            ZeroVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 1 && series.MaxVolumeCount < 10);
            OneVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 10 && series.MaxVolumeCount < 20);
            TwoVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 20 && series.MaxVolumeCount < 30);
            ThreeVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 30 && series.MaxVolumeCount < 40);
            FourVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 40 && series.MaxVolumeCount < 50);
            FiveVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 50 && series.MaxVolumeCount < 60);
            SixVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 60 && series.MaxVolumeCount < 70);
            SevenVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 70 && series.MaxVolumeCount < 80);
            EightVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 80 && series.MaxVolumeCount < 90);
            NineVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 90 && series.MaxVolumeCount < 100);
            TenVolumeCount.Value = MainWindowViewModel.UserCollection.Count(series => series.MaxVolumeCount >= 100);

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
            FinishedPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && FinishedCount.Value != double.NaN ? FinishedCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            OngoingPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && OngoingCount.Value != double.NaN ? OngoingCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            CancelledPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && CancelledCount.Value != double.NaN ? CancelledCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            HiatusPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && HiatusCount.Value != double.NaN ? HiatusCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
        }

        /// <summary>
        /// Updates the percentages for the demographic pie chart legend
        /// </summary>
        public void UpdateDemographicPercentages()
        {
            ShounenPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && ShounenCount.Value != double.NaN ? ShounenCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            SeinenPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && SeinenCount.Value != double.NaN ? SeinenCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            ShoujoPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && ShoujoCount.Value != double.NaN ? ShoujoCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            JoseiPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && JoseiCount.Value != double.NaN ? JoseiCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
            UnknownPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.UserCollection.Count != 0 && UnknownCount.Value != double.NaN ? UnknownCount.Value / MainWindowViewModel.UserCollection.Count * 100 : 0), 2);
        }

        public void UpdateCollectionPrice()
        {
            decimal valueVal = decimal.Zero;
            foreach (Series x in MainWindowViewModel.UserCollection)
            {
                valueVal = decimal.Add(valueVal, x.Value);
            }
            CollectionPrice = $"{CurCurrencyInstance}{decimal.Round(valueVal, 2)}";
        }

        public void UpdateCollectionVolumesRead()
        {
            uint volumesRead = 0;
            foreach (Series x in MainWindowViewModel.UserCollection)
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
            foreach (Series x in MainWindowViewModel.UserCollection.ToList())
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
            SeriesCount = (uint)MainUser.UserCollection.Count;
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
            foreach (Series x in MainWindowViewModel.UserCollection)
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
            string testCollectionPriceString = $"{CurCurrencyInstance}{decimal.Round(testCollectionPrice, 2)}";

            // Crash protection for aggregate values
            if (MainUser.VolumesRead == testVolumesRead && MainUser.MeanRating == testMeanRating && MainUser.CollectionPrice.Equals(testCollectionPriceString) && testUsersNumVolumesCollected == MainUser.NumVolumesCollected && testUsersNumVolumesToBeCollected == MainUser.NumVolumesToBeCollected)
            {
                MeanRating = MainUser.MeanRating;
                VolumesRead = MainUser.VolumesRead;
                CollectionPrice = MainUser.CollectionPrice;
                UsersNumVolumesCollected = MainUser.NumVolumesCollected;
                UsersNumVolumesToBeCollected = MainUser.NumVolumesToBeCollected;
            }
            else
            {
                MeanRating = testMeanRating;
                VolumesRead = testVolumesRead;
                CollectionPrice = testCollectionPriceString;
                UsersNumVolumesCollected = testUsersNumVolumesCollected;
                UsersNumVolumesToBeCollected = testUsersNumVolumesToBeCollected;
            }

            GenerateCharts();
        }
    }
}
