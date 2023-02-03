using Avalonia;
using GemBox.Spreadsheet;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Avalonia.Controls;
using System.Diagnostics;
using System.Text;
using System;

namespace Tsundoku.Views
{
    public partial class SettingsWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public UserSettingsViewModel? UserSettingsVM => DataContext as UserSettingsViewModel;
        public bool IsOpen = false;

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

        private void OpenCoversFolder(object sender, RoutedEventArgs args)
        {
            Process.Start("explorer.exe", @$"Covers");
        }

        private void OpenScreenshotsFolder(object sender, RoutedEventArgs args)
        {
            Process.Start("explorer.exe", @$"Screenshots");
        }

        private void ChangeUsername(object sender, RoutedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(UsernameChange.Text))
            {
                (((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Windows[0] as MainWindow).CollectionViewModel.UserName = UsernameChange.Text;
                Logger.Info($"Username Changed To -> {UsernameChange.Text}");
            }
            else
            {
                Logger.Warn("Change Username Field is Missing Input");
            }
        }  

        private void OpenSite(object sender, RoutedEventArgs args)
        {
            Logger.Info(@$"Opening Coolors Website https://{(sender as Button).Name}");
            try
            {
                Process.Start(new ProcessStartInfo(@$"https://{(sender as Button).Name}") { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                Logger.Error(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Logger.Error(other.Message);
            }
        }

        private void OpenYoutuberSite(object sender, RoutedEventArgs args)
        {
            Logger.Info(@$"Opening Coolors Website https://www.youtube.com/@{(sender as Button).Name}");
            try
            {
                Process.Start(new ProcessStartInfo(@$"https://www.youtube.com/@{(sender as Button).Name}") { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                Logger.Error(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Logger.Error(other.Message);
            }
        }

        private void OpenCoolorsSite(object sender, RoutedEventArgs args)
        {
            Logger.Info(@"Opening Coolors Website https://coolors.co/generate");
            try
            {
                Process.Start(new ProcessStartInfo(@"https://coolors.co/generate") { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                Logger.Error(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Logger.Error(other.Message);
            }
        }

        private void JoinDiscord(object sender, RoutedEventArgs args)
        {
            Logger.Info(@"Opening Issue Repo https://discord.gg/QcZ5jcFPeU");
            try
            {
                Process.Start(new ProcessStartInfo(@"https://discord.gg/QcZ5jcFPeU") { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                Logger.Error(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Logger.Error(other.Message);
            }
        }

        private void ReportBug(object sender, RoutedEventArgs args)
        {
            Logger.Info(@"Opening Issue Repo https://github.com/Sigrec/TsundokuApp/issues/new");
            try
            {
                Process.Start(new ProcessStartInfo(@"https://github.com/Sigrec/TsundokuApp/issues/new") { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                Logger.Error(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Logger.Error(other.Message);
            }
        }

        private void ExportToSpreadsheet(object sender, RoutedEventArgs args)
        {
            string file = @"TsundokuCollection.csv";
            StringBuilder output = new StringBuilder();
            string[] headers = new string[] { "Title", "Staff", "Format", "Status", "Cur Volumes", "Max Volumes", "Notes" };
            output.AppendLine(string.Join(",", headers));

            foreach (Models.Series curSeries in MainWindowViewModel.Collection)
            {
                switch(MainWindowViewModel.MainUser.CurLanguage)
                {
                    case "Native":
                        String[] nativeLine = { curSeries.Titles[2], curSeries.Staff[1], curSeries.Format, curSeries.Status, curSeries.CurVolumeCount.ToString(), curSeries.MaxVolumeCount.ToString(), $"\"{curSeries.SeriesNotes}\"" };
                        output.AppendLine(string.Join(",", nativeLine));
                        break;
                    case "English":
                        String[] englishLine = { curSeries.Titles[1], curSeries.Staff[0], curSeries.Format, curSeries.Status, curSeries.CurVolumeCount.ToString(), curSeries.MaxVolumeCount.ToString(), $"\"{curSeries.SeriesNotes}\"" };
                        output.AppendLine(string.Join(",", englishLine));
                        break;
                    default:
                        String[] romajiLine = { curSeries.Titles[0], curSeries.Staff[0], curSeries.Format, curSeries.Status, curSeries.CurVolumeCount.ToString(), curSeries.MaxVolumeCount.ToString(), $"\"{curSeries.SeriesNotes}\"" };
                        output.AppendLine(string.Join(",", romajiLine));
                        break;
                }
            }

            try
            {
                System.IO.File.WriteAllText(file, output.ToString(), Encoding.UTF8);
                Logger.Info($"Exported {MainWindowViewModel.MainUser.UserName}'s Data To -> TsundokuCollection.csv");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Could not Export {MainWindowViewModel.MainUser.UserName}'s Data To -> TsundokuCollection.csv");
            }
        }
    }
}
