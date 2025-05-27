using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class CollectionThemeWindow : ReactiveWindow<ThemeSettingsViewModel>
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private TsundokuTheme? BackupCurrentTheme { get; set; }
        public bool IsOpen = false;
        private bool _isInitialized = false;
        private string CurThemeName;

        public CollectionThemeWindow(ThemeSettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

            Opened += (s, e) =>
            {
                TsundokuTheme curTheme = ViewModel.GetCurrentTheme();
                BackupCurrentTheme = curTheme.Cloning();
                CurThemeName = curTheme.ThemeName;
                BackupCurrentTheme.ThemeName = "Backup";
                ViewModel.OverrideCurrentTheme(BackupCurrentTheme);
                IsOpen = true;
            };

            Closing += (s, e) =>
            {
                if (IsOpen)
                {
                    this.Hide();
                    NewThemeName.Text = "";
                    Topmost = false;
                    IsOpen = false;

                    TsundokuTheme? curSelectedTheme = ViewModel.GetMainTheme();
                    if (!BackupCurrentTheme.Equals(curSelectedTheme!))
                    {
                        ViewModel.SetTheme(curSelectedTheme);
                    }
                    BackupCurrentTheme = null;
                }
                e.Cancel = true;
            };

            this.WhenAnyValue(x => x.NewThemeName.Text, x => x.MainColor1.Text, x => x.MainColor2.Text, x => x.TextColor1.Text, x => x.TextColor2.Text, x => x.AccentColor1.Text, x => x.AccentColor2.Text, (name, mc1, mc2, tc1, tc2, ac1, ac2) => _isInitialized && !string.IsNullOrWhiteSpace(name) && !name.Equals("Default", StringComparison.OrdinalIgnoreCase) && !mc1.Contains('_') && !mc2.Contains('_') && !tc1.Contains('_') && !tc2.Contains('_') && !ac1.Contains('_') && !ac2.Contains('_')).Subscribe(x => ViewModel.IsGenerateThemeButtonEnabled = x);

            this.WhenAnyValue(x => x.NewThemeName.Text, (newName) => _isInitialized && !string.IsNullOrWhiteSpace(newName) && !newName.Equals("Default", StringComparison.OrdinalIgnoreCase))
                .DistinctUntilChanged()
                .Subscribe(x => ViewModel.IsSaveThemeButtonEnabled = x);

            _isInitialized = true;
        }

        /// <summary>
        /// Clears the hex values in the text boxes for generating a theme
        /// </summary>
        private void ClearThemeValues(object sender, RoutedEventArgs args)
        {
            // LOGGER.Info("MaskFull? = {}", !string.IsNullOrWhiteSpace(NewThemeName.Text) && !NewThemeName.Text.Equals("Default", StringComparison.OrdinalIgnoreCase) && MainColor1.MaskFull == true && MainColor2.MaskFull == true && TextColor1.MaskFull == true && TextColor2.MaskFull == true && AccentColor1.MaskFull == true && AccentColor2.MaskFull == true);
            MainColor1.Clear();
            MainColor2.Clear();
            TextColor1.Clear();
            TextColor2.Clear();
            AccentColor1.Clear();
            AccentColor2.Clear();
        }

        /// <summary>
        /// Generates the Type1 theme
        /// </summary>
        private void GenerateThemeType1(object sender, RoutedEventArgs args)
        {
            LOGGER.Info("Generating New Theme1");

            BackupCurrentTheme.ThemeName = NewThemeName.Text;

            // Apply menu colors
            BackupCurrentTheme.MenuBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.UsernameColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.UserIconBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.MenuTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.SearchBarBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SearchBarBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SearchBarTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.DividerColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.MenuButtonBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.MenuButtonBGHoverColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            BackupCurrentTheme.MenuButtonBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.MenuButtonBorderHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.MenuButtonTextAndIconColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.MenuButtonTextAndIconHoverColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));

            // Apply Colleciton Colors
            BackupCurrentTheme.CollectionBGColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            BackupCurrentTheme.StatusAndBookTypeBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.StatusAndBookTypeBGHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.StatusAndBookTypeTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.StatusAndBookTypeTextHoverColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SeriesCardBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.SeriesCardTitleColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesCardPublisherColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.SeriesCardStaffColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.SeriesCardDescColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesProgressBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SeriesProgressBarColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesProgressBarBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.SeriesProgressBarBorderColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesProgressTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesProgressButtonsHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.SeriesButtonIconColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesButtonIconHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesEditPaneBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.SeriesNotesBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SeriesNotesBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesNotesTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsBGColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsBGHoverColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsBorderHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsIconColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsIconHoverColor = new SolidColorBrush(Color.Parse(TextColor1.Text));

            // Generate Theme
            ViewModel.SaveTheme(BackupCurrentTheme);  
        }

        /// <summary>
        /// Generates the Type2 theme
        /// </summary>
        private void GenerateThemeType2(object sender, RoutedEventArgs args)
        {
           LOGGER.Info("Generating New Theme2");

            BackupCurrentTheme.ThemeName = NewThemeName.Text;

            // Apply menu colors
            BackupCurrentTheme.MenuBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.UsernameColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.UserIconBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.MenuTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.SearchBarBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SearchBarBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SearchBarTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.DividerColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.MenuButtonBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.MenuButtonBGHoverColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            BackupCurrentTheme.MenuButtonBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.MenuButtonBorderHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.MenuButtonTextAndIconColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.MenuButtonTextAndIconHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));

            // Apply Colleciton Colors
            BackupCurrentTheme.CollectionBGColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            BackupCurrentTheme.StatusAndBookTypeBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.StatusAndBookTypeBGHoverColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.StatusAndBookTypeTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.StatusAndBookTypeTextHoverColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SeriesCardBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.SeriesCardTitleColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesCardPublisherColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.SeriesCardStaffColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.SeriesCardDescColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesProgressBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SeriesProgressBarColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.SeriesProgressBarBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.SeriesProgressBarBorderColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesProgressTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesProgressButtonsHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.SeriesButtonIconColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesButtonIconHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.SeriesEditPaneBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            BackupCurrentTheme.SeriesNotesBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            BackupCurrentTheme.SeriesNotesBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesNotesTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsBGColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsBGHoverColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsBorderHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsIconColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            BackupCurrentTheme.SeriesEditPaneButtonsIconHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));

            // Generate Theme
            ViewModel.SaveTheme(BackupCurrentTheme);    
        }

        private async Task ShowDialog(string title, string info = "Unable to Add Theme")
        {
            PopupWindow dialog = App.ServiceProvider.GetRequiredService<PopupWindow>();
            dialog.SetWindowText(title, "fa-solid fa-circle-exclamation", info);
            await dialog.ShowDialog(this);
        }
        private async void SaveNewTheme(object sender, RoutedEventArgs args)
        {
            try
            {
                BackupCurrentTheme.ThemeName = NewThemeName.Text.Trim();
                ViewModel.SaveTheme(BackupCurrentTheme);
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Unable to Save New Theme \"{BackupCurrentTheme.ThemeName}\"");
                LOGGER.Warn(ex.Message);
            }
        }

        private async void ExportThemeAsync(object sender, RoutedEventArgs args)
        {
            try
            {
                ViewModel.ExportTheme(CurThemeName);
                await ShowDialog("Info", $"Exported \"{CurThemeName}\" Theme");
                LOGGER.Info("Exported \"{ThemeName}\"", CurThemeName);
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Failed to export \"{CurThemeName}\" Theme.");
                LOGGER.Error(ex, "Failed to export \"{ThemeName}\"", CurThemeName);
            }
        }

        private async void ImportThemeAsync(object sender, RoutedEventArgs args)
        {
            string themesFolderPath = AppFileHelper.GetThemesFolderPath();

            // Safely await the folder retrieval
            IStorageFolder? suggestedStartFolder = await StorageProvider.TryGetFolderFromPathAsync(themesFolderPath);

            IReadOnlyList<IStorageFile> file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Theme File",
                // Use the awaited IStorageFolder result here
                SuggestedStartLocation = suggestedStartFolder,
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("JSON Theme File") { Patterns = ["*.json"] }
                ]
            });

            if (file.Count > 0 && file[0] != null)
            {
                // Get the local path from the selected file
                string newThemeFileLocalPath = file[0].Path.LocalPath;
                try
                {
                    await ViewModel.ImportThemeAsync(newThemeFileLocalPath);
                }
                catch
                {
                    await ShowDialog("Error", $"Unable to Import Theme @ \"{newThemeFileLocalPath}\"");
                }
            }
            NewThemeName.Text = "";
        }

        private async void RemoveSavedTheme(object sender, RoutedEventArgs args)
        {
            if (ThemeSelector.SelectedItem is TsundokuTheme selectedTheme)
            {
                try
                {
                    ViewModel.RemoveTheme(selectedTheme.ThemeName);
                }
                catch
                {
                    await ShowDialog("Error", $"Unable to Remove Theme \"{selectedTheme.ThemeName}\"");
                }
            }
        }

        /// <summary>
        /// SelectionChangedEvent to change the theme when a user selects it in the ComboBox
        /// </summary>
        private void ChangeMainTheme(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized && ThemeSelector.IsDropDownOpen)
            {
                if (ThemeSelector.SelectedItem is TsundokuTheme selectedTheme)
                {
                    CurThemeName = selectedTheme.ThemeName;
                    ViewModel.SetTheme(selectedTheme);
                    BackupCurrentTheme = selectedTheme.Cloning();
                    BackupCurrentTheme.ThemeName = "Backup";
                    ViewModel.OverrideCurrentTheme(BackupCurrentTheme);
                }
            }
        }
    }
}