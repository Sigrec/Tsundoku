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
                IsOpen = false;
                Topmost = false;
                e.Cancel = true;
            }
        };
    }

    private async void CopyTextAsync(object sender, PointerPressedEventArgs args)
    {
        try
        {
            if (sender is not Controls.ValueStat valueStat)
            {
                return;
            }
            string curText = $"{valueStat.Text} {valueStat.Title}";
            LOGGER.Info("Copying {Text} to Clipboard", curText);
            await TextCopy.ClipboardService.SetTextAsync(curText);
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to copy text to clipboard");
        }
    }
}
