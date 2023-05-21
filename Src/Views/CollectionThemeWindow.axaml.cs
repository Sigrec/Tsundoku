using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class CollectionThemeWindow : Window
    {
        public ThemeSettingsViewModel? ThemeSettingsVM => DataContext as ThemeSettingsViewModel;
        private TsundokuTheme NewTheme;
        public bool IsOpen = false;
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
        }

        private string GenerateThemeValidation()
        {
            string errorMessage = "";

            if(string.IsNullOrWhiteSpace(NewThemeName.Text))
            {
                errorMessage += "No Title for the Theme was Entered\n";
            }

            if (!string.IsNullOrWhiteSpace(NewThemeName.Text) && NewThemeName.Text.Equals("Default"))
            {
                errorMessage += "Title Entered Cannot be Default\n";
            }

            if (MainColor1.MaskCompleted == false)
            {
                errorMessage += "MainColor1 has a Invalid Hex Color Value\n";
            }

            if (MainColor2.MaskCompleted == false)
            {
                errorMessage += "MainColor2 has a Invalid Hex Color Value\n";
            }

            if (TextColor1.MaskCompleted == false)
            {
                errorMessage += "TextColor1 has a Invalid Hex Color Value\n";
            }

            if (TextColor2.MaskCompleted == false)
            {
                errorMessage += "TextColor2 has a Invalid Hex Color Value\n";
            }

            if (AccentColor1.MaskCompleted == false)
            {
                errorMessage += "AccentColor1 has a Invalid Hex Color Value\n";
            }

            if (AccentColor2.MaskCompleted == false)
            {
                errorMessage += "AccentColor2 has a Invalid Hex Color Value\n";
            }
            return errorMessage;
        }

        private void GenerateThemeType1(object sender, RoutedEventArgs args)
        {
            string errorMessage = GenerateThemeValidation();
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                Constants.Logger.Info("Generating New Theme");

                NewTheme.ThemeName = NewThemeName.Text;

                // Apply menu colors
                NewTheme.MenuBGColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.UsernameColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.MenuTextColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.SearchBarBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SearchBarBorderColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SearchBarTextColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.DividerColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.MenuButtonBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.MenuButtonBGHoverColor = Color.Parse(MainColor2.Text).ToUint32();
                NewTheme.MenuButtonBorderColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.MenuButtonBorderHoverColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.MenuButtonTextAndIconColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.MenuButtonTextAndIconHoverColor = Color.Parse(AccentColor2.Text).ToUint32();

                // Apply Colleciton Colors
                NewTheme.CollectionBGColor = Color.Parse(MainColor2.Text).ToUint32();
                NewTheme.StatusAndBookTypeBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.StatusAndBookTypeBGHoverColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.StatusAndBookTypeTextColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.StatusAndBookTypeTextHoverColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesCardBGColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.SeriesCardTitleColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesCardStaffColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.SeriesCardDescColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesProgressBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesProgressBarColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesProgressBarBGColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.SeriesProgressBarBorderColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesProgressTextColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesProgressButtonsHoverColor = Color.Parse(MainColor1.Text).ToUint32(); 
                NewTheme.SeriesSwitchPaneButtonBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesSwitchPaneButtonBGHoverColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesSwitchPaneButtonIconColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesSwitchPaneButtonIconHoverColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesEditPaneBGColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.SeriesNotesBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesNotesBorderColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesNotesTextColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsBGColor = Color.Parse(MainColor2.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsBGHoverColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsBorderColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsBorderHoverColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsIconColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsIconHoverColor = Color.Parse(TextColor1.Text).ToUint32();

                // Generate Theme
                ApplyTheme();    
            }
            else
            {
                Constants.Logger.Warn("User Input to Generate Theme is Invalid");
                var errorBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBox.Avalonia.DTO.MessageBoxStandardParams
                {
                    ContentTitle = "Error Generating Theme",
                    ContentMessage = errorMessage
                });
                errorBox.Show();
            }
        }

        private void GenerateThemeType2(object sender, RoutedEventArgs args)
        {
            string errorMessage = GenerateThemeValidation();
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                Constants.Logger.Info("Generating New Theme");

                NewTheme.ThemeName = NewThemeName.Text;

                // Apply menu colors
                NewTheme.MenuBGColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.UsernameColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.MenuTextColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.SearchBarBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SearchBarBorderColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SearchBarTextColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.DividerColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.MenuButtonBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.MenuButtonBGHoverColor = Color.Parse(MainColor2.Text).ToUint32();
                NewTheme.MenuButtonBorderColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.MenuButtonBorderHoverColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.MenuButtonTextAndIconColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.MenuButtonTextAndIconHoverColor = Color.Parse(MainColor1.Text).ToUint32();

                // Apply Colleciton Colors
                NewTheme.CollectionBGColor = Color.Parse(MainColor2.Text).ToUint32();
                NewTheme.StatusAndBookTypeBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.StatusAndBookTypeBGHoverColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.StatusAndBookTypeTextColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.StatusAndBookTypeTextHoverColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesCardBGColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.SeriesCardTitleColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesCardStaffColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.SeriesCardDescColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesProgressBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesProgressBarColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.SeriesProgressBarBGColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.SeriesProgressBarBorderColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesProgressTextColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesProgressButtonsHoverColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.SeriesSwitchPaneButtonBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesSwitchPaneButtonBGHoverColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesSwitchPaneButtonIconColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesSwitchPaneButtonIconHoverColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.SeriesEditPaneBGColor = Color.Parse(MainColor1.Text).ToUint32();
                NewTheme.SeriesNotesBGColor = Color.Parse(AccentColor2.Text).ToUint32();
                NewTheme.SeriesNotesBorderColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesNotesTextColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsBGColor = Color.Parse(MainColor2.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsBGHoverColor = Color.Parse(TextColor1.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsBorderColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsBorderHoverColor = Color.Parse(AccentColor1.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsIconColor = Color.Parse(TextColor2.Text).ToUint32();
                NewTheme.SeriesEditPaneButtonsIconHoverColor = Color.Parse(MainColor1.Text).ToUint32();

                // Generate Theme
                ApplyTheme();    
            }
            else
            {
                Constants.Logger.Warn("User Input to Generate Theme is Invalid");
                var errorBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBox.Avalonia.DTO.MessageBoxStandardParams
                {
                    ContentTitle = "Error Generating Theme",
                    ContentMessage = errorMessage
                });
                errorBox.Show();
            }
        }

        private void ApplyTheme()
        {
            NewThemeName.Text = NewThemeName.Text.Trim();
            if (!string.IsNullOrWhiteSpace(NewThemeName.Text) && !NewThemeName.Text.Equals("Default"))
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
                                    ThemeSelector.SelectedIndex = x;
                                    replaceTheme = null;
                                    GC.Collect();
                                    GC.WaitForPendingFinalizers();
                                    return;
                                }
                            }
                        }
                    }

                    if (!duplicateCheck)
                    {
                        ThemeSettingsViewModel.UserThemes.Insert(0, NewTheme.Cloning());
                        Constants.Logger.Info($"Added New Theme {NewTheme.ThemeName} to Saved Themes");
                        ThemeSelector.SelectedIndex = 0;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
            else
            {
                Constants.Logger.Debug($"{NewTheme.ThemeName} | {ThemeSettingsVM.CurrentTheme.ThemeName} | {NewTheme.MenuBGColor} | {ThemeSettingsVM.CurrentTheme.MenuBGColor}");
                Constants.Logger.Debug($"CAN'T SAVE THEME {!string.IsNullOrWhiteSpace(NewThemeName.Text) && !NewThemeName.Text.Equals("Default")} | {NewTheme != ThemeSettingsVM.CurrentTheme}");
            }
        }

        // TODO : test and see if forced garbage collection is actually necassary
        private void SaveNewTheme(object sender, RoutedEventArgs args)
        {
            ApplyTheme();
        }

        private void RemoveSavedTheme(object sender, RoutedEventArgs args)
        {
            if (ThemeSettingsViewModel.UserThemes.Count > 1 && !(ThemeSelector.SelectedItem as TsundokuTheme).ThemeName.Equals("Default"))
            {
                int curIndex = ThemeSettingsViewModel.UserThemes.IndexOf((ThemeSelector.SelectedItem as TsundokuTheme)); //1
                for (int x = 0; x < ThemeSettingsViewModel.UserThemes.Count(); x++)
                {
                    if (x != curIndex)
                    {
                        ThemeSelector.SelectedIndex = x;
                        Constants.Logger.Info($"Removed Theme {ThemeSettingsViewModel.UserThemes[curIndex].ThemeName} From Saved Themes");
                        ThemeSettingsViewModel.UserThemes.Remove(ThemeSettingsViewModel.UserThemes[curIndex]);
                        break; 
                    }
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void ChangeMainTheme(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsActive)
            {
                CollectionWindow.CollectionViewModel.CurrentTheme = (ThemeSelector.SelectedItem as TsundokuTheme);
                ThemeSettingsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                CollectionWindow.CollectionViewModel.newSeriesWindow.AddNewSeriesVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                CollectionWindow.CollectionViewModel.settingsWindow.UserSettingsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                
                Constants.Logger.Info($"Theme Changed To {(ThemeSelector.SelectedItem as TsundokuTheme).ThemeName}");
                ApplyColors();
            }
        }

        // private static void UpdateChartColors()
        // {
        //     // CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CountryDistribution.Clear();
        //     Constants.Logger.Debug("Updating Chart Colors");
        //     CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.CountryDistribution = new ObservableCollection<ISeries>
        //     {
        //         new PieSeries<double> 
        //         { 
        //             Values = new double[] { 1 }, 
        //             Name = "Japan",
        //             Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBGColor))
        //         },
        //         new PieSeries<double>
        //         { 
        //             Values = new double[] { 1 }, 
        //             Name = "Korea",
        //             Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBorderColor))
        //         },
        //         new PieSeries<double>
        //         { 
        //             Values = new double[] { 1 }, 
        //             Name = "America",
        //             Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonTextAndIconColor))
        //         },
        //         new PieSeries<double>
        //         { 
        //             Values = new double[] { 1 }, 
        //             Name = "China",
        //             Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionWindow.CollectionViewModel.CurrentTheme.CollectionBGColor))
        //         },
        //         new PieSeries<double>
        //         { 
        //             Values = new double[] { 1 }, 
        //             Name = "France",
        //             Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionWindow.CollectionViewModel.CurrentTheme.MenuTextColor))
        //         }
        //     };
        // }

        private void ApplyColors()
        {
            Menu_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.MenuBGColor);
            Username.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.UsernameColor);
            Menu_Text.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.MenuTextColor);
            SearchBar_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SearchBarBGColor);
            SearchBar_Border.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SearchBarBorderColor);
            SearchBar_Text.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SearchBarTextColor);
            Divider.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.DividerColor);
            MenuButton_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBGColor);
            MenuButton_BG_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBGHoverColor);
            MenuButton_Border.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBorderColor);
            MenuButton_Border_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonBorderHoverColor);
            MenuButton_IconAndText.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonTextAndIconColor);
            MenuButton_IconAndText_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.MenuButtonTextAndIconHoverColor);
            Collection_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.CollectionBGColor);
            Status_And_BookType_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeBGColor);
            Status_And_BookType_BG_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeBGHoverColor);
            Status_And_BookType_Text.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeTextColor);
            Status_And_BookType_Text_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeTextHoverColor);
            SeriesCard_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesCardBGColor);
            SeriesCard_Title.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesCardTitleColor);
            SeriesCard_Staff.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesCardStaffColor);
            SeriesCard_Desc.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesCardDescColor);
            SeriesProgress_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressBGColor);
            SeriesProgress_Bar.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressBarColor);
            SeriesProgress_Bar_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressBarBGColor);
            SeriesProgress_Bar_Border.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressBarBorderColor);
            SeriesProgress_Text.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressTextColor);
            SeriesProgress_Buttons_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesProgressButtonsHoverColor);
            SeriesSwitchPaneButton_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesSwitchPaneButtonBGColor);
            SeriesSwitchPaneButton_BG_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesSwitchPaneButtonBGHoverColor);
            SeriesSwitchPaneButton_Icon.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesSwitchPaneButtonIconColor);
            SeriesSwitchPaneButton_Icon_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesSwitchPaneButtonIconHoverColor);
            SeriesEditPane_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneBGColor);
            SeriesNotes_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesNotesBGColor);
            SeriesNotes_Border.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesNotesBorderColor);
            SeriesNotes_Text.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesNotesTextColor);
            SeriesEditPane_Buttons_BG.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsBGColor);
            SeriesEditPane_Buttons_BG_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsBGHoverColor);
            SeriesEditPane_Buttons_Border.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsBorderColor);
            SeriesEditPane_Buttons_Border_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsBorderHoverColor);
            SeriesEditPane_Buttons_Icon.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsIconColor);
            SeriesEditPane_Buttons_Icon_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.SeriesEditPaneButtonsIconHoverColor);
            // UpdateChartColors();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void MenuColorChanges()
        {
            Menu_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.Navigation.Background = new SolidColorBrush(Menu_BG.Color);
                    Menu_BG_Button.Background = new SolidColorBrush(Menu_BG.Color);
                    NewTheme.MenuBGColor = Menu_BG.Color.ToUint32();
                }
            };

            Username.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // CollectionWindow.Username.Foreground = new SolidColorBrush(Username.Color);
                    Username_Button.Background = new SolidColorBrush(Username.Color);
                    NewTheme.UsernameColor = Username.Color.ToUint32();
                }
            };

            Menu_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // CollectionWindow.NumVolumesCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    // CollectionWindow.NumVolumesToBeCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    // CollectionWindow.SearchCollectionHeader.Foreground = new SolidColorBrush(Menu_Text.Color);
                    Menu_Text_Button.Background = new SolidColorBrush(Menu_Text.Color);
                    NewTheme.MenuTextColor = Menu_Text.Color.ToUint32();
                }
            };

            SearchBar_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // CollectionWindow.SearchBar.Background = new SolidColorBrush(SearchBar_BG.Color);
                    SearchBar_BG_Button.Background = new SolidColorBrush(SearchBar_BG.Color);
                    NewTheme.SearchBarBGColor = SearchBar_BG.Color.ToUint32();
                }
            };

            SearchBar_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // CollectionWindow.SearchBar.BorderBrush = new SolidColorBrush(SearchBar_Border.Color);
                    SearchBar_Border_Button.Background = new SolidColorBrush(SearchBar_Border.Color);
                    NewTheme.SearchBarBorderColor = SearchBar_Border.Color.ToUint32();
                }
            };

            SearchBar_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // CollectionWindow.SearchBar.Foreground = new SolidColorBrush(SearchBar_Text.Color);
                    SearchBar_Text_Button.Background = new SolidColorBrush(SearchBar_Text.Color);
                    NewTheme.SearchBarTextColor = SearchBar_Text.Color.ToUint32();
                }
            };

            Divider.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // CollectionWindow.Navigation.BorderBrush = new SolidColorBrush(Divider.Color);
                    Divider_Button.Background = new SolidColorBrush(Divider.Color);
                    NewTheme.DividerColor = Divider.Color.ToUint32();
                }
            };

            MenuButton_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // SolidColorBrush menuButtonBG = new SolidColorBrush(MenuButton_BG.Color);
                    // CollectionWindow.SettingsButton.Background = menuButtonBG;
                    // CollectionWindow.ThemeButton.Background = menuButtonBG;
                    // CollectionWindow.AddNewSeriesButton.Background = menuButtonBG;
                    // CollectionWindow.DisplaySelector.Background = menuButtonBG;
                    // CollectionWindow.LanguageSelector.Background = menuButtonBG;
                    MenuButton_BG_Button.Background = new SolidColorBrush(MenuButton_BG.Color);
                    NewTheme.MenuButtonBGColor = MenuButton_BG.Color.ToUint32();
                }
            };

            MenuButton_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    MenuButton_BG_Hover_Button.Background = new SolidColorBrush(MenuButton_BG_Hover.Color);
                    NewTheme.MenuButtonBGHoverColor = MenuButton_BG_Hover.Color.ToUint32();
                }
            };

            MenuButton_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // SolidColorBrush menuButtonBorder = new SolidColorBrush(MenuButton_Border.Color);
                    // CollectionWindow.SettingsButton.BorderBrush = menuButtonBorder;
                    // CollectionWindow.ThemeButton.BorderBrush = menuButtonBorder;
                    // CollectionWindow.AddNewSeriesButton.BorderBrush = menuButtonBorder;
                    // CollectionWindow.DisplaySelector.BorderBrush = menuButtonBorder;
                    // CollectionWindow.LanguageSelector.BorderBrush = menuButtonBorder;
                    MenuButton_Border_Button.Background = new SolidColorBrush(MenuButton_Border.Color);
                    NewTheme.MenuButtonBorderColor = MenuButton_Border.Color.ToUint32();
                }
            };

            MenuButton_Border_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    MenuButton_Border_Hover_Button.Background = new SolidColorBrush(MenuButton_Border_Hover.Color);
                    NewTheme.MenuButtonBorderHoverColor = MenuButton_Border_Hover.Color.ToUint32();
                }
            };

            MenuButton_IconAndText.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // SolidColorBrush menuButtonTextAndIconColor = new SolidColorBrush(MenuButton_IconAndText.Color);
                    // CollectionWindow.SettingsButton.Foreground = menuButtonTextAndIconColor;
                    // CollectionWindow.ThemeButton.Foreground = menuButtonTextAndIconColor;
                    // CollectionWindow.AddNewSeriesButton.Foreground = menuButtonTextAndIconColor;
                    // CollectionWindow.DisplaySelector.Foreground = menuButtonTextAndIconColor;
                    // CollectionWindow.LanguageSelector.Foreground = menuButtonTextAndIconColor;
                    MenuButton_IconAndText_Button.Background = new SolidColorBrush(MenuButton_IconAndText.Color);
                    NewTheme.MenuButtonTextAndIconColor = MenuButton_IconAndText.Color.ToUint32();
                }
            };

            MenuButton_IconAndText_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                    MenuButton_IconAndText_Hover_Button.Background = new SolidColorBrush(MenuButton_IconAndText_Hover.Color);
                    NewTheme.MenuButtonTextAndIconHoverColor = MenuButton_IconAndText_Hover.Color.ToUint32();
                }
            };
        }

        private void CollectionColorChanges()
        {
            Collection_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    // CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Collection_BG.Color);
                    Collection_BG_Button.Background = new SolidColorBrush(Collection_BG.Color);
                    NewTheme.CollectionBGColor = Collection_BG.Color.ToUint32();
                }
            };

            Status_And_BookType_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.FindLogicalDescendantOfType<TextBlock>(false).Background = new SolidColorBrush(Status_And_BookType_BG.Color);
                    //((MainWindowViewModel)CollectionWindow.CollectionTheme.DataContext).CurrentTheme.StatusAndBookTypeBGColor = Status_And_BookType_BG.Color.ToUint32();
                    //ThemeSettingsVM.CurrentTheme.StatusAndBookTypeBGColor = Status_And_BookType_BG.Color.ToUint32();
                    Status_And_BookType_BG_Button.Background = new SolidColorBrush(Status_And_BookType_BG.Color);
                    NewTheme.StatusAndBookTypeBGColor = Status_And_BookType_BG.Color.ToUint32();
                }
            };

            Status_And_BookType_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_BG_Hover.Color);
                    Status_And_BookType_BG_Hover_Button.Background = new SolidColorBrush(Status_And_BookType_BG_Hover.Color);
                    NewTheme.StatusAndBookTypeBGHoverColor = Status_And_BookType_BG_Hover.Color.ToUint32();
                }
            };

            Status_And_BookType_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text.Color);
                    Status_And_BookType_Text_Button.Background = new SolidColorBrush(Status_And_BookType_Text.Color);
                    NewTheme.StatusAndBookTypeTextColor = Status_And_BookType_Text.Color.ToUint32();
                }
            };

            Status_And_BookType_Text_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text_Hover.Color);
                    Status_And_BookType_Text_Hover_Button.Background = new SolidColorBrush(Status_And_BookType_Text_Hover.Color);
                    NewTheme.StatusAndBookTypeTextHoverColor = Status_And_BookType_Text_Hover.Color.ToUint32();
                }
            };

            SeriesCard_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_BG.Color);
                    //ThemeSettingsVM.CurrentTheme.SeriesCardBGColor = SeriesCard_BG.Color.ToUint32();
                    SeriesCard_BG_Button.Background = new SolidColorBrush(SeriesCard_BG.Color);
                    NewTheme.SeriesCardBGColor = SeriesCard_BG.Color.ToUint32();
                    //ApplyThemeChanges("SeriesCardData", "DockPanel");
                }
            };

            SeriesCard_Title.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Title.Color);
                    SeriesCard_Title_Button.Background = new SolidColorBrush(SeriesCard_Title.Color);
                    NewTheme.SeriesCardTitleColor = SeriesCard_Title.Color.ToUint32();
                }
            };

            SeriesCard_Staff.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Staff.Color);
                    SeriesCard_Staff_Button.Background = new SolidColorBrush(SeriesCard_Staff.Color);
                    NewTheme.SeriesCardStaffColor = SeriesCard_Staff.Color.ToUint32();
                }
            };

            SeriesCard_Desc.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Desc.Color);
                    SeriesCard_Desc_Button.Background = new SolidColorBrush(SeriesCard_Desc.Color);
                    NewTheme.SeriesCardDescColor = SeriesCard_Desc.Color.ToUint32();
                }
            };

            SeriesProgress_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_BG.Color);
                    SeriesProgress_BG_Button.Background = new SolidColorBrush(SeriesProgress_BG.Color);
                    NewTheme.SeriesProgressBGColor = SeriesProgress_BG.Color.ToUint32();
                }
            };

            SeriesProgress_Bar.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar.Color);
                    SeriesProgress_Bar_Button.Background = new SolidColorBrush(SeriesProgress_Bar.Color);
                    NewTheme.SeriesProgressBarColor = SeriesProgress_Bar.Color.ToUint32();
                }
            };

            SeriesProgress_Bar_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar_BG.Color);
                    SeriesProgress_Bar_BG_Button.Background = new SolidColorBrush(SeriesProgress_Bar_BG.Color);
                    NewTheme.SeriesProgressBarBGColor = SeriesProgress_Bar_BG.Color.ToUint32();
                }
            };

            SeriesProgress_Bar_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar_Border.Color);
                    SeriesProgress_Bar_Border_Button.Background = new SolidColorBrush(SeriesProgress_Bar_Border.Color);
                    NewTheme.SeriesProgressBarBorderColor = SeriesProgress_Bar_Border.Color.ToUint32();
                }
            };

            SeriesProgress_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Text.Color);
                    SeriesProgress_Text_Button.Background = new SolidColorBrush(SeriesProgress_Text.Color);
                    NewTheme.SeriesProgressTextColor = SeriesProgress_Text.Color.ToUint32();
                }
            };

            SeriesProgress_Buttons_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Buttons_Hover.Color);
                    SeriesProgress_Buttons_Hover_Button.Background = new SolidColorBrush(SeriesProgress_Buttons_Hover.Color);
                    NewTheme.SeriesProgressButtonsHoverColor = SeriesProgress_Buttons_Hover.Color.ToUint32();
                }
            };

            SeriesSwitchPaneButton_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesSwitchPaneButton_BG.Color);
                    SeriesSwitchPaneButton_BG_Button.Background = new SolidColorBrush(SeriesSwitchPaneButton_BG.Color);
                    NewTheme.SeriesSwitchPaneButtonBGColor = SeriesSwitchPaneButton_BG.Color.ToUint32();
                }
            };

            SeriesSwitchPaneButton_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesSwitchPaneButton_BG_Hover.Color);
                    SeriesSwitchPaneButton_BG_Hover_Button.Background = new SolidColorBrush(SeriesSwitchPaneButton_BG_Hover.Color);
                    NewTheme.SeriesSwitchPaneButtonBGHoverColor = SeriesSwitchPaneButton_BG_Hover.Color.ToUint32();
                }
            };

            SeriesSwitchPaneButton_Icon.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesSwitchPaneButton_Icon.Color);
                    SeriesSwitchPaneButton_Icon_Button.Background = new SolidColorBrush(SeriesSwitchPaneButton_Icon.Color);
                    NewTheme.SeriesSwitchPaneButtonIconColor = SeriesSwitchPaneButton_Icon.Color.ToUint32();
                }
            };

            SeriesSwitchPaneButton_Icon_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesSwitchPaneButton_Icon_Hover.Color);
                    SeriesSwitchPaneButton_Icon_Hover_Button.Background = new SolidColorBrush(SeriesSwitchPaneButton_Icon_Hover.Color);
                    NewTheme.SeriesSwitchPaneButtonIconHoverColor = SeriesSwitchPaneButton_Icon_Hover.Color.ToUint32();
                }
            };

            SeriesEditPane_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_BG.Color);
                    SeriesEditPane_BG_Button.Background = new SolidColorBrush(SeriesEditPane_BG.Color);
                    NewTheme.SeriesEditPaneBGColor = SeriesEditPane_BG.Color.ToUint32();
                }
            };

            SeriesNotes_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_BG.Color);
                    SeriesNotes_BG_Button.Background = new SolidColorBrush(SeriesNotes_BG.Color);
                    NewTheme.SeriesNotesBGColor = SeriesNotes_BG.Color.ToUint32();
                }
            };

            SeriesNotes_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_Border.Color);
                    SeriesNotes_Border_Button.Background = new SolidColorBrush(SeriesNotes_Border.Color);
                    NewTheme.SeriesNotesBorderColor = SeriesNotes_Border.Color.ToUint32();
                }
            };

            SeriesNotes_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_Text.Color);
                    SeriesNotes_Text_Button.Background = new SolidColorBrush(SeriesNotes_Text.Color);
                    NewTheme.SeriesNotesTextColor = SeriesNotes_Text.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG.Color);
                    SeriesEditPane_Buttons_BG_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG.Color);
                    NewTheme.SeriesEditPaneButtonsBGColor = SeriesEditPane_Buttons_BG.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG_Hover.Color);
                    SeriesEditPane_Buttons_BG_Hover_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG_Hover.Color);
                    NewTheme.SeriesEditPaneButtonsBGHoverColor = SeriesEditPane_Buttons_BG_Hover.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border.Color);
                    SeriesEditPane_Buttons_Border_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border.Color);
                    NewTheme.SeriesEditPaneButtonsBorderColor = SeriesEditPane_Buttons_Border.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_Border_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border_Hover.Color);
                    SeriesEditPane_Buttons_Border_Hover_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border_Hover.Color);
                    NewTheme.SeriesEditPaneButtonsBorderHoverColor = SeriesEditPane_Buttons_Border_Hover.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_Icon.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon.Color);
                    SeriesEditPane_Buttons_Icon_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon.Color);
                    NewTheme.SeriesEditPaneButtonsIconColor = SeriesEditPane_Buttons_Icon.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_Icon_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == ColorPicker.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon_Hover.Color);
                    SeriesEditPane_Buttons_Icon_Hover_Button.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon_Hover.Color);
                    NewTheme.SeriesEditPaneButtonsIconHoverColor = SeriesEditPane_Buttons_Icon_Hover.Color.ToUint32();
                }
            };
        }
    }
}