using System.Collections.ObjectModel;
using ReactiveUI.SourceGenerators;
using Tsundoku.Models;

namespace Tsundoku.ViewModels;

/// <summary>
/// View model for theme settings, managing theme selection, import, export, and removal.
/// </summary>
public sealed partial class ThemeSettingsViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    [Reactive] public partial bool IsSaveThemeButtonEnabled { get; set; }
    [Reactive] public partial bool IsGenerateThemeButtonEnabled { get; set; } = false;
    [Reactive] public partial uint SelectedThemeIndex { get; set; }

    public ReadOnlyObservableCollection<TsundokuTheme> SavedThemes => _userService.SavedThemes;

    public ThemeSettingsViewModel(IUserService userService) : base(userService)
    {
        UpdateSelectedThemeIndex();
    }

    /// <summary>
    /// Synchronizes the selected theme index with the current theme from the user service.
    /// </summary>
    public void UpdateSelectedThemeIndex()
    {
        uint index = _userService.GetCurrentThemeIndex();
        if (SelectedThemeIndex != index)
        {
            SelectedThemeIndex = _userService.GetCurrentThemeIndex();
            LOGGER.Debug("Updated Selected Theme Index to {val}", SelectedThemeIndex);
        }
    }

    /// <summary>
    /// Gets the user's main (default) theme.
    /// </summary>
    /// <returns>The main theme.</returns>
    public TsundokuTheme GetMainTheme()
    {
        return _userService.GetMainTheme()!;
    }

    /// <summary>
    /// Gets a snapshot of the currently active theme.
    /// </summary>
    /// <returns>The current theme.</returns>
    public TsundokuTheme GetCurrentTheme()
    {
        return _userService.GetCurrentThemeSnapshot()!;
    }

    /// <summary>
    /// Temporarily overrides the current theme for preview without saving the preference.
    /// </summary>
    /// <param name="theme">The theme to preview.</param>
    public void OverrideCurrentTheme(TsundokuTheme theme)
    {
        _userService.OverrideCurrentTheme(theme);
    }

    /// <summary>
    /// Sets the specified theme as the active theme and updates the user's preference.
    /// </summary>
    /// <param name="theme">The theme to activate.</param>
    public void SetTheme(TsundokuTheme theme)
    {
        _userService.SetCurrentTheme(theme);
    }

    /// <summary>
    /// Sets the theme with the specified name as the active theme.
    /// </summary>
    /// <param name="themeName">The name of the theme to activate.</param>
    public void SetTheme(string themeName)
    {
        _userService.SetCurrentTheme(themeName);
    }

    /// <summary>
    /// Removes the theme with the specified name and updates the selected index.
    /// </summary>
    /// <param name="themeName">The name of the theme to remove.</param>
    public void RemoveTheme(string themeName)
    {
        _userService.RemoveTheme(themeName);
        UpdateSelectedThemeIndex();
    }

    /// <summary>
    /// Exports the current theme to a JSON file.
    /// </summary>
    /// <param name="fileName">The base file name for the exported theme.</param>
    public void ExportTheme(string fileName)
    {
        _userService.ExportTheme(fileName);
    }

    /// <summary>
    /// Imports a theme from a JSON file, saves user data, and updates the selected index.
    /// </summary>
    /// <param name="filePath">The path to the theme JSON file.</param>
    public async Task ImportThemeAsync(string filePath)
    {
        await _userService.ImportThemeAsync(filePath);
        _userService.SaveUserData();
        UpdateSelectedThemeIndex();
    }

    /// <summary>
    /// Saves a theme to the user's collection and persists user data.
    /// </summary>
    /// <param name="theme">The theme to save.</param>
    public void SaveTheme(TsundokuTheme theme)
    {
        _userService.AddTheme(theme);
        UpdateSelectedThemeIndex();
        _userService.SaveUserData();
    }
}