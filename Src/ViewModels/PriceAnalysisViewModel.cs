using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using MangaLightNovelWebScrape;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels
{
    public class PriceAnalysisViewModel : ViewModelBase
    {
        public ObservableCollection<EntryModel> AnalyzedList { get; set; }
        [Reactive] public string CurBrowser { get; set; }
        [Reactive] public int BrowserIndex { get; set; }
        [Reactive] public bool IsAnalyzeButtonEnabled { get; set; } = false;
        public ObservableCollection<ListBoxItem> SelectedWebsites { get; } = new ObservableCollection<ListBoxItem>();

        public PriceAnalysisViewModel()
        {
            
        }
    }
}