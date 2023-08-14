using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels
{
    public class PriceAnalysisViewModel : ViewModelBase
    {
        // public static readonly List<string> AvailableWebsites = new List<string> { "BarnesAndNoble", "BooksAMillion", "InStockTrades", "KinokuniyaUSA", "RightStufAnime", "RobertsAnimeCornerStore"};

        [Reactive]
        public string CurBrowser { get; set; }

        public PriceAnalysisViewModel()
        {
        }
    }
}