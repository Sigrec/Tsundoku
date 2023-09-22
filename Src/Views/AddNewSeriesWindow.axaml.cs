using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class AddNewSeriesWindow : Window
    {
        public AddNewSeriesViewModel? AddNewSeriesVM => DataContext as AddNewSeriesViewModel;
        private ushort MaxVolNum;
        private ushort CurVolNum;
        public bool IsOpen = false;
        MainWindow CollectionWindow;

        public AddNewSeriesWindow()
        {
            InitializeComponent();
            DataContext = new AddNewSeriesViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                AddNewSeriesVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                IsOpen ^= true;
            };

            Closing += (s, e) =>
            {
                ((AddNewSeriesWindow)s).Hide();
                ClearFields();
                IsOpen ^= true;
                Topmost = false;
                e.Cancel = true;
            };

            this.WhenAnyValue(x => x.MaxVolCount.Text).Subscribe(x => MaxVolNum = ConvertNumText(x.Replace("_", "")));
            this.WhenAnyValue(x => x.CurVolCount.Text).Subscribe(x => CurVolNum = ConvertNumText(x.Replace("_", "")));
            this.WhenAnyValue(x => x.TitleBox.Text, x => x.MaxVolCount.Text, x => x.CurVolCount.Text, x => x.MangaButton.IsChecked, x => x.NovelButton.IsChecked, (title, max, cur, manga, novel) => !string.IsNullOrWhiteSpace(title) && CurVolNum <= MaxVolNum && MaxVolNum != 0 && !(manga == false && novel == false) && manga != null && novel != null).Subscribe(x => AddNewSeriesVM.IsAddSeriesButtonEnabled = x);
        }

        private void IsMangaButtonClicked(object sender, RoutedEventArgs args)
        {
            NovelButton.IsChecked = false;
        }

        private void IsNovelButtonClicked(object sender, RoutedEventArgs args)
        {
            MangaButton.IsChecked = false;
        }

        private void ClearFields()
        {
            NovelButton.IsChecked = false;
            MangaButton.IsChecked = false;
            TitleBox.Text = string.Empty;
            CurVolCount.Text = string.Empty;
            MaxVolCount.Text = string.Empty;
        }

        private static ushort ConvertNumText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }
            return ushort.Parse(value);
        }

        public async void OnButtonClicked(object sender, RoutedEventArgs args)
        {
            bool validSeries = await AddNewSeriesViewModel.GetSeriesDataAsync(TitleBox.Text.Trim(), (MangaButton.IsChecked == true) ? "MANGA" : "NOVEL", CurVolNum, MaxVolNum, AddNewSeriesViewModel.ConvertSelectedLangList(AddNewSeriesVM.SelectedAdditionalLanguages));
            if (!validSeries) // Boolean returns whether the series added is a duplicate
            {
                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected += CurVolNum;
                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected += (uint)(MaxVolNum - CurVolNum);
                CollectionWindow.CollectionViewModel.SearchText = "";
                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.SeriesCount = (uint)MainWindowViewModel.Collection.Count;

                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateStatusChartValues();
                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateStatusPercentages();

                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicChartValues();
                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicPercentages();

                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateScoreChartValues();
                ClearFields();
            }
        }
    }
}
