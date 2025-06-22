using ReactiveUI.Fody.Helpers;

namespace Tsundoku.ViewModels;

public sealed class PopupDialogViewModel(IUserService userService) : ViewModelBase(userService)
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    [Reactive] public string Title { get; set; }
    [Reactive] public string InfoText { get; set; }
    [Reactive] public string Icon { get; set; }

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