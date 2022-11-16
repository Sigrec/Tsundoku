using System.Security.Cryptography.X509Certificates;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using ReactiveUI;

namespace Tsundoku.Views
{
    public partial class MainWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public MainWindow()
        {
            InitializeComponent();
            //Logger.Debug("Test = " + NumVolumesToBeCollectedText.);
            // MainWindowViewModel.SearchedCollection.CollectionChanged += (sender, e) =>
            // {
            //     if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            //     {
            //         UpdateCollectionViewNumbers();
            //     }
            // };
        }

        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            switch (text)
            {
                case "Native":
                    MainWindowViewModel._curLanguage = "Native";
                    break;
                case "English":
                    MainWindowViewModel._curLanguage = "English";
                    break;
                default:
                    MainWindowViewModel._curLanguage = "Romaji";
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
            Logger.Info($"Current Display = {MainWindowViewModel._curDisplay}");
        }

        private void SaveOnClose(object sender, CancelEventArgs e)
        {
            Logger.Info("Closing & Saving TsundOku");
            MainWindowViewModel.SaveUsersData();
        }

        private void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            string link = ((Canvas)sender).Name;
            Logger.Info($"Opening Site {link}");
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
            Logger.Info($"Copying{title}to Clipboard");
            await Application.Current.Clipboard.SetTextAsync(title);
        }

        public void UpdateCollectionViewNumbers()
        {
            NumVolumesCollectedText.Text = $"Collected\n{MainWindowViewModel._curVolumesCollected} Volumes";
            NumVolumesToBeCollectedText.Text = $"Need To Collect\n{MainWindowViewModel._curVolumesToBeCollected} Volumes";
        }

        /*
         Decrements the series current volume count
        */
        public void AddVolume(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext;
            if (curSeries.CurVolumeCount < curSeries.MaxVolumeCount)
            {
                //MainWindowViewModel.UsersNumVolumesCollected += 1;
                MainWindowViewModel._curVolumesToBeCollected -= 1;
                //UpdateCollectionViewNumbers();
                TextBlock volumeDisplay = (TextBlock)((Button)sender).GetLogicalSiblings().ToList()[0];
                string log = "Adding 1 Volume To " + curSeries.Titles[0] + " : " + volumeDisplay.Text + " -> ";
                ProgressBar seriesProgressBar = (ProgressBar)((Button)sender).Parent.Parent.Parent.GetLogicalChildren().ToList()[1];
                curSeries.CurVolumeCount += 1;
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                seriesProgressBar.Value = curSeries.CurVolumeCount;
                Logger.Info(log + volumeDisplay.Text);
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
                MainWindowViewModel._curVolumesCollected -= 1;
                MainWindowViewModel._curVolumesToBeCollected += 1;
                //UpdateCollectionViewNumbers();
                TextBlock volumeDisplay = (TextBlock)((Button)sender).GetLogicalSiblings().ToList()[0];
                string log = "Removing 1 Volume From " + curSeries.Titles[0] + " : " + volumeDisplay.Text + " -> ";
                ProgressBar seriesProgressBar = (ProgressBar)((Button)sender).Parent.Parent.Parent.GetLogicalChildren().ToList()[1];
                curSeries.CurVolumeCount -= 1;
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                seriesProgressBar.Value = curSeries.CurVolumeCount;
                Logger.Info(log + volumeDisplay.Text);
            }
        }
    }
}
