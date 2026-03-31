using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class ChangelogWindow : ReactiveWindow<ViewModelBase>
{
    private string[] _sortedVersions = [];
    private int _currentIndex;

    public ChangelogWindow()
    {
        InitializeComponent();

        ChangesScroll.GetObservable(ScrollViewer.ExtentProperty).Subscribe(_ => UpdateScrollHint());
        ChangesScroll.GetObservable(ScrollViewer.ViewportProperty).Subscribe(_ => UpdateScrollHint());
        ChangesScroll.GetObservable(ScrollViewer.OffsetProperty).Subscribe(_ => UpdateScrollHint());

        _sortedVersions = Changelog.Entries.Keys
            .OrderBy(v => Version.TryParse(v, out Version? parsed) ? parsed : new Version(0, 0))
            .ToArray();
    }

    public void SetVersion(string version)
    {
        _currentIndex = Array.IndexOf(_sortedVersions, version);
        if (_currentIndex < 0)
        {
            _currentIndex = _sortedVersions.Length - 1;
        }
        ShowVersion(_sortedVersions[_currentIndex]);
    }

    private void ShowVersion(string version)
    {
        VersionHeader.Text = $"What's New in v{version}";
        ChangesScroll.Offset = default;

        if (Changelog.Entries.TryGetValue(version, out ChangelogEntry? entry))
        {
            ChangesItems.ItemsSource = entry.Changes;

            if (entry.Actions is { Length: > 0 })
            {
                ActionsItems.ItemsSource = entry.Actions;
                ActionsSection.IsVisible = true;
            }
            else
            {
                ActionsItems.ItemsSource = null;
                ActionsSection.IsVisible = false;
            }
        }
        else
        {
            ChangesItems.ItemsSource = null;
            ActionsItems.ItemsSource = null;
            ActionsSection.IsVisible = false;
        }

        bool hasPrev = _currentIndex > 0;
        PrevVersionButton.Opacity = hasPrev ? 1 : 0;
        PrevVersionButton.IsHitTestVisible = hasPrev;
        ToolTip.SetTip(PrevVersionButton, hasPrev ? $"v{_sortedVersions[_currentIndex - 1]}" : null);

        bool hasNext = _currentIndex < _sortedVersions.Length - 1;
        NextVersionButton.Opacity = hasNext ? 1 : 0;
        NextVersionButton.IsHitTestVisible = hasNext;
        ToolTip.SetTip(NextVersionButton, hasNext ? $"v{_sortedVersions[_currentIndex + 1]}" : null);
    }

    private void OnPrevVersionClicked(object? sender, RoutedEventArgs e)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            ShowVersion(_sortedVersions[_currentIndex]);
        }
    }

    private void OnNextVersionClicked(object? sender, RoutedEventArgs e)
    {
        if (_currentIndex < _sortedVersions.Length - 1)
        {
            _currentIndex++;
            ShowVersion(_sortedVersions[_currentIndex]);
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
