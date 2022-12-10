using System;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace Tsundoku.ViewModels
{
    public class UserSettingsViewModel : ViewModelBase
    {
        [Reactive]
        public string UsernameText { get; set; }

        [Reactive]
        public bool IsChangeUsernameButtonEnabled { get; set; }

        public UserSettingsViewModel()
        {
            this.WhenAnyValue(x => x.UsernameText, x => !string.IsNullOrWhiteSpace(x)).Subscribe(x => IsChangeUsernameButtonEnabled = x);
        }
    }
}
