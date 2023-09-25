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
using Microsoft.IdentityModel.Tokens;
using static Src.Models.Constants;
using System.Linq;
using MangaLightNovelWebScrape.Websites;

namespace Tsundoku.Views
{
    public partial class PriceAnalysisWindow : Window
    {
        public PriceAnalysisViewModel? PriceAnalysisVM => DataContext as PriceAnalysisViewModel;
        public bool IsOpen = false, Manga;
        public readonly MasterScrape Scrape = new MasterScrape().DisableDebugMode();
        private string ScrapeTitle;
        private readonly string[] EXCLUDE_NONE_FILTER = [];
        private readonly string[] EXCLUDE_BOTH_FILTER = ["PO", "OOS"];
        private readonly string[] EXCLUDE_PO_FILTER = ["PO"];
        private readonly string[] EXCLUDE_OOS_FILTER = ["OOS"];
        MainWindow CollectionWindow;

        public PriceAnalysisWindow()
        {
            InitializeComponent();
            DataContext = new PriceAnalysisViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                PriceAnalysisVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                IsOpen ^= true;
                if (Screens.Primary.WorkingArea.Height < 611)
                {
                    this.Height = 500;
                }
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
                ScrapeTitle = TitleBox.Text;

                LOGGER.Debug($"Memberships = {string.Join(" | ", ViewModelBase.MainUser.Memberships)}");
                LOGGER.Debug($"Websites = [{string.Join(", ", PriceAnalysisVM.SelectedWebsites.Select(site => site.Content.ToString()))}]");

                StartScrapeButton.IsEnabled = false;
                LOGGER.Info($"Started Scrape For {TitleBox.Text} on {PriceAnalysisVM.CurBrowser} Browser w/ {(StockFilterSelector.SelectedItem as ComboBoxItem).Content} Filter");
                await Scrape.InitializeScrapeAsync(
                        TitleBox.Text, 
                        MangaButton.IsChecked != null && MangaButton.IsChecked.Value ? Book.Manga : Book.LightNovel, 
                        (StockFilterSelector.SelectedItem as ComboBoxItem).Content.ToString() switch
                        {
                            "Exclude PO & OOS" => EXCLUDE_BOTH_FILTER,
                            "Exclude PO" => EXCLUDE_PO_FILTER,
                            "Exclude OOS" => EXCLUDE_OOS_FILTER,
                            _ => EXCLUDE_NONE_FILTER
                        }, 
                        PriceAnalysisVM.SelectedWebsites.Select(site => site.Content.ToString()).ToList(), 
                        PriceAnalysisVM.CurBrowser, 
                        ViewModelBase.MainUser.Memberships[RightStufAnime.WEBSITE_TITLE], 
                        ViewModelBase.MainUser.Memberships[BarnesAndNoble.WEBSITE_TITLE], 
                        ViewModelBase.MainUser.Memberships[BooksAMillion.WEBSITE_TITLE], 
                        ViewModelBase.MainUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE],
                        false
                    );
                StartScrapeButton.IsEnabled = PriceAnalysisVM.IsAnalyzeButtonEnabled;
                LOGGER.Info($"Scrape Finished");

                PriceAnalysisVM.AnalyzedList.Clear();
                PriceAnalysisVM.AnalyzedList.AddRange(Scrape.GetResults());
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
