using System.Text.RegularExpressions;
using System.Web;
using Tsundoku.Helpers;
using Tsundoku.Models.Enums;
using static Tsundoku.Models.Enums.SeriesGenreEnum;

namespace Tsundoku.Clients;

public sealed partial class MangaDex(IHttpClientFactory httpClientFactory)
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly HttpClient _mangadexClient = httpClientFactory.CreateClient("MangaDexClient");

    [GeneratedRegex(@"(?:(?:\n\n---\n\*\*Links:\*\*)|\n{3,}---|\n\n\*\*|\[(?:Official|Wikipedia).*?\]|\n___\n|\r\n\s+\r\n|\*\*\*Won.*)[\s\S]*|- Winner.*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex MangaDexDescCleanupRegex();

    [GeneratedRegex(@"(?<=\bNative\b[^:\n]*:\s*)(?:[\p{P}\s_]+)?(.+?)(?=[\p{P}\p{S}\s]|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex NativeStaffRegex();

    [GeneratedRegex(@"\(([^()\n]+)\)")]
    private static partial Regex NativeStaffFallbackRegex();

    [GeneratedRegex(@"\s*\([^)]*\)")]
    private static partial Regex StaffNameRegex();

    /// <summary>
    /// Asynchronously retrieves MangaDex series data by its title.
    /// Logs and handles HTTP and JSON errors, and URL-encodes the title.
    /// </summary>
    /// <param name="title">The series title to search for on MangaDex.</param>
    /// <returns>
    /// A <see cref="JsonDocument"/> containing the series data, or <c>null</c> if the request fails.
    /// </returns>
    public async Task<JsonDocument?> GetSeriesByTitleAsync(string title)
    {
        try
        {
            string encodedTitle = HttpUtility.UrlEncode(title);

            Dictionary<string, string> query = new()
            {
                ["title"] = encodedTitle,
                ["order[relevance]"] = "desc"
            };

            string queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            string endpoint = $"manga?{queryString}";

            LOGGER.Debug($"MangaDex: Getting series by title async \"{_mangadexClient.BaseAddress}{endpoint}\"");

            using HttpResponseMessage response = await _mangadexClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream);
        }
        catch (HttpRequestException ex)
        {
            LOGGER.Error(ex, $"HTTP error while getting series for title: \"{title}\"");
        }
        catch (JsonException ex)
        {
            LOGGER.Error(ex, $"JSON parsing error for series title: \"{title}\"");
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, $"Unexpected error for series title: \"{title}\"");
        }

        return null;
    }

    /// <summary>
    /// Asynchronously retrieves MangaDex series data by its unique series ID.
    /// </summary>
    /// <param name="id">The unique MangaDex series ID.</param>
    /// <returns>
    /// A <see cref="JsonDocument"/> containing the series data, or <c>null</c> if the request fails.
    /// </returns>
    public async Task<JsonDocument?> GetSeriesByIdAsync(string id)
    {
        try
        {
            string endpoint = $"manga/{id}";
            LOGGER.Debug("Getting series by ID async {Uri}", _mangadexClient.BaseAddress + endpoint);

            using HttpResponseMessage response = await _mangadexClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream);
        }
        catch (HttpRequestException ex)
        {
            LOGGER.Error(ex, $"MangaDex HTTP error while getting series for ID: {id}");
        }
        catch (JsonException ex)
        {
            LOGGER.Error(ex, $"MangaDex JSON parsing error for series ID: {id}");
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, $"MangaDex unexpected error for series ID: {id}");
        }

        return null;
    }

    /// <summary>
    /// Asynchronously retrieves the author name for a given MangaDex author ID.
    /// </summary>
    /// <param name="id">The unique MangaDex author ID.</param>
    /// <returns>
    /// The author's name if found, or <c>null</c> if the request fails.
    /// </returns>
    public async Task<(string name, string? bio)> GetRelationshipAsync(string id, string type)
    {
        try
        {
            string endpoint = $"author/{id}";
            LOGGER.Debug("Getting {Type} relationship async via {Url}", type, _mangadexClient.BaseAddress + endpoint);

            using HttpResponseMessage response = await _mangadexClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync();
            using JsonDocument json = await JsonDocument.ParseAsync(stream);

            JsonElement attributes = json.RootElement
                                         .GetProperty("data")
                                         .GetProperty("attributes");

            string name = attributes.GetProperty("name").GetString()!;
            string? bio = null;

            if (attributes.TryGetProperty("biography", out JsonElement bioElem))
            {
                // Try to get an English or any available language entry from the object
                if (bioElem.ValueKind == JsonValueKind.Object && bioElem.EnumerateObject().Any())
                {
                    // Prefer English ("en") biography if available
                    if (bioElem.TryGetProperty("en", out JsonElement enBio))
                    {
                        bio = enBio.GetString();
                    }
                    else
                    {
                        bio = bioElem.EnumerateObject().First().Value.GetString();
                    }
                }
            }

            return (name, string.IsNullOrWhiteSpace(bio) ? null : bio);
        }
        catch (HttpRequestException ex)
        {
            LOGGER.Error(ex, $"MangaDex HTTP error while getting author for ID: {id}");
        }
        catch (JsonException ex)
        {
            LOGGER.Error(ex, $"MangaDex JSON parsing error for author ID: {id}");
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, $"MangaDex unexpected error for author ID: {id}");
        }

        return (string.Empty, null);
    }

    /// <summary>
    /// Asynchronously builds lists of native and full author/artist names from a MangaDex relationship array.
    /// Matches names using regex patterns to separate native and full names when available.
    /// Skips duplicate names (case-insensitive).
    /// </summary>
    /// <param name="relationships">A <see cref="JsonElement"/> array from the MangaDex API, expected to contain relationship entries.</param>
    /// <param name="title">The title of the series, used for logging if author info is missing.</param>
    /// <returns>
    /// A tuple of <see cref="StringBuilder"/>s representing native and full staff names,
    /// or <c>null</c> if any author name could not be retrieved.
    /// </returns>
    public async Task<(StringBuilder FullStaff, StringBuilder NativeStaff)> GetStaffAsync(JsonElement[] relationships, string title)
    {
        StringBuilder nativeStaffBuilder = new StringBuilder();
        StringBuilder fullStaffBuilder = new StringBuilder();

        AuthorEntry[] authorEntries =
        [
        .. relationships.AsValueEnumerable()
            .Where(static r =>
                r.TryGetProperty("type", out JsonElement typeElem) &&
                r.TryGetProperty("id", out JsonElement idElem) &&
                (typeElem.ValueEquals("author") || typeElem.ValueEquals("artist")) &&
                idElem.ValueKind == JsonValueKind.String)
            .Select(static r => new AuthorEntry(
                r.GetProperty("id").GetString()!,
                r.GetProperty("type").GetString()!))
            .GroupBy(static e => e.Id)
            .Select(static g => new AuthorEntry(
                g.Key,
                string.Join(" & ", g.Select(static e => e.Type).Distinct(StringComparer.Ordinal))))
        ];

        SemaphoreSlim semaphore = new SemaphoreSlim(5);
        Task<(AuthorEntry Entry, string StaffNameRaw, string? Bio)>[] fetchTasks = [.. authorEntries.Select(async entry =>
        {
            await semaphore.WaitAsync();
            try
            {
                (string staffNameRaw, string? bio) = await GetRelationshipAsync(entry.Id, entry.Type);
                return (entry, staffNameRaw, bio);
            }
            finally
            {
                semaphore.Release();
            }
        })];

        (AuthorEntry Entry, string StaffNameRaw, string? Bio)[] results = await Task.WhenAll(fetchTasks);

        foreach ((AuthorEntry entry, string staffNameRaw, string? bio) in results)
        {
            if (string.IsNullOrWhiteSpace(staffNameRaw))
            {
                LOGGER.Warn("Unable to get MangaDex staff {Id} type {Type} for series {Title}", entry.Id, entry.Type, title);
                continue;
            }

            string staffName = StaffNameRegex().Replace(staffNameRaw, string.Empty).Trim();
            string nativeName = string.Empty;

            if (!string.IsNullOrWhiteSpace(bio))
            {
                Match match = NativeStaffRegex().Match(bio);
                if (match.Success)
                {
                    nativeName = match.Groups[1].Value.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(nativeName))
            {
                Match match = NativeStaffFallbackRegex().Match(staffNameRaw);
                if (match.Success)
                {
                    nativeName = match.Groups[1].Value.Trim();
                }
            }

            nativeStaffBuilder.Append(string.IsNullOrWhiteSpace(nativeName) ? staffName : nativeName).Append(" | ");
            fullStaffBuilder.Append(staffName).Append(" | ");
        }

        if (fullStaffBuilder.Length > 3)
        {
            fullStaffBuilder.Length -= 3;
        }

        if (nativeStaffBuilder.Length > 3)
        {
            nativeStaffBuilder.Length -= 3;
        }

        return (fullStaffBuilder, nativeStaffBuilder);
    }

    /// <summary>
    /// Asynchronously retrieves the cover image file name for a MangaDex series by its ID.
    /// Handles both single-object and array responses from the API.
    /// Logs and distinguishes between HTTP, JSON, and unexpected exceptions.
    /// </summary>
    /// <param name="id">The MangaDex series ID used to query the cover endpoint.</param>
    /// <returns>
    /// The cover image file name if available; otherwise, <c>null</c> if an error occurs or no data is returned.
    /// </returns>
    public async Task<string?> GetCoverLinkAsync(string? id, string? mangaDexId, string title)
    {
        if (id is null)
        {
            return null;
        }

        try
        {
            string endpoint = $"cover/{id}";
            LOGGER.Debug("Getting cover async from {Uri}", _mangadexClient.BaseAddress + endpoint);

            using HttpResponseMessage response = await _mangadexClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            using JsonDocument json = await JsonDocument.ParseAsync(contentStream);

            JsonElement data = json.RootElement.GetProperty("data");

            if (data.ValueKind == JsonValueKind.Array)
            {
                JsonElement? first = data.EnumerateArray().FirstOrDefault();
                return first?.GetProperty("attributes").GetProperty("fileName").GetString();
            }

            mangaDexId ??= data.GetProperty("id").GetString();
            string? coverImage = data.GetProperty("attributes").GetProperty("fileName").GetString();

            if (coverImage is null)
            {
                LOGGER.Warn("Unable to get MangaDex cover for {Title}", title);
                return null;
            }

            return @$"https://uploads.mangadex.org/covers/{mangaDexId}/{coverImage}";
        }
        catch (HttpRequestException ex)
        {
            LOGGER.Error(ex, $"MangaDex HTTP error while getting cover for ID: {id}");
        }
        catch (JsonException ex)
        {
            LOGGER.Error(ex, $"MangaDex JSON parsing error for cover ID: {id}");
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, $"MangaDex unexpected error for cover ID: {id}");
        }

        return null;
    }

    public static bool IsMangaDexId(string input)
    {
        return Guid.TryParse(input, out _);
    }

    /// <summary>
    /// Determines if a series is considered digital or "invalid" based on specific keyword criteria across its titles.
    /// A series is deemed invalid if its English title contains "Digital", AND
    /// neither the user input nor its alternative English title contain
    /// "Digital", "Fan Colored", or "Official Colored" (case-insensitive).
    /// </summary>
    /// <param name="input">The user input used to search for the series.</param>
    /// <param name="englishTitle">The English title of the series.</param>
    /// <param name="englishAltTitle">An alternative English title for the series.</param>
    /// <returns>True if the series matches the invalid criteria; otherwise, false.</returns>
    public static bool IsSeriesDigital(string input, string englishTitle, string? englishAltTitle)
    {
        ReadOnlySpan<char> userInput = input;
        if (userInput.Contains("Digital", StringComparison.OrdinalIgnoreCase) ||
            userInput.Contains("Fan Colored", StringComparison.OrdinalIgnoreCase) ||
            userInput.Contains("Official Colored", StringComparison.OrdinalIgnoreCase))
        {
            LOGGER.Debug("User Input contains digital string");
            return true;
        }

        if (!string.IsNullOrWhiteSpace(englishTitle))
        {
            ReadOnlySpan<char> enTitleSpan = englishTitle.AsSpan();
            if (string.IsNullOrWhiteSpace(englishTitle) ||
                enTitleSpan.Contains("Digital", StringComparison.OrdinalIgnoreCase) ||
                enTitleSpan.Contains("Fan Colored", StringComparison.OrdinalIgnoreCase) ||
                enTitleSpan.Contains("Official Colored", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(englishAltTitle))
            {
                ReadOnlySpan<char> alt = englishAltTitle;
                if (alt.Contains("Digital", StringComparison.OrdinalIgnoreCase) ||
                    alt.Contains("Fan Colored", StringComparison.OrdinalIgnoreCase) ||
                    alt.Contains("Official Colored", StringComparison.OrdinalIgnoreCase))
                {
                    LOGGER.Debug("English Alt Title contains digital dtring");
                    return true;
                }
            }

        return false;
    }

    /// <summary>
    /// Constructs a URI link for an anime/manga, prioritizing AniList if an 'al' ID is available,
    /// otherwise falling back to MangaDex using a provided ID or one from a data element.
    /// Handles missing JSON properties gracefully.
    /// </summary>
    /// <param name="attributesBlock">A JsonElement representing the attributes block, expected to contain 'links' property.</param>
    /// <param name="data">A JsonElement representing a data block, expected to contain an 'id' property as a fallback for MangaDex.</param>
    /// <param name="curMangaDexId">An optional existing MangaDex ID to prioritize as a fallback.</param>
    /// <returns>A Uri object for the anime/manga.</returns>
    /// <exception cref="ArgumentException">Thrown if no valid ID (AniList or MangaDex) can be determined.</exception>
    public static Uri ConstructMangaLink(JsonElement attributesBlock, JsonElement data, string? curMangaDexId)
    {
        // Attempt to get the AniList ID from attributesBlock.links.al
        if (attributesBlock.TryGetProperty("links", out JsonElement linksProp) &&
            linksProp.ValueKind == JsonValueKind.Object) // Ensure 'links' is an object
        {
            if (linksProp.TryGetProperty("al", out JsonElement alIdProp) &&
                alIdProp.ValueKind == JsonValueKind.String) // Ensure 'al' is a string
            {
                return new Uri($"https://anilist.co/manga/{alIdProp.GetString()}");
            }
        }

        // If curMangaDexId is null or empty, try to get from data.id
        if (string.IsNullOrWhiteSpace(curMangaDexId))
        {
            if (data.TryGetProperty("id", out JsonElement dataIdProp) &&
                dataIdProp.ValueKind == JsonValueKind.String) // Ensure 'id' is a string
            {
                return new Uri($"https://mangadex.org/title/{dataIdProp.GetString()}");
            }
            else
            {
                // Neither AniList nor MangaDex ID could be determined. This is an error.
                // Log and throw an exception, or return a default/null based on desired behavior.
                // LOGGER?.LogError("Failed to construct manga link: No valid AniList 'al' ID, 'curMangaDexId', or 'data.id' found.");
                throw new ArgumentException("Could not determine a valid Manga ID from provided data.");
            }
        }

        return new Uri($"https://mangadex.org/title/{curMangaDexId}");
    }

    /// <summary>
    /// Scans a MangaDex data array for a series whose English title or alternate English title is not considered "digital".
    /// Matches are checked against the provided user input <paramref name="input"/>.
    /// </summary>
    /// <param name="dataArray">The root <c>JsonElement</c> expected to be an array of series entries.</param>
    /// <param name="input">The user input title to evaluate for filtering.</param>
    /// <returns>The matching <c>JsonElement</c> representing the series, or <c>null</c> if no match is found.</returns>
    public static JsonElement TryFindValidMangaDexSeries(JsonElement dataArray, string input)
    {
        foreach (JsonElement series in dataArray.EnumerateArray())
        {
            if (!series.TryGetProperty("attributes", out JsonElement attributes))
            {
                continue;
            }

            string enTitle = attributes.TryGetProperty("title", out JsonElement titleObj) &&
                titleObj.TryGetProperty("en", out JsonElement enTitleElem)
                ? enTitleElem.GetString() ?? string.Empty
                : string.Empty;

            JsonElement altTitles = attributes.TryGetProperty("altTitles", out JsonElement altRaw) &&
                altRaw.ValueKind == JsonValueKind.Array
                ? altRaw
                : default;

            // Check if any alternate title matches the input
            bool altMatch = enTitle.Contains(input, StringComparison.OrdinalIgnoreCase);
            string? enAlt = null;
            if (!altMatch && altTitles.ValueKind == JsonValueKind.Array)
            {
                LOGGER.Debug("Checking alt titles for matching title input");
                foreach (JsonElement altEntry in altTitles.EnumerateArray())
                {
                    foreach (JsonProperty prop in altEntry.EnumerateObject())
                    {
                        string? altValue = prop.Value.GetString();
                        if (string.IsNullOrWhiteSpace(altValue))
                        {
                            continue;
                        }

                        if (prop.Name == "en")
                        {
                            enAlt = altValue;
                        }

                        if (altValue.Contains(input, StringComparison.OrdinalIgnoreCase))
                        {
                            altMatch = true;
                            break;
                        }
                    }

                    if (altMatch)
                    {
                        break;
                    }
                }
            }

            // Determine if this series is acceptable
            if (!IsSeriesDigital(input, enTitle, enAlt) && altMatch)
            {
                return series;
            }
            else
            {
                LOGGER.Debug("Series ({Input} | {EnTitle} | {EnAlt}) is digital only, skipping", input, enTitle, enAlt);
            }
        }

        LOGGER.Info("{Input} did not return any valid series from MangaDex", input);
        return default;
    }


    /// <summary>
    /// Attempts to locate the "altTitles" array from a MangaDex-style JSON structure.
    /// Searches through either:
    /// 1. A root array of objects with attributes.title["en"] or altTitles containing a match,
    /// 2. A root object with data.attributes.altTitles directly present.
    /// </summary>
    /// <param name="root">The root <see cref="JsonElement"/> to search in.</param>
    /// <param name="englishTitle">The English title to match against title["en"].</param>
    /// <param name="nativeTitle">The native title to match against altTitles.</param>
    /// <returns>The <c>altTitles</c> <see cref="JsonElement"/> if found; otherwise, <c>null</c>.</returns>
    private static JsonElement? FindAltTitlesElement(JsonElement root, string englishTitle, string nativeTitle)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in root.EnumerateArray())
            {
                if (!item.TryGetProperty("attributes", out JsonElement attrs))
                {
                    LOGGER.Debug("Skipping item: missing 'attributes'.");
                    continue;
                }

                // Check title["en"]
                if (attrs.TryGetProperty("title", out JsonElement titleObj) &&
                    titleObj.TryGetProperty("en", out JsonElement titleEn))
                {
                    string? titleValue = titleEn.GetString();
                    if (!string.IsNullOrWhiteSpace(titleValue))
                    {
                        LOGGER.Debug($"Checking title[\"en\"] = \"{titleValue}\" against \"{englishTitle}\"");

                        if (string.Equals(titleValue, englishTitle, StringComparison.OrdinalIgnoreCase))
                        {
                            LOGGER.Debug("Match found in title[\"en\"]");
                            return attrs.TryGetProperty("altTitles", out JsonElement matchedAltTitles)
                                ? matchedAltTitles
                                : null;
                        }
                    }
                }
                else
                {
                    LOGGER.Debug("title[\"en\"] is missing or empty.");
                }

                // Check altTitles array
                if (attrs.TryGetProperty("altTitles", out JsonElement altTitles) &&
                    altTitles.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement altEntry in altTitles.EnumerateArray())
                    {
                        foreach (JsonProperty altProp in altEntry.EnumerateObject())
                        {
                            string? altValue = altProp.Value.GetString();
                            if (!string.IsNullOrWhiteSpace(altValue))
                            {
                                LOGGER.Debug($"Checking altTitles[{altProp.Name}] = \"{altValue}\" against \"{nativeTitle}\"");

                                if (string.Equals(altValue, nativeTitle, StringComparison.OrdinalIgnoreCase))
                                {
                                    LOGGER.Debug($"Match found in altTitles[{altProp.Name}]");
                                    return altTitles;
                                }
                            }
                        }
                    }
                }
                else
                {
                    LOGGER.Debug("altTitles is missing or not an array.");
                }
            }

            LOGGER.Debug("No matching title[\"en\"] or altTitles entry found in any array item.");
        }
        else if (root.ValueKind == JsonValueKind.Object &&
                 root.TryGetProperty("data", out JsonElement data) &&
                 data.TryGetProperty("attributes", out JsonElement attrs) &&
                 attrs.TryGetProperty("altTitles", out JsonElement altTitles) &&
                 altTitles.ValueKind == JsonValueKind.Array)
        {
            LOGGER.Debug("Found direct data.attributes.altTitles path in root object.");
            return altTitles;
        }

        return null;
    }

    /// <summary>
    /// Extracts alternative titles from a MangaDex-like JSON structure,
    /// de-duplicating entries by language code, ensuring only the first
    /// occurrence of each language's title is returned.
    /// </summary>
    /// <param name="root">The JsonElement representing the root of the MangaDex JSON data.</param>
    /// <returns>An array of JsonElement, where each element is a unique alternative title object (e.g., {"en": "Title"}).
    /// Returns an empty array if no alternative titles are found or the structure is invalid.</returns>
    public static JsonElement[] GetAdditionalMangaDexTitleList(JsonElement root, string englishTitle, string nativeTitle)
    {
        JsonElement? altTitlesToProcess = FindAltTitlesElement(root, englishTitle, nativeTitle);

        if (altTitlesToProcess is null || altTitlesToProcess.Value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        HashSet<string> seenLanguages = [];
        List<JsonElement> uniqueAltTitles = [];

        foreach (JsonElement altTitleObject in altTitlesToProcess.Value.EnumerateArray())
        {
            if (altTitleObject.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in altTitleObject.EnumerateObject())
                {
                    string languageCode = property.Name;
                    if (seenLanguages.Add(languageCode))
                    {
                        uniqueAltTitles.Add(altTitleObject);
                    }
                    break;
                }
            }
        }

        return [.. uniqueAltTitles];
    }

    /// <summary>
    /// Attempts to retrieve an alternative title for a specific country from a list of altTitles.
    /// </summary>
    /// <param name="country">The two-letter country code (e.g., "en", "ja", "ko").</param>
    /// <param name="altTitles">An array of JsonElement, where each element is an object
    /// representing an alternative title, with country codes as keys and titles as values
    /// (e.g., [{"en": "My Title"}, {"ja": "私のタイトル"}]).</param>
    /// <returns>The alternative title string if found for the specified country; otherwise, string.Empty.</returns>
    public static string? GetAltTitle(string country, JsonElement[] altTitles)
    {
        foreach (JsonElement altTitleEntry in altTitles)
        {
            // Check if the current altTitleEntry object has a property matching the 'country'
            if (altTitleEntry.ValueKind == JsonValueKind.Object &&
                altTitleEntry.TryGetProperty(country, out JsonElement specificCountryTitle))
            {
                // If the property exists and its value is a string, return it or null
                return specificCountryTitle.GetString();
            }
        }

        // If no matching alternative title for the specified country was found after checking all entries
        return null;
    }

    /// <summary>
    /// Attempts to retrieve an alternative title for a specific country from a list of altTitles.
    /// </summary>
    /// <param name="country">The two-letter country code (e.g., "en", "ja", "ko").</param>
    /// <param name="altTitles">An array of JsonElement, where each element is an object
    /// representing an alternative title, with country codes as keys and titles as values
    /// (e.g., [{"en": "My Title"}, {"ja": "私のタイトル"}]).</param>
    /// <param name="foundTitle">When this method returns, contains the alternative title string if found for the specified country;
    /// otherwise, contains null or string.Empty.</param>
    /// <returns>true if an alternative title was found for the specified country; otherwise, false.</returns>
    public static bool TryGetAltTitle(string country, JsonElement[] altTitles, out string? foundTitle)
    {
        foundTitle = null; // Initialize the out parameter to null

        foreach (JsonElement altTitleEntry in altTitles)
        {
            // Ensure the current element is an object and try to get the property for the country
            if (altTitleEntry.ValueKind == JsonValueKind.Object &&
                altTitleEntry.TryGetProperty(country, out JsonElement specificCountryTitle))
            {
                // If found, get the string value. GetString() returns null if the JSON value is null.
                foundTitle = specificCountryTitle.GetString()!;
                return true; // Success!
            }
        }

        return false;
    }

    /// <summary>
    /// Parses a set of genres from a MangaDex tag JSON array. Only tags with the group "genre"
    /// and a valid English name that maps to a known <see cref="SeriesGenre"/> enum are included.
    /// </summary>
    /// <param name="romajiTitle">The title of the series, used for logging when no genres are found.</param>
    /// <param name="tags">The JSON array of tag elements from MangaDex.</param>
    /// <returns>A <see cref="HashSet{T}"/> of <see cref="SeriesGenre"/> values parsed from the input.</returns>
    public static HashSet<SeriesGenre> ParseGenreData(string romajiTitle, JsonElement attributes)
    {
        if(attributes.ValueKind != JsonValueKind.Object || 
            !attributes.TryGetProperty("tags", out var tags) || 
            tags.ValueKind != JsonValueKind.Array)
        {
            LOGGER.Info("No genre(s) returned for {Title} from MangaDex", romajiTitle);
            return [];
        }

        HashSet<SeriesGenre> genres = [];

        foreach (JsonElement tag in tags.EnumerateArray())
        {
            if (!tag.TryGetProperty("attributes", out JsonElement attr) ||
                !attr.TryGetProperty("group", out JsonElement group) ||
                group.ValueKind != JsonValueKind.String ||
                !group.GetString().Equals("genre", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!attr.TryGetProperty("name", out JsonElement nameObj) ||
                !nameObj.TryGetProperty("en", out JsonElement enName) ||
                enName.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            if (SeriesGenreEnum.TryParse(enName.GetString(), out SeriesGenre genre))
            {
                genres.Add(genre);
            }
        }

        return genres;
    }

    public static string ParseDescription(string seriesDescription)
    {
        ReadOnlySpan<char> span = seriesDescription.AsSpan();

        if (!span.Contains("***Won", StringComparison.OrdinalIgnoreCase) &&
            !span.Contains("- Winner", StringComparison.OrdinalIgnoreCase) &&
            !span.Contains("**Links:**", StringComparison.OrdinalIgnoreCase) &&
            !span.Contains("Wikipedia", StringComparison.OrdinalIgnoreCase) &&
            !span.Contains("Official", StringComparison.OrdinalIgnoreCase) &&
            !span.Contains("MangaPlus:", StringComparison.OrdinalIgnoreCase) &&
            !span.Contains("---", StringComparison.OrdinalIgnoreCase) &&
            !span.Contains("___", StringComparison.OrdinalIgnoreCase) &&
            !span.Contains("\r\n \r\n", StringComparison.OrdinalIgnoreCase))
        {
            return ExtensionMethods.NormalizeQuotes(seriesDescription.TrimEnd('\n')).Trim();
        }
        
        return ExtensionMethods.NormalizeQuotes(MangaDexDescCleanupRegex().Replace(seriesDescription, string.Empty)).Trim();
    }

    private readonly struct AuthorEntry(string id, string type)
    {
        public readonly string Id = id;
        public readonly string Type = type;
    }
}
