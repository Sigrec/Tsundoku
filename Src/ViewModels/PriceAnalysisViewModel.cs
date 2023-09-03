using System.Collections.Generic;
using System.Collections.ObjectModel;
using MangaLightNovelWebScrape;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels
{
    public class PriceAnalysisViewModel : ViewModelBase
    {
        public static readonly List<string> AvailableWebsites = new List<string> { "BarnesAndNoble", "BooksAMillion", "InStockTrades", "KinokuniyaUSA", "RightStufAnime", "RobertsAnimeCornerStore"};

        public ObservableCollection<EntryModel> AnalyzedList { get; }

        [Reactive]
        public string CurBrowser { get; set; }

        public PriceAnalysisViewModel()
        {
            var test = new List<EntryModel>
            {
                new EntryModel("World Trigger Vol 1", "$7.99", "IS", "RightStufAnime"),
                new EntryModel("World Trigger Vol 2", "$7.99", "IS", "RightStufAnime"),
                new EntryModel("World Trigger Vol 3", "$7.99", "IS", "RightStufAnime")
            };
            AnalyzedList = new ObservableCollection<EntryModel>(test);
        }
    }
}