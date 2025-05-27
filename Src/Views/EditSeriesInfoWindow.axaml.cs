using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using static Tsundoku.Models.TsundokuLanguageModel;

namespace Tsundoku.Views;

public partial class EditSeriesInfoWindow : ReactiveWindow<EditSeriesInfoViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly BitmapHelper _bitmapHelper;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly CollectionStatsViewModel _collectionStatsViewModel;
    public EditSeriesInfoWindow(MainWindowViewModel mainWindowViewModel, CollectionStatsViewModel collectionStatsViewModel, BitmapHelper bitmapHelper)
    {
        _bitmapHelper = bitmapHelper;
        _mainWindowViewModel = mainWindowViewModel;
        _collectionStatsViewModel = collectionStatsViewModel;
        InitializeComponent();

        Opened += (s, e) =>
        {;
            this.Title = $"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]} Info";
            VolumesReadTextBlock.Text = $"{ViewModel.Series.VolumesRead} Vol{(ViewModel.Series.VolumesRead > 1 ? "s" : string.Empty)} Read";
            UpdateSelectedGenres();
            LOGGER.Debug("{} | {}", this.Height, this.Width);
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
        string customImageUrl = CoverImageUrlTextBox.Text.Trim();
        string fullCoverPath = AppFileHelper.GetFullCoverPath(ViewModel.Series.Cover);
        Bitmap newCover = await _bitmapHelper.GenerateAvaloniaBitmapAsync(fullCoverPath, string.Empty, customImageUrl, true);

        GenerateNewBitmap(newCover, customImageUrl);

        CoverImageUrlTextBox.Clear();
    }

    /// <summary>
    /// Changes the chosen demographic for a particular series
    /// </summary>
    private void DemographicChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DemographicComboBox.IsDropDownOpen)
        {
            Demographic demographic = Series.GetSeriesDemographic((DemographicComboBox.SelectedItem as ComboBoxItem).Content.ToString());
            ViewModel.Series.Demographic = demographic;

            _collectionStatsViewModel.UpdateDemographicChartValues();
            _collectionStatsViewModel.UpdateDemographicPercentages();

            LOGGER.Info($"Changed Demographic for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" to {demographic}");
        }
    }

    /// <summary>
    /// Saves the stats for the series when the button is clicked
    /// </summary>
    private void SaveStats(object sender, RoutedEventArgs args)
    {
        string volumesReadText = VolumesReadMaskedTextBox.Text.Replace("_", string.Empty);
        if (!string.IsNullOrWhiteSpace(volumesReadText))
        {
            bool isValidVolumesReadInput = uint.TryParse(volumesReadText, out uint newVolumesRead);
            if (isValidVolumesReadInput && ViewModel.Series.VolumesRead != newVolumesRead)
            {
                ViewModel.Series.VolumesRead = newVolumesRead;
                VolumesReadTextBlock.Text = $"{newVolumesRead} Vol{(ViewModel.Series.VolumesRead > 1 ? "s" : string.Empty)} Read";
                VolumesReadMaskedTextBox.Clear();

                _collectionStatsViewModel.UpdateCollectionVolumesRead();

                LOGGER.Info($"Updated # of Volumes Read for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" from {ViewModel.Series.VolumesRead} to {newVolumesRead}");
            }
            else
            {
                LOGGER.Warn($"Volumes Read Input {VolumesReadMaskedTextBox.Text} is Invalid");
            }
        }

        if (!RatingMaskedTextBox.Text.StartsWith("__._"))
        {
            bool isValidRatingVal = decimal.TryParse(RatingMaskedTextBox.Text[..4].Trim().Replace("_", "0"), out decimal ratingVal);
            if (isValidRatingVal && decimal.Compare(ViewModel.Series.Rating, ratingVal) != 0 && decimal.Compare(ratingVal, new decimal(10.0)) <= 0)
            {
                ViewModel.Series.Rating = ratingVal;
                RatingTextBlock.Text = $"Rating {ratingVal}/10.0";
                RatingMaskedTextBox.Clear();

                // Update rating Distribution Chart
                // TODO - Need to actually update the rating of the series object here
                //ViewModel.UserCollection.First(series => series == Series).Rating = ratingVal;
                _collectionStatsViewModel.UpdateRatingChartValues();
                _collectionStatsViewModel.UpdateCollectionRating();

                LOGGER.Info($"Updating rating for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" {(ViewModel.Series.Rating == -1 ? string.Empty : $"from \"{ViewModel.Series.Rating}/10.0\"")} to \"{decimal.Round(ratingVal, 1)}/10.0\"");
            }
            else
            {
                LOGGER.Warn($"Rating Value {ratingVal} is larger than 10.0 or was Invalid");
            }
        }

        string valueText = ValueMaskedTextBox.Text[1..];
        decimal valueVal = Convert.ToDecimal(valueText.Trim().Replace("_", "0"));
        if (!valueText.Equals("__________________.__") && decimal.Compare(ViewModel.Series.Value, valueVal) != 0)
        {
            string logMsg = $"value for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" from {ViewModel.CurCurrencyInstance}{ViewModel.Series.Value} to {ViewModel.CurCurrencyInstance}{valueVal}";
            LOGGER.Info($"Updating {logMsg}");

            ViewModel.Series.Value = valueVal;
            // ValueTextBlock.Text = $"{ViewModel.CurCurrency}{valueVal}";
            ValueMaskedTextBox.Clear();

            _collectionStatsViewModel.UpdateCollectionPrice();
            LOGGER.Info($"Updated {logMsg}");
        }
        
        string publisherText = PublisherTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(publisherText) && !publisherText.Equals(ViewModel.Series.Publisher))
        {
            ViewModel.Series.Publisher = publisherText;
            _mainWindowViewModel.UpdateSeriesCard(ViewModel.Series);
            PublisherTextBox.Clear();

            LOGGER.Info($"Updated Publisher for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" from \"{ViewModel.Series.Publisher}\" to \"{publisherText}\"");

        }
        
        HashSet<Genre> curGenres = EditSeriesInfoViewModel.GetCurrentGenresSelected();
        if (curGenres.Count > 0 && (ViewModel.Series.Genres == null || !curGenres.SetEquals(ViewModel.Series.Genres)))
        {
            LOGGER.Info($"Updating Genres for \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" from [{string.Join(", ", ViewModel.Series.Genres)}] to [{string.Join(", ", curGenres)}]");
            if (ViewModel.Series.Genres != null)
            {
                _collectionStatsViewModel.UpdateGenreChart(curGenres.Except(ViewModel.Series.Genres), ViewModel.Series.Genres.Except(curGenres)); 
            }
            else
            {
                _collectionStatsViewModel.UpdateGenreChart(curGenres, []); 
            }
            ViewModel.Series.Genres = curGenres;
        }
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
            Bitmap newCover = await _bitmapHelper.GenerateAvaloniaBitmapAsync(fullCoverPath, file[0].Path.LocalPath);
            GenerateNewBitmap(newCover, fullCoverPath);
        }
        else
        {
            LOGGER.Warn("User selected multiple files for user icon.");
        }
    }

    private async void RefreshSeriesAsync(object sender, RoutedEventArgs args)
    {
        await _mainWindowViewModel.RefreshSeries(ViewModel.Series);
        GenreSelector.SelectedItems.Clear();
        UpdateSelectedGenres();
        _collectionStatsViewModel.UpdateGenreChart();
        
    }

    private void RemoveSeries(object sender, RoutedEventArgs args)
    {
        ViewModelBase.newCoverCheck = true;
        _mainWindowViewModel.DeleteSeries(ViewModel.Series);
        this.Close();
    }

    private async void ChangeSeriesVolumeCountsAsync(object sender, RoutedEventArgs args)
    {
        _ = await Observable.Start(() => 
        {
            string curVolumeString = CurVolumeMaskedTextBox.Text.Replace("_", string.Empty);
            string maxVolumeString = MaxVolumeMaskedTextBox.Text.Replace("_", string.Empty);
            if (!string.IsNullOrWhiteSpace(curVolumeString) && !string.IsNullOrWhiteSpace(maxVolumeString))
            {
                if (ushort.TryParse(curVolumeString, out ushort newCurVols) && ushort.TryParse(maxVolumeString, out ushort newMaxVols))
                {
                    if (newMaxVols >= newCurVols)
                    {
                        LOGGER.Info($"Changing Series Volume Counts For \"{ViewModel.Series.Titles[TsundokuLanguage.Romaji]}\" From {ViewModel.Series.CurVolumeCount}/{ViewModel.Series.MaxVolumeCount} -> {newCurVols}/{newMaxVols}");
                        
                        _collectionStatsViewModel.UpdateVolumeCounts(ViewModel.Series, newCurVols, newMaxVols);

                        ViewModel.Series.CurVolumeCount = newCurVols;
                        ViewModel.Series.MaxVolumeCount = newMaxVols;
                        _mainWindowViewModel.UpdateSeriesCard(ViewModel.Series);

                        _collectionStatsViewModel.UpdateVolumeCountChartValues();
                        
                        CurVolumeMaskedTextBox.Clear();
                        MaxVolumeMaskedTextBox.Clear();
                    }
                    else
                    {
                        LOGGER.Warn($"{newCurVols} Is Not Less Than or Equal To {newMaxVols}");
                    }
                }
                else
                {
                    LOGGER.Warn($"\"{curVolumeString}\" and \"{maxVolumeString}\" are not Valid ushort Inputs");
                }
            }
        }, RxApp.MainThreadScheduler);
    }

    public void UpdateSelectedGenres()
    {
        if (ViewModel.Series.Genres != null)
        {
            foreach (Genre genre in ViewModel.Series.Genres)
            {
                switch (genre)
                {
                    case Genre.Action:
                        GenreSelector.SelectedItems.Add(ActionListBoxItem);
                        break;
                    case Genre.Adventure:
                        GenreSelector.SelectedItems.Add(AdventureListBoxItem);
                        break;
                    case Genre.Comedy:
                        GenreSelector.SelectedItems.Add(ComedyListBoxItem);
                        break;
                    case Genre.Drama:
                        GenreSelector.SelectedItems.Add(DramaListBoxItem);
                        break;
                    case Genre.Ecchi:
                        GenreSelector.SelectedItems.Add(EcchiListBoxItem);
                        break;
                    case Genre.Fantasy:
                        GenreSelector.SelectedItems.Add(FantasyListBoxItem);
                        break;
                    case Genre.Horror:
                        GenreSelector.SelectedItems.Add(HorrorListBoxItem);
                        break;
                    case Genre.MahouShoujo:
                        GenreSelector.SelectedItems.Add(MahouShoujoListBoxItem);
                        break;
                    case Genre.Mecha:
                        GenreSelector.SelectedItems.Add(MechaListBoxItem);
                        break;
                    case Genre.Music:
                        GenreSelector.SelectedItems.Add(MusicListBoxItem);
                        break;
                    case Genre.Mystery:
                        GenreSelector.SelectedItems.Add(MysteryListBoxItem);
                        break;
                    case Genre.Psychological:
                        GenreSelector.SelectedItems.Add(PsychologicalListBoxItem);
                        break;
                    case Genre.Romance:
                        GenreSelector.SelectedItems.Add(RomanceListBoxItem);
                        break;
                    case Genre.SciFi:
                        GenreSelector.SelectedItems.Add(SciFiListBoxItem);
                        break;
                    case Genre.SliceOfLife:
                        GenreSelector.SelectedItems.Add(SliceOfLifeListBoxItem);
                        break;
                    case Genre.Sports:
                        GenreSelector.SelectedItems.Add(SportsListBoxItem);
                        break;
                    case Genre.Supernatural:
                        GenreSelector.SelectedItems.Add(SupernaturalListBoxItem);
                        break;
                    case Genre.Thriller:
                        GenreSelector.SelectedItems.Add(ThrillerListBoxItem);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}