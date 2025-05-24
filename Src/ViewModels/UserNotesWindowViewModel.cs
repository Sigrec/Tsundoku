using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels
{
    public class UserNotesWindowViewModel : ViewModelBase
    {
        [Reactive] public string Notes { get; set; }

        public UserNotesWindowViewModel()
        {
            Notes = MainUser.Notes;   
            this.WhenAnyValue(x => x.Notes).Throttle(TimeSpan.FromMilliseconds(1000)).Subscribe(x => MainUser.Notes = x);
        }
    }
}
