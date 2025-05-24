using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public partial class UserNotesWindow : ReactiveWindow<UserNotesWindowViewModel>
{
    public bool IsOpen = false;
    private MainWindow CollectionWindow;

    public UserNotesWindow()
    {
        InitializeComponent();
        
        Opened += (s, e) =>
        {
            CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
            IsOpen ^= true;
        };

        Closing += (s, e) =>
        {
            if (IsOpen) 
            { 
                MainWindow.ResetMenuButton(CollectionWindow.UserNotesButton);
                ((UserNotesWindow)s).Hide();
                IsOpen ^= true;
            }
            e.Cancel = true;
        };
    }
}