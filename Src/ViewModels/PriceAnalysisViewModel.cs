using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using MangaAndLightNovelWebScrape;
using MangaAndLightNovelWebScrape.Models;
using MangaAndLightNovelWebScrape.Websites;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static MangaAndLightNovelWebScrape.Models.Constants;

namespace Tsundoku.ViewModels
{
    public class PriceAnalysisViewModel : ViewModelBase
    {
        public AvaloniaList<EntryModel> AnalyzedList { get; set; } = new AvaloniaList<EntryModel>();
        [Reactive] public int BrowserIndex { get; set; }
        [Reactive] public bool IsAnalyzeButtonEnabled { get; set; } = false;
        [Reactive] public bool WebsitesSelected { get; set; }
        [Reactive] public string WebsitesToolTipText { get; set; }
        [Reactive] public int CurRegionIndex { get; set; }
        public AvaloniaList<ListBoxItem> SelectedWebsites { get; } = [];
        private static readonly StringBuilder CurWebsites = new StringBuilder();
        public PriceAnalysisViewModel()
        {
            this.CurrentTheme = MainUser.SavedThemes.First(theme => theme.ThemeName.Equals(MainUser.MainTheme));
            SelectedWebsites.CollectionChanged += WebsiteCollectionChanged;
            this.WhenAnyValue(x => x.CurRegion).Subscribe(x => CurRegionIndex = Array.IndexOf(Enum.GetValues<Region>(), x));
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