using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class UserNotesWindowViewModel : ViewModelBase
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        [Reactive] public string Notes { get; set; }

        public UserNotesWindowViewModel(IUserService userService) : base(userService)
        {
            this.WhenAnyValue(x => x.CurrentUser)
                .DistinctUntilChanged()
                .Where(user => user != null)
                .Subscribe(user =>
                {
                    Notes = user!.Notes;
                    LOGGER.Info($"Notes initialized from user: '{Notes}'");
                });

            this.WhenAnyValue(x => x.Notes)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(1000))
                .Where(_ => CurrentUser != null)
                .Subscribe(notes =>
                {
                    _userService.UpdateUser(user => user.Notes = notes);
                    LOGGER.Debug($"User notes updated to service: '{notes}'");
                });
        }
    }
}
