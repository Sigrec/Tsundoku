using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class AddNewSeriesWindow : ReactiveWindow<AddNewSeriesViewModel>
    {
        private ushort MaxVolNum;
        private ushort CurVolNum;
        public bool IsOpen = false;
        private MainWindow CollectionWindow;

        public AddNewSeriesWindow()
        {
            InitializeComponent();
            Opened += (s, e) =>
            {
                IsOpen ^= true;
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
            };

            Closing += (s, e) =>
            {
                if (IsOpen)
                {
                    MainWindow.ResetMenuButton(CollectionWindow.AddNewSeriesButton);
                    ((AddNewSeriesWindow)s).Hide();
                    IsOpen ^= true;
                    Topmost = false;
                }
                e.Cancel = true;
            };

            this.WhenAnyValue(x => x.MaxVolCount.Text).Subscribe(x => MaxVolNum = ConvertNumText(x.Replace("_", "")));
            this.WhenAnyValue(x => x.CurVolCount.Text).Subscribe(x => CurVolNum = ConvertNumText(x.Replace("_", "")));
            this.WhenAnyValue(x => x.TitleBox.Text, x => x.MaxVolCount.Text, x => x.CurVolCount.Text, x => x.MangaButton.IsChecked, x => x.NovelButton.IsChecked, (title, max, cur, manga, novel) => !string.IsNullOrWhiteSpace(title) && CurVolNum <= MaxVolNum && MaxVolNum != 0 && !(manga == false && novel == false) && manga != null && novel != null).Subscribe(x => ViewModel.IsAddSeriesButtonEnabled = x);
        }


        private async Task ShowErrorDialog(string info = "Unable to Add Series")
        {
            PopupWindow errorDialog = new PopupWindow();
            errorDialog.SetWindowText("Error", "fa-solid fa-circle-exclamation", info);
            await errorDialog.ShowDialog(this);
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
            PublisherTextBox.Text = string.Empty;
            DemographicCombobox.SelectedIndex = 4;
            VolumesRead.Text = string.Empty;
            Rating.Text = string.Empty;
            ValueMaskedTextBox.Text = string.Empty;
            CoverImageUrlTextBox.Text = string.Empty;
        }

        private static ushort ConvertNumText(string value)
        {
            return (ushort)(string.IsNullOrWhiteSpace(value) ? 0 : ushort.Parse(value));
        }

        // TODO - Remove cover if series doesn't get added due to error or something
        public async void OnAddSeriesButtonClicked(object sender, RoutedEventArgs args)
        {
            AddSeriesButton.IsEnabled = false;
            ViewModelBase.newCoverCheck = true;
            string customImageUrl = CoverImageUrlTextBox.Text;
            _ = uint.TryParse(VolumesRead.Text.Replace("_", ""), out uint volumesRead);
            _ = decimal.TryParse(Rating.Text[..4].Replace("_", "0"), out decimal rating);
            _ = decimal.TryParse(ValueMaskedTextBox.Text[1..].Replace("_", "0"), out decimal seriesValue);
            
            KeyValuePair<bool, string> validSeries = await AddNewSeriesViewModel.GetSeriesDataAsync(
                TitleBox.Text.Trim(), 
                (MangaButton.IsChecked == true) ? Format.Manga : Format.Novel, 
                CurVolNum, 
                MaxVolNum, 
                AddNewSeriesViewModel.SelectedAdditionalLanguages.Count != 0 ? AddNewSeriesViewModel.ConvertSelectedLangList(AddNewSeriesViewModel.SelectedAdditionalLanguages) : [],
                !string.IsNullOrWhiteSpace(customImageUrl) ? customImageUrl.Trim() : string.Empty, 
                !string.IsNullOrWhiteSpace(PublisherTextBox.Text) ? PublisherTextBox.Text.Trim() : "Unknown", //Publisher
                Series.GetSeriesDemographic((DemographicCombobox.SelectedItem as ComboBoxItem).Content.ToString()), 
                volumesRead, 
                !Rating.Text[..4].StartsWith("__._") ? rating : -1, 
                seriesValue,
                ViewModel.AllowDuplicate
            );
            
            if (!validSeries.Key) // Boolean returns whether the series added is a duplicate
            {
                // Update User Stats
                MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateAllStats(CurVolNum, (uint)(MaxVolNum - CurVolNum));

                // Clear the fields in this window
                ClearFields();
            }
            else
            {
                await ShowErrorDialog($"Unable to add \"{TitleBox.Text.Trim()}\" to Collection{(!string.IsNullOrWhiteSpace(validSeries.Value) ? $", {validSeries.Value}" : string.Empty)}");
            }
            AddSeriesButton.IsEnabled = ViewModel.IsAddSeriesButtonEnabled;
        }

         // public void AddLangListBoxItem(string lang)
        // {
        //     AddNewSeriesViewModel.SelectedAdditionalLanguages.Remove(GetLangListBoxItem(PreviousLanguage));

        //     ListBoxItem? newItem = GetLangListBoxItem(lang);
        //     if (newItem != null)
        //     {
        //         LOGGER.Debug("Adding To Selected Lang List");
        //         AddNewSeriesViewModel.SelectedAdditionalLanguages.Add(newItem);
        //     }
        // }

        // public ListBoxItem? GetLangListBoxItem(string lang)
        // {
        //     return lang switch
        //     {
        //         "Arabic" => Arabic,
        //         "Azerbaijan" => Azerbaijan,
        //         "Bengali" => Bengali,
        //         "Bulgarian" => Bulgarian,
        //         "Burmese" => Burmese,
        //         "Catalan" => Catalan,
        //         "Chinese" => Chinese,
        //         "Croatian" => Croatian,
        //         "Czech" => Czech,
        //         "Danish" => Danish,
        //         "Dutch" => Dutch,
        //         "Esperanto" => Esperanto,
        //         "Estonian" => Estonian,
        //         "Filipino" => Filipino,
        //         "Finnish" => Finnish,
        //         "French" => French,
        //         "German" => German,
        //         "Greek" => Greek,
        //         "Hebrew" => Hebrew,
        //         "Hindi" => Hindi,
        //         "Hungarian" => Hungarian,
        //         "Indonesian" => Indonesian,
        //         "Italian" => Italian,
        //         "Kazakh" => Kazakh,
        //         "Korean" => Korean,
        //         "Latin" => Latin,
        //         "Lithuanian" => Lithuanian,
        //         "Malay" => Malay,
        //         "Mongolian" => Mongolian,
        //         "Nepali" => Nepali,
        //         "Norwegian" => Norwegian,
        //         "Persian" => Persian,
        //         "Polish" => Polish,
        //         "Portuguese" => Portuguese,
        //         "Romanian" => Romanian,
        //         "Russian" => Russian,
        //         "Serbian" => Serbian,
        //         "Slovak" => Slovak,
        //         "Spanish" => Spanish,
        //         "Swedish" => Swedish,
        //         "Tamil" => Tamil,
        //         "Thai" => Thai,
        //         "Turkish" => Turkish,
        //         "Ukrainian" => Ukrainian,
        //         "Vietnamese" => Vietnamese,
        //         _ => null,
        //     };
        // }
    }
}
