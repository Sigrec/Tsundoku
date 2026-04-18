using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using MangaAndLightNovelWebScrape;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using MangaAndLightNovelWebScrape.Models;
using System.Linq.Dynamic.Core;
using MangaAndLightNovelWebScrape.Websites;
using ReactiveUI.Avalonia;
using Tsundoku.Helpers;

namespace Tsundoku.Views;

public sealed partial class PriceAnalysisWindow : ReactiveWindow<PriceAnalysisViewModel>, IManagedWindow
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public bool IsOpen { get; set; }
    public bool Manga;
    public readonly MasterScrape _scrape = new(StockStatusFilter.EXCLUDE_NONE_FILTER);
    private Region CurRegion;

    public PriceAnalysisWindow(PriceAnalysisViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        this.ConfigureHideOnClose(onClosing: () => SearchTextBox.Text = string.Empty);

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(
                    x => x.SearchTextBox.Text,
                    x => x.MangaButton.IsChecked,
                    x => x.NovelButton.IsChecked)
                .CombineLatest(
                    this.WhenAnyValue(
                        x => x.BrowserSelector.SelectedItem,
                        x => x.RegionComboBox.SelectedItem,
                        x => x.ViewModel.WebsitesSelected))
                .Select(values =>
                {
                    var (title, manga, novel) = values.First;
                    var (browser, region, websiteCheck) = values.Second;
                    return !string.IsNullOrWhiteSpace(title)
                        && !(manga == false && novel == false && websiteCheck)
                        && browser is not null
                        && region is not null
                        && websiteCheck;
                })
                .Subscribe(x => ViewModel.IsAnalyzeButtonEnabled = x)
                .DisposeWith(disposables);
        });
    }

    private void IsMangaButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is ToggleButton { IsChecked: true })
        {
            NovelButton.IsChecked = false;
        }
    }

    private void IsNovelButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is ToggleButton { IsChecked: true })
        {
            MangaButton.IsChecked = false;
        }
    }

    public async void PerformAnalysis(object sender, RoutedEventArgs args)
    {
        string scrapeScenario = string.Empty;
        try
        {
            HashSet<Website> websiteList = [.. ViewModel.SelectedWebsites.AsValueEnumerable().Select(website => MangaAndLightNovelWebScrape.Helpers.GetWebsiteFromString(website.Content?.ToString() ?? string.Empty))];

            scrapeScenario = $"\"{SearchTextBox.Text}\" on {_scrape.Browser} Browser w/ Region = \"{_scrape.Region}\" & \"{StockFilterSelector.SelectedItem as string} Filter\" & Websites = [{string.Join(", ", websiteList)}] & Memberships = ({string.Join(" & ", ViewModel.CurrentUser.Memberships)})";

            ToggleControlEnablement();
            StartScrapeButtonText.Text = "Analyzing...";

            _scrape.Browser = MangaAndLightNovelWebScrape.Helpers.GetBrowserFromString((BrowserSelector.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty);
            _scrape.Region = ViewModel.CurrentUser.Region;
            _scrape.Filter = MangaAndLightNovelWebScrape.Helpers.GetStockStatusFilterFromString(StockFilterSelector.SelectedItem as string);
            _scrape.IsBooksAMillionMember = ViewModel.CurrentUser.Memberships[BooksAMillion.TITLE];
            _scrape.IsKinokuniyaUSAMember = ViewModel.CurrentUser.Memberships[KinokuniyaUSA.TITLE];

            LOGGER.Info("Started Scrape For {Scenario}", scrapeScenario);

            await _scrape.InitializeScrapeAsync(
                title: SearchTextBox.Text,
                bookType: MangaButton.IsChecked is not null && MangaButton.IsChecked.Value ? BookType.Manga : BookType.LightNovel,
                websiteList
            );
            LOGGER.Info("Scrape Finished");

            ViewModel.AnalyzedList.Clear();
            ViewModel.AnalyzedList.AddRange(_scrape.GetResults());
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Price Analysis Scrape {Scenario} Failed", scrapeScenario);
        }
        finally
        {
            ToggleControlEnablement();
            StartScrapeButton.IsEnabled = ViewModel.IsAnalyzeButtonEnabled;
            StartScrapeButtonText.Text = "Start Analysis";
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
            CurRegion = newRegion;
            ViewModel.UpdateUserRegion(newRegion);
            LOGGER.Info("Region Changed to {}", selectedRegion);
        }
    }

    private void WebsiteSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox) { return; }
        string[] list = [.. listBox.SelectedItems.AsValueEnumerable().Cast<ListBoxItem>().Select(x => x.Content?.ToString() ?? string.Empty)];
        ViewModel.WebsitesSelected = list.Length != 0 && MangaAndLightNovelWebScrape.Helpers.IsWebsiteListValid(CurRegion, list);
    }

    private async void OpenSiteLinkAsync(object sender, PointerPressedEventArgs args)
    {
        await ViewModelBase.OpenSiteLink(_scrape.GetResultUrls()[MangaAndLightNovelWebScrape.Helpers.GetWebsiteFromString((sender as TextBlock).Text)]);
    }
}
