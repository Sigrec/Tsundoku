using System;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;

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
            CurCurrency = MainUser.Currency;
            this.WhenAnyValue(x => x.CurCurrency).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.Currency = x);
            this.WhenAnyValue(x => x.UsernameText, x => !string.IsNullOrWhiteSpace(x)).Subscribe(x => IsChangeUsernameButtonEnabled = x);
        }
    }
}
