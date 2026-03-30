using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Avalonia.Controls;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using ReactiveUI;
using Avalonia.Platform.Storage;
using Tsundoku.Helpers;
using ReactiveUI.Avalonia;

namespace Tsundoku.Views;

public sealed partial class UserSettingsWindow : ReactiveWindow<UserSettingsViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public bool IsOpen = false;
    private readonly IPopupDialogService _popupDialogService;

    public UserSettingsWindow(UserSettingsViewModel viewModel, IPopupDialogService popupDialogService)
    {
        _popupDialogService = popupDialogService;
        InitializeComponent();

        ViewModel = viewModel;

        Opened += (s, e) =>
        {
            IsOpen = true;
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
                IsOpen = false;
                e.Cancel = true;
            }
        };

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.BooksAMillionButton.IsChecked, (member) => member is not null && member == true)
                .Subscribe(x => ViewModel.BooksAMillionMember = x)
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.KinokuniyaUSAButton.IsChecked, (member) => member is not null && member == true)
                .Subscribe(x => ViewModel.KinokuniyaUSAMember = x)
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.UsernameChangeTextBox.Text, (newUsername) => !string.IsNullOrWhiteSpace(newUsername) && !newUsername.Equals(ViewModel.CurrentUser.UserName))
                .Subscribe(x => ViewModel.IsChangeUsernameButtonEnabled = x)
                .DisposeWith(disposables);
        });
    }

    private async void RefreshAllCoversAsync(object sender, RoutedEventArgs args)
    {
        await ViewModel.RefreshAllCoversAsync(this.Owner as Window ?? this);
    }

    private async void ExportToSpreadSheetAsync(object sender, RoutedEventArgs e)
    {
        await ViewModel.ExportToSpreadSheetAsync(this);
    }


    private void ToggleControlsSection(object? sender, RoutedEventArgs e)
    {
        ControlsContent.IsVisible = !ControlsContent.IsVisible;
        ControlsChevron.Value = ControlsContent.IsVisible ? "fa-solid fa-chevron-up" : "fa-solid fa-chevron-down";
    }

    private void ToggleYoutubersSection(object? sender, RoutedEventArgs e)
    {
        YoutubersContent.IsVisible = !YoutubersContent.IsVisible;
        YoutubersChevron.Value = YoutubersContent.IsVisible ? "fa-solid fa-chevron-up" : "fa-solid fa-chevron-down";
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
                await ViewModel.ImportLibibDataFromCsv([.. files.AsValueEnumerable().Select(f => f.Path.LocalPath)], this.Owner as Window);
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
                await ViewModel.ImportGoodreadsDataFromCsv([.. files.AsValueEnumerable().Select(f => f.Path.LocalPath)], this.Owner as Window);
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
    
    public void OpenApplicationFolder(object sender, RoutedEventArgs args) => OpenFolder(AppFileHelper.GetFolderPath(string.Empty));
    public void OpenCoversFolder(object sender, RoutedEventArgs args) => OpenFolder(AppFileHelper.GetCoversFolderPath());
    public void OpenScreenshotsFolder(object sender, RoutedEventArgs args) => OpenFolder(AppFileHelper.GetScreenshotsFolderPath());
    public void OpenThemesFolder(object sender, RoutedEventArgs args) => OpenFolder(AppFileHelper.GetThemesFolderPath());

    private static void OpenFolder(string folderPath)
    {
        try
        {
            Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true, Verb = "open" });
            LOGGER.Debug("Opened folder: {FolderPath}", folderPath);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            try { Process.Start("explorer.exe", folderPath); }
            catch (Exception ex) { LOGGER.Error(ex, "Failed to open folder: {FolderPath}", folderPath); }
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to open folder: {FolderPath}", folderPath);
        }
    }

    public void ChangeUsername(object sender, RoutedEventArgs args)
    {
        string newUsername = UsernameChangeTextBox.Text;
        if (!string.IsNullOrWhiteSpace(newUsername))
        {
            ViewModel.UpdateUserName(newUsername);
            LOGGER.Info("Username Changed to {Username}", newUsername);
        }
    }

    public async void OpenYoutuberSite(object sender, RoutedEventArgs args)
    {
        if (sender is Button button)
        {
            await ViewModelBase.OpenSiteLink(@$"https://www.youtube.com/@{button.Name}");
        }
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
