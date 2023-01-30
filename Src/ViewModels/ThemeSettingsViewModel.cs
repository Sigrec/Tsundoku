using System.Collections.ObjectModel;
using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels
{
    public class ThemeSettingsViewModel : ViewModelBase
    {
        public static ObservableCollection<TsundokuTheme> UserThemes { get; set; }

        [Reactive]
        public string ThemeName { get; set; }

        [Reactive]
        public bool IsSaveThemeButtonEnabled { get; set; } = true;

        public ThemeSettingsViewModel()
        {
            //this.WhenAnyValue(x => x.ThemeName, x => !string.IsNullOrWhiteSpace(x) && !x.Equals("Default")).Subscribe(x => IsSaveThemeButtonEnabled = x);
        }
    }
}