using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DynamicData;
using ReactiveUI;
using System;
using System.Linq;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class CollectionThemeWindow : ReactiveWindow<ThemeSettingsViewModel>
    {
        public ThemeSettingsViewModel? ThemeSettingsVM => DataContext as ThemeSettingsViewModel;
        private TsundokuTheme NewTheme;
        public bool IsOpen, ThemeChanged = false;
        MainWindow CollectionWindow;

        public CollectionThemeWindow () 
        {
            InitializeComponent();
            DataContext = new ThemeSettingsViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                ThemeSettingsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                NewTheme = ThemeSettingsVM.CurrentTheme.Cloning();
                IsOpen ^= true;
                ThemeSettingsVM.CurThemeIndex = Array.IndexOf(ThemeSettingsViewModel.UserThemes.ToArray(), ThemeSettingsVM.CurrentTheme);
                ApplyColors();
            };

            Closing += (s, e) =>
            {
                ((CollectionThemeWindow)s).Hide();
                NewThemeName.Text = "";
                Topmost = false;
                IsOpen ^= true;
                e.Cancel = true;
            };
            MenuColorChanges();
            CollectionColorChanges();

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
            Constants.Logger.Info("Generating New Theme1");

            NewTheme.ThemeName = NewThemeName.Text;

            // Apply menu colors
            NewTheme.MenuBGColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.UsernameColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.MenuTextColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.SearchBarBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SearchBarBorderColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SearchBarTextColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.DividerColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.MenuButtonBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.MenuButtonBGHoverColor = Color.Parse(MainColor2.Text).ToUInt32();
            NewTheme.MenuButtonBorderColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.MenuButtonBorderHoverColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.MenuButtonTextAndIconColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.MenuButtonTextAndIconHoverColor = Color.Parse(AccentColor2.Text).ToUInt32();

            // Apply Colleciton Colors
            NewTheme.CollectionBGColor = Color.Parse(MainColor2.Text).ToUInt32();
            NewTheme.StatusAndBookTypeBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.StatusAndBookTypeBGHoverColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.StatusAndBookTypeTextColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.StatusAndBookTypeTextHoverColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesCardBGColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.SeriesCardTitleColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesCardStaffColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.SeriesCardDescColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesProgressBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesProgressBarColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesProgressBarBGColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.SeriesProgressBarBorderColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesProgressTextColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesProgressButtonsHoverColor = Color.Parse(MainColor1.Text).ToUInt32(); 
            NewTheme.SeriesButtonBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesButtonBGHoverColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesButtonIconColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesButtonIconHoverColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesEditPaneBGColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.SeriesNotesBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesNotesBorderColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesNotesTextColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsBGColor = Color.Parse(MainColor2.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsBGHoverColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsBorderColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsBorderHoverColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsIconColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsIconHoverColor = Color.Parse(TextColor1.Text).ToUInt32();

            // Generate Theme
            AddTheme();  
        }

        /// <summary>
        /// Generates the Type2 theme
        /// </summary>
        private void GenerateThemeType2(object sender, RoutedEventArgs args)
        {
           Constants.Logger.Info("Generating New Theme2");

            NewTheme.ThemeName = NewThemeName.Text;

            // Apply menu colors
            NewTheme.MenuBGColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.UsernameColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.MenuTextColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.SearchBarBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SearchBarBorderColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SearchBarTextColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.DividerColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.MenuButtonBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.MenuButtonBGHoverColor = Color.Parse(MainColor2.Text).ToUInt32();
            NewTheme.MenuButtonBorderColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.MenuButtonBorderHoverColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.MenuButtonTextAndIconColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.MenuButtonTextAndIconHoverColor = Color.Parse(MainColor1.Text).ToUInt32();

            // Apply Colleciton Colors
            NewTheme.CollectionBGColor = Color.Parse(MainColor2.Text).ToUInt32();
            NewTheme.StatusAndBookTypeBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.StatusAndBookTypeBGHoverColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.StatusAndBookTypeTextColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.StatusAndBookTypeTextHoverColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesCardBGColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.SeriesCardTitleColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesCardStaffColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.SeriesCardDescColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesProgressBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesProgressBarColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.SeriesProgressBarBGColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.SeriesProgressBarBorderColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesProgressTextColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesProgressButtonsHoverColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.SeriesButtonBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesButtonBGHoverColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesButtonIconColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesButtonIconHoverColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.SeriesEditPaneBGColor = Color.Parse(MainColor1.Text).ToUInt32();
            NewTheme.SeriesNotesBGColor = Color.Parse(AccentColor2.Text).ToUInt32();
            NewTheme.SeriesNotesBorderColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesNotesTextColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsBGColor = Color.Parse(MainColor2.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsBGHoverColor = Color.Parse(TextColor1.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsBorderColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsBorderHoverColor = Color.Parse(AccentColor1.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsIconColor = Color.Parse(TextColor2.Text).ToUInt32();
            NewTheme.SeriesEditPaneButtonsIconHoverColor = Color.Parse(MainColor1.Text).ToUInt32();

            // Generate Theme
            AddTheme();    
        }

        /// <summary>
        /// Adds a new theme to the users themes
        /// </summary>
        private void AddTheme()
        {
            NewThemeName.Text = NewThemeName.Text.Trim();
            if (!string.IsNullOrWhiteSpace(NewThemeName.Text) && !NewThemeName.Text.Equals("Default", StringComparison.OrdinalIgnoreCase))
            {
                NewTheme.ThemeName = NewThemeName.Text;
                if (!NewTheme.Equals(ThemeSettingsVM.CurrentTheme))
                {
                    bool duplicateCheck = false;
                    TsundokuTheme replaceTheme;
                    for (int x = 0; x < ThemeSettingsViewModel.UserThemes.Count; x++)
                    {
                        // Checks if the new theme already exists (some other theme has the same name as what the theme the user is currently trying to save)
                        if (NewTheme.ThemeName.Equals(ThemeSettingsViewModel.UserThemes[x].ThemeName))
                        {
                            Constants.Logger.Info($"{NewTheme.ThemeName} Already Exists Replacing Color Values at {x}");
                            duplicateCheck = true;
                            replaceTheme = NewTheme.Cloning();

                            for (int y = 0; y < ThemeSettingsViewModel.UserThemes.Count; y++)
                            {
                                if (y != x)
                                {
                                    ThemeSelector.SelectedIndex = y; // Set the current theme to the next closet theme so the new theme can be replaced
                                    ThemeSettingsViewModel.UserThemes[x] = replaceTheme;
                                    ThemeChanged = true;
                                    ThemeSelector.SelectedIndex = x;
                                    return;
                                }
                            }
                        }
                    }

                    if (!duplicateCheck)
                    {
                        ThemeChanged = true;
                        TsundokuTheme placeHolderTheme = NewTheme.Cloning();
                        int index = ThemeSettingsViewModel.UserThemes.BinarySearch(placeHolderTheme);
                        index = index < 0 ? ~index : index;
                        ThemeSettingsViewModel.UserThemes.Insert(index, placeHolderTheme);
                        Constants.Logger.Info($"Added New Theme {NewTheme.ThemeName} to Saved Themes");
                        ThemeSelector.SelectedIndex = index;

                    }
                }
            }
            else
            {
                Constants.Logger.Debug($"{NewTheme.ThemeName} | {ThemeSettingsVM.CurrentTheme.ThemeName} | {NewTheme.MenuBGColor} | {ThemeSettingsVM.CurrentTheme.MenuBGColor}");
                Constants.Logger.Error($"CAN'T SAVE THEME {!string.IsNullOrWhiteSpace(NewThemeName.Text) && !NewThemeName.Text.Equals("Default")} | {NewTheme != ThemeSettingsVM.CurrentTheme}");
            }
        }

        // TODO : test and see if forced garbage collection is actually necassary
        /// <summary>
        /// RoutedEvent method used to call the AddTheme method to save the users new theme
        /// </summary>
        private void SaveNewTheme(object sender, RoutedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(NewThemeName.Text) && !NewThemeName.Text.Equals("Default", StringComparison.OrdinalIgnoreCase))
            {
                AddTheme();
            }
            else
            {
                Constants.Logger.Info("Empty or Invalid Theme Name");
            }
        }

        /// <summary>
        /// RoutedEvent that removes the current theme the user has selected
        /// </summary>
        private void RemoveSavedTheme(object sender, RoutedEventArgs args)
        {
            if (ThemeSettingsViewModel.UserThemes.Count > 1 && !(ThemeSelector.SelectedItem as TsundokuTheme).ThemeName.Equals("Default"))
            {
                int curIndex = ThemeSettingsViewModel.UserThemes.IndexOf(ThemeSelector.SelectedItem as TsundokuTheme);
                for (int x = 0; x < ThemeSettingsViewModel.UserThemes.Count; x++)
                {
                    if (x != curIndex)
                    {
                        Constants.Logger.Info($"Removed Theme {ThemeSettingsViewModel.UserThemes[curIndex].ThemeName} From Saved Themes");
                        ThemeSettingsViewModel.UserThemes.Remove(ThemeSettingsViewModel.UserThemes[curIndex]);
                        ThemeChanged = true;
                        ThemeSelector.SelectedIndex = curIndex == 0 ? 0 : --curIndex;
                        break; 
                    }
                }
            }
        }

        /// <summary>
        /// SelectionChangedEvent to change the theme when a user selects it in the ComboBox
        /// </summary>
        private void ChangeMainTheme(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelector.IsDropDownOpen || ThemeChanged)
            {
                CollectionWindow.CollectionViewModel.CurrentTheme = ThemeSelector.SelectedItem as TsundokuTheme;
                ThemeSettingsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                CollectionWindow.CollectionViewModel.newSeriesWindow.AddNewSeriesVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                CollectionWindow.CollectionViewModel.settingsWindow.UserSettingsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                CollectionWindow.CollectionViewModel.priceAnalysisWindow.PriceAnalysisVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                
                Constants.Logger.Info($"Theme Changed To {(ThemeSelector.SelectedItem as TsundokuTheme).ThemeName}");
                ApplyColors();
                CollectionWindow.CollectionViewModel.collectionStatsWindow.UpdateChartColors();
                ThemeChanged = false;
            }
        }

        /// <summary>
        /// Applies the theme colors of the theme to the corresponding variable
        /// </summary>
        private void ApplyColors()
        {
            Menu_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.MenuBGColor);
            Username.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.UsernameColor);
            Menu_Text.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.MenuTextColor);
            SearchBar_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SearchBarBGColor);
            SearchBar_Border.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SearchBarBorderColor);
            SearchBar_Text.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SearchBarTextColor);
            Divider.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.DividerColor);
            MenuButton_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBGColor);
            MenuButton_BG_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBGHoverColor);
            MenuButton_Border.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBorderColor);
            MenuButton_Border_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBorderHoverColor);
            MenuButton_IconAndText.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonTextAndIconColor);
            MenuButton_IconAndText_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonTextAndIconHoverColor);
            Collection_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.CollectionBGColor);
            Status_And_BookType_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeBGColor);
            Status_And_BookType_BG_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeBGHoverColor);
            Status_And_BookType_Text.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeTextColor);
            Status_And_BookType_Text_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeTextHoverColor);
            SeriesCard_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesCardBGColor);
            SeriesCard_Title.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesCardTitleColor);
            SeriesCard_Staff.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesCardStaffColor);
            SeriesCard_Desc.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesCardDescColor);
            SeriesProgress_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressBGColor);
            SeriesProgress_Bar.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressBarColor);
            SeriesProgress_Bar_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressBarBGColor);
            SeriesProgress_Bar_Border.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressBarBorderColor);
            SeriesProgress_Text.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressTextColor);
            SeriesProgress_Buttons_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressButtonsHoverColor);
            SeriesButton_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesButtonBGColor);
            SeriesButton_BG_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesButtonBGHoverColor);
            SeriesButton_Icon.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesButtonIconColor);
            SeriesButton_Icon_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesButtonIconHoverColor);
            SeriesEditPane_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneBGColor);
            SeriesNotes_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesNotesBGColor);
            SeriesNotes_Border.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesNotesBorderColor);
            SeriesNotes_Text.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesNotesTextColor);
            SeriesEditPane_Buttons_BG.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsBGColor);
            SeriesEditPane_Buttons_BG_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsBGHoverColor);
            SeriesEditPane_Buttons_Border.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsBorderColor);
            SeriesEditPane_Buttons_Border_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsBorderHoverColor);
            SeriesEditPane_Buttons_Icon.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsIconColor);
            SeriesEditPane_Buttons_Icon_Hover.Color = Color.FromUInt32(CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsIconHoverColor);

            // if (CollectionWindow.CollectionViewModel.collectionStatsWindow.) { CollectionWindow.CollectionViewModel.collectionStatsWindow.UpdateChartColors(); }
        }

        /// <summary>
        /// Creates the color property changed events for the menu color pickers
        /// </summary>
        private void MenuColorChanges()
        {
            Menu_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.Navigation.Background = new SolidColorBrush(Menu_BG.Color);
                    Menu_BG_Button.Background = new SolidColorBrush(Menu_BG.Color);
                    NewTheme.MenuBGColor = Menu_BG.Color.ToUInt32();
                }
            };

            Username.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // CollectionWindow.Username.Foreground = new SolidColorBrush(Username.Color);
                    Username_Button.Background = new SolidColorBrush(Username.Color);
                    NewTheme.UsernameColor = Username.Color.ToUInt32();
                }
            };

            Menu_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // CollectionWindow.NumVolumesCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    // CollectionWindow.NumVolumesToBeCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    // CollectionWindow.SearchCollectionHeader.Foreground = new SolidColorBrush(Menu_Text.Color);
                    Menu_Text_Button.Background = new SolidColorBrush(Menu_Text.Color);
                    NewTheme.MenuTextColor = Menu_Text.Color.ToUInt32();
                }
            };

            SearchBar_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // CollectionWindow.SearchBar.Background = new SolidColorBrush(SearchBar_BG.Color);
                    SearchBar_BG_Button.Background = new SolidColorBrush(SearchBar_BG.Color);
                    NewTheme.SearchBarBGColor = SearchBar_BG.Color.ToUInt32();
                }
            };

            SearchBar_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // CollectionWindow.SearchBar.BorderBrush = new SolidColorBrush(SearchBar_Border.Color);
                    SearchBar_Border_Button.Background = new SolidColorBrush(SearchBar_Border.Color);
                    NewTheme.SearchBarBorderColor = SearchBar_Border.Color.ToUInt32();
                }
            };

            SearchBar_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // CollectionWindow.SearchBar.Foreground = new SolidColorBrush(SearchBar_Text.Color);
                    SearchBar_Text_Button.Background = new SolidColorBrush(SearchBar_Text.Color);
                    NewTheme.SearchBarTextColor = SearchBar_Text.Color.ToUInt32();
                }
            };

            Divider.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // CollectionWindow.Navigation.BorderBrush = new SolidColorBrush(Divider.Color);
                    Divider_Button.Background = new SolidColorBrush(Divider.Color);
                    NewTheme.DividerColor = Divider.Color.ToUInt32();
                }
            };

            MenuButton_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // SolidColorBrush menuButtonBG = new SolidColorBrush(MenuButton_BG.Color);
                    // CollectionWindow.SettingsButton.Background = menuButtonBG;
                    // CollectionWindow.ThemeButton.Background = menuButtonBG;
                    // CollectionWindow.AddNewSeriesButton.Background = menuButtonBG;
                    // CollectionWindow.DisplaySelector.Background = menuButtonBG;
                    // CollectionWindow.LanguageSelector.Background = menuButtonBG;
                    MenuButton_BG_Button.Background = new SolidColorBrush(MenuButton_BG.Color);
                    NewTheme.MenuButtonBGColor = MenuButton_BG.Color.ToUInt32();
                }
            };

            MenuButton_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    MenuButton_BG_Hover_Button.Background = new SolidColorBrush(MenuButton_BG_Hover.Color);
                    NewTheme.MenuButtonBGHoverColor = MenuButton_BG_Hover.Color.ToUInt32();
                }
            };

            MenuButton_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // SolidColorBrush menuButtonBorder = new SolidColorBrush(MenuButton_Border.Color);
                    // CollectionWindow.SettingsButton.BorderBrush = menuButtonBorder;
                    // CollectionWindow.ThemeButton.BorderBrush = menuButtonBorder;
                    // CollectionWindow.AddNewSeriesButton.BorderBrush = menuButtonBorder;
                    // CollectionWindow.DisplaySelector.BorderBrush = menuButtonBorder;
                    // CollectionWindow.LanguageSelector.BorderBrush = menuButtonBorder;
                    MenuButton_Border_Button.Background = new SolidColorBrush(MenuButton_Border.Color);
                    NewTheme.MenuButtonBorderColor = MenuButton_Border.Color.ToUInt32();
                }
            };

            MenuButton_Border_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    MenuButton_Border_Hover_Button.Background = new SolidColorBrush(MenuButton_Border_Hover.Color);
                    NewTheme.MenuButtonBorderHoverColor = MenuButton_Border_Hover.Color.ToUInt32();
                }
            };

            MenuButton_IconAndText.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // SolidColorBrush menuButtonTextAndIconColor = new SolidColorBrush(MenuButton_IconAndText.Color);
                    // CollectionWindow.SettingsButton.Foreground = menuButtonTextAndIconColor;
                    // CollectionWindow.ThemeButton.Foreground = menuButtonTextAndIconColor;
                    // CollectionWindow.AddNewSeriesButton.Foreground = menuButtonTextAndIconColor;
                    // CollectionWindow.DisplaySelector.Foreground = menuButtonTextAndIconColor;
                    // CollectionWindow.LanguageSelector.Foreground = menuButtonTextAndIconColor;
                    MenuButton_IconAndText_Button.Background = new SolidColorBrush(MenuButton_IconAndText.Color);
                    NewTheme.MenuButtonTextAndIconColor = MenuButton_IconAndText.Color.ToUInt32();
                }
            };

            MenuButton_IconAndText_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                    MenuButton_IconAndText_Hover_Button.Background = new SolidColorBrush(MenuButton_IconAndText_Hover.Color);
                    NewTheme.MenuButtonTextAndIconHoverColor = MenuButton_IconAndText_Hover.Color.ToUInt32();
                }
            };
        }

        /// <summary>
        /// Creates the color property changed events for the collection color pickers
        /// </summary>
        private void CollectionColorChanges()
        {
            Collection_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    // CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Collection_BG.Color);
                    Collection_BG_Button.Background = new SolidColorBrush(Collection_BG.Color);
                    NewTheme.CollectionBGColor = Collection_BG.Color.ToUInt32();
                }
            };

            Status_And_BookType_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.FindLogicalDescendantOfType<TextBlock>(false).Background = new SolidColorBrush(Status_And_BookType_BG.Color);
                    //((MainWindowViewModel)CollectionWindow.CollectionTheme.DataContext).CurrentTheme.StatusAndBookTypeBGColor = Status_And_BookType_BG.Color.ToUInt32();
                    //ThemeSettingsVM.CurrentTheme.StatusAndBookTypeBGColor = Status_And_BookType_BG.Color.ToUInt32();
                    Status_And_BookType_BG_Button.Background = new SolidColorBrush(Status_And_BookType_BG.Color);
                    NewTheme.StatusAndBookTypeBGColor = Status_And_BookType_BG.Color.ToUInt32();
                }
            };

            Status_And_BookType_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_BG_Hover.Color);
                    Status_And_BookType_BG_Hover_Button.Background = new SolidColorBrush(Status_And_BookType_BG_Hover.Color);
                    NewTheme.StatusAndBookTypeBGHoverColor = Status_And_BookType_BG_Hover.Color.ToUInt32();
                }
            };

            Status_And_BookType_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text.Color);
                    Status_And_BookType_Text_Button.Background = new SolidColorBrush(Status_And_BookType_Text.Color);
                    NewTheme.StatusAndBookTypeTextColor = Status_And_BookType_Text.Color.ToUInt32();
                }
            };

            Status_And_BookType_Text_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text_Hover.Color);
                    Status_And_BookType_Text_Hover_Button.Background = new SolidColorBrush(Status_And_BookType_Text_Hover.Color);
                    NewTheme.StatusAndBookTypeTextHoverColor = Status_And_BookType_Text_Hover.Color.ToUInt32();
                }
            };

            SeriesCard_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_BG.Color);
                    //ThemeSettingsVM.CurrentTheme.SeriesCardBGColor = SeriesCard_BG.Color.ToUInt32();
                    SeriesCard_BG_Button.Background = new SolidColorBrush(SeriesCard_BG.Color);
                    NewTheme.SeriesCardBGColor = SeriesCard_BG.Color.ToUInt32();
                    //AddThemeChanges("SeriesCardData", "DockPanel");
                }
            };

            SeriesCard_Title.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Title.Color);
                    SeriesCard_Title_Button.Background = new SolidColorBrush(SeriesCard_Title.Color);
                    NewTheme.SeriesCardTitleColor = SeriesCard_Title.Color.ToUInt32();
                }
            };

            SeriesCard_Staff.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Staff.Color);
                    SeriesCard_Staff_Button.Background = new SolidColorBrush(SeriesCard_Staff.Color);
                    NewTheme.SeriesCardStaffColor = SeriesCard_Staff.Color.ToUInt32();
                }
            };

            SeriesCard_Desc.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Desc.Color);
                    SeriesCard_Desc_Button.Background = new SolidColorBrush(SeriesCard_Desc.Color);
                    NewTheme.SeriesCardDescColor = SeriesCard_Desc.Color.ToUInt32();
                }
            };

            SeriesProgress_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_BG.Color);
                    SeriesProgress_BG_Button.Background = new SolidColorBrush(SeriesProgress_BG.Color);
                    NewTheme.SeriesProgressBGColor = SeriesProgress_BG.Color.ToUInt32();
                }
            };

            SeriesProgress_Bar.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar.Color);
                    SeriesProgress_Bar_Button.Background = new SolidColorBrush(SeriesProgress_Bar.Color);
                    NewTheme.SeriesProgressBarColor = SeriesProgress_Bar.Color.ToUInt32();
                }
            };

            SeriesProgress_Bar_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar_BG.Color);
                    SeriesProgress_Bar_BG_Button.Background = new SolidColorBrush(SeriesProgress_Bar_BG.Color);
                    NewTheme.SeriesProgressBarBGColor = SeriesProgress_Bar_BG.Color.ToUInt32();
                }
            };

            SeriesProgress_Bar_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar_Border.Color);
                    SeriesProgress_Bar_Border_Button.Background = new SolidColorBrush(SeriesProgress_Bar_Border.Color);
                    NewTheme.SeriesProgressBarBorderColor = SeriesProgress_Bar_Border.Color.ToUInt32();
                }
            };

            SeriesProgress_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Text.Color);
                    SeriesProgress_Text_Button.Background = new SolidColorBrush(SeriesProgress_Text.Color);
                    NewTheme.SeriesProgressTextColor = SeriesProgress_Text.Color.ToUInt32();
                }
            };

            SeriesProgress_Buttons_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Buttons_Hover.Color);
                    SeriesProgress_Buttons_Hover_Button.Background = new SolidColorBrush(SeriesProgress_Buttons_Hover.Color);
                    NewTheme.SeriesProgressButtonsHoverColor = SeriesProgress_Buttons_Hover.Color.ToUInt32();
                }
            };

            SeriesButton_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesButton_BG.Color);
                    SeriesButton_BG_Button.Background = new SolidColorBrush(SeriesButton_BG.Color);
                    NewTheme.SeriesButtonBGColor = SeriesButton_BG.Color.ToUInt32();
                }
            };

            SeriesButton_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesButton_BG_Hover.Color);
                    SeriesButton_BG_Hover_Button.Background = new SolidColorBrush(SeriesButton_BG_Hover.Color);
                    NewTheme.SeriesButtonBGHoverColor = SeriesButton_BG_Hover.Color.ToUInt32();
                }
            };

            SeriesButton_Icon.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesButton_Icon.Color);
                    SeriesButton_Icon_Button.Background = new SolidColorBrush(SeriesButton_Icon.Color);
                    NewTheme.SeriesButtonIconColor = SeriesButton_Icon.Color.ToUInt32();
                }
            };

            SeriesButton_Icon_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesButton_Icon_Hover.Color);
                    SeriesButton_Icon_Hover_Button.Background = new SolidColorBrush(SeriesButton_Icon_Hover.Color);
                    NewTheme.SeriesButtonIconHoverColor = SeriesButton_Icon_Hover.Color.ToUInt32();
                }
            };

            SeriesEditPane_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_BG.Color);
                    SeriesEditPane_BG_Button.Background = new SolidColorBrush(SeriesEditPane_BG.Color);
                    NewTheme.SeriesEditPaneBGColor = SeriesEditPane_BG.Color.ToUInt32();
                }
            };

            SeriesNotes_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_BG.Color);
                    SeriesNotes_BG_Button.Background = new SolidColorBrush(SeriesNotes_BG.Color);
                    NewTheme.SeriesNotesBGColor = SeriesNotes_BG.Color.ToUInt32();
                }
            };

            SeriesNotes_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_Border.Color);
                    SeriesNotes_Border_Button.Background = new SolidColorBrush(SeriesNotes_Border.Color);
                    NewTheme.SeriesNotesBorderColor = SeriesNotes_Border.Color.ToUInt32();
                }
            };

            SeriesNotes_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_Text.Color);
                    SeriesNotes_Text_Button.Background = new SolidColorBrush(SeriesNotes_Text.Color);
                    NewTheme.SeriesNotesTextColor = SeriesNotes_Text.Color.ToUInt32();
                }
            };

            SeriesEditPane_Buttons_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG.Color);
                    SeriesEditPane_Buttons_BG_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG.Color);
                    NewTheme.SeriesEditPaneButtonsBGColor = SeriesEditPane_Buttons_BG.Color.ToUInt32();
                }
            };

            SeriesEditPane_Buttons_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG_Hover.Color);
                    SeriesEditPane_Buttons_BG_Hover_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG_Hover.Color);
                    NewTheme.SeriesEditPaneButtonsBGHoverColor = SeriesEditPane_Buttons_BG_Hover.Color.ToUInt32();
                }
            };

            SeriesEditPane_Buttons_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border.Color);
                    SeriesEditPane_Buttons_Border_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border.Color);
                    NewTheme.SeriesEditPaneButtonsBorderColor = SeriesEditPane_Buttons_Border.Color.ToUInt32();
                }
            };

            SeriesEditPane_Buttons_Border_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border_Hover.Color);
                    SeriesEditPane_Buttons_Border_Hover_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border_Hover.Color);
                    NewTheme.SeriesEditPaneButtonsBorderHoverColor = SeriesEditPane_Buttons_Border_Hover.Color.ToUInt32();
                }
            };

            SeriesEditPane_Buttons_Icon.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon.Color);
                    SeriesEditPane_Buttons_Icon_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon.Color);
                    NewTheme.SeriesEditPaneButtonsIconColor = SeriesEditPane_Buttons_Icon.Color.ToUInt32();
                }
            };

            SeriesEditPane_Buttons_Icon_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorView.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon_Hover.Color);
                    SeriesEditPane_Buttons_Icon_Hover_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon_Hover.Color);
                    NewTheme.SeriesEditPaneButtonsIconHoverColor = SeriesEditPane_Buttons_Icon_Hover.Color.ToUInt32();
                }
            };
        }
    }
}