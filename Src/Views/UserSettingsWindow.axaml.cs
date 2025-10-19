using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Avalonia.Controls;
using System.Diagnostics;
using ReactiveUI;
using Avalonia.Platform.Storage;
using Tsundoku.Helpers;
using ReactiveUI.Avalonia;

namespace Tsundoku.Views;

public sealed partial class UserSettingsWindow : ReactiveWindow<UserSettingsViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public bool IsOpen = false;
    public int currencyLength = 0;
    private readonly IPopupDialogService _popupDialogService;

    public UserSettingsWindow(UserSettingsViewModel viewModel, IPopupDialogService popupDialogService)
    {
        _popupDialogService = popupDialogService;
        InitializeComponent();

        ViewModel = viewModel;

        Opened += (s, e) =>
        {
            IsOpen ^= true;
            if (Screens.Primary.WorkingArea.Height < 955)
            {
                this.Height = 550;
            }
        };

        Closing += (s, e) =>
        {
            if (IsOpen)
            {
                this.Hide();
                Topmost = false;
                IsOpen ^= true;
            }
            e.Cancel = true;
        };

        this.WhenAnyValue(x => x.BooksAMillionButton.IsChecked, (member) => member is not null && member == true).Subscribe(x => ViewModel.BooksAMillionMember = x);

        this.WhenAnyValue(x => x.KinokuniyaUSAButton.IsChecked, (member) => member is not null && member == true).Subscribe(x => ViewModel.KinokuniyaUSAMember = x);

        this.WhenAnyValue(x => x.UsernameChangeTextBox.Text, (newUsername) => !string.IsNullOrWhiteSpace(newUsername) && !newUsername.Equals(ViewModel.CurrentUser.UserName))
            .Subscribe(x => ViewModel.IsChangeUsernameButtonEnabled = x);
    }

    private async void ExportToSpreadSheetAsync(object sender, RoutedEventArgs e)
    {
        await ViewModel.ExportToSpreadSheetAsync(this);
    }

    private void CurrencyChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CurrencyComboBox.SelectedItem is string selectedCurrency)
        {
            ViewModel.UpdateUserCurrency(selectedCurrency);
            LOGGER.Info($"Currency Changed To {selectedCurrency}");
        }
    }

    private async Task ShowFileErrorDialog(string info = "Unable to Open File\nCheck if it is being used by another app")
    {
        await _popupDialogService.ShowAsync("Error", "fa-solid fa-circle-exclamation", info, this);
    }

    /// <summary>
    /// Allows user to import a new Json file to be used as their new data, it additionall creates a backup file of the users last save
    /// </summary>
    private async void ImportUserDataAsync(object sender, RoutedEventArgs args)
    {
        try
        {
            IReadOnlyList<IStorageFile> files = await this.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("JSON File")
                        {
                            Patterns = [ "*.json" ]
                        }
                    ]
                }
            );
            if (files.Count == 1)
            {
                ViewModel.ImportUserDataFromJson(files[0].Path.LocalPath, this);
            }
        }
        catch (IOException ex)
        {
            LOGGER.Error(ex);
            await ShowFileErrorDialog();
        }
    }

    private async void ImportLibibDataAsync(object sender, RoutedEventArgs args)
    {
        try
        {
            IReadOnlyList<IStorageFile> files = await this.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    AllowMultiple = true,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("JSON File")
                    {
                        Patterns = [ "*.csv" ]
                    }
                    ]
                }
            );
            if (files.Count > 0)
            {
                await ViewModel.ImportLibibDataFromCsv([.. files.Select(f => f.Path.LocalPath)], this.Owner as Window);
            }
            else
            {
                LOGGER.Debug("User tried to import libib data but no files were selected");
            }
        }
        catch (IOException ex)
        {
            LOGGER.Error(ex);
            await ShowFileErrorDialog();
        }
    }

    private async void ImportGoodreadsDataAsync(object sender, RoutedEventArgs args)
    {
        try
        {
            IReadOnlyList<IStorageFile> files = await this.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    AllowMultiple = true,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("JSON File")
                        {
                            Patterns = [ "*.csv" ]
                        }
                    ]
                }
            );
            if (files.Count > 0)
            {
                await ViewModel.ImportGoodreadsDataFromCsv([.. files.Select(f => f.Path.LocalPath)], this.Owner as Window);
            }
            else
            {
                LOGGER.Debug("User tried to import goodreads data but no files were selected");
            }
        }
        catch (IOException ex)
        {
            LOGGER.Error(ex);
            await ShowFileErrorDialog();
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
                LOGGER.Debug($"Opened Tsundoku application data folder: {tsundokuAppFolderPath}");
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
                LOGGER.Debug($"Opened Covers folder: {coversPath}");
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
                LOGGER.Debug($"Opened Screenshots folder: {screenshotsPath}");
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
                LOGGER.Debug($"Opened Themes folder: {themesPath}");
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
        string newUsername = UsernameChangeTextBox.Text;
        if (!string.IsNullOrWhiteSpace(newUsername))
        {
            ViewModel.UpdateUserName(newUsername);
            LOGGER.Info("Username Changed to {Username");
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
