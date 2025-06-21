using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using static Tsundoku.Models.Enums.SeriesFormatEnum;
using static Tsundoku.Models.Enums.SeriesGenreEnum;

namespace Tsundoku.Clients;

public sealed class AniListGraphQLClient : GraphQLHttpClient
{
    public AniListGraphQLClient(HttpClient httpClient)
        : base(new GraphQLHttpClientOptions
        {
            EndPoint = httpClient.BaseAddress!
        }, new SystemTextJsonSerializer(), httpClient)
    {
    }
}

public sealed partial class AniList
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly AniListGraphQLClient _aniListClient;

    [GeneratedRegex(@"(?is)<i>Note[s]?:.*?</i>|<br\s*/?>|\(Source:.*?\)", RegexOptions.IgnoreCase)] private static partial Regex AniListDescRegex();
    [GeneratedRegex(@"(?:\s*\n)?(?:<br\s*/?>\s*){2,}(?:\n\s*)?", RegexOptions.IgnoreCase)] private static partial Regex AniListBreakRegex();
    [GeneratedRegex(@" \(.*\)")] private static partial Regex StaffRegex();
    [GeneratedRegex(@"</?[^>]+>")] private static partial Regex HtmlTagStripRegex();
    [GeneratedRegex("[“”‘’]")] private static partial Regex SmartQuotesRegex();

    private static readonly string[] VALID_STAFF_ROLES = ["Story & Art", "Story", "Art", "Original Creator", "Character Design", "Cover Illustration", "Illustration", "Mechanical Design", "Original Story", "Original Character Design", "Original Story", "Supervisor"];

    // Constructor for Dependency Injection
    public AniList(AniListGraphQLClient aniListClient)
    {
        _aniListClient = aniListClient;
    }

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
    /// Executes a GraphQL query against the AniList API using the given media selector and variables.
    /// Includes automatic retry handling if rate-limited.
    /// </summary>
    /// <param name="mediaSelector">The GraphQL selection string (e.g., Media(...)).</param>
    /// <param name="variables">An anonymous object containing GraphQL variables.</param>
    /// <param name="contextLabel">A descriptive label used for logging purposes.</param>
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
                field = "Media(search: $title, format: $format)";
                variableDeclarations = "($title: String, $format: MediaFormat, $pageNum: Int)";
            }
            else
            {
                field = "Media(id: $seriesId, format: $format)";
                variableDeclarations = "($seriesId: Int, $format: MediaFormat, $pageNum: Int)";
            }

            GraphQLRequest queryRequest = new GraphQLRequest
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
                        staff(sort: RELEVANCE, perPage: 25, page: $pageNum) {{
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
                maxRetries,
                cancellationToken
            );

            if (response == null)
            {
                LOGGER.Warn("AniList query for {Context} returned null (rate-limited or canceled).", contextLabel);
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

    private Task<GraphQLResponse<JsonDocument?>?> SendWithRateLimitRetryAsync<T>(
        GraphQLRequest request,
        ushort maxRetries = 5,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            int attempts = 0;
            while (attempts < maxRetries)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    LOGGER.Info("Request canceled before sending GraphQL query.");
                    return null;
                }

                GraphQLResponse<JsonDocument?> response = await _aniListClient
                    .SendQueryAsync<JsonDocument?>(request, cancellationToken);

                TimeSpan? delay = GetRateLimitDelay(response.AsGraphQLHttpResponse().ResponseHeaders);
                if (delay == null)
                {
                    return response;
                }

                attempts++;

                if (cancellationToken.IsCancellationRequested)
                {
                    LOGGER.Info("Request canceled during delay wait.");
                    return null;
                }

                LOGGER.Warn("Rate limit hit (attempt {Attempt}/{Max}). Delaying {Delay}...", attempts, maxRetries, delay.Value);
                await Task.Delay(delay.Value, cancellationToken);
            }

            LOGGER.Error("Exceeded maximum retry attempts ({MaxRetries}) due to rate limiting. Skipping request.", maxRetries);
            return null;
        }, cancellationToken);
    }

    /// <summary>
    /// Parses the AniList rate limit headers and returns the retry delay if rate-limited.
    /// </summary>
    /// <param name="headers">HTTP response headers.</param>
    /// <returns>
    /// A <see cref="TimeSpan"/> representing the delay until the rate limit resets,
    /// or <c>null</c> if no delay is needed.
    /// </returns>
    private static TimeSpan? GetRateLimitDelay(HttpResponseHeaders headers)
    {
        if (headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues) &&
            short.TryParse(remainingValues.FirstOrDefault(), out short remaining))
        {
            LOGGER.Info("AniList Rate Remaining = {Remaining}", remaining);

            if (remaining > 0)
            {
                return null;
            }
        }
        else
        {
            LOGGER.Warn("AniList response missing or invalid 'X-RateLimit-Remaining' header.");
        }

        if (headers.TryGetValues("Retry-After", out var retryAfterValues) &&
            short.TryParse(retryAfterValues.FirstOrDefault(), out short delaySeconds))
        {
            LOGGER.Warn("AniList rate limit reached. Delaying for {Seconds} seconds.", delaySeconds);
            return TimeSpan.FromSeconds(delaySeconds);
        }

        LOGGER.Warn("AniList response missing or invalid 'Retry-After' header. Defaulting to 30s.");
        return TimeSpan.FromSeconds(30); // fallback to prevent hammering the API
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
            "“" or "”" => "\"",
            "‘" or "’" => "'",
            _ => m.Value
        });

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
                string romajiTitle = romajiElement.GetString();
                newTitles["Romaji"] = romajiTitle;
                LOGGER.Debug("Added AniList romaji title: {Title}", romajiTitle);
            }

            if (titleElement.TryGetProperty("english", out JsonElement englishElement) && englishElement.ValueKind == JsonValueKind.String)
            {
                string englishTitle = englishElement.GetString();
                newTitles["English"] = englishTitle;
                LOGGER.Debug("Added AniList English title: {Title}, englishTitle");
            }

            if (titleElement.TryGetProperty("native", out JsonElement nativeElement) && nativeElement.ValueKind == JsonValueKind.String)
            {
                string nativeTitle = nativeElement.GetString();
                newTitles["Japanese"] = nativeTitle;
                LOGGER.Debug("Added AniList Japanese title: {Title}", nativeTitle);
            }
        }
        else
        {
            LOGGER.Info("AniList series data is missing the 'title' object or it's not an object.");
        }
    }

    public static bool SeriesHasNextPage(JsonElement staff)
    {
        return staff.GetProperty("staff").GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean();
    }

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

        StringBuilder nativeBuilder = new StringBuilder(nativeStaff);
        StringBuilder fullBuilder = new StringBuilder(fullStaff);
        HashSet<string> nativeSeen = new HashSet<string>(StringComparer.Ordinal);
        HashSet<string> fullSeen = new HashSet<string>(StringComparer.Ordinal);

        foreach (JsonElement edge in staffEdges.EnumerateArray())
        {
            string roleRaw = edge.GetProperty("role").GetString();
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

            string native = nameNode.TryGetProperty("native", out JsonElement nativeVal) ? nativeVal.GetString() : null;
            string full = nameNode.TryGetProperty("full", out JsonElement fullVal) ? fullVal.GetString() : null;

            string alt = null;
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
                nativeBuilder.Append(native.Trim()).Append(" | ");
            }

            if (!string.IsNullOrWhiteSpace(full) && fullSeen.Add(full))
            {
                fullBuilder.Append(full.Trim()).Append(" | ");
            }
        }

        nativeStaff = nativeBuilder.Length > 3 ? nativeBuilder.ToString(0, nativeBuilder.Length - 3) : string.Empty;
        fullStaff = fullBuilder.Length > 3 ? fullBuilder.ToString(0, fullBuilder.Length - 3) : string.Empty;

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