using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System.ComponentModel;
using System.Diagnostics;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using FileWatcherEx;
using Avalonia.Media;
using Tsundoku.Helpers;
using System.Threading;
using Avalonia.Controls.Primitives;

namespace Tsundoku.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        WindowState previousWindowState;
        private static readonly List<Series> CoverChangedSeriesList = [];
        private static readonly FileSystemWatcherEx CoverFolderWatcher = new FileSystemWatcherEx(@"Covers")
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName,
        };
        private static StringBuilder newSearchText = new StringBuilder();
        private static string itemString;
        private static readonly TimeSpan AdvancedSearchPopulateDelay = new TimeSpan(TimeSpan.TicksPerSecond / 4);

        public MainWindow()
        {
            InitializeComponent();

            SetupAdvancedSearchBar(" & ");

            CoverFolderWatcher.OnCreated += (s, e) =>
            {
                string path = e.FullPath.Replace(".crdownload", "");
                LOGGER.Debug("{} | {} | {} | {}", path, !ViewModelBase.newCoverCheck, path.EndsWith("jpg", StringComparison.OrdinalIgnoreCase), path.EndsWith("png", StringComparison.OrdinalIgnoreCase));
                if (!ViewModelBase.newCoverCheck && (path.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) || path.EndsWith("png", StringComparison.OrdinalIgnoreCase)))
                {
                    string pathFileExtension = path[^3..];
                    Series series = MainWindowViewModel.SearchedCollection.AsParallel().SingleOrDefault(series => series.Cover.Equals(path)) ?? MainWindowViewModel.UserCollection.AsParallel().SingleOrDefault(series => series.Cover.Equals(path));
                    
                    if (series == null)
                    { 
                        string newPath = path[..^3] + (pathFileExtension.Equals("jpg") ? "png" : "jpg");
                        series = MainWindowViewModel.SearchedCollection.AsParallel().SingleOrDefault(series => series.Cover.Equals(newPath)) ?? MainWindowViewModel.UserCollection.AsParallel().SingleOrDefault(series => series.Cover.Equals(newPath));
                    }

                    if (!series.Cover[^3..].Equals(pathFileExtension))
                    {
                        series.Cover = series.Cover[..^3] + pathFileExtension;
                        File.Delete(series.Cover[..^3] + (pathFileExtension.Equals("jpg") ? "png" : "jpg"));
                        LOGGER.Info("Changed File Extention for {} to {}", series.Titles["Romaji"], pathFileExtension);
                    }

                    if (!CoverChangedSeriesList.Contains(series))
                    {
                        CoverChangedSeriesList.Add(series);
                        LOGGER.Info($"Added \"{series.Titles["Romaji"]}\" to Cover Change List");
                    }
                    else
                    {
                        LOGGER.Info($"\"{series.Titles["Romaji"]}\" Cover Changed Again");
                    }
                }
                ViewModelBase.newCoverCheck = false;
            };
            CoverFolderWatcher.Start();

            KeyDown += async (s, e) => 
            {
                if (e.Key == Key.F11) 
                {
                    if (WindowState != WindowState.FullScreen)
                    {
                        previousWindowState = WindowState;
                        WindowState = WindowState.FullScreen;
                        await ToggleNotificationPopup("To Exit Fullscreen Press F11");
                    }
                    else if (WindowState == WindowState.FullScreen)
                    {
                        WindowState = previousWindowState;
                    }
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.P)
                {
                    LOGGER.Info($"Saving Screenshot of Collection for \"{ViewModel.CurrentTheme.ThemeName} Theme\"");
                    // if (NotificationPopup.IsVisible)
                    // {
                    //     await Task.Delay(TimeSpan.FromSeconds(3));
                    // }
                    ScreenCaptureWindows();
                    await ToggleNotificationPopup($"Saved Screenshot for \"{ViewModel.CurrentTheme.ThemeName}\" Theme");
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S)
                {
                    ViewModel.SearchText = "";
                    MainWindowViewModel.SaveUsersData();
                    await ToggleNotificationPopup($"Saved \"{ViewModel.UserName}'s\" Data");
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.R)
                {
                    LOGGER.Info("Reloading Covers");
                    if (CoverChangedSeriesList.Count != 0)
                    {
                        ReloadCoverBitmaps();
                    }
                    else
                    {
                        LOGGER.Debug("No Covers Reloaded");
                    }
                    await ToggleNotificationPopup($"Reloaded Covers");
                }
                else if (e.KeyModifiers == KeyModifiers.Shift && e.Key == Key.R)
                {
                    LOGGER.Info("Reloading Filter/Sort on Collection");
                    ViewModel.FilterCollection(ViewModel.CurFilter);
                    await ToggleNotificationPopup($"Reloadeded \"{ViewModel.CurFilter}\" Filter/Sort on Collection");
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.F)
                {
                    if (AdvancedSearchPopup.IsVisible)
                    {
                        AdvancedSearchBar.MinimumPopulateDelay = TimeSpan.Zero;
                        AdvancedSearchBar.Text = string.Empty;
                        AdvancedSearchBar.MinimumPopulateDelay = AdvancedSearchPopulateDelay;
                    }
                    else
                    {
                        await ToggleNotificationPopup($"To Exit Advanced Search Press CTRL+F");
                    }
                    AdvancedSearchPopup.IsVisible ^= true;
                }
            };

            Closing += (s, e) => { SaveOnClose(ViewModelBase.isReloading); };
        }

        private async Task ToggleNotificationPopup(string notiText)
        {
            ViewModel.NotificationText = notiText;
            NotificationPopup.IsVisible = true;
            await Task.Delay(TimeSpan.FromSeconds(3));
            NotificationPopup.IsVisible = false;
        }

        private void CloseNotificationPopup(object sender, RoutedEventArgs args) => NotificationPopup.IsVisible = false;

        private void OpenAddSeriesDialog(object sender, RoutedEventArgs args)
        {
            MainWindowViewModel.newSeriesWindow.Show(this);
        }

        private void OpenSettingsDialog(object sender, RoutedEventArgs args)
        {
            MainWindowViewModel.settingsWindow.Show(this);
        }

        private void OpenCollectionStatsDialog(object sender, RoutedEventArgs args)
        {
           MainWindowViewModel.collectionStatsWindow.Show(this);
        }

        private void OpenPriceAnalysisDialog(object sender, RoutedEventArgs args)
        {
            MainWindowViewModel.priceAnalysisWindow.Show(this);
        }

        private void OpenThemeSettingsDialog(object sender, RoutedEventArgs args)
        {
            MainWindowViewModel.themeSettingsWindow.Show(this);
        }

        private void OpenUserNotesDialog(object sender, RoutedEventArgs args)
        {
            MainWindowViewModel.userNotesWindow.Show(this);
        }

        public void SetupAdvancedSearchBar(string delimeter)
        {
            AdvancedSearchBar.ItemsSource = ADVANCED_SEARCH_FILTERS;
            AdvancedSearchBar.FilterMode = AutoCompleteFilterMode.Custom;
            AdvancedSearchBar.MinimumPopulateDelay = AdvancedSearchPopulateDelay; // Might just remove this delay in the end
            AdvancedSearchBar.ItemSelector = (query, item) =>
            {
                newSearchText.Clear();
                if (MainWindowViewModel.AdvancedQueryRegex().IsMatch(query))
                {
                    newSearchText.Append(query[..query.LastIndexOf(delimeter)]).Append(delimeter);
                }
                return !item.Equals("Notes==") ? newSearchText.Append(item).ToString() : newSearchText.Append(item).ToString();
            };
            AdvancedSearchBar.ItemFilter = (query, item) =>
            {
                itemString = item as string;
                if (!MainWindowViewModel.AdvancedQueryRegex().IsMatch(query))
                {
                    return itemString.StartsWith(query, StringComparison.OrdinalIgnoreCase);
                }
                else if (query.Contains(itemString) || itemString.Last() != '=' && query.Contains(itemString[..itemString.IndexOf("==")]))
                {
                    return false;
                }
                
                string filterName = itemString[..^2];
                if (itemString.Contains("==") && (query.Contains($"{filterName}<=") || query.Contains($"{filterName}>=")))
                {
                    return false;
                }
                else if ((itemString.Contains('>') || itemString.Contains('<')) && query.Contains($"{filterName}=="))
                {
                    return false;
                }
 
                return AdvancedSearchBar.IsVisible && itemString.StartsWith(query[(query.LastIndexOf(delimeter) + delimeter.Length)..], StringComparison.OrdinalIgnoreCase);
            };
        }
        
        private void UnShowAdvancedSearchPopup(object sender, PointerPressedEventArgs args)
        {
            AdvancedSearchPopup.IsVisible = false;
        }

        private void RemoveErrorMessage(object sender, KeyEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(ViewModel.AdvancedSearchQueryErrorMessage))
            {
                ViewModel.AdvancedSearchQueryErrorMessage = string.Empty;
            }
        }   

        /// <summary>
        /// Reloads Covers for Series where the cover was changed
        /// </summary>
        private static void ReloadCoverBitmaps()
        {
            LOGGER.Debug("Reloading Cover Bitmaps");
            uint cache = 0;
            for (int x = 0; x < MainWindowViewModel.UserCollection.Count; x++)
            {
                // If the image does not exist in the covers folder then don't create a bitmap for it
                if (CoverChangedSeriesList.Contains(MainWindowViewModel.UserCollection[x]))
                {
                    MainWindowViewModel.ChangeCover(MainWindowViewModel.UserCollection[x], MainWindowViewModel.UserCollection[x].Cover);
                    cache++;
                }
                else if (cache == CoverChangedSeriesList.Count)
                {
                    break;
                }
            }
            CoverChangedSeriesList.Clear();
        }

        private async void RefreshSeries(object sender, RoutedEventArgs args)
        {
            ViewModelBase.newCoverCheck = true;
            Series curSeries = (Series)((Button)sender).DataContext;
            Series? newSeriesData = await Series.CreateNewSeriesCardAsync(curSeries.Titles["Romaji"], curSeries.Format, curSeries.MaxVolumeCount, curSeries.CurVolumeCount, curSeries.SeriesContainsAdditionalLanagues(), curSeries.Publisher, curSeries.Demographic, curSeries.VolumesRead, curSeries.Rating, curSeries.Value);

            if (newSeriesData != null)
            {
                bool titleChanged = false, staffChanged = false, statusChanged = false;
                LOGGER.Info($"Refreshing/Updating \"{curSeries.Titles["Romaji"]}\" Data");

                int searchIndex = MainWindowViewModel.SearchedCollection.ToList().BinarySearch(curSeries, new SeriesComparer(ViewModelBase.MainUser.CurLanguage));
                searchIndex = searchIndex < 0 ? ~searchIndex : searchIndex;
                if (searchIndex > -1)
                {
                    MainWindowViewModel.SearchedCollection.RemoveAt(searchIndex);
                    MainWindowViewModel.SearchedCollection.Insert(searchIndex, curSeries);
                }

                int mainIndex = MainWindowViewModel.UserCollection.BinarySearch(curSeries, new SeriesComparer(ViewModelBase.MainUser.CurLanguage));
                mainIndex = mainIndex < 0 ? ~mainIndex : mainIndex;

                if (!curSeries.Titles.Equals(newSeriesData.Titles))
                {
                    curSeries.Titles = newSeriesData.Titles;
                    MainWindowViewModel.UserCollection[mainIndex].Titles = newSeriesData.Titles;
                    titleChanged = true;
                }
                if (!curSeries.Staff.Equals(newSeriesData.Staff))
                {
                    curSeries.Staff = newSeriesData.Staff;
                    MainWindowViewModel.UserCollection[mainIndex].Staff = newSeriesData.Staff;
                    staffChanged = true;
                }
                if (!curSeries.Description.Equals(newSeriesData.Description))
                {
                    curSeries.Description = newSeriesData.Description;
                    MainWindowViewModel.UserCollection[mainIndex].Description = newSeriesData.Description;
                }
                if (curSeries.Status != newSeriesData.Status)
                {
                    curSeries.Status = newSeriesData.Status;
                    MainWindowViewModel.UserCollection[mainIndex].Status = newSeriesData.Status;
                    MainWindowViewModel.UpdateChartStats();
                    statusChanged = true;
                }
                
                // If there is a change and the user is searching or filtering apply the filter
                if (MainWindowViewModel.SearchIsBusy && (titleChanged || staffChanged))
                {
                    if (ViewModel.CurFilter == TsundokuFilter.Query)
                    {
                        ViewModel.AdvancedSearchCollection(ViewModel.AdvancedSearchText);
                    }
                    else
                    {
                        ViewModel.SearchCollection(ViewModel.SearchText);
                    }
                }
                else if ((titleChanged && !ViewModel.CurFilter.Equals("None")) || (statusChanged && (ViewModel.CurFilter.Equals("Ongoing") || ViewModel.CurFilter.Equals("Finished") || ViewModel.CurFilter.Equals("Hiatus") || ViewModel.CurFilter.Equals("Cancelled"))))
                {
                    ViewModel.FilterCollection(ViewModel.CurFilter);
                }
            }
            else
            {
                LOGGER.Error($"Refresh Returned Null Series Data for \"{curSeries.Titles["Romaji"]}\"");
            }
        }

        private async void ChangeSeriesCover(object sender, RoutedEventArgs args)
        {
            ViewModelBase.newCoverCheck = true;
            var file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>() { new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg"] } }
            });
            if (file.Count > 0)
            {
                for (int x = 0; x < MainWindowViewModel.UserCollection.Count; x++)
                {
                    if (MainWindowViewModel.UserCollection[x] == (Series)((Button)sender).DataContext)
                    {
                        Series curSeries = MainWindowViewModel.UserCollection[x];

                        string filePath = file[0].Path.LocalPath;
                        string fileExtension = filePath[^3..];
                        if (!curSeries.Cover.EndsWith(fileExtension))
                        {
                            MainWindowViewModel.DeleteCover(curSeries);
                            curSeries.Cover = curSeries.Cover.Remove(curSeries.Cover.Length - 3, 3) + fileExtension;
                        };

                        MainWindowViewModel.ChangeCover(curSeries, filePath);
                        (sender as Button).FindLogicalAncestorOfType<Grid>(false).IsVisible = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Saves the stats for the series when the button is clicked
        /// </summary>
        private void SaveStats(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext;
            var stackPanels = ((Button)sender).GetLogicalParent<StackPanel>().GetLogicalParent<Grid>().GetLogicalChildren();
            string volumesRead = (stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(1) as MaskedTextBox).Text.Trim().Replace("_", "");
            if (!string.IsNullOrWhiteSpace(volumesRead))
            {
                uint volumesReadVal = Convert.ToUInt32(volumesRead);
                if (curSeries.VolumesRead != volumesReadVal)
                {
                    LOGGER.Info($"Updating # of Volumes Read for \"{curSeries.Titles["Romaji"]}\" from {curSeries.VolumesRead} to {volumesReadVal}");

                    curSeries.VolumesRead = volumesReadVal;
                    (stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(0) as TextBlock).Text = $"{volumesReadVal} Vol(s)";
                    volumesReadVal = 0;
                    MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateCollectionVolumesRead();
                    ((MaskedTextBox)stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(1)).Text = "";
                }
            }
            LOGGER.Debug("Passed Volumes Read Check");

            string ratingValText = ((MaskedTextBox)stackPanels.ElementAt(3).GetLogicalChildren().ElementAt(1)).Text;
            decimal ratingVal = Convert.ToDecimal(ratingValText[..4].Trim().Replace("_", "0"));
            if (decimal.Compare(ratingVal, 0) >= 0 && curSeries.Rating != ratingVal && !ratingValText.StartsWith("__._"))
            {
                if (decimal.Compare(ratingVal, new decimal(10.0)) <= 0)
                {
                    LOGGER.Info($"Updating rating for \"{curSeries.Titles["Romaji"]}\" from \"{curSeries.Rating}/10.0\" to \"{decimal.Round(ratingVal, 1)}/10.0\"");

                    curSeries.Rating = ratingVal;
                    ((TextBlock)stackPanels.ElementAt(3).GetLogicalChildren().ElementAt(0)).Text = $"Rating {ratingVal}/10.0";
                    ((MaskedTextBox)stackPanels.ElementAt(3).GetLogicalChildren().ElementAt(1)).Text = "";
                    
                    // Update rating Distribution Chart
                    MainWindowViewModel.UserCollection.First(series => series == curSeries).Rating = ratingVal;
                    MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateRatingChartValues();

                    MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateCollectionRating();
                }
                else
                {
                    LOGGER.Warn($"Rating Value {ratingVal} is larger than 10.0");
                }
            }
            LOGGER.Debug("Passed Rating Check");

            var bottomPanel = stackPanels.ElementAt(4).GetLogicalChildren();
            string valueText = ((MaskedTextBox)bottomPanel.ElementAt(0).GetLogicalChildren().ElementAt(1)).Text;
            decimal valueVal = Convert.ToDecimal(valueText.Trim().Replace("_", "0"));
            if (decimal.Compare(valueVal, 0) >= 0 && curSeries.Value != valueVal && !valueText.Equals("_________.__"))
            {
                LOGGER.Info($"Updating value for \"{curSeries.Titles["Romaji"]}\" from {ViewModel.CurCurrency}{curSeries.Value} to {ViewModel.CurCurrency}{valueVal}");

                curSeries.Value = valueVal;
                ((TextBlock)bottomPanel.ElementAt(0).GetLogicalDescendants().ElementAt(0)).Text = $"{ViewModel.CurCurrency}{valueVal}";
                MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateCollectionPrice();
                ((MaskedTextBox)bottomPanel.ElementAt(0).GetLogicalDescendants().ElementAt(1)).Text = "";
            }
            LOGGER.Debug("Passed Value Check");

            TextBox publisherTextBox = (TextBox)bottomPanel.ElementAt(1).GetLogicalChildren().ElementAt(1);
            string publisherText = string.IsNullOrWhiteSpace(publisherTextBox.Text) ? publisherTextBox.Text : publisherTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(publisherText) && !publisherText.Equals(curSeries.Publisher))
            {
                LOGGER.Info($"Updating Publisher for \"{curSeries.Titles["Romaji"]}\" from \"{curSeries.Publisher}\" to \"{publisherText}\"");

                curSeries.Publisher = publisherText;
                (((Button)sender).GetLogicalParent<StackPanel>().GetLogicalParent<Grid>().GetLogicalSiblings().ElementAt(2).GetLogicalChildren().ElementAt(0) as TextBlock).Text = publisherText;
                // TODO - Update publisher stats
                publisherTextBox.Clear();
            }
            LOGGER.Debug("Passed Publisher Check");
        }

        /// <summary>
        /// Takes a screenshot of the current collection window
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private void ScreenCaptureWindows()
        {
            Directory.CreateDirectory(@"Screenshots");
            using (System.Drawing.Bitmap bitmap = new((int)this.Width, (int)this.Height))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new System.Drawing.Point((int)this.Bounds.Left, (int)this.Bounds.Top), System.Drawing.Point.Empty, new System.Drawing.Size((int)this.Width, (int)this.Height));
                }
                bitmap.Save(@$"Screenshots\{ViewModel.UserName}-Collection-Screenshot-{ViewModel.CurrentTheme.ThemeName}.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private void SearchCollection(object sender, KeyEventArgs args) => MainWindowViewModel.UserIsSearching(true);

        private async void ChangeSeriesVolumeCounts(object sender, RoutedEventArgs args)
        {
            var result = await Observable.Start(() => 
            {
                var textBoxes = ((Button)sender).GetLogicalSiblings();
                Series curSeries = (Series)((Button)sender).DataContext;
                string curVolumeString = ((MaskedTextBox)textBoxes.ElementAt(1)).Text.Trim().Replace("_", "");
                string maxVolumeString = ((MaskedTextBox)textBoxes.ElementAt(2)).Text.Trim().Replace("_", "");

                ushort curVolumeChange = !string.IsNullOrWhiteSpace(curVolumeString) ? Convert.ToUInt16(curVolumeString) : curSeries.CurVolumeCount;
                ushort maxVolumeChange = !string.IsNullOrWhiteSpace(maxVolumeString) ? Convert.ToUInt16(maxVolumeString) : curSeries.MaxVolumeCount;
                if (maxVolumeChange >= curVolumeChange)
                {
                    MainWindowViewModel.collectionStatsWindow.ViewModel.UsersNumVolumesCollected = MainWindowViewModel.collectionStatsWindow.ViewModel.UsersNumVolumesCollected - curSeries.CurVolumeCount + curVolumeChange;
                    MainWindowViewModel.collectionStatsWindow.ViewModel.UsersNumVolumesToBeCollected = MainWindowViewModel.collectionStatsWindow.ViewModel.UsersNumVolumesToBeCollected - (uint)(curSeries.MaxVolumeCount - curSeries.CurVolumeCount) + (uint)(maxVolumeChange - curVolumeChange);
                    LOGGER.Info($"Changed Series Volume Values For \"{curSeries.Titles["Romaji"]}\" From {curSeries.CurVolumeCount}/{curSeries.MaxVolumeCount} -> {curVolumeChange}/{maxVolumeChange}");
                    curSeries.CurVolumeCount = curVolumeChange;
                    curSeries.MaxVolumeCount = maxVolumeChange;

                    var parentControl = (sender as Button).FindLogicalAncestorOfType<Grid>(false).GetLogicalSiblings().ElementAt(3);
                    ((TextBlock)parentControl.FindLogicalDescendantOfType<StackPanel>(false).GetLogicalChildren().ElementAt(1)).Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                    
                    ProgressBar seriesProgressBar = parentControl.FindLogicalDescendantOfType<ProgressBar>(false);
                    seriesProgressBar.Maximum = maxVolumeChange;
                    seriesProgressBar.Value = curVolumeChange;
                    parentControl = null;
                    seriesProgressBar = null; 
                    ((MaskedTextBox)textBoxes.ElementAt(1)).Text = "";
                    ((MaskedTextBox)textBoxes.ElementAt(2)).Text = "";

                    MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateVolumeCountChartValues();
                }
                else
                {
                    LOGGER.Warn($"{curVolumeChange} Is Not Less Than or Equal To {maxVolumeChange}");
                }
            }, RxApp.MainThreadScheduler);
        }

        private async void RemoveSeries(object sender, RoutedEventArgs args)
        {
            // Close the edit pane before removing
            ViewModelBase.newCoverCheck = true;
            (sender as Button).FindLogicalAncestorOfType<Grid>(false).IsVisible &= false;
            var result = await Observable.Start(() => 
            {
                MainWindowViewModel.DeleteSeries((Series)((Button)sender).DataContext);
            }, RxApp.MainThreadScheduler);
        }

        private void ShowEditPane(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)(sender as Button).DataContext;
            Button button = (Button)sender;
            if (curSeries.IsStatsPaneOpen)
            {
                curSeries.IsStatsPaneOpen = false;
                (button.FindLogicalAncestorOfType<Grid>(false).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible = curSeries.IsStatsPaneOpen;
                (button.GetLogicalSiblings().ElementAt(2) as Button).Foreground = SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesButtonIconColor);
            }
            curSeries.IsEditPaneOpen ^= true;
            button.FindLogicalAncestorOfType<Grid>(false).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible = curSeries.IsEditPaneOpen;
            button.Foreground = curSeries.IsEditPaneOpen ? SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesButtonIconHoverColor) : SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesButtonIconColor);
        }

        private void ShowStatsPane(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)(sender as Button).DataContext;
            Button button = (Button)sender;
            if (curSeries.IsEditPaneOpen)
            {
                curSeries.IsEditPaneOpen = false;
                button.FindLogicalAncestorOfType<Grid>(false).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible = curSeries.IsEditPaneOpen;
                (button.GetLogicalSiblings().ElementAt(0) as Button).Foreground = SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesButtonIconColor);
            }
            curSeries.IsStatsPaneOpen ^= true;
            (button.FindLogicalAncestorOfType<Grid>(false).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible = curSeries.IsStatsPaneOpen;
            button.Foreground = curSeries.IsStatsPaneOpen ? SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesButtonIconHoverColor) : SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesButtonIconColor);
        }

        /// <summary>
        /// Changes the language for the users collection
        /// </summary>
        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                if (!string.IsNullOrWhiteSpace(ViewModel.SearchText)) // Checks if the user is filtering after a search
                {
                    // ViewModel.SearchIsBusy = false;
                    ViewModel.SearchText = string.Empty;
                }
                // AddNewSeriesWindow.PreviousLanguage = ViewModel.CurLanguage;
                ViewModel.CurLanguage = (LanguageSelector.SelectedItem as ComboBoxItem).Content.ToString();
                LOGGER.Info($"Changed Langauge to \"{ViewModel.CurLanguage}\"");
                MainWindowViewModel.SortCollection();
            }
        }

        /// <summary>
        /// Changes the filter on the users collection
        /// </summary>
        private void CollectionFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                ViewModel.UpdateCurFilter(CollectionFilterSelector.SelectedItem as ComboBoxItem);
            }
        }

        /// <summary>
        /// Changes the chosen demographic for a particular series
        /// </summary>
        private void DemographicChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                Series curSeries = (Series)((ComboBox)sender).DataContext;
                Demographic demographic = Series.GetSeriesDemographic((sender as ComboBox).SelectedItem.ToString());
                
                // Update Demographic Chart Values
                curSeries.Demographic = demographic;
                MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateDemographicChartValues();
                MainWindowViewModel.collectionStatsWindow.ViewModel.UpdateDemographicPercentages();
                
                curSeries.Demographic = demographic;
                LOGGER.Info($"Changed Demographic for \"{curSeries.Titles["Romaji"]}\" to {demographic}");
            }
        }

        // private void DisplayChanged(object sender, SelectionChangedEventArgs e)
        // {
        //     if ((sender as ComboBox).IsDropDownOpen)
        //     {
        //         string display = (sender as ComboBox).SelectedItem as string;
        //         if (display.Equals("Card"))
        //         {
        //             ViewModel.CurDisplay = "Card";
        //         }
        //         else if (display.Equals("Mini-Card"))
        //         {
        //             ViewModel.CurDisplay = "Mini-Card";
        //         }
        //         LOGGER.Info($"Changed Display To {ViewModel.CurDisplay}");
        //     }
        // }

        public void SaveOnClose(bool isReloading)
        {
            LOGGER.Info("Closing Tsundoku");
            ViewModel.SearchText = "";
            if (!isReloading) { MainWindowViewModel.SaveUsersData(); }
            Helpers.DiscordRP.Deinitialize();
            CoverFolderWatcher.Dispose();

            if (MainWindowViewModel.newSeriesWindow != null)
            {
                MainWindowViewModel.newSeriesWindow.Closing += (s, e) => { e.Cancel = false; };
                MainWindowViewModel.newSeriesWindow.Close();
            }

            if (MainWindowViewModel.settingsWindow != null)
            {
                MainWindowViewModel.settingsWindow.Closing += (s, e) => { e.Cancel = false; };
                MainWindowViewModel.settingsWindow.Close();
            }

            if (MainWindowViewModel.themeSettingsWindow != null)
            {
                MainWindowViewModel.themeSettingsWindow.Closing += (s, e) => { e.Cancel = false; };
                MainWindowViewModel.themeSettingsWindow.Close();
            }

            if (MainWindowViewModel.priceAnalysisWindow != null)
            {
                MainWindowViewModel.priceAnalysisWindow.Closing += (s, e) => { e.Cancel = false; };
                MainWindowViewModel.priceAnalysisWindow.Close();
            }

            if (MainWindowViewModel.collectionStatsWindow != null)
            {
                MainWindowViewModel.collectionStatsWindow.Closing += (s, e) => { e.Cancel = false; };
                MainWindowViewModel.collectionStatsWindow.Close();
            }

            if (MainWindowViewModel.userNotesWindow != null)
            {
                MainWindowViewModel.userNotesWindow.Closing += (s, e) => { e.Cancel = false; };
                MainWindowViewModel.userNotesWindow.Close();
            }

            NLog.LogManager.Shutdown();
            App.DisposeMutex();
        }

        /// <summary>
        /// Opens the AniList or MangaDex website link in the users browser when users clicks on the left side of the series card
        /// </summary>
        private async void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            await ViewModelBase.OpenSiteLink(((sender as Canvas).DataContext as Series).Link.ToString());
        }
        
        private async void ChangeUserIcon(object sender, PointerPressedEventArgs args)
        {
            var file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>() { new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.crdownload"] } }
            });
            if (file.Count > 0)
            {
                ViewModel.UserIcon = new Bitmap(file[0].Path.LocalPath).CreateScaledBitmap(new PixelSize(USER_ICON_WIDTH, USER_ICON_HEIGHT), BitmapInterpolationMode.HighQuality);
                LOGGER.Info($"Changed Users Icon to {file[0].Path.LocalPath}");
            }
        }

        /// <summary>
        /// Opens my PayPal for users to donate if they want to
        /// </summary>
        private void OpenDonationLink(object sender, RoutedEventArgs args)
        {
            LOGGER.Info($"Opening PayPal Donation Lionk");
            try
            {
                Process.Start(new ProcessStartInfo("https://paypal.me/Preminence8?country.x=US&locale.x=en_US") { UseShellExecute = true });
            }
            catch (Win32Exception noBrowser)
            {
                LOGGER.Error(noBrowser.Message);
            }
            catch (Exception other)
            {
                LOGGER.Error(other.Message);
            }
        }

        /// <summary>
        /// Copies the title of the series when clicked om
        /// </summary>
        private async void CopySeriesTitleAsync(object sender, PointerPressedEventArgs args)
        {
            string title = ((TextBlock)sender).Text;
            LOGGER.Info("Copying {} to Clipboard", title);
            await TextCopy.ClipboardService.SetTextAsync(title);
        }

        /// <summary>
        /// Increments the series current volume count
        /// </summary>
        public void AddVolume(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext;
            if (curSeries.CurVolumeCount < curSeries.MaxVolumeCount)
            {
                MainWindowViewModel.collectionStatsWindow.ViewModel.UsersNumVolumesCollected++;
                MainWindowViewModel.collectionStatsWindow.ViewModel.UsersNumVolumesToBeCollected--;
                curSeries.CurVolumeCount += 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).GetLogicalSiblings().ElementAt(1);
                string log = $"Adding 1 Volume To \"{curSeries.Titles["Romaji"]}\" : {volumeDisplay.Text} -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                LOGGER.Info(log + volumeDisplay.Text);

                (sender as Button).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<ProgressBar>(false).Value = curSeries.CurVolumeCount;
            }
        }

        /// <summary>
        /// Deccrements the series current volume count
        /// </summary>
        public void SubtractVolume(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext; //Get the current series data
            if (curSeries.CurVolumeCount >= 1) //Only decrement if the user currently has 1 or more volumes
            {
                MainWindowViewModel.collectionStatsWindow.ViewModel.UsersNumVolumesCollected--;
                MainWindowViewModel.collectionStatsWindow.ViewModel.UsersNumVolumesToBeCollected++;
                curSeries.CurVolumeCount -= 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).GetLogicalSiblings().ElementAt(1);
                string log = $"Removing 1 Volume From \"{curSeries.Titles["Romaji"]}\" : {volumeDisplay.Text} -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                LOGGER.Info(log + volumeDisplay.Text);

                (sender as Button).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<ProgressBar>(false).Value = curSeries.CurVolumeCount;
            }
        }
    }
}
