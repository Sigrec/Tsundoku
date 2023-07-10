using System.Collections.ObjectModel;
using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;
using System;
using System.Linq;

namespace Tsundoku.ViewModels
{
    public partial class ThemeSettingsViewModel : ViewModelBase
    {
        public static ObservableCollection<TsundokuTheme> UserThemes { get; set; }
        [Reactive] public string ThemeName { get; set; }
        [Reactive] public bool IsSaveThemeButtonEnabled { get; set; }
        [Reactive] public int CurThemeIndex { get; set; }

        public ThemeSettingsViewModel()
        {
            this.WhenAnyValue(x => x.ThemeName, x => !string.IsNullOrWhiteSpace(x) && !x.Equals("Default", StringComparison.OrdinalIgnoreCase)).Subscribe(x => IsSaveThemeButtonEnabled = x);
            this.WhenAnyValue(x => x.CurrentTheme).Subscribe(x => CurThemeIndex =  UserThemes.IndexOf(x));
        }
    }
}