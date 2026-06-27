using System.Collections.Frozen;
using System.Collections.Specialized;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Collections;
using MangaAndLightNovelWebScrape;
using MangaAndLightNovelWebScrape.Websites;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Tsundoku.ViewModels;

/// <summary>
/// View model for the price analysis feature, managing website selection, region, and analysis results.
/// </summary>
public sealed partial class PriceAnalysisViewModel : ViewModelBase, IDisposable
{
    [Reactive] public partial AvaloniaList<EntryModel> AnalyzedList { get; set; } = [];
    [Reactive] public partial int BrowserIndex { get; set; }
    [Reactive] public partial bool IsAnalyzeButtonEnabled { get; set; } = false;
    [Reactive] public partial bool WebsitesSelected { get; set; } = false;
    [Reactive] public partial string WebsitesToolTipText { get; set; }
    [Reactive] public partial int CurRegionIndex { get; set; }
    public AvaloniaList<string> AvailableWebsites { get; } = [];
    public AvaloniaList<string> SelectedWebsites { get; } = [];
    private readonly StringBuilder _curWebsites = new();
    private static readonly Region[] CachedRegionValues = Enum.GetValues<Region>();

    private static readonly FrozenDictionary<Website, string> WebsiteTitles =
        new Dictionary<Website, string>
        {
            [Website.AllStarComics] = AllStarComics.TITLE,
            [Website.AmazonUSA] = AmazonUSA.TITLE,
            [Website.BooksAMillion] = BooksAMillion.TITLE,
            [Website.Crunchyroll] = Crunchyroll.TITLE,
            [Website.ForbiddenPlanet] = ForbiddenPlanet.TITLE,
            [Website.InStockTrades] = InStockTrades.TITLE,
            [Website.KingsComics] = KingsComics.TITLE,
            [Website.KinokuniyaUSA] = KinokuniyaUSA.TITLE,
            [Website.MangaMart] = MangaMart.TITLE,
            [Website.MangaMate] = MangaMate.TITLE,
            [Website.MerryManga] = MerryManga.TITLE,
            [Website.OKComics] = OKComics.TITLE,
            [Website.RobertsAnimeCornerStore] = RobertsAnimeCornerStore.TITLE,
            [Website.SciFier] = SciFier.TITLE,
            [Website.TravellingMan] = TravellingMan.TITLE,
        }.ToFrozenDictionary();

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

        this.WhenAnyValue(x => x.CurrentUser.Region)
            .DistinctUntilChanged()
            .Subscribe(region =>
            {
                CurRegionIndex = Array.IndexOf(CachedRegionValues, region);
                RebuildAvailableWebsites(region);
            })
            .DisposeWith(_disposables);
    }

    private void RebuildAvailableWebsites(Region region)
    {
        SelectedWebsites.Clear();
        AvailableWebsites.Clear();
        foreach (Website site in MangaAndLightNovelWebScrape.Helpers.GetRegionWebsiteList(region))
        {
            if (WebsiteTitles.TryGetValue(site, out string? title))
            {
                AvailableWebsites.Add(title);
            }
        }
    }

    /// <summary>
    /// Updates the current user's region for price analysis.
    /// </summary>
    /// <param name="newRegion">The new region to set.</param>
    public void UpdateUserRegion(Region newRegion)
    {
        _userService.UpdateUser(user => user.Region = newRegion);
    }

    private void WebsiteCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (SelectedWebsites is null || SelectedWebsites.Count == 0)
        {
            WebsitesToolTipText = string.Empty;
            return;
        }

        for (int x = 0; x < SelectedWebsites.Count - 1; x++)
        {
            _curWebsites.AppendLine(SelectedWebsites[x]);
        }
        _curWebsites.Append(SelectedWebsites[^1]);
        WebsitesToolTipText = _curWebsites.ToString();
        _curWebsites.Clear();
    }

    public void Dispose()
    {
        SelectedWebsites.CollectionChanged -= WebsiteCollectionChanged;
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}