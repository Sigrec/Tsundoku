using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;

/*
    Country Distribution (Pie)
    Score Distribution (Pie)
    Status Distribution (Pie)
    Mean Score (Value)
    Volumes Read (Value)
    Series Count (Value)
    Total Price(Value)
    Price Distribution (Bar)
*/

namespace Tsundoku.ViewModels
{
    public partial class CollectionStatsViewModel : ViewModelBase
    {
        // private ObservableCollection<int> countryCounts = new ObservableCollection<int>();
        // public ObservableCollection<ISeries> CountryDistribution { get; set; }

        // public ISeries[] Series { get; set; }
        //     = new ISeries[]
        //     {
        //         new PieSeries<double> { Values = new double[] { 2 } },
        //         new PieSeries<double> { Values = new double[] { 4 } },
        //         new PieSeries<double> { Values = new double[] { 1 } },
        //         new PieSeries<double> { Values = new double[] { 4 } },
        //         new PieSeries<double> { Values = new double[] { 3 } }
        //     };

        [Reactive]
        public decimal MeanScore { get; set; }

        [Reactive]
        public uint VolumesRead { get; set; }

        [Reactive]
        public string CollectionPrice { get; set; }

        [Reactive]
        public uint SeriesCount { get; set; } = (uint)MainUser.UserCollection.Count;

        public CollectionStatsViewModel()
        {
            // CountryDistribution = new ObservableCollection<ISeries>
            // {
            //     new PieSeries<double> 
            //     { 
            //         Values = new ObservableCollection<double> { 2 }, 
            //         Name = "Japan"
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new ObservableCollection<double> { 4 }, 
            //         Name = "Korea"
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new ObservableCollection<double> { 1 }, 
            //         Name = "America"
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new ObservableCollection<double> { 4 }, 
            //         Name = "China"
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new ObservableCollection<double> { 3 }, 
            //         Name = "France"
            //     }
            // };
            GenerateStats();

            this.WhenAnyValue(x => x.MeanScore).Subscribe(x => MainUser.MeanScore = x);
            this.WhenAnyValue(x => x.VolumesRead).Subscribe(x => MainUser.VolumesRead = x);
            this.WhenAnyValue(x => x.CollectionPrice).Subscribe(x => MainUser.CollectionPrice = x);
        }

        public void GenerateStats()
        {
            uint testVolumesRead = 0;
            decimal testCollectionPrice = 0, testMeanScore = 0, countMeanScore = 0;
            string testCollectionPriceString = "";
            foreach (Models.Series x in MainWindowViewModel.Collection)
            {
                testMeanScore += x.Score;
                testVolumesRead += x.VolumesRead;
                testCollectionPrice += x.Cost;

                if (x.Score != 0)
                {
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
