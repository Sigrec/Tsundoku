using Tsundoku.Helpers;

namespace Tsundoku.Tests.Helpers;

[TestFixture]
public class VolumeMathTests
{
    #region IsMain

    [TestCase(1, true)]
    [TestCase(100, true)]
    [TestCase(0, false)]
    [TestCase(18.5, false)]
    [TestCase(0.5, false)]
    public void IsMain_ClassifiesVolumeNumbers(double volume, bool expected) =>
        Assert.That(VolumeMath.IsMain((decimal)volume), Is.EqualTo(expected));

    #endregion

    #region Normalize

    [Test]
    public void Normalize_SortsDeduplicatesAndDropsNegatives()
    {
        List<decimal> result = VolumeMath.Normalize([3, 1, 2, 2, -1, 18.5m]);
        Assert.That(result, Is.EqualTo(new List<decimal> { 1, 2, 3, 18.5m }));
    }

    [Test]
    public void NormalizeSpecials_KeepsOnlyNonMainNumbers()
    {
        List<decimal> result = VolumeMath.NormalizeSpecials([0, 1, 18.5m, 2, 0.5m]);
        Assert.That(result, Is.EqualTo(new List<decimal> { 0, 0.5m, 18.5m }));
    }

    #endregion

    #region Counting and gaps

    [Test]
    public void CountMain_IgnoresSpecials()
    {
        Assert.That(VolumeMath.CountMain([0, 1, 2, 18.5m, 19]), Is.EqualTo(3u));
    }

    [Test]
    public void MissingMain_ListsGapsUpToMax()
    {
        List<decimal> missing = VolumeMath.MissingMain([1, 2, 3, 5], maxVolumeCount: 6);
        Assert.That(missing, Is.EqualTo(new List<decimal> { 4, 6 }));
    }

    [Test]
    public void MissingMain_UnknownMax_StopsAtHighestOwned()
    {
        List<decimal> missing = VolumeMath.MissingMain([1, 4], maxVolumeCount: 0);
        Assert.That(missing, Is.EqualTo(new List<decimal> { 2, 3 }));
    }

    [Test]
    public void HasGaps_ContiguousRun_IsFalseEvenWhenIncomplete()
    {
        Assert.That(VolumeMath.HasGaps([1, 2, 3], []), Is.False);
    }

    [Test]
    public void HasGaps_SkippedMainVolume_IsTrue()
    {
        Assert.That(VolumeMath.HasGaps([1, 2, 4], []), Is.True);
    }

    [Test]
    public void HasGaps_MissingSpecialBelowHighest_IsTrue()
    {
        Assert.That(VolumeMath.HasGaps([1, 2, 3], [2.5m]), Is.True);
    }

    [Test]
    public void HasGaps_MissingSpecialAboveHighest_IsFalse()
    {
        Assert.That(VolumeMath.HasGaps([1, 2, 3], [3.5m]), Is.False);
    }

    #endregion

    #region NextToAdd

    [Test]
    public void NextToAdd_BelowMax_AddsAfterHighest()
    {
        Assert.That(VolumeMath.NextToAdd([1, 2, 4], maxVolumeCount: 10), Is.EqualTo(5m));
    }

    [Test]
    public void NextToAdd_AtMax_FillsLowestGap()
    {
        Assert.That(VolumeMath.NextToAdd([1, 2, 4, 5], maxVolumeCount: 5), Is.EqualTo(3m));
    }

    [Test]
    public void NextToAdd_AllOwned_ReturnsNull()
    {
        Assert.That(VolumeMath.NextToAdd([1, 2, 3], maxVolumeCount: 3), Is.Null);
    }

    #endregion

    #region Ranges

    [Test]
    public void FormatRanges_CollapsesRunsAndListsSpecialsLast()
    {
        string text = VolumeMath.FormatRanges([1, 2, 3, 5, 7, 8, 18.5m]);
        Assert.That(text, Is.EqualTo("1-3, 5, 7-8, 18.5"));
    }

    [Test]
    public void FormatRanges_SpecialInsideARun_DoesNotSplitTheRun()
    {
        string text = VolumeMath.FormatRanges([17, 18, 18.5m, 19, 20]);
        Assert.That(text, Is.EqualTo("17-20, 18.5"));
    }

    [Test]
    public void TryParseRanges_ParsesRangesSinglesAndSpecials()
    {
        string? error = VolumeMath.TryParseRanges("1-3, 5, 18.5", maxVolumeCount: 20, out List<decimal> volumes);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(error, Is.Null);
            Assert.That(volumes, Is.EqualTo(new List<decimal> { 1, 2, 3, 5, 18.5m }));
        }
    }

    [Test]
    public void TryParseRanges_RangePastMax_ReturnsError()
    {
        string? error = VolumeMath.TryParseRanges("1-25", maxVolumeCount: 20, out _);
        Assert.That(error, Does.Contain("outside 1–20"));
    }

    [Test]
    public void TryParseRanges_Garbage_ReturnsError()
    {
        string? error = VolumeMath.TryParseRanges("1-3, abc", maxVolumeCount: 20, out _);
        Assert.That(error, Does.Contain("abc"));
    }

    [Test]
    public void TryParseRanges_RoundTripsWithFormatRanges()
    {
        List<decimal> original = [1, 2, 3, 10, 11, 0.5m, 18.5m];
        string text = VolumeMath.FormatRanges(original);
        VolumeMath.TryParseRanges(text, maxVolumeCount: 20, out List<decimal> parsed);
        Assert.That(parsed, Is.EqualTo(VolumeMath.Normalize(original)));
    }

    #endregion
}
