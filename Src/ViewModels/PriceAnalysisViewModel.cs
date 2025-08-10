using System.Collections.Frozen;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using MangaAndLightNovelWebScrape;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels;

public sealed class PriceAnalysisViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public AvaloniaList<EntryModel> AnalyzedList { get; set; } = [];
    [Reactive] public int BrowserIndex { get; set; }
    [Reactive] public bool IsAnalyzeButtonEnabled { get; set; } = false;
    [Reactive] public bool WebsitesSelected { get; set; } = false;
    [Reactive] public string WebsitesToolTipText { get; set; }
    [Reactive] public int CurRegionIndex { get; set; }
    public AvaloniaList<ListBoxItem> SelectedWebsites { get; } = [];
    private static readonly StringBuilder CurWebsites = new();

    public static readonly FrozenSet<string> ANALYSIS_COUNTRY_OPTIONS = new[]
    {
        "America",
        "Australia",
        "Britain",
        "Canada",
        "Europe"
    }.ToFrozenSet();

    public static readonly FrozenSet<string> EXCLUDE_ANALYSIS_OPTIONS = new[]
    {
        "Exclude None",
        "Exclude All",
        "Exclude OOS & PO",
        "Exclude OOS & BO",
        "Exclude PO & BO",
        "Exclude OOS",
        "Exclude PO",
        "Exclude BO"
    }.ToFrozenSet();

    public PriceAnalysisViewModel(IUserService userService) : base(userService)
    {

        SelectedWebsites.CollectionChanged += WebsiteCollectionChanged;
        this.WhenAnyValue(x => x.CurrentUser.Region).Subscribe(x => CurRegionIndex = Array.IndexOf(Enum.GetValues<Region>(), x));
    }

    public void UpdateUserRegion(Region newRegion)
    {
        _userService.UpdateUser(user => user.Region = newRegion);
    }

    private void WebsiteCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
                if (SelectedWebsites is not null && SelectedWebsites.Count != 0)
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