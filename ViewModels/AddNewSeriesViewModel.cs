using Tsundoku.Models;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels
{
    public class AddNewSeriesViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public string[] AdditionalLanguages { get; } = new string[] {"French", "Italian", "German", "Spanish"};
        
        public AddNewSeriesViewModel()
        {

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
                    Logger.Info("New Series JSON\n" + newSeries.ToString());
                    MainWindowViewModel.Collection.Add(newSeries);
                    MainWindowViewModel.SortCollection();
                }
                else
                {
                    Logger.Info("Duplicate Check, Series Already Exists");
                }
            }
        }
    }
}
