using Avalonia;
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
using Avalonia.Media.Imaging;

namespace Tsundoku.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public MainWindowViewModel? CollectionViewModel => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += (s, e) => 
            {
                if (e.Key == Key.F11) 
                {
                    if (this.WindowState != WindowState.FullScreen)
                    {
                        this.WindowState = WindowState.FullScreen;
                    }
                    else if (this.WindowState == WindowState.FullScreen)
                    {
                        this.WindowState = WindowState.Maximized;
                    }
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.P)
                {
                    Logger.Info("Saving Screenshot of Collection");
                    ScreenCaptureWindows();
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S)
                {
                    Logger.Info("Saving Collection");
                    MainWindowViewModel.SaveUsersData();
                }
            };

        }

        private void ScreenCaptureWindows()
        {
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)this.Width, (int)this.Height))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new System.Drawing.Point((int)this.Bounds.Left, (int)this.Bounds.Top), System.Drawing.Point.Empty, new System.Drawing.Size((int)this.Width, (int)this.Height));
                }
                bitmap.Save(@$"{MainWindowViewModel.MainUser.UserName}-Collection-ScreenShot.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
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
                ushort curVolumeChange = Convert.ToUInt16(((MaskedTextBox)textBoxes.ElementAt(1)).Text.Replace("_", ""));
                ushort maxVolumeChange = Convert.ToUInt16(((MaskedTextBox)textBoxes.ElementAt(2)).Text.Replace("_", ""));
                Series curSeries = (Series)((Button)sender).DataContext;
                if (maxVolumeChange >= curVolumeChange)
                {
                    CollectionViewModel.UsersNumVolumesCollected = CollectionViewModel.UsersNumVolumesCollected - curSeries.CurVolumeCount + curVolumeChange;
                    CollectionViewModel.UsersNumVolumesToBeCollected = CollectionViewModel.UsersNumVolumesToBeCollected - (uint)(curSeries.MaxVolumeCount - curSeries.CurVolumeCount) + (uint)(maxVolumeChange - curVolumeChange);
                    Logger.Info($"Changed Series Values For {curSeries.Titles[0]} From {curSeries.CurVolumeCount}/{curSeries.MaxVolumeCount} -> {curVolumeChange}/{maxVolumeChange}");
                    curSeries.CurVolumeCount = curVolumeChange;
                    curSeries.MaxVolumeCount = maxVolumeChange;
                    
                    var parentControl = (sender as Button).FindLogicalAncestorOfType<Border>(false).GetLogicalChildren().ElementAt(0).GetLogicalChildren().ElementAt(2);
                    parentControl.FindLogicalDescendantOfType<TextBlock>(false).Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                    
                    ProgressBar seriesProgressBar = parentControl.FindLogicalDescendantOfType<ProgressBar>(false);
                    seriesProgressBar.Maximum = maxVolumeChange;
                    seriesProgressBar.Value = curVolumeChange;
                    parentControl = null;
                    seriesProgressBar = null;  
                }
            }, RxApp.MainThreadScheduler);
        }

        private async void RemoveSeries(object sender, RoutedEventArgs args)
        {
            var result = await Observable.Start(() => 
            {
                Series curSeries = (Series)MainWindowViewModel.Collection.Single(series => series == (Series)((Button)sender).DataContext);
                CollectionViewModel.UsersNumVolumesCollected -= curSeries.CurVolumeCount;
                CollectionViewModel.UsersNumVolumesToBeCollected -= (uint)(curSeries.MaxVolumeCount - curSeries.CurVolumeCount);
                File.Delete(curSeries.Cover);
                Logger.Info($"Deleted Cover -> {curSeries.Cover}");
                MainWindowViewModel.SearchedCollection.Remove(curSeries);
                MainWindowViewModel.Collection.Remove(curSeries);
                curSeries.Dispose();
                Logger.Info($"Removed {curSeries.Titles[0]} From Collection");
            }, RxApp.MainThreadScheduler);
        }

        private async void ShowEditPane(object sender, RoutedEventArgs args)
        {
            var result = await Observable.Start(() => 
            {
                ((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible ^= true;
            }, RxApp.MainThreadScheduler);
        }

        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                switch (LanguageSelector.SelectedItem)
                {
                    case "Native":
                        CollectionViewModel.CurLanguage = "Native";
                        break;
                    case "English":
                        CollectionViewModel.CurLanguage = "English";
                        break;
                    default:
                        CollectionViewModel.CurLanguage = "Romaji";
                        break;
                }
                Logger.Info($"Changed Langauge to {CollectionViewModel.CurLanguage}");
                MainWindowViewModel.SortCollection();
            }
        }

        private void DisplayChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                string display = (sender as ComboBox).SelectedItem as string;
                if (display.Equals("Card"))
                {
                    CollectionViewModel.CurDisplay = "Card";
                }
                else if (display.Equals("Mini-Card"))
                {
                    CollectionViewModel.CurDisplay = "Mini-Card";
                }
                Logger.Info($"Changed Display To {CollectionViewModel.CurDisplay}");
            }
        }

        public void SaveOnClose(object sender, CancelEventArgs e)
        {
            CollectionViewModel.SearchText = "";
            Src.DiscordRP.Deinitialize();
            Logger.Info("Closing Tsundoku");

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

            // Move the users current theme to the front of the list so when opening again it applies the correct theme
            ThemeSettingsViewModel.UserThemes.Move(ThemeSettingsViewModel.UserThemes.IndexOf(ThemeSettingsViewModel.UserThemes.Single(x => x.ThemeName == MainWindowViewModel.MainUser.MainTheme)), 0);

            MainWindowViewModel.SaveUsersData();
        }

        private void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            string link = ((Canvas)sender).Name;
            Logger.Info($"Opening Link {link}");
            try
            {
                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch (Win32Exception noBrowser)
            {
                Logger.Error(noBrowser.Message);
            }
            catch (Exception other)
            {
                Logger.Error(other.Message);
            }
        }

        private async void CopySeriesTitleAsync(object sender, PointerPressedEventArgs args)
        {
            string title = ((TextBlock)sender).Text;
            Logger.Info($"Copying {title} to Clipboard");
            await Application.Current.Clipboard.SetTextAsync(title);
        }

        /*
         Decrements the series current volume count
        */
        public void AddVolume(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext;
            if (curSeries.CurVolumeCount < curSeries.MaxVolumeCount)
            {
                CollectionViewModel.UsersNumVolumesCollected++;
                CollectionViewModel.UsersNumVolumesToBeCollected--;
                curSeries.CurVolumeCount += 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).Parent.GetLogicalSiblings().ElementAt(0);
                string log = "Adding 1 Volume To " + curSeries.Titles[0] + " : " + volumeDisplay.Text + " -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                Logger.Info(log + volumeDisplay.Text);

                (sender as Button).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<ProgressBar>(false).Value = curSeries.CurVolumeCount;
            }
        }

        /*
         Decrements the series current volume count
        */
        public void SubtractVolume(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext; //Get the current series data
            if (curSeries.CurVolumeCount >= 1) //Only decrement if the user currently has 1 or more volumes
            {
                CollectionViewModel.UsersNumVolumesCollected--;
                CollectionViewModel.UsersNumVolumesToBeCollected++;
                curSeries.CurVolumeCount -= 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).Parent.GetLogicalSiblings().ElementAt(0);
                string log = "Removing 1 Volume From " + curSeries.Titles[0] + " : " + volumeDisplay.Text + " -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                Logger.Info(log + volumeDisplay.Text);

                (sender as Button).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<ProgressBar>(false).Value = curSeries.CurVolumeCount;
            }
        }
    }
}
