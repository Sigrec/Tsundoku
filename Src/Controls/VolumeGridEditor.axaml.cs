using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Tsundoku.Helpers;
using Tsundoku.Models;

namespace Tsundoku.Controls;

/// <summary>
/// Edit-window grid of owned volumes: rows of ten chips (specials sit after the volume they follow),
/// a range box for typing <c>1-46, 48-100, 18.5</c>, and a box for adding specials. Changes apply
/// to the series immediately, like the genre and demographic pickers.
/// </summary>
public sealed partial class VolumeGridEditor : UserControl
{
    private const int RowSize = 10;
    private const int UnknownMaxExtraVolumes = 5;

    public static readonly StyledProperty<Series?> SeriesProperty =
        AvaloniaProperty.Register<VolumeGridEditor, Series?>(nameof(Series));

    private readonly HashSet<uint> _expandedRows = [];
    private Series? _observedSeries;
    private decimal? _lastClickedMain;

    static VolumeGridEditor() =>
        SeriesProperty.Changed.AddClassHandler<VolumeGridEditor>((editor, _) => editor.OnSeriesChanged());

    public VolumeGridEditor() => InitializeComponent();

    public Series? Series
    {
        get => GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
    }

    /// <summary>Height of the rows area before it scrolls, at the window's smallest size.</summary>
    public const double MinimumRowsHeight = 172;

    /// <summary>Full height of every row, which the rows area needs to show them without scrolling.</summary>
    public double RowsContentHeight => RowsPanel.DesiredSize.Height;

    /// <summary>Raised after a rebuild that changed how many rows the grid has (e.g. the Extras line appeared).</summary>
    public event EventHandler? RowCountChanged;

    private int _lastRowCount = -1;

    /// <summary>Shows a message under the inputs, e.g. when the max volume count is rejected.</summary>
    public void ShowMessage(string message)
    {
        MessageText.Text = message;
        MessageText.IsVisible = !string.IsNullOrEmpty(message);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Observe(null);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OnSeriesChanged();
    }

    private void OnSeriesChanged()
    {
        _expandedRows.Clear();
        _lastClickedMain = null;
        SpecialBox.Text = string.Empty;
        ShowMessage(string.Empty);
        Observe(Series);
        Rebuild();
    }

    private void Observe(Series? series)
    {
        if (_observedSeries is not null)
        {
            _observedSeries.PropertyChanged -= OnSeriesPropertyChanged;
        }
        _observedSeries = series;
        if (_observedSeries is not null)
        {
            _observedSeries.PropertyChanged += OnSeriesPropertyChanged;
        }
    }

    private void OnSeriesPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Models.Series.OwnedVolumes) or nameof(Models.Series.SpecialVolumes)
            or nameof(Models.Series.ExtraVolumes) or nameof(Models.Series.MaxVolumeCount))
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        RowsPanel.Children.Clear();
        Series? series = Series;
        if (series is null)
        {
            HeaderText.Text = "VOLUMES OWNED";
            RangeBox.Text = string.Empty;
            return;
        }

        IReadOnlyList<decimal> owned = series.OwnedVolumes;
        IReadOnlyList<decimal> specials = series.SpecialVolumes;
        HashSet<decimal> ownedSet = [.. owned];
        uint max = series.MaxVolumeCount;
        uint displayEnd = max > 0 ? max : VolumeMath.HighestMain(owned) + UnknownMaxExtraVolumes;

        int specialTotal = specials.Count + series.ExtraVolumes.Count;
        int ownedSpecials = series.OwnedSpecialCount + series.OwnedExtraCount;
        string specialsText = specialTotal > 0 ? $" · {ownedSpecials} OF {specialTotal} SPECIALS" : string.Empty;
        string maxText = max > 0 ? max.ToString(CultureInfo.InvariantCulture) : "?";
        HeaderText.Text = $"VOLUMES OWNED · {series.CurVolumeCount} OF {maxText}{specialsText}";
        RangeBox.Text = VolumeMath.FormatRanges(owned);

        bool collapseFullRows = displayEnd > VolumeMath.DenseStripThreshold;
        int specialIndex = 0;
        for (uint start = 1; start <= displayEnd; start += RowSize)
        {
            uint end = Math.Min(start + RowSize - 1, displayEnd);

            List<decimal> rowSpecials = [];
            while (specialIndex < specials.Count && (specials[specialIndex] < end + 1 || end == displayEnd))
            {
                rowSpecials.Add(specials[specialIndex++]);
            }

            bool rowFull = rowSpecials.All(ownedSet.Contains);
            for (uint v = start; v <= end && rowFull; v++)
            {
                rowFull = ownedSet.Contains(v);
            }

            Grid row = new()
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto) { SharedSizeGroup = "RowLabel" }, new ColumnDefinition(1, GridUnitType.Star) },
            };
            TextBlock label = new() { Text = $"{start}–{end}", Margin = new Thickness(0, 0, 8, 4) };
            label.Classes.Add("row-label");
            row.Children.Add(label);

            Control content = rowFull && collapseFullRows && !_expandedRows.Contains(start)
                ? BuildCollapsedRow(start, end, rowSpecials)
                : BuildChipRow(start, end, rowSpecials, ownedSet);
            Grid.SetColumn(content, 1);
            row.Children.Add(content);

            RowsPanel.Children.Add(row);
        }

        if (series.ExtraVolumes.Count > 0)
        {
            RowsPanel.Children.Add(BuildExtrasRow(series.ExtraVolumes));
        }

        int rowCount = RowsPanel.Children.Count;
        if (_lastRowCount >= 0 && rowCount != _lastRowCount)
        {
            RowCountChanged?.Invoke(this, EventArgs.Empty);
        }
        _lastRowCount = rowCount;
    }

    /// <summary>Named extras on their own line after every numbered row, each chip sized to its name.</summary>
    private Grid BuildExtrasRow(IReadOnlyList<ExtraVolume> extras)
    {
        Grid row = new()
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto) { SharedSizeGroup = "RowLabel" }, new ColumnDefinition(1, GridUnitType.Star) },
            Margin = new Thickness(0, 4, 0, 0),
        };
        TextBlock label = new() { Text = "Extras", Margin = new Thickness(0, 0, 8, 4) };
        label.Classes.Add("row-label");
        row.Children.Add(label);

        WrapPanel chips = new() { Orientation = Orientation.Horizontal };
        foreach (ExtraVolume extra in extras)
        {
            chips.Children.Add(BuildExtraChip(extra));
        }
        Grid.SetColumn(chips, 1);
        row.Children.Add(chips);
        return row;
    }

    private Border BuildExtraChip(ExtraVolume extra)
    {
        Border chip = new()
        {
            Margin = new Thickness(0, 0, 4, 4),
            Padding = new Thickness(10, 0),
            Child = new TextBlock { Text = extra.Name },
        };
        chip.Classes.Add("vol-chip");
        chip.Classes.Add("special");
        if (extra.Owned) chip.Classes.Add("owned");

        chip.PointerPressed += (_, e) =>
        {
            if (Series is not Series series) return;
            PointerPointProperties point = e.GetCurrentPoint(chip).Properties;
            e.Handled = true;
            ShowMessage(string.Empty);

            if (point.IsRightButtonPressed)
            {
                series.RemoveExtraVolume(extra.Name);
            }
            else if (point.IsLeftButtonPressed)
            {
                series.ToggleExtraVolume(extra.Name);
            }
        };
        return chip;
    }

    /// <summary>
    /// One row of chips stretched across the panel. Every row has <see cref="RowSize"/> equal main
    /// columns (empty ones pad a short last row) so chips line up down the grid; specials take
    /// narrower columns right after the volume they follow.
    /// </summary>
    private Grid BuildChipRow(uint start, uint end, List<decimal> rowSpecials, HashSet<decimal> ownedSet)
    {
        Grid chips = new();
        int column = 0;
        void Add(decimal volume, bool isSpecial)
        {
            chips.ColumnDefinitions.Add(isSpecial ? new ColumnDefinition(GridLength.Auto) : new ColumnDefinition(1, GridUnitType.Star));
            Border chip = BuildChip(volume, isSpecial, ownedSet);
            Grid.SetColumn(chip, column++);
            chips.Children.Add(chip);
        }

        int rowSpecialIndex = 0;
        while (rowSpecialIndex < rowSpecials.Count && rowSpecials[rowSpecialIndex] < start)
        {
            Add(rowSpecials[rowSpecialIndex++], isSpecial: true);
        }
        for (uint v = start; v <= end; v++)
        {
            Add(v, isSpecial: false);
            while (rowSpecialIndex < rowSpecials.Count && rowSpecials[rowSpecialIndex] < v + 1)
            {
                Add(rowSpecials[rowSpecialIndex++], isSpecial: true);
            }
        }
        while (rowSpecialIndex < rowSpecials.Count)
        {
            Add(rowSpecials[rowSpecialIndex++], isSpecial: true);
        }
        for (uint padding = end - start + 1; padding < RowSize; padding++)
        {
            chips.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        }
        return chips;
    }

    private Border BuildChip(decimal volume, bool isSpecial, HashSet<decimal> ownedSet)
    {
        Border chip = new()
        {
            Margin = new Thickness(0, 0, 4, 4),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = new TextBlock { Text = VolumeMath.Format(volume) },
        };
        chip.Classes.Add("vol-chip");
        if (isSpecial) chip.Classes.Add("special");
        if (ownedSet.Contains(volume)) chip.Classes.Add("owned");

        chip.PointerPressed += (_, e) =>
        {
            if (Series is not Series series) return;
            PointerPointProperties point = e.GetCurrentPoint(chip).Properties;
            e.Handled = true;
            ShowMessage(string.Empty);

            if (point.IsRightButtonPressed)
            {
                if (isSpecial) series.RemoveSpecialVolume(volume);
                return;
            }
            if (!point.IsLeftButtonPressed) return;

            if (!isSpecial && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && _lastClickedMain is decimal anchor)
            {
                decimal low = Math.Min(anchor, volume);
                decimal high = Math.Max(anchor, volume);
                List<decimal> range = [];
                for (decimal v = low; v <= high; v++)
                {
                    range.Add(v);
                }
                series.SetVolumesOwned(range, owned: !series.OwnedVolumes.Contains(volume));
            }
            else
            {
                series.ToggleVolume(volume);
            }

            if (!isSpecial)
            {
                _lastClickedMain = volume;
            }
        };
        return chip;
    }

    private Border BuildCollapsedRow(uint start, uint end, List<decimal> rowSpecials)
    {
        string specialsText = rowSpecials.Count > 0 ? " + " + string.Join(", ", rowSpecials.Select(VolumeMath.Format)) : string.Empty;
        Border collapsed = new()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 4, 4),
            Child = new TextBlock { Text = $"✓ All {end - start + 1} owned{specialsText}" },
        };
        collapsed.Classes.Add("vol-full");
        ToolTip.SetTip(collapsed, BuildTip("Click to show each volume"));
        collapsed.PointerPressed += (_, e) =>
        {
            e.Handled = true;
            _expandedRows.Add(start);
            Rebuild();
        };
        return collapsed;
    }

    private static TextBlock BuildTip(string text) => new()
    {
        Text = text,
        TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
        FontWeight = Avalonia.Media.FontWeight.Bold,
        FontSize = 12,
    };

    private void OwnAll(object? sender, RoutedEventArgs e)
    {
        if (Series is not Series series) return;
        ShowMessage(string.Empty);
        uint end = VolumeMath.MainRunEnd(series.OwnedVolumes, series.MaxVolumeCount);
        series.SetVolumesOwned([.. VolumeMath.Sequence(end), .. series.SpecialVolumes], owned: true);
        series.SetAllExtrasOwned(true);
    }

    private void OwnNone(object? sender, RoutedEventArgs e)
    {
        if (Series is not Series series) return;
        ShowMessage(string.Empty);
        _expandedRows.Clear();
        series.SetOwnedVolumes([]);
        series.SetAllExtrasOwned(false);
    }

    private void ApplyRanges(object? sender, RoutedEventArgs e)
    {
        if (Series is not Series series) return;
        string? error = VolumeMath.TryParseRanges(RangeBox.Text ?? string.Empty, series.MaxVolumeCount, out List<decimal> volumes);
        if (error is not null)
        {
            ShowMessage(error);
            return;
        }
        ShowMessage(string.Empty);
        series.SetOwnedVolumes(volumes);
    }

    private void RangeBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            ApplyRanges(sender, e);
        }
    }

    private void AddSpecial(object? sender, RoutedEventArgs e)
    {
        if (Series is not Series series) return;
        string text = (SpecialBox.Text ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            ShowMessage("Enter a special volume number like 18.5, or a name like Fanbook");
            return;
        }

        if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal volume))
        {
            if (text.Length > Models.Series.MaxExtraNameLength)
            {
                ShowMessage($"Names can be up to {Models.Series.MaxExtraNameLength} characters");
                return;
            }
            if (!series.AddExtraVolume(text))
            {
                ShowMessage($"\"{text}\" is already listed");
                return;
            }
            SpecialBox.Text = string.Empty;
            ShowMessage(string.Empty);
            return;
        }

        if (decimal.Round(volume, 2) != volume)
        {
            ShowMessage("Special volume numbers can have up to two decimal places, like 12.75");
            return;
        }
        if (VolumeMath.IsMain(volume))
        {
            ShowMessage($"Volume {VolumeMath.Format(volume)} is a regular volume. Specials are numbers like 18.5 or 0");
            return;
        }
        if (!series.AddSpecialVolume(volume))
        {
            ShowMessage($"Special {VolumeMath.Format(volume)} is already listed");
            return;
        }
        SpecialBox.Text = string.Empty;
        ShowMessage(string.Empty);
    }

    private void SpecialBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            AddSpecial(sender, e);
        }
    }
}
