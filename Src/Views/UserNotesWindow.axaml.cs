using Avalonia.ReactiveUI;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public partial class UserNotesWindow : ReactiveWindow<UserNotesWindowViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
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