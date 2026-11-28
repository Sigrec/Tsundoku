using System.Reactive.Concurrency;
using DynamicData;

namespace Tsundoku.Tests.Helpers;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class ChangeSetSnapshotExtensionsTests
{
    private static readonly TimeSpan DueTime = TimeSpan.FromMilliseconds(100);

    [Test]
    public void Burst_EmitsOneSnapshotWithFinalItems()
    {
        HistoricalScheduler scheduler = new();
        using SourceCache<(int Id, string Value), int> cache = new(x => x.Id);
        List<IReadOnlyList<(int Id, string Value)>> snapshots = [];
        using IDisposable subscription = cache.Connect().ThrottledSnapshot(DueTime, scheduler).Subscribe(snapshots.Add);

        for (int i = 0; i < 50; i++)
        {
            cache.AddOrUpdate((i, "a"));
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(10));
        }
        cache.AddOrUpdate((3, "b"));
        cache.RemoveKey(7);

        Assert.That(snapshots, Is.Empty);

        scheduler.AdvanceBy(DueTime);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshots, Has.Count.EqualTo(1));
            Assert.That(snapshots[0], Has.Count.EqualTo(49));
            Assert.That(snapshots[0], Does.Contain((3, "b")));
            Assert.That(snapshots[0].Select(x => x.Id), Does.Not.Contain(7));
        }
    }

    [Test]
    public void SeparateQuietPeriods_EmitSeparateSnapshots()
    {
        HistoricalScheduler scheduler = new();
        using SourceCache<(int Id, string Value), int> cache = new(x => x.Id);
        List<IReadOnlyList<(int Id, string Value)>> snapshots = [];
        using IDisposable subscription = cache.Connect().ThrottledSnapshot(DueTime, scheduler).Subscribe(snapshots.Add);

        cache.AddOrUpdate((1, "a"));
        scheduler.AdvanceBy(DueTime * 2);
        cache.AddOrUpdate((2, "a"));
        scheduler.AdvanceBy(DueTime * 2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshots, Has.Count.EqualTo(2));
            Assert.That(snapshots[0], Has.Count.EqualTo(1));
            Assert.That(snapshots[1], Has.Count.EqualTo(2));
        }
    }

    [Test]
    public void Snapshot_IsACopy_NotChangedByLaterEdits()
    {
        HistoricalScheduler scheduler = new();
        using SourceCache<(int Id, string Value), int> cache = new(x => x.Id);
        List<IReadOnlyList<(int Id, string Value)>> snapshots = [];
        using IDisposable subscription = cache.Connect().ThrottledSnapshot(DueTime, scheduler).Subscribe(snapshots.Add);

        cache.AddOrUpdate((1, "a"));
        scheduler.AdvanceBy(DueTime);
        cache.AddOrUpdate((2, "a"));

        Assert.That(snapshots[0], Has.Count.EqualTo(1));
    }

    [Test]
    public void ExistingItems_AreInFirstSnapshot()
    {
        HistoricalScheduler scheduler = new();
        using SourceCache<(int Id, string Value), int> cache = new(x => x.Id);
        cache.AddOrUpdate([(1, "a"), (2, "b")]);
        List<IReadOnlyList<(int Id, string Value)>> snapshots = [];
        using IDisposable subscription = cache.Connect().ThrottledSnapshot(DueTime, scheduler).Subscribe(snapshots.Add);

        scheduler.AdvanceBy(DueTime);

        Assert.That(snapshots.Single(), Has.Count.EqualTo(2));
    }
}
