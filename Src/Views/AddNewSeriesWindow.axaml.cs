using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.Services;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

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
        await _popupDialogService.ShowAsync("Error", "fa7-solid fa7-circle-exclamation", info, this);
    }

    private void IsMangaButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is ToggleButton { IsChecked: true })
        {
            NovelButton.IsChecked = false;
        }
    }

    private void IsNovelButtonClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is ToggleButton { IsChecked: true })
        {
            MangaButton.IsChecked = false;
        }
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

    private void SetButtonLoading(string text)
    {
        Optris.Icons.Avalonia.Icon spinner = new() { Value = "fa7-solid fa7-arrows-rotate", FontSize = 13 };
        Animation rotateAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(1),
            IterationCount = IterationCount.Infinite,
            Children =
            {
                new KeyFrame { Cue = new Cue(0), Setters = { new Setter(RotateTransform.AngleProperty, 0.0) } },
                new KeyFrame { Cue = new Cue(1), Setters = { new Setter(RotateTransform.AngleProperty, 360.0) } }
            }
        };
        rotateAnimation.RunAsync(spinner);

        AddSeriesButton.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { spinner, new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center } }
        };
    }

    private void ClearButtonLoading()
    {
        AddSeriesButton.Content = "Add Series";
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
                out decimal enteredCost);
            System.Globalization.CultureInfo costCulture = System.Globalization.CultureInfo.GetCultureInfo(
                Tsundoku.Models.Enums.CurrencyModel.AVAILABLE_CURRENCY_WITH_CULTURE[ViewModel.CurrentUser.Currency].Culture);
            decimal seriesValue = CurrencyValueHelper.ToBaseline(enteredCost, costCulture);

            TsundokuLanguage[] requestedLanguages = ViewModel!.SelectedAdditionalLanguages.Count != 0 ? ViewModel.ConvertSelectedLangList() : [];

            string inputDisplay = ViewModel.SelectedSuggestion?.Display ?? SeriesInputTextBox.Text.Trim();
            string truncated = inputDisplay.Length > 30 ? string.Concat(inputDisplay.AsSpan(0, 30), "...") : inputDisplay;
            SetButtonLoading($"Loading \"{truncated}\"");

            (bool success, string message, Series? addedSeries) = await ViewModel!.GetSeriesDataAsync(
                input: ViewModel.SelectedSuggestion is not null ? ViewModel.SelectedSuggestion.Id : SeriesInputTextBox.Text.Trim(),
                bookType: (MangaButton.IsChecked == true) ? SeriesFormat.Manga : SeriesFormat.Novel,
                curVolCount: ConvertNumText(CurVolCount.Text.Replace("_", string.Empty)),
                maxVolCount: ConvertNumText(MaxVolCount.Text.Replace("_", string.Empty)),
                additionalLanguages: requestedLanguages,
                customImageUrl: !string.IsNullOrWhiteSpace(customImageUrl) ? customImageUrl.Trim() : string.Empty,
                publisher: !string.IsNullOrWhiteSpace(PublisherTextBox.Text) ? PublisherTextBox.Text.Trim() : "Unknown",
                demographic: DemographicCombobox.SelectedItem is null ? SeriesDemographic.Unknown : (SeriesDemographic)DemographicCombobox.SelectedItem,
                volumesRead: volumesRead,
                rating: ratingText.Length >= 4 && !ratingText[..4].StartsWith("__._", StringComparison.Ordinal) ? rating : -1,
                value: seriesValue,
                allowDuplicate: AllowDuplicateButton.IsChecked.GetValueOrDefault(false)
            );

            if (success)
            {
                string addedTitle = addedSeries is not null
                    ? addedSeries.Titles.GetValueOrDefault(ViewModel.CurrentUser.Language)
                        ?? addedSeries.Titles.GetValueOrDefault(TsundokuLanguage.Romaji, SeriesInputTextBox.Text.Trim())
                    : SeriesInputTextBox.Text.Trim();
                ClearFields();
                ViewModel.ClearSuggestions();

                if (addedSeries is not null && requestedLanguages.Length > 0)
                {
                    await PromptForMissingLanguagesAsync(addedSeries, requestedLanguages);
                }

                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.ShowNotification($"Added \"{addedTitle}\" to Collection");
                }
            }
            else
            {
                await ShowErrorDialog($"Unable to add \"{SeriesInputTextBox.Text.Trim()}\" to Collection{(!string.IsNullOrWhiteSpace(message) ? $", {message}" : string.Empty)}");
            }
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to add series");
            await ShowErrorDialog();
        }
        finally
        {
            ClearButtonLoading();
            AddSeriesButton.IsEnabled = ViewModel!.IsAddSeriesButtonEnabled;
        }
    }

    private async Task PromptForMissingLanguagesAsync(Series addedSeries, TsundokuLanguage[] requestedLanguages)
    {
        TsundokuLanguage[] missingLanguages = requestedLanguages
            .AsValueEnumerable()
            .Where(lang => !addedSeries.Titles.ContainsKey(lang))
            .ToArray();

        if (missingLanguages.Length == 0)
        {
            return;
        }

        string romajiTitle = addedSeries.Titles.GetValueOrDefault(TsundokuLanguage.Romaji, "this series");
        foreach (TsundokuLanguage lang in missingLanguages)
        {
            string? enteredTitle = await _popupDialogService.InputAsync(
                "Missing Title",
                "fa7-solid fa7-language",
                $"MangaDex doesn't have a {lang} title for \"{romajiTitle}\".\nEnter it manually or skip.",
                this
            );

            if (!string.IsNullOrWhiteSpace(enteredTitle))
            {
                addedSeries.Titles[lang] = enteredTitle.Trim();
                LOGGER.Info("Manually added {Lang} title \"{Title}\" for {Series}", lang, enteredTitle.Trim(), romajiTitle);
            }
        }
    }
}