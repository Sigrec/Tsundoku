using System.Collections.ObjectModel;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using static Tsundoku.Models.Enums.SeriesStatusModel;
using static Tsundoku.Models.Enums.TsundokuFilterModel;

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
    [Reactive] public string SeriesFilterText { get; set; } = string.Empty;
    [Reactive] public TsundokuFilter SelectedFilter { get; set; } = TsundokuFilter.None;
    [Reactive] public string AdvancedSearchQueryErrorMessage { get; set; }
    [Reactive] public string AdvancedSearchQuery { get; set; }

    [GeneratedRegex(@"(?<FilterName>\w+)(?<Operator>==|>=|<=|>|<)(?<FilterValue>\"".*?\""|\w+)?", RegexOptions.ExplicitCapture | RegexOptions.Compiled)] public static partial Regex AdvancedQueryRegex();

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
            .SelectMany(async query =>
            {
                // Clear previous error message
                AdvancedSearchQueryErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(query))
                {
                    return series => true; // No advanced filter
                }

                // Attempt to parse the query string and build the Dynamic LINQ expression
                string dynamicLinqString = await GenerateDynamicLinqExpression(query);

                if (string.IsNullOrWhiteSpace(dynamicLinqString))
                {
                    AdvancedSearchQueryErrorMessage = "Incorrectly formatted advanced search query.";
                    LOGGER.Warn("Advanced search query parsing resulted in an empty/invalid LINQ expression for: {Query}", query);
                    return series => true;
                }

                try
                {
                    LambdaExpression predicate = DynamicExpressionParser.ParseLambda<Series, bool>(ParsingConfig.Default, true, dynamicLinqString);
                    Func<Series, bool> compiledPredicate = (Func<Series, bool>)predicate.Compile();

                    LOGGER.Info("Advanced Search Query => {query}", dynamicLinqString.Replace("it.", "series."));
                    return compiledPredicate;
                }
                catch (Exception ex)
                {
                    AdvancedSearchQueryErrorMessage = "Invalid advanced search syntax.";
                    LOGGER.Error(ex, "Error compiling dynamic LINQ expression for query: {Query}, Expression: {Expression}", query, dynamicLinqString);
                    return series => true;
                }
            })
            .StartWith(s => true); // Start with no filter

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

        // 4. Define the series comparer (for sorting) based on the current user's language
        IObservable<IComparer<Series>> seriesComparerChanged = _userService.CurrentUser
            .Where(user => user is not null)
            .Select(user => user.Language)
            .DistinctUntilChanged()
            .Select(curLang => (IComparer<Series>)new SeriesComparer(curLang));
        // .PublishReplay(1) // Ensures new subscribers immediately get the last emitted comparer
        // .RefCount();      // Ensures the observable stays active as long as there are subscribers

        // 5. Build the DynamicData pipeline
        _userService.UserCollectionChanges
            .Filter(series => series is not null)
            .Filter(combinedFilter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out _dynamicUserCollection, seriesComparerChanged)
            .Subscribe()
            .DisposeWith(_disposables);
    }

    private static async Task<string> GenerateDynamicLinqExpression(string advancedSearchQuery)
    {
        // This method needs to be called from a context where it's safe to run parsing logic.
        // Wrapping it in Task.Run to ensure it doesn't block the UI thread during parsing.
        return await Task.Run(() =>
        {
            advancedSearchQuery = advancedSearchQuery.Trim();
            MatchCollection advancedSearchMatches = AdvancedQueryRegex().Matches(advancedSearchQuery);

            if (advancedSearchMatches.Count == 0)
            {
                return string.Empty; // No valid filter parts
            }

            StringBuilder advancedFilterExpression = new StringBuilder();

            bool firstFilter = true;
            foreach (Match searchFilter in advancedSearchMatches)
            {
                string filterName = searchFilter.Groups["FilterName"].Value;
                string operatorType = searchFilter.Groups["Operator"].Value;
                string filterValue = searchFilter.Groups["FilterValue"].Success ? searchFilter.Groups["FilterValue"].Value : string.Empty;

                if (!firstFilter)
                {
                    advancedFilterExpression.Append(" && ");
                }
                firstFilter = false;

                string parsedFilter = ParseAdvancedFilter(filterName, operatorType, filterValue);

                if (string.IsNullOrWhiteSpace(parsedFilter))
                {
                    // If parsing failed for any part, return empty string to signal an error
                    return string.Empty;
                }

                advancedFilterExpression.Append($"({parsedFilter})");
            }

            return advancedFilterExpression.ToString();
        });
    }


    // --- Your existing ParseAdvancedFilter method ---
    private static string ParseAdvancedFilter(string filterName, string operatorType, string filterValue)
    {
        operatorType = operatorType.Equals("=") ? "==" : operatorType;

        static string BuildStringContainsExpression(string propertyName, string val)
        {
            // Extract the actual search value, removing outer quotes if present.
            // This logic correctly handles user input like "Notes=="My note""
            string searchValue = val.StartsWith('"') && val.EndsWith('"') && val.Length > 1
                                 ? val[1..^1]
                                 : val;

            // Escape the search value for safe inclusion in the C# string literal.
            string escapedSearchValue = EscapeForCSharpStringLiteral(searchValue);

            // Construct the dynamic LINQ expression.
            // Use ToLowerInvariant() on both sides for case-insensitive comparison,
            // as Dynamic LINQ does not directly support StringComparison.OrdinalIgnoreCase.
            // The !string.IsNullOrWhiteSpace check ensures the property is not null/empty before ToLowerInvariant().
            return $"!string.IsNullOrWhiteSpace(it.{propertyName}) && it.{propertyName}.ToLowerInvariant().Contains(\"{escapedSearchValue.ToLowerInvariant()}\")";
        }

        return filterName.ToLowerInvariant() switch
        {
            "rating" or "value" or "read" or "curvolumes" or "maxvolumes" =>
                int.TryParse(filterValue, out _) ? $"it.{filterName} {operatorType} {filterValue}" : string.Empty,

            "format" =>
                $"(it.Format {operatorType} Format.{filterValue})",

            "status" =>
                $"(it.Status {operatorType} SeriesStatus.{filterValue})",

            "demographic" =>
                $"(it.Demographic {operatorType} Demographic.{filterValue})",

            "series" =>
                filterValue.Equals("Complete", StringComparison.OrdinalIgnoreCase) ? $"it.CurVolumeCount > 0 && it.CurVolumeCount == it.MaxVolumeCount" :
                filterValue.Equals("InComplete", StringComparison.OrdinalIgnoreCase) ? $"it.MaxVolumeCount > 0 && it.CurVolumeCount < it.MaxVolumeCount || it.MaxVolumeCount == 0" :
                string.Empty,

            "favorite" => $"{(filterValue.Equals("True") ? '!' : "")}it.IsFavorite",

            "notes" => BuildStringContainsExpression("SeriesNotes", filterValue),
            "publisher" => BuildStringContainsExpression("Publisher", filterValue),

            "genre" =>
                // Assuming Series.Genres is List<string> and filterValue is the actual genre string (e.g., "Action")
                // If Genre is an enum in your project, change this to:
                // `it.Genres is not null && it.Genres.Contains(YourProject.Models.GenreEnum.{filterValue})`
                $"it.Genres is not null && it.Genres.Contains(Genre.{EscapeForCSharpStringLiteral(filterValue)}\")",

            _ => string.Empty,
        };
    }

    private static string EscapeForCSharpStringLiteral(string value)
    {
        // Replace backslashes first, then quotes
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
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