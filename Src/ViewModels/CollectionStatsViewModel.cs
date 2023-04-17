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
        public ObservableCollection<ISeries> CountryDistribution { get; set; }

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
            //         Values = new double[] { 1 }, 
            //         Name = "Japan",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CurrentTheme.MenuButtonBGColor))
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new double[] { 1 }, 
            //         Name = "Korea",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CurrentTheme.MenuButtonBorderColor))
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new double[] { 1 }, 
            //         Name = "America",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CurrentTheme.MenuButtonTextAndIconColor))
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new double[] { 1 }, 
            //         Name = "China",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CurrentTheme.CollectionBGColor))
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new double[] { 1 }, 
            //         Name = "France",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CurrentTheme.MenuTextColor))
            //     }
            // };

            GenerateStats();
            //SeriesCount = (uint)MainUser.UserCollection.Count;

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
