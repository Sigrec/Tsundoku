using Avalonia;
using Avalonia.Input;
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
        NotesPreviewScroller.AddHandler(
            InputElement.PointerWheelChangedEvent,
            (_, e) =>
            {
                double delta = e.Delta.Y * 40;
                Vector current = NotesPreviewScroller.Offset;
                double maxY = Math.Max(0, NotesPreviewScroller.Extent.Height - NotesPreviewScroller.Viewport.Height);
                double newY = Math.Clamp(current.Y - delta, 0, maxY);
                if (Math.Abs(newY - current.Y) < 0.01) return;
                NotesPreviewScroller.Offset = new Vector(current.X, newY);
                e.Handled = true;
            },
            RoutingStrategies.Bubble,
            handledEventsToo: true);
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
        MarkdownRenderer.ApplyTo(NotesPreview, ViewModel.Notes, OpenLink);
        ForcePreviewLayoutRefresh();
    }

    private async void OpenLink(string url) => await Tsundoku.ViewModels.ViewModelBase.OpenSiteLink(url);

    private void ForcePreviewLayoutRefresh()
    {
        NotesPreview.InvalidateMeasure();
        NotesPreviewScroller.InvalidateMeasure();
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            NotesPreview.InvalidateMeasure();
            NotesPreviewScroller.InvalidateMeasure();
        }, Avalonia.Threading.DispatcherPriority.ContextIdle);
    }
}
