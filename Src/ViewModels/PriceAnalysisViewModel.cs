using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
using MangaLightNovelWebScrape;
using ReactiveUI.Fody.Helpers;
using static Src.Models.Constants;

namespace Tsundoku.ViewModels
{
    public class PriceAnalysisViewModel : ViewModelBase
    {
        public ObservableCollection<EntryModel> AnalyzedList { get; set; } = new ObservableCollection<EntryModel>();
        [Reactive] public int BrowserIndex { get; set; }
        [Reactive] public bool IsAnalyzeButtonEnabled { get; set; } = false;
        [Reactive] public bool WebsitesSelected { get; set; }
        [Reactive] public string WebsitesToolTipText { get; set; }
        public ObservableCollection<ListBoxItem> SelectedWebsites { get; } = [];
        private static readonly StringBuilder CurWebsites = new StringBuilder();

        public PriceAnalysisViewModel()
        {
            AnalyzedList.Add(new EntryModel("World Trigger Vol 1", "$0.00", StockStatus.IS, "Crunchyroll"));
            AnalyzedList.Add(new EntryModel("World Trigger Vol 1", "$0.00", StockStatus.IS, "Crunchyroll"));
            AnalyzedList.Add(new EntryModel("World Trigger Vol 1", "$0.00", StockStatus.IS, "Crunchyroll"));
            AnalyzedList.Add(new EntryModel("World Trigger Vol 1", "$0.00", StockStatus.IS, "Crunchyroll"));
            AnalyzedList.Add(new EntryModel("World Trigger Vol 1", "$0.00", StockStatus.IS, "Crunchyroll"));
            this.CurrentTheme = MainUser.SavedThemes.First(theme => theme.ThemeName.Equals(MainUser.MainTheme));
            SelectedWebsites.CollectionChanged += WebsiteCollectionChanged;
        }

        private void WebsiteCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (SelectedWebsites != null && SelectedWebsites.Any())
                    {
                        for (int x = 0; x < SelectedWebsites.Count - 1; x++)
                        {
                            CurWebsites.AppendLine(SelectedWebsites[x].Content.ToString());
                        }
                        CurWebsites.Append(SelectedWebsites.Last().Content.ToString());
                        WebsitesToolTipText = CurWebsites.ToString();
                        CurWebsites.Clear();
                    }
                    else
                    {
                        CurWebsites.Clear();
                        WebsitesToolTipText = CurWebsites.ToString();
                    }
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }
}