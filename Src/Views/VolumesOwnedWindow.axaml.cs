using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Tsundoku.Controls;
using Tsundoku.Models;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Views;

/// <summary>
/// Dialog for marking which volumes and specials of one series are owned. Changes apply to the
/// series as they are made, so closing the window is all that's needed to finish.
/// </summary>
/// <remarks>
/// Opens tall enough to show every row without scrolling when the screen has room, keeping a margin
/// from the top and bottom of the work area. It can be resized in height only, down to the size where
/// the rows area shows <see cref="VolumeGridEditor.MinimumRowsHeight"/>.
/// </remarks>
public sealed partial class VolumesOwnedWindow : Window
{
    private const double ScreenMargin = 24;

    public VolumesOwnedWindow()
    {
        InitializeComponent();
        Opened += (_, _) => Dispatcher.UIThread.Post(FitToScreen, DispatcherPriority.Loaded);
        VolumeGrid.RowCountChanged += (_, _) => Refit();
    }

    /// <summary>Lets the window size to its content again, then re-applies the screen limits.</summary>
    private void Refit()
    {
        MinHeight = 0;
        MaxHeight = double.PositiveInfinity;
        SizeToContent = SizeToContent.Height;
        Dispatcher.UIThread.Post(FitToScreen, DispatcherPriority.Loaded);
    }

    public VolumesOwnedWindow(Series series, TsundokuLanguage language) : this()
    {
        string title = series.Titles.TryGetValue(language, out string? t) ? t : series.Titles[TsundokuLanguage.Romaji];
        SeriesTitleText.Text = title;
        Title = $"Volumes Owned · {title}";
        VolumeGrid.Series = series;
    }

    /// <summary>Opens the dialog for <paramref name="series"/> over <paramref name="owner"/>.</summary>
    public static Task ShowForAsync(Series series, TsundokuLanguage language, Window owner) =>
        new VolumesOwnedWindow(series, language).ShowDialog(owner);

    private void FitToScreen()
    {
        Screen? screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null) return;

        double scaling = screen.Scaling;
        PixelRect workArea = screen.WorkingArea;
        double availableHeight = (workArea.Height / scaling) - (ScreenMargin * 2);

        // SizeToContent has sized the window to show every row; the rows area may shrink to the minimum.
        double naturalHeight = Bounds.Height;
        double shrinkableBy = Math.Max(0, VolumeGrid.RowsContentHeight - VolumeGridEditor.MinimumRowsHeight);
        double minimumHeight = naturalHeight - shrinkableBy;

        SizeToContent = SizeToContent.Manual;
        MaxHeight = Math.Max(availableHeight, minimumHeight);
        MinHeight = Math.Min(minimumHeight, MaxHeight);
        Height = Math.Clamp(naturalHeight, MinHeight, MaxHeight);

        int topLimit = workArea.Y + (int)(ScreenMargin * scaling);
        int bottomLimit = workArea.Bottom - (int)(ScreenMargin * scaling) - (int)(Height * scaling);
        int y = Math.Clamp(Position.Y, topLimit, Math.Max(topLimit, bottomLimit));
        Position = new PixelPoint(Position.X, y);
    }
}
