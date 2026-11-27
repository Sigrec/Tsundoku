using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesStatusModel;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Tests.Models;

[TestFixture]
public class SeriesOwnedVolumesTests
{
    [Test]
    public void NewSeries_OwnsVolumesOneThroughCurrentCount()
    {
        Series series = CreateSeries(max: 10, cur: 3);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 1, 2, 3 }));
            Assert.That(series.CurVolumeCount, Is.EqualTo(3u));
        }
    }

    [Test]
    public void Increment_AddsNextAfterHighest_ThenFillsGapsAtMax()
    {
        Series series = CreateSeries(max: 4, cur: 0);
        series.SetOwnedVolumes([1, 3]);

        series.IncrementCurVolumeCount();
        Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 1, 3, 4 }));

        series.IncrementCurVolumeCount();
        Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 1, 2, 3, 4 }));
    }

    [Test]
    public void Decrement_RemovesHighestMainVolume_KeepsSpecials()
    {
        Series series = CreateSeries(max: 10, cur: 0);
        series.SetOwnedVolumes([1, 2, 2.5m]);

        series.DecrementCurVolumeCount();

        Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 1, 2.5m }));
    }

    [Test]
    public void Specials_DoNotCountTowardCurrentCount()
    {
        Series series = CreateSeries(max: 2, cur: 2);
        bool added = series.AddSpecialVolume(1.5m);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(added, Is.True);
            Assert.That(series.CurVolumeCount, Is.EqualTo(2u));
            Assert.That(series.OwnedSpecialCount, Is.EqualTo(1));
            Assert.That(series.ProgressText, Is.EqualTo("2 / 2 +1"));
        }
    }

    [Test]
    public void AddSpecialVolume_RejectsMainNumbersAndDuplicates()
    {
        Series series = CreateSeries(max: 10, cur: 0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(series.AddSpecialVolume(3), Is.False);
            Assert.That(series.AddSpecialVolume(3.5m), Is.True);
            Assert.That(series.AddSpecialVolume(3.5m), Is.False);
        }
    }

    [Test]
    public void ToggleVolume_NewDecimal_IsListedAsSpecial()
    {
        Series series = CreateSeries(max: 10, cur: 0);
        series.ToggleVolume(18.5m);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(series.SpecialVolumes, Is.EqualTo(new List<decimal> { 18.5m }));
            Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 18.5m }));
        }
    }

    [Test]
    public void RemoveSpecialVolume_RemovesFromBothLists()
    {
        Series series = CreateSeries(max: 10, cur: 1);
        series.AddSpecialVolume(0.5m);
        series.RemoveSpecialVolume(0.5m);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(series.SpecialVolumes, Is.Empty);
            Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 1 }));
        }
    }

    [Test]
    public void UpdateMaxVolumeCount_BelowHighestOwned_IsRefused()
    {
        Series series = CreateSeries(max: 10, cur: 0);
        series.SetOwnedVolumes([1, 8]);

        bool changed = series.UpdateMaxVolumeCount(5);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(changed, Is.False);
            Assert.That(series.MaxVolumeCount, Is.EqualTo(10u));
        }
    }

    [Test]
    public void MarkMainRunComplete_OwnsEveryMainVolume_LeavesSpecials()
    {
        Series series = CreateSeries(max: 4, cur: 1);
        series.SpecialVolumes = [2.5m];

        series.MarkMainRunComplete();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 1, 2, 3, 4 }));
            Assert.That(series.MissingVolumes, Is.EqualTo(new List<decimal> { 2.5m }));
        }
    }

    [Test]
    public void GapText_ListsMissingRangesAndSpecials()
    {
        Series series = CreateSeries(max: 10, cur: 0);
        series.SpecialVolumes = [3.5m];
        series.SetOwnedVolumes([1, 2, 5, 6, 10]);

        Assert.That(series.GapText, Is.EqualTo("Missing 3–4, 7–9 · special 3.5"));
    }

    [Test]
    public void SettingCurrentCount_AddsAndRemovesFromTheTop()
    {
        Series series = CreateSeries(max: 10, cur: 0);
        series.SetOwnedVolumes([1, 3]);

        series.CurVolumeCount = 4;
        Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 1, 3, 4, 5 }));

        series.CurVolumeCount = 2;
        Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 1, 3 }));
    }

    [Test]
    public void Snapshot_RestoresOwnedAndSpecialVolumes()
    {
        Series series = CreateSeries(max: 10, cur: 0);
        series.SetOwnedVolumes([1, 4, 4.5m]);
        Tsundoku.Helpers.SeriesSnapshot snapshot = Tsundoku.Helpers.SeriesSnapshot.Capture(series);

        series.SetOwnedVolumes([1, 2, 3]);
        series.RemoveSpecialVolume(4.5m);
        snapshot.ApplyTo(series);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(series.OwnedVolumes, Is.EqualTo(new List<decimal> { 1, 4, 4.5m }));
            Assert.That(series.SpecialVolumes, Is.EqualTo(new List<decimal> { 4.5m }));
            Assert.That(series.CurVolumeCount, Is.EqualTo(2u));
        }
    }

    [Test]
    public void AddExtraVolume_AddsOwnedAndCountsBeyondMain()
    {
        Series series = CreateSeries(max: 5, cur: 5);
        bool added = series.AddExtraVolume("  Fanbook ");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(added, Is.True);
            Assert.That(series.ExtraVolumes, Is.EqualTo(new List<ExtraVolume> { new("Fanbook", Owned: true) }));
            Assert.That(series.CurVolumeCount, Is.EqualTo(5u));
            Assert.That(series.ProgressText, Is.EqualTo("5 / 5 +1"));
        }
    }

    [Test]
    public void AddExtraVolume_RejectsBlankAndDuplicateNames()
    {
        Series series = CreateSeries(max: 5, cur: 0);
        series.AddExtraVolume("Fanbook");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(series.AddExtraVolume("   "), Is.False);
            Assert.That(series.AddExtraVolume("fanbook"), Is.False);
            Assert.That(series.AddExtraVolume(new string('x', Series.MaxExtraNameLength + 1)), Is.False);
        }
    }

    [Test]
    public void ToggleExtraVolume_MissingExtraCountsAsMissing()
    {
        Series series = CreateSeries(max: 3, cur: 3);
        series.AddExtraVolume("Box Set Bonus");
        series.ToggleExtraVolume("Box Set Bonus");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(series.OwnedExtraCount, Is.Zero);
            Assert.That(series.MissingExtraNames, Is.EqualTo(new List<string> { "Box Set Bonus" }));
            Assert.That(series.MissingVolumeCount, Is.EqualTo(1));
        }
    }

    [Test]
    public void Snapshot_RestoresExtras()
    {
        Series series = CreateSeries(max: 3, cur: 3);
        series.AddExtraVolume("Fanbook");
        Tsundoku.Helpers.SeriesSnapshot snapshot = Tsundoku.Helpers.SeriesSnapshot.Capture(series);

        series.RemoveExtraVolume("Fanbook");
        snapshot.ApplyTo(series);

        Assert.That(series.ExtraVolumes, Is.EqualTo(new List<ExtraVolume> { new("Fanbook", Owned: true) }));
    }

    private static Series CreateSeries(uint max, uint cur) => new(
        Titles: new() { [TsundokuLanguage.Romaji] = "Test" },
        Staff: new() { [TsundokuLanguage.Romaji] = "Author" },
        Description: string.Empty,
        Format: SeriesFormat.Manga,
        Status: SeriesStatus.Ongoing,
        Cover: "test.png",
        Link: new Uri("https://anilist.co/manga/1"),
        Genres: [],
        MaxVolumeCount: max,
        CurVolumeCount: cur,
        Rating: -1,
        VolumesRead: 0,
        Value: 0,
        Demographic: SeriesDemographic.Unknown,
        CoverBitMap: null);
}
