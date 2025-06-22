using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using System.Collections.Frozen;
using Tsundoku.Helpers;
using Tsundoku.Models.Enums;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.SeriesDemographicEnum;
using static Tsundoku.Models.Enums.SeriesGenreEnum;
using static Tsundoku.Models.TsundokuLanguageModel;

namespace Tsundoku.Views;

public sealed partial class EditSeriesInfoWindow : ReactiveWindow<EditSeriesInfoViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly BitmapHelper _bitmapHelper;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private bool _IsInitialized = false;
    private readonly FrozenDictionary<SeriesGenre, object> GenreItemMap;

    public EditSeriesInfoWindow(MainWindowViewModel mainWindowViewModel, BitmapHelper bitmapHelper)
    {
        _bitmapHelper = bitmapHelper;
        _mainWindowViewModel = mainWindowViewModel;
        InitializeComponent();

        GenreItemMap = new Dictionary<SeriesGenre, object>
        {
            [SeriesGenre.Action] = ActionListBoxItem,
            [SeriesGenre.Adventure] = AdventureListBoxItem,
            [SeriesGenre.Comedy] = ComedyListBoxItem,
            [SeriesGenre.Drama] = DramaListBoxItem,
            [SeriesGenre.Ecchi] = EcchiListBoxItem,
            [SeriesGenre.Fantasy] = FantasyListBoxItem,
            [SeriesGenre.Horror] = HorrorListBoxItem,
            [SeriesGenre.MahouShoujo] = MahouShoujoListBoxItem,
            [SeriesGenre.Mecha] = MechaListBoxItem,
            [SeriesGenre.Music] = MusicListBoxItem,
            [SeriesGenre.Mystery] = MysteryListBoxItem,
            [SeriesGenre.Psychological] = PsychologicalListBoxItem,
            [SeriesGenre.Romance] = RomanceListBoxItem,
            [SeriesGenre.SciFi] = SciFiListBoxItem,
            [SeriesGenre.SliceOfLife] = SliceOfLifeListBoxItem,
            [SeriesGenre.Sports] = SportsListBoxItem,
            [SeriesGenre.Supernatural] = SupernaturalListBoxItem,
            [SeriesGenre.Thriller] = ThrillerListBoxItem
        }.ToFrozenDictionary();

        Opened += (s, e) =>
        {
            string curTitle = ViewModel.Series.Titles[ViewModel.CurrentUser.Language];
            DiscordRP.SetPresence(state: $"Editing {curTitle} {ViewModel.Series.Format}", refreshTimestamp: true);
            this.Title = $"{curTitle}";
            VolumesReadTextBlock.Text = $"{ViewModel.Series.VolumesRead} Vol{(ViewModel.Series.VolumesRead > 1 ? "s" : string.Empty)} Read";
            UpdateSelectedGenres();
            _IsInitialized = true;
        };

        Closed += (s, e) =>
        {
            DiscordRP.SetPresence(refreshTimestamp: true);
        };
    }

    private void GenerateNewBitmap(Bitmap newCover, string path)
    {
        if (newCover != null)
        {
            if (!ViewModel.Series.Cover.EndsWith(".png"))
            {
                ViewModel.Series.DeleteCover();
                ViewModel.Series.Cover = Path.ChangeExtension(ViewModel.Series.Cover, ".png");
            }

            _mainWindowViewModel.ChangeCover(ViewModel.Series, newCover);
            LOGGER.Info($"Changed Cover for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\"");
        }
        else
        {
            LOGGER.Warn($"\"{path}\" is not a valid Image Url");
        }
    }

    private async void ChangeCoverFromLinkAsync(object sender, RoutedEventArgs args)
    {
        ChangeCoverButtonIcon.Value = "fa-solid fa-arrow-rotate-right";
        ChangeCoverButtonIcon.Animation = IconAnimation.Spin;
        string customImageUrl = CoverImageUrlTextBox.Text.Trim();
        string fullCoverPath = AppFileHelper.GetFullCoverPath(ViewModel.Series.Cover);
        Bitmap? newCover = await _bitmapHelper.UpdateCoverFromUrlAsync(customImageUrl, fullCoverPath);

        if (newCover != null)
        {
            GenerateNewBitmap(newCover, customImageUrl);
            CoverImageUrlTextBox.Clear();
        }

        ChangeCoverButtonIcon.Animation = IconAnimation.None;
        ChangeCoverButtonIcon.Value = "fa-solid fa-circle-down";
    }

    private async void ChangeSeriesCoverFromFileAsync(object sender, RoutedEventArgs args)
    {
        ViewModelBase.newCoverCheck = true;
        IReadOnlyList<IStorageFile> file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg"] }]
        });

        if (file.Count == 1)
        {
            string fullCoverPath = AppFileHelper.GetFullCoverPath(ViewModel.Series.Cover);
            Bitmap? newCover = BitmapHelper.UpdateCoverFromFilePath(file[0].Path.LocalPath, fullCoverPath);
            if (newCover != null)
            {
                GenerateNewBitmap(newCover, fullCoverPath);
            }
        }
        else
        {
            LOGGER.Warn("User selected multiple files for user icon");
        }
    }
    
    /// <summary>
    /// Saves the stats for the series when the button is clicked
    /// </summary>
    private void SaveStats(object sender, RoutedEventArgs args)
    {
        string volumesReadText = VolumesReadMaskedTextBox.Text.Replace("_", string.Empty);
        if (!string.IsNullOrWhiteSpace(volumesReadText) &&
            uint.TryParse(volumesReadText, out uint newVolumesRead) &&
            ViewModel.Series.VolumesRead != newVolumesRead)
        {
            uint oldVolumesRead = ViewModel.Series.VolumesRead;

            ViewModel.Series.VolumesRead = newVolumesRead;
            VolumesReadTextBlock.Text = $"{newVolumesRead} Vol{(newVolumesRead == 1 ? string.Empty : "s")} Read";
            VolumesReadMaskedTextBox.Clear();

            LOGGER.Info($"Updated # of Volumes Read for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" from {oldVolumesRead} to {newVolumesRead}");
        }
        else if (!string.IsNullOrWhiteSpace(volumesReadText))
        {
            LOGGER.Warn($"Volumes Read Input \"{volumesReadText}\" is invalid or unchanged.");
        }

        if (!RatingMaskedTextBox.Text.StartsWith("__._"))
        {
            string rawInput = RatingMaskedTextBox.Text[..4].Trim().Replace("_", "0");
            if (decimal.TryParse(rawInput, out decimal ratingVal))
            {
                // Clamp to max value and avoid unnecessary assignment
                if (ratingVal <= 10.0m && ratingVal != ViewModel.Series.Rating)
                {
                    ViewModel.Series.Rating = ratingVal;
                    RatingTextBlock.Text = $"Rating {decimal.Round(ratingVal, 1)}/10.0";
                    RatingMaskedTextBox.Clear();

                    LOGGER.Info($"Updating rating for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" {(ViewModel.Series.Rating == -1 ? string.Empty : $"from \"{ViewModel.Series.Rating}/10.0\" ")} to \"{decimal.Round(ratingVal, 1)}/10.0\"");
                }
                else
                {
                    LOGGER.Warn($"Rating Value {ratingVal} is larger than 10.0");
                }
            }
            else
            {
                LOGGER.Warn($"Invalid Rating Input: {rawInput}");
            }
        }

        string costTextRaw = CostMaskedTextBox.Text;
        if (!costTextRaw.EndsWith("__________________.__") && !string.IsNullOrWhiteSpace(costTextRaw))
        {
            string costText = costTextRaw[1..].Replace("_", "0");
            if (!costText.Contains('_') && decimal.TryParse(costText, out decimal newValue) &&
                decimal.Compare(ViewModel.Series.Value, newValue) != 0)
            {
                string currency = ViewModel.CurrentUser.Currency;
                decimal oldValue = ViewModel.Series.Value;

                ViewModel.Series.Value = newValue;
                CostMaskedTextBox.Clear();

                LOGGER.Info($"Updated Cost for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" from {currency}{oldValue} to {currency}{newValue}");
            }
            else
            {
                LOGGER.Warn($"Invalid or unchanged cost input: \"{costTextRaw}\"");
            }
        }

        string publisherText = PublisherTextBox.Text.Trim();
        if (!string.IsNullOrEmpty(publisherText) && !publisherText.Equals(ViewModel.Series.Publisher, StringComparison.Ordinal))
        {
            string oldPublisher = ViewModel.Series.Publisher;
            ViewModel.Series.Publisher = publisherText;
            PublisherTextBox.Clear();

            LOGGER.Info($"Updated Publisher for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" from \"{oldPublisher}\" to \"{publisherText}\"");
        }
    }

    private void GenreSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_IsInitialized || (e.AddedItems.Count == 0 && e.RemovedItems.Count == 0))
            return;

        HashSet<SeriesGenre> curGenres = ViewModel!.GetCurrentGenresSelected();
        LOGGER.Info($"Updating Genres for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" from [{string.Join(", ", ViewModel.Series.Genres)}] to [{string.Join(", ", curGenres)}]");
        ViewModel.Series.Genres = curGenres;
    }

    /// <summary>
    /// Changes the chosen demographic for a particular series
    /// </summary>
    private void DemographicChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_IsInitialized && DemographicComboBox.IsDropDownOpen)
        {
            SeriesDemographic demographic = SeriesDemographicEnum.Parse((DemographicComboBox.SelectedItem as ComboBoxItem).Content.ToString());
            ViewModel.Series.Demographic = demographic;
            LOGGER.Info($"Changed Demographic for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" to {demographic}");
        }
    }


    private async void RefreshSeriesAsync(object sender, RoutedEventArgs args)
    {
        await _mainWindowViewModel.RefreshSeries(ViewModel.Series);
    }

    private void RemoveSeries(object sender, RoutedEventArgs args)
    {
        _mainWindowViewModel.DeleteSeries(ViewModel.Series);
        this.Close();
    }

    private void ChangeSeriesVolumeCounts(object sender, RoutedEventArgs args)
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
            LOGGER.Warn($"\"{curText}\" is not a valid ushort input for current volume");
            return;
        }
        if (!string.IsNullOrWhiteSpace(maxText) && !hasMax)
        {
            LOGGER.Warn($"\"{maxText}\" is not a valid ushort input for max volume");
            return;
        }

        // Both were provided:
        if (hasCur && hasMax)
        {
            if (newMax >= newCur)
            {
                uint oldCur = ViewModel.Series.CurVolumeCount;
                uint oldMax = ViewModel.Series.MaxVolumeCount;
                ViewModel.Series.CurVolumeCount = newCur;
                ViewModel.Series.MaxVolumeCount = newMax;

                LOGGER.Info(
                    $"Changed Series Volume Counts For \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" " +
                    $"From {oldCur}/{oldMax} -> {newCur}/{newMax}"
                );

                CurVolumeMaskedTextBox.Clear();
                MaxVolumeMaskedTextBox.Clear();
            }
            else
            {
                LOGGER.Warn($"{newCur} cannot be greater than {newMax}");
            }
        }
        // Only “current” was provided:
        else if (hasCur)
        {
            if (newCur <= ViewModel.Series.MaxVolumeCount)
            {
                uint oldCur = ViewModel.Series.CurVolumeCount;
                ViewModel.Series.CurVolumeCount = newCur;

                LOGGER.Info(
                    $"Updated Series Current Volume Count For \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" " +
                    $"From {oldCur} to {newCur}"
                );

                CurVolumeMaskedTextBox.Clear();
            }
            else
            {
                LOGGER.Warn($"{newCur} cannot be greater than {ViewModel.Series.MaxVolumeCount}");
            }
        }
        // Only “max” was provided:
        else if (hasMax)
        {
            if (newMax >= ViewModel.Series.CurVolumeCount)
            {
                uint oldMax = ViewModel.Series.MaxVolumeCount;
                ViewModel.Series.MaxVolumeCount = newMax;

                LOGGER.Info(
                    $"Updated Series Max Volume Count For \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" " +
                    $"From {oldMax} to {newMax}"
                );

                MaxVolumeMaskedTextBox.Clear();
            }
            else
            {
                LOGGER.Warn($"{newMax} cannot be less than {ViewModel.Series.CurVolumeCount}");
            }
        }
    }

    private void UpdateSelectedGenres()
    {
        if (ViewModel.Series.Genres == null)
        {
            return;
        }

        foreach (SeriesGenre genre in ViewModel.Series.Genres)
        {
            if (GenreItemMap.TryGetValue(genre, out object? item))
            {
                GenreSelector.SelectedItems.Add(item);
            }
        }
    }
}