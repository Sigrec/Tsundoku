using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels;

public sealed class UserNotesWindowViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    [Reactive] public string Notes { get; set; }

    public UserNotesWindowViewModel(IUserService userService) : base(userService)
    {
        Notes = CurrentUser.Notes;
        this.WhenAnyValue(x => x.Notes)
            .DistinctUntilChanged()
            .Throttle(TimeSpan.FromMilliseconds(1000))
            .Where(_ => CurrentUser != null)
            .Subscribe(notes =>
            {
                _userService.UpdateUser(user => user.Notes = notes);
                LOGGER.Trace($"User notes updated to '{notes}'");
            });
    }
}
