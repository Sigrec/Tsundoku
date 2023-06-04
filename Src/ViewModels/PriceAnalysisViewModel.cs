using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels
{
    public class PriceAnalysisViewModel : ViewModelBase
    {
        public static ObservableCollection<string[]> testData = new ObservableCollection<string[]> { new string[]{"World Trigger Vol 1", "$7.99", "IS", "RightStufAnime"}, new string[]{"World Trigger Vol 2", "$7.99", "IS", "RightStufAnime"}, new string[]{"World Trigger Vol 3", "$7.99", "IS", "RightStufAnime"}, new string[]{"World Trigger Vol 4", "$7.99", "IS", "RightStufAnime"} };

        [Reactive]
        public string CurBrowser { get; set; }

        private string[] AvailableBrowsers { get; } = new string[] { "Edge", "Chrome", "FireFox" };

        private string[] AvailableWebsites { get; } = new string[] { "RightStufAnime", "RobertsAnimeCornerStore", "InStockTrades", "Kinokuniya USA", "Barnes & Noble", "Books-A-Million" };

        public PriceAnalysisViewModel()
        {

        }
    }
}