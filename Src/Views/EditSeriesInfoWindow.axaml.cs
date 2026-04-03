using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Projektanker.Icons.Avalonia;
using ReactiveUI.Avalonia;
using System.Reactive.Linq;
using Tsundoku.Helpers;
using Tsundoku.Services;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Views;

public sealed partial class EditSeriesInfoWindow : ReactiveWindow<EditSeriesInfoViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly BitmapHelper _bitmapHelper;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly IApiHealthCheckService _apiHealthCheckService;
    private readonly IPopupDialogService _popupDialogService;
    private bool _IsInitialized = false;

    public EditSeriesInfoWindow(MainWindowViewModel mainWindowViewModel, BitmapHelper bitmapHelper, IApiHealthCheckService apiHealthCheckService, IPopupDialogService popupDialogService)
    {
        _bitmapHelper = bitmapHelper;
        _mainWindowViewModel = mainWindowViewModel;
        _apiHealthCheckService = apiHealthCheckService;
        _popupDialogService = popupDialogService;
        InitializeComponent();

        Opened += (s, e) =>
        {
            string curTitle = ViewModel.Series.Titles.TryGetValue(ViewModel.CurrentUser.Language, out string? title)
                ? title
                : ViewModel.Series.Titles[TsundokuLanguage.Romaji];

            DiscordRP.SetPresence(
                state: $"Editing {curTitle} {ViewModel.Series.Format}",
                refreshTimestamp: true,
                additionalButton: new DiscordRPC.Button { Label = "AniList", Url = ViewModel.Series.Link.ToString() });
                
            this.Title = $"{curTitle}";

            UpdateSelectedGenres();
            _IsInitialized = true;

            // Disable refresh if AniList is down
            System.ObservableExtensions.Subscribe(
                _apiHealthCheckService.IsAniListAvailable.ObserveOn(AvaloniaScheduler.Instance),
                isAvailable => ChangeSeriesVolumeCountButton.IsEnabled = isAvailable);
        };

        Closed += (s, e) =>
        {
            DiscordRP.SetPresence(refreshTimestamp: true);
        };
    }

    private void GenerateNewBitmap(Bitmap newCover, string path)
    {
        if (newCover is not null)
        {
            if (!ViewModel.Series.Cover.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                ViewModel.Series.DeleteCover();
                ViewModel.Series.Cover = Path.ChangeExtension(ViewModel.Series.Cover, ".png");
            }

            _mainWindowViewModel.ChangeCover(ViewModel.Series, newCover);
            LOGGER.Info("Changed Cover for {RomajiTitle}", ViewModel.Series.Titles[TsundokuLanguage.Romaji]);
        }
        else
        {
            LOGGER.Warn("{Path} is not a valid Image URL", path);
        }
    }

    private async void ChangeCoverFromLinkAsync(object sender, RoutedEventArgs args)
    {
        string customImageUrl = CoverImageUrlTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(customImageUrl))
        {
            await _popupDialogService.ShowAsync("No URL", "fa-solid fa-circle-info", "No image URL provided.", this);
            return;
        }

        ChangeCoverButtonIcon.Value = "fa-solid fa-arrow-rotate-right";
        ChangeCoverButtonIcon.Animation = IconAnimation.Spin;
        try
        {
            string fullCoverPath = AppFileHelper.GetFullCoverPath(ViewModel.Series.Cover);
            Bitmap? newCover = await _bitmapHelper.UpdateCoverFromUrlAsync(customImageUrl, fullCoverPath);

            if (newCover is not null)
            {
                GenerateNewBitmap(newCover, customImageUrl);
                CoverImageUrlTextBox.Clear();
                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.ShowNotification("Cover image updated");
                }
            }
            else
            {
                await _popupDialogService.ShowAsync("Error", "fa-solid fa-circle-exclamation", "Failed to load cover image from the provided URL.", this);
            }
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to change cover from link");
            await _popupDialogService.ShowAsync("Error", "fa-solid fa-circle-exclamation", "Failed to change cover image.", this);
        }
        finally
        {
            ChangeCoverButtonIcon.Animation = IconAnimation.None;
            ChangeCoverButtonIcon.Value = "fa-solid fa-circle-down";
        }
    }

    private async void ChangeSeriesCoverFromFileAsync(object sender, RoutedEventArgs args)
    {
        IReadOnlyList<IStorageFile> file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg"] }]
        });

        if (file.Count == 1)
        {
            string fullCoverPath = AppFileHelper.GetFullCoverPath(ViewModel.Series.Cover);
            Bitmap? newCover = BitmapHelper.UpdateCoverFromFilePath(file[0].Path.LocalPath, fullCoverPath);
            if (newCover is not null)
            {
                GenerateNewBitmap(newCover, fullCoverPath);
            }
        }
        else
        {
            LOGGER.Warn("User selected multiple files for user icon");
        }
    }
    
    private void SaveStats(object sender, RoutedEventArgs args)
    {
        ChangeSeriesVolumeCounts();
        ChangeSeriesVolumesRead();
        ChangeSeriesRating();
        ChangeSeriesValue();
        ChangeSeriesPublisher();
    }

    private void ChangeSeriesVolumesRead()
    {
        string volumesReadText = VolumesReadMaskedTextBox.Text.Replace("_", string.Empty);
        if (!string.IsNullOrWhiteSpace(volumesReadText) &&
            uint.TryParse(volumesReadText, out uint newVolumesRead) &&
            ViewModel.Series.VolumesRead != newVolumesRead)
        {
            uint oldVolumesRead = ViewModel.Series.VolumesRead;
            ViewModel.Series.VolumesRead = newVolumesRead;
            VolumesReadMaskedTextBox.Clear();
            LOGGER.Info("Updated Volumes Read for {Title} from {Old} to {New}", ViewModel.Series.Titles[TsundokuLanguage.Romaji], oldVolumesRead, newVolumesRead);
        }
    }

    private void ChangeSeriesRating()
    {
        if (RatingMaskedTextBox.Text.StartsWith("__._", StringComparison.Ordinal)) return;

        string rawInput = RatingMaskedTextBox.Text[..4].Trim().Replace("_", "0");
        if (decimal.TryParse(rawInput, out decimal ratingVal) && ratingVal <= 10.0m && ratingVal != ViewModel.Series.Rating)
        {
            ratingVal = decimal.Round(ratingVal, 1);
            ViewModel.Series.Rating = ratingVal;
            RatingMaskedTextBox.Clear();
            LOGGER.Info("Updated Rating for {Title} to {Rating}/10.0", ViewModel.Series.Titles[TsundokuLanguage.Romaji], ratingVal);
        }
    }

    private void ChangeSeriesValue()
    {
        string valueTextRaw = ValueMaskedTextBox.Text;
        if (string.IsNullOrWhiteSpace(valueTextRaw) || !valueTextRaw.Any(char.IsDigit)) return;

        string valueText = valueTextRaw[1..].Replace("_", "0");
        if (!valueText.Contains('_') && decimal.TryParse(valueText, out decimal newValue) &&
            decimal.Compare(ViewModel.Series.Value, newValue) != 0)
        {
            decimal oldValue = ViewModel.Series.Value;
            ViewModel.Series.Value = newValue;
            ValueMaskedTextBox.Clear();
            LOGGER.Info("Updated Value for {Title} from {Old} to {New}", ViewModel.Series.Titles[TsundokuLanguage.Romaji], oldValue, newValue);
        }
    }

    private void ChangeSeriesPublisher()
    {
        string publisherText = PublisherTextBox.Text.Trim();
        if (!string.IsNullOrEmpty(publisherText) && !publisherText.Equals(ViewModel.Series.Publisher, StringComparison.Ordinal))
        {
            string oldPublisher = ViewModel.Series.Publisher;
            ViewModel.Series.Publisher = publisherText;
            PublisherTextBox.Clear();
            LOGGER.Info("Updated Publisher for {Title} from {Old} to {New}", ViewModel.Series.Titles[TsundokuLanguage.Romaji], oldPublisher, publisherText);
        }
    }

    private void GenreSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_IsInitialized || (e.AddedItems.Count == 0 && e.RemovedItems.Count == 0))
        {
            return;
        }

        HashSet<SeriesGenre> curGenres = ViewModel!.GetCurrentGenresSelected();

        LOGGER.Info(
            "Updating Genres for {RomajiTitle} from [{OldGenres}] to [{NewGenres}]",
            ViewModel.Series.Titles[TsundokuLanguage.Romaji],
            string.Join(", ", ViewModel.Series.Genres),
            string.Join(", ", curGenres)
        );

        ViewModel.Series.Genres = curGenres;
    }

    /// <summary>
    /// Changes the chosen demographic for a particular series
    /// </summary>
    private void DemographicChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_IsInitialized && DemographicComboBox.IsDropDownOpen)
        {
            SeriesDemographic newDemographic = DemographicComboBox.SelectedItem is null ? SeriesDemographic.Unknown : (SeriesDemographic)DemographicComboBox.SelectedItem;
            ViewModel.Series.Demographic = newDemographic;

            LOGGER.Info(
                "Changed Demographic for {RomajiTitle} to {Demographic}",
                ViewModel.Series.Titles[TsundokuLanguage.Romaji],
                newDemographic
            );
        }
    }

    private async void RefreshSeriesAsync(object sender, RoutedEventArgs args)
    {
        if (sender is Button btn)
        {
            btn.IsEnabled = false;
        }
        try
        {
            await _mainWindowViewModel.RefreshSeries(ViewModel.Series);
            if (Owner is MainWindow mainWindow)
            {
                mainWindow.ShowNotification($"Refreshed \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\"");
            }
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to refresh series {Title}", ViewModel.Series.Titles[TsundokuLanguage.Romaji]);
            await _popupDialogService.ShowAsync("Error", "fa-solid fa-circle-exclamation", $"Failed to refresh series.", this);
        }
        finally
        {
            if (sender is Button btn2)
            {
                btn2.IsEnabled = true;
            }
        }
    }

    private async void RemoveSeries(object sender, RoutedEventArgs args)
    {
        string title = ViewModel.Series.Titles.TryGetValue(TsundokuLanguage.Romaji, out string? t) ? t : "this series";
        bool confirmed = await _popupDialogService.ConfirmAsync(
            "Delete Series",
            "fa-solid fa-triangle-exclamation",
            $"Are you sure you want to delete \"{title}\" from your collection? This cannot be undone.",
            this);

        if (confirmed)
        {
            _mainWindowViewModel.DeleteSeries(ViewModel.Series);
            this.Close();
        }
    }

    private void ChangeSeriesVolumeCounts()
    {
        string curText = CurVolumeMaskedTextBox.Text.Replace("_", string.Empty).Trim();
        string maxText = MaxVolumeMaskedTextBox.Text.Replace("_", string.Empty).Trim();

        // Try parsing each non‐empty field:
        ushort newCur = 0, newMax = 0;
        bool hasCur = !string.IsNullOrWhiteSpace(curText) && ushort.TryParse(curText, out newCur);
        bool hasMax = !string.IsNullOrWhiteSpace(maxText) && ushort.TryParse(maxText, out newMax);

        // If either field was non‐empty but failed to parse, log a warning and stop:
        if (!string.IsNullOrWhiteSpace(curText) && !hasCur)
        {
            LOGGER.Warn("{CurText} is not a valid ushort input for current volume", curText);
            return;
        }
        if (!string.IsNullOrWhiteSpace(maxText) && !hasMax)
        {
            LOGGER.Warn("{MaxText} is not a valid ushort input for max volume", maxText);
            return;
        }

        // Both were provided:
        if (hasCur && hasMax)
        {
            ViewModel.Series.UpdateVolumeCounts(newCur, newMax, () =>
            {
                CurVolumeMaskedTextBox.Clear();
                MaxVolumeMaskedTextBox.Clear();
            });
        }
        else if (hasCur) // Only “current” was provided:
        {
            ViewModel.Series.UpdateCurVolumeCount(newCur, CurVolumeMaskedTextBox.Clear);
        }
        // Only “max” was provided:
        else if (hasMax)
        {
            ViewModel.Series.UpdateMaxVolumeCount(newMax, MaxVolumeMaskedTextBox.Clear);
        }
    }

    private void UpdateSelectedGenres()
    {
        if (ViewModel.Series.Genres is null)
        {
            return;
        }

        foreach (SeriesGenre genre in ViewModel.Series.Genres.AsValueEnumerable().OrderBy(g => g.ToString()))
        {
            GenreSelector.SelectedItems.Add(GetPrimaryAlias(genre));
        }
    }
}