﻿using System.Collections.ObjectModel;
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

        public ObservableCollection<ISeries> RatingDistribution { get; set; } = new ObservableCollection<ISeries>();
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
        [Reactive] public uint UsersNumVolumesCollected { get; set; }
        [Reactive] public uint UsersNumVolumesToBeCollected { get; set; }
        private List<double?> RatingArray = new List<double?>(10);

        public CollectionStatsViewModel()
        {
            this.CurrentTheme = MainUser.SavedThemes.First(theme => theme.ThemeName.Equals(MainUser.MainTheme));
            this.CurCurrency = MainUser.Currency;
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
            decimal costVal = decimal.Zero;
            foreach (Series x in MainWindowViewModel.UserCollection)
            {
                costVal = decimal.Add(costVal, x.Cost);
            }
            CollectionPrice = $"{CurCurrency}{decimal.Round(costVal, 2)}";
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
            // MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateRatingChartValues();
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
            MeanRating = decimal.Round(decimal.Divide(rating, countRating), 1);
        }

        public void UpdateAllStats(uint additionalCurVols, uint additionalVolToBeCol)
        {
            UsersNumVolumesCollected += additionalCurVols;
            UsersNumVolumesToBeCollected += additionalVolToBeCol;
            SeriesCount = (uint)MainUser.UserCollection.Count;
            
            UpdateCollectionPrice();
            UpdateCollectionVolumesRead();
            UpdateCollectionRating();

            UpdateStatusChartValues();
            UpdateStatusPercentages();

            UpdateDemographicChartValues();
            UpdateDemographicPercentages();
            
            UpdateRatingChartValues();
        }

        /// <summary>
        /// Generates all of the values for the users stats
        /// </summary>
        public void GenerateStats()
        {
            UpdateStatusPercentages();
            UpdateDemographicPercentages();
            UpdateRatingChartValues();

            uint testVolumesRead = 0, testUsersNumVolumesCollected = 0, testUsersNumVolumesToBeCollected = 0;
            decimal testCollectionPrice = 0, testMeanRating = 0, countMeanRating = 0;
            foreach (Series x in MainWindowViewModel.UserCollection)
            {
                testVolumesRead += x.VolumesRead;
                testCollectionPrice += x.Cost;
                testUsersNumVolumesCollected += x.CurVolumeCount;
                testUsersNumVolumesToBeCollected += (uint)(x.MaxVolumeCount - x.CurVolumeCount);

                if (x.Rating >= 0)
                {
                    testMeanRating += x.Rating;
                    countMeanRating++;
                }
            }
            
            testMeanRating = countMeanRating == 0 ? 0 : decimal.Round(testMeanRating / countMeanRating, 1);
            string testCollectionPriceString = $"{MainUser.Currency}{decimal.Round(testCollectionPrice, 2)}";

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
