using Tsundoku.ViewModels;

namespace Tsundoku.Tests.ViewModels;

[TestFixture]
public class FilterBuilderViewModelTests
{

    [AvaloniaTest]
    public void Constructor_SynthesizedQueryIsEmpty()
    {
        FilterBuilderViewModel vm = new();

        Assert.That(vm.SynthesizedQuery, Is.EqualTo(string.Empty));

        vm.Dispose();
    }

    [AvaloniaTest]
    public void AddChip_IncreasesChipCount()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();

        Assert.That(vm.Chips, Has.Count.EqualTo(1));

        vm.Dispose();
    }

    [AvaloniaTest]
    public void AddChip_MultipleTimes_IncreasesChipCount()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();
        vm.AddChip();
        vm.AddChip();

        Assert.That(vm.Chips, Has.Count.EqualTo(3));

        vm.Dispose();
    }

    [AvaloniaTest]
    public void AddChip_SingleChip_ShowConnectorIsFalse()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();

        Assert.That(vm.Chips[0].ShowConnector, Is.False);

        vm.Dispose();
    }

    [AvaloniaTest]
    public void AddChip_TwoChips_FirstShowsConnectorSecondDoesNot()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();
        vm.AddChip();

        Assert.Multiple(() =>
        {
            Assert.That(vm.Chips[0].ShowConnector, Is.True);
            Assert.That(vm.Chips[1].ShowConnector, Is.False);
        });

        vm.Dispose();
    }

    [AvaloniaTest]
    public void AddChip_ThreeChips_ConnectorVisibilityIsCorrect()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();
        vm.AddChip();
        vm.AddChip();

        Assert.Multiple(() =>
        {
            Assert.That(vm.Chips[0].ShowConnector, Is.True);
            Assert.That(vm.Chips[1].ShowConnector, Is.True);
            Assert.That(vm.Chips[2].ShowConnector, Is.False);
        });

        vm.Dispose();
    }

    [AvaloniaTest]
    public void RemoveChip_RemovesFromCollection()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();
        vm.AddChip();

        FilterChipViewModel chipToRemove = vm.Chips[0];
        vm.RemoveChip(chipToRemove);

        Assert.That(vm.Chips, Has.Count.EqualTo(1));

        vm.Dispose();
    }

    [AvaloniaTest]
    public void RemoveChip_UpdatesConnectorVisibility()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();
        vm.AddChip();
        vm.AddChip();

        // Remove middle chip
        vm.RemoveChip(vm.Chips[1]);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Chips, Has.Count.EqualTo(2));
            Assert.That(vm.Chips[0].ShowConnector, Is.True);
            Assert.That(vm.Chips[1].ShowConnector, Is.False);
        });

        vm.Dispose();
    }

    [AvaloniaTest]
    public void RemoveChip_LastChip_SetsSynthesizedQueryToEmpty()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();
        FilterChipViewModel chip = vm.Chips[0];
        vm.RemoveChip(chip);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Chips, Has.Count.EqualTo(0));
            Assert.That(vm.SynthesizedQuery, Is.EqualTo(string.Empty));
        });

        vm.Dispose();
    }

    [AvaloniaTest]
    public void ClearAll_RemovesAllChips()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();
        vm.AddChip();
        vm.AddChip();

        vm.ClearAll();

        Assert.Multiple(() =>
        {
            Assert.That(vm.Chips, Has.Count.EqualTo(0));
            Assert.That(vm.SynthesizedQuery, Is.EqualTo(string.Empty));
        });

        vm.Dispose();
    }

    [AvaloniaTest]
    public void RemoveChip_SingleRemainingChip_ShowConnectorIsFalse()
    {
        FilterBuilderViewModel vm = new();

        vm.AddChip();
        vm.AddChip();

        vm.RemoveChip(vm.Chips[1]);

        Assert.That(vm.Chips[0].ShowConnector, Is.False);

        vm.Dispose();
    }
}
