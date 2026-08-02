using Avalonia.Interactivity;
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
        this.ConfigureHideOnClose(onOpened: RefreshPreview);
    }

    private void NotesEditToggled(object? sender, RoutedEventArgs e)
    {
        if (NotesEditToggle.IsChecked != true)
        {
            RefreshPreview();
        }
    }

    private void RefreshPreview()
    {
        if (ViewModel is null) return;
        MarkdownRenderer.ApplyTo(NotesPreview, ViewModel.Notes);
    }
}
