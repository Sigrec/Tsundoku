using System.Collections.ObjectModel;
using ReactiveUI.SourceGenerators;
using Tsundoku.Models;

namespace Tsundoku.ViewModels;

public sealed class ThemeSettingsViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    [Reactive] public bool IsSaveThemeButtonEnabled { get; set; }
    [Reactive] public bool IsGenerateThemeButtonEnabled { get; set; } = false;
    [Reactive] public uint SelectedThemeIndex { get; set; }

    public ReadOnlyObservableCollection<TsundokuTheme> SavedThemes => _userService.SavedThemes;

    public ThemeSettingsViewModel(IUserService userService) : base(userService)
    {
        UpdateSelectedThemeIndex();
    }

    public void UpdateSelectedThemeIndex()
    {
        uint index = _userService.GetCurrentThemeIndex();
        if (SelectedThemeIndex != index)
        {
            SelectedThemeIndex = _userService.GetCurrentThemeIndex();
            LOGGER.Debug("Updated Selected Theme Index to {val}", SelectedThemeIndex);
        }
    }

    public TsundokuTheme GetMainTheme()
    {
        return _userService.GetMainTheme()!;
    }

    public TsundokuTheme GetCurrentTheme()
    {
        return _userService.GetCurrentThemeSnapshot()!;
    }

    public void OverrideCurrentTheme(TsundokuTheme theme)
    {
        _userService.OverrideCurrentTheme(theme);
    }

    public void SetTheme(TsundokuTheme theme)
    {
        _userService.SetCurrentTheme(theme);
    }

    public void SetTheme(string themeName)
    {
        _userService.SetCurrentTheme(themeName);
    }

    public void RemoveTheme(string themeName)
    {
        _userService.RemoveTheme(themeName);
        UpdateSelectedThemeIndex();
    }

    public void ExportTheme(string fileName)
    {
        _userService.ExportTheme(fileName);
    }

    public async Task ImportThemeAsync(string filePath)
    {
        await _userService.ImportThemeAsync(filePath);
        _userService.SaveUserData();
        UpdateSelectedThemeIndex();
    }

    public void SaveTheme(TsundokuTheme theme)
    {
        _userService.AddTheme(theme);
        UpdateSelectedThemeIndex();
        _userService.SaveUserData();
    }
}