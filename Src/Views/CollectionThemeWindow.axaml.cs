using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using DynamicData;
using ReactiveUI;
using System.Collections.ObjectModel;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class CollectionThemeWindow : ReactiveWindow<ThemeSettingsViewModel>
    {
        private TsundokuTheme? NewTheme { get; set; }
        private string SelectedTheme;
        public bool IsOpen, ThemeChanged = false;
        private MainWindow CollectionWindow;

        public CollectionThemeWindow()
        {
            InitializeComponent();

            DataContext = new ThemeSettingsViewModel();

            Opened += (s, e) =>
            {

                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                NewTheme = ViewModelBase.CurrentTheme.Cloning();
                SelectedTheme = ViewModelBase.CurrentTheme.ThemeName;
                ViewModelBase.CurrentTheme = NewTheme;
                IsOpen ^= true;
                ThemeChanged = false;

                int index = ThemeSettingsViewModel.UserThemesDisplay.IndexOf(ViewModelBase.CurrentTheme.ThemeName);
                ViewModel.CurThemeIndex = index != -1 ? index : ThemeSettingsViewModel.UserThemesDisplay.IndexOf("Default");
            };

            Closing += (s, e) =>
            {
                if (IsOpen)
                {
                    MainWindow.ResetMenuButton(CollectionWindow.ThemeButton);
                    ((CollectionThemeWindow)s).Hide();
                    NewThemeName.Text = "";
                    Topmost = false;
                    IsOpen ^= true;

                    TsundokuTheme curSelectedTheme = ViewModelBase.MainUser.SavedThemes.Single(theme => theme.ThemeName.Equals(SelectedTheme));
                    if (!NewTheme.Equals(curSelectedTheme))
                    {
                        LOGGER.Info("Resetting theme to {0}", curSelectedTheme.ThemeName);
                        ViewModelBase.CurrentTheme = curSelectedTheme;
                    }
                    NewTheme = null;
                }
                e.Cancel = true;
            };

            this.WhenAnyValue(x => x.NewThemeName.Text, x => x.MainColor1.Text, x => x.MainColor2.Text, x => x.TextColor1.Text, x => x.TextColor2.Text, x => x.AccentColor1.Text, x => x.AccentColor2.Text, (name, mc1, mc2, tc1, tc2, ac1, ac2) => !string.IsNullOrWhiteSpace(name) && !name.Equals("Default", StringComparison.OrdinalIgnoreCase) && !mc1.Contains('_') && !mc2.Contains('_') && !tc1.Contains('_') && !tc2.Contains('_') && !ac1.Contains('_') && !ac2.Contains('_')).Subscribe(x => ViewModel.IsGenerateThemeButtonEnabled = x);
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

            NewTheme.ThemeName = NewThemeName.Text;

            // Apply menu colors
            NewTheme.MenuBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.UsernameColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.UserIconBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.MenuTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.SearchBarBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SearchBarBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SearchBarTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.DividerColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.MenuButtonBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.MenuButtonBGHoverColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            NewTheme.MenuButtonBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.MenuButtonBorderHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.MenuButtonTextAndIconColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.MenuButtonTextAndIconHoverColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));

            // Apply Colleciton Colors
            NewTheme.CollectionBGColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            NewTheme.StatusAndBookTypeBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.StatusAndBookTypeBGHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.StatusAndBookTypeTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.StatusAndBookTypeTextHoverColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SeriesCardBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.SeriesCardTitleColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesCardPublisherColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.SeriesCardStaffColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.SeriesCardDescColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesProgressBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SeriesProgressBarColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesProgressBarBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.SeriesProgressBarBorderColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesProgressTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesProgressButtonsHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.SeriesButtonIconColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesButtonIconHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesEditPaneBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.SeriesNotesBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SeriesNotesBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesNotesTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.SeriesEditPaneButtonsBGColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            NewTheme.SeriesEditPaneButtonsBGHoverColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SeriesEditPaneButtonsBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesEditPaneButtonsBorderHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesEditPaneButtonsIconColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SeriesEditPaneButtonsIconHoverColor = new SolidColorBrush(Color.Parse(TextColor1.Text));

            // Generate Theme
            AddTheme(NewTheme);  
        }

        /// <summary>
        /// Generates the Type2 theme
        /// </summary>
        private void GenerateThemeType2(object sender, RoutedEventArgs args)
        {
           LOGGER.Info("Generating New Theme2");

            NewTheme.ThemeName = NewThemeName.Text;

            // Apply menu colors
            NewTheme.MenuBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.UsernameColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.UserIconBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.MenuTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.SearchBarBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SearchBarBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SearchBarTextColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.DividerColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.MenuButtonBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.MenuButtonBGHoverColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            NewTheme.MenuButtonBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.MenuButtonBorderHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.MenuButtonTextAndIconColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.MenuButtonTextAndIconHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));

            // Apply Colleciton Colors
            NewTheme.CollectionBGColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            NewTheme.StatusAndBookTypeBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.StatusAndBookTypeBGHoverColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.StatusAndBookTypeTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.StatusAndBookTypeTextHoverColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SeriesCardBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.SeriesCardTitleColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesCardPublisherColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.SeriesCardStaffColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.SeriesCardDescColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesProgressBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SeriesProgressBarColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.SeriesProgressBarBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.SeriesProgressBarBorderColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesProgressTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesProgressButtonsHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.SeriesButtonIconColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesButtonIconHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.SeriesEditPaneBGColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
            NewTheme.SeriesNotesBGColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
            NewTheme.SeriesNotesBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesNotesTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesEditPaneButtonsBGColor = new SolidColorBrush(Color.Parse(MainColor2.Text));
            NewTheme.SeriesEditPaneButtonsBGHoverColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
            NewTheme.SeriesEditPaneButtonsBorderColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesEditPaneButtonsBorderHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
            NewTheme.SeriesEditPaneButtonsIconColor = new SolidColorBrush(Color.Parse(TextColor2.Text));
            NewTheme.SeriesEditPaneButtonsIconHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));

            // Generate Theme
            AddTheme(NewTheme);    
        }

        private async Task ShowDialog(string title, string info = "Unable to Add Theme")
        {
            PopupWindow dialog = new PopupWindow();
            dialog.SetWindowText(title, "fa-solid fa-circle-exclamation", info);
            await dialog.ShowDialog(this);
        }

        /// <summary>
        /// Adds a new theme to the users themes
        /// </summary>
        private async void AddTheme(TsundokuTheme newTheme)
        {
            NewThemeName.Text = string.IsNullOrWhiteSpace(NewThemeName.Text) ? newTheme.ThemeName.Trim() : NewThemeName.Text.Trim();
            if (!string.IsNullOrWhiteSpace(NewThemeName.Text) && !NewThemeName.Text.Equals("Default", StringComparison.OrdinalIgnoreCase) && NewThemeName.Text.Length <= 60)
            {
                newTheme.ThemeName = NewThemeName.Text;
                bool duplicateCheck = false;
                TsundokuTheme replaceTheme = newTheme.Cloning(); 
                // Checks if the new theme already exists (some other theme has the same name as what the theme the user is currently trying to save)
                for (int x = 0; x < ViewModelBase.MainUser.SavedThemes.Count; x++)
                {
                    if (newTheme.ThemeName.Equals(ViewModelBase.MainUser.SavedThemes[x].ThemeName))
                    {
                        LOGGER.Debug($"{newTheme.ThemeName} Already Exists Replacing Theme");
                        duplicateCheck = true;

                        for (int y = 0; y < ViewModelBase.MainUser.SavedThemes.Count; y++)
                        {
                            if (y != x)
                            {
                                ViewModelBase.MainUser.SavedThemes[x] = replaceTheme;
                                ThemeSettingsViewModel.UserThemesDisplay.Replace(ViewModelBase.MainUser.SavedThemes[x].ThemeName, newTheme.ThemeName);
                                LOGGER.Info($"Replaced Theme \"{newTheme.ThemeName}\"");
                                ThemeChanged = true;
                                ThemeSelector.SelectedIndex = x;
                                return;
                            }
                        }
                    }
                }

                if (!duplicateCheck)
                {
                    int index = ViewModelBase.MainUser.SavedThemes.BinarySearch(replaceTheme);
                    index = index < 0 ? ~index : index;
                    ViewModelBase.MainUser.SavedThemes.Insert(index, replaceTheme);
                    ThemeSettingsViewModel.UserThemesDisplay.Insert(index, replaceTheme.ThemeName);
                    LOGGER.Info($"Added New Theme \"{newTheme.ThemeName}\"");
                    ThemeChanged = true;
                    ThemeSelector.SelectedIndex = index;
                }
            }
            else
            {
                await ShowDialog("Error", $"Unable to Add Theme \"{NewThemeName.Text}\"");
                LOGGER.Warn($"Empty, Invalid, or Theme Name > 60 Chars for \"{NewThemeName.Text}\"");
            }
        }

        private void SaveNewTheme(object sender, RoutedEventArgs args)
        {
            SaveTheme(NewTheme);
        }

        private void SaveTheme(TsundokuTheme newTheme)
        {
            AddTheme(newTheme);
            // ViewModelBase.MainUser.SavedThemes.Add(TsundokuTheme.DEFAULT_THEME);
            // ViewModelBase.MainUser.SavedThemes = new ObservableCollection<TsundokuTheme>(ViewModelBase.MainUser.SavedThemes.OrderBy(theme => theme.ThemeName));
            ViewModelBase.MainUser.SaveUserData();
        }

        private async void ExportThemeAsync(object sender, RoutedEventArgs args)
        {
            string themesFolderPath = AppFileHelper.GetThemesFolderPath();

            // Construct the full file path for the theme
            string themeFileName = $"{ViewModelBase.CurrentTheme.ThemeName.Replace(" ", "_")}.json";
            string themeFullPath = Path.Combine(themesFolderPath, themeFileName);

            try
            {
                // Serialize the theme and write it to the file
                string jsonContent = JsonSerializer.Serialize(ViewModelBase.CurrentTheme, typeof(TsundokuTheme), User.UserJsonModel);
                File.WriteAllText(themeFullPath, jsonContent);

                await ShowDialog("Info", $"Exported \"{ViewModelBase.CurrentTheme.ThemeName}\" Theme");
                LOGGER.Info("Exported \"{ThemeName}\" Theme to {Path}", ViewModelBase.CurrentTheme.ThemeName, themeFullPath);
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Failed to export \"{ViewModelBase.CurrentTheme.ThemeName}\" Theme.");
                LOGGER.Error(ex, "Failed to export \"{ThemeName}\" Theme to {Path}", ViewModelBase.CurrentTheme.ThemeName, themeFullPath);
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

                if (newThemeFileLocalPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Use async version of File.ReadAllText
                        string jsonContent = await File.ReadAllTextAsync(newThemeFileLocalPath);
                        TsundokuTheme? importedTheme = JsonSerializer.Deserialize(jsonContent, typeof(TsundokuTheme), User.UserJsonModel) as TsundokuTheme;

                        if (importedTheme != null)
                        {
                            SaveTheme(importedTheme);
                            await ShowDialog("Info", $"Imported \"{importedTheme.ThemeName}\" Theme");
                            LOGGER.Info("Imported Theme \"{ThemeName}\" from {Path}", importedTheme.ThemeName, newThemeFileLocalPath);
                        }
                        else
                        {
                            await ShowDialog("Error", $"Unable to Import \"{newThemeFileLocalPath}\", Deserialization failed.");
                            LOGGER.Error("Deserialization failed for theme file {File}", newThemeFileLocalPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        await ShowDialog("Error", $"Unable to Import \"{newThemeFileLocalPath}\", Invalid Data or Access Error.");
                        LOGGER.Error(ex, "Unable to Import {File}, Invalid Data or Access Error", newThemeFileLocalPath);
                    }
                }
                else
                {
                    await ShowDialog("Error", $"Unable to Import \"{newThemeFileLocalPath}\", Not a JSON file.");
                    LOGGER.Error("Unable to Import {File} - Not a JSON file", newThemeFileLocalPath);
                }
            }
            NewThemeName.Text = "";
        }

        private void RemoveSavedTheme(object sender, RoutedEventArgs args)
        {
            if (!ThemeSelector.SelectedItem.ToString().Equals("Default"))
            {
                LOGGER.Info($"Removed Theme \"{ThemeSelector.SelectedItem}\"");
                int curIndex = ViewModelBase.MainUser.SavedThemes.IndexOf(ViewModelBase.CurrentTheme);
                ViewModelBase.MainUser.SavedThemes.RemoveAt(curIndex);
                ThemeSettingsViewModel.UserThemesDisplay.RemoveAt(curIndex);
                ThemeChanged = true;
                ThemeSelector.SelectedIndex = curIndex == 0 ? curIndex : --curIndex;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void ChangeTheme()
        {
            ThemeChanged = true;
            SelectedTheme = ThemeSelector.SelectedItem == null ? SelectedTheme : ThemeSelector.SelectedItem.ToString();

            NewTheme = ViewModelBase.MainUser.SavedThemes.Single(theme => theme.ThemeName.Equals(SelectedTheme)).Cloning();
            ViewModelBase.CurrentTheme = NewTheme;

            LOGGER.Info("Theme Changed To {}", SelectedTheme);
            ThemeChanged = false;
        }

        /// <summary>
        /// SelectionChangedEvent to change the theme when a user selects it in the ComboBox
        /// </summary>
        private void ChangeMainTheme(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelector.IsDropDownOpen || ThemeChanged)
            {
                ChangeTheme();
            }
        }
    }
}