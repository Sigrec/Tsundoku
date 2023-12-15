using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using DynamicData;
using ReactiveUI;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class CollectionThemeWindow : ReactiveWindow<ThemeSettingsViewModel>
    {
        public ThemeSettingsViewModel? ThemeSettingsVM => DataContext as ThemeSettingsViewModel;
        private TsundokuTheme NewTheme { get; set; }
        public bool IsOpen, ThemeChanged = false;
        MainWindow CollectionWindow;
        private static readonly FilePickerFileType fileOptions = new FilePickerFileType("JSON Source File")
        {
            Patterns = new string[1] { "*.json" }
        };

        public CollectionThemeWindow () 
        {
            InitializeComponent();
            DataContext = new ThemeSettingsViewModel();

            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                NewTheme = ThemeSettingsVM.CurrentTheme.Cloning();
                IsOpen ^= true;
                ThemeChanged = false;
                int index = ThemeSettingsViewModel.UserThemesDisplay.IndexOf(ThemeSettingsVM.CurrentTheme.ThemeName);
                ThemeSettingsVM.CurThemeIndex = index != -1 ? index : ThemeSettingsViewModel.UserThemesDisplay.IndexOf("Default");
                ApplyColors();
                MenuColorChanges();
                CollectionColorChanges();
            };

            Closing += (s, e) =>
            {
                if (IsOpen) 
                { 
                    ((CollectionThemeWindow)s).Hide();
                    NewThemeName.Text = "";
                    Topmost = false;
                    IsOpen ^= true;
                    CollectionWindow.CollectionViewModel.CurrentTheme = ThemeSettingsVM.CurrentTheme;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                e.Cancel = true;
            };

            this.WhenAnyValue(x => x.NewThemeName.Text, x => x.MainColor1.Mask.Length, x => x.MainColor2.Mask.Length, x => x.TextColor1.Mask.Length, x => x.TextColor2.Mask.Length, x => x.AccentColor1.Mask.Length, x => x.AccentColor2.Mask.Length, (name, mc1 ,mc2, tc1, tc2, ac1, ac2) => !string.IsNullOrWhiteSpace(name) && !name.Equals("Default", StringComparison.OrdinalIgnoreCase) && mc1 != 7 && mc2 != 7 && tc1 != 7 && tc2 != 7 && ac1 != 7 && ac2 != 7).Subscribe(x => ThemeSettingsVM.IsGenerateThemeButtonEnabled = x);
        }

        /// <summary>
        /// Clears the hex values in the text boxes for generating a theme
        /// </summary>
        private void ClearThemeValues(object sender, RoutedEventArgs args)
        {
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
            NewTheme.MenuBGColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.UsernameColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.MenuTextColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.SearchBarBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SearchBarBorderColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SearchBarTextColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.DividerColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.MenuButtonBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.MenuButtonBGHoverColor = Color.Parse(MainColor2.Text).ToString();
            NewTheme.MenuButtonBorderColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.MenuButtonBorderHoverColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.MenuButtonTextAndIconColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.MenuButtonTextAndIconHoverColor = Color.Parse(AccentColor2.Text).ToString();

            // Apply Colleciton Colors
            NewTheme.CollectionBGColor = Color.Parse(MainColor2.Text).ToString();
            NewTheme.StatusAndBookTypeBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.StatusAndBookTypeBGHoverColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.StatusAndBookTypeTextColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.StatusAndBookTypeTextHoverColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesCardBGColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.SeriesCardTitleColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesCardStaffColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.SeriesCardDescColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesProgressBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesProgressBarColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesProgressBarBGColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.SeriesProgressBarBorderColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesProgressTextColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesProgressButtonsHoverColor = Color.Parse(MainColor1.Text).ToString(); 
            NewTheme.SeriesButtonBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesButtonBGHoverColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesButtonIconColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesButtonIconHoverColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesEditPaneBGColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.SeriesNotesBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesNotesBorderColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesNotesTextColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.SeriesEditPaneButtonsBGColor = Color.Parse(MainColor2.Text).ToString();
            NewTheme.SeriesEditPaneButtonsBGHoverColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesEditPaneButtonsBorderColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesEditPaneButtonsBorderHoverColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesEditPaneButtonsIconColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesEditPaneButtonsIconHoverColor = Color.Parse(TextColor1.Text).ToString();

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
            NewTheme.MenuBGColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.UsernameColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.MenuTextColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.SearchBarBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SearchBarBorderColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SearchBarTextColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.DividerColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.MenuButtonBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.MenuButtonBGHoverColor = Color.Parse(MainColor2.Text).ToString();
            NewTheme.MenuButtonBorderColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.MenuButtonBorderHoverColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.MenuButtonTextAndIconColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.MenuButtonTextAndIconHoverColor = Color.Parse(MainColor1.Text).ToString();

            // Apply Colleciton Colors
            NewTheme.CollectionBGColor = Color.Parse(MainColor2.Text).ToString();
            NewTheme.StatusAndBookTypeBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.StatusAndBookTypeBGHoverColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.StatusAndBookTypeTextColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.StatusAndBookTypeTextHoverColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesCardBGColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.SeriesCardTitleColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesCardStaffColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.SeriesCardDescColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesProgressBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesProgressBarColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.SeriesProgressBarBGColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.SeriesProgressBarBorderColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesProgressTextColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesProgressButtonsHoverColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.SeriesButtonBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesButtonBGHoverColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesButtonIconColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesButtonIconHoverColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.SeriesEditPaneBGColor = Color.Parse(MainColor1.Text).ToString();
            NewTheme.SeriesNotesBGColor = Color.Parse(AccentColor2.Text).ToString();
            NewTheme.SeriesNotesBorderColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesNotesTextColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesEditPaneButtonsBGColor = Color.Parse(MainColor2.Text).ToString();
            NewTheme.SeriesEditPaneButtonsBGHoverColor = Color.Parse(TextColor1.Text).ToString();
            NewTheme.SeriesEditPaneButtonsBorderColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesEditPaneButtonsBorderHoverColor = Color.Parse(AccentColor1.Text).ToString();
            NewTheme.SeriesEditPaneButtonsIconColor = Color.Parse(TextColor2.Text).ToString();
            NewTheme.SeriesEditPaneButtonsIconHoverColor = Color.Parse(MainColor1.Text).ToString();

            // Generate Theme
            AddTheme(NewTheme);    
        }

        /// <summary>
        /// Adds a new theme to the users themes
        /// </summary>
        private void AddTheme(TsundokuTheme newTheme)
        {
            NewThemeName.Text = string.IsNullOrWhiteSpace(NewThemeName.Text) ? newTheme.ThemeName.Trim() : NewThemeName.Text.Trim();
            if (!string.IsNullOrWhiteSpace(NewThemeName.Text) && !NewThemeName.Text.Equals("Default", StringComparison.OrdinalIgnoreCase))
            {
                newTheme.ThemeName = NewThemeName.Text;
                bool duplicateCheck = false;
                TsundokuTheme replaceTheme = newTheme.Cloning(); 
                // Checks if the new theme already exists (some other theme has the same name as what the theme the user is currently trying to save)
                for (int x = 0; x < ViewModelBase.MainUser.SavedThemes.Count; x++)
                {
                    if (newTheme.ThemeName.Equals(ViewModelBase.MainUser.SavedThemes[x].ThemeName))
                    {
                        LOGGER.Info($"{newTheme.ThemeName} Already Exists Replacing Theme");
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
                LOGGER.Warn($"Empty or Invalid Theme Name {NewThemeName.Text}");
            }
        }

        private void SaveNewTheme(object sender, RoutedEventArgs args)
        {
            SaveTheme(NewTheme);
        }

        private void SaveTheme(TsundokuTheme newTheme)
        {
            AddTheme(newTheme);
            MainWindowViewModel.SaveUsersData();
            ViewModelBase.MainUser.SavedThemes.Add(TsundokuTheme.DEFAULT_THEME);
            ViewModelBase.MainUser.SavedThemes = new ObservableCollection<TsundokuTheme>(ViewModelBase.MainUser.SavedThemes.OrderBy(theme => theme.ThemeName));
        }

        private void ExportCurrentTheme(object sender, RoutedEventArgs args)
        {
            Directory.CreateDirectory(@"Themes");
            File.WriteAllText(@$"Themes\{ThemeSettingsVM.CurrentTheme.ThemeName.Replace(" ", "_")}.json", JsonSerializer.Serialize(ThemeSettingsVM.CurrentTheme, typeof(TsundokuTheme), User.UserJsonModel));
            LOGGER.Info("Exported {} Theme", ThemeSettingsVM.CurrentTheme.ThemeName);
        }

        private async void ImportCurrentTheme(object sender, RoutedEventArgs args)
        {
            var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                SuggestedStartLocation = StorageProvider.TryGetFolderFromPathAsync(@"Themes").Result,
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>() { fileOptions }
            });
            if (file.Count > 0)
            {
                string newThemeFile = file[0].Path.LocalPath;
                if (newThemeFile.EndsWith(".json"))
                {
                    try
                    {
                        TsundokuTheme importedTheme = JsonSerializer.Deserialize(File.ReadAllText(newThemeFile), typeof(TsundokuTheme), User.UserJsonModel) as TsundokuTheme;
                        SaveTheme(importedTheme);
                        LOGGER.Info("Imported Theme {}", importedTheme.ThemeName);
                    }
                    catch (Exception)
                    {
                        LOGGER.Error("Unable to Import {} Invalid Data", newThemeFile);
                    }
                }
                else
                {
                    LOGGER.Error("Unable to Import {} Not Correct File Format/Extension", newThemeFile);
                }
            }
        }

        private void RemoveSavedTheme(object sender, RoutedEventArgs args)
        {
            if (!ThemeSelector.SelectedItem.ToString().Equals("Default"))
            {
                LOGGER.Info($"Removed Theme \"{ThemeSelector.SelectedItem}\"");
                int curIndex = ViewModelBase.MainUser.SavedThemes.IndexOf(ThemeSettingsVM.CurrentTheme);
                ViewModelBase.MainUser.SavedThemes.RemoveAt(curIndex);
                ThemeSettingsViewModel.UserThemesDisplay.RemoveAt(curIndex);
                ThemeChanged = true;
                ThemeSelector.SelectedIndex = curIndex == 0 ? curIndex : --curIndex;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void UpdateMainWindowColors(TsundokuTheme newTheme)
        {
            CollectionWindow.CollectionViewModel.CurrentTheme = newTheme;
        }

        private void UpdateAllWindowColors(TsundokuTheme newTheme)
        {
            UpdateMainWindowColors(newTheme);
            MainWindowViewModel.newSeriesWindow.AddNewSeriesVM.CurrentTheme = newTheme;
            MainWindowViewModel.settingsWindow.UserSettingsVM.CurrentTheme = newTheme;
            MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.CurrentTheme = newTheme;
            MainWindowViewModel.priceAnalysisWindow.PriceAnalysisVM.CurrentTheme = newTheme;
            ThemeSettingsVM.CurrentTheme = newTheme;
            MainWindowViewModel.collectionStatsWindow.UpdateChartColors();
        }

        private void ChangeTheme()
        {
            ThemeChanged = true;
            string toThemeName = ThemeSelector.SelectedItem.ToString();

            // Update Windows
            UpdateAllWindowColors(ViewModelBase.MainUser.SavedThemes.Single(theme => theme.ThemeName.Equals(toThemeName)));
            ApplyColors();

            LOGGER.Info($"Theme Changed To \"{toThemeName}\"");
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

        /// <summary>
        /// Updates the color of the buttons
        /// </summary>
        private void ApplyColors()
        {
            Menu_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.MenuBGColor);
            Username.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.UsernameColor);
            Menu_Text.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.MenuTextColor);
            SearchBar_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SearchBarBGColor);
            SearchBar_Border.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SearchBarBorderColor);
            SearchBar_Text.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SearchBarTextColor);
            Divider.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.DividerColor);
            MenuButton_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.MenuButtonBGColor);
            MenuButton_BG_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.MenuButtonBGHoverColor);
            MenuButton_Border.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.MenuButtonBorderColor);
            MenuButton_Border_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.MenuButtonBorderHoverColor);
            MenuButton_IconAndText.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.MenuButtonTextAndIconColor);
            MenuButton_IconAndText_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.MenuButtonTextAndIconHoverColor);
            Collection_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.CollectionBGColor);
            Status_And_BookType_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.StatusAndBookTypeBGColor);
            Status_And_BookType_BG_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.StatusAndBookTypeBGHoverColor);
            Status_And_BookType_Text.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.StatusAndBookTypeTextColor);
            Status_And_BookType_Text_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.StatusAndBookTypeTextHoverColor);
            SeriesCard_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesCardBGColor);
            SeriesCard_Title.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesCardTitleColor);
            SeriesCard_Staff.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesCardStaffColor);
            SeriesCard_Desc.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesCardDescColor);
            SeriesProgress_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesProgressBGColor);
            SeriesProgress_Bar.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesProgressBarColor);
            SeriesProgress_Bar_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesProgressBarBGColor);
            SeriesProgress_Bar_Border.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesProgressBarBorderColor);
            SeriesProgress_Text.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesProgressTextColor);
            SeriesProgress_Buttons_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesProgressButtonsHoverColor);
            SeriesButton_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesButtonBGColor);
            SeriesButton_BG_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesButtonBGHoverColor);
            SeriesButton_Icon.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesButtonIconColor);
            SeriesButton_Icon_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesButtonIconHoverColor);
            SeriesEditPane_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesEditPaneBGColor);
            SeriesNotes_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesNotesBGColor);
            SeriesNotes_Border.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesNotesBorderColor);
            SeriesNotes_Text.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesNotesTextColor);
            SeriesEditPane_Buttons_BG.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesEditPaneButtonsBGColor);
            SeriesEditPane_Buttons_BG_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesEditPaneButtonsBGHoverColor);
            SeriesEditPane_Buttons_Border.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesEditPaneButtonsBorderColor);
            SeriesEditPane_Buttons_Border_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesEditPaneButtonsBorderHoverColor);
            SeriesEditPane_Buttons_Icon.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesEditPaneButtonsIconColor);
            SeriesEditPane_Buttons_Icon_Hover.Color = Color.Parse(ThemeSettingsVM.CurrentTheme.SeriesEditPaneButtonsIconHoverColor);

            // if (MainWindowViewModel.collectionStatsWindow.) { MainWindowViewModel.collectionStatsWindow.UpdateChartColors(); }
        }

        /// <summary>
        /// Creates the color property changed events for the menu color pickers
        /// </summary>
        private void MenuColorChanges()
        {
            Menu_BG.ColorChanged += (sender, e) =>
            {
                Menu_BG_Button.Background = new SolidColorBrush(Menu_BG.Color);
                NewTheme.MenuBGColor = Menu_BG.Color.ToString();
                if (!ThemeChanged) { if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); } }
                LOGGER.Debug("TEST");
            };

            Username.ColorChanged += (sender, e) =>
            {
                Username_Button.Background = new SolidColorBrush(Username.Color);
                NewTheme.UsernameColor = Username.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            Menu_Text.ColorChanged += (sender, e) =>
            {
                Menu_Text_Button.Background = new SolidColorBrush(Menu_Text.Color);
                NewTheme.MenuTextColor = Menu_Text.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            SearchBar_BG.ColorChanged += (sender, e) =>
            {
                SearchBar_BG_Button.Background = new SolidColorBrush(SearchBar_BG.Color);
                NewTheme.SearchBarBGColor = SearchBar_BG.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            SearchBar_Border.ColorChanged += (sender, e) =>
            {
                SearchBar_Border_Button.Background = new SolidColorBrush(SearchBar_Border.Color);
                NewTheme.SearchBarBorderColor = SearchBar_Border.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            SearchBar_Text.ColorChanged += (sender, e) =>
            {
                SearchBar_Text_Button.Background = new SolidColorBrush(SearchBar_Text.Color);
                NewTheme.SearchBarTextColor = SearchBar_Text.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            Divider.ColorChanged += (sender, e) =>
            {
                Divider_Button.Background = new SolidColorBrush(Divider.Color);
                NewTheme.DividerColor = Divider.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            MenuButton_BG.ColorChanged += (sender, e) =>
            {
                MenuButton_BG_Button.Background = new SolidColorBrush(MenuButton_BG.Color);
                NewTheme.MenuButtonBGColor = MenuButton_BG.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            MenuButton_BG_Hover.ColorChanged += (sender, e) =>
            {
                MenuButton_BG_Hover_Button.Background = new SolidColorBrush(MenuButton_BG_Hover.Color);
                NewTheme.MenuButtonBGHoverColor = MenuButton_BG_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            MenuButton_Border.ColorChanged += (sender, e) =>
            {
                MenuButton_Border_Button.Background = new SolidColorBrush(MenuButton_Border.Color);
                NewTheme.MenuButtonBorderColor = MenuButton_Border.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            MenuButton_Border_Hover.ColorChanged += (sender, e) =>
            {
                MenuButton_Border_Hover_Button.Background = new SolidColorBrush(MenuButton_Border_Hover.Color);
                NewTheme.MenuButtonBorderHoverColor = MenuButton_Border_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            MenuButton_IconAndText.ColorChanged += (sender, e) =>
            {
                MenuButton_IconAndText_Button.Background = new SolidColorBrush(MenuButton_IconAndText.Color);
                NewTheme.MenuButtonTextAndIconColor = MenuButton_IconAndText.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            MenuButton_IconAndText_Hover.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                MenuButton_IconAndText_Hover_Button.Background = new SolidColorBrush(MenuButton_IconAndText_Hover.Color);
                NewTheme.MenuButtonTextAndIconHoverColor = MenuButton_IconAndText_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };
        }

        /// <summary>
        /// Creates the color property changed events for the collection color pickers
        /// </summary>
        private void CollectionColorChanges()
        {
            Collection_BG.ColorChanged += (sender, e) =>
            {
                Collection_BG_Button.Background = new SolidColorBrush(Collection_BG.Color);
                NewTheme.CollectionBGColor = Collection_BG.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            Status_And_BookType_BG.ColorChanged += (sender, e) =>
            {
                Status_And_BookType_BG_Button.Background = new SolidColorBrush(Status_And_BookType_BG.Color);
                NewTheme.StatusAndBookTypeBGColor = Status_And_BookType_BG.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            Status_And_BookType_BG_Hover.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_BG_Hover.Color);
                Status_And_BookType_BG_Hover_Button.Background = new SolidColorBrush(Status_And_BookType_BG_Hover.Color);
                NewTheme.StatusAndBookTypeBGHoverColor = Status_And_BookType_BG_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            Status_And_BookType_Text.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text.Color);
                Status_And_BookType_Text_Button.Background = new SolidColorBrush(Status_And_BookType_Text.Color);
                NewTheme.StatusAndBookTypeTextColor = Status_And_BookType_Text.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            Status_And_BookType_Text_Hover.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text_Hover.Color);
                Status_And_BookType_Text_Hover_Button.Background = new SolidColorBrush(Status_And_BookType_Text_Hover.Color);
                NewTheme.StatusAndBookTypeTextHoverColor = Status_And_BookType_Text_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesCard_BG.ColorChanged += (sender, e) =>
            {
                // foreach (var x in CollectionWindow.CollectionItems.GetLogicalChildren())
                // {
                //     (x.FindLogicalDescendantOfType<Grid>(false).FindLogicalDescendantOfType<Grid>(false).GetLogicalChildren().ElementAt(2) as Grid).Background = new SolidColorBrush(SeriesCard_BG.Color);
                // }
                SeriesCard_BG_Button.Background = new SolidColorBrush(SeriesCard_BG.Color);
                NewTheme.SeriesCardBGColor = SeriesCard_BG.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesCard_Title.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Title.Color);
                SeriesCard_Title_Button.Background = new SolidColorBrush(SeriesCard_Title.Color);
                NewTheme.SeriesCardTitleColor = SeriesCard_Title.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesCard_Staff.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Staff.Color);
                SeriesCard_Staff_Button.Background = new SolidColorBrush(SeriesCard_Staff.Color);
                NewTheme.SeriesCardStaffColor = SeriesCard_Staff.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesCard_Desc.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Desc.Color);
                SeriesCard_Desc_Button.Background = new SolidColorBrush(SeriesCard_Desc.Color);
                NewTheme.SeriesCardDescColor = SeriesCard_Desc.Color.ToString();
                if (!ThemeChanged) { UpdateAllWindowColors(NewTheme.Cloning()); }
            };

            SeriesProgress_BG.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_BG.Color);
                SeriesProgress_BG_Button.Background = new SolidColorBrush(SeriesProgress_BG.Color);
                NewTheme.SeriesProgressBGColor = SeriesProgress_BG.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesProgress_Bar.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar.Color);
                SeriesProgress_Bar_Button.Background = new SolidColorBrush(SeriesProgress_Bar.Color);
                NewTheme.SeriesProgressBarColor = SeriesProgress_Bar.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesProgress_Bar_BG.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar_BG.Color);
                SeriesProgress_Bar_BG_Button.Background = new SolidColorBrush(SeriesProgress_Bar_BG.Color);
                NewTheme.SeriesProgressBarBGColor = SeriesProgress_Bar_BG.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesProgress_Bar_Border.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar_Border.Color);
                SeriesProgress_Bar_Border_Button.Background = new SolidColorBrush(SeriesProgress_Bar_Border.Color);
                NewTheme.SeriesProgressBarBorderColor = SeriesProgress_Bar_Border.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesProgress_Text.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Text.Color);
                SeriesProgress_Text_Button.Background = new SolidColorBrush(SeriesProgress_Text.Color);
                NewTheme.SeriesProgressTextColor = SeriesProgress_Text.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesProgress_Buttons_Hover.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Buttons_Hover.Color);
                SeriesProgress_Buttons_Hover_Button.Background = new SolidColorBrush(SeriesProgress_Buttons_Hover.Color);
                NewTheme.SeriesProgressButtonsHoverColor = SeriesProgress_Buttons_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesButton_BG.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesButton_BG.Color);
                SeriesButton_BG_Button.Background = new SolidColorBrush(SeriesButton_BG.Color);
                NewTheme.SeriesButtonBGColor = SeriesButton_BG.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesButton_BG_Hover.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesButton_BG_Hover.Color);
                SeriesButton_BG_Hover_Button.Background = new SolidColorBrush(SeriesButton_BG_Hover.Color);
                NewTheme.SeriesButtonBGHoverColor = SeriesButton_BG_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesButton_Icon.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesButton_Icon.Color);
                SeriesButton_Icon_Button.Background = new SolidColorBrush(SeriesButton_Icon.Color);
                NewTheme.SeriesButtonIconColor = SeriesButton_Icon.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesButton_Icon_Hover.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesButton_Icon_Hover.Color);
                SeriesButton_Icon_Hover_Button.Background = new SolidColorBrush(SeriesButton_Icon_Hover.Color);
                NewTheme.SeriesButtonIconHoverColor = SeriesButton_Icon_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesEditPane_BG.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_BG.Color);
                SeriesEditPane_BG_Button.Background = new SolidColorBrush(SeriesEditPane_BG.Color);
                NewTheme.SeriesEditPaneBGColor = SeriesEditPane_BG.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesNotes_BG.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_BG.Color);
                SeriesNotes_BG_Button.Background = new SolidColorBrush(SeriesNotes_BG.Color);
                NewTheme.SeriesNotesBGColor = SeriesNotes_BG.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesNotes_Border.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_Border.Color);
                SeriesNotes_Border_Button.Background = new SolidColorBrush(SeriesNotes_Border.Color);
                NewTheme.SeriesNotesBorderColor = SeriesNotes_Border.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesNotes_Text.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_Text.Color);
                SeriesNotes_Text_Button.Background = new SolidColorBrush(SeriesNotes_Text.Color);
                NewTheme.SeriesNotesTextColor = SeriesNotes_Text.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesEditPane_Buttons_BG.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG.Color);
                SeriesEditPane_Buttons_BG_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG.Color);
                NewTheme.SeriesEditPaneButtonsBGColor = SeriesEditPane_Buttons_BG.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesEditPane_Buttons_BG_Hover.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG_Hover.Color);
                SeriesEditPane_Buttons_BG_Hover_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG_Hover.Color);
                NewTheme.SeriesEditPaneButtonsBGHoverColor = SeriesEditPane_Buttons_BG_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesEditPane_Buttons_Border.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border.Color);
                SeriesEditPane_Buttons_Border_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border.Color);
                NewTheme.SeriesEditPaneButtonsBorderColor = SeriesEditPane_Buttons_Border.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesEditPane_Buttons_Border_Hover.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border_Hover.Color);
                SeriesEditPane_Buttons_Border_Hover_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border_Hover.Color);
                NewTheme.SeriesEditPaneButtonsBorderHoverColor = SeriesEditPane_Buttons_Border_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesEditPane_Buttons_Icon.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon.Color);
                SeriesEditPane_Buttons_Icon_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon.Color);
                NewTheme.SeriesEditPaneButtonsIconColor = SeriesEditPane_Buttons_Icon.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };

            SeriesEditPane_Buttons_Icon_Hover.ColorChanged += (sender, e) =>
            {
                //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon_Hover.Color);
                SeriesEditPane_Buttons_Icon_Hover_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon_Hover.Color);
                NewTheme.SeriesEditPaneButtonsIconHoverColor = SeriesEditPane_Buttons_Icon_Hover.Color.ToString();
                if (!ThemeChanged) { UpdateMainWindowColors(NewTheme.Cloning()); }
            };
        }
    }
}