using GraphQL;
using GraphQL.Client.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Tsundoku.Models;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;

namespace Tsundoku.Clients;

/// <summary>
/// Client for querying the AniList GraphQL API for manga and light novel metadata.
/// </summary>
public sealed partial class AniList
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly AniListGraphQLClient _aniListClient;

    /// <summary>Extra seconds added to rate limit delays to avoid hitting the limit again immediately.</summary>
    private const int RateLimitPaddingSeconds = 5;

    /// <summary>Length of the AniList rate limit window, used when the reset header is missing or implausible.</summary>
    private const int RateLimitWindowSeconds = 60;

    /// <summary>UTC ticks before which no request should be sent because the rate limit budget is used up.</summary>
    private long _rateLimitResumeUtcTicks;

    /// <summary>Base multiplier for exponential backoff when a 429 response includes headers.</summary>
    private const int BackoffBaseWithHeaders = 5;

    /// <summary>Base multiplier for exponential backoff when a 429 response has no headers (longer fallback).</summary>
    private const int BackoffBaseWithoutHeaders = 10;

    /// <summary>Number of staff entries requested per page from the AniList API.</summary>
    private const int StaffPerPage = 25;

    /// <summary>Separator used between staff names in the formatted output.</summary>
    private const string StaffSeparator = " | ";

    /// <summary>Length of the staff separator, used when trimming trailing separators.</summary>
    private static readonly int StaffSeparatorLength = StaffSeparator.Length;

    [GeneratedRegex(@"(?is)<i>Note[s]?:.*?</i>|<br\s*/?>|\(Source:.*?\)", RegexOptions.IgnoreCase)] private static partial Regex AniListDescRegex();
    [GeneratedRegex(@"(?:\s*\n)?(?:<br\s*/?>\s*){2,}(?:\n\s*)?", RegexOptions.IgnoreCase)] private static partial Regex AniListBreakRegex();
    [GeneratedRegex(@"[ \t]*\r?\n(?:[ \t]*\r?\n){2,}")] private static partial Regex ExcessNewlinesRegex();
    [GeneratedRegex(@" \(.*\)")] private static partial Regex StaffRegex();
    [GeneratedRegex(@"</?[^>]+>")] private static partial Regex HtmlTagStripRegex();
    [GeneratedRegex("[“”‘’]")] private static partial Regex SmartQuotesRegex();

    private static readonly string[] VALID_STAFF_ROLES = ["Story & Art", "Story", "Art", "Original Creator", "Character Design", "Cover Illustration", "Illustration", "Mechanical Design", "Original Story", "Original Character Design", "Original Story", "Supervisor"];

    /// <summary>
    /// Initializes a new instance of <see cref="AniList"/> with the specified GraphQL client.
    /// </summary>
    /// <param name="aniListClient">The AniList GraphQL HTTP client.</param>
    public AniList(AniListGraphQLClient aniListClient) => _aniListClient = aniListClient;

    /// <summary>
    /// Retrieves AniList series information by its title.
    /// </summary>
    /// <param name="title">The title of the series (can include synonyms).</param>
    /// <param name="format">The media format (e.g., manga or light novel).</param>
    /// <param name="pageNum">The staff pagination page number.</param>
    /// <returns>A <see cref="JsonDocument"/> containing the series data, or null if the request fails.</returns>
    public async Task<JsonDocument?> GetSeriesByTitleAsync(string title, SeriesFormat format, int pageNum)
    {
        object variables = new
        {
            title,
            format,
            pageNum
        };

        return await ExecuteAniListQueryAsync("Title", variables, $"Title: {title}");
    }

    /// <summary>
    /// Retrieves AniList series information by its AniList ID.
    /// </summary>
    /// <param name="seriesId">The unique AniList ID of the series.</param>
    /// <param name="format">The media format (e.g., manga or light novel).</param>
    /// <param name="pageNum">The staff pagination page number.</param>
    /// <returns>A <see cref="JsonDocument"/> containing the series data, or null if the request fails.</returns>
    public async Task<JsonDocument?> GetSeriesByIDAsync(int seriesId, SeriesFormat format, int pageNum)
    {
        object variables = new
        {
            seriesId,
            format,
            pageNum
        };

        return await ExecuteAniListQueryAsync("Id", variables, $"SeriesId: {seriesId}");
    }

    /// <summary>
    /// Retrieves autocomplete suggestions for the AniList series title picker.
    /// </summary>
    /// <param name="search">The search text to query.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of matching suggestions sorted by relevance.</returns>
    public async Task<IReadOnlyList<AniListPickerSuggestion>> GetPickerSuggestionsAsync(
        string search,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return [];
        }

        const int perPage = 1000;

        GraphQLRequest request = new()
        {
            Query = $@"
            query ($search: String!, $perPage: Int!) {{
                Page(perPage: $perPage) {{
                    media(search: $search, type: MANGA, format_in: [MANGA, NOVEL], sort: [SEARCH_MATCH, POPULARITY_DESC]) {{
                        id
                        format
                        title {{ romaji english native }}
                    }}
                }}
            }}",
            Variables = new { search, perPage }
        };

        GraphQLResponse<JsonDocument?>? response = await SendWithRateLimitRetryAsync<JsonDocument?>(
            request,
            $"Picker Typeahead: '{search}'",
            cancellationToken: cancellationToken);

        if (response?.Data is null)
        {
            return [];
        }

        if (!response.Data.RootElement.TryGetProperty("Page", out JsonElement page) ||
            !page.TryGetProperty("media", out JsonElement media) ||
            media.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        static int MatchScore(string candidate, string needle)
        {
            if (candidate.Equals(needle, StringComparison.OrdinalIgnoreCase))
            {
                return 3; // Exact match — highest quality
            }
            if (candidate.StartsWith(needle, StringComparison.OrdinalIgnoreCase))
            {
                return 2; // Prefix match — high quality
            }
            if (candidate.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return 1; // Substring match — partial quality
            }
            return 0; // No match
        }

        List<AniListPickerSuggestion> list = new(Math.Min(media.GetArrayLength(), perPage));
        string needle = search.Trim();

        foreach (JsonElement m in media.EnumerateArray())
        {
            if (!m.TryGetProperty("id", out JsonElement idEl)) continue;

            string? romaji = m.GetProperty("title").GetProperty("romaji").GetString();
            if (romaji is null) continue;

            string? english = m.GetProperty("title").GetProperty("english").GetString();
            string? native = m.GetProperty("title").GetProperty("native").GetString();

            bool isRomajiNotEmpty = !string.IsNullOrWhiteSpace(romaji);
            bool isEnglishNotEmpty = !string.IsNullOrWhiteSpace(english);
            bool isNativeNotEmpty = !string.IsNullOrWhiteSpace(native);

            bool inRomaji = isRomajiNotEmpty && romaji.Contains(needle, StringComparison.OrdinalIgnoreCase);
            bool inEnglish = isEnglishNotEmpty && english!.Contains(needle, StringComparison.OrdinalIgnoreCase);
            bool inNative = isNativeNotEmpty && native!.Contains(needle, StringComparison.OrdinalIgnoreCase);
            if (!inRomaji && !inEnglish && !inNative) continue;

            // Find best matching title without allocating a list per iteration
            string chosen = romaji;
            int best = isRomajiNotEmpty ? MatchScore(romaji, needle) : 0;
            if (best < 3 && isEnglishNotEmpty)
            {
                int score = MatchScore(english!, needle);
                if (score > best) { best = score; chosen = english!; }
            }
            if (best < 3 && isNativeNotEmpty)
            {
                int score = MatchScore(native!, needle);
                if (score > best) { chosen = native!; }
            }

            string? format = m.TryGetProperty("format", out JsonElement f) && f.ValueKind == JsonValueKind.String ? f.GetString()!.Trim() : null;
            int id = idEl.GetInt32();
            string display = $"{chosen} ({format ?? "MEDIA"}) [{id}]";

            list.Add(new AniListPickerSuggestion(id.ToString(), display, format ?? string.Empty));
        }

        return list;
    }

    /// <summary>
    /// Executes a GraphQL query against the AniList API using the given media selector and variables.
    /// Includes automatic retry handling if rate-limited.
    /// </summary>
    /// <param name="queryType">The name of the predefined query to run (e.g., "Title").</param>
    /// <param name="variables">An anonymous object containing GraphQL variables.</param>
    /// <param name="contextLabel">A descriptive label used for logging purposes.</param>
    /// <param name="maxRetries">The maximum number of attempts before giving up.</param>
    /// <param name="cancellationToken">Token used to cancel the request early.</param>
    /// <returns>A <see cref="JsonDocument"/> containing the query response, or null if the request fails or is canceled.</returns>
    private async Task<JsonDocument?> ExecuteAniListQueryAsync(
        string queryType,
        object variables,
        string contextLabel,
        ushort maxRetries = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            LOGGER.Info("Getting AniList Data for {Context}...", contextLabel);

            string field;
            string variableDeclarations;
            if (queryType.Equals("Title", StringComparison.OrdinalIgnoreCase))
            {
                field = "Media(search: $title, format: $format, sort: [SEARCH_MATCH, POPULARITY_DESC])";
                variableDeclarations = "($title: String!, $format: MediaFormat!, $pageNum: Int!)";
            }
            else
            {
                field = "Media(id: $seriesId, format: $format, sort: [SEARCH_MATCH, POPULARITY_DESC])";
                variableDeclarations = "($seriesId: Int!, $format: MediaFormat!, $pageNum: Int!)";
            }

            GraphQLRequest queryRequest = new()
            {
                Query = @$"
                query {variableDeclarations} {{
                    {field} {{
                        id
                        countryOfOrigin
                        title {{
                            romaji
                            english
                            native
                        }}
                        staff(sort: RELEVANCE, perPage: {StaffPerPage}, page: $pageNum) {{
                            pageInfo {{
                                hasNextPage
                            }}
                            edges {{
                                role
                                node {{
                                    name {{
                                        full
                                        native
                                        alternative
                                    }}
                                }}
                            }}
                        }}
                        genres
                        description(asHtml: false)
                        status(version: 2)
                        siteUrl
                        coverImage {{
                            extraLarge
                        }}
                    }}
                }}",
                Variables = variables
            };

            GraphQLResponse<JsonDocument?>? response = await SendWithRateLimitRetryAsync<JsonDocument?>(
                queryRequest,
                contextLabel,
                maxRetries,
                cancellationToken
            );

            if (response is null)
            {
                LOGGER.Warn("AniList query for {Context} returned null", contextLabel);
                return null;
            }

            LOGGER.Debug("Finished getting AniList Data for {Context}", contextLabel);

            return response.Data;
        }
        catch (Exception ex)
        {
            LOGGER.Error("AniList query for {Context} failed -> {Error}", contextLabel, ex.Message);
            return null;
        }
    }
    


    private async Task<GraphQLResponse<JsonDocument?>?> SendWithRateLimitRetryAsync<T>(
        GraphQLRequest request,
        string context,
        ushort maxRetries = 5,
        CancellationToken cancellationToken = default)
    {
        int attempts = 0;

        while (attempts < maxRetries)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                LOGGER.Info("Request canceled before sending GraphQL query.");
                return null;
            }

            try
            {
                TimeSpan pendingWait = new(Interlocked.Read(ref _rateLimitResumeUtcTicks) - DateTime.UtcNow.Ticks);
                if (pendingWait > TimeSpan.Zero)
                {
                    LOGGER.Info("AniList rate limit budget exhausted. Waiting {Delay} before sending {Context}.", pendingWait, context);
                    await Task.Delay(pendingWait, cancellationToken);
                }

                GraphQLResponse<JsonDocument?> response = await _aniListClient
                    .SendQueryAsync<JsonDocument?>(request, cancellationToken);

                RecordRateLimitWindow(response.AsGraphQLHttpResponse().ResponseHeaders);
                return response;
            }
            catch (GraphQLHttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                attempts++;
                TimeSpan delay;

                // Access headers directly from graphqlEx.ResponseHeaders
                if (ex.ResponseHeaders is not null)
                {
                    TimeSpan? headersDelay = GetRateLimitDelay(ex.ResponseHeaders, RateLimitPaddingSeconds);
                    if (headersDelay.HasValue)
                    {
                        delay = headersDelay.Value;
                        LOGGER.Warn("HTTP 429 Too Many Requests (attempt {Attempt}/{Max}). Delaying for {Delay} based on 'Retry-After'.", attempts, maxRetries, delay.TotalSeconds);
                    }
                    else
                    {
                        // Fallback if Retry-After is unexpectedly missing from GraphQLHttpRequestException's response headers
                        delay = TimeSpan.FromSeconds(Math.Pow(2, attempts) * BackoffBaseWithHeaders + RateLimitPaddingSeconds);
                        LOGGER.Warn("HTTP 429 Too Many Requests (attempt {Attempt}/{Max}). 'Retry-After' header not found in GraphQLHttpRequestException.ResponseHeaders Retrying after fallback delay of {FallbackDelay}.", attempts, maxRetries, delay.TotalSeconds);
                    }
                }
                else
                {
                    // Fallback if GraphQLHttpRequestException's ResponseHeaders are null (very unlikely for a 429 response)
                    delay = TimeSpan.FromSeconds(Math.Pow(2, attempts) * BackoffBaseWithoutHeaders + RateLimitPaddingSeconds); // Longer fallback
                    LOGGER.Warn("HTTP 429 Too Many Requests (attempt {Attempt}/{Max}). GraphQLHttpRequestException did not contain ResponseHeaders. Retrying after fallback delay of {FallbackDelay}.", attempts, maxRetries, delay.TotalSeconds);
                }

                await Task.Delay(delay, cancellationToken);
                continue;
            }
            catch (GraphQLHttpRequestException ex) when (ex.StatusCode == HttpStatusCode.InternalServerError)
            {
                attempts++;
                LOGGER.Warn("AniList returned 500 for {Context} (attempt {Attempt}/{Max}). Retrying after {Seconds}s...", context, attempts, maxRetries, RateLimitPaddingSeconds);
                await Task.Delay(TimeSpan.FromSeconds(RateLimitPaddingSeconds), cancellationToken);
                continue;
            }
            catch (GraphQLHttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                LOGGER.Warn("{Series} not found", context);
                return null;
            }
            catch (TaskCanceledException)
            {
                LOGGER.Debug("Request canceled during send or delay.");
                return null;
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Unexpected error during GraphQL request -> ");
                return null;
            }
        }

        LOGGER.Error("Exceeded maximum retry attempts ({MaxRetries}) due to rate limiting.");
        return null;
    }

    /// <summary>
    /// Records when the next request may be sent if a successful response used up the rate limit budget.
    /// </summary>
    /// <param name="headers">HTTP response headers of a successful response.</param>
    private void RecordRateLimitWindow(HttpResponseHeaders headers)
    {
        if (!headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? remainingValues) ||
            !short.TryParse(remainingValues.FirstOrDefault(), out short remaining))
        {
            return;
        }

        LOGGER.Debug("AniList Rate Remaining = {Remaining}", remaining);
        if (remaining > 0)
        {
            return;
        }

        DateTime nowUtc = DateTime.UtcNow;
        TimeSpan delay = TimeSpan.FromSeconds(RateLimitWindowSeconds);
        if (headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? resetValues) &&
            long.TryParse(resetValues.FirstOrDefault(), out long resetUnixSeconds))
        {
            TimeSpan untilReset = DateTimeOffset.FromUnixTimeSeconds(resetUnixSeconds).UtcDateTime - nowUtc;
            if (untilReset > TimeSpan.Zero && untilReset < delay)
            {
                delay = untilReset;
            }
        }

        delay += TimeSpan.FromSeconds(RateLimitPaddingSeconds);
        Interlocked.Exchange(ref _rateLimitResumeUtcTicks, (nowUtc + delay).Ticks);
        LOGGER.Warn("AniList rate limit budget used up. Next request held for {Delay}.", delay);
    }

    /// <summary>
    /// Parses the AniList rate limit headers and returns the retry delay if rate-limited.
    /// </summary>
    /// <param name="headers">HTTP response headers.</param>
    /// <param name="paddingSeconds">Extra seconds added to the delay reported by the headers.</param>
    /// <returns>
    /// A <see cref="TimeSpan"/> representing the delay until the rate limit resets,
    /// or <c>null</c> if no delay is needed.
    /// </returns>
    private static TimeSpan? GetRateLimitDelay(HttpResponseHeaders headers, int paddingSeconds = RateLimitPaddingSeconds)
    {
        if (headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? remainingValues) &&
            short.TryParse(remainingValues.FirstOrDefault(), out short remaining))
        {
            LOGGER.Debug("AniList Rate Remaining = {Remaining}", remaining);

            if (remaining > 0)
            {
                return null;
            }
        }
        else
        {
            LOGGER.Warn("AniList response missing or invalid 'X-RateLimit-Remaining' header.");
        }

        if (headers.TryGetValues("Retry-After", out IEnumerable<string>? retryAfterValues) &&
            short.TryParse(retryAfterValues.FirstOrDefault(), out short delaySeconds))
        {
            LOGGER.Warn("AniList rate limit reached. Delaying for {Seconds} seconds.", delaySeconds + paddingSeconds);
            return TimeSpan.FromSeconds(delaySeconds + paddingSeconds);
        }

        LOGGER.Warn("AniList response missing or invalid 'Retry-After' header. Defaulting to 90s.");
        return TimeSpan.FromSeconds(90); // fallback to prevent hammering the API
    }

    /// <summary>
    /// Cleans and formats an AniList series description by:
    /// - Replacing line breaks and redundant HTML tags
    /// - Removing source and note metadata via regex
    /// - Decoding HTML entities (e.g., &amp;, &quot;)
    /// </summary>
    /// <param name="seriesDescription">The raw description string from AniList API.</param>
    /// <returns>A cleaned and readable plain-text description.</returns>
    public static string ParseSeriesDescription(string seriesDescription)
    {
        if (string.IsNullOrWhiteSpace(seriesDescription))
            return string.Empty;

        string cleaned = seriesDescription;
        // 1. Normalize <br> sequences to \n\n (before decoding)
        if (seriesDescription.Contains("<br", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = AniListBreakRegex().Replace(seriesDescription, "\n\n");
        }

        // 2. Remove known notes and sources
        if (seriesDescription.Contains("Note", StringComparison.OrdinalIgnoreCase) || seriesDescription.Contains("(Source:", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = AniListDescRegex().Replace(cleaned, string.Empty);
        }

        // 3. Strip leftover HTML tags (like <i>, <b>, but not escaped ones)
        cleaned = HtmlTagStripRegex().Replace(cleaned, string.Empty);

        // 4. Decode HTML entities (now safe, as no tags remain)
        cleaned = System.Web.HttpUtility.HtmlDecode(cleaned);

        // 5. Normalize smart quotes
        cleaned = SmartQuotesRegex().Replace(cleaned, m => m.Value switch
        {
            """ or """ => "\"",
            "‘" or "’" => "'",
            _ => m.Value
        });

        // 6. Collapse 3+ consecutive newlines (with optional trailing/leading whitespace)
        //    down to a single blank line so AniList's stacked \n\n\n runs don't render
        //    as huge vertical gaps.
        if (cleaned.Contains("\n\n\n", StringComparison.Ordinal))
        {
            cleaned = ExcessNewlinesRegex().Replace(cleaned, "\n\n");
        }

        return cleaned.Trim();
    }

    /// <summary>
    /// Parses a JSON array of genre strings into a set of strongly typed <see cref="SeriesGenre"/> values.
    /// If no genres are present or the value is null, logs a debug message and returns an empty set.
    /// </summary>
    /// <param name="romajiTitle">The Romaji title of the series, used for logging.</param>
    /// <param name="genres">The JSON element representing an array of genre strings.</param>
    /// <returns>A <see cref="HashSet{Genre}"/> containing all recognized genres, or an empty set if none are valid.</returns>
    public static HashSet<SeriesGenre> ParseGenreArray(string romajiTitle, JsonElement genres)
    {
        if (genres.ValueKind != JsonValueKind.Array || genres.GetArrayLength() == 0)
        {
            LOGGER.Info("No genre(s) returned for {Title} from AniList", romajiTitle);
            return [];
        }

        int estimatedCapacity = genres.GetArrayLength();
        HashSet<SeriesGenre> newGenres = new(estimatedCapacity);

        foreach (JsonElement element in genres.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String && TryParse(element.GetString()!, out SeriesGenre genre))
            {
                newGenres.Add(genre);
            }
        }

        return newGenres;
    }

    /// <summary>
    /// Extracts the Romaji, English, and Native (Japanese) titles from the provided AniList series data.
    /// Adds them to the specified <paramref name="newTitles"/> dictionary under the keys "Romaji", "English", and "Japanese".
    /// Logs the extracted titles at the debug level.
    /// </summary>
    /// <param name="seriesData">A <see cref="JsonElement"/> containing the AniList series data with a "title" property.</param>
    /// <param name="newTitles">A reference to a dictionary where the extracted titles will be stored.</param>
    public static void ExtractTitlesFromAniList(JsonElement seriesData, ref Dictionary<string, string> newTitles)
    {
        if (seriesData.TryGetProperty("title", out JsonElement titleElement) && titleElement.ValueKind == JsonValueKind.Object)
        {
            if (titleElement.TryGetProperty("romaji", out JsonElement romajiElement) && romajiElement.ValueKind == JsonValueKind.String)
            {
                string romajiTitle = romajiElement.GetString()!;
                newTitles["Romaji"] = romajiTitle;
                LOGGER.Debug("Added AniList romaji title: {Title}", romajiTitle);
            }

            if (titleElement.TryGetProperty("english", out JsonElement englishElement) && englishElement.ValueKind == JsonValueKind.String)
            {
                string englishTitle = englishElement.GetString()!;
                newTitles["English"] = englishTitle;
                LOGGER.Debug("Added AniList English title: {Title}", englishTitle);
            }

            if (titleElement.TryGetProperty("native", out JsonElement nativeElement) && nativeElement.ValueKind == JsonValueKind.String)
            {
                string nativeTitle = nativeElement.GetString()!;
                newTitles["Japanese"] = nativeTitle;
                LOGGER.Debug("Added AniList Japanese title: {Title}", nativeTitle);
            }
        }
        else
        {
            LOGGER.Info("AniList series data is missing the 'title' object or it's not an object.");
        }
    }

    /// <summary>
    /// Extracts staff names from AniList series data, filtering by valid roles and deduplicating entries.
    /// </summary>
    /// <param name="seriesData">The JSON element containing AniList series data with a "staff" property.</param>
    /// <param name="nativeStaff">Accumulates native staff names, separated by " | ".</param>
    /// <param name="fullStaff">Accumulates full (romanized) staff names, separated by " | ".</param>
    /// <param name="disallowIllustrationStaff">If <c>true</c>, excludes staff with illustration-only roles.</param>
    /// <returns><c>true</c> if there are more staff pages to fetch; otherwise, <c>false</c>.</returns>
    public static bool ExtractStaffFromAniList(JsonElement seriesData, ref string nativeStaff, ref string fullStaff, bool disallowIllustrationStaff = true)
    {
        if (!seriesData.TryGetProperty("staff", out JsonElement staffData) ||
            !staffData.TryGetProperty("edges", out JsonElement staffEdges) ||
            staffEdges.ValueKind != JsonValueKind.Array)
        {
            nativeStaff = fullStaff = string.Empty;
            LOGGER.Debug("Input media data does not have staff property");
            return false;
        }

        StringBuilder nativeBuilder = new(nativeStaff);
        StringBuilder fullBuilder = new(fullStaff);
        HashSet<string> nativeSeen = new(StringComparer.Ordinal);
        HashSet<string> fullSeen = new(StringComparer.Ordinal);

        foreach (JsonElement edge in staffEdges.EnumerateArray())
        {
            string? roleRaw = edge.GetProperty("role").GetString();
            if (string.IsNullOrWhiteSpace(roleRaw))
            {
                continue;
            }

            string role = StaffRegex().Replace(roleRaw, string.Empty).Trim();
            if (!VALID_STAFF_ROLES.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                LOGGER.Debug("{Role} is not a valid role, skipping staff member", role);
                continue;
            }

            if (disallowIllustrationStaff &&
                (role.Equals("Illustration", StringComparison.OrdinalIgnoreCase) ||
                 role.Equals("Cover Illustration", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            JsonElement nameNode = edge.GetProperty("node").GetProperty("name");

            string? native = nameNode.TryGetProperty("native", out JsonElement nativeVal) ? nativeVal.GetString() : null;
            string? full = nameNode.TryGetProperty("full", out JsonElement fullVal) ? fullVal.GetString() : null;

            string? alt = null;
            if (nameNode.TryGetProperty("alternative", out JsonElement altArr) &&
                altArr.ValueKind == JsonValueKind.Array &&
                altArr.GetArrayLength() > 0)
            {
                alt = altArr[0].GetString();
            }

            // Fallbacks for native
            if (string.IsNullOrWhiteSpace(native))
            {
                if (!string.IsNullOrWhiteSpace(full))
                {
                    native = full;
                }
                else if (!string.IsNullOrWhiteSpace(alt))
                {
                    native = alt;
                }
            }

            // Fallbacks for full
            if (string.IsNullOrWhiteSpace(full))
            {
                if (!string.IsNullOrWhiteSpace(native))
                {
                    full = native;
                }
                else if (!string.IsNullOrWhiteSpace(alt))
                {
                    full = alt;
                }
            }

            if (string.IsNullOrWhiteSpace(native) && string.IsNullOrWhiteSpace(full))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(native) && nativeSeen.Add(native))
            {
                nativeBuilder.Append(native.Trim()).Append(StaffSeparator);
            }

            if (!string.IsNullOrWhiteSpace(full) && fullSeen.Add(full))
            {
                fullBuilder.Append(full.Trim()).Append(StaffSeparator);
            }
        }

        nativeStaff = nativeBuilder.Length > StaffSeparatorLength ? nativeBuilder.ToString(0, nativeBuilder.Length - StaffSeparatorLength) : string.Empty;
        fullStaff = fullBuilder.Length > StaffSeparatorLength ? fullBuilder.ToString(0, fullBuilder.Length - StaffSeparatorLength) : string.Empty;

        if (nativeStaff.Length == 0 && fullStaff.Length == 0)
        {
            LOGGER.Info("AniList staff extraction found no valid entries.");
        }
        else
        {
            LOGGER.Debug("Extracted AniList staff — Native: {NativeStaff}, Full: {FullStaff}", nativeStaff, fullStaff);
        }

        bool hasNextPage = staffData.TryGetProperty("pageInfo", out JsonElement pageInfo)
                       && pageInfo.TryGetProperty("hasNextPage", out JsonElement hasNextPageProp)
                       && hasNextPageProp.GetBoolean();
        return hasNextPage;
    }
}