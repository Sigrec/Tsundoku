using System.Diagnostics;
using System.Collections.ObjectModel;
using ReactiveUI;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class ThemeSettingsViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static ObservableCollection<TsundokuTheme> UserThemes { get; set; }

        public TsundokuTheme currentTheme = MainWindowViewModel.MainUser.MainTheme;
        public TsundokuTheme CurrentTheme
        {
            get => currentTheme;
            set => this.RaiseAndSetIfChanged(ref currentTheme, value);
        }
        public ThemeSettingsViewModel()
        {
            
        }
    }
}