using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Media.Imaging;
using System.Reactive.Linq;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;
using static Tsundoku.Models.Enums.TsundokuFilterModel;
using static Tsundoku.Models.Enums.TsundokuSortModel;
using ReactiveUI.Avalonia;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Collections.Specialized;
using Tsundoku.Services;
using Avalonia.Animation;
using Avalonia.Media.Transformation;
using Avalonia.Styling;

namespace Tsundoku.Views;

public sealed partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly IUserService _userService;
    private readonly IApiHealthCheckService _apiHealthCheckService;
    private readonly IPopupDialogService _popupDialogService;
    private readonly AddNewSeriesWindow _newSeriesWindow;
    private readonly UserSettingsWindow _userSettingsWindow;
    private readonly CollectionThemeWindow _themeSettingsWindow;
    private readonly PriceAnalysisWindow _priceAnalysisWindow;
    private readonly CollectionStatsWindow _collectionStatsWindow;
    private readonly UserNotesWindow _userNotesWindow;
    private WindowState previousWindowState;
    private CancellationTokenSource? _notificationCts;
    private bool _isShuttingDown;
    private bool _aniListDown;
    private bool _mangaDexDown;

    public MainWindow(
        MainWindowViewModel viewModel,
        IUserService userService,
        IApiHealthCheckService apiHealthCheckService,
        IPopupDialogService popupDialogService,
        AddNewSeriesWindow newSeriesWindow,
        UserSettingsWindow userSettingsWindow,
        CollectionThemeWindow themeSettingsWindow,
        PriceAnalysisWindow priceAnalysisWindow,
        CollectionStatsWindow collectionStatsWindow,
        UserNotesWindow userNotesWindow)
    {
        _userService = userService;
        _apiHealthCheckService = apiHealthCheckService;
        _popupDialogService = popupDialogService;
        _newSeriesWindow = newSeriesWindow;
        _userSettingsWindow = userSettingsWindow;
        _themeSettingsWindow = themeSettingsWindow;
        _priceAnalysisWindow = priceAnalysisWindow;
        _collectionStatsWindow = collectionStatsWindow;
        _userNotesWindow = userNotesWindow;
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
#if DEBUG
        this.AttachDevTools();
#endif

        this.WhenActivated(disposables =>
        {
            ViewModel!.EditSeriesInfoDialog
                .RegisterHandler(DoShowEditSeriesInfoDialogAsync)
                .DisposeWith(disposables);

            // Back to top button visibility based on scroll offset
            CollectionPane.GetObservable(ScrollViewer.OffsetProperty)
                .Subscribe(offset =>
                {
                    bool show = offset.Y > 300;
                    BackToTopButton.Opacity = show ? 1 : 0;
                    BackToTopButton.IsVisible = offset.Y > 50;
                })
                .DisposeWith(disposables);

            // Reset scroll to top only on collection Reset (filter/sort change), not on individual item updates
            Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => ((INotifyCollectionChanged)ViewModel.UserCollection).CollectionChanged += h,
                h => ((INotifyCollectionChanged)ViewModel.UserCollection).CollectionChanged -= h)
                .Where(e => e.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                .Subscribe(_ =>
                {
                    CollectionPane.Offset = new Avalonia.Vector(0, 0);
                    Dispatcher.UIThread.Post(() => CollectionItems.InvalidateMeasure(), DispatcherPriority.Render);
                })
                .DisposeWith(disposables);

            // API health check — combine both statuses and show modal on transitions
            Observable.CombineLatest(
                    _apiHealthCheckService.IsAniListAvailable,
                    _apiHealthCheckService.IsMangaDexAvailable,
                    (aniList, mangaDex) => (AniList: aniList, MangaDex: mangaDex))
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(async status =>
                {
                    bool aniListChanged = status.AniList == _aniListDown;
                    bool mangaDexChanged = status.MangaDex == _mangaDexDown;

                    if (!aniListChanged && !mangaDexChanged) return;

                    bool wasAniListDown = _aniListDown;
                    bool wasMangaDexDown = _mangaDexDown;
                    _aniListDown = !status.AniList;
                    _mangaDexDown = !status.MangaDex;

                    // Disable add series button based on AniList status
                    AddNewSeriesButton.IsEnabled = status.AniList;

                    // Build message for newly down services
                    if (_aniListDown && !wasAniListDown && _mangaDexDown && !wasMangaDexDown)
                    {
                        await _popupDialogService.ShowAsync(
                            "API Outage",
                            "fa-solid fa-triangle-exclamation",
                            "AniList and MangaDex APIs are currently unavailable. Adding new series, refreshing series, and importing from Libib or Goodreads have been disabled until AniList is back online.",
                            this);
                    }
                    else if (_aniListDown && !wasAniListDown && !_mangaDexDown)
                    {
                        bool enableAdd = await _popupDialogService.ConfirmAsync(
                            "API Outage",
                            "fa-solid fa-triangle-exclamation",
                            "AniList API is currently unavailable. Refreshing series and importing from Libib or Goodreads have been disabled.\n\nMangaDex is online — would you like to enable adding new series via MangaDex?",
                            this);

                        if (enableAdd)
                        {
                            AddNewSeriesButton.IsEnabled = true;
                        }
                    }
                    else if (_aniListDown && !wasAniListDown && _mangaDexDown)
                    {
                        await _popupDialogService.ShowAsync(
                            "API Outage",
                            "fa-solid fa-triangle-exclamation",
                            "AniList API is currently unavailable. Adding new series, refreshing series, and importing from Libib or Goodreads have been disabled until it is back online.",
                            this);
                    }
                    else if (_mangaDexDown && !wasMangaDexDown)
                    {
                        await _popupDialogService.ShowAsync(
                            "API Outage",
                            "fa-solid fa-triangle-exclamation",
                            "MangaDex API is currently unavailable. You can still add and refresh series using AniList.",
                            this);
                    }

                    // Notify when services come back online
                    if (!_aniListDown && wasAniListDown && !_mangaDexDown && wasMangaDexDown)
                    {
                        await _popupDialogService.ShowAsync(
                            "APIs Restored",
                            "fa-solid fa-circle-check",
                            "AniList and MangaDex APIs are back online. All features have been re-enabled.",
                            this);
                    }
                    else if (!_aniListDown && wasAniListDown)
                    {
                        await _popupDialogService.ShowAsync(
                            "API Restored",
                            "fa-solid fa-circle-check",
                            "AniList API is back online. All features have been re-enabled.",
                            this);
                    }
                    else if (!_mangaDexDown && wasMangaDexDown)
                    {
                        await _popupDialogService.ShowAsync(
                            "API Restored",
                            "fa-solid fa-circle-check",
                            "MangaDex API is back online.",
                            this);
                    }
                })
                .DisposeWith(disposables);

            _apiHealthCheckService.Start();
            Disposable.Create(() => _apiHealthCheckService.Stop())
                .DisposeWith(disposables);
        });

        // Fade in the cards once first realized (collection background stays visible)
        CollectionItems.Opacity = 0;
        bool _hasAnimated = false;
        CollectionItems.ElementPrepared += (s, e) =>
        {
            if (_hasAnimated) return;
            _hasAnimated = true;

            // Wait for layout + render to fully complete before starting fade
            Dispatcher.UIThread.Post(async () =>
            {
                // Allow multiple layout passes to complete so all visible cards are rendered
                await Task.Delay(100);

                Animation animation = new Animation
                {
                    Duration = TimeSpan.FromMilliseconds(600),
                    Easing = new Avalonia.Animation.Easings.SineEaseInOut(),
                    FillMode = FillMode.Forward,
                    Children =
                    {
                        new KeyFrame { Cue = new Cue(0), Setters = { new Setter(OpacityProperty, 0.0) } },
                        new KeyFrame { Cue = new Cue(1), Setters = { new Setter(OpacityProperty, 1.0) } }
                    }
                };
                await animation.RunAsync(CollectionItems);
            }, DispatcherPriority.Background);
        };

        KeyDown += async (s, e) =>
        {
            if (e.Key == Key.F11) // Fullscreen
            {
                if (WindowState != WindowState.FullScreen)
                {
                    previousWindowState = WindowState;
                    WindowState = WindowState.FullScreen;
                    await ToggleNotificationPopup("To Exit Fullscreen Press F11");
                }
                else if (WindowState == WindowState.FullScreen)
                {
                    WindowState = previousWindowState;
                }
            }
            else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.P) // Take Screenshot
            {
                LOGGER.Info("Saving Screenshot of Collection for \"{ThemeName} Theme\"", ViewModel.CurrentTheme.ThemeName);
                await ScreenCaptureWindows();
            }
            else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S) // Save User Data
            {
                ViewModel.SaveUserData();
                await ToggleNotificationPopup($"Saved \"{ViewModel.CurrentUser.UserName}'s\" Data");
            }
            else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.F)
            {
                AdvancedSearchPopup.IsVisible = !AdvancedSearchPopup.IsVisible;
                if (AdvancedSearchPopup.IsVisible)
                {
                    await ToggleNotificationPopup($"To Exit Advanced Search Press CTRL+F or Click Anywhere");
                }
            }
        };

        Closing += (s, e) =>
        {
            if (_isShuttingDown) return;
            _isShuttingDown = true;

            ViewModel.SaveOnClose();

            // Hide all children so their Closing handlers won't cancel
            foreach (Window child in (Window[])[_newSeriesWindow, _userSettingsWindow, _themeSettingsWindow, _priceAnalysisWindow, _collectionStatsWindow, _userNotesWindow])
            {
                if (child.IsVisible)
                {
                    child.Hide();
                }
            }

            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        };
    }

    private async Task ScreenCaptureWindows()
    {
        string userName = ViewModel.CurrentUser.UserName;
        string themeName = ViewModel.CurrentTheme.ThemeName;
        TsundokuLanguage language = ViewModel.CurrentUser.Language;
        string filterSegment = (ViewModel.SelectedFilter != TsundokuFilter.None && ViewModel.SelectedFilter != TsundokuFilter.Query)
            ? $"-{ViewModel.SelectedFilter.GetEnumMemberValue()}"
            : string.Empty;

        string baseFileNameWithoutExtension = $"{userName}-Collection-Screenshot-{themeName}-{language}{filterSegment}";
        string finalScreenshotPath = AppFileHelper.CreateUniqueScreenshotPath(baseFileNameWithoutExtension, ".png");

        try
        {
            var pixelSize = new PixelSize((int)Bounds.Width, (int)Bounds.Height);
            using var rtb = new RenderTargetBitmap(pixelSize, new Avalonia.Vector(96, 96));
            rtb.Render(this);
            rtb.Save(finalScreenshotPath);

            LOGGER.Info("Screenshot saved to: {Path}", finalScreenshotPath);
            await ToggleNotificationPopup($"Saved Screenshot for \"{baseFileNameWithoutExtension}\"");
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to capture or save screenshot to {Path}", finalScreenshotPath);
            await ToggleNotificationPopup($"Unable to Save Screenshot for \"{baseFileNameWithoutExtension}\"");
        }
    }

    private async Task DoShowEditSeriesInfoDialogAsync(IInteractionContext<EditSeriesInfoViewModel, MainWindowViewModel?> interaction)
    {
        EditSeriesInfoWindow dialog = App.ServiceProvider.GetRequiredService<EditSeriesInfoWindow>();
        dialog.DataContext = interaction.Input;

        MainWindowViewModel? resultFromDialog = await this.OpenManagedDialogWithResultAsync<EditSeriesInfoWindow, EditSeriesInfoViewModel, MainWindowViewModel?>(
            dialog,
            "Edit Series Info Dialog"
        );

        interaction.SetOutput(resultFromDialog);
    }

    public async void ShowNotification(string notiText) => await ToggleNotificationPopup(notiText);

    private async Task ToggleNotificationPopup(string notiText)
    {
        _notificationCts?.Cancel();
        _notificationCts = new CancellationTokenSource();
        CancellationToken token = _notificationCts.Token;

        ViewModel.NotificationText = notiText;
        NotificationPopup.IsVisible = true;
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3), token);
            NotificationPopup.IsVisible = false;
        }
        catch (TaskCanceledException) { }
    }

    private void OpenAddSeriesDialog(object sender, RoutedEventArgs args)
    {
        AddNewSeriesWindow? window = this.OpenManagedWindow<AddNewSeriesWindow, AddNewSeriesViewModel>(_newSeriesWindow, "Add New Series Window");
        if (window is not null)
        {
            AddNewSeriesButton.IsChecked = true;
            window.Closing += (s, e) => AddNewSeriesButton.IsChecked = false;
        }
    }

    private void OpenSettingsDialog(object sender, RoutedEventArgs args)
    {
        UserSettingsWindow? window = this.OpenManagedWindow<UserSettingsWindow, UserSettingsViewModel>(_userSettingsWindow, "Settings Window");
        if (window is not null)
        {
            SettingsButton.IsChecked = true;
            window.Closing += (s, e) => SettingsButton.IsChecked = false;
        }
    }

    private void OpenCollectionStatsDialog(object sender, RoutedEventArgs args)
    {
        CollectionStatsWindow? window = this.OpenManagedWindow<CollectionStatsWindow, CollectionStatsViewModel>(_collectionStatsWindow, "Collection Stats Window");
        if (window is not null)
        {
            StatsButton.IsChecked = true;
            window.Closing += (s, e) => StatsButton.IsChecked = false;
        }
    }

    private void OpenPriceAnalysisDialog(object sender, RoutedEventArgs args)
    {
        PriceAnalysisWindow? window = this.OpenManagedWindow<PriceAnalysisWindow, PriceAnalysisViewModel>(_priceAnalysisWindow, "Price Analysis Window");
        if (window is not null)
        {
            AnalysisButton.IsChecked = true;
            window.Closing += (s, e) => AnalysisButton.IsChecked = false;
        }
    }

    private void OpenThemeSettingsDialog(object sender, RoutedEventArgs args)
    {
        CollectionThemeWindow? window = this.OpenManagedWindow<CollectionThemeWindow, ThemeSettingsViewModel>(_themeSettingsWindow, "Theme Settings Window");
        if (window is not null)
        {
            ThemeButton.IsChecked = true;
            window.Closing += (s, e) => ThemeButton.IsChecked = false;
        }
    }

    private void OpenUserNotesDialog(object sender, RoutedEventArgs args)
    {
        UserNotesWindow? window = this.OpenManagedWindow<UserNotesWindow, UserNotesWindowViewModel>(_userNotesWindow, "User Notes Window");
        if (window is not null)
        {
            UserNotesButton.IsChecked = true;
            window.Closing += (s, e) => UserNotesButton.IsChecked = false;
        }
    }


    /// <summary>
    /// Changes the language for the users collection
    /// </summary>
    private void LanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageSelector.SelectedItem is TsundokuLanguage selectedItem)
        {
            ViewModel.UpdateUserLanguage(selectedItem);
            LOGGER.Info("Changed Language to {Lang}", selectedItem);
        }
    }

    private void BackToTop_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (CollectionPane.Offset.Y <= 0) return;
        CollectionPane.Offset = new Avalonia.Vector(0, 0);
    }

    /// <summary>
    /// Changes the sort on the users collection
    /// </summary>
    private void SortChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SortSelector.SelectedItem is string selectedItem)
        {
            LOGGER.Debug("Changing Sort to {sort}", selectedItem);
            ViewModel.SelectedSort = selectedItem.GetEnumValueFromMemberValue(TsundokuSort.TitleAZ);
        }
    }

    private void ToggleThemeSelector(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ThemeGrid.IsVisible = !ThemeGrid.IsVisible;
        if (ThemeGrid.IsVisible)
        {
            TsundokuTheme? current = _userService.GetMainTheme();
            if (current is not null)
            {
                ThemeListItems.SelectedItem = current;
            }
        }
    }

    private void ThemeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ThemeListItems.SelectedItem is TsundokuTheme selectedTheme)
        {
            LOGGER.Debug("Changing Theme to {theme}", selectedTheme.ThemeName);
            _userService.SetCurrentTheme(selectedTheme);
        }
    }

    /// <summary>
    /// Changes the filter on the users collection
    /// </summary>
    private void CollectionFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CollectionFilterSelector.SelectedItem is string selectedItem)
        {
            TsundokuFilter filter = selectedItem.GetEnumValueFromMemberValue(TsundokuFilter.None);
            LOGGER.Debug("Changing Filter to {filter}", filter);
            ViewModel.SelectedFilter = filter;
            ClearFilterButton.IsVisible = filter != TsundokuFilter.None;
        }
    }

    private void ClearFilterClicked(object sender, RoutedEventArgs e)
    {
        CollectionFilterSelector.SelectedIndex = -1;
        ViewModel.SelectedFilter = TsundokuFilter.None;
        ViewModel.SelectedFilterIndex = -1;
        ClearFilterButton.IsVisible = false;
        LOGGER.Debug("Cleared Filter");
    }

    private void PublisherFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PublisherFilterSelector.SelectedItem is string selectedPublisher)
        {
            LOGGER.Debug("Changing Publisher Filter to {publisher}", selectedPublisher);
            ViewModel.SelectedPublisher = selectedPublisher;
        }
        else
        {
            ViewModel.SelectedPublisher = string.Empty;
        }
    }

    private void ClearPublisherFilterClicked(object sender, RoutedEventArgs e)
    {
        PublisherFilterSelector.SelectedItem = null;
        ViewModel.SelectedPublisher = string.Empty;
        LOGGER.Debug("Cleared Publisher Filter");
    }

    /// <summary>
    /// Opens the AniList or MangaDex website link in the users browser when users clicks on the left side of the series card
    /// </summary>
    private async void OpenSiteLink(object sender, PointerPressedEventArgs args)
    {
        if (sender is Canvas { DataContext: Series series })
        {
            await ViewModelBase.OpenSiteLink(series.Link.ToString());
        }
    }

    private async void ChangeUserIcon(object sender, PointerPressedEventArgs args)
    {
        IReadOnlyList<IStorageFile> file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg"] }]
        });
        if (file.Count == 1)
        {
            ViewModel.UpdateUserIcon(Path.ChangeExtension(file[0].Path.LocalPath, ".png"));
        }
        else
        {
            LOGGER.Warn("User selected multiple files for user icon.");
        }
    }

    /// <summary>
    /// Opens my PayPal for users to donate if they want to
    /// </summary>
    private void OpenDonationLink(object sender, RoutedEventArgs args)
    {
        LOGGER.Info("Opening PayPal Donation Link");
        try
        {
            Process.Start(new ProcessStartInfo("https://www.paypal.com/donate/?business=JAYCVEJGDF4GY&no_recurring=0&item_name=Help+keep+Tsundoku+Supported+into+the+Future%21&currency_code=USD") { UseShellExecute = true });
        }
        catch (Win32Exception noBrowser)
        {
            LOGGER.Error(noBrowser.Message);
        }
        catch (Exception other)
        {
            LOGGER.Error(other.Message);
        }
    }

    /// <summary>
    /// Copies the title of the series when clicked om
    /// </summary>
    private async void CopySeriesTitleAsync(object sender, PointerPressedEventArgs args)
    {
        await ClipboardHelper.CopyToClipboardAsync(((TextBlock)sender).Text);
    }

    /// <summary>
    /// Increments the series current volume count
    /// </summary>
    public void AddVolume(object sender, RoutedEventArgs args)
    {
        if (sender is Button { DataContext: Series curSeries } && curSeries.CurVolumeCount < curSeries.MaxVolumeCount)
        {
            curSeries.CurVolumeCount += 1;
            LOGGER.Info("Added 1 Volume to {title}", curSeries.Titles[TsundokuLanguage.Romaji]);
        }
    }

    /// <summary>
    /// Decrements the series current volume count
    /// </summary>
    public void SubtractVolume(object sender, RoutedEventArgs args)
    {
        if (sender is Button { DataContext: Series curSeries } && curSeries.CurVolumeCount >= 1)
        {
            curSeries.CurVolumeCount -= 1;
            LOGGER.Info("Removed 1 Volume from {title}", curSeries.Titles[TsundokuLanguage.Romaji]);
        }
    }
}
