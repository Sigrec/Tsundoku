using Avalonia.Controls;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using MangaAndLightNovelWebScrape;
using ReactiveUI;
using System.Reactive.Linq;
using MangaAndLightNovelWebScrape.Websites;
using static MangaAndLightNovelWebScrape.Models.Constants;
using Src.Models;

namespace Tsundoku.Views
{
    public partial class PriceAnalysisWindow : Window
    {
        public PriceAnalysisViewModel? PriceAnalysisVM => DataContext as PriceAnalysisViewModel;
        public bool IsOpen = false, Manga;
        public readonly MasterScrape Scrape = new MasterScrape(StockStatusFilter.EXCLUDE_NONE_FILTER);

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
                if (IsOpen)
                {
                    ((PriceAnalysisWindow)s).Hide();
                    TitleBox.Text = string.Empty;
                    Topmost = false;
                    IsOpen ^= true;
                }
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
                Scrape.Browser = MangaAndLightNovelWebScrape.Helpers.GetBrowserFromString((BrowserSelector.SelectedItem as ComboBoxItem).Content.ToString());
                Scrape.Region = MangaAndLightNovelWebScrape.Helpers.GetRegionFromString((RegionSelector.SelectedItem as ComboBoxItem).Content.ToString());
                LOGGER.Info($"Started Scrape For \"{TitleBox.Text}\" on {Scrape.Browser} Browser w/ Region = \"{Scrape.Region}\" & \"{(StockFilterSelector.SelectedItem as ComboBoxItem).Content} Filter\" & Websites = [{string.Join(", ", PriceAnalysisVM.SelectedWebsites.Select(site => site.Content.ToString()))}] & Memberships = ({string.Join(" & ", ViewModelBase.MainUser.Memberships)})");
                Scrape.Filter = (StockFilterSelector.SelectedItem as ComboBoxItem).Content.ToString() switch
                                {
                                    "Exclude All" => StockStatusFilter.EXCLUDE_ALL_FILTER,
                                    "Exclude OOS & PO" => StockStatusFilter.EXCLUDE_OOS_AND_PO_FILTER,
                                    "Exclude OOS & BO" => StockStatusFilter.EXCLUDE_OOS_AND_BO_FILTER,
                                    "Exclude PO & BO" => StockStatusFilter.EXCLUDE_PO_AND_BO_FILTER,
                                    "Exclude OOS" => StockStatusFilter.EXCLUDE_OOS_FILTER,
                                    "Exclude PO" => StockStatusFilter.EXCLUDE_PO_FILTER,
                                    "Exclude bo" => StockStatusFilter.EXCLUDE_BO_FILTER,
                                    _ => StockStatusFilter.EXCLUDE_NONE_FILTER
                                };
                await Scrape.InitializeScrapeAsync(
                        TitleBox.Text, 
                        MangaButton.IsChecked != null && MangaButton.IsChecked.Value ? BookType.Manga : BookType.LightNovel, 
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

        private void RegionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RegionSelector.IsDropDownOpen)
            {
                Region newRegion = MangaAndLightNovelWebScrape.Helpers.GetRegionFromString((RegionSelector.SelectedItem as ComboBoxItem).Content.ToString());
                LOGGER.Info("Region Changed to {}", newRegion.ToString());
                ViewModelBase.MainUser.Region = newRegion;
                PriceAnalysisVM.CurRegion = newRegion;
            }
        }

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
