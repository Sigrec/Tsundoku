using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;
using System.Collections.ObjectModel;

namespace Tsundoku.ViewModels
{
    public class ThemeSettingsViewModel : ViewModelBase
    {
        public static ObservableCollection<string> UserThemesDisplay { get; set; }
        [Reactive] public string ThemeName { get; set; }
        [Reactive] public bool IsSaveThemeButtonEnabled { get; set; }
        [Reactive] public bool IsGenerateThemeButtonEnabled { get; set; } = false;
        [Reactive] public int CurThemeIndex { get; set; }

        public ThemeSettingsViewModel()
        {
            this.WhenAnyValue(x => x.ThemeName, x => !string.IsNullOrWhiteSpace(x) && !x.Equals("Default", StringComparison.OrdinalIgnoreCase)).Subscribe(x => IsSaveThemeButtonEnabled = x);
            this.WhenAnyValue(x => x.CurrentTheme).Subscribe(x => CurThemeIndex = MainUser.SavedThemes.IndexOf(x));
            UserThemesDisplay = new ObservableCollection<string>(MainUser.SavedThemes.OrderBy(theme => theme.ThemeName).Select(theme => theme.ThemeName));
        }
    }
}