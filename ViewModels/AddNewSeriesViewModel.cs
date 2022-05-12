using Tsundoku.Models;
using System.Diagnostics;

namespace Tsundoku.ViewModels
{
    public class AddNewSeriesViewModel : ViewModelBase
    {
        public AddNewSeriesViewModel()
        {
            
        }

        public static void GetSeriesData(string title, string bookType, ushort curVolCount, ushort maxVolCount)
        {
            Series _series = Series.CreateNewSeriesCard(title, bookType, maxVolCount, curVolCount);
            Debug.WriteLine(_series.ToString());
            MainWindowViewModel.Collection.Add(_series);
        }
    }
}
