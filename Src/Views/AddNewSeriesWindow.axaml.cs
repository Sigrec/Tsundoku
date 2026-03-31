using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Tsundoku.Helpers;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;

namespace Tsundoku.Views;

public sealed partial class AddNewSeriesWindow : ReactiveWindow<AddNewSeriesViewModel>, IManagedWindow
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public bool IsOpen { get; set; }
    private readonly IPopupDialogService _popupDialogService;

    public AddNewSeriesWindow(AddNewSeriesViewModel viewModel, IPopupDialogService popupDialogService)
    {
        ViewModel = viewModel;
        _popupDialogService = popupDialogService;
        InitializeComponent();

        this.ConfigureHideOnClose(onClosing: () => viewModel.ClearSuggestions());

        Deactivated += (s, e) =>
        {
            viewModel.IsSuggestionsOpen = false;
        };

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(
                x => x.SeriesInputTextBox.Text,
                x => x.CurVolCount.Text,
                x => x.MaxVolCount.Text,
                x => x.MangaButton.IsChecked,
                x => x.NovelButton.IsChecked,
            (title, curVolText, maxVolText, mangaChecked, novelChecked) =>
            {
                // Convert the volume text to numbers
                ushort currentVolume = ConvertNumText(curVolText.Replace("_", string.Empty));
                ushort maxVolume = ConvertNumText(maxVolText.Replace("_", string.Empty));

                // Use the named variables for clarity
                bool isAnyChecked = mangaChecked.GetValueOrDefault() || novelChecked.GetValueOrDefault();

                return !string.IsNullOrWhiteSpace(title) &&
                    currentVolume <= maxVolume &&
                    maxVolume != 0 &&
                    isAnyChecked;
            })
            .Subscribe(isEnabled => ViewModel.IsAddSeriesButtonEnabled = isEnabled)
            .DisposeWith(disposables);

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
                })
                .DisposeWith(disposables);
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
        return ushort.TryParse(value, out ushort result) ? result : (ushort)1;
    }

    public async void OnAddSeriesButtonClicked(object sender, RoutedEventArgs args)
    {
        try
        {
            AddSeriesButton.IsEnabled = false;
            string customImageUrl = CoverImageUrlTextBox.Text;
            _ = uint.TryParse(VolumesRead.Text?.Replace("_", string.Empty), out uint volumesRead);

            string ratingText = Rating.Text ?? string.Empty;
            _ = decimal.TryParse(
                (ratingText.Length >= 4 ? ratingText[..4] : ratingText).Replace("_", "0"),
                out decimal rating);

            string costText = CostMaskedTextBox.Text ?? string.Empty;
            _ = decimal.TryParse(
                (costText.Length > 1 ? costText[1..] : costText).Replace("_", "0"),
                out decimal seriesValue);

            KeyValuePair<bool, string> validSeries = await ViewModel!.GetSeriesDataAsync(
                input: ViewModel.SelectedSuggestion is not null ? ViewModel.SelectedSuggestion.Id : SeriesInputTextBox.Text.Trim(),
                bookType: (MangaButton.IsChecked == true) ? SeriesFormat.Manga : SeriesFormat.Novel,
                curVolCount: ConvertNumText(CurVolCount.Text.Replace("_", string.Empty)),
                maxVolCount: ConvertNumText(MaxVolCount.Text.Replace("_", string.Empty)),
                additionalLanguages: ViewModel!.SelectedAdditionalLanguages.Count != 0 ? ViewModel.ConvertSelectedLangList() : [],
                customImageUrl: !string.IsNullOrWhiteSpace(customImageUrl) ? customImageUrl.Trim() : string.Empty,
                publisher: !string.IsNullOrWhiteSpace(PublisherTextBox.Text) ? PublisherTextBox.Text.Trim() : "Unknown",
                demographic: DemographicCombobox.SelectedItem is null ? SeriesDemographic.Unknown : (SeriesDemographic)DemographicCombobox.SelectedItem,
                volumesRead: volumesRead,
                rating: ratingText.Length >= 4 && !ratingText[..4].StartsWith("__._", StringComparison.Ordinal) ? rating : -1,
                value: seriesValue,
                allowDuplicate: AllowDuplicateButton.IsChecked.GetValueOrDefault(false)
            );

            if (validSeries.Key) // Boolean returns whether the series added succeeded
            {
                string addedTitle = !string.IsNullOrWhiteSpace(validSeries.Value) ? validSeries.Value : SeriesInputTextBox.Text.Trim();
                ClearFields();
                ViewModel.ClearSuggestions();

                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.ShowNotification($"Added \"{addedTitle}\" to Collection");
                }
            }
            else
            {
                await ShowErrorDialog($"Unable to add \"{SeriesInputTextBox.Text.Trim()}\" to Collection{(!string.IsNullOrWhiteSpace(validSeries.Value) ? $", {validSeries.Value}" : string.Empty)}");
            }
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to add series");
            await ShowErrorDialog();
        }
        finally
        {
            AddSeriesButton.IsEnabled = ViewModel!.IsAddSeriesButtonEnabled;
        }
    }
}