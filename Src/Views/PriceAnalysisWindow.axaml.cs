using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Tsundoku.Models;
using System.Diagnostics.CodeAnalysis;

namespace Tsundoku.Views
{
    public partial class PriceAnalysisWindow : Window
    {
        public PriceAnalysisViewModel? PriceAnalysisVM => DataContext as PriceAnalysisViewModel;
        public bool IsOpen = false;
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
            };

            Closing += (s, e) =>
            {
                ((PriceAnalysisWindow)s).Hide();
                Topmost = false;
                IsOpen ^= true;
                e.Cancel = true;
            };
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
            Constants.Logger.Debug("Started Scrape");
        }

        private void BrowserChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                switch (BrowserSelector.SelectedItem)
                {
                    case "Edge":
                        PriceAnalysisVM.CurBrowser = "Edge";
                        break;
                    case "Chrome":
                        PriceAnalysisVM.CurBrowser = "Chrome";
                        break;
                    case "FireFox":
                        PriceAnalysisVM.CurBrowser = "Firefox";
                        break;
                }
                Constants.Logger.Info($"Browser Langauge to {BrowserSelector.SelectedItem}");
            }
        }
    }
}
