using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using FileWatcherEx;
using Avalonia.Logging;

namespace Tsundoku.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindowViewModel? CollectionViewModel => DataContext as MainWindowViewModel;
        private WindowState previousWindowState;
        private static readonly List<FilePickerFileType> fileOptions = [FilePickerFileTypes.ImageAll];
        private static readonly List<Series> CoverChangedSeriesList = [];
        private static readonly FileSystemWatcherEx coverFolderWatcher = new(@"Covers")
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName
        };

        public MainWindow()
        {
            InitializeComponent();
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
                    if (CollectionViewModel.CurFilter != "None")
                    {
                        MainWindowViewModel.FilterCollection("None");
                    }
                    
                    if (CoverChangedSeriesList.Count != 0)
                    {
                        ReloadCoverBitmaps();
                    }
                    else
                    {
                        LOGGER.Info("No Covers Changed");
                    }
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.F)
                {
                    LOGGER.Info("Reloading Filter/Sort on Collection");
                    MainWindowViewModel.FilterCollection(CollectionViewModel.CurFilter);
                }
            };

            coverFolderWatcher.OnCreated += static (s, e) =>
            {
                e.FullPath = e.FullPath.Replace(".crdownload", "");
                if (!(!ViewModelBase.updatedVersion || (!e.FullPath.EndsWith(".jpg") && !e.FullPath.EndsWith(".png"))))
                {
                    Series series = MainWindowViewModel.UserCollection.SingleOrDefault(series => series.Cover.Equals(e.FullPath));
                    if (series != null && !CoverChangedSeriesList.Contains(series))
                    {
                        CoverChangedSeriesList.Add(series);
                        LOGGER.Info($"{e.FullPath} Changed");
                    }
                    else
                    {
                        LOGGER.Info($"{e.FullPath} Changed Again");
                    }
                }
            };
            coverFolderWatcher.Start();

            Closing += (s, e) =>
            {
                SaveOnClose();
            };
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
                    int index = MainWindowViewModel.SearchedCollection.IndexOf(MainWindowViewModel.UserCollection[x]);
                    MainWindowViewModel.SearchedCollection.Remove(MainWindowViewModel.UserCollection[x]);
                    Bitmap newCover = new Bitmap(MainWindowViewModel.UserCollection[x].Cover).CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                    newCover.Save(MainWindowViewModel.UserCollection[x].Cover, 100);
                    MainWindowViewModel.UserCollection[x].CoverBitMap = newCover;
                    MainWindowViewModel.SearchedCollection.Insert(index, MainWindowViewModel.UserCollection[x]);
                    cache++;
                    LOGGER.Info($"Updated {MainWindowViewModel.UserCollection[x].Titles["Romaji"]} Cover");
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
        public void SaveStats(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext;
            var stackPanels = ((Button)sender).GetLogicalSiblings();
            string volumesRead = ((MaskedTextBox)stackPanels.ElementAt(0).GetLogicalChildren().ElementAt(1)).Text.Replace("_", "");
            if (!string.IsNullOrWhiteSpace(volumesRead))
            {
                uint volumesReadVal = Convert.ToUInt32(volumesRead), countVolumesRead = 0;
                curSeries.VolumesRead = volumesReadVal;
                ((TextBlock)stackPanels.ElementAt(0).GetLogicalChildren().ElementAt(0)).Text = $"Read {volumesReadVal} Vol(s)";
                LOGGER.Info($"Updated # of Volumes Read for {curSeries.Titles["Romaji"]} to {volumesReadVal}");
                volumesReadVal = 0;
                foreach (Series x in CollectionsMarshal.AsSpan(MainWindowViewModel.UserCollection.ToList()))
                {
                    volumesReadVal += x.VolumesRead;
                    if (x.VolumesRead != 0)
                    {
                        countVolumesRead++;
                    }
                }
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.VolumesRead = volumesReadVal;
                ((MaskedTextBox)stackPanels.ElementAt(0).GetLogicalChildren().ElementAt(1)).Text = "";
            }

            string cost = ((MaskedTextBox)stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(1)).Text.Replace("_", "");
            if (!cost.Equals("."))
            {
                decimal costVal = Convert.ToDecimal(cost);
                curSeries.Cost = costVal;
                ((TextBlock)stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(0)).Text = $"Cost {CollectionViewModel.CurCurrency}{costVal}";
                LOGGER.Info($"Updated Cost for {curSeries.Titles["Romaji"]} to {CollectionViewModel.CurCurrency}{costVal}");
                costVal = 0;
                foreach (Series x in MainWindowViewModel.UserCollection)
                {
                    costVal += x.Cost;
                }
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice = $"{CollectionViewModel.CurCurrency}{decimal.Round(costVal, 2)}";
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
                    ((TextBlock)stackPanels.ElementAt(2).GetLogicalChildren().ElementAt(0)).Text = $"rating {ratingVal}/10.0";
                    
                    // Update rating Distribution Chart
                    MainWindowViewModel.UserCollection.First(series => series == curSeries).Rating = ratingVal;
                    CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateRatingChartValues();

                    LOGGER.Info($"Updated rating for {curSeries.Titles["Romaji"]} to {ratingVal}/10.0");
                    ratingVal = 0;
                    foreach (Series x in CollectionsMarshal.AsSpan(MainWindowViewModel.UserCollection.ToList()))
                    {
                        if (x.Rating >= 0)
                        {
                            ratingVal += x.Rating;
                            countrating++;
                        }
                    }
                    CollectionViewModel.collectionStatsWindow.CollectionStatsVM.MeanRating = decimal.Round(ratingVal / countrating, 1);
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
            CollectionViewModel.SearchIsBusy = true;
        }

        private async void ChangeSeriesVolumeCountsAsync(object sender, RoutedEventArgs args)
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
                        CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected = CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected - curSeries.CurVolumeCount + curVolumeChange;
                        CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected = CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected - (uint)(curSeries.MaxVolumeCount - curSeries.CurVolumeCount) + (uint)(maxVolumeChange - curVolumeChange);
                        LOGGER.Info($"Changed Series Values For {curSeries.Titles["Romaji"]} From {curSeries.CurVolumeCount}/{curSeries.MaxVolumeCount} -> {curVolumeChange}/{maxVolumeChange}");
                        curSeries.CurVolumeCount = curVolumeChange;
                        curSeries.MaxVolumeCount = maxVolumeChange;

                        var parentControl = (sender as Button).FindLogicalAncestorOfType<DockPanel>(false).GetLogicalChildren().ElementAt(1).GetLogicalChildren().ElementAt(3);
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
            var result = await Observable.Start(() => 
            {
                Series curSeries = MainWindowViewModel.UserCollection.Single(series => series == (Series)((Button)sender).DataContext);
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected -= curSeries.CurVolumeCount;
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected -= (uint)(curSeries.MaxVolumeCount - curSeries.CurVolumeCount);

                // Update Stats Window
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.VolumesRead -= curSeries.VolumesRead;
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice = $"{CollectionViewModel.CurCurrency}{decimal.Round(Convert.ToDecimal(CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice[CollectionViewModel.CurCurrency.Length..]) - curSeries.Cost, 2)}";

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
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.MeanRating = countrating != 0 ? decimal.Round(ratingVal / countrating, 1) : 0;
                
                LOGGER.Info($"Removed {curSeries.Titles["Romaji"]} From Collection");
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.SeriesCount = (uint)MainWindowViewModel.UserCollection.Count;

                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicChartValues();
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicPercentages();
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateStatusChartValues();
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateStatusPercentages();
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateRatingChartValues();
            }, RxApp.MainThreadScheduler);
        }

        private void ShowEditPane(object sender, RoutedEventArgs args)
        {
            if (((sender as Button).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible == true)
            {
                ((sender as Button).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible = false;
            }
            ((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible ^= true;
        }

        private void ShowStatsPane(object sender, RoutedEventArgs args)
        {
            if (((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible == true)
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
                CollectionViewModel.CurFilter = (CollectionFilterSelector.SelectedItem as ComboBoxItem).Content.ToString();
                MainWindowViewModel.FilterCollection(CollectionViewModel.CurFilter);
                LOGGER.Info($"Changed Collection Filter To {CollectionViewModel.CurFilter}");
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
                string demographic = (sender as ComboBox).SelectedItem as string;
                
                // Update Demographic Chart Values
                MainWindowViewModel.UserCollection.First(series => series == curSeries).Demographic = demographic;
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicChartValues();
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicPercentages();
                
                curSeries.Demographic = demographic;
                LOGGER.Info($"Changed Demographic for {curSeries.Titles["Romaji"]} to {demographic}");
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
            CollectionViewModel.SearchText = "";
            Helpers.DiscordRP.Deinitialize();
            coverFolderWatcher.Dispose();
            LOGGER.Info("Closing Tsundoku");

            if (CollectionViewModel.newSeriesWindow != null)
            {
                CollectionViewModel.newSeriesWindow.Closing += (s, e) => { e.Cancel = false; };
                CollectionViewModel.newSeriesWindow.Close();
            }

            if (CollectionViewModel.settingsWindow != null)
            {
                CollectionViewModel.settingsWindow.Closing += (s, e) => { e.Cancel = false; };
                CollectionViewModel.settingsWindow.Close();
            }

            if (CollectionViewModel.themeSettingsWindow != null)
            {
                CollectionViewModel.themeSettingsWindow.Closing += (s, e) => { e.Cancel = false; };
                CollectionViewModel.themeSettingsWindow.Close();
            }

            if (CollectionViewModel.priceAnalysisWindow != null)
            {
                CollectionViewModel.priceAnalysisWindow.Closing += (s, e) => { e.Cancel = false; };
                CollectionViewModel.priceAnalysisWindow.Close();
            }

            if (CollectionViewModel.collectionStatsWindow != null)
            {
                CollectionViewModel.collectionStatsWindow.Closing += (s, e) => { e.Cancel = false; };
                CollectionViewModel.collectionStatsWindow.Close();
            }

            // Move the users current theme to the front of the list so when opening again it applies the correct theme
            ThemeSettingsViewModel.UserThemes.Move(ThemeSettingsViewModel.UserThemes.IndexOf(ThemeSettingsViewModel.UserThemes.Single(x => x.ThemeName == ViewModelBase.MainUser.MainTheme)), 0);

            MainWindowViewModel.SaveUsersData();
            App.Mutex.Dispose();
        }

        /// <summary>
        /// Opens the AniList or MangaDex website link in the users browser when users clicks on the left side of the series card
        /// </summary>
        private void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            ViewModelBase.OpenSiteLink(((Canvas)sender).Name);
        }
        
        /// <summary>
        /// Allows the user to pick a image file to for their icon
        /// </summary>
        private async void ChangeUserIcon(object sender, PointerPressedEventArgs args)
        {
            var file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                AllowMultiple = false,
                FileTypeFilter = fileOptions
            });
            if (file.Count > 0)
            {
                CollectionViewModel.UserIcon = new Bitmap(file[0].Path.LocalPath).CreateScaledBitmap(new PixelSize(USER_ICON_WIDTH, USER_ICON_HEIGHT), BitmapInterpolationMode.HighQuality);
                LOGGER.Debug($"Changed Users Icon to {file[0].Path.LocalPath}");
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
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected++;
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected--;
                curSeries.CurVolumeCount += 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).GetLogicalSiblings().ElementAt(1);
                string log = $"Adding 1 Volume To {curSeries.Titles["Romaji"]} : {volumeDisplay.Text} -> ";
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
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected--;
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected++;
                curSeries.CurVolumeCount -= 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).GetLogicalSiblings().ElementAt(1);
                string log = $"Removing 1 Volume From {curSeries.Titles["Romaji"]} : {volumeDisplay.Text} -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                LOGGER.Info(log + volumeDisplay.Text);

                (sender as Button).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<ProgressBar>(false).Value = curSeries.CurVolumeCount;
            }
        }
    }
}
