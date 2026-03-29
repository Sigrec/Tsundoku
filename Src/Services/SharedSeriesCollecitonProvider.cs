using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tsundoku.Helpers;
using Tsundoku.Models;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using static Tsundoku.Models.Enums.SeriesStatusModel;
using static Tsundoku.Models.Enums.TsundokuFilterModel;
using static Tsundoku.Models.Enums.TsundokuSortModel;

namespace Tsundoku.Services;

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
    TsundokuSort SelectedSort { get; set; }
    string AdvancedSearchQuery { get; set; }
    string AdvancedSearchQueryErrorMessage { get; set; }
}

/// <summary>
/// Provides a globally filtered and sorted ReadOnlyObservableCollection of Series
/// that can be consumed by multiple ViewModels.
/// </summary>
public sealed partial class SharedSeriesCollectionProvider : ReactiveObject, ISharedSeriesCollectionProvider, IDisposable
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly CompositeDisposable _disposables = [];
    private ReadOnlyObservableCollection<Series> _dynamicUserCollection;

    /// <summary>
    /// Gets the filtered and sorted collection of Series. ViewModels will bind to this property.
    /// </summary>
    public ReadOnlyObservableCollection<Series> DynamicUserCollection => _dynamicUserCollection;

    // These Reactive properties will now drive the global filtering.
    // ViewModels that want to control the global filter will bind to these properties.
    [Reactive] public partial string SeriesFilterText { get; set; } = string.Empty;
    [Reactive] public partial TsundokuFilter SelectedFilter { get; set; } = TsundokuFilter.None;
    [Reactive] public partial TsundokuSort SelectedSort { get; set; } = TsundokuSort.TitleAZ;
    [Reactive] public partial string AdvancedSearchQueryErrorMessage { get; set; }
    [Reactive] public partial string AdvancedSearchQuery { get; set; }

    [GeneratedRegex(@"(?<FilterName>\w+)(?<Operator>==|>=|<=|>|<)(?<FilterValue>\"".*?\""|\w+)?", RegexOptions.ExplicitCapture)] public static partial Regex AdvancedQueryRegex();

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
                        series.Titles.AsValueEnumerable().Any(t => t.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                        series.Staff.AsValueEnumerable().Any(t => t.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                        series.Publisher.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                });
            });

        // 2. Define the TsundokuFilter (enum-based status) filter predicate
        IObservable<Func<Series, bool>> tsundokuFilter = this.WhenAnyValue(x => x.SelectedFilter)
            .Where(filter => filter != TsundokuFilter.Query)
            .DistinctUntilChanged()
            .Select(filterEnum =>
            {
                // Return a predicate function (Func<Series, bool>)
                return (Func<Series, bool>)(series =>
                {
                    return filterEnum switch
                    {
                        // Status Filters
                        TsundokuFilter.Ongoing => series.Status == SeriesStatus.Ongoing,
                        TsundokuFilter.Finished => series.Status == SeriesStatus.Finished,
                        TsundokuFilter.Hiatus => series.Status == SeriesStatus.Hiatus,
                        TsundokuFilter.Cancelled => series.Status == SeriesStatus.Cancelled,

                        // Collection Completion Filters
                        TsundokuFilter.Complete => series.CurVolumeCount == series.MaxVolumeCount,
                        TsundokuFilter.Incomplete => series.CurVolumeCount != series.MaxVolumeCount,

                        // Favorites Filter
                        TsundokuFilter.Favorites => series.IsFavorite,

                        // Format Filters
                        TsundokuFilter.Manga => series.Format == SeriesFormat.Manga,
                        TsundokuFilter.Manfra => series.Format == SeriesFormat.Manfra,
                        TsundokuFilter.Manhwa => series.Format == SeriesFormat.Manhwa,
                        TsundokuFilter.Manhua => series.Format == SeriesFormat.Manhua,
                        TsundokuFilter.Comic => series.Format == SeriesFormat.Comic,
                        TsundokuFilter.Novel => series.Format == SeriesFormat.Novel,

                        // Demographic Filters
                        TsundokuFilter.Shounen => series.Demographic == SeriesDemographic.Shounen,
                        TsundokuFilter.Shoujo => series.Demographic == SeriesDemographic.Shoujo,
                        TsundokuFilter.Seinen => series.Demographic == SeriesDemographic.Seinen,
                        TsundokuFilter.Josei => series.Demographic == SeriesDemographic.Josei,

                        // Read Status Filters
                        TsundokuFilter.Read => series.VolumesRead > 0,
                        TsundokuFilter.Unread => series.VolumesRead == 0,

                        // Genre Filters
                        TsundokuFilter.Action => series.HasGenre(SeriesGenre.Action),
                        TsundokuFilter.Adventure => series.HasGenre(SeriesGenre.Adventure),
                        TsundokuFilter.Comedy => series.HasGenre(SeriesGenre.Comedy),
                        TsundokuFilter.Drama => series.HasGenre(SeriesGenre.Drama),
                        TsundokuFilter.Ecchi => series.HasGenre(SeriesGenre.Ecchi),
                        TsundokuFilter.Fantasy => series.HasGenre(SeriesGenre.Fantasy),
                        TsundokuFilter.Hentai => series.HasGenre(SeriesGenre.Hentai),
                        TsundokuFilter.Horror => series.HasGenre(SeriesGenre.Horror),
                        TsundokuFilter.MahouShoujo => series.HasGenre(SeriesGenre.MahouShoujo),
                        TsundokuFilter.Mecha => series.HasGenre(SeriesGenre.Mecha),
                        TsundokuFilter.Music => series.HasGenre(SeriesGenre.Music),
                        TsundokuFilter.Mystery => series.HasGenre(SeriesGenre.Mystery),
                        TsundokuFilter.Psychological => series.HasGenre(SeriesGenre.Psychological),
                        TsundokuFilter.Romance => series.HasGenre(SeriesGenre.Romance),
                        TsundokuFilter.SciFi => series.HasGenre(SeriesGenre.SciFi),
                        TsundokuFilter.SliceOfLife => series.HasGenre(SeriesGenre.SliceOfLife),
                        TsundokuFilter.Sports => series.HasGenre(SeriesGenre.Sports),
                        TsundokuFilter.Supernatural => series.HasGenre(SeriesGenre.Supernatural),
                        TsundokuFilter.Thriller => series.HasGenre(SeriesGenre.Thriller),

                        // Default case: No filter applied
                        _ => true
                    };
                });
            });

        IObservable<Func<Series, bool>> advancedFilter = this.WhenAnyValue(x => x.AdvancedSearchQuery)
            .DistinctUntilChanged()
            .Select(query =>
            {
                AdvancedSearchQueryErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(query))
                {
                    return (Func<Series, bool>)(series => true);
                }

                Func<Series, bool>? predicate = BuildAdvancedPredicate(query);
                if (predicate is null)
                {
                    AdvancedSearchQueryErrorMessage = "Incorrectly formatted advanced search query.";
                    LOGGER.Warn("Advanced search query could not be parsed: {Query}", query);
                    return series => true;
                }

                LOGGER.Info("Advanced Search Query applied: {Query}", query);
                return predicate;
            })
            .StartWith(series => true);

        // 3. Combine filter predicates into a single observable predicate
        IObservable<Func<Series, bool>> combinedFilter = Observable.CombineLatest(
            textFilter, tsundokuFilter, advancedFilter,
            (textSearchPred, tsundokuFilterPred, advancedSearchPred) =>
                (Func<Series, bool>)(series =>
                    (textSearchPred is null || textSearchPred(series)) &&
                    (tsundokuFilterPred is null || tsundokuFilterPred(series)) &&
                    (advancedSearchPred is null || advancedSearchPred(series))
                )
        );

        // 4. Define the series comparer (for sorting) based on the current user's language and selected sort
        IObservable<IComparer<Series>> seriesComparerChanged = Observable.CombineLatest(
                _userService.CurrentUser
                    .Where(user => user is not null)
                    .Select(user => user.Language)
                    .DistinctUntilChanged(),
                this.WhenAnyValue(x => x.SelectedSort)
                    .DistinctUntilChanged(),
                (curLang, sort) => (IComparer<Series>)new SeriesComparer(curLang, sort)
            )
            .Replay(1)
            .RefCount();

        // 5. Build the DynamicData pipeline
        _userService.UserCollectionChanges
            .Filter(series => series is not null)
            .Filter(combinedFilter)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .SortAndBind(out _dynamicUserCollection, seriesComparerChanged)
            .Subscribe()
            .DisposeWith(_disposables);
    }

    /// <summary>
    /// Numeric property accessors keyed by user-facing filter name (case-insensitive).
    /// </summary>
    private static readonly FrozenDictionary<string, Func<Series, decimal>> NumericAccessors =
        new Dictionary<string, Func<Series, decimal>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Rating"] = s => s.Rating,
            ["Value"] = s => s.Value,
            ["Read"] = s => s.VolumesRead,
            ["CurVolumes"] = s => s.CurVolumeCount,
            ["MaxVolumes"] = s => s.MaxVolumeCount,
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parses the advanced search query and builds a composed predicate directly.
    /// Supports <c>&amp;</c> (AND) and <c>|</c> (OR) connectors between filter tokens.
    /// </summary>
    private static Func<Series, bool>? BuildAdvancedPredicate(string query)
    {
        query = query.Trim();
        MatchCollection matches = AdvancedQueryRegex().Matches(query);

        if (matches.Count == 0)
        {
            return null;
        }

        Func<Series, bool>? result = null;

        for (int i = 0; i < matches.Count; i++)
        {
            Match match = matches[i];
            string filterName = match.Groups["FilterName"].Value;
            string op = match.Groups["Operator"].Value;
            string filterValue = match.Groups["FilterValue"].Success ? match.Groups["FilterValue"].Value : string.Empty;

            Func<Series, bool>? predicate = BuildSinglePredicate(filterName, op, filterValue);
            if (predicate is null)
            {
                return null;
            }

            if (result is null)
            {
                result = predicate;
            }
            else
            {
                // Determine connector by inspecting text between previous and current match
                int prevEnd = matches[i - 1].Index + matches[i - 1].Length;
                string between = query[prevEnd..match.Index];
                bool isOr = between.Contains('|');

                Func<Series, bool> left = result;
                Func<Series, bool> right = predicate;
                result = isOr
                    ? s => left(s) || right(s)
                    : s => left(s) && right(s);
            }
        }

        return result;
    }

    /// <summary>
    /// Builds a single <c>Func&lt;Series, bool&gt;</c> predicate for one filter token.
    /// </summary>
    private static Func<Series, bool>? BuildSinglePredicate(string filterName, string op, string filterValue)
    {
        // Numeric filters (Rating, Value, Read, CurVolumes, MaxVolumes)
        if (NumericAccessors.TryGetValue(filterName, out Func<Series, decimal>? accessor))
        {
            if (!decimal.TryParse(filterValue, out decimal target))
            {
                return null;
            }
            return op switch
            {
                "==" => s => accessor(s) == target,
                ">=" => s => accessor(s) >= target,
                "<=" => s => accessor(s) <= target,
                ">" => s => accessor(s) > target,
                "<" => s => accessor(s) < target,
                _ => null,
            };
        }

        return filterName.ToLowerInvariant() switch
        {
            "format" => TryParseEnum<SeriesFormat>(filterValue, out SeriesFormat fmt)
                ? BuildEnumPredicate(s => s.Format, fmt, op)
                : null,

            "status" => TryParseEnum<SeriesStatus>(filterValue, out SeriesStatus status)
                ? BuildEnumPredicate(s => s.Status, status, op)
                : null,

            "demographic" => TryParseEnum<SeriesDemographic>(filterValue, out SeriesDemographic demo)
                ? BuildEnumPredicate(s => s.Demographic, demo, op)
                : null,

            "series" => filterValue.Equals("Complete", StringComparison.OrdinalIgnoreCase)
                    ? s => s.CurVolumeCount > 0 && s.CurVolumeCount == s.MaxVolumeCount
                : filterValue.Equals("InComplete", StringComparison.OrdinalIgnoreCase)
                    ? s => (s.MaxVolumeCount > 0 && s.CurVolumeCount < s.MaxVolumeCount) || s.MaxVolumeCount == 0
                : null,

            "favorite" => filterValue.Equals("True", StringComparison.OrdinalIgnoreCase)
                ? s => s.IsFavorite
                : s => !s.IsFavorite,

            "notes" => BuildStringContainsPredicate(s => s.SeriesNotes, filterValue),
            "publisher" => BuildStringContainsPredicate(s => s.Publisher, filterValue),

            "genre" => filterValue.TryGetEnumValueFromMemberValue<SeriesGenre>(out SeriesGenre genre)
                ? s => s.Genres is not null && s.Genres.Contains(genre)
                : null,

            _ => null,
        };
    }

    private static bool TryParseEnum<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
    {
        return Enum.TryParse(value, true, out result);
    }

    private static Func<Series, bool>? BuildEnumPredicate<TEnum>(Func<Series, TEnum> accessor, TEnum target, string op)
        where TEnum : struct, Enum
    {
        return op switch
        {
            "==" => s => EqualityComparer<TEnum>.Default.Equals(accessor(s), target),
            _ => null,
        };
    }

    private static Func<Series, bool> BuildStringContainsPredicate(Func<Series, string?> accessor, string filterValue)
    {
        string searchValue = filterValue.StartsWith('"') && filterValue.EndsWith('"') && filterValue.Length > 1
            ? filterValue[1..^1]
            : filterValue;

        return s =>
        {
            string? val = accessor(s);
            return !string.IsNullOrWhiteSpace(val) && val.Contains(searchValue, StringComparison.OrdinalIgnoreCase);
        };
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