using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Avalonia.Controls;
using System.Diagnostics;
using System.Text;
using System;
using Avalonia;
using System.ComponentModel;
using Tsundoku.Models;
using System.Threading.Tasks;

namespace Tsundoku.Views
{
    public partial class SettingsWindow : Window
    {
        public UserSettingsViewModel? UserSettingsVM => DataContext as UserSettingsViewModel;
        public bool IsOpen = false;
        public int currencyLength = 0;
        MainWindow CollectionWindow;
        
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new UserSettingsViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                UserSettingsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                IsOpen ^= true;
            };

            Closing += (s, e) =>
            {
                ((SettingsWindow)s).Hide();
                Topmost = false;
                IsOpen ^= true;
                e.Cancel = true;
            };
        }

        private void CurrencyChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                string newCurrency = (sender as ComboBox).SelectedItem as string;
                currencyLength = CollectionWindow.CollectionViewModel.CurCurrency.Length;
                CollectionWindow.CollectionViewModel.CurCurrency = newCurrency;
                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice = $"{newCurrency}{ CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice.Substring(currencyLength)}";
                Constants.Logger.Info($"Currency Changed To {newCurrency}");
            }
        }

        private void OpenAniListLink(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                Constants.Logger.Info(@"Opening Link https://anilist.co/");
                try
                {
                    Process.Start(new ProcessStartInfo(@"https://anilist.co/") { UseShellExecute = true });
                }
                catch (Win32Exception noBrowser)
                {
                    Constants.Logger.Error(noBrowser.Message);
                }
                catch (Exception other)
                {
                    Constants.Logger.Error(other.Message);
                }
            });
        }

        private void OpenMangadexLink(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                Constants.Logger.Info(@"Opening Link https://mangadex.org/");
                try
                {
                    Process.Start(new ProcessStartInfo(@"https://mangadex.org/") { UseShellExecute = true });
                }
                catch (Win32Exception noBrowser)
                {
                    Constants.Logger.Error(noBrowser.Message);
                }
                catch (Exception other)
                {
                    Constants.Logger.Error(other.Message);
                }
            });
        }

        private void OpenCoversFolder(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                Process.Start("explorer.exe", @$"Covers");
            });
        }

        private void OpenScreenshotsFolder(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                Process.Start("explorer.exe", @$"Screenshots");
            });
        }

        private void ChangeUsername(object sender, RoutedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(UsernameChange.Text))
            {
                CollectionWindow.CollectionViewModel.UserName = UsernameChange.Text;
                Constants.Logger.Info($"Username Changed To -> {UsernameChange.Text}");
            }
            else
            {
                Constants.Logger.Warn("Change Username Field is Missing Input");
            }
        }

        private void OpenYoutuberSite(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                Constants.Logger.Info(@$"Opening Coolors Website https://www.youtube.com/@{(sender as Button).Name}");
                try
                {
                    Process.Start(new ProcessStartInfo(@$"https://www.youtube.com/@{(sender as Button).Name}") { UseShellExecute = true });
                }
                catch (System.ComponentModel.Win32Exception noBrowser)
                {
                    Constants.Logger.Error(noBrowser.Message);
                }
                catch (System.Exception other)
                {
                    Constants.Logger.Error(other.Message);
                }
            });
        }

        private void OpenCoolorsSite(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                Constants.Logger.Info(@"Opening Coolors Website https://coolors.co/generate");
                try
                {
                    Process.Start(new ProcessStartInfo(@"https://coolors.co/generate") { UseShellExecute = true });
                }
                catch (System.ComponentModel.Win32Exception noBrowser)
                {
                    Constants.Logger.Error(noBrowser.Message);
                }
                catch (System.Exception other)
                {
                    Constants.Logger.Error(other.Message);
                }
            });
        }

        private void JoinDiscord(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                Constants.Logger.Info(@"Opening Issue Repo https://discord.gg/QcZ5jcFPeU");
                try
                {
                    Process.Start(new ProcessStartInfo(@"https://discord.gg/QcZ5jcFPeU") { UseShellExecute = true });
                }
                catch (System.ComponentModel.Win32Exception noBrowser)
                {
                    Constants.Logger.Error(noBrowser.Message);
                }
                catch (System.Exception other)
                {
                    Constants.Logger.Error(other.Message);
                }
            });
        }

        private void ReportBug(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                Constants.Logger.Info(@"Opening Issue Repo https://github.com/Sigrec/TsundokuApp/issues/new");
                try
                {
                    Process.Start(new ProcessStartInfo(@"https://github.com/Sigrec/TsundokuApp/issues/new") { UseShellExecute = true });
                }
                catch (System.ComponentModel.Win32Exception noBrowser)
                {
                    Constants.Logger.Error(noBrowser.Message);
                }
                catch (System.Exception other)
                {
                    Constants.Logger.Error(other.Message);
                }
            });
        }
    }
}
