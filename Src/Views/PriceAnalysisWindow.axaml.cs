using Avalonia.Controls;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using MangaAndLightNovelWebScrape;
using ReactiveUI;
using System.Reactive.Linq;
using MangaAndLightNovelWebScrape.Websites;
using MangaAndLightNovelWebScrape.Models;
using System.Collections;
using Avalonia.ReactiveUI;

namespace Tsundoku.Views;

public sealed partial class PriceAnalysisWindow : ReactiveWindow<PriceAnalysisViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public bool IsOpen = false, Manga;
    public readonly MasterScrape Scrape = new(StockStatusFilter.EXCLUDE_NONE_FILTER);


    public PriceAnalysisWindow(PriceAnalysisViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        Opened += (s, e) =>
        {
            IsOpen ^= true;
        };

        Closing += (s, e) =>
        {
            if (IsOpen)
            {
                this.Hide();
                SearchTextBox.Text = string.Empty;
                Topmost = false;
                IsOpen ^= true;
            }
            e.Cancel = true;
        };

        this.WhenAnyValue(x => x.SearchTextBox.Text, x => x.MangaButton.IsChecked, x => x.NovelButton.IsChecked, x => x.BrowserSelector.SelectedItem, x => x.RegionComboBox.SelectedItem, x => x.ViewModel.WebsitesSelected, (title, manga, novel, browser, region, websiteCheck) => !string.IsNullOrWhiteSpace(title) && !(manga == false && novel == false && websiteCheck) && browser is not null && region is not null && websiteCheck)
            .Subscribe(x => ViewModel.IsAnalyzeButtonEnabled = x);
    }

    private void IsMangaButtonClicked(object sender, RoutedEventArgs args)
    {
        NovelButton.IsChecked = false;
    }

    private void IsNovelButtonClicked(object sender, RoutedEventArgs args)
    {
        MangaButton.IsChecked = false;
    }

    private bool IsWebsiteListValid(IList input)
    {
        Region region = ViewModel.CurrentUser.Region;
        foreach (ListBoxItem website in input)
        {
            bool isValid = website.Content.ToString() switch
            {
                AmazonJapan.WEBSITE_TITLE => AmazonJapan.REGION.HasFlag(region),
                AmazonUSA.WEBSITE_TITLE => AmazonUSA.REGION.HasFlag(region),
                BooksAMillion.WEBSITE_TITLE => BooksAMillion.REGION.HasFlag(region),
                CDJapan.WEBSITE_TITLE => CDJapan.REGION.HasFlag(region),
                Crunchyroll.WEBSITE_TITLE => Crunchyroll.REGION.HasFlag(region),
                ForbiddenPlanet.WEBSITE_TITLE => ForbiddenPlanet.REGION.HasFlag(region),
                Indigo.WEBSITE_TITLE => Indigo.REGION.HasFlag(region),
                InStockTrades.WEBSITE_TITLE => InStockTrades.REGION.HasFlag(region),
                KinokuniyaUSA.WEBSITE_TITLE => KinokuniyaUSA.REGION.HasFlag(region),
                MangaMate.WEBSITE_TITLE => MangaMate.REGION.HasFlag(region),
                MerryManga.WEBSITE_TITLE => MerryManga.REGION.HasFlag(region),
                RobertsAnimeCornerStore.WEBSITE_TITLE => RobertsAnimeCornerStore.REGION.HasFlag(region),
                SciFier.WEBSITE_TITLE => SciFier.REGION.HasFlag(region),
                Waterstones.WEBSITE_TITLE => Waterstones.REGION.HasFlag(region),
                _ => throw new NotImplementedException(),
            };
            if (!isValid) { return false; }
        }
        return true;
    }

    public async void PerformAnalysis(object sender, RoutedEventArgs args)
    {
        string scrapeScenario = string.Empty;
        try
        {
            HashSet<string> websiteList = [.. ViewModel.SelectedWebsites.AsValueEnumerable().Select(website => website.Content.ToString())];

            scrapeScenario = $"\"{SearchTextBox.Text}\" on {Scrape.Browser} Browser w/ Region = \"{Scrape.Region}\" & \"{StockFilterSelector.SelectedItem as string} Filter\" & Websites = [{string.Join(", ", websiteList)}] & Memberships = ({string.Join(" & ", ViewModel.CurrentUser.Memberships)})";

            ToggleControlEnablement();
            StartScrapeButton.Content = "Analyzing...";

            Scrape.Browser = MangaAndLightNovelWebScrape.Helpers.GetBrowserFromString((BrowserSelector.SelectedItem as ComboBoxItem).Content.ToString());
            Scrape.Region = ViewModel.CurrentUser.Region;
            Scrape.Filter = MangaAndLightNovelWebScrape.Helpers.GetStockStatusFilterFromString(StockFilterSelector.SelectedItem as string);
            Scrape.IsBooksAMillionMember = ViewModel.CurrentUser.Memberships[BooksAMillion.WEBSITE_TITLE];
            Scrape.IsKinokuniyaUSAMember = ViewModel.CurrentUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE];
            Scrape.IsIndigoMember = ViewModel.CurrentUser.Memberships[Indigo.WEBSITE_TITLE];

            LOGGER.Info("Started Scrape For {Scenario}", scrapeScenario);

            await Scrape.InitializeScrapeAsync(
                title: SearchTextBox.Text,
                bookType: MangaButton.IsChecked is not null && MangaButton.IsChecked.Value ? BookType.Manga : BookType.LightNovel,
                Scrape.GenerateWebsiteList(websiteList)
            );
            LOGGER.Info($"Scrape Finished");

            ViewModel.AnalyzedList.Clear();
            ViewModel.AnalyzedList.AddRange(Scrape.GetResults());
            // AnalysisDataGrid.Columns[3].Width = DataGridLength.SizeToCells;
            this.SizeToContent = SizeToContent.Height;
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Price Analysis Scrape {Scenario} Failed", scrapeScenario);
        }
        finally
        {
            ToggleControlEnablement();
            StartScrapeButton.IsEnabled = ViewModel.IsAnalyzeButtonEnabled;
            StartScrapeButton.Content = "Analyze";
        }
    }

    private void ToggleControlEnablement()
    {
        SearchTextBox.IsEnabled ^= true;
        MangaButton.IsEnabled ^= true;
        NovelButton.IsEnabled ^= true;
        WebsiteSelector.IsEnabled ^= true;
        BrowserSelector.IsEnabled ^= true;
        StockFilterSelector.IsEnabled ^= true;
        RegionComboBox.IsEnabled ^= true;
        StartScrapeButton.IsEnabled ^= true;
    }

    private void RegionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RegionComboBox.SelectedItem is string selectedRegion)
        {
            Region newRegion = MangaAndLightNovelWebScrape.Helpers.GetRegionFromString(selectedRegion);
            ViewModel.UpdateUserRegion(newRegion);
            LOGGER.Info("Region Changed to {}", selectedRegion);
        }
    }

    private void WebsiteSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        IList? list = (sender as ListBox).SelectedItems;
        ViewModel.WebsitesSelected = list.Count != 0 && IsWebsiteListValid(list);
    }

    private async void OpenSiteLinkAsync(object sender, PointerPressedEventArgs args)
    {
        await ViewModelBase.OpenSiteLink(Scrape.GetResultUrls()[(sender as TextBlock).Text]);
    }
}
