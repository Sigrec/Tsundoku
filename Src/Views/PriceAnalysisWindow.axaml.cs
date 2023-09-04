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

namespace Tsundoku.Views
{
    public partial class PriceAnalysisWindow : Window
    {
        public PriceAnalysisViewModel? PriceAnalysisVM => DataContext as PriceAnalysisViewModel;
        public bool IsOpen = false;
        private MasterScrape Scrape = new MasterScrape();
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
            this.WhenAnyValue(x => x.TitleBox.Text, x => x.MangaButton.IsChecked, x => x.NovelButton.IsChecked, x => x.PriceAnalysisVM.CurBrowser, (title, manga, novel, browser) => !string.IsNullOrWhiteSpace(title) && !(manga == false && novel == false) && !string.IsNullOrWhiteSpace(browser)).Subscribe(x => PriceAnalysisVM.IsAnalyzeButtonEnabled = x);
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
            Constants.Logger.Info($"Selected Browser = {PriceAnalysisVM.CurBrowser}");

            // Create the Website list for the scrape
            List<Website> scrapeWebsiteList = new List<Website>();
            string website;
            foreach (ListBoxItem x in PriceAnalysisVM.SelectedWebsites)
            {
                website = x.Content.ToString();
                switch (website)
                {
                    case "RightStufAnime":
                        scrapeWebsiteList.Add(Website.RightStufAnime);
                        break;
                    case "BarnesAndNoble":
                        scrapeWebsiteList.Add(Website.BarnesAndNoble);
                        break;
                    case "BooksAMillion":
                        scrapeWebsiteList.Add(Website.BooksAMillion);
                        break;
                    case "RobertsAnimeCornerStore":
                        scrapeWebsiteList.Add(Website.RobertsAnimeCornerStore);
                        break;
                    case "InStockTrades":
                        scrapeWebsiteList.Add(Website.InStockTrades);
                        break;
                    case "AmazonUSA":
                        scrapeWebsiteList.Add(Website.AmazonUSA);
                        break;
                    case "AmazonJapan":
                        scrapeWebsiteList.Add(Website.AmazonJapan);
                        break;
                    case "WorldOfBooks":
                        scrapeWebsiteList.Add(Website.WorldOfBooks);
                        break;
                }
                Constants.Logger.Info($"Added {website} to Scrape");
            }

            // Initialize Scrape & D
            Constants.Logger.Info($"Started Scrape");
            Scrape.InitializeScrape(TitleBox.Text, MangaButton.IsChecked == true ? 'M' : 'N', scrapeWebsiteList);
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
