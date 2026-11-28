using Avalonia.Controls;
using Tsundoku.Helpers;
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
    public VolumesOwnedWindow()
    {
        InitializeComponent();
        WindowFit.SetShrinkableHeight(this, () => VolumeGrid.RowsContentHeight - VolumeGridEditor.MinimumRowsHeight);
        VolumeGrid.RowCountChanged += (_, _) => WindowFit.Refit(this);
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
}
