using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using FileWatcherEx;
using ReactiveUI;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reactive.Linq;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using static Tsundoku.Models.TsundokuFilterModel;
using System.Reactive.Disposables;
using static Tsundoku.Models.TsundokuLanguageModel;
using Tsundoku.Services;

namespace Tsundoku.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        WindowState previousWindowState;
        private static StringBuilder newSearchText = new StringBuilder();
        private static string itemString;
        private static readonly TimeSpan AdvancedSearchPopulateDelay = new TimeSpan(TimeSpan.TicksPerSecond / 4);

        public MainWindow(MainWindowViewModel viewModel)
        {
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
            });

            SetupAdvancedSearchBar(" & ");

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
                    LOGGER.Info($"Saving Screenshot of Collection for \"{ViewModel.CurrentTheme.ThemeName} Theme\"");
                    await ScreenCaptureWindows();
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S) // Save User Data
                {
                    ViewModel.SaveUserData();
                    await ToggleNotificationPopup($"Saved \"{ViewModel.CurrentUser.UserName}'s\" Data");
                }
                else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.F)
                {
                    if (AdvancedSearchPopup.IsVisible)
                    {
                        AdvancedSearchBar.MinimumPopulateDelay = TimeSpan.Zero;
                        AdvancedSearchBar.Text = string.Empty;
                        AdvancedSearchPopup.IsVisible ^= true;
                    }
                    else
                    {
                        AdvancedSearchPopup.IsVisible ^= true;
                        await ToggleNotificationPopup($"To Exit Advanced Search Press CTRL+F or Click Anywhere");
                    }
                }
            };

            Closing += (s, e) =>
            {
                ViewModel.SaveOnClose();
            };
        }

        private async Task ScreenCaptureWindows()
        {
            // Construct the base filename parts
            string userName = ViewModel.CurrentUser.UserName;
            string themeName = ViewModel.CurrentTheme.ThemeName;
            TsundokuLanguage language = ViewModel.CurrentUser.Language;
            string filterSegment = (ViewModel.SelectedFilter != TsundokuFilter.None && ViewModel.SelectedFilter != TsundokuFilter.Query)
                ? $"-{ViewModel.SelectedFilter.GetStringValue()}" // Assumes GetStringValue() exists for TsundokuFilter
                : string.Empty;

            // Create the full base filename without extension
            string baseFileNameWithoutExtension = $"{userName}-Collection-Screenshot-{themeName}-{language}{filterSegment}";

            // Get a unique path for the screenshot
            string finalScreenshotPath = AppFileHelper.CreateUniqueScreenshotPath(baseFileNameWithoutExtension, ".png");

            try
            {
                // The rest of your screenshot capture logic remains the same
                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)this.Width, (int)this.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Adjust point to use the screen coordinates of the window
                        g.CopyFromScreen(new System.Drawing.Point((int)this.Bounds.Left, (int)this.Bounds.Top), System.Drawing.Point.Empty, new System.Drawing.Size((int)this.Width, (int)this.Height));
                    }
                    bitmap.Save(finalScreenshotPath, System.Drawing.Imaging.ImageFormat.Png);
                }

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

        private async void OpenEditSeriesInfoWindow(object sender, RoutedEventArgs args)
        {
            Button seriesButton = (Button)sender;
            seriesButton.Foreground = ViewModel.CurrentTheme.SeriesButtonIconHoverColor;
            await ViewModel!.CreateEditSeriesDialog(seriesButton.DataContext as Series);
            seriesButton.Foreground = ViewModel.CurrentTheme.SeriesButtonIconColor;
        }

        private async Task ToggleNotificationPopup(string notiText)
        {
            ViewModel.NotificationText = notiText;
            NotificationPopup.IsVisible = true;
            await Task.Delay(TimeSpan.FromSeconds(3));
            NotificationPopup.IsVisible = false;
        }

        private void OpenAddSeriesDialog(object sender, RoutedEventArgs args)
        {
            if (this.DataContext is MainWindowViewModel viewModel)
            {
                AddNewSeriesWindow? window = this.OpenManagedWindow<AddNewSeriesWindow, AddNewSeriesViewModel>(viewModel.NewSeriesWindow, "Add New Series Window");

                if (window != null)
                {
                    AddNewSeriesButton.IsChecked = true;
                    window.Closing += (s, e) => AddNewSeriesButton.IsChecked = false;
                }
            }
            else
            {
                LOGGER.Error("MainWindow DataContext is null or not MainWindowViewModel. Cannot open Add New Series window.");
            }
        }

        private void OpenSettingsDialog(object sender, RoutedEventArgs args)
        {
            if (this.DataContext is MainWindowViewModel viewModel)
            {
                SettingsWindow? window = this.OpenManagedWindow<SettingsWindow, UserSettingsViewModel>(viewModel.SettingsWindow, "Settings Window");
                if (window != null)
                {
                    SettingsButton.IsChecked = true;
                    window.Closing += (s, e) => SettingsButton.IsChecked = false;
                }
            }
            else
            {
                LOGGER.Error("MainWindow DataContext is null or not MainWindowViewModel. Cannot open Settings window.");
            }
        }

        private void OpenCollectionStatsDialog(object sender, RoutedEventArgs args)
        {
            if (this.DataContext is MainWindowViewModel viewModel)
            {
                CollectionStatsWindow? window = this.OpenManagedWindow<CollectionStatsWindow, CollectionStatsViewModel>(viewModel.CollectionStatsWindow, "Collection Stats Window");
                if (window != null)
                {
                    StatsButton.IsChecked = true;
                    window.Closing += (s, e) => StatsButton.IsChecked = false;
                }
            }
            else
            {
                LOGGER.Error("MainWindow DataContext is null or not MainWindowViewModel. Cannot open Collection Stats window.");
            }
        }

        private void OpenPriceAnalysisDialog(object sender, RoutedEventArgs args)
        {
            if (this.DataContext is MainWindowViewModel viewModel)
            {
                PriceAnalysisWindow? window = this.OpenManagedWindow<PriceAnalysisWindow, PriceAnalysisViewModel>(viewModel.PriceAnalysisWindow, "Price Analysis Window");
                if (window != null)
                {
                    AnalysisButton.IsChecked = true;
                    window.Closing += (s, e) => AnalysisButton.IsChecked = false;
                }
            }
            else
            {
                LOGGER.Error("MainWindow DataContext is null or not MainWindowViewModel. Cannot open Price Analysis window.");
            }
        }

        private void OpenThemeSettingsDialog(object sender, RoutedEventArgs args)
        {
            if (this.DataContext is MainWindowViewModel viewModel)
            {
                CollectionThemeWindow? window = this.OpenManagedWindow<CollectionThemeWindow, ThemeSettingsViewModel>(viewModel.ThemeSettingsWindow, "Theme Settings Window");
                if (window != null)
                {
                    ThemeButton.IsChecked = true;
                    window.Closing += (s, e) => ThemeButton.IsChecked = false;
                }
            }
            else
            {
                LOGGER.Error("MainWindow DataContext is null or not MainWindowViewModel. Cannot open Theme Settings window.");
            }
        }

        private void OpenUserNotesDialog(object sender, RoutedEventArgs args)
        {
            if (this.DataContext is MainWindowViewModel viewModel)
            {
                UserNotesWindow? window = this.OpenManagedWindow<UserNotesWindow, UserNotesWindowViewModel>(viewModel.UserNotesWindow, "User Notes Window");
                if (window != null)
                {
                    UserNotesButton.IsChecked = true;
                    window.Closing += (s, e) => UserNotesButton.IsChecked = false;
                }
            }
            else
            {
                LOGGER.Error("MainWindow DataContext is null or not MainWindowViewModel. Cannot open User Notes window.");
            }
        }

        private void StartAdvancedQuery(object sender, RoutedEventArgs args)
        {
            ViewModel.AdvancedSearchQuery = AdvancedSearchBar.Text;
        }

        public void SetupAdvancedSearchBar(string delimiter)
        {
            AdvancedSearchBar.MinimumPopulateDelay = AdvancedSearchPopulateDelay; // Might just remove this delay in the end
            AdvancedSearchBar.AddHandler(PointerPressedEvent, (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(AdvancedSearchBar.Text))
                {
                    AdvancedSearchBar.IsDropDownOpen = true;
                }
            }, RoutingStrategies.Tunnel);
            
            AdvancedSearchBar.ItemSelector = (query, item) =>
            {
                newSearchText.Clear();
                if (SharedSeriesCollectionProvider.AdvancedQueryRegex().IsMatch(query))
                {
                    newSearchText.Append(query[..query.LastIndexOf(delimiter)]).Append(delimiter);
                }
                return !item.Equals("Notes==") ? newSearchText.Append(item).ToString() : newSearchText.Append(item).ToString();
            };
            AdvancedSearchBar.ItemFilter = (query, item) =>
            {
                itemString = item as string;

                // Show all available filters when the query is empty
                if (string.IsNullOrWhiteSpace(query))
                    return true;

                if (!SharedSeriesCollectionProvider.AdvancedQueryRegex().IsMatch(query))
                {
                    return itemString.StartsWith(query, StringComparison.OrdinalIgnoreCase);
                }
                else if (query.Contains(itemString) || itemString.Last() != '=' && query.Contains(itemString[..itemString.IndexOf("==")]))
                {
                    return false;
                }

                string filterName = itemString[..^2];
                if (itemString.Contains("==") && (query.Contains($"{filterName}<=") || query.Contains($"{filterName}>=")))
                {
                    return false;
                }
                else if ((itemString.Contains('>') || itemString.Contains('<')) && query.Contains($"{filterName}=="))
                {
                    return false;
                }

                return AdvancedSearchBar.IsVisible &&
                    itemString.StartsWith(query[(query.LastIndexOf(delimiter) + delimiter.Length)..], StringComparison.OrdinalIgnoreCase);
            };
        }
        
        private void UnShowAdvancedSearchPopup(object sender, PointerPressedEventArgs args)
        {
            AdvancedSearchPopup.IsVisible = false;
        }

        private void RemoveErrorMessage(object sender, KeyEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(ViewModel.AdvancedSearchQueryErrorMessage))
            {
                ViewModel.AdvancedSearchQueryErrorMessage = string.Empty;
            }
        }

        /// <summary>
        /// Changes the language for the users collection
        /// </summary>
        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string? newLang = selectedItem.Content?.ToString();
                if (newLang != null)
                {
                    ViewModel.UpdateUserLanguage(newLang);
                    LOGGER.Info("Changed Langauge to {Lang}", newLang);
                }
            }
        }

        /// <summary>
        /// Changes the filter on the users collection
        /// </summary>
        private void CollectionFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CollectionFilterSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string newFilter = selectedItem.Content?.ToString() ?? string.Empty;
                LOGGER.Debug("Changing Filter to {filter}", newFilter);
                ViewModel.SelectedFilter = newFilter.GetEnumValueFromMemberValue(TsundokuFilter.None);
            }
        }

        /// <summary>
        /// Opens the AniList or MangaDex website link in the users browser when users clicks on the left side of the series card
        /// </summary>
        private async void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            await ViewModelBase.OpenSiteLink(((sender as Canvas).DataContext as Series).Link.ToString());
        }

        private async void ChangeUserIcon(object sender, PointerPressedEventArgs args)
        {
            IReadOnlyList<IStorageFile> file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>() { new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg"] } }
            });
            if (file.Count == 1)
            {
                string path = file[0].Path.LocalPath;
                if (Path.GetExtension(path).Equals(".png"))
                {
                    Path.ChangeExtension(path, ".png");
                }
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
            LOGGER.Info($"Opening PayPal Donation Lionk");
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
            string title = ((TextBlock)sender).Text;
            LOGGER.Info("Copying {} to Clipboard", title);
            await TextCopy.ClipboardService.SetTextAsync(title);
        }

        /// <summary>
        /// Increments the series current volume count
        /// </summary>
        public void AddVolume(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext;
            if (curSeries.CurVolumeCount < curSeries.MaxVolumeCount)
            {
                ViewModel.AddUserVolumeCount();
                curSeries.CurVolumeCount += 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).GetLogicalSiblings().ElementAt(1);
                string log = $"Adding 1 Volume To \"{curSeries.Titles[TsundokuLanguage.Romaji]}\" : {volumeDisplay.Text} -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                LOGGER.Info(log + volumeDisplay.Text);

                (sender as Button).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<ProgressBar>(false).Value = curSeries.CurVolumeCount;
            }
        }

        /// <summary>
        /// Deccrements the series current volume count
        /// </summary>
        public void SubtractVolume(object sender, RoutedEventArgs args)
        {
            Series curSeries = (Series)((Button)sender).DataContext; //Get the current series data
            if (curSeries.CurVolumeCount >= 1) //Only decrement if the user currently has 1 or more volumes
            {
                ViewModel.SubstractUserVolumeCount();
                curSeries.CurVolumeCount -= 1;
                TextBlock volumeDisplay = (TextBlock)((Button)sender).GetLogicalSiblings().ElementAt(1);
                string log = $"Removing 1 Volume From \"{curSeries.Titles[TsundokuLanguage.Romaji]}\" : {volumeDisplay.Text} -> ";
                volumeDisplay.Text = curSeries.CurVolumeCount + "/" + curSeries.MaxVolumeCount;
                LOGGER.Info(log + volumeDisplay.Text);

                (sender as Button).FindLogicalAncestorOfType<Grid>(false).FindLogicalDescendantOfType<ProgressBar>(false).Value = curSeries.CurVolumeCount;
            }
        }
    }
}
