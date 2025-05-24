using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public partial class EditSeriesInfoWindow : ReactiveWindow<EditSeriesInfoViewModel>
{
    private Series Series;
    public EditSeriesInfoWindow()
    {
        InitializeComponent();
        Opened += (s, e) =>
        {
            Series = ViewModel!.Series;
            ViewModel!.Button.Foreground = ViewModelBase.CurrentTheme.SeriesButtonIconHoverColor;
            this.Title = $"{Series.Titles["Romaji"]} Info";
            VolumesReadTextBlock.Text = $"{Series.VolumesRead} Vol{(Series.VolumesRead > 1 ? "s" : string.Empty)} Read";
            UpdateSelectedGenres();
            LOGGER.Debug("{} | {}", this.Height, this.Width);
        };

        Closed += (s, e) =>
        {
            ViewModel!.Button.Foreground = ViewModelBase.CurrentTheme.SeriesButtonIconColor;
        };
    }

    private async void ChangeCoverFromLinkAsync(object sender, RoutedEventArgs args)
    {
        string customImageUrl = CoverImageUrlTextBox.Text.Trim();
        Bitmap newCover = await Common.GenerateAvaloniaBitmap(@$"{Series.Cover.Replace("\\\\", "\\")}", string.Empty, customImageUrl);
        if (newCover != null)
        {
            string fileExtension = customImageUrl[^3..];
            if (!Series.Cover.EndsWith(fileExtension))
            {
                Series.DeleteCover();
                Series.Cover = Series.Cover.Remove(Series.Cover.Length - 3, 3) + fileExtension;
            };
            MainWindowViewModel.ChangeCover(Series, newCover);
            LOGGER.Info($"Changed Cover for \"{Series.Titles["Romaji"]}\" to {customImageUrl}");
        }
        else
        {
            LOGGER.Warn($"\"{customImageUrl}\" is not a valid Image Url");
        }
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
            Series.Demographic = demographic;

            MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateDemographicChartValues();
            MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateDemographicPercentages();

            LOGGER.Info($"Changed Demographic for \"{Series.Titles["Romaji"]}\" to {demographic}");
        }
    }

    private async void ToggleSeriesFavoriteAsync(object sender, RoutedEventArgs args)
    {
        await ((MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow).ViewModel.RefreshCollection();
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
            if (isValidVolumesReadInput && Series.VolumesRead != newVolumesRead)
            {
                Series.VolumesRead = newVolumesRead;
                VolumesReadTextBlock.Text = $"{newVolumesRead} Vol{(Series.VolumesRead > 1 ? "s" : string.Empty)} Read";
                VolumesReadMaskedTextBox.Clear();

                MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateCollectionVolumesRead();

                LOGGER.Info($"Updated # of Volumes Read for \"{Series.Titles["Romaji"]}\" from {Series.VolumesRead} to {newVolumesRead}");
            }
            else
            {
                LOGGER.Warn($"Volumes Read Input {VolumesReadMaskedTextBox.Text} is Invalid");
            }
        }

        if (!RatingMaskedTextBox.Text.StartsWith("__._"))
        {
            bool isValidRatingVal = decimal.TryParse(RatingMaskedTextBox.Text[..4].Trim().Replace("_", "0"), out decimal ratingVal);
            if (isValidRatingVal && decimal.Compare(Series.Rating, ratingVal) != 0 && decimal.Compare(ratingVal, new decimal(10.0)) <= 0)
            {
                Series.Rating = ratingVal;
                RatingTextBlock.Text = $"Rating {ratingVal}/10.0";
                RatingMaskedTextBox.Clear();
                
                // Update rating Distribution Chart
                MainWindowViewModel.UserCollection.First(series => series == Series).Rating = ratingVal;
                MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateRatingChartValues();
                MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateCollectionRating();

                LOGGER.Info($"Updating rating for \"{Series.Titles["Romaji"]}\" {(Series.Rating == -1 ? string.Empty : $"from \"{Series.Rating}/10.0\"")} to \"{decimal.Round(ratingVal, 1)}/10.0\"");
            }
            else
            {
                LOGGER.Warn($"Rating Value {ratingVal} is larger than 10.0 or was Invalid");
            }
        }

        string valueText = ValueMaskedTextBox.Text[1..];
        decimal valueVal = Convert.ToDecimal(valueText.Trim().Replace("_", "0"));
        if (!valueText.Equals("__________________.__") && decimal.Compare(Series.Value, valueVal) != 0)
        {
            string logMsg = $"value for \"{Series.Titles["Romaji"]}\" from {ViewModel.CurCurrencyInstance}{Series.Value} to {ViewModel.CurCurrencyInstance}{valueVal}";
            LOGGER.Info($"Updating {logMsg}");

            Series.Value = valueVal;
            // ValueTextBlock.Text = $"{ViewModel.CurCurrency}{valueVal}";
            ValueMaskedTextBox.Clear();

            MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateCollectionPrice();
            LOGGER.Info($"Updated {logMsg}");
        }
        
        string publisherText = PublisherTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(publisherText) && !publisherText.Equals(Series.Publisher))
        {
            Series.Publisher = publisherText;
            MainWindowViewModel.UpdateSeriesCard(Series);
            PublisherTextBox.Clear();

            LOGGER.Info($"Updated Publisher for \"{Series.Titles["Romaji"]}\" from \"{Series.Publisher}\" to \"{publisherText}\"");

        }
        
        HashSet<Genre> curGenres = EditSeriesInfoViewModel.GetCurrentGenresSelected();
        if (curGenres.Count > 0 && (Series.Genres == null || !curGenres.SetEquals(Series.Genres)))
        {
            LOGGER.Info($"Updating Genres for \"{Series.Titles["Romaji"]}\" from [{string.Join(", ", Series.Genres)}] to [{string.Join(", ", curGenres)}]");
            if (Series.Genres != null)
            {
                MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateGenreChart(curGenres.Except(Series.Genres), Series.Genres.Except(curGenres)); 
            }
            else
            {
                MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateGenreChart(curGenres, []); 
            }
            Series.Genres = curGenres;
        }
    }

    private async void ChangeSeriesCoverFromFileAsync(object sender, RoutedEventArgs args)
    {
        ViewModelBase.newCoverCheck = true;
        IReadOnlyList<IStorageFile> file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions 
        {
            AllowMultiple = false,
            FileTypeFilter = [ new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg"] } ]
        });

        if (file.Count > 0)
        {
            for (int x = 0; x < MainWindowViewModel.UserCollection.Count; x++)
            {
                if (MainWindowViewModel.UserCollection[x] == Series)
                {
                    Series Series = MainWindowViewModel.UserCollection[x];

                    string filePath = file[0].Path.LocalPath;
                    string fileExtension = filePath[^3..];
                    if (!Series.Cover.EndsWith(fileExtension))
                    {
                        Series.DeleteCover();
                        Series.Cover = Series.Cover.Remove(Series.Cover.Length - 3, 3) + fileExtension;
                    };

                    MainWindowViewModel.ChangeCover(Series, filePath);
                    break;
                }
            }
        }
    }

    private async void RefreshSeriesAsync(object sender, RoutedEventArgs args)
    {
        await ((MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow).ViewModel.RefreshSeries(Series);
        GenreSelector.SelectedItems.Clear();
        UpdateSelectedGenres();
        MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateGenreChart();
        
    }

    private async void RemoveSeriesAsync(object sender, RoutedEventArgs args)
    {
        ViewModelBase.newCoverCheck = true;
        _ = await Observable.Start(() => 
        {
            MainWindowViewModel.DeleteSeries(Series);
        }, RxApp.MainThreadScheduler);
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
                        LOGGER.Info($"Changing Series Volume Counts For \"{Series.Titles["Romaji"]}\" From {Series.CurVolumeCount}/{Series.MaxVolumeCount} -> {newCurVols}/{newMaxVols}");
                        
                        MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateVolumeCounts(Series, newCurVols, newMaxVols);

                        Series.CurVolumeCount = newCurVols;
                        Series.MaxVolumeCount = newMaxVols;
                        MainWindowViewModel.UpdateSeriesCard(Series);

                        MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateVolumeCountChartValues();
                        
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
        if (Series.Genres != null)
        {
            foreach (Genre genre in Series.Genres)
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