using Avalonia.Controls;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using MangaLightNovelWebScrape;
using DynamicData;
using ReactiveUI;
using System.Reactive.Linq;
using Avalonia.Input;
using MangaLightNovelWebScrape.Websites.America;
using static Src.Models.Constants;
using MangaLightNovelWebScrape.Websites.Canada;

namespace Tsundoku.Views
{
    public partial class PriceAnalysisWindow : Window
    {
        public PriceAnalysisViewModel? PriceAnalysisVM => DataContext as PriceAnalysisViewModel;
        public bool IsOpen = false, Manga;
        public readonly MasterScrape Scrape = new MasterScrape().DisableDebugMode();

        public PriceAnalysisWindow()
        {
            InitializeComponent();
            DataContext = new PriceAnalysisViewModel();
            Opened += (s, e) =>
            {
                IsOpen ^= true;
            };

            Closing += (s, e) =>
            {
                ((PriceAnalysisWindow)s).Hide();
                TitleBox.Text = string.Empty;
                Topmost = false;
                IsOpen ^= true;
                e.Cancel = true;
            };

            this.WhenAnyValue(x => x.TitleBox.Text, x => x.MangaButton.IsChecked, x => x.NovelButton.IsChecked, x => x.BrowserSelector.SelectedItem, x => x.PriceAnalysisVM.WebsitesSelected, x => x.RegionSelector.SelectedItem, (title, manga, novel, browser, websites, region) => !string.IsNullOrWhiteSpace(title) && !(manga == false && novel == false) && browser != null && websites && region != null).Subscribe(x => PriceAnalysisVM.IsAnalyzeButtonEnabled = x);
        }

        private void IsMangaButtonClicked(object sender, RoutedEventArgs args)
        {
            NovelButton.IsChecked = false;
        }

        private void IsNovelButtonClicked(object sender, RoutedEventArgs args)
        {
            MangaButton.IsChecked = false;
        }
        
        public async void PerformAnalysis(object sender, RoutedEventArgs args)
        {
            try
            {
                StartScrapeButton.IsEnabled = false;
                Scrape.Browser = MasterScrape.GetBrowserFromString((BrowserSelector.SelectedItem as ComboBoxItem).Content.ToString());
                Scrape.Region = MasterScrape.GetRegionFromString((RegionSelector.SelectedItem as ComboBoxItem).Content.ToString());
                LOGGER.Info($"Started Scrape For {TitleBox.Text} on {Scrape.Browser} Browser w/ Region = {Scrape.Region} {(StockFilterSelector.SelectedItem as ComboBoxItem).Content} Filter & Websites = [{string.Join(", ", PriceAnalysisVM.SelectedWebsites.Select(site => site.Content.ToString()))}] & Memberships = {string.Join(" | ", ViewModelBase.MainUser.Memberships)}");
                await Scrape.InitializeScrapeAsync(
                        TitleBox.Text, 
                        MangaButton.IsChecked != null && MangaButton.IsChecked.Value ? BookType.Manga : BookType.LightNovel, 
                        (StockFilterSelector.SelectedItem as ComboBoxItem).Content.ToString() switch
                        {
                            "Exclude PO & OOS" => MasterScrape.EXCLUDE_BOTH_FILTER,
                            "Exclude PO" => MasterScrape.EXCLUDE_PO_FILTER,
                            "Exclude OOS" => MasterScrape.EXCLUDE_OOS_FILTER,
                            _ => MasterScrape.EXCLUDE_NONE_FILTER
                        }, 
                        Scrape.GenerateWebsiteList(PriceAnalysisVM.SelectedWebsites.Select(site => site.Content.ToString()).ToList()),
                        ViewModelBase.MainUser.Memberships[BarnesAndNoble.WEBSITE_TITLE], 
                        ViewModelBase.MainUser.Memberships[BooksAMillion.WEBSITE_TITLE], 
                        ViewModelBase.MainUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE],
                        ViewModelBase.MainUser.Memberships[Indigo.WEBSITE_TITLE]
                    );
                StartScrapeButton.IsEnabled = PriceAnalysisVM.IsAnalyzeButtonEnabled;
                LOGGER.Info($"Scrape Finished");

                PriceAnalysisVM.AnalyzedList.Clear();
                PriceAnalysisVM.AnalyzedList.AddRange(Scrape.GetResults()); 
                this.SizeToContent = SizeToContent.Height;
            }
            catch (Exception e)
            {
                LOGGER.Error($"Price Analysis Scraped Failed -> {e}");
            }
        }

        // private void BrowserChanged(object sender, SelectionChangedEventArgs e)
        // {
        //     if ((sender as ComboBox).IsDropDownOpen)
        //     {
        //         PriceAnalysisVM.CurBrowser = (BrowserSelector.SelectedItem as ComboBoxItem).Content.ToString();
        //     }
        // }

        private void WebsiteSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PriceAnalysisVM.WebsitesSelected = (sender as ListBox).SelectedItems.Count > 0;
        }

        private void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            ViewModelBase.OpenSiteLink(Scrape.GetResultUrls()[(sender as TextBlock).Text]);
        }
    }
}
