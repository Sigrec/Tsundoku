using System.Reflection;
using Tsundoku.ViewModels;

namespace Tsundoku.Tests.ViewModels;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CollectionStatsViewModelTests
{
    private static readonly MethodInfo CalculatePercentageMethod =
        typeof(CollectionStatsViewModel).GetMethod(
            "CalculatePercentage",
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static decimal InvokeCalculatePercentage(double? count, decimal total)
    {
        object? result = CalculatePercentageMethod.Invoke(null, [count, total]);
        return (decimal)result!;
    }

    [Test]
    public void CalculatePercentage_NullCount_ReturnsZero()
    {
        decimal result = InvokeCalculatePercentage(null, 100m);

        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void CalculatePercentage_ZeroTotal_ReturnsZero()
    {
        decimal result = InvokeCalculatePercentage(5.0, 0m);

        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void CalculatePercentage_NullCountAndZeroTotal_ReturnsZero()
    {
        decimal result = InvokeCalculatePercentage(null, 0m);

        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void CalculatePercentage_HalfOfTotal_Returns50()
    {
        decimal result = InvokeCalculatePercentage(50.0, 100m);

        Assert.That(result, Is.EqualTo(50.00m));
    }

    [Test]
    public void CalculatePercentage_AllOfTotal_Returns100()
    {
        decimal result = InvokeCalculatePercentage(100.0, 100m);

        Assert.That(result, Is.EqualTo(100.00m));
    }

    [Test]
    public void CalculatePercentage_OneOutOfThree_ReturnsRoundedValue()
    {
        decimal result = InvokeCalculatePercentage(1.0, 3m);

        Assert.That(result, Is.EqualTo(33.33m));
    }

    [Test]
    public void CalculatePercentage_TwoOutOfThree_ReturnsRoundedValue()
    {
        decimal result = InvokeCalculatePercentage(2.0, 3m);

        Assert.That(result, Is.EqualTo(66.67m));
    }

    [Test]
    public void CalculatePercentage_SmallFraction_RoundsToTwoDecimals()
    {
        decimal result = InvokeCalculatePercentage(1.0, 7m);

        Assert.That(result, Is.EqualTo(14.29m));
    }

    [Test]
    public void CalculatePercentage_ZeroCount_ReturnsZero()
    {
        decimal result = InvokeCalculatePercentage(0.0, 100m);

        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void CalculatePercentage_LargeValues_CalculatesCorrectly()
    {
        decimal result = InvokeCalculatePercentage(999.0, 1000m);

        Assert.That(result, Is.EqualTo(99.90m));
    }

    [Test]
    public void CalculatePercentage_MethodExists()
    {
        Assert.That(CalculatePercentageMethod, Is.Not.Null,
            "CalculatePercentage static method should exist on CollectionStatsViewModel");
    }
}
