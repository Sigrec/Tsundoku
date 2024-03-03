using Avalonia.Controls;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using MangaAndLightNovelWebScrape;
using ReactiveUI;
using System.Reactive.Linq;
using MangaAndLightNovelWebScrape.Websites;
using MangaAndLightNovelWebScrape.Models;
using System.Collections;

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

            this.WhenAnyValue(x => x.TitleBox.Text, x => x.MangaButton.IsChecked, x => x.NovelButton.IsChecked, x => x.BrowserSelector.SelectedItem, x => x.RegionSelector.SelectedItem, x => x.PriceAnalysisVM.WebsitesSelected, (title, manga, novel, browser, region, websiteCheck) => !string.IsNullOrWhiteSpace(title) && !(manga == false && novel == false && websiteCheck) && browser != null && region != null && websiteCheck).Subscribe(x => PriceAnalysisVM.IsAnalyzeButtonEnabled = x);
        }

        private void IsMangaButtonClicked(object sender, RoutedEventArgs args)
        {
            NovelButton.IsChecked = false;
        }

        private void IsNovelButtonClicked(object sender, RoutedEventArgs args)
        {
            MangaButton.IsChecked = false;
        }

        private static bool IsWebsiteListValid(IList input)
        {
            Region region = ViewModelBase.MainUser.Region;
            foreach (ListBoxItem website in input)
            {
                bool isValid = website.Content.ToString() switch
                {
                    AmazonJapan.WEBSITE_TITLE => AmazonJapan.REGION.HasFlag(region),
                    AmazonUSA.WEBSITE_TITLE => AmazonUSA.REGION.HasFlag(region),
                    BarnesAndNoble.WEBSITE_TITLE => BarnesAndNoble.REGION.HasFlag(region),
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
                    SpeedyHen.WEBSITE_TITLE => SpeedyHen.REGION.HasFlag(region),
                    Waterstones.WEBSITE_TITLE => Waterstones.REGION.HasFlag(region),
                    Wordery.WEBSITE_TITLE => Wordery.REGION.HasFlag(region),
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
                StartScrapeButton.IsEnabled = false;
                Scrape.Browser = MangaAndLightNovelWebScrape.Helpers.GetBrowserFromString((BrowserSelector.SelectedItem as ComboBoxItem).Content.ToString());
                Scrape.Region = MangaAndLightNovelWebScrape.Helpers.GetRegionFromString((RegionSelector.SelectedItem as ComboBoxItem).Content.ToString());
                Scrape.Filter = MangaAndLightNovelWebScrape.Helpers.GetStockStatusFilterFromString((StockFilterSelector.SelectedItem as ComboBoxItem).Content.ToString());
                Scrape.IsBarnesAndNobleMember = ViewModelBase.MainUser.Memberships[BarnesAndNoble.WEBSITE_TITLE];
                Scrape.IsBooksAMillionMember = ViewModelBase.MainUser.Memberships[BooksAMillion.WEBSITE_TITLE];
                Scrape.IsKinokuniyaUSAMember = ViewModelBase.MainUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE];
                Scrape.IsIndigoMember = ViewModelBase.MainUser.Memberships[Indigo.WEBSITE_TITLE];

                LOGGER.Info($"Started Scrape For \"{TitleBox.Text}\" on {Scrape.Browser} Browser w/ Region = \"{Scrape.Region}\" & \"{(StockFilterSelector.SelectedItem as ComboBoxItem).Content} Filter\" & Websites = [{string.Join(", ", PriceAnalysisVM.SelectedWebsites.Select(site => site.Content.ToString()))}] & Memberships = ({string.Join(" & ", ViewModelBase.MainUser.Memberships)})");
                
                await Scrape.InitializeScrapeAsync(
                        TitleBox.Text, 
                        MangaButton.IsChecked != null && MangaButton.IsChecked.Value ? BookType.Manga : BookType.LightNovel, 
                        Scrape.GenerateWebsiteList(PriceAnalysisVM.SelectedWebsites.Select(site => site.Content.ToString()).ToList())
                    );
                StartScrapeButton.IsEnabled = PriceAnalysisVM.IsAnalyzeButtonEnabled;
                LOGGER.Info($"Scrape Finished");

                PriceAnalysisVM.AnalyzedList.Clear();
                PriceAnalysisVM.AnalyzedList.AddRange(Scrape.GetResults());
                AnalysisDataGrid.Columns[3].Width = DataGridLength.SizeToCells;
                this.SizeToContent = SizeToContent.Height;
            }
            catch (Exception e)
            {
                LOGGER.Error("Price Analysis Scrape {} Failed -> {}", scrapeScenario, e.Message);
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
            var list = (sender as ListBox).SelectedItems;
            PriceAnalysisVM.WebsitesSelected = list.Count != 0 && IsWebsiteListValid(list);
        }

        private void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            ViewModelBase.OpenSiteLink(Scrape.GetResultUrls()[(sender as TextBlock).Text]);
        }
    }
}
