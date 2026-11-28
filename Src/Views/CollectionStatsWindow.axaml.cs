using Tsundoku.Helpers;
using Tsundoku.ViewModels;
using ReactiveUI.Avalonia;

namespace Tsundoku.Views;

public sealed partial class CollectionStatsWindow : ReactiveWindow<CollectionStatsViewModel>, IManagedWindow
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public bool IsOpen { get; set; }
    public bool CanUpdate = true; // On First Update

    public CollectionStatsWindow(CollectionStatsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        this.ConfigureHideOnClose(
            onOpened: () => CanUpdate = false);
    }

    private async void CopyTextAsync(object sender, PointerPressedEventArgs args)
    {
        if (sender is Controls.ValueStat valueStat)
        {
            await ClipboardHelper.CopyToClipboardAsync($"{valueStat.Text} {valueStat.Title}");
        }
    }
}
