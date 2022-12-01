using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Drawing;
using System.Reactive.Linq;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class CollectionThemeWindow : ReactiveWindow<MainWindowViewModel>
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public ThemeSettingsViewModel ThemeVM => DataContext as ThemeSettingsViewModel;
        private TsundokuTheme NewTheme;
        MainWindow CollectionWindow;

        public CollectionThemeWindow()
        {
            InitializeComponent();
            DataContext = new ThemeSettingsViewModel();
            Activated += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                NewTheme = new TsundokuTheme();
            };

            Closing += (s, e) =>
            {
                ((CollectionThemeWindow)s).Hide();
                e.Cancel = true;
                ApplyTheme();
            };

            MenuColorChanges();
            CollectionColorChanges();
        }

        private void SaveNewTheme(object sender, RoutedEventArgs args)
        {
            NewTheme.ThemeName = NewThemeName.Text;
            bool duplicateCheck = false;
            for (int x = 0; x < ThemeSettingsViewModel.UserThemes.Count; x++)
            {
                if (NewTheme.ThemeName.Equals(ThemeSettingsViewModel.UserThemes[x].ThemeName))
                {
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

            ThemeSettingsViewModel.UserThemes.Insert(0, NewTheme);
            ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[0];
        }

        private void RemoveSavedTheme(object sender, RoutedEventArgs args)
        {
            if (ThemeSettingsViewModel.UserThemes.Count > 1)
            {
                ThemeSelector.SelectedItem = ThemeSettingsViewModel.UserThemes[1];
                Logger.Info($"Removed Theme {ThemeSettingsViewModel.UserThemes[0].ThemeName} From Saved Themes");
                ThemeSettingsViewModel.UserThemes.Remove(ThemeSettingsViewModel.UserThemes[0]);
            }
        }

        private void ChangeMainTheme(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsActive)
            {
                CollectionWindow.CollectionViewModel.CurrentTheme = (ThemeSelector.SelectedItem as TsundokuTheme);
                Logger.Info($"Theme Changed To {(ThemeSelector.SelectedItem as TsundokuTheme).ThemeName}");
                ApplyTheme();
            }
        }

        private void ApplyTheme()
        {
            CollectionWindow.Navigation.Background = new SolidColorBrush(CollectionWindow.CollectionViewModel.CurrentTheme.MenuBGColor);
            Logger.Debug($"Setting Main Theme to {MainWindowViewModel.MainUser.MainTheme.ThemeName}");
        }

        private void MenuColorChanges()
        {
            Menu_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.Navigation.Background = new SolidColorBrush(Menu_BG.Color.ToUint32());
                    NewTheme.MenuBGColor = Menu_BG.Color.ToUint32();
                    Logger.Info($"Changed Color for Menu BG to {Menu_BG.Color}");
                }
            };

            Username.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.Username.Foreground = new SolidColorBrush(Username.Color);
                    Logger.Info($"Changed Color for Username to {Username.Color}");
                }
            };

            Menu_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.NumVolumesCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    CollectionWindow.NumVolumesToBeCollected.Foreground = new SolidColorBrush(Menu_Text.Color);
                    Logger.Info($"Changed Color for Menu Text to {Menu_Text.Color}");
                }
            };

            SearchBar_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SearchBar.Background = new SolidColorBrush(SearchBar_BG.Color);
                    Logger.Info($"Changed Color for SearchBar BG to {SearchBar_BG.Color}");
                }
            };

            SearchBar_Border.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SearchBar.BorderBrush = new SolidColorBrush(SearchBar_Border.Color);
                    Logger.Info($"Changed Color for SearchBar Border to {SearchBar_Border.Color}");
                }
            };

            SearchBar_Text.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SearchBar.Foreground = new SolidColorBrush(SearchBar_Text.Color);
                    Logger.Info($"Changed Color for SearchBar Text to {SearchBar_Text.Color}");
                }
            };

            Seperator.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.Navigation.BorderBrush = new SolidColorBrush(Seperator.Color);
                    Logger.Info($"Changed Color for Seperator to {Seperator.Color}");
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
                    Logger.Info($"Changed Color for Menu Button Text to {menuButtonBG}");
                }
            };

            MenuButton_BG_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
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
                    Logger.Info($"Changed Color for Menu Button Border to {menuButtonBorder}");
                }
            };

            MenuButton_Border_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
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
                    Logger.Info($"Changed Color for Menu Button Text & Icon to {menuButtonTextAndIconColor}");
                }
            };

            MenuButton_IconAndText_Hover.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.SettingsButton.PointerEntered += new EventHandler<Avalonia.Input.PointerEventArgs>(Button_PointerEnter);
                    Logger.Info($"Changed Color for Menu Button Text & Icon (Hover) to {MenuButton_IconAndText_Hover.Color}");
                }
            };
        }

        private void Button_PointerEnter(object sender, PointerEventArgs args)
        {
            (sender as Button).Background = new SolidColorBrush(MenuButton_BG_Hover.Color);
        }

        private void CollectionColorChanges()
        {
            Collection_BG.PropertyChanged += (sender, e) =>
            {
                if (e.Property == Egorozh.ColorPicker.ColorPickerButtonBase.ColorProperty)
                {
                    CollectionWindow.CollectionTheme.Background = new SolidColorBrush(Collection_BG.Color);
                    Logger.Info($"Changed Color for Collection BG to {Collection_BG.Color}");
                }
            };
        }
    }
}