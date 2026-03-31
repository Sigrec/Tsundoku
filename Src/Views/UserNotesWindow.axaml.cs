using ReactiveUI.Avalonia;
using Tsundoku.Helpers;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class UserNotesWindow : ReactiveWindow<UserNotesWindowViewModel>, IManagedWindow
{
    public bool IsOpen { get; set; }

    public UserNotesWindow(UserNotesWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        this.ConfigureHideOnClose();
    }
}