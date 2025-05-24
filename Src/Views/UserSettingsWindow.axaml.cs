using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Avalonia.Controls;
using System.Diagnostics;
using ReactiveUI;
using Avalonia.Platform.Storage;
using System.Diagnostics.CodeAnalysis;
using Avalonia.ReactiveUI;
using Tsundoku.Helpers;
using Windows.Storage;
using Windows.System;

namespace Tsundoku.Views
{
    public partial class SettingsWindow : ReactiveWindow<UserSettingsViewModel>
    {
        public bool IsOpen = false;
        public int currencyLength = 0;
        private static readonly FilePickerFileType fileOptions = new FilePickerFileType("JSON File")
        {
            Patterns = [ "*.json" ]
        };
        private static readonly FilePickerOpenOptions filePickerOptions =  new FilePickerOpenOptions {
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>() { fileOptions }
        };
        private MainWindow CollectionWindow;
        
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new UserSettingsViewModel();
            Opened += (s, e) =>
            {
                IsOpen ^= true;
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;

                if (Screens.Primary.WorkingArea.Height < 955)
                {
                    this.Height = 550;
                }
            };

            Closing += (s, e) =>
            {
                if (IsOpen)
                {
                    MainWindow.ResetMenuButton(CollectionWindow.SettingsButton);
                    ((SettingsWindow)s).Hide();
                    Topmost = false;
                    IsOpen ^= true;
                }
                e.Cancel = true;
            };

            this.WhenAnyValue(x => x.IndigoButton.IsChecked, (member) => member != null && member == true).Subscribe(x => ViewModel.IndigoMember = x);
            this.WhenAnyValue(x => x.BooksAMillionButton.IsChecked, (member) => member != null && member == true).Subscribe(x => ViewModel.BooksAMillionMember = x);
            this.WhenAnyValue(x => x.KinokuniyaUSAButton.IsChecked, (member) => member != null && member == true).Subscribe(x => ViewModel.KinokuniyaUSAMember = x);
        }

        public void CurrencyChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                string newCurrency = (CurrencySelector.SelectedItem as ComboBoxItem).Content.ToString();
                currencyLength = ViewModelBase.CurCurrency.Length;
                ViewModelBase.CurCurrency = newCurrency;
                MainWindowViewModel.collectionStatsWindow.ViewModel.CollectionPrice = $"{newCurrency}{ MainWindowViewModel.collectionStatsWindow.ViewModel.CollectionPrice[currencyLength..]}";
                LOGGER.Info($"Currency Changed To {newCurrency}");
            }
        }


        /// <summary>
        /// Allows user to upload a new Json file to be used as their new data, it additionall creates a backup file of the users last save
        /// </summary>
        [RequiresUnreferencedCode("Calls Tsundoku.ViewModels.MainWindowViewModel.VersionUpdate(JsonNode)")]
        private async void UploadUserData(object sender, RoutedEventArgs args)
        {
            IReadOnlyList<Avalonia.Platform.Storage.IStorageFile> file = await this.StorageProvider.OpenFilePickerAsync(filePickerOptions);
            if (file.Count > 0)
            {
                UserSettingsViewModel.ImportUserData(file);
                ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).TryShutdown();
                Process.Start(@$"{AppDomain.CurrentDomain.BaseDirectory}\Tsundoku.exe");
            }
        }

        private async void OpenReleasesPage(object sender, PointerPressedEventArgs args)
        {
            await ViewModelBase.OpenSiteLink(@"https://github.com/Sigrec/Tsundoku/releases");
        }

        public async void OpenAniListLink(object sender, RoutedEventArgs args)
        {
            await ViewModelBase.OpenSiteLink(@"https://anilist.co/");
        }

        public async void OpenMangadexLink(object sender, RoutedEventArgs args)
        {
            await ViewModelBase.OpenSiteLink(@"https://mangadex.org/");
        }
        
        public async void OpenApplicationFolder(object sender, RoutedEventArgs args)
        {
            await Task.Run(() =>
            {
                // Get the full path to the base "Tsundoku" application data folder.
                // Passing an empty string to GetFolderPath returns the base folder.
                string tsundokuAppFolderPath = AppFileHelper.GetFolderPath("");
                try
                {
                    // Use ProcessStartInfo with UseShellExecute = true for robustness
                    Process.Start(new ProcessStartInfo(tsundokuAppFolderPath)
                    {
                        UseShellExecute = true, // Essential for letting the OS shell handle opening the folder
                        Verb = "open"           // Explicitly ask the shell to "open" the target
                    });
                    LOGGER.Info($"Opened Tsundoku application data folder: {tsundokuAppFolderPath}");
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    // This exception occurs if the shell (explorer.exe) cannot find the path,
                    // or if there's a permissions issue, which can happen in MSIX sandbox.
                    LOGGER.Error(ex, $"Win32Exception: Failed to open Tsundoku application data folder at: {tsundokuAppFolderPath}");
                    // Optional: Fallback to direct explorer.exe launch if shell execute fails
                    try
                    {
                        Process.Start("explorer.exe", tsundokuAppFolderPath);
                    }
                    catch (Exception directEx)
                    {
                        LOGGER.Error(directEx, $"Fallback: Direct explorer.exe launch also failed for Tsundoku application data folder: {tsundokuAppFolderPath}");
                    }
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex, $"An unexpected error occurred while trying to open the Tsundoku application data folder: {tsundokuAppFolderPath}");
                }
            });
        }

        public async void OpenCoversFolder(object sender, RoutedEventArgs args)
        {
            await Task.Run(() =>
            {
                string coversPath = AppFileHelper.GetCoversFolderPath();
                try
                {
                    // Use ProcessStartInfo with UseShellExecute = true for robustness
                    Process.Start(new ProcessStartInfo(coversPath)
                    {
                        UseShellExecute = true, // Essential for letting the OS shell handle opening the folder
                        Verb = "open"           // Explicitly ask the shell to "open" the target
                    });
                    LOGGER.Info($"Opened Covers folder: {coversPath}");
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    // This exception occurs if the shell (explorer.exe) cannot find the path,
                    // or if there's a permissions issue, which can happen in MSIX sandbox.
                    LOGGER.Error(ex, $"Win32Exception: Failed to open Covers folder at: {coversPath}");
                    // Optional: Fallback to direct explorer.exe launch if shell execute fails,
                    // though UseShellExecute is usually more reliable.
                    try
                    {
                        Process.Start("explorer.exe", coversPath);
                    }
                    catch (Exception directEx)
                    {
                        LOGGER.Error(directEx, $"Fallback: Direct explorer.exe launch also failed for Covers folder: {coversPath}");
                    }
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex, $"An unexpected error occurred while trying to open the Covers folder: {coversPath}");
                }
            });
        }

        public async void OpenScreenshotsFolder(object sender, RoutedEventArgs args)
        {
            await Task.Run(() =>
            {
                string screenshotsPath = AppFileHelper.GetScreenshotsFolderPath();
                try
                {
                    Process.Start(new ProcessStartInfo(screenshotsPath)
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    });
                    LOGGER.Info($"Opened Screenshots folder: {screenshotsPath}");
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    LOGGER.Error(ex, $"Win32Exception: Failed to open Screenshots folder at: {screenshotsPath}");
                    try { Process.Start("explorer.exe", screenshotsPath); }
                    catch (Exception directEx) { LOGGER.Error(directEx, $"Fallback: Direct explorer.exe launch also failed for Screenshots folder: {screenshotsPath}"); }
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex, $"An unexpected error occurred while trying to open the Screenshots folder: {screenshotsPath}");
                }
            });
        }

        public async void OpenThemesFolder(object sender, RoutedEventArgs args)
        {
            await Task.Run(() =>
            {
                string themesPath = AppFileHelper.GetThemesFolderPath();
                try
                {
                    Process.Start(new ProcessStartInfo(themesPath)
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    });
                    LOGGER.Info($"Opened Themes folder: {themesPath}");
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    LOGGER.Error(ex, $"Win32Exception: Failed to open Themes folder at: {themesPath}");
                    try { Process.Start("explorer.exe", themesPath); }
                    catch (Exception directEx) { LOGGER.Error(directEx, $"Fallback: Direct explorer.exe launch also failed for Themes folder: {themesPath}"); }
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex, $"An unexpected error occurred while trying to open the Themes folder: {themesPath}");
                }
            });
        }

        public void ChangeUsername(object sender, RoutedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(UsernameChange.Text))
            {
                CollectionWindow.ViewModel.UserName = UsernameChange.Text;
                LOGGER.Info($"Username Changed To -> {UsernameChange.Text}");
            }
            else
            {
                LOGGER.Warn("Change Username Field is Missing Input");
            }
        }

        public async void OpenYoutuberSite(object sender, RoutedEventArgs args)
        {
            await ViewModelBase.OpenSiteLink(@$"https://www.youtube.com/@{(sender as Button).Name}");
        }

        public async void OpenCoolorsSite(object sender, RoutedEventArgs args)
        {
            await ViewModelBase.OpenSiteLink(@"https://coolors.co/generate");
        }

        public async void JoinDiscord(object sender, RoutedEventArgs args)
        {
            await ViewModelBase.OpenSiteLink(@"https://discord.gg/QcZ5jcFPeU");
        }

        public async void ReportBug(object sender, RoutedEventArgs args)
        {
            await ViewModelBase.OpenSiteLink(@"https://github.com/Sigrec/Tsundoku/issues/new/choose");
        }
    }
}
