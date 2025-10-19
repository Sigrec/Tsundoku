using Tsundoku.ViewModels;
using ReactiveUI.Avalonia;

namespace Tsundoku.Views;

public sealed partial class CollectionStatsWindow : ReactiveWindow<CollectionStatsViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public bool IsOpen = false;
    public bool CanUpdate = true; // On First Update

    public CollectionStatsWindow(CollectionStatsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        Opened += (s, e) =>
        {
            CanUpdate = false;
            IsOpen ^= true;

            if (Screens.Primary.WorkingArea.Height < 1250)
            {
                this.Height = 550;
            }
        };

        Closing += (s, e) =>
        {
            if (IsOpen)
            {
                this.Hide();
                IsOpen ^= true;
                Topmost = false;
            }
            e.Cancel = true;
        };
    }

    private async void CopyTextAsync(object sender, PointerPressedEventArgs args)
    {
        string curText = $"{(sender as Controls.ValueStat).Text} {(sender as Controls.ValueStat).Title}";
        LOGGER.Info($"Copying {curText} to Clipboard");
        await TextCopy.ClipboardService.SetTextAsync(curText);
    }
}
