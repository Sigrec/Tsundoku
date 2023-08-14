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

namespace Tsundoku.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindowViewModel? CollectionViewModel => DataContext as MainWindowViewModel;
        private WindowState previousWindowState;
        private static readonly List<FilePickerFileType> fileOptions = new() { FilePickerFileTypes.ImageAll };

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
                    Constants.Logger.Info("Saving Screenshot of Collection");
                    ScreenCaptureWindows();
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S)
                {
                    Constants.Logger.Info("Saving Collection");
                    CollectionViewModel.SearchText = "";
                    MainWindowViewModel.SaveUsersData();
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.R)
                {
                    Constants.Logger.Info("Reloading Collection");
                    if (CollectionViewModel.CurFilter != "None")
                    {
                        MainWindowViewModel.FilterCollection("None");
                    }
                    MainWindowViewModel.AllocateCoverBitmaps();
                }
            };

            Closing += (s, e) =>
            {
                SaveOnClose();
            };
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
                Constants.Logger.Info($"Updated # of Volumes Read for {curSeries.Titles["Romaji"]} to {volumesReadVal}");
                volumesReadVal = 0;
                foreach (Series x in CollectionsMarshal.AsSpan(MainWindowViewModel.Collection.ToList()))
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
                Constants.Logger.Info($"Updated Cost for {curSeries.Titles["Romaji"]} to {CollectionViewModel.CurCurrency}{costVal}");
                costVal = 0;
                foreach (Series x in CollectionsMarshal.AsSpan(MainWindowViewModel.Collection.ToList()))
                {
                    costVal += x.Cost;
                }
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice = $"{CollectionViewModel.CurCurrency}{decimal.Round(costVal, 2)}";
                ((MaskedTextBox)stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(1)).Text = "";
            }

            string score = ((MaskedTextBox)stackPanels.ElementAt(2).GetLogicalChildren().ElementAt(1)).Text[..4].Replace("_", "");
            if (!score.Equals("."))
            {
                decimal scoreVal = Convert.ToDecimal(score);
                if (decimal.Compare(scoreVal, new decimal(10.0)) <= 0)
                {
                    int countScore = 0;
                    curSeries.Score = scoreVal;
                    ((TextBlock)stackPanels.ElementAt(2).GetLogicalChildren().ElementAt(0)).Text = $"Score {scoreVal}/10.0";
                    
                    // Update Score Distribution Chart
                    MainWindowViewModel.Collection.First(series => series == curSeries).Score = scoreVal;
                    CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateScoreChartValues();

                    Constants.Logger.Info($"Updated Score for {curSeries.Titles["Romaji"]} to {scoreVal}/10.0");
                    scoreVal = 0;
                    foreach (Series x in CollectionsMarshal.AsSpan(MainWindowViewModel.Collection.ToList()))
                    {
                        if (x.Score >= 0)
                        {
                            scoreVal += x.Score;
                            countScore++;
                        }
                    }
                    CollectionViewModel.collectionStatsWindow.CollectionStatsVM.MeanScore = decimal.Round(scoreVal / countScore, 1);
                    ((MaskedTextBox)stackPanels.ElementAt(1).GetLogicalChildren().ElementAt(1)).Text = "";
                }
                else
                {
                    Constants.Logger.Warn($"Score Value {scoreVal} larger than 10.0");
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
                        Constants.Logger.Info($"Changed Series Values For {curSeries.Titles["Romaji"]} From {curSeries.CurVolumeCount}/{curSeries.MaxVolumeCount} -> {curVolumeChange}/{maxVolumeChange}");
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
                        Constants.Logger.Warn($"{curVolumeChange} Is Not Less Than or Equal To {maxVolumeChange}");
                    }
                }
                else
                {
                    Constants.Logger.Warn("Change Series Volume Count has Empty Input");
                }
            }, RxApp.MainThreadScheduler);
        }

        private async void RemoveSeries(object sender, RoutedEventArgs args)
        {
            var result = await Observable.Start(() => 
            {
                Series curSeries = MainWindowViewModel.Collection.Single(series => series == (Series)((Button)sender).DataContext);
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesCollected -= curSeries.CurVolumeCount;
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UsersNumVolumesToBeCollected -= (uint)(curSeries.MaxVolumeCount - curSeries.CurVolumeCount);

                // Update Stats Window
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.VolumesRead -= curSeries.VolumesRead;
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice = $"{CollectionViewModel.CurCurrency}{decimal.Round(Convert.ToDecimal(CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice[CollectionViewModel.CurCurrency.Length..]) - curSeries.Cost, 2)}";

                MainWindowViewModel.SearchedCollection.Remove(curSeries);
                MainWindowViewModel.Collection.Remove(curSeries);

                if (File.Exists(curSeries.Cover))
                {
                    File.SetAttributes(curSeries.Cover, FileAttributes.Normal);
                    File.Delete(curSeries.Cover);
                    curSeries.Dispose();
                    Constants.Logger.Info($"Deleted Cover -> {curSeries.Cover}");
                }

                // Update Mean Score
                int countScore = 0;
                decimal scoreVal = 0;
                foreach (Series x in CollectionsMarshal.AsSpan(MainWindowViewModel.Collection.ToList()))
                {
                    if (x.Score >= 0)
                    {
                        scoreVal += x.Score;
                        countScore++;
                    }
                }
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.MeanScore = countScore != 0 ? decimal.Round(scoreVal / countScore, 1) : 0;
                
                Constants.Logger.Info($"Removed {curSeries.Titles["Romaji"]} From Collection");
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.SeriesCount = (uint)MainWindowViewModel.Collection.Count;

                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicChartValues();
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicPercentages();

                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateStatusChartValues();
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateStatusPercentages();

                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateScoreChartValues();
            }, RxApp.MainThreadScheduler);
        }

        private async void ShowEditPane(object sender, RoutedEventArgs args)
        {
            var result = await Observable.Start(() => 
            {
                if (((sender as Button).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible == true)
                {
                    ((sender as Button).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible = false;
                }
                ((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible ^= true;
            }, RxApp.MainThreadScheduler);
        }

        private async void ShowStatsPane(object sender, RoutedEventArgs args)
        {
            var result = await Observable.Start(() => 
            {
                if (((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible == true)
                {
                    ((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible = false;
                }
                ((sender as Button).FindLogicalAncestorOfType<Grid>(false).GetLogicalChildren().ElementAt(1) as Grid).IsVisible ^= true;
            }, RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Changes the language for the users collection
        /// </summary>
        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                CollectionViewModel.CurLanguage = (LanguageSelector.SelectedItem as ComboBoxItem).Content.ToString();
                Constants.Logger.Info($"Changed Langauge to {CollectionViewModel.CurLanguage}");
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
                Constants.Logger.Info($"Changed Collection Filter To {CollectionViewModel.CurFilter}");
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
                MainWindowViewModel.Collection.First(series => series == curSeries).Demographic = demographic;
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicChartValues();
                CollectionViewModel.collectionStatsWindow.CollectionStatsVM.UpdateDemographicPercentages();
                
                curSeries.Demographic = demographic;
                Constants.Logger.Info($"Changed Demographic for {curSeries.Titles["Romaji"]} to {demographic}");
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
        //         Constants.Logger.Info($"Changed Display To {CollectionViewModel.CurDisplay}");
        //     }
        // }

        public void SaveOnClose()
        {
            CollectionViewModel.SearchText = "";
            Helpers.DiscordRP.Deinitialize();
            Constants.Logger.Info("Closing Tsundoku");

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
        }

        /// <summary>
        /// Opens the AniList or MangaDex website link in the users browser when users clicks on the left side of the series card
        /// </summary>
        private void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            string link = ((Canvas)sender).Name;
            Constants.Logger.Info($"Opening Link {link}");
            try
            {
                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch (Win32Exception noBrowser)
            {
                Constants.Logger.Error(noBrowser.Message);
            }
            catch (Exception other)
            {
                Constants.Logger.Error(other.Message);
            }
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
                CollectionViewModel.UserIcon = new Bitmap(file[0].Path.LocalPath).CreateScaledBitmap(new Avalonia.PixelSize(Constants.USER_ICON_WIDTH, Constants.USER_ICON_HEIGHT), BitmapInterpolationMode.HighQuality);
                Constants.Logger.Debug($"Changed Users Icon to {file[0].Path.LocalPath}");
            }
        }

        /// <summary>
        /// Opens my PayPal for users to donate if they want to
        /// </summary>
        private void OpenDonationLink(object sender, RoutedEventArgs args)
        {
            Constants.Logger.Info($"Opening PayPal Donation Lionk");
            try
            {
                Process.Start(new ProcessStartInfo("https://www.paypal.com/donate/?business=JAYCVEJGDF4GY&no_recurring=0&item_name=Anyone+amount+helps+and+keeps+the+app+going.&currency_code=USD") { UseShellExecute = true });
            }
            catch (Win32Exception noBrowser)
            {
                Constants.Logger.Error(noBrowser.Message);
            }
            catch (Exception other)
            {
                Constants.Logger.Error(other.Message);
            }
        }

        /// <summary>
        /// Copies the title of the series when clicked om
        /// </summary>
        private async void CopySeriesTitleAsync(object sender, PointerPressedEventArgs args)
        {
            string title = ((TextBlock)sender).Text;
            Constants.Logger.Info($"Copying {title} to Clipboard");
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
                string log = "Adding 1 Volume To " + curSeries.Titles["Romaji"] + " : " + volumeDisplay.Text + " -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                Constants.Logger.Info(log + volumeDisplay.Text);

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
                string log = "Removing 1 Volume From " + curSeries.Titles["Romaji"] + " : " + volumeDisplay.Text + " -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                Constants.Logger.Info(log + volumeDisplay.Text);

                (sender as Button).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<ProgressBar>(false).Value = curSeries.CurVolumeCount;
            }
        }
    }
}
