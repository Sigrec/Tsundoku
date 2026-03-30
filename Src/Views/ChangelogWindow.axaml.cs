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
    }

    public void SetVersion(string version)
    {
        VersionHeader.Text = $"What's New in v{version}";
        if (Changelog.Entries.TryGetValue(version, out ChangelogEntry? entry))
        {
            ChangesItems.ItemsSource = entry.Changes;

            if (entry.Actions.Length > 0)
            {
                ActionsItems.ItemsSource = entry.Actions;
                ActionsSection.IsVisible = true;
            }
        }
    }

    private void OnDismissClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
