using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tsundoku.Models.Enums;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.SeriesFormatEnum;

namespace Tsundoku.Views;

public sealed partial class AddNewSeriesWindow : ReactiveWindow<AddNewSeriesViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private ushort MaxVolNum;
    private ushort CurVolNum;
    public bool IsOpen = false;

    public AddNewSeriesWindow(AddNewSeriesViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        Opened += (s, e) =>
        {
            IsOpen ^= true;
        };

        Closing += (s, e) =>
        {
            if (IsOpen)
            {
                this.Hide();
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
        PopupWindow errorDialog = App.ServiceProvider.GetRequiredService<PopupWindow>();
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
        CostMaskedTextBox.Text = string.Empty;
        CoverImageUrlTextBox.Text = string.Empty;
    }

    private static ushort ConvertNumText(string value)
    {
        return (ushort)(string.IsNullOrWhiteSpace(value) ? 0 : ushort.Parse(value));
    }

    public async void OnAddSeriesButtonClicked(object sender, RoutedEventArgs args)
    {
        AddSeriesButton.IsEnabled = false;
        ViewModelBase.newCoverCheck = true;
        string customImageUrl = CoverImageUrlTextBox.Text;
        _ = uint.TryParse(VolumesRead.Text.Replace("_", ""), out uint volumesRead);
        _ = decimal.TryParse(Rating.Text[..4].Replace("_", "0"), out decimal rating);
        _ = decimal.TryParse(CostMaskedTextBox.Text[1..].Replace("_", "0"), out decimal seriesValue);
        
        KeyValuePair<bool, string> validSeries = await ViewModel!.GetSeriesDataAsync(
            TitleBox.Text.Trim(), 
            (MangaButton.IsChecked == true) ? SeriesFormat.Manga : SeriesFormat.Novel, 
            CurVolNum, 
            MaxVolNum, 
            ViewModel.SelectedAdditionalLanguages.Count != 0 ? ViewModel.ConvertSelectedLangList() : [],
            !string.IsNullOrWhiteSpace(customImageUrl) ? customImageUrl.Trim() : string.Empty, 
            !string.IsNullOrWhiteSpace(PublisherTextBox.Text) ? PublisherTextBox.Text.Trim() : "Unknown",
            SeriesDemographicEnum.Parse((DemographicCombobox.SelectedItem as ComboBoxItem).Content.ToString()), 
            volumesRead, 
            !Rating.Text[..4].StartsWith("__._") ? rating : -1, 
            seriesValue,
            AllowDuplicateButton.IsChecked.GetValueOrDefault(false)
        );
        
        if (validSeries.Key) // Boolean returns whether the series added succeeded
        {
            // _collectionStatsViewModel.UpdateAllStats(CurVolNum, (uint)(MaxVolNum - CurVolNum));
            ClearFields();
        }
        else
        {
            await ShowErrorDialog($"Unable to add \"{TitleBox.Text.Trim()}\" to Collection{(!string.IsNullOrWhiteSpace(validSeries.Value) ? $", {validSeries.Value}" : string.Empty)}");
        }
        AddSeriesButton.IsEnabled = ViewModel.IsAddSeriesButtonEnabled;
    }
}