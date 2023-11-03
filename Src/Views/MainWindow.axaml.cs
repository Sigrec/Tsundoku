using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System.ComponentModel;
using System.Diagnostics;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Runtime.InteropServices;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using FileWatcherEx;

namespace Tsundoku.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindowViewModel? CollectionViewModel => DataContext as MainWindowViewModel;
        WindowState previousWindowState;
        private static readonly FilePickerFileType fileOptions = new FilePickerFileType("All Images")
        {
            Patterns = new string[4] { "*.png", "*.jpg", "*.jpeg", "*.crdownload" }
        };
        private static readonly List<Series> CoverChangedSeriesList = [];
        private static readonly FileSystemWatcherEx CoverFolderWatcher = new FileSystemWatcherEx(@"Covers")
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName,
        };
        private static StringBuilder newSearchText = new StringBuilder();
        private static string itemString;

        public MainWindow()
        {
            InitializeComponent();
            SetupAdvancedSearchBar(" & ");

            CoverFolderWatcher.OnCreated += static (s, e) =>
            {
                string path = e.FullPath.Replace(".crdownload", "");
                if (!ViewModelBase.newCoverCheck && (!path.EndsWith(".jpg") || !path.EndsWith(".png")))
                {
                    Series series = MainWindowViewModel.SearchedCollection.AsParallel().Single(series => series.Cover.Equals(path));
                    if (series != null && !CoverChangedSeriesList.Contains(series))
                    {
                        CoverChangedSeriesList.Add(series);
                        LOGGER.Info($"Added \"{series.Titles["Romaji"]}\" to Cover Change List");
                    }
                    else
                    {
                        LOGGER.Info($"\"{series.Titles["Romaji"]}\" Cover Changed Again");
                    }
                }
            };
            CoverFolderWatcher.Start();

            KeyDown += (s, e) => 
            {
                if (e.Key == Key.F11) 
                {
                    if (WindowState != WindowState.FullScreen)
                    {
                        previousWindowState = WindowState;
                        WindowState = WindowState.FullScreen;
                    }
                    else if (WindowState == WindowState.FullScreen)
                    {
                        WindowState = previousWindowState;
                    }
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.P)
                {
                    LOGGER.Info("Saving Screenshot of Collection");
                    ScreenCaptureWindows();
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S)
                {
                    LOGGER.Info("Saving Collection");
                    CollectionViewModel.SearchText = "";
                    MainWindowViewModel.SaveUsersData();
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
                        LOGGER.Info("No Covers Changed");
                    }
                }
                else if (e.KeyModifiers == KeyModifiers.Shift && e.Key == Key.R)
                {
                    LOGGER.Info("Reloading Filter/Sort on Collection");
                    CollectionViewModel.FilterCollection(CollectionViewModel.CurFilter);
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.F)
                {
                    AdvancedSearchPopup.IsVisible ^= true;
                }
            };

            Closing += (s, e) =>
            {
                SaveOnClose();
            };
        }

        public void SetupAdvancedSearchBar(string delimeter)
        {
            AdvancedSearchBar.ItemsSource = ADVANCED_SEARCH_FILTERS;
            AdvancedSearchBar.FilterMode = AutoCompleteFilterMode.Custom;
            AdvancedSearchBar.MinimumPopulateDelay = new TimeSpan(TimeSpan.TicksPerSecond / 2); // Might just remove this delay in the end
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
            if (!string.IsNullOrWhiteSpace(CollectionViewModel.AdvancedSearchQueryErrorMessage))
            {
                CollectionViewModel.AdvancedSearchQueryErrorMessage = string.Empty;
            }
        }   

        /// <summary>
        /// Reloads Covers for Series where the cover was changed
        /// </summary>
        private static void ReloadCoverBitmaps()
        {
            uint cache = 0;
            for (int x = 0; x < MainWindowViewModel.UserCollection.Count; x++)
            {
                // If the image does not exist in the covers folder then don't create a bitmap for it
                if (CoverChangedSeriesList.Contains(MainWindowViewModel.UserCollection[x]))
                {
                    Bitmap newCover = new Bitmap(MainWindowViewModel.UserCollection[x].Cover).CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                    newCover.Save(MainWindowViewModel.UserCollection[x].Cover, 100);

                    MainWindowViewModel.UserCollection[x].CoverBitMap = newCover;
                    int index = MainWindowViewModel.SearchedCollection.IndexOf(MainWindowViewModel.UserCollection[x]);
                    MainWindowViewModel.SearchedCollection.Remove(MainWindowViewModel.UserCollection[x]);
                    MainWindowViewModel.SearchedCollection.Insert(index, MainWindowViewModel.UserCollection[x]);
                    cache++;
                    LOGGER.Info($"Updated Cover for \"{MainWindowViewModel.UserCollection[x].Titles["Romaji"]}\"");
                }
                else if (cache == CoverChangedSeriesList.Count)
                {
                    break;
                }
            }
            CoverChangedSeriesList.Clear();
        }

        /// <summary>
        /// Saves the stats for the series when the button is clicked
        /// </summary>
        private void SaveStats(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext;
            var stackPanels = ((Button)sender).GetLogicalSiblings();
            string volumesRead = ((MaskedTextBox)stackPanels.ElementAt(0).GetLogicalChildren().ElementAt(1)).Text.Replace("_", "");
            if (!string.IsNullOrWhiteSpace(volumesRead))
            {
                uint volumesReadVal = Convert.ToUInt32(volumesRead), countVolumesRead = 0;
                curSeries.VolumesRead = volumesReadVal;
                ((TextBlock)stackPanels.ElementAt(0).GetLogicalChildren().ElementAt(0)).Text = $"Read {volumesReadVal} Vol(s)";
                LOGGER.Info($"Updated # of Volumes Read for \"{curSeries.Titles["Romaji"]}\" to {volumesReadVal}");
                volumesReadVal = 0;
                foreach (Series x in CollectionsMarshal.AsSpan(MainWindowViewModel.UserCollection.ToList()))
                {
                    volumesReadVal += x.VolumesRead;
                    if (x.VolumesRead != 0)
                    {
                        countVolumesRead++;
                    }
                }
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.VolumesRead = volumesReadVal;
                ((MaskedTextBox)stackPanels.ElementAt(0).GetLogicalChildren().ElementAt(1)).Text = "";
            }

            string cost = ((MaskedTextBox)stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(1)).Text.Replace("_", "");
            if (!cost.Equals("."))
            {
                decimal costVal = Convert.ToDecimal(cost);
                curSeries.Cost = costVal;
                ((TextBlock)stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(0)).Text = $"Cost {CollectionViewModel.CurCurrency}{costVal}";
                LOGGER.Info($"Updated Cost for \"{curSeries.Titles["Romaji"]}\" to {CollectionViewModel.CurCurrency}{costVal}");
                costVal = 0;
                foreach (Series x in MainWindowViewModel.UserCollection)
                {
                    costVal += x.Cost;
                }
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice = $"{CollectionViewModel.CurCurrency}{decimal.Round(costVal, 2)}";
                ((MaskedTextBox)stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(1)).Text = "";
            }

            string rating = ((MaskedTextBox)stackPanels.ElementAt(2).GetLogicalChildren().ElementAt(1)).Text[..4].Replace("_", "");
            if (!rating.Equals("."))
            {
                decimal ratingVal = Convert.ToDecimal(rating);
                if (decimal.Compare(ratingVal, new decimal(10.0)) <= 0)
                {
                    int countrating = 0;
                    curSeries.Rating = ratingVal;
                    ((TextBlock)stackPanels.ElementAt(2).GetLogicalChildren().ElementAt(0)).Text = $"Rating {ratingVal}/10.0";
                    
                    // Update rating Distribution Chart
                    MainWindowViewModel.UserCollection.First(series => series == curSeries).Rating = ratingVal;
                    MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateRatingChartValues();

                    LOGGER.Info($"Updated rating for \"{curSeries.Titles["Romaji"]}\" to {ratingVal}/10.0");
                    ratingVal = 0;
                    foreach (Series x in CollectionsMarshal.AsSpan(MainWindowViewModel.UserCollection.ToList()))
                    {
                        if (x.Rating >= 0)
                        {
                            ratingVal += x.Rating;
                            countrating++;
                        }
                    }
                    MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.MeanRating = decimal.Round(ratingVal / countrating, 1);
                    ((MaskedTextBox)stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(1)).Text = "";
                }
                else
                {
                    LOGGER.Warn($"Rating Value {ratingVal} larger than 10.0");
                }
            }
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
                bitmap.Save(@$"Screenshots\{CollectionViewModel.UserName}-Collection-ScreenShot-{CollectionViewModel.CurrentTheme.ThemeName}.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private void SearchCollection(object sender, KeyEventArgs args)
        {
            MainWindowViewModel.UserIsSearching(true);
        }

        private async void ChangeSeriesVolumeCounts(object sender, RoutedEventArgs args)
        {
            var result = await Observable.Start(() => 
            {
                var textBoxes = ((Button)sender).GetLogicalSiblings();
                string curVolumeString = ((MaskedTextBox)textBoxes.ElementAt(1)).Text.Replace("_", "");
                string maxVolumeString = ((MaskedTextBox)textBoxes.ElementAt(2)).Text.Replace("_", "");
                Series curSeries = (Series)((Button)sender).DataContext;
                if (!string.IsNullOrWhiteSpace(curVolumeString) || !string.IsNullOrWhiteSpace(maxVolumeString))
                {
                    ushort curVolumeChange = Convert.ToUInt16(curVolumeString);
                    ushort maxVolumeChange = Convert.ToUInt16(maxVolumeString);
                    if (maxVolumeChange >= curVolumeChange)
                    {
                        MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected = MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected - curSeries.CurVolumeCount + curVolumeChange;
                        MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected = MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected - (uint)(curSeries.MaxVolumeCount - curSeries.CurVolumeCount) + (uint)(maxVolumeChange - curVolumeChange);
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
                    }
                    else
                    {
                        LOGGER.Warn($"{curVolumeChange} Is Not Less Than or Equal To {maxVolumeChange}");
                    }
                }
                else
                {
                    LOGGER.Warn("Change Series Volume Count has Empty Input");
                }
            }, RxApp.MainThreadScheduler);
        }

        private async void RemoveSeries(object sender, RoutedEventArgs args)
        {
            // Close the edit pane before removing
            (sender as Button).FindLogicalAncestorOfType<Grid>(false).IsVisible = false;
            var result = await Observable.Start(() => 
            {
                Series curSeries = MainWindowViewModel.UserCollection.Single(series => series == (Series)((Button)sender).DataContext);
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected -= curSeries.CurVolumeCount;
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected -= (uint)(curSeries.MaxVolumeCount - curSeries.CurVolumeCount);

                // Update Stats Window
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.VolumesRead -= curSeries.VolumesRead;
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice = $"{CollectionViewModel.CurCurrency}{decimal.Round(Convert.ToDecimal(MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice[CollectionViewModel.CurCurrency.Length..]) - curSeries.Cost, 2)}";

                MainWindowViewModel.SearchedCollection.Remove(curSeries);
                MainWindowViewModel.UserCollection.Remove(curSeries);

                if (File.Exists(curSeries.Cover))
                {
                    File.SetAttributes(curSeries.Cover, FileAttributes.Normal);
                    File.Delete(curSeries.Cover);
                    curSeries.Dispose();
                    LOGGER.Info($"Deleted Cover -> {curSeries.Cover}");
                }

                // Update Mean rating
                int countrating = 0;
                decimal ratingVal = 0;
                foreach (Series x in CollectionsMarshal.AsSpan(MainWindowViewModel.UserCollection.ToList()))
                {
                    if (x.Rating >= 0)
                    {
                        ratingVal += x.Rating;
                        countrating++;
                    }
                }
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.MeanRating = countrating != 0 ? decimal.Round(ratingVal / countrating, 1) : 0;
                
                LOGGER.Info($"Removed \"{curSeries.Titles["Romaji"]}\" From Collection");
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.SeriesCount = (uint)MainWindowViewModel.UserCollection.Count;

                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicChartValues();
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicPercentages();
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateStatusChartValues();
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateStatusPercentages();
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateRatingChartValues();
            }, RxApp.MainThreadScheduler);
        }

        private void ShowEditPane(object sender, RoutedEventArgs args)
        {
            if (((sender as Button).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible)
            {
                ((sender as Button).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible = false;
            }
            ((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible ^= true;
        }

        private void ShowStatsPane(object sender, RoutedEventArgs args)
        {
            if (((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible)
            {
                ((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible = false;
            }
            ((sender as Button).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible ^= true;
        }

        /// <summary>
        /// Changes the language for the users collection
        /// </summary>
        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                if (!string.IsNullOrWhiteSpace(CollectionViewModel.SearchText)) // Checks if the user is filtering after a search
                {
                    // CollectionViewModel.SearchIsBusy = false;
                    CollectionViewModel.SearchText = string.Empty;
                }
                CollectionViewModel.CurLanguage = (LanguageSelector.SelectedItem as ComboBoxItem).Content.ToString();
                LOGGER.Info($"Changed Langauge to {CollectionViewModel.CurLanguage}");
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
                CollectionViewModel.UpdateCurFilter(CollectionFilterSelector.SelectedItem as ComboBoxItem);
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
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicChartValues();
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicPercentages();
                
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
        //             CollectionViewModel.CurDisplay = "Card";
        //         }
        //         else if (display.Equals("Mini-Card"))
        //         {
        //             CollectionViewModel.CurDisplay = "Mini-Card";
        //         }
        //         LOGGER.Info($"Changed Display To {CollectionViewModel.CurDisplay}");
        //     }
        // }

        public void SaveOnClose()
        {
            LOGGER.Info("Closing Tsundoku");
            CollectionViewModel.SearchText = "";
            MainWindowViewModel.SaveUsersData();
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

            NLog.LogManager.Shutdown();
            App.DisposeMutex();
        }

        /// <summary>
        /// Opens the AniList or MangaDex website link in the users browser when users clicks on the left side of the series card
        /// </summary>
        private void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            ViewModelBase.OpenSiteLink(((sender as Canvas).DataContext as Series).Link.ToString());
        }
        
        /// <summary>
        /// Allows the user to pick a image file to for their icon
        /// </summary>
        private async void ChangeUserIcon(object sender, PointerPressedEventArgs args)
        {
            var file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>() { fileOptions }
            });
            if (file.Count > 0)
            {
                CollectionViewModel.UserIcon = new Bitmap(file[0].Path.LocalPath).CreateScaledBitmap(new PixelSize(USER_ICON_WIDTH, USER_ICON_HEIGHT), BitmapInterpolationMode.HighQuality);
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
                Process.Start(new ProcessStartInfo("https://www.paypal.com/donate/?business=JAYCVEJGDF4GY&no_recurring=0&item_name=Anyone+amount+helps+and+keeps+the+app+going.&currency_code=USD") { UseShellExecute = true });
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
            LOGGER.Info($"Copying {title} to Clipboard");
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
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected++;
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected--;
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
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected--;
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected++;
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
