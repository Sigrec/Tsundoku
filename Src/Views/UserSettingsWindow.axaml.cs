using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Avalonia.Controls;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using ReactiveUI;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;
using Tsundoku.Helpers;
using Tsundoku.Services;
using ReactiveUI.Avalonia;

namespace Tsundoku.Views;

public sealed partial class UserSettingsWindow : ReactiveWindow<UserSettingsViewModel>, IManagedWindow
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public bool IsOpen { get; set; }
    private readonly IPopupDialogService _popupDialogService;
    private readonly IApiHealthCheckService _apiHealthCheckService;

    public UserSettingsWindow(UserSettingsViewModel viewModel, IPopupDialogService popupDialogService, IApiHealthCheckService apiHealthCheckService)
    {
        _popupDialogService = popupDialogService;
        _apiHealthCheckService = apiHealthCheckService;
        InitializeComponent();

        ViewModel = viewModel;

        this.ConfigureHideOnClose(
            onOpened: () => { if (Screens.Primary.WorkingArea.Height < 955) this.Height = 550; });

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

            _apiHealthCheckService.IsAniListAvailable
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(isAvailable =>
                {
                    RefreshSeriesButton.IsEnabled = isAvailable;
                    ImportLibibData.IsEnabled = isAvailable;
                    ImportGoodreadsData.IsEnabled = isAvailable;
                })
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

    private async Task ImportFileAsync(bool allowMultiple, string fileTypeLabel, string pattern, Func<IReadOnlyList<IStorageFile>, Task> onFilesSelected)
    {
        try
        {
            IReadOnlyList<IStorageFile> files = await this.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    AllowMultiple = allowMultiple,
                    FileTypeFilter = [ new FilePickerFileType(fileTypeLabel) { Patterns = [ pattern ] } ]
                }
            );
            if (files.Count > 0)
            {
                await onFilesSelected(files);
            }
        }
        catch (IOException ex)
        {
            LOGGER.Error(ex);
            await ShowFileErrorDialog();
        }
    }

    private async void ImportUserDataAsync(object sender, RoutedEventArgs args)
    {
        await ImportFileAsync(false, "JSON File", "*.json", files =>
        {
            ViewModel.ImportUserDataFromJson(files[0].Path.LocalPath, this);
            return Task.CompletedTask;
        });
    }

    private async void ImportLibibDataAsync(object sender, RoutedEventArgs args)
    {
        await ImportFileAsync(true, "CSV File", "*.csv", async files =>
        {
            await ViewModel.ImportLibibDataFromCsv([.. files.AsValueEnumerable().Select(f => f.Path.LocalPath)], this.Owner as Window);
        });
    }

    private async void ImportGoodreadsDataAsync(object sender, RoutedEventArgs args)
    {
        await ImportFileAsync(true, "CSV File", "*.csv", async files =>
        {
            await ViewModel.ImportGoodreadsDataFromCsv([.. files.AsValueEnumerable().Select(f => f.Path.LocalPath)], this.Owner as Window);
        });
    }

    private async void OpenReleasesPage(object sender, PointerPressedEventArgs args)
    {
        await ViewModelBase.OpenSiteLink(@"https://github.com/Sigrec/Tsundoku/releases");
    }

    private async void ShowChangelogAsync(object sender, RoutedEventArgs args)
    {
        ChangelogWindow changelog = new() { DataContext = ViewModel };
        changelog.SetVersion(ViewModelBase.CUR_TSUNDOKU_VERSION);
        await changelog.ShowDialog(this);
    }

    private async void CheckApiStatusAsync(object sender, RoutedEventArgs args)
    {
        CheckApiStatusButton.IsEnabled = false;
        CheckApiStatusButton.Content = "Checking...";
        try
        {
            (bool aniList, bool mangaDex) = await _apiHealthCheckService.CheckNowAsync();

            if (aniList && mangaDex)
            {
                await _popupDialogService.ShowAsync("API Status", "fa-solid fa-circle-check", "AniList and MangaDex are both online.", this);
            }
            else if (!aniList && !mangaDex)
            {
                await _popupDialogService.ShowAsync("API Status", "fa-solid fa-triangle-exclamation", "AniList and MangaDex are both unavailable. Adding new series, refreshing, and imports are disabled.", this);
            }
            else if (!aniList)
            {
                bool enableAdd = await _popupDialogService.ConfirmAsync(
                    "API Status",
                    "fa-solid fa-triangle-exclamation",
                    "AniList is unavailable but MangaDex is online. Refreshing series and imports remain disabled.\n\nWould you like to enable adding new series via MangaDex?",
                    this);

                if (enableAdd && Owner is MainWindow mainWindow)
                {
                    mainWindow.AddNewSeriesButton.IsEnabled = true;
                }
            }
            else
            {
                await _popupDialogService.ShowAsync("API Status", "fa-solid fa-triangle-exclamation", "MangaDex is unavailable. You can still add and refresh series using AniList.", this);
            }
        }
        finally
        {
            CheckApiStatusButton.Content = "Check API Status";
            CheckApiStatusButton.IsEnabled = true;
        }
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
