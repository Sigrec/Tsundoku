using Avalonia.Controls;
using Avalonia.Media;
using Tsundoku.Helpers;
using Tsundoku.Models;

namespace Tsundoku.Controls;

/// <summary>
/// The series card's progress strip: one segment per main volume, plus narrower segments for
/// specials placed after the volume they follow. Up to <see cref="VolumeMath.DenseStripThreshold"/>
/// volumes the segments are drawn apart so gaps read at a glance; above that they run together and
/// missing volumes below the highest owned one show as notches. Drawn in one pass so a card costs a
/// single control however many volumes the series has.
/// </summary>
public sealed class VolumeStrip : Control
{
    private const double OuterBorderThickness = 2;
    private const double InnerPadding = 3;
    private const double SegmentGap = 2;
    private const double SpecialWidthRatio = 0.45;
    private const double MissingOutlineOpacity = 0.45;
    private const double MissingSpecialOpacity = 0.35;

    public static readonly StyledProperty<IReadOnlyList<decimal>?> OwnedVolumesProperty =
        AvaloniaProperty.Register<VolumeStrip, IReadOnlyList<decimal>?>(nameof(OwnedVolumes));

    public static readonly StyledProperty<IReadOnlyList<decimal>?> SpecialVolumesProperty =
        AvaloniaProperty.Register<VolumeStrip, IReadOnlyList<decimal>?>(nameof(SpecialVolumes));

    public static readonly StyledProperty<IReadOnlyList<ExtraVolume>?> ExtraVolumesProperty =
        AvaloniaProperty.Register<VolumeStrip, IReadOnlyList<ExtraVolume>?>(nameof(ExtraVolumes));

    public static readonly StyledProperty<uint> MaxVolumeCountProperty =
        AvaloniaProperty.Register<VolumeStrip, uint>(nameof(MaxVolumeCount));

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<VolumeStrip, IBrush?>(nameof(Fill));

    public static readonly StyledProperty<IBrush?> TrackProperty =
        AvaloniaProperty.Register<VolumeStrip, IBrush?>(nameof(Track));

    public static readonly StyledProperty<IBrush?> OutlineProperty =
        AvaloniaProperty.Register<VolumeStrip, IBrush?>(nameof(Outline));

    public static readonly StyledProperty<IBrush?> SpecialFillProperty =
        AvaloniaProperty.Register<VolumeStrip, IBrush?>(nameof(SpecialFill));

    public static readonly StyledProperty<IBrush?> MissingFillProperty =
        AvaloniaProperty.Register<VolumeStrip, IBrush?>(nameof(MissingFill));

    static VolumeStrip()
    {
        AffectsRender<VolumeStrip>(
            OwnedVolumesProperty, SpecialVolumesProperty, ExtraVolumesProperty, MaxVolumeCountProperty,
            FillProperty, TrackProperty, OutlineProperty, SpecialFillProperty, MissingFillProperty);
    }

    public IReadOnlyList<decimal>? OwnedVolumes
    {
        get => GetValue(OwnedVolumesProperty);
        set => SetValue(OwnedVolumesProperty, value);
    }

    public IReadOnlyList<decimal>? SpecialVolumes
    {
        get => GetValue(SpecialVolumesProperty);
        set => SetValue(SpecialVolumesProperty, value);
    }

    /// <summary>Named extras, drawn as special segments after every numbered volume.</summary>
    public IReadOnlyList<ExtraVolume>? ExtraVolumes
    {
        get => GetValue(ExtraVolumesProperty);
        set => SetValue(ExtraVolumesProperty, value);
    }

    public uint MaxVolumeCount
    {
        get => GetValue(MaxVolumeCountProperty);
        set => SetValue(MaxVolumeCountProperty, value);
    }

    /// <summary>Owned main volumes.</summary>
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>Background behind the segments.</summary>
    public IBrush? Track
    {
        get => GetValue(TrackProperty);
        set => SetValue(TrackProperty, value);
    }

    /// <summary>Outer border of the strip.</summary>
    public IBrush? Outline
    {
        get => GetValue(OutlineProperty);
        set => SetValue(OutlineProperty, value);
    }

    /// <summary>Special volumes (owned solid, missing faded).</summary>
    public IBrush? SpecialFill
    {
        get => GetValue(SpecialFillProperty);
        set => SetValue(SpecialFillProperty, value);
    }

    /// <summary>Notches for missing volumes in the dense (no-gap) layout.</summary>
    public IBrush? MissingFill
    {
        get => GetValue(MissingFillProperty);
        set => SetValue(MissingFillProperty, value);
    }

    private List<(decimal Volume, bool IsSpecial, bool IsOwned)>? _units;
    private uint _mainCount;
    private double _totalWeight;
    private uint _highestOwned;
    private Pen? _outlinePen;
    private Pen? _missingMainPen;
    private Pen? _missingSpecialPen;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OwnedVolumesProperty
            || change.Property == SpecialVolumesProperty
            || change.Property == ExtraVolumesProperty
            || change.Property == MaxVolumeCountProperty)
        {
            _units = null;
        }
        else if (change.Property == OutlineProperty)
        {
            _outlinePen = null;
        }
        else if (change.Property == FillProperty)
        {
            _missingMainPen = null;
        }
        else if (change.Property == SpecialFillProperty)
        {
            _missingSpecialPen = null;
        }
    }

    private List<(decimal Volume, bool IsSpecial, bool IsOwned)> GetUnits()
    {
        if (_units is not null) return _units;

        IReadOnlyList<decimal> owned = OwnedVolumes ?? [];
        List<(decimal Volume, bool IsSpecial, bool IsOwned)> units = BuildUnits(owned, SpecialVolumes ?? [], ExtraVolumes ?? [], MaxVolumeCount);

        uint mainCount = 0;
        double totalWeight = 0;
        foreach ((_, bool isSpecial, _) in units)
        {
            totalWeight += isSpecial ? SpecialWidthRatio : 1;
            if (!isSpecial) mainCount++;
        }

        _mainCount = mainCount;
        _totalWeight = totalWeight;
        _highestOwned = VolumeMath.HighestMain(owned);
        return _units = units;
    }

    public override void Render(DrawingContext context)
    {
        Rect bounds = new(Bounds.Size);
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        Rect outer = bounds.Deflate(OuterBorderThickness / 2);
        if (Outline is not null) _outlinePen ??= new Pen(Outline, OuterBorderThickness);
        context.DrawRectangle(Track, Outline is null ? null : _outlinePen, outer, 6, 6);

        List<(decimal Volume, bool IsSpecial, bool IsOwned)> units = GetUnits();
        if (units.Count == 0) return;

        Rect inner = bounds.Deflate(OuterBorderThickness + InnerPadding);
        if (inner.Width <= 0 || inner.Height <= 0) return;

        bool dense = _mainCount > VolumeMath.DenseStripThreshold;
        double gap = dense ? 0 : SegmentGap;
        double unitWidth = (inner.Width - (gap * (units.Count - 1))) / _totalWeight;
        if (unitWidth <= 0) return;

        uint highestOwned = _highestOwned;
        if (Fill is not null) _missingMainPen ??= new Pen(Fill, 1);
        if (SpecialFill is not null) _missingSpecialPen ??= new Pen(SpecialFill, 1);
        Pen? missingMainPen = Fill is null ? null : _missingMainPen;
        Pen? missingSpecialPen = SpecialFill is null ? null : _missingSpecialPen;

        using DrawingContext.PushedState clip = context.PushClip(new RoundedRect(inner, 3));
        double x = inner.X;
        foreach ((decimal volume, bool isSpecial, bool isOwned) in units)
        {
            double width = unitWidth * (isSpecial ? SpecialWidthRatio : 1);
            Rect segment = new(x, inner.Y, Math.Max(width, 0.5), inner.Height);

            if (dense)
            {
                if (isOwned)
                {
                    context.DrawRectangle(isSpecial ? SpecialFill : Fill, null, segment);
                }
                else if (isSpecial)
                {
                    using DrawingContext.PushedState faded = context.PushOpacity(MissingSpecialOpacity);
                    context.DrawRectangle(SpecialFill, null, segment);
                }
                else if (volume < highestOwned)
                {
                    context.DrawRectangle(MissingFill, null, segment);
                }
            }
            else if (isOwned)
            {
                context.DrawRectangle(isSpecial ? SpecialFill : Fill, null, segment, 2, 2);
            }
            else
            {
                using DrawingContext.PushedState faded = context.PushOpacity(isSpecial ? 1 : MissingOutlineOpacity);
                context.DrawRectangle(null, isSpecial ? missingSpecialPen : missingMainPen, segment.Deflate(0.5), 2, 2);
            }

            x += width + gap;
        }
    }

    /// <summary>
    /// The segments in display order: specials below 1 first, then each main volume followed by any
    /// specials between it and the next, then specials past the last main volume, then named extras.
    /// </summary>
    private static List<(decimal Volume, bool IsSpecial, bool IsOwned)> BuildUnits(
        IReadOnlyList<decimal> owned, IReadOnlyList<decimal> specials, IReadOnlyList<ExtraVolume> extras, uint maxVolumeCount)
    {
        HashSet<decimal> ownedSet = [.. owned];
        uint end = VolumeMath.MainRunEnd(owned, maxVolumeCount);
        List<(decimal, bool, bool)> units = [with((int)end + specials.Count + extras.Count)];
        int specialIndex = 0;

        void AddSpecial(decimal special) => units.Add((special, true, ownedSet.Contains(special)));

        while (specialIndex < specials.Count && specials[specialIndex] < 1m)
        {
            AddSpecial(specials[specialIndex++]);
        }
        for (uint v = 1; v <= end; v++)
        {
            units.Add((v, false, ownedSet.Contains(v)));
            while (specialIndex < specials.Count && specials[specialIndex] < v + 1)
            {
                AddSpecial(specials[specialIndex++]);
            }
        }
        while (specialIndex < specials.Count)
        {
            AddSpecial(specials[specialIndex++]);
        }
        foreach (ExtraVolume extra in extras)
        {
            units.Add((decimal.MaxValue, true, extra.Owned));
        }
        return units;
    }
}
