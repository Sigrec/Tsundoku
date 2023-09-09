using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using MangaLightNovelWebScrape;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels
{
    public class PriceAnalysisViewModel : ViewModelBase
    {
        public ObservableCollection<EntryModel> AnalyzedList { get; set; } = new ObservableCollection<EntryModel>();
        [Reactive] public string CurBrowser { get; set; }
        [Reactive] public int BrowserIndex { get; set; }
        [Reactive] public bool IsAnalyzeButtonEnabled { get; set; } = false;
        [Reactive] public bool WebsitesSelected { get; set; }
        public ObservableCollection<ListBoxItem> SelectedWebsites { get; } = new ObservableCollection<ListBoxItem>();

        public PriceAnalysisViewModel()
        {
            // AnalyzedList.Add(new EntryModel("World Trigger #1", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #2", "$9.99", "IS", "Kinokuniya USA"));
            // AnalyzedList.Add(new EntryModel("World Trigger #3", "$9.99", "IS", "InStockTrades"));
            // AnalyzedList.Add(new EntryModel("World Trigger #4", "$9.99", "IS", "Banres & Noble"));
            // AnalyzedList.Add(new EntryModel("World Trigger #5", "$9.99", "IS", "Books-A-Million"));
            // AnalyzedList.Add(new EntryModel("World Trigger #6", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #7", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #8", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #9", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #100", "$109.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger # 2", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #3", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #4", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #5", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #6", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #7", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #8", "$9.99", "IS", "RightStufAnime"));
            // AnalyzedList.Add(new EntryModel("World Trigger #9", "$9.99", "IS", "RightStufAnime"));
        }
    }
}