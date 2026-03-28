using ReactiveUI.Avalonia;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class UserNotesWindow : ReactiveWindow<UserNotesWindowViewModel>
{
    public bool IsOpen = false;

    public UserNotesWindow(UserNotesWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        
        Opened += (s, e) =>
        {
            IsOpen ^= true;
        };

        Closing += (s, e) =>
        {
            if (IsOpen) 
            {
                this.Hide();
                IsOpen ^= true;
            }
            e.Cancel = true;
        };
    }
}