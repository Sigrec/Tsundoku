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
using System.Threading.Tasks;
using System.Reactive;

namespace Tsundoku.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private MainWindowViewModel CollectionViewModel => DataContext as MainWindowViewModel;
        private bool SeriesEditPaneIsOpenView = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChangeSeriesVolumeCounts(object sender, RoutedEventArgs args)
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
                TextBlock volumeDisplay = (TextBlock)(((Button)sender)).Parent.Parent.GetLogicalSiblings().ElementAt(2).GetLogicalChildren().ElementAt(2).GetLogicalChildren().ElementAt(0).GetLogicalChildren().ElementAt(0);
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
            }
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
                    Logger.Info($"Removed {curSeries.Titles[0]} From Collection");
                }
            }
            MainWindowViewModel.Collection = MainWindowViewModel.SearchedCollection;
        }

        private void ShowEditPane(object sender, RoutedEventArgs args)
        {
            ((Grid)((Button)sender).Parent.Parent.GetLogicalChildren().ElementAt(0)).IsVisible = (SeriesEditPaneIsOpenView ^= true);
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
            MainWindowViewModel.SortCollection();
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

        private void SaveOnClose(object sender, CancelEventArgs e)
        {
            if (CollectionViewModel.newSeriesWindow != null)
            {
                CollectionViewModel.newSeriesWindow.Closing += (s, e) => { e.Cancel = false; };
                CollectionViewModel.newSeriesWindow.Close();
            }
            Logger.Info("Closing & Saving TsundOku");
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
                curSeries.CurVolumeCount += 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).Parent.GetLogicalSiblings().ElementAt(0);
                string log = "Adding 1 Volume To " + curSeries.Titles[0] + " : " + volumeDisplay.Text + " -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                Logger.Info(log + volumeDisplay.Text);

                ProgressBar curProgressBar = (ProgressBar)((Button)sender).Parent.Parent.Parent.Parent.GetLogicalChildren().ElementAt(1);
                curProgressBar.Value = curSeries.CurVolumeCount;
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
                curSeries.CurVolumeCount -= 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).Parent.GetLogicalSiblings().ElementAt(0);
                string log = "Removing 1 Volume From " + curSeries.Titles[0] + " : " + volumeDisplay.Text + " -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                Logger.Info(log + volumeDisplay.Text);

                ProgressBar curProgressBar = (ProgressBar)((Button)sender).Parent.Parent.Parent.Parent.GetLogicalChildren().ElementAt(1);
                curProgressBar.Value = curSeries.CurVolumeCount;
            }
        }
    }
}
