using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicData;

namespace Tsundoku.Helpers;

public static class ChangeSetSnapshotExtensions
{
    /// <summary>
    /// Emits a copy of the cache's items once a burst of changes has been quiet for <paramref name="dueTime"/>.
    /// Unlike <c>ToCollection().Throttle(...)</c>, the items are copied once per emitted snapshot
    /// instead of once per change, so a bulk edit of N series costs one copy rather than N.
    /// </summary>
    public static IObservable<IReadOnlyList<TObject>> ThrottledSnapshot<TObject, TKey>(
        this IObservable<IChangeSet<TObject, TKey>> source,
        TimeSpan dueTime,
        IScheduler? scheduler = null)
        where TObject : notnull
        where TKey : notnull
    {
        return Observable.Create<IReadOnlyList<TObject>>(observer =>
        {
            Dictionary<TKey, TObject> items = [];
            Lock gate = new();

            return source
                .Do(changes =>
                {
                    lock (gate)
                    {
                        foreach (Change<TObject, TKey> change in changes)
                        {
                            switch (change.Reason)
                            {
                                case ChangeReason.Add:
                                case ChangeReason.Update:
                                    items[change.Key] = change.Current;
                                    break;
                                case ChangeReason.Remove:
                                    items.Remove(change.Key);
                                    break;
                            }
                        }
                    }
                })
                .Throttle(dueTime, scheduler ?? DefaultScheduler.Instance)
                .Select(_ =>
                {
                    lock (gate)
                    {
                        return (IReadOnlyList<TObject>)[.. items.Values];
                    }
                })
                .Subscribe(observer);
        });
    }
}
