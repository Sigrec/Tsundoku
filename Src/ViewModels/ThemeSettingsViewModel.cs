using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class ThemeSettingsViewModel : ViewModelBase
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        [Reactive] public bool IsSaveThemeButtonEnabled { get; set; }
        [Reactive] public bool IsGenerateThemeButtonEnabled { get; set; } = false;
        [Reactive] public int SelectedThemeIndex { get; set; }

        public ReadOnlyObservableCollection<TsundokuTheme> SavedThemes => _userService.SavedThemes;

        public ThemeSettingsViewModel(IUserService userService) : base(userService)
        {
            UpdateSelectedThemeIndex();
        }

        public void UpdateSelectedThemeIndex()
        {
            int initialIndex = SavedThemes.ToList().FindIndex(t => t.ThemeName == CurrentTheme.ThemeName);
            LOGGER.Debug("Updating Selected Theme Index to {val}", initialIndex);
            SelectedThemeIndex = initialIndex != -1 ? initialIndex : 0;
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
            UpdateSelectedThemeIndex();
            _userService.SaveUserData();
        }

        public void SaveTheme(TsundokuTheme theme)
        {
            _userService.AddTheme(theme);
            UpdateSelectedThemeIndex();
            _userService.SaveUserData();
        }
    }
}