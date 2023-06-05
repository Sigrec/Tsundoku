using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using System.Linq;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Tsundoku.ViewModels;
using Tsundoku.Models;

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

                // Set the headers without any data
                for (int x = 0; x < 4; x++)
                {
                    DataGridTextColumn col = new DataGridTextColumn();
                    switch (x)
                    {
                        case 0:
                            col.Header = "Item";
                            break;
                        case 1:
                            col.Header = "Price";
                            break;
                        case 2:
                            col.Header = "Stock Status";
                            break;
                        case 3:
                            col.Header = "Website";
                            break;
                    }
                    col.FontSize = 18;
                    col.FontWeight = Avalonia.Media.FontWeight.Bold;
                    AnalysisDataGrid.Columns.Add(col);
                }
                AnalysisDataGrid.ItemsSource = PriceAnalysisViewModel.testData;
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
            // foreach (var x in ((Button)sender).GetLogicalSiblings())
            // {
            //     Constants.Logger.Debug(x.GetType());
            // } , ((DataGrid)((Button)sender).GetLogicalSiblings().ElementAt(8))
            AddDataPoints(PriceAnalysisViewModel.testData);
        }

        public void AddDataPoints(ObservableCollection<string[]> masterData)
        {
            for (int idx = 0; idx < 4; idx++)
            {
                ((DataGridTextColumn)AnalysisDataGrid.Columns[idx]).Binding = new Binding(string.Format($"[{idx}]"));
                ((DataGridTextColumn)AnalysisDataGrid.Columns[idx]).FontSize = 18;
            }

            AnalysisDataGrid.ItemsSource = masterData;
            AnalysisDataGrid.AutoGenerateColumns = false;
        }

        private void BrowserChanged(object sender, SelectionChangedEventArgs e)
        {
            //Constants.Logger.Debug(((sender as ComboBox).GetLogicalSiblings().ElementAt(1) as ComboBox).Name);
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
