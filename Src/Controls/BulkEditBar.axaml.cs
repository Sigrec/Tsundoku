using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.SeriesDemographicModel;

namespace Tsundoku.Controls;

/// <summary>
/// The bar shown along the bottom of the main window while series cards are selected.
/// Simple edits run on <see cref="BulkEditViewModel"/> directly; refresh and trash are raised to
/// the window because they need a loading or confirmation dialog.
/// </summary>
public sealed partial class BulkEditBar : UserControl
{
    /// <summary>Raised with the text to show in the window's notification popup.</summary>
    public event EventHandler<string>? NotificationRequested;

    /// <summary>Raised when the user asks to refresh the selected series.</summary>
    public event EventHandler? RefreshRequested;

    /// <summary>Raised when the user asks to move the selected series to the trash.</summary>
    public event EventHandler? TrashRequested;

    public BulkEditBar() => InitializeComponent();

    private BulkEditViewModel? ViewModel => DataContext as BulkEditViewModel;

    private void Notify(string? message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            NotificationRequested?.Invoke(this, message);
        }
    }

    /// <summary>Lets the window turn refresh off while AniList is unavailable.</summary>
    public void SetRefreshEnabled(bool isEnabled) => RefreshButton.IsEnabled = isEnabled;

    private void OpenDemographicMenu(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not BulkEditViewModel vm) return;

        MenuFlyout menu = new() { Placement = PlacementMode.Top };
        foreach (SeriesDemographic demographic in vm.Demographics)
        {
            MenuItem item = new() { Header = demographic.ToString() };
            item.Click += (_, _) => Notify(vm.SetDemographic(demographic));
            menu.Items.Add(item);
        }
        menu.ShowAt(DemographicButton);
    }

    private void FocusPublisherBox(object? sender, EventArgs e) => PublisherBox.Focus();

    private void FocusRatingBox(object? sender, EventArgs e) => RatingBox.Focus();

    private void ApplyPublisher(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not BulkEditViewModel vm) return;
        string message = vm.SetPublisher(PublisherBox.Text ?? string.Empty);
        PublisherBox.Text = string.Empty;
        CloseFlyoutOf(PublisherBox);
        Notify(message);
    }

    private void PublisherBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            ApplyPublisher(sender, e);
        }
    }

    private void ApplyRating(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not BulkEditViewModel vm) return;
        string message = vm.SetRating(RatingBox.Text ?? string.Empty);
        RatingBox.Text = string.Empty;
        CloseFlyoutOf(RatingBox);
        Notify(message);
    }

    private void RatingBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            ApplyRating(sender, e);
        }
    }

    private void AddGenre(object? sender, RoutedEventArgs e) => ApplyGenre(add: true);

    private void RemoveGenre(object? sender, RoutedEventArgs e) => ApplyGenre(add: false);

    private void ApplyGenre(bool add)
    {
        if (ViewModel is not BulkEditViewModel vm) return;
        if (GenreComboBox.SelectedItem is not string genre)
        {
            Notify("Pick a genre first");
            return;
        }
        Notify(add ? vm.AddGenre(genre) : vm.RemoveGenre(genre));
    }

    private void AddFavorite(object? sender, RoutedEventArgs e) => Notify(ViewModel?.SetFavorite(true));

    private void RemoveFavorite(object? sender, RoutedEventArgs e) => Notify(ViewModel?.SetFavorite(false));

    private void MarkComplete(object? sender, RoutedEventArgs e) => Notify(ViewModel?.MarkComplete());

    private void MarkAllRead(object? sender, RoutedEventArgs e) => Notify(ViewModel?.MarkAllRead());

    private void Undo(object? sender, RoutedEventArgs e) => Notify(ViewModel?.Undo());

    private void ClearSelection(object? sender, RoutedEventArgs e) => ViewModel?.Clear();

    private void RequestRefresh(object? sender, RoutedEventArgs e) => RefreshRequested?.Invoke(this, EventArgs.Empty);

    private void RequestTrash(object? sender, RoutedEventArgs e) => TrashRequested?.Invoke(this, EventArgs.Empty);

    private static void CloseFlyoutOf(Control content)
    {
        if (content.FindLogicalAncestorOfType<Popup>() is Popup popup)
        {
            popup.IsOpen = false;
        }
    }
}
