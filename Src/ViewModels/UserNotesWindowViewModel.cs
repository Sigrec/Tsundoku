using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Tsundoku.ViewModels;

public sealed partial class UserNotesWindowViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    [Reactive] public partial string Notes { get; set; }
    [Reactive] public partial double NotesFontSize { get; set; } = 16;

    public UserNotesWindowViewModel(IUserService userService) : base(userService)
    {
        // Sync from user data once CurrentUser is available
        this.WhenAnyValue(x => x.CurrentUser)
            .Where(user => user is not null)
            .Take(1)
            .Subscribe(user =>
            {
                Notes = user.Notes ?? string.Empty;
                NotesFontSize = user.NotesFontSize > 0 ? user.NotesFontSize : 16;
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.Notes)
            .DistinctUntilChanged()
            .Throttle(TimeSpan.FromMilliseconds(1000))
            .Where(_ => CurrentUser is not null)
            .Subscribe(notes =>
            {
                _userService.UpdateUser(user => user.Notes = notes);
                LOGGER.Trace("User notes updated to '{Notes}'", notes);
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.NotesFontSize)
            .DistinctUntilChanged()
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Where(_ => CurrentUser is not null)
            .Subscribe(fontSize =>
            {
                _userService.UpdateUser(user => user.NotesFontSize = fontSize);
            })
            .DisposeWith(_disposables);
    }
}
