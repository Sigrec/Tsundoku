using System.Reflection;
using Avalonia.Media;
using Tsundoku.Controls;

namespace Tsundoku.Tests.Controls;

[TestFixture]
public class VolumeStripTests
{
    private static readonly MethodInfo GetUnitsMethod =
        typeof(VolumeStrip).GetMethod("GetUnits", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly FieldInfo MissingMainPenField =
        typeof(VolumeStrip).GetField("_missingMainPen", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static List<(decimal Volume, bool IsSpecial, bool IsOwned)> GetUnits(VolumeStrip strip) =>
        (List<(decimal Volume, bool IsSpecial, bool IsOwned)>)GetUnitsMethod.Invoke(strip, null)!;

    [AvaloniaTest]
    public void Units_AreReused_UntilAVolumePropertyChanges()
    {
        VolumeStrip strip = new() { OwnedVolumes = [1m, 2m], MaxVolumeCount = 3 };

        List<(decimal Volume, bool IsSpecial, bool IsOwned)> first = GetUnits(strip);

        Assert.That(GetUnits(strip), Is.SameAs(first));
    }

    [AvaloniaTest]
    public void Units_Rebuild_WhenOwnedVolumesChange()
    {
        VolumeStrip strip = new() { OwnedVolumes = [1m, 2m], MaxVolumeCount = 3 };
        _ = GetUnits(strip);

        strip.OwnedVolumes = [1m, 2m, 3m];

        Assert.That(GetUnits(strip).Select(u => u.IsOwned), Is.EqualTo(new[] { true, true, true }));
    }

    [AvaloniaTest]
    public void Units_Rebuild_WhenSpecialsExtrasOrMaxChange()
    {
        VolumeStrip strip = new() { OwnedVolumes = [1m], MaxVolumeCount = 2 };
        _ = GetUnits(strip);

        strip.SpecialVolumes = [1.5m];
        Assert.That(GetUnits(strip), Has.Count.EqualTo(3));

        strip.ExtraVolumes = [new ExtraVolume("Fanbook", Owned: true)];
        Assert.That(GetUnits(strip), Has.Count.EqualTo(4));

        strip.MaxVolumeCount = 4;
        Assert.That(GetUnits(strip), Has.Count.EqualTo(6));
    }

    [AvaloniaTest]
    public void MissingPen_IsCleared_WhenFillChanges()
    {
        VolumeStrip strip = new() { Fill = Brushes.Red };
        MissingMainPenField.SetValue(strip, new Pen(Brushes.Red, 1));

        strip.Fill = Brushes.Blue;

        Assert.That(MissingMainPenField.GetValue(strip), Is.Null);
    }
}
