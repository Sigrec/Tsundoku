using System.Reactive.Linq;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;

namespace Tsundoku.Views;

public sealed partial class AddNewSeriesWindow : ReactiveWindow<AddNewSeriesViewModel>
{
    public bool IsOpen = false;
    private readonly IPopupDialogService _popupDialogService;

    public AddNewSeriesWindow(AddNewSeriesViewModel viewModel, IPopupDialogService popupDialogService)
    {
        ViewModel = viewModel;
        _popupDialogService = popupDialogService;
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
                viewModel.ClearSuggestions();
            }
            e.Cancel = true;
        };

        this.WhenAnyValue(
            x => x.SeriesInputTextBox.Text,
            x => x.CurVolCount.Text,
            x => x.MaxVolCount.Text,
            x => x.MangaButton.IsChecked,
            x => x.NovelButton.IsChecked,
        (title, curVolText, maxVolText, mangaChecked, novelChecked) =>
        {
            // Convert the volume text to numbers
            ushort currentVolume = ConvertNumText(curVolText.Replace("_", ""));
            ushort maxVolume = ConvertNumText(maxVolText.Replace("_", ""));

            // Use the named variables for clarity
            bool isAnyChecked = mangaChecked.GetValueOrDefault() || novelChecked.GetValueOrDefault();

            return !string.IsNullOrWhiteSpace(title) &&
                currentVolume <= maxVolume &&
                maxVolume != 0 &&
                isAnyChecked;
        })
        .Subscribe(isEnabled => ViewModel.IsAddSeriesButtonEnabled = isEnabled);

        this.WhenAnyValue(x => x.ViewModel.SelectedSuggestion)
            .Subscribe(selectedSuggestion =>
            {
                if (selectedSuggestion is not null)
                {
                    if (selectedSuggestion.Format.Equals("NOVEL", StringComparison.OrdinalIgnoreCase))
                    {
                        NovelButton.IsChecked = true;
                        MangaButton.IsChecked = false;
                    }
                    else if (selectedSuggestion.Format.Equals("MANGA", StringComparison.OrdinalIgnoreCase))
                    {
                        NovelButton.IsChecked = false;
                        MangaButton.IsChecked = true;
                    }
                    else
                    {
                        NovelButton.IsChecked = false;
                        MangaButton.IsChecked = false;
                    }
                }
                else
                {
                    NovelButton.IsChecked = false;
                    MangaButton.IsChecked = false;
                }
            });
    }


    private async Task ShowErrorDialog(string info = "Unable to Add Series")
    {
        await _popupDialogService.ShowAsync("Error", "fa-solid fa-circle-exclamation", info, this);
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
        SeriesInputTextBox.Text = string.Empty;
        CurVolCount.Text = string.Empty;
        MaxVolCount.Text = string.Empty;
        PublisherTextBox.Text = string.Empty;
        DemographicCombobox.SelectedItem = SeriesDemographic.Unknown;
        VolumesRead.Text = string.Empty;
        Rating.Text = string.Empty;
        CostMaskedTextBox.Text = string.Empty;
        CoverImageUrlTextBox.Text = string.Empty;
    }

    private static ushort ConvertNumText(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? (ushort)1 : ushort.Parse(value);
    }

    public async void OnAddSeriesButtonClicked(object sender, RoutedEventArgs args)
    {
        AddSeriesButton.IsEnabled = false;
        string customImageUrl = CoverImageUrlTextBox.Text;
        _ = uint.TryParse(VolumesRead.Text.Replace("_", ""), out uint volumesRead);
        _ = decimal.TryParse(Rating.Text[..4].Replace("_", "0"), out decimal rating);
        _ = decimal.TryParse(CostMaskedTextBox.Text[1..].Replace("_", "0"), out decimal seriesValue);

        KeyValuePair<bool, string> validSeries = await ViewModel!.GetSeriesDataAsync(
            input: ViewModel.SelectedSuggestion is not null ? ViewModel.SelectedSuggestion.Id : SeriesInputTextBox.Text.Trim(),
            bookType: (MangaButton.IsChecked == true) ? SeriesFormat.Manga : SeriesFormat.Novel,
            curVolCount: ConvertNumText(CurVolCount.Text.Replace("_", "")),
            maxVolCount: ConvertNumText(MaxVolCount.Text.Replace("_", "")),
            additionalLanguages: ViewModel!.SelectedAdditionalLanguages.Count != 0 ? ViewModel.ConvertSelectedLangList() : [],
            customImageUrl: !string.IsNullOrWhiteSpace(customImageUrl) ? customImageUrl.Trim() : string.Empty,
            publisher: !string.IsNullOrWhiteSpace(PublisherTextBox.Text) ? PublisherTextBox.Text.Trim() : "Unknown",
            demographic: DemographicCombobox.SelectedItem is null ? SeriesDemographic.Unknown : (SeriesDemographic)DemographicCombobox.SelectedItem,
            volumesRead: volumesRead,
            rating: !Rating.Text[..4].StartsWith("__._") ? rating : -1,
            value: seriesValue,
            allowDuplicate: AllowDuplicateButton.IsChecked.GetValueOrDefault(false)
        );

        if (validSeries.Key) // Boolean returns whether the series added succeeded
        {
            ClearFields();
            ViewModel.ClearSuggestions();
        }
        else
        {
            await ShowErrorDialog($"Unable to add \"{SeriesInputTextBox.Text.Trim()}\" to Collection{(!string.IsNullOrWhiteSpace(validSeries.Value) ? $", {validSeries.Value}" : string.Empty)}");
        }
        AddSeriesButton.IsEnabled = ViewModel.IsAddSeriesButtonEnabled;
    }
}