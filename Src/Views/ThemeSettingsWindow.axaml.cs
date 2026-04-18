using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Tsundoku.Clients;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.Services;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class CollectionThemeWindow : ReactiveWindow<ThemeSettingsViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private TsundokuTheme? BackupCurrentTheme { get; set; }
    public bool IsOpen = false;
    private bool _isInitialized = false;
    private string CurThemeName;
    private readonly IPopupDialogService _popupDialogService;
    private readonly IThemeResourceService _themeResourceService;
    private readonly ColorApi _colorApi;
    private IDisposable? _themeObserveDisposable;

    public CollectionThemeWindow(ThemeSettingsViewModel viewModel, IPopupDialogService popupDialogService, ColorApi colorApi)
    {
        ViewModel = viewModel;
        _popupDialogService = popupDialogService;
        _colorApi = colorApi;
        _themeResourceService = App.ServiceProvider!.GetRequiredService<IThemeResourceService>();
        InitializeComponent();

        Opened += (s, e) =>
        {
            IsOpen = true;
            Dispatcher.UIThread.Post(() =>
            {
                TsundokuTheme curTheme = ViewModel.GetCurrentTheme();
                BackupCurrentTheme = curTheme.Cloning();
                CurThemeName = curTheme.ThemeName;
                BackupCurrentTheme.ThemeName = "Backup";
                ViewModel.OverrideCurrentTheme(BackupCurrentTheme);
                _themeResourceService.ApplyTheme(BackupCurrentTheme);
                _themeObserveDisposable = _themeResourceService.ObserveAndApply(BackupCurrentTheme);
            }, DispatcherPriority.Background);
        };

        Closing += (s, e) =>
        {
            if (IsOpen)
            {
                _themeObserveDisposable?.Dispose();
                _themeObserveDisposable = null;

                this.Hide();
                NewThemeName.Text = string.Empty;
                Topmost = false;
                IsOpen = false;

                TsundokuTheme? curSelectedTheme = ViewModel.GetMainTheme();
                if (!BackupCurrentTheme.Equals(curSelectedTheme!))
                {
                    ViewModel.SetTheme(curSelectedTheme);
                }
                _themeResourceService.ApplyTheme(curSelectedTheme!);
                BackupCurrentTheme = null;
                e.Cancel = true;
            }
        };

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(x => x.MainColor1.Text, x => x.MainColor2.Text, x => x.TextColor1.Text, x => x.TextColor2.Text, x => x.AccentColor1.Text, x => x.AccentColor2.Text, (mc1, mc2, tc1, tc2, ac1, ac2) => _isInitialized && !mc1.Contains('_') && !mc2.Contains('_') && !tc1.Contains('_') && !tc2.Contains('_') && !ac1.Contains('_') && !ac2.Contains('_'))
                .Subscribe(x => ViewModel.IsGenerateThemeButtonEnabled = x)
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.NewThemeName.Text, (newName) => _isInitialized && !string.IsNullOrWhiteSpace(newName) && !newName.Equals("Default", StringComparison.OrdinalIgnoreCase))
                .DistinctUntilChanged()
                .Subscribe(x => ViewModel.IsSaveThemeButtonEnabled = x)
                .DisposeWith(disposables);
        });

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
    /// Applies shared color assignments common to both generation styles.
    /// </summary>
    private void ApplySharedThemeColors()
    {
        SolidColorBrush mc1 = new(Color.Parse(MainColor1.Text));
        SolidColorBrush mc2 = new(Color.Parse(MainColor2.Text));
        SolidColorBrush tc1 = new(Color.Parse(TextColor1.Text));
        SolidColorBrush tc2 = new(Color.Parse(TextColor2.Text));
        SolidColorBrush ac1 = new(Color.Parse(AccentColor1.Text));
        SolidColorBrush ac2 = new(Color.Parse(AccentColor2.Text));

        // MainColor1 roles
        BackupCurrentTheme.MenuBGColor = mc1;
        BackupCurrentTheme.SeriesCardBGColor = mc1;
        BackupCurrentTheme.SeriesProgressBarBGColor = mc1;

        // MainColor2 roles
        BackupCurrentTheme.CollectionBGColor = mc2;
        BackupCurrentTheme.MenuButtonBGHoverColor = mc2;
        BackupCurrentTheme.SeriesCardButtonBGHoverColor = mc2;

        // TextColor1 roles
        BackupCurrentTheme.UsernameColor = tc1;
        BackupCurrentTheme.MenuTextColor = tc1;
        BackupCurrentTheme.SearchBarTextColor = tc1;
        BackupCurrentTheme.MenuButtonTextAndIconColor = tc1;
        BackupCurrentTheme.SeriesCardPublisherColor = tc1;
        BackupCurrentTheme.SeriesCardStaffColor = tc1;

        // TextColor2 roles
        BackupCurrentTheme.SeriesButtonIconColor = tc2;
        BackupCurrentTheme.SeriesCardDescColor = tc2;
        BackupCurrentTheme.SeriesProgressBarBorderColor = tc2;
        BackupCurrentTheme.StatusAndBookTypeTextColor = tc2;

        // AccentColor1 roles
        BackupCurrentTheme.DividerColor = ac1;
        BackupCurrentTheme.SearchBarBorderColor = ac1;
        BackupCurrentTheme.MenuButtonBorderColor = ac1;
        BackupCurrentTheme.MenuButtonBorderHoverColor = ac1;
        BackupCurrentTheme.SeriesCardTitleColor = ac1;
        BackupCurrentTheme.SeriesCardBorderColor = ac1;
        BackupCurrentTheme.StatusAndBookTypeBorderColor = ac1;
        BackupCurrentTheme.SeriesCardButtonBorderColor = ac1;
        BackupCurrentTheme.SeriesCardButtonBorderHoverColor = ac1;
        BackupCurrentTheme.SeriesCardDividerColor = ac1;

        // AccentColor2 roles
        BackupCurrentTheme.SearchBarBGColor = ac2;
        BackupCurrentTheme.MenuButtonBGColor = ac2;
        BackupCurrentTheme.StatusAndBookTypeBGColor = ac2;
        BackupCurrentTheme.SeriesCoverBGColor = ac2;
        BackupCurrentTheme.SeriesCardButtonBGColor = ac2;
    }

    /// <summary>
    /// Generate Type 1 (Classy style): MainColor1 used for icon hover, TextColor1 for progress bar.
    /// </summary>
    private void GenerateThemeType1(object sender, RoutedEventArgs args)
    {
        LOGGER.Info("Generating Theme Type 1");
        _themeObserveDisposable?.Dispose();

        ApplySharedThemeColors();
        BackupCurrentTheme.MenuButtonTextAndIconHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
        BackupCurrentTheme.SeriesButtonIconHoverColor = new SolidColorBrush(Color.Parse(MainColor1.Text));
        BackupCurrentTheme.SeriesProgressBarColor = new SolidColorBrush(Color.Parse(TextColor1.Text));
        BackupCurrentTheme.SeriesProgressTextColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));

        _themeResourceService.ApplyTheme(BackupCurrentTheme);
        _themeObserveDisposable = _themeResourceService.ObserveAndApply(BackupCurrentTheme);
        SaveGeneratedThemeIfNamed();
    }

    private void GenerateThemeType2(object sender, RoutedEventArgs args)
    {
        LOGGER.Info("Generating Theme Type 2");
        _themeObserveDisposable?.Dispose();

        ApplySharedThemeColors();
        BackupCurrentTheme.MenuButtonTextAndIconHoverColor = new SolidColorBrush(Color.Parse(AccentColor2.Text));
        BackupCurrentTheme.SeriesButtonIconHoverColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
        BackupCurrentTheme.SeriesProgressBarColor = new SolidColorBrush(Color.Parse(AccentColor1.Text));
        BackupCurrentTheme.SeriesProgressTextColor = new SolidColorBrush(Color.Parse(TextColor2.Text));

        _themeResourceService.ApplyTheme(BackupCurrentTheme);
        _themeObserveDisposable = _themeResourceService.ObserveAndApply(BackupCurrentTheme);
        SaveGeneratedThemeIfNamed();
    }

    private void SaveGeneratedThemeIfNamed()
    {
        string name = NewThemeName.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(name) && !name.Equals("Default", StringComparison.OrdinalIgnoreCase))
        {
            BackupCurrentTheme.ThemeName = name;
            TsundokuTheme themeToSave = BackupCurrentTheme.Cloning();
            themeToSave.ThemeName = name;
            ViewModel.SaveTheme(themeToSave);
        }
    }

    private async void GenerateFromColorApiAsync(object sender, RoutedEventArgs args)
    {
        string seedHex = MainColor1.Text;
        if (string.IsNullOrWhiteSpace(seedHex) || seedHex.Contains('_'))
        {
            await ShowDialog("Error", "Enter a valid hex color in Main Color 1 to generate from.");
            return;
        }

        GenerateApiButton.IsEnabled = false;
        GenerateApiButton.Content = "Generating...";
        try
        {
            GeneratedColor[] colors = await _colorApi.GenerateSchemeAsync(
                seedHex.TrimStart('#'),
                ColorSchemeMode.AnalogicComplement,
                6);

            if (colors.Length < 6)
            {
                await ShowDialog("Error", "Color API did not return enough colors. Try a different seed color.");
                return;
            }

            // Map generated colors to the 5 remaining input fields (keep MainColor1 as the user's seed)
            MainColor2.Text = colors[1].Hex;
            TextColor1.Text = colors[2].Hex;
            TextColor2.Text = colors[3].Hex;
            AccentColor1.Text = colors[4].Hex;
            AccentColor2.Text = colors[5].Hex;

            LOGGER.Info("Generated palette from Color API: {Colors}", string.Join(", ", colors.Select(c => c.Hex)));
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to generate colors from API");
            await ShowDialog("Error", "Failed to generate colors from the Color API.");
        }
        finally
        {
            GenerateApiButton.Content = "Auto Generate";
            GenerateApiButton.IsEnabled = true;
        }
    }

    private async Task ShowDialog(string title, string info = "Unable to Add Theme")
    {
        await _popupDialogService.ShowAsync(title, "fa7-solid fa7-circle-exclamation", info, this);
    }
    private async void SaveNewTheme(object sender, RoutedEventArgs args)
    {
        try
        {
            string themeName = NewThemeName.Text.Trim();
            bool themeExists = ViewModel.SavedThemes.Any(t => t.ThemeName.Equals(themeName, StringComparison.OrdinalIgnoreCase));

            if (themeExists)
            {
                bool confirmed = await _popupDialogService.ConfirmAsync(
                    "Overwrite Theme",
                    "fa7-solid fa7-triangle-exclamation",
                    $"Theme \"{themeName}\" already exists.\nDo you want to overwrite it?",
                    this);

                if (!confirmed) return;
            }

            TsundokuTheme themeToSave = BackupCurrentTheme.Cloning();
            themeToSave.ThemeName = themeName;
            ViewModel.SaveTheme(themeToSave);
        }
        catch (Exception ex)
        {
            await ShowDialog("Error", $"Unable to Save New Theme \"{NewThemeName.Text}\"");
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

        if (file.Count > 0 && file[0] is not null)
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
        NewThemeName.Text = string.Empty;
    }

    private async void RemoveSavedTheme(object sender, RoutedEventArgs args)
    {
        if (ThemeSelector.SelectedItem is TsundokuTheme selectedTheme)
        {
            bool confirmed = await _popupDialogService.ConfirmAsync(
                "Delete Theme",
                "fa7-solid fa7-triangle-exclamation",
                $"Are you sure you want to delete the \"{selectedTheme.ThemeName}\" theme?",
                this);

            if (!confirmed) return;

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

                _themeObserveDisposable?.Dispose();
                _themeResourceService.ApplyTheme(BackupCurrentTheme);
                _themeObserveDisposable = _themeResourceService.ObserveAndApply(BackupCurrentTheme);
            }
        }
    }
}