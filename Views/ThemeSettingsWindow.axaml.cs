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
                for (int x = 0; x < ThemeSettingsViewModel.UserThemes.Count(); x++)
                {
                    if (ThemeSettingsViewModel.UserThemes[x] == MainWindowViewModel.MainUser.MainTheme)
                    {
                        //ThemeSettingsViewModel.UserThemes.Move(x, 0);
                        ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[x];
                        break;
                    }
                }
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
                for (int x = 0; x < ThemeSettingsViewModel.UserThemes.Count; x++)
                {
                    if (NewTheme.ThemeName.Equals(ThemeSettingsViewModel.UserThemes[x].ThemeName))
                    {
                        duplicateCheck = true;
                        ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[1];
                        ThemeSettingsViewModel.UserThemes.Remove(ThemeSettingsViewModel.UserThemes[x]);
                        Logger.Info($"{NewTheme.ThemeName} Already Exists Replacing Color Values");
                        break;
                    }
                }

                if (!duplicateCheck)
                {
                    Logger.Info($"Added New Theme {NewTheme.ThemeName} to Saved Themes");
                }

                ThemeSettingsViewModel.UserThemes.Insert(0, NewTheme.Cloning());
                ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[0];
                //MainWindowViewModel.SaveUsersData();
                // NewTheme = ThemeSettingsVM.CurrentTheme;
                // NewTheme.ThemeName = "";
            }
        }

        private void RemoveSavedTheme(object sender, RoutedEventArgs args)
        {
            if (ThemeSettingsViewModel.UserThemes.Count > 1 && !ThemeSettingsViewModel.UserThemes[0].ThemeName.Equals("Default"))
            {
                ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[1];
                Logger.Info($"Removed Theme {ThemeSettingsViewModel.UserThemes[0].ThemeName} From Saved Themes");
                ThemeSettingsViewModel.UserThemes.Remove(ThemeSettingsViewModel.UserThemes[0]);
            }
        }

        private void ChangeMainTheme(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsEnabled)
            {
                //ThemeSettingsVM.CurrentTheme = (ThemeSelector.SelectedItem as TsundokuTheme);
                CollectionWindow.CollectionViewModel.CurrentTheme = (ThemeSelector.SelectedItem as TsundokuTheme);
                Logger.Info($"Theme Changed To {(ThemeSelector.SelectedItem as TsundokuTheme).ThemeName}");
                ApplyColors();
            }
        }

        private void MenuColorChanges()
        {
            Menu_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.Navigation.Background = new SolidColorBrush(Menu_BG.Color);
                    NewTheme.MenuBGColor = Menu_BG.Color.ToUint32();
                    Logger.Info($"Changed Color for Menu BG to {Menu_BG.Color}");
                }
            };

            Username.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.Username.Foreground = new SolidColorBrush(Username.Color);
                    NewTheme.UsernameColor = Username.Color.ToUint32();
                    Logger.Info($"Changed Color for Username to {Username.Color}");
                }
            };

            Menu_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.NumVolumesCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    CollectionWindow.NumVolumesToBeCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    NewTheme.MenuTextColor = Menu_Text.Color.ToUint32();
                    Logger.Info($"Changed Color for Menu Text to {Menu_Text.Color}");
                }
            };

            SearchBar_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SearchBar.Background = new SolidColorBrush(SearchBar_BG.Color);
                    NewTheme.SearchBarBGColor = SearchBar_BG.Color.ToUint32();
                    Logger.Info($"Changed Color for SearchBar BG to {SearchBar_BG.Color}");
                }
            };

            SearchBar_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SearchBar.BorderBrush = new SolidColorBrush(SearchBar_Border.Color);
                    NewTheme.SearchBarBorderColor = SearchBar_Border.Color.ToUint32();
                    Logger.Info($"Changed Color for SearchBar Border to {SearchBar_Border.Color}");
                }
            };

            SearchBar_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SearchBar.Foreground = new SolidColorBrush(SearchBar_Text.Color);
                    NewTheme.SearchBarTextColor = SearchBar_Text.Color.ToUint32();
                    Logger.Info($"Changed Color for SearchBar Text to {SearchBar_Text.Color}");
                }
            };

            Divider.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.Navigation.BorderBrush = new SolidColorBrush(Divider.Color);
                    NewTheme.DividerColor = Divider.Color.ToUint32();
                    Logger.Info($"Changed Color for Divider to {Divider.Color}");
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
                    Logger.Info($"Changed Color for Menu Button Text to {menuButtonBG}");
                }
            };

            MenuButton_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                    Logger.Info($"Changed Color for Menu Button Text (Hover) to {MenuButton_BG_Hover.Color}");
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
                    Logger.Info($"Changed Color for Menu Button Border to {menuButtonBorder}");
                }
            };

            MenuButton_Border_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                    Logger.Info($"Changed Color for Menu Button Border (Hover) to {MenuButton_Border_Hover.Color}");
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
                    Logger.Info($"Changed Color for Menu Button Text & Icon to {menuButtonTextAndIconColor}");
                }
            };

            MenuButton_IconAndText_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                    Logger.Info($"Changed Color for Menu Button Text & Icon (Hover) to {MenuButton_IconAndText_Hover.Color}");
                }
            };
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
            Status_And_BookType_Text.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeBGColor);
            Status_And_BookType_Text_Hover.Color = Color.FromUInt32((uint)CollectionWindow.CollectionViewModel.CurrentTheme.StatusAndBookTypeBGHoverColor);
        }

        private void CollectionColorChanges()
        {
            Collection_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Collection_BG.Color);
                    NewTheme.CollectionBGColor = Collection_BG.Color.ToUint32();
                    Logger.Info($"Changed Color for Collection BG to {Collection_BG.Color}");
                }
            };

            Status_And_BookType_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionControl.GetLogicalChildren() = new SolidColorBrush(Status_And_BookType_BG.Color);
                    NewTheme.StatusAndBookTypeBGColor = Status_And_BookType_BG.Color.ToUint32();
                    Logger.Info($"Changed Color for Status & BookType BG to {Status_And_BookType_BG.Color}");
                }
            };

            Status_And_BookType_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_BG_Hover.Color);
                    NewTheme.StatusAndBookTypeBGHoverColor = Status_And_BookType_BG_Hover.Color.ToUint32();
                    Logger.Info($"Changed Color for Status & BookType BG (Hover) to {Status_And_BookType_BG_Hover.Color}");
                }
            };

            Status_And_BookType_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text.Color);
                    NewTheme.StatusAndBookTypeTextColor = Status_And_BookType_Text.Color.ToUint32();
                    Logger.Info($"Changed Color for Status & BookType Text to {Status_And_BookType_Text.Color}");
                }
            };

            Status_And_BookType_Text_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    //CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Status_And_BookType_Text_Hover.Color);
                    NewTheme.StatusAndBookTypeTextHoverColor = Status_And_BookType_Text_Hover.Color.ToUint32();
                    Logger.Info($"Changed Color for Status & BookType Text (Hover) to {Status_And_BookType_Text_Hover.Color}");
                }
            };
        }
    }
}