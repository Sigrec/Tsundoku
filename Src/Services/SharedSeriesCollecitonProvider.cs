using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;
using static Tsundoku.Models.TsundokuFilterModel;

namespace Tsundoku.Services
{
    /// <summary>
    /// Defines the contract for a service that provides a globally filtered and sorted
    /// ReadOnlyObservableCollection of Series.
    /// </summary>
    public interface ISharedSeriesCollectionProvider
    {
        /// <summary>
        /// Gets the filtered and sorted collection of Series that ViewModels can bind to.
        /// </summary>
        ReadOnlyObservableCollection<Series> DynamicUserCollection { get; }

        // These properties are added to the interface because they are now part of the
        // provider's public contract, allowing ViewModels to control the global filter criteria.
        string SeriesFilterText { get; set; }
        TsundokuFilter SelectedFilter { get; set; }
    }

    /// <summary>
    /// Provides a globally filtered and sorted ReadOnlyObservableCollection of Series
    /// that can be consumed by multiple ViewModels.
    /// </summary>
    public class SharedSeriesCollectionProvider : ReactiveObject, ISharedSeriesCollectionProvider, IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private ReadOnlyObservableCollection<Series> _dynamicUserCollection;

        /// <summary>
        /// Gets the filtered and sorted collection of Series. ViewModels will bind to this property.
        /// </summary>
        public ReadOnlyObservableCollection<Series> DynamicUserCollection => _dynamicUserCollection;

        // These Reactive properties will now drive the global filtering.
        // ViewModels that want to control the global filter will bind to these properties.
        [Reactive] public string SeriesFilterText { get; set; } = string.Empty;
        [Reactive] public TsundokuFilter SelectedFilter { get; set; } = TsundokuFilter.None;

        private readonly IUserService _userService; // Injected to get raw data and user language

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedSeriesCollectionProvider"/> class.
        /// Sets up the reactive pipeline to filter and sort the Series collection.
        /// </summary>
        /// <param name="userService">The service providing raw Series data and user information.</param>
        public SharedSeriesCollectionProvider(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            // 1. Define the text search filter predicate
            IObservable<Func<Series, bool>> textFilter = this.WhenAnyValue(x => x.SeriesFilterText)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(searchText =>
                {
                    if (string.IsNullOrWhiteSpace(searchText))
                        return series => true; // No filter if search text is empty

                    // Create the predicate function
                    return (Func<Series, bool>)(series =>
                    {
                        return
                            series.Titles.Any(t => t.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                            series.Staff.Any(t => t.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                            series.Publisher.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                    });
                });

            // 2. Define the TsundokuFilter (enum-based status) filter predicate
            IObservable<Func<Series, bool>> tsundokuFilter = this.WhenAnyValue(x => x.SelectedFilter)
                .DistinctUntilChanged()
                .Select(filterEnum =>
                {
                    // Return a predicate function (Func<Series, bool>)
                    return (Func<Series, bool>)(series =>
                    {
                        return filterEnum switch
                        {
                            // Status Filters
                            TsundokuFilter.Ongoing => series.Status == Status.Ongoing,
                            TsundokuFilter.Finished => series.Status == Status.Finished,
                            TsundokuFilter.Hiatus => series.Status == Status.Hiatus,
                            TsundokuFilter.Cancelled => series.Status == Status.Cancelled,

                            // Collection Completion Filters
                            TsundokuFilter.Complete => series.CurVolumeCount == series.MaxVolumeCount,
                            TsundokuFilter.Incomplete => series.CurVolumeCount != series.MaxVolumeCount,

                            // Favorites Filter
                            TsundokuFilter.Favorites => series.IsFavorite,

                            // Format Filters
                            TsundokuFilter.Manga => series.Format == Format.Manga,
                            TsundokuFilter.Manfra => series.Format == Format.Manfra,
                            TsundokuFilter.Manhwa => series.Format == Format.Manhwa,
                            TsundokuFilter.Manhua => series.Format == Format.Manhua,
                            TsundokuFilter.Comic => series.Format == Format.Comic, 
                            TsundokuFilter.Novel => series.Format == Format.Novel,

                            // Demographic Filters
                            TsundokuFilter.Shounen => series.Demographic == Demographic.Shounen,
                            TsundokuFilter.Shoujo => series.Demographic == Demographic.Shoujo,
                            TsundokuFilter.Seinen => series.Demographic == Demographic.Seinen,
                            TsundokuFilter.Josei => series.Demographic == Demographic.Josei,

                            // Read Status Filters
                            TsundokuFilter.Read => series.VolumesRead > 0,
                            TsundokuFilter.Unread => series.VolumesRead == 0,

                            // Genre Filters
                            TsundokuFilter.Action => series.Genres.Contains(Genre.Action),
                            TsundokuFilter.Adventure => series.Genres.Contains(Genre.Adventure),
                            TsundokuFilter.Comedy => series.Genres.Contains(Genre.Comedy),
                            TsundokuFilter.Drama => series.Genres.Contains(Genre.Drama),
                            TsundokuFilter.Ecchi => series.Genres.Contains(Genre.Ecchi),
                            TsundokuFilter.Fantasy => series.Genres.Contains(Genre.Fantasy),
                            TsundokuFilter.Horror => series.Genres.Contains(Genre.Horror),
                            TsundokuFilter.MahouShoujo => series.Genres.Contains(Genre.MahouShoujo),
                            TsundokuFilter.Mecha => series.Genres.Contains(Genre.Mecha),
                            TsundokuFilter.Music => series.Genres.Contains(Genre.Music),
                            TsundokuFilter.Mystery => series.Genres.Contains(Genre.Mystery),
                            TsundokuFilter.Psychological => series.Genres.Contains(Genre.Psychological),
                            TsundokuFilter.Romance => series.Genres.Contains(Genre.Romance),
                            TsundokuFilter.SciFi => series.Genres.Contains(Genre.SciFi),
                            TsundokuFilter.SliceOfLife => series.Genres.Contains(Genre.SliceOfLife),
                            TsundokuFilter.Sports => series.Genres.Contains(Genre.Sports),
                            TsundokuFilter.Supernatural => series.Genres.Contains(Genre.Supernatural),
                            TsundokuFilter.Thriller => series.Genres.Contains(Genre.Thriller),

                            // Default case: No filter applied
                            _ => true
                        };
                    });
                });

            // 3. Combine filter predicates into a single observable predicate
            var combinedFilter = Observable.CombineLatest(
                textFilter,
                tsundokuFilter,
                (textPredicate, tsundokuPredicate) =>
                    (Func<Series, bool>)(series => textPredicate(series) && tsundokuPredicate(series))
            );

            // 4. Define the series comparer (for sorting) based on the current user's language
            var seriesComparerChanged = _userService.CurrentUser
                .Where(user => user != null)
                .Select(user => user!.Language)
                .DistinctUntilChanged()
                .Select(curLang => (IComparer<Series>)new SeriesComparer(curLang));
            // .PublishReplay(1) // Ensures new subscribers immediately get the last emitted comparer
            // .RefCount();      // Ensures the observable stays active as long as there are subscribers

            // 5. Build the DynamicData pipeline
            _userService.UserCollectionChanges
                .Filter(combinedFilter)
                .SortAndBind(out _dynamicUserCollection, seriesComparerChanged)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe()
                .DisposeWith(_disposables);
        }

        /// <summary>
        /// Disposes of all active subscriptions managed by this provider.
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
            GC.SuppressFinalize(this); // Suppress finalization if Dispose is called explicitly
        }
    }
}