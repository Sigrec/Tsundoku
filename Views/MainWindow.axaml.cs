using System.Collections.ObjectModel;
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
using System.Text.RegularExpressions;
using System.IO;
using ReactiveUI;
using System.Threading.Tasks;
using System.Reactive.Concurrency;

namespace Tsundoku.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public MainWindowViewModel CollectionViewModel => DataContext as MainWindowViewModel;
        private bool SeriesEditPaneIsOpenView = false;
        
        public MainWindow()
        {
            InitializeComponent();
           //RxApp.MainThreadScheduler.Schedule(() => ChangeSeriesVolumeCountsAsync());
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
                ushort maxVolumeChange = Convert.ToUInt16(((MaskedTextBox)textBoxes.ElementAt(1)).Text.Replace("_", ""));
                ushort curVolumeChange = Convert.ToUInt16(((MaskedTextBox)textBoxes.ElementAt(2)).Text.Replace("_", ""));
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

        private void RemoveSeries(object sender, RoutedEventArgs args)
        {
            for (int x = 0; x < MainWindowViewModel.SearchedCollection.Count(); x++)
            {
                Series curSeries = MainWindowViewModel.SearchedCollection[x];
                if (curSeries.Equals((Series)((Button)sender).DataContext))
                {
                    CollectionViewModel.UsersNumVolumesCollected -= curSeries.CurVolumeCount;
                    CollectionViewModel.UsersNumVolumesToBeCollected -= (uint)(curSeries.MaxVolumeCount - curSeries.CurVolumeCount);
                    MainWindowViewModel.SearchedCollection.Remove(curSeries);
                    MainWindowViewModel.Collection.Remove(curSeries);
                    Logger.Info($"Removed {curSeries.Titles[0]} From Collection");
                }
            }
        }

        private void ShowEditPane(object sender, RoutedEventArgs args)
        {
            ((Button)sender).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).IsVisible = (SeriesEditPaneIsOpenView ^= true);
        }

        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
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
            if ((sender as ComboBox).IsDropDownOpen)
            {
                MainWindowViewModel.SortCollection();
            }
        }

        private void DisplayChanged(object sender, SelectionChangedEventArgs e)
        {
            string display = (sender as ComboBox).SelectedItem as string;
            if (display.Equals("Card"))
            {
                MainWindowViewModel._curDisplay = "Card";
            }
            else if (display.Equals("Mini-Card"))
            {
                MainWindowViewModel._curDisplay = "Mini-Card";
            }
            Logger.Info($"Changed Display To {MainWindowViewModel._curDisplay}");
        }

        public static string Slice(string source, int start, int end)
        {
            if (end < 0) // Keep this for negative end support
            {
                end = source.Length + end;
            }
            int len = end - start;               // Calculate length
            return source.Substring(start, len); // Return Substring of length
        }

        public void SaveOnCloseTwo()
        {
            Logger.Info("Closing TsundOku");
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

            // Cleans the Covers asset folder of images for series that is not in the users collection on close/save
            bool removeSeriesCheck = true;
            foreach (string coverPath in Directory.GetFiles(@"Assets\Covers"))
            {
                int underscoreIndex = coverPath.IndexOf("_");
                int periodIndex = coverPath.IndexOf(".");
                foreach (Series curSeries in MainWindowViewModel.Collection)
                {
                    string curTitle = Regex.Replace(curSeries.Titles[0], @"[^A-Za-z\d]", "");
                    string coverPathTitleAndFormat = coverPath.Substring(14);
                    if (Slice(coverPathTitleAndFormat, 0, coverPathTitleAndFormat.IndexOf("_")).Equals(curTitle) && Slice(coverPathTitleAndFormat, coverPathTitleAndFormat.IndexOf("_") + 1, coverPathTitleAndFormat.IndexOf(".")).Equals(curSeries.Format.ToUpper()))
                    {
                        removeSeriesCheck = false;
                        break;
                    }
                }

                if (removeSeriesCheck)
                {
                    Logger.Info($"Deleted Cover -> {coverPath}");
                    File.Delete(coverPath);
                }
                removeSeriesCheck = true;
            }
            
            ThemeSettingsViewModel.UserThemes.Move(ThemeSettingsViewModel.UserThemes.IndexOf(ThemeSettingsViewModel.UserThemes.Single(x => x.ThemeName == MainWindowViewModel.MainUser.MainTheme)), 0);

            MainWindowViewModel.SaveUsersData();
        }

        public void SaveOnClose(object sender, CancelEventArgs e)
        {
            SaveOnCloseTwo();
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
                // Uncomment when fix is implemented
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
