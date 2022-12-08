using System.Security.Cryptography.X509Certificates;
using System;
using Tsundoku.Models;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using Tsundoku.Views;

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
        public bool IsAddSeriesButtonEnabled { get; set; }

        public AddNewSeriesViewModel()
        {
            this.WhenAnyValue(x => x.TitleText, x => x.MaxVolumeCount, x => x.CurVolumeCount, (title, max, cur) => !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(max) && !string.IsNullOrWhiteSpace(cur) && Convert.ToUInt16(cur) <= Convert.ToUInt16(max)).BindTo(this, x => x.IsAddSeriesButtonEnabled);
        }
        
        public static void GetSeriesData(string title, string bookType, ushort curVolCount, ushort maxVolCount)
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
                    MainWindowViewModel.SortCollection();
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
