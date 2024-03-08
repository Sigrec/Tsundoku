using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class AddNewSeriesWindow : Window
    {
        public AddNewSeriesViewModel? AddNewSeriesVM => DataContext as AddNewSeriesViewModel;
        private ushort MaxVolNum;
        private ushort CurVolNum;
        public bool IsOpen = false;

        public AddNewSeriesWindow()
        {
            InitializeComponent();
            Opened += (s, e) =>
            {
                IsOpen ^= true;
            };

            Closing += (s, e) =>
            {
                if (IsOpen)
                {
                    ((AddNewSeriesWindow)s).Hide();
                    IsOpen ^= true;
                    Topmost = false;
                }
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
            DemographicCombobox.SelectedIndex = 4;
            VolumesRead.Text = string.Empty;
            Rating.Text = string.Empty;
            Cost.Text = string.Empty;
            CoverImageUrlTextBox.Text = string.Empty;
        }

        private static ushort ConvertNumText(string value)
        {
            return (ushort)(string.IsNullOrWhiteSpace(value) ? 0 : ushort.Parse(value));
        }

        public async void OnButtonClicked(object sender, RoutedEventArgs args)
        {
            AddSeriesButton.IsEnabled = false;
            ViewModelBase.newCoverCheck = true;
            string customImageUrl = CoverImageUrlTextBox.Text;
            _ = uint.TryParse(VolumesRead.Text.Replace("_", ""), out uint volumesRead);
            _ = decimal.TryParse(Rating.Text[..4].Replace("_", "0"), out decimal rating);
            _ = decimal.TryParse(Cost.Text.Replace("_", "0"), out decimal cost);
            bool validSeries = await AddNewSeriesViewModel.GetSeriesDataAsync(TitleBox.Text.Trim(), (MangaButton.IsChecked == true) ? Format.Manga : Format.Novel, CurVolNum, MaxVolNum, AddNewSeriesViewModel.ConvertSelectedLangList(AddNewSeriesVM.SelectedAdditionalLanguages), !string.IsNullOrWhiteSpace(customImageUrl) ? customImageUrl.Trim() : string.Empty, Series.GetSeriesDemographic((DemographicCombobox.SelectedItem as ComboBoxItem).Content.ToString()), volumesRead, !Rating.Text[..4].StartsWith("__._") ? rating : -1, cost);
            if (!validSeries) // Boolean returns whether the series added is a duplicate
            {
                // Update User Stats
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateAllStats(CurVolNum, (uint)(MaxVolNum - CurVolNum));

                // Clear the fields in this window
                ClearFields();
            }
            AddSeriesButton.IsEnabled = AddNewSeriesVM.IsAddSeriesButtonEnabled;
        }
    }
}
