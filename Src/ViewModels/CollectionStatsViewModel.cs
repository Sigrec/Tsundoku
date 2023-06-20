using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Tsundoku.Models;

/*
    Country Distribution (Pie)
    Score Distribution (Bar)
    Status Distribution (Pie)
    Mean Score (Value)
    Volumes Read (Value)
    Series Count (Value)
    Total Price(Value)
*/

namespace Tsundoku.ViewModels
{
    public partial class CollectionStatsViewModel : ViewModelBase
    {
        public ObservableValue ShounenCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Shounen")));
        public ObservableValue SeinenCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Seinen")));
        public ObservableValue ShoujoCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Shoujo")));
        public ObservableValue JoseiCount { get; set; } = new ObservableValue(MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Josei")));
        public ObservableCollection<ISeries> Demographics { get; set; } = new ObservableCollection<ISeries>();

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

        [Reactive] public decimal MeanScore { get; set; }
        [Reactive] public uint VolumesRead { get; set; }
        [Reactive] public string CollectionPrice { get; set; }
        [Reactive] public uint SeriesCount { get; set; } = (uint)MainUser.UserCollection.Count;
        [Reactive] public decimal FinishedPercentage { get; set; }
        [Reactive] public decimal OngoingPercentage { get; set; }
        [Reactive] public decimal CancelledPercentage { get; set; }
        [Reactive] public decimal HiatusPercentage { get; set; }
        [Reactive] public decimal ShounenPercentage { get; set; }
        [Reactive] public decimal SeinenPercentage { get; set; }
        [Reactive] public decimal ShoujoPercentage { get; set; }
        [Reactive] public decimal JoseiPercentage { get; set; }

        public CollectionStatsViewModel()
        {
            GenerateStats();

            this.WhenAnyValue(x => x.MeanScore).Subscribe(x => MainUser.MeanScore = x);
            this.WhenAnyValue(x => x.VolumesRead).Subscribe(x => MainUser.VolumesRead = x);
            this.WhenAnyValue(x => x.CollectionPrice).Subscribe(x => MainUser.CollectionPrice = x);
        }

        public void UpdateDemographicChartValues()
        {
            ShounenCount.Value = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Shounen"));

            SeinenCount.Value = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Seinen"));

            ShoujoCount.Value = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Shoujo"));

            JoseiCount.Value = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic) && series.Demographic.Equals("Josei"));
        }

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

            Constants.Logger.Debug(MaxScoreCount.Value);
            // ScoreDistribution.Add(new ColumnSeries<ObservableValue> 
            // { 
            //     IsHoverable = false,
            //     Values = new ObservableCollection<ObservableValue> { MaxScoreCount, MaxScoreCount, MaxScoreCount, MaxScoreCount, MaxScoreCount, MaxScoreCount, MaxScoreCount, MaxScoreCount, MaxScoreCount, MaxScoreCount },
            //     IgnoresBarPosition = true
            // });
            ScoreDistribution.Add(new ColumnSeries<ObservableValue> 
            { 
                IsHoverable = false,
                Values = new ObservableCollection<ObservableValue>
                {
                    ZeroScoreCount, OneScoreCount, TwoScoreCount, ThreeScoreCount, FourScoreCount, FiveScoreCount, SixScoreCount, SevenScoreCount, EightScoreCount, NineScoreCount, TenScoreCount
                },
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
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
                Labels = new string[] {  },
                MinLimit = 0
            });
        }

        public void UpdateStatusChartValues()
        {
            OngoingCount.Value = MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Ongoing"));

            FinishedCount.Value = MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Finished"));

            CancelledCount.Value = MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Cancelled"));

            HiatusCount.Value = MainWindowViewModel.Collection.Count(series => !series.Status.Equals("Error") && series.Status.Equals("Hiatus"));
        }

        public void UpdateScoreChartValues()
        {
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
            // MaxScoreCount.Value = GetMaxScoreCount();
        }

        public void UpdateStatusPercentages()
        {
            FinishedPercentage = Math.Round(Convert.ToDecimal((MainWindowViewModel.Collection.Count != 0 && FinishedCount.Value != Double.NaN ? FinishedCount.Value / MainWindowViewModel.Collection.Count * 100 : 0)), 2);
            OngoingPercentage = Math.Round(Convert.ToDecimal((MainWindowViewModel.Collection.Count != 0 && OngoingCount.Value != Double.NaN ? OngoingCount.Value / MainWindowViewModel.Collection.Count * 100 : 0)), 2);
            CancelledPercentage = Math.Round(Convert.ToDecimal((MainWindowViewModel.Collection.Count != 0 && CancelledCount.Value != Double.NaN ? CancelledCount.Value / MainWindowViewModel.Collection.Count * 100 : 0)), 2);
            HiatusPercentage = Math.Round(Convert.ToDecimal((MainWindowViewModel.Collection.Count != 0 && HiatusCount.Value != Double.NaN ? HiatusCount.Value / MainWindowViewModel.Collection.Count * 100 : 0)), 2);
        }

        public void UpdateDemographicPercentages()
        {
            int actualCount = MainWindowViewModel.Collection.Count(series => !string.IsNullOrWhiteSpace(series.Demographic));
            ShounenPercentage = Math.Round(Convert.ToDecimal((actualCount != 0 && ShounenCount.Value != Double.NaN ? ShounenCount.Value / actualCount * 100 : 0)), 2);
            SeinenPercentage = Math.Round(Convert.ToDecimal((actualCount != 0 && SeinenCount.Value != Double.NaN ? SeinenCount.Value / actualCount * 100 : 0)), 2);
            ShoujoPercentage = Math.Round(Convert.ToDecimal((actualCount != 0 && ShoujoCount.Value != Double.NaN ? ShoujoCount.Value / actualCount * 100 : 0)), 2);
            JoseiPercentage = Math.Round(Convert.ToDecimal((actualCount != 0 && JoseiCount.Value != Double.NaN ? JoseiCount.Value / actualCount * 100 : 0)), 2);
        }

        // private uint GetMaxScoreCount()
        // {
        //     return (uint)Math.Max((uint)OneScoreCount.Value, Math.Max((uint)TwoScoreCount.Value, Math.Max((uint)ThreeScoreCount.Value, Math.Max((uint)FourScoreCount.Value, Math.Max((uint)FiveScoreCount.Value, Math.Max((uint)SixScoreCount.Value, Math.Max((uint)SevenScoreCount.Value, Math.Max((uint)EightScoreCount.Value, Math.Max((uint)NineScoreCount.Value, (uint)TenScoreCount.Value)))))))));
        // }

        public void GenerateStats()
        {
            UpdateStatusPercentages();
            UpdateDemographicPercentages();
            UpdateScoreChartValues();

            // Constants.Logger.Debug("Generate Stats");
            uint testVolumesRead = 0;
            decimal testCollectionPrice = 0, testMeanScore = 0, countMeanScore = 0;
            string testCollectionPriceString = "";
            foreach (Models.Series x in MainWindowViewModel.Collection)
            {
                testVolumesRead += x.VolumesRead;
                testCollectionPrice += x.Cost;

                if (x.Score >= 0)
                {
                    testMeanScore += x.Score;
                    countMeanScore++;
                }
            }

            testMeanScore = countMeanScore == 0 ? 0 : Decimal.Round(testMeanScore / countMeanScore, 1);
            testCollectionPriceString = $"{MainUser.Currency}{Decimal.Round(testCollectionPrice, 2)}";

            // Crash protection for aggregate values
            if (MainUser.VolumesRead == testVolumesRead && MainUser.MeanScore == testMeanScore && MainUser.CollectionPrice.Equals(testCollectionPriceString))
            {
                MeanScore = MainUser.MeanScore;
                VolumesRead = MainUser.VolumesRead;
                CollectionPrice = MainUser.CollectionPrice;
            }
            else
            {
                MeanScore = testMeanScore;
                VolumesRead = testVolumesRead;
                CollectionPrice = testCollectionPriceString;
            }
        }
    }
}
