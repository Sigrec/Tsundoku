using System.Collections.ObjectModel;
using Avalonia.Controls;
using MangaLightNovelWebScrape;
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
        public ObservableCollection<ListBoxItem> SelectedWebsites { get; } = [];

        public PriceAnalysisViewModel()
        {

        }
    }
}