using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Tsundoku.Models;
using System.Collections.Generic;
using Avalonia.Logging;

namespace Tsundoku.ViewModels
{
    public partial class CollectionStatsViewModel : ViewModelBase
    {
        public ObservableCollection<ISeries> Demographics { get; set; } = new ObservableCollection<ISeries>();
        public ObservableValue ShounenCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Shounen")));
        public ObservableValue SeinenCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Seinen")));
        public ObservableValue ShoujoCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Shoujo")));
        public ObservableValue JoseiCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Josei")));

        public ObservableCollection<ISeries> StatusDistribution { get; set; } = new ObservableCollection<ISeries>();
        public ObservableValue OngoingCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Ongoing")));
        public ObservableValue FinishedCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Finished")));
        public ObservableValue CancelledCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Cancelled")));
        public ObservableValue HiatusCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Hiatus")));

        public ObservableCollection<ISeries> ScoreDistribution { get; set; } = new ObservableCollection<ISeries>();
        public ObservableCollection<Axis> ScoreXAxes { get; set; } = new ObservableCollection<Axis>();
        public ObservableCollection<Axis> ScoreYAxes { get; set; } = new ObservableCollection<Axis>();
        public ObservableValue ZeroScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score > 0 && series.Score < 1));
        public ObservableValue OneScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score >= 1 && series.Score < 2));
        public ObservableValue TwoScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score >= 2 && series.Score < 3));
        public ObservableValue ThreeScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score >= 3 && series.Score < 4));
        public ObservableValue FourScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score >= 4 && series.Score < 5));
        public ObservableValue FiveScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score >= 5 && series.Score < 6));
        public ObservableValue SixScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score >= 7 && series.Score < 8));
        public ObservableValue SevenScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score >= 7 && series.Score < 8));
        public ObservableValue EightScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score >= 8 && series.Score < 9));
        public ObservableValue NineScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score >= 9 && series.Score < 10));
        public ObservableValue TenScoreCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => series.Score == 10));
        public ObservableValue MaxScoreCount { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount1 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount2 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount3 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount4 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount5 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount6 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount7 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount8 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount9 { get; set; } = new ObservableValue(0);
        public ObservableValue MaxScoreCount10 { get; set; } = new ObservableValue(0);

        [Reactive] public decimal MeanScore { get; set; }
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
        [Reactive] public uint UsersNumVolumesCollected { get; set; }
        [Reactive] public uint UsersNumVolumesToBeCollected { get; set; }
        private List<double?> ScoreArray = new List<double?>(10);

        public CollectionStatsViewModel()
        {
            GenerateStats();

            this.WhenAnyValue(x => x.MeanScore).Subscribe(x => MainUser.MeanScore = x);
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
            ShounenCount.Value = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Shounen"));

            SeinenCount.Value = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Seinen"));

            ShoujoCount.Value = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Shoujo"));

            JoseiCount.Value = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Josei"));
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

            ScoreDistribution.Add(new ColumnSeries<ObservableValue> 
            { 
                IsHoverable = false,
                Values = new ObservableCollection<ObservableValue> 
                { 
                    MaxScoreCount, MaxScoreCount1, MaxScoreCount2, MaxScoreCount3, MaxScoreCount4, MaxScoreCount5, MaxScoreCount6, MaxScoreCount7, MaxScoreCount8, MaxScoreCount9, MaxScoreCount10
                },
                Stroke = null,
                IgnoresBarPosition = true
            });
            ScoreDistribution.Add(new ColumnSeries<ObservableValue> 
            { 
                IsHoverable = false,
                Values = new ObservableCollection<ObservableValue>
                {
                    ZeroScoreCount, OneScoreCount, TwoScoreCount, ThreeScoreCount, FourScoreCount, FiveScoreCount, SixScoreCount, SevenScoreCount, EightScoreCount, NineScoreCount, TenScoreCount
                },
                DataLabelsSize = 15,
                IgnoresBarPosition = true
            });

            ScoreXAxes.Add(new Axis
            {
                LabelsRotation = 0,
                TextSize = 14,
                MinStep = 1,
                ForceStepToMin = true
            });

            ScoreYAxes.Add(new Axis
            {
                Labels = Array.Empty<string>(),
                MinLimit = 0
            });
        }

        /// <summary>
        /// Updates the values in the status pie chart
        /// </summary>
        public void UpdateStatusChartValues()
        {
            OngoingCount.Value = MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Ongoing"));

            FinishedCount.Value = MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Finished"));

            CancelledCount.Value = MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Cancelled"));

            HiatusCount.Value = MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Hiatus"));
        }

        /// <summary>
        /// Updates the values in the score bar/cartersian chart
        /// </summary>
        public void UpdateScoreChartValues()
        {
            ScoreArray.Clear();
            ZeroScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 0 && series.Score < 1);
            OneScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 1 && series.Score < 2);
            TwoScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 2 && series.Score < 3);
            ThreeScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 3 && series.Score < 4);
            FourScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 4 && series.Score < 5);
            FiveScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 5 && series.Score < 6);
            SixScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 6 && series.Score < 7);
            SevenScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 7 && series.Score < 8);
            EightScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 8 && series.Score < 9);
            NineScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score >= 9 && series.Score < 10);
            TenScoreCount.Value = MainWindowViewModel.Collection.Count(series => series.Score == 10);

            ScoreArray.Add(ZeroScoreCount.Value);
            ScoreArray.Add(OneScoreCount.Value);
            ScoreArray.Add(TwoScoreCount.Value);
            ScoreArray.Add(ThreeScoreCount.Value);
            ScoreArray.Add(FourScoreCount.Value);
            ScoreArray.Add(FiveScoreCount.Value);
            ScoreArray.Add(SixScoreCount.Value);
            ScoreArray.Add(SevenScoreCount.Value);
            ScoreArray.Add(EightScoreCount.Value);
            ScoreArray.Add(NineScoreCount.Value);
            ScoreArray.Add(TenScoreCount.Value);

            MaxScoreCount.Value = ScoreArray.Max();
            MaxScoreCount1.Value = MaxScoreCount.Value;
            MaxScoreCount2.Value = MaxScoreCount.Value;
            MaxScoreCount3.Value = MaxScoreCount.Value;
            MaxScoreCount4.Value = MaxScoreCount.Value;
            MaxScoreCount5.Value = MaxScoreCount.Value;
            MaxScoreCount6.Value = MaxScoreCount.Value;
            MaxScoreCount7.Value = MaxScoreCount.Value;
            MaxScoreCount8.Value = MaxScoreCount.Value;
            MaxScoreCount9.Value = MaxScoreCount.Value;
            MaxScoreCount10.Value = MaxScoreCount.Value;
        }

        /// <summary>
        /// Updates the percentages for the status pie chart legend
        /// </summary>
        public void UpdateStatusPercentages()
        {
            FinishedPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.Collection.Count != 0 && FinishedCount.Value != double.NaN ? FinishedCount.Value / MainWindowViewModel.Collection.Count * 100 : 0), 2);
            OngoingPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.Collection.Count != 0 && OngoingCount.Value != double.NaN ? OngoingCount.Value / MainWindowViewModel.Collection.Count * 100 : 0), 2);
            CancelledPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.Collection.Count != 0 && CancelledCount.Value != double.NaN ? CancelledCount.Value / MainWindowViewModel.Collection.Count * 100 : 0), 2);
            HiatusPercentage = Math.Round(Convert.ToDecimal(MainWindowViewModel.Collection.Count != 0 && HiatusCount.Value != double.NaN ? HiatusCount.Value / MainWindowViewModel.Collection.Count * 100 : 0), 2);
        }

        /// <summary>
        /// Updates the percentages for the demographic pie chart legend
        /// </summary>
        public void UpdateDemographicPercentages()
        {
            int actualCount = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic));
            ShounenPercentage = Math.Round(Convert.ToDecimal(actualCount != 0 && ShounenCount.Value != double.NaN ? ShounenCount.Value / actualCount * 100 : 0), 2);
            SeinenPercentage = Math.Round(Convert.ToDecimal(actualCount != 0 && SeinenCount.Value != double.NaN ? SeinenCount.Value / actualCount * 100 : 0), 2);
            ShoujoPercentage = Math.Round(Convert.ToDecimal(actualCount != 0 && ShoujoCount.Value != double.NaN ? ShoujoCount.Value / actualCount * 100 : 0), 2);
            JoseiPercentage = Math.Round(Convert.ToDecimal(actualCount != 0 && JoseiCount.Value != double.NaN ? JoseiCount.Value / actualCount * 100 : 0), 2);
        }

        /// <summary>
        /// Generates all of the values for the users stats
        /// </summary>
        public void GenerateStats()
        {
            UpdateStatusPercentages();
            UpdateDemographicPercentages();
            UpdateScoreChartValues();

            // LOGGER.Debug("Generate Stats");
            uint testVolumesRead = 0, testUsersNumVolumesCollected = 0, testUsersNumVolumesToBeCollected = 0;
            decimal testCollectionPrice = 0, testMeanScore = 0, countMeanScore = 0;
            foreach (Series x in MainWindowViewModel.Collection)
            {
                testVolumesRead += x.VolumesRead;
                testCollectionPrice += x.Cost;
                testUsersNumVolumesCollected += x.CurVolumeCount;
                testUsersNumVolumesToBeCollected += (uint)(x.MaxVolumeCount - x.CurVolumeCount);

                if (x.Score >= 0)
                {
                    testMeanScore += x.Score;
                    countMeanScore++;
                }
            }
            
            testMeanScore = countMeanScore == 0 ? 0 : decimal.Round(testMeanScore / countMeanScore, 1);
            string testCollectionPriceString = $"{MainUser.Currency}{decimal.Round(testCollectionPrice, 2)}";

            // Crash protection for aggregate values
            if (MainUser.VolumesRead == testVolumesRead && MainUser.MeanScore == testMeanScore && MainUser.CollectionPrice.Equals(testCollectionPriceString) && testUsersNumVolumesCollected == MainUser.NumVolumesCollected && testUsersNumVolumesToBeCollected == MainUser.NumVolumesToBeCollected)
            {
                MeanScore = MainUser.MeanScore;
                VolumesRead = MainUser.VolumesRead;
                CollectionPrice = MainUser.CollectionPrice;
                UsersNumVolumesCollected = MainUser.NumVolumesCollected;
                UsersNumVolumesToBeCollected = MainUser.NumVolumesToBeCollected;
            }
            else
            {
                MeanScore = testMeanScore;
                VolumesRead = testVolumesRead;
                CollectionPrice = testCollectionPriceString;
                UsersNumVolumesCollected = testUsersNumVolumesCollected;
                UsersNumVolumesToBeCollected = testUsersNumVolumesToBeCollected;
            }

            GenerateCharts();
        }
    }
}
