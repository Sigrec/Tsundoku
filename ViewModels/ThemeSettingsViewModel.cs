using System.Diagnostics;
using System.Collections.ObjectModel;
using ReactiveUI;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class ThemeSettingsViewModel : ViewModelBase
    {
        public static ObservableCollection<TsundokuTheme> UserThemes { get; set; }

        public ThemeSettingsViewModel()
        {
            
        }
    }
}