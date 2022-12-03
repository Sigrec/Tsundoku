using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using System;
using System.Linq;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class CollectionThemeWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public ThemeSettingsViewModel ThemeSettingsVM => DataContext as ThemeSettingsViewModel;
        private TsundokuTheme NewTheme;
        MainWindow CollectionWindow;

        public CollectionThemeWindow()
        {
            InitializeComponent();
            DataContext = new ThemeSettingsViewModel();
            //TsundokuTheme NewTheme = new;
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                //Logger.Debug(Tree);
                ThemeSettingsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                NewTheme = ThemeSettingsVM.CurrentTheme;
                // for (int x = 0; x < ThemeSettingsViewModel.UserThemes.Count(); x++)
                // {
                //     if (ThemeSettingsViewModel.UserThemes[x] == MainWindowViewModel.MainUser.MainTheme)
                //     {
                //         //ThemeSettingsViewModel.UserThemes.Move(x, 0);
                //         ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[x];
                //         break;
                //     }
                // }
                //ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[0];

                ApplyColors();
            };

            Closing += (s, e) =>
            {
                ((CollectionThemeWindow)s).Hide();
                e.Cancel = true;
            };

            MenuColorChanges();
            CollectionColorChanges();
        }

        private void SaveNewTheme(object sender, RoutedEventArgs args)
        {
            if (!NewThemeName.Text.Equals("Default"))
            {
                NewTheme.ThemeName = NewThemeName.Text;
                bool duplicateCheck = false;
                TsundokuTheme replaceTheme;
                for (int x = 0; x < ThemeSettingsViewModel.UserThemes.Count; x++)
                {
                    if (NewTheme.ThemeName.Equals(ThemeSettingsViewModel.UserThemes[x].ThemeName))
                    {
                        duplicateCheck = true;
                        replaceTheme = NewTheme.Cloning();
                        ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[1];
                        ThemeSettingsViewModel.UserThemes.Remove(ThemeSettingsViewModel.UserThemes[x]);
                        Logger.Info($"{NewTheme.ThemeName} Already Exists Replacing Color Values");
                        ThemeSettingsViewModel.UserThemes.Insert(0, replaceTheme);
                        break;
                    }
                }

                if (!duplicateCheck)
                {
                    ThemeSettingsViewModel.UserThemes.Insert(0, NewTheme.Cloning());
                    Logger.Info($"Added New Theme {NewTheme.ThemeName} to Saved Themes");
                }

                replaceTheme = null;
                ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[0];
            }
        }

        private void RemoveSavedTheme(object sender, RoutedEventArgs args)
        {
            if (ThemeSettingsViewModel.UserThemes.Count > 1 && !(ThemeSelector.SelectedItem as TsundokuTheme).ThemeName.Equals("Default"))
            {
                int curIndex = ThemeSettingsViewModel.UserThemes.IndexOf((ThemeSelector.SelectedItem as TsundokuTheme));
                for (int x = 0; x < ThemeSettingsViewModel.UserThemes.Count(); x++)
                {
                    if (x != curIndex)
                    {
                        ThemeSelector.SelectedIndex = x;
                        Logger.Info($"Removed Theme {ThemeSettingsViewModel.UserThemes[curIndex].ThemeName} From Saved Themes");
                        ThemeSettingsViewModel.UserThemes.Remove(ThemeSettingsViewModel.UserThemes[curIndex]);
                        break; 
                    }
                }
            }
        }

        private void ChangeMainTheme(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsActive)
            {
                CollectionWindow.CollectionViewModel.CurrentTheme = (ThemeSelector.SelectedItem as TsundokuTheme);
                Logger.Info($"Theme Changed To {(ThemeSelector.SelectedItem as TsundokuTheme).ThemeName}");
                ApplyColors();
            }
        }

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
        }

        private void MenuColorChanges()
        {
            Menu_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.Navigation.Background = new SolidColorBrush(Menu_BG.Color);
                    NewTheme.MenuBGColor = Menu_BG.Color.ToUint32();
                }
            };

            Username.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.Username.Foreground = new SolidColorBrush(Username.Color);
                    NewTheme.UsernameColor = Username.Color.ToUint32();
                }
            };

            Menu_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.NumVolumesCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    CollectionWindow.NumVolumesToBeCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    NewTheme.MenuTextColor = Menu_Text.Color.ToUint32();
                }
            };

            SearchBar_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SearchBar.Background = new SolidColorBrush(SearchBar_BG.Color);
                    NewTheme.SearchBarBGColor = SearchBar_BG.Color.ToUint32();
                }
            };

            SearchBar_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SearchBar.BorderBrush = new SolidColorBrush(SearchBar_Border.Color);
                    NewTheme.SearchBarBorderColor = SearchBar_Border.Color.ToUint32();
                }
            };

            SearchBar_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SearchBar.Foreground = new SolidColorBrush(SearchBar_Text.Color);
                    NewTheme.SearchBarTextColor = SearchBar_Text.Color.ToUint32();
                }
            };

            Divider.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.Navigation.BorderBrush = new SolidColorBrush(Divider.Color);
                    NewTheme.DividerColor = Divider.Color.ToUint32();
                }
            };

            MenuButton_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    SolidColorBrush menuButtonBG = new SolidColorBrush(MenuButton_BG.Color);
                    CollectionWindow.SettingsButton.Background = menuButtonBG;
                    CollectionWindow.ThemeButton.Background = menuButtonBG;
                    CollectionWindow.AddNewSeriesButton.Background = menuButtonBG;
                    CollectionWindow.DisplaySelector.Background = menuButtonBG;
                    CollectionWindow.LanguageSelector.Background = menuButtonBG;
                    NewTheme.MenuButtonBGColor = MenuButton_BG.Color.ToUint32();
                }
            };

            MenuButton_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                }
            };

            MenuButton_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    SolidColorBrush menuButtonBorder = new SolidColorBrush(MenuButton_Border.Color);
                    CollectionWindow.SettingsButton.BorderBrush = menuButtonBorder;
                    CollectionWindow.ThemeButton.BorderBrush = menuButtonBorder;
                    CollectionWindow.AddNewSeriesButton.BorderBrush = menuButtonBorder;
                    CollectionWindow.DisplaySelector.BorderBrush = menuButtonBorder;
                    CollectionWindow.LanguageSelector.BorderBrush = menuButtonBorder;
                    NewTheme.MenuButtonBorderColor = MenuButton_Border.Color.ToUint32();
                }
            };

            MenuButton_Border_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                }
            };

            MenuButton_IconAndText.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    SolidColorBrush menuButtonTextAndIconColor = new SolidColorBrush(MenuButton_IconAndText.Color);
                    CollectionWindow.SettingsButton.Foreground = menuButtonTextAndIconColor;
                    CollectionWindow.ThemeButton.Foreground = menuButtonTextAndIconColor;
                    CollectionWindow.AddNewSeriesButton.Foreground = menuButtonTextAndIconColor;
                    CollectionWindow.DisplaySelector.Foreground = menuButtonTextAndIconColor;
                    CollectionWindow.LanguageSelector.Foreground = menuButtonTextAndIconColor;
                    NewTheme.MenuButtonTextAndIconColor = MenuButton_IconAndText.Color.ToUint32();
                }
            };

            MenuButton_IconAndText_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                }
            };
        }

        private void CollectionColorChanges()
        {
            Collection_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Collection_BG.Color);
                    NewTheme.CollectionBGColor = Collection_BG.Color.ToUint32();
                }
            };

            Status_And_BookType_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionControl.GetLogicalChildren() = new SolidColorBrush(Status_And_BookType_BG.Color);
                    Logger.Debug("Status and BookType Color Changed to -> " + Status_And_BookType_BG.Color);
                    NewTheme.StatusAndBookTypeBGColor = Status_And_BookType_BG.Color.ToUint32();
                }
            };

            Status_And_BookType_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_BG_Hover.Color);
                    NewTheme.StatusAndBookTypeBGHoverColor = Status_And_BookType_BG_Hover.Color.ToUint32();
                }
            };

            Status_And_BookType_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text.Color);
                    NewTheme.StatusAndBookTypeTextColor = Status_And_BookType_Text.Color.ToUint32();
                }
            };

            Status_And_BookType_Text_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text_Hover.Color);
                    NewTheme.StatusAndBookTypeTextHoverColor = Status_And_BookType_Text_Hover.Color.ToUint32();
                }
            };

            SeriesCard_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_BG.Color);
                    NewTheme.SeriesCardBGColor = SeriesCard_BG.Color.ToUint32();
                }
            };

            SeriesCard_Title.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Title.Color);
                    NewTheme.SeriesCardTitleColor = SeriesCard_Title.Color.ToUint32();
                }
            };

            SeriesCard_Staff.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Staff.Color);
                    NewTheme.SeriesCardStaffColor = SeriesCard_Staff.Color.ToUint32();
                }
            };

            SeriesCard_Desc.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesCard_Desc.Color);
                    NewTheme.SeriesCardDescColor = SeriesCard_Desc.Color.ToUint32();
                }
            };

            SeriesProgress_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_BG.Color);
                    NewTheme.SeriesProgressBGColor = SeriesProgress_BG.Color.ToUint32();
                }
            };

            SeriesProgress_Bar.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar.Color);
                    NewTheme.SeriesProgressBarColor = SeriesProgress_Bar.Color.ToUint32();
                }
            };

            SeriesProgress_Bar_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar_BG.Color);
                    NewTheme.SeriesProgressBarBGColor = SeriesProgress_Bar_BG.Color.ToUint32();
                }
            };

            SeriesProgress_Bar_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Bar_Border.Color);
                    NewTheme.SeriesProgressBarBorderColor = SeriesProgress_Bar_Border.Color.ToUint32();
                }
            };

            SeriesProgress_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Text.Color);
                    NewTheme.SeriesProgressTextColor = SeriesProgress_Text.Color.ToUint32();
                }
            };

            SeriesProgress_Buttons_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesProgress_Buttons_Hover.Color);
                    NewTheme.SeriesProgressButtonsHoverColor = SeriesProgress_Buttons_Hover.Color.ToUint32();
                }
            };

            SeriesSwitchPaneButton_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesSwitchPaneButton_BG.Color);
                    NewTheme.SeriesSwitchPaneButtonBGColor = SeriesSwitchPaneButton_BG.Color.ToUint32();
                }
            };

            SeriesSwitchPaneButton_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesSwitchPaneButton_BG_Hover.Color);
                    NewTheme.SeriesSwitchPaneButtonBGHoverColor = SeriesSwitchPaneButton_BG_Hover.Color.ToUint32();
                }
            };

            SeriesSwitchPaneButton_Icon.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesSwitchPaneButton_Icon.Color);
                    NewTheme.SeriesSwitchPaneButtonIconColor = SeriesSwitchPaneButton_Icon.Color.ToUint32();
                }
            };

            SeriesSwitchPaneButton_Icon_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesSwitchPaneButton_Icon_Hover.Color);
                    NewTheme.SeriesSwitchPaneButtonIconHoverColor = SeriesSwitchPaneButton_Icon_Hover.Color.ToUint32();
                }
            };

            SeriesEditPane_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_BG.Color);
                    NewTheme.SeriesEditPaneBGColor = SeriesEditPane_BG.Color.ToUint32();
                }
            };

            SeriesNotes_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_BG.Color);
                    NewTheme.SeriesNotesBGColor = SeriesNotes_BG.Color.ToUint32();
                }
            };

            SeriesNotes_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_Border.Color);
                    NewTheme.SeriesNotesBorderColor = SeriesNotes_Border.Color.ToUint32();
                }
            };

            SeriesNotes_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesNotes_Text.Color);
                    NewTheme.SeriesNotesTextColor = SeriesNotes_Text.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG.Color);
                    NewTheme.SeriesEditPaneButtonsBGColor = SeriesEditPane_Buttons_BG.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_BG_Hover.Color);
                    NewTheme.SeriesEditPaneButtonsBGHoverColor = SeriesEditPane_Buttons_BG_Hover.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border.Color);
                    NewTheme.SeriesEditPaneButtonsBorderColor = SeriesEditPane_Buttons_Border.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_Border_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Border_Hover.Color);
                    NewTheme.SeriesEditPaneButtonsBorderHoverColor = SeriesEditPane_Buttons_Border_Hover.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_Icon.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon.Color);
                    NewTheme.SeriesEditPaneButtonsIconColor = SeriesEditPane_Buttons_Icon.Color.ToUint32();
                }
            };

            SeriesEditPane_Buttons_Icon_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(SeriesEditPane_Buttons_Icon_Hover.Color);
                    NewTheme.SeriesEditPaneButtonsIconHoverColor = SeriesEditPane_Buttons_Icon_Hover.Color.ToUint32();
                }
            };
        }
    }
}