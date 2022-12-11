using System;
using Tsundoku.Models;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using DynamicData;

namespace Tsundoku.ViewModels
{
    public class AddNewSeriesViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public string[] AdditionalLanguages { get; } = new string[] {"French", "Italian", "German", "Spanish"};

        [Reactive]
        public string TitleText { get; set; }

        [Reactive]
        public string MaxVolumeCount { get; set; }

        [Reactive]
        public string CurVolumeCount { get; set; }

        [Reactive]
        public bool IsAddSeriesButtonEnabled { get; set; } = true;

        public AddNewSeriesViewModel()
        {
            // Disable the button to add a series to the users collection if the fields aren't populated and validated correctly
            // this.WhenAnyValue(x => x.TitleText, x => x.MaxVolumeCount, x => x.CurVolumeCount, (title, max, cur) => !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(max) && !string.IsNullOrWhiteSpace(cur) && Convert.ToUInt16(cur) <= Convert.ToUInt16(max)).Subscribe(x => IsAddSeriesButtonEnabled = x);
        }
        
        /**
        * Modified On: 08 December 2022
        *  by: Sean Njenga
        * Desc: Recieves the call from the AddNewSeriesWindow Button "Add" press which sends the data to this method which calls for
        *       the method to get the series data and create a new series object. Then it checks to see if it's a valid series and
        *       whether the series already exists in the users collection if not then it adds.
        * Params:
        *      title | string | The titles of the new series to add
        *      bookType | string | The book type or format of the series to add either a Manga(Comic) or Novel
        *      ushort | curVolCount | The current # of volumes the user has collected for this series
        *      ushort | maxVolCount | The max # of volumes this series currently has
        */
        public void GetSeriesData(string title, string bookType, ushort curVolCount, ushort maxVolCount)
        {
            Logger.Info($"Adding New Series -> {title} | {bookType} | {curVolCount} | {maxVolCount}");
            Series newSeries = Series.CreateNewSeriesCard(title, bookType, maxVolCount, curVolCount);
            if (newSeries!= null)
            {
                bool duplicateSeriesCheck = false;
                Parallel.ForEach(MainWindowViewModel.Collection, (series, state) =>
                {
                    if (Enumerable.SequenceEqual(newSeries.Titles, series.Titles) && newSeries.Format.Equals(series.Format))
                    {
                        duplicateSeriesCheck = true;
                        state.Break();
                    }
                });

                if (!duplicateSeriesCheck)
                {
                    Logger.Info(newSeries.ToString());
                    MainWindowViewModel.Collection.Add(newSeries);
                    int index = MainWindowViewModel.SearchForSort(newSeries);

                    MainWindowViewModel.SearchedCollection.Insert(index < 0 ? ~index : index, newSeries);
                    MainWindowViewModel.Collection = new System.Collections.ObjectModel.ObservableCollection<Series>(MainWindowViewModel.SearchedCollection);
                }
                else
                {
                    Logger.Info("Series Already Exists");
                }
            }
            else
            {
                Logger.Info($"{title} -> Does Not Exist");
            }
        }
    }
}
