using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class ChangelogWindow : ReactiveWindow<ViewModelBase>
{
    public ChangelogWindow()
    {
        InitializeComponent();

        ChangesScroll.GetObservable(ScrollViewer.ExtentProperty).Subscribe(_ => UpdateScrollHint());
        ChangesScroll.GetObservable(ScrollViewer.ViewportProperty).Subscribe(_ => UpdateScrollHint());
        ChangesScroll.GetObservable(ScrollViewer.OffsetProperty).Subscribe(_ => UpdateScrollHint());
    }

    public void SetVersion(string version)
    {
        VersionHeader.Text = $"What's New in v{version}";
        if (Changelog.Entries.TryGetValue(version, out ChangelogEntry? entry))
        {
            ChangesItems.ItemsSource = entry.Changes;

            if (entry.Actions is { Length: > 0 })
            {
                ActionsItems.ItemsSource = entry.Actions;
                ActionsSection.IsVisible = true;
            }
        }
    }

    private void UpdateScrollHint()
    {
        bool canScroll = ChangesScroll.Extent.Height > ChangesScroll.Viewport.Height;
        bool atBottom = ChangesScroll.Offset.Y >= ChangesScroll.Extent.Height - ChangesScroll.Viewport.Height - 1;
        ScrollHint.IsVisible = canScroll && !atBottom;
    }

    private void OnDismissClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
