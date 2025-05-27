using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class PopupWindowViewModel : ViewModelBase
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        [Reactive] public string Title { get; set; }
        [Reactive] public string InfoText { get; set; }
        [Reactive] public string Icon { get; set; }

        public PopupWindowViewModel(IUserService userService) : base(userService)
        {

        }

        public void SetPopupInfo(string title, string icon, string infoText)
        {
            Title = title;
            Icon = icon;
            InfoText = infoText;
        }

        public void ResetPopupInfo()
        {
            Title = string.Empty;
            Icon = string.Empty;
            InfoText = string.Empty;
        }
    }
}