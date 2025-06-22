namespace Tsundoku.Tests.Extensions;

[TestFixture]
public class StringSimilarExtensionMethodTests
{
    [Test]
    public void Similar_BothStringsEqual_ReturnsZero()
    {
        int result = ExtensionMethods.Similar("Berserk", "Berserk", 5);
        Assert.That(result, Is.Zero);
    }

    [Test]
    public void Similar_OneCharDifferenceWithinThreshold_ReturnsDistance()
    {
        int result = ExtensionMethods.Similar("Berserk", "Berzerk", 2); // 1 substitution
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void Similar_OneCharDifferenceOutsideThreshold_ReturnsMinusOne()
    {
        int result = ExtensionMethods.Similar("Berserk", "Berzerk", 0); // 1 substitution, threshold 0
        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void Similar_EmptySourceString_ReturnsLengthOfTarget()
    {
        int result = ExtensionMethods.Similar("", "Test", 4);
        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void Similar_EmptyTargetString_ReturnsLengthOfSource()
    {
        int result = ExtensionMethods.Similar("Test", "", 4);
        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void Similar_NullSource_ReturnsLengthOfTarget()
    {
        int result = ExtensionMethods.Similar(null, "Alt", 4);
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void Similar_NullTarget_ReturnsLengthOfSource()
    {
        int result = ExtensionMethods.Similar("Alt", null, 4);
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void Similar_NullAndLongTarget_ExceedsThreshold_ReturnsMinusOne()
    {
        int result = ExtensionMethods.Similar(null, "VeryLongName", 4);
        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void Similar_NullAndLongSource_ExceedsThreshold_ReturnsMinusOne()
    {
        int result = ExtensionMethods.Similar("VeryLongName", null, 4);
        Assert.That(result, Is.EqualTo(-1));
    }

    [Test]
    public void Similar_CompletelyDifferentStringsWithinThreshold_ReturnsDistance()
    {
        int result = ExtensionMethods.Similar("abc", "xyz", 3); // All 3 chars different
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void Similar_CompletelyDifferentStringsExceedThreshold_ReturnsMinusOne()
    {
        int result = ExtensionMethods.Similar("abc", "xyz", 2); // All 3 chars different
        Assert.That(result, Is.EqualTo(-1));
    }
}
