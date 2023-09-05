using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Tsundoku.Models;
using MangaLightNovelWebScrape;
using System.Collections.ObjectModel;
using DynamicData;
using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Threading.Tasks;

namespace Tsundoku.Views
{
    public partial class PriceAnalysisWindow : Window
    {
        public PriceAnalysisViewModel? PriceAnalysisVM => DataContext as PriceAnalysisViewModel;
        public bool IsOpen = false;
        private MasterScrape Scrape = new MasterScrape();
        private static List<MasterScrape.Website> scrapeWebsiteList = new List<MasterScrape.Website>();
        MainWindow CollectionWindow;

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
                Topmost = false;
                IsOpen ^= true;
                e.Cancel = true;
            };

            // TODO Need to figure out how to check for website list being empty
            this.WhenAnyValue(x => x.TitleBox.Text, x => x.MangaButton.IsChecked, x => x.NovelButton.IsChecked, x => x.PriceAnalysisVM.CurBrowser, (title, manga, novel, browser) => !string.IsNullOrWhiteSpace(title) && !((manga == false) && novel == false) && manga != null && novel != null && !string.IsNullOrWhiteSpace(browser)).Subscribe(x => PriceAnalysisVM.IsAnalyzeButtonEnabled = x);
        }

        private void IsMangaButtonClicked(object sender, RoutedEventArgs args)
        {
            NovelButton.IsChecked = false;
        }


        private void IsNovelButtonClicked(object sender, RoutedEventArgs args)
        {
            MangaButton.IsChecked = false;
        }
        
        public void PerformAnalysis(object sender, RoutedEventArgs args)
        {
            // Create the Website list for the scrape
            scrapeWebsiteList.Clear();
            string website;
            foreach (ListBoxItem x in PriceAnalysisVM.SelectedWebsites)
            {
                Constants.Logger.Debug("Check2");
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
                    // case "CDJapan":
                    //     scrapeWebsiteList.Add(Website.CDJapan);
                    //     break;
                }
                Constants.Logger.Info($"Added {website} to Scrape");
            }
            Constants.Logger.Debug($"Memberships = {ViewModelBase.MainUser.Memberships["RightStufAnime"]} {ViewModelBase.MainUser.Memberships["BarnesAndNoble"]} {ViewModelBase.MainUser.Memberships["BooksAMillion"]} {ViewModelBase.MainUser.Memberships["KinokuniyaUSA"]}");
            Constants.Logger.Info($"Started Scrape w/ {PriceAnalysisVM.CurBrowser} Browser");
            Scrape.InitializeScrape(TitleBox.Text, MangaButton.IsChecked == true ? 'M' : 'N', scrapeWebsiteList, PriceAnalysisVM.CurBrowser, ViewModelBase.MainUser.Memberships["RightStufAnime"], ViewModelBase.MainUser.Memberships["BarnesAndNoble"], ViewModelBase.MainUser.Memberships["BooksAMillion"], ViewModelBase.MainUser.Memberships["KinokuniyaUSA"]);
            Constants.Logger.Info($"Scrape Finished");
            PriceAnalysisVM.AnalyzedList.Clear();
            PriceAnalysisVM.AnalyzedList.AddRange(Scrape.GetResults());
        }

        private void BrowserChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                PriceAnalysisVM.CurBrowser = (BrowserSelector.SelectedItem as ComboBoxItem).Content.ToString();
                Constants.Logger.Info($"Browser Langauge to {PriceAnalysisVM.CurBrowser}");
            }
        }
    }
}
