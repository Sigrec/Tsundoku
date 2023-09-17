using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using MangaLightNovelWebScrape;
using DynamicData;
using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Reactive.Linq;
using Avalonia.Input;

namespace Tsundoku.Views
{
    public partial class PriceAnalysisWindow : Window
    {
        public PriceAnalysisViewModel? PriceAnalysisVM => DataContext as PriceAnalysisViewModel;
        public bool IsOpen = false, manga;
        private readonly MasterScrape Scrape = new();
        private string browser, website;
        private readonly string[] EXCLUDE_NONE_FILTER = Array.Empty<string>();
        private readonly string[] EXCLUDE_BOTH_FILTER = { "PO", "OOS" };
        private readonly string[] EXCLUDE_PO_FILTER = { "PO" };
        private readonly string[] EXCLUDE_OOS_FILTER = { "OOS" };
        private static string[] CurStockFilter;
        private static List<MasterScrape.Website> scrapeWebsiteList = new List<MasterScrape.Website>();
        MainWindow CollectionWindow;

        // [AMD64-Windows_NT] 2023-09-08 08:29:28.8109 | DEBUG > Analysis Window Width = 720
        // [AMD64-Windows_NT] 2023-09-08 08:29:28.8109 | DEBUG > Analysis Window HEight = 611
        public PriceAnalysisWindow()
        {
            InitializeComponent();
            MasterScrape.DisableDebugMode();
            DataContext = new PriceAnalysisViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                PriceAnalysisVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
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

            this.WhenAnyValue(x => x.TitleBox.Text, x => x.MangaButton.IsChecked, x => x.NovelButton.IsChecked, x => x.PriceAnalysisVM.CurBrowser, x => x.PriceAnalysisVM.WebsitesSelected, (title, manga, novel, browser, websites) => !string.IsNullOrWhiteSpace(title) && !(manga == false && novel == false) && !string.IsNullOrWhiteSpace(browser) && websites).Subscribe(x => PriceAnalysisVM.IsAnalyzeButtonEnabled = x);
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
                scrapeWebsiteList.Clear();
                browser = PriceAnalysisVM.CurBrowser;
                Title = TitleBox.Text;
                manga = MangaButton.IsChecked != null ? MangaButton.IsChecked.Value : false;
                CurStockFilter = (StockFilterSelector.SelectedItem as ComboBoxItem).Content.ToString() switch
                {
                    "Exclude PO & OOS" => EXCLUDE_BOTH_FILTER,
                    "Exclude PO" => EXCLUDE_PO_FILTER,
                    "Exclude OOS" => EXCLUDE_OOS_FILTER,
                    _ => EXCLUDE_NONE_FILTER
                };

                foreach (ListBoxItem x in PriceAnalysisVM.SelectedWebsites)
                {
                    website = x.Content.ToString();
                    switch (website)
                    {
                        case "RightStufAnime":
                            scrapeWebsiteList.Add(MasterScrape.Website.RightStufAnime);
                            break;
                        case "Barnes & Noble":
                            scrapeWebsiteList.Add(MasterScrape.Website.BarnesAndNoble);
                            break;
                        case "Books-A-Million":
                            scrapeWebsiteList.Add(MasterScrape.Website.BooksAMillion);
                            break;
                        case "RobertsAnimeCornerStore":
                            scrapeWebsiteList.Add(MasterScrape.Website.RobertsAnimeCornerStore);
                            break;
                        case "InStockTrades":
                            scrapeWebsiteList.Add(MasterScrape.Website.InStockTrades);
                            break;
                        case "Kinokuniya USA":
                            scrapeWebsiteList.Add(MasterScrape.Website.KinokuniyaUSA);
                            break;
                        case "AmazonUSA":
                            scrapeWebsiteList.Add(MasterScrape.Website.AmazonUSA);
                            break;
                        case "AmazonJapan":
                            scrapeWebsiteList.Add(MasterScrape.Website.AmazonJapan);
                            break;
                        case "CDJapan":
                            scrapeWebsiteList.Add(MasterScrape.Website.CDJapan);
                            break;
                    }
                    LOGGER.Info($"Added {website} to Scrape");
                }
                LOGGER.Debug($"Memberships = {ViewModelBase.MainUser.Memberships["RightStufAnime"]} {ViewModelBase.MainUser.Memberships["BarnesAndNoble"]} {ViewModelBase.MainUser.Memberships["BooksAMillion"]} {ViewModelBase.MainUser.Memberships["KinokuniyaUSA"]}");

                StartScrapeButton.IsEnabled = false;
                LOGGER.Info($"Started Scrape w/ {browser} Browser & [{string.Join(",", CurStockFilter)}] Filters");
                await Scrape.InitializeScrapeAsync(Title, manga == true ? 'M' : 'N', CurStockFilter, scrapeWebsiteList, browser, ViewModelBase.MainUser.Memberships["RightStufAnime"], ViewModelBase.MainUser.Memberships["BarnesAndNoble"], ViewModelBase.MainUser.Memberships["BooksAMillion"], ViewModelBase.MainUser.Memberships["KinokuniyaUSA"]);

                StartScrapeButton.IsEnabled = PriceAnalysisVM.IsAnalyzeButtonEnabled;
                LOGGER.Info($"Scrape Finished");

                PriceAnalysisVM.AnalyzedList.Clear();
                PriceAnalysisVM.AnalyzedList.AddRange(Scrape.GetResults());
                MasterScrape.ClearAllWebsiteData();
            }
            catch (Exception e)
            {
                LOGGER.Error($"{e}");
            }
        }

        private void BrowserChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                PriceAnalysisVM.CurBrowser = (BrowserSelector.SelectedItem as ComboBoxItem).Content.ToString();
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
