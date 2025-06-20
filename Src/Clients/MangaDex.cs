using System.Text.RegularExpressions;
using System.Web;
using Tsundoku.Models;

namespace Tsundoku.Clients;

public sealed partial class MangaDex(IHttpClientFactory httpClientFactory)
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly HttpClient _mangadexClient = httpClientFactory.CreateClient("MangaDexClient");
    [GeneratedRegex(@"(?:\n\n---\n\*\*Links:\*\*|\n\n\n---|\n\n\*\*|\[(?:Official|Wikipedia).*?\]|\n___\n|\r\n\s+\r\n)[\S\s]*|- Winner.*$")] private static partial Regex MangaDexDescCleanupRegex();
    [GeneratedRegex(@"[a-z\d]{8}-[a-z\d]{4}-[a-z\d]{4}-[a-z\d]{4}-[a-z\d]{11,}")] public static partial Regex MangaDexIDRegex();
    [GeneratedRegex(@" \((.*)\)")] public static partial Regex NativeStaffRegex();
    [GeneratedRegex(@"^.*(?= \(.*\))")] public static partial Regex FullStaffRegex();

    // TODO - Add unit tests for Async calls, also checkf or improvements on regex

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
            string endpoint = $"manga?title={encodedTitle}";

            LOGGER.Debug($"MangaDex: Getting series by title async \"{_mangadexClient.BaseAddress}{endpoint}\"");

            using HttpResponseMessage response = await _mangadexClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream);
        }
        catch (HttpRequestException ex)
        {
            LOGGER.Error(ex, $"MangaDex HTTP error while getting series for title: \"{title}\"");
        }
        catch (JsonException ex)
        {
            LOGGER.Error(ex, $"MangaDex JSON parsing error for series title: \"{title}\"");
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, $"MangaDex unexpected error for series title: \"{title}\"");
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
            LOGGER.Debug($"MangaDex: Getting series by ID async \"{_mangadexClient.BaseAddress}{endpoint}\"");

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
    public async Task<string?> GetAuthorAsync(string id)
    {
        try
        {
            string endpoint = $"author/{id}";
            LOGGER.Debug($"MangaDex: Getting author async \"{_mangadexClient.BaseAddress}{endpoint}\"");

            using HttpResponseMessage response = await _mangadexClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync();
            using JsonDocument json = await JsonDocument.ParseAsync(stream);

            return json.RootElement
                       .GetProperty("data")
                       .GetProperty("attributes")
                       .GetProperty("name")
                       .GetString();
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

        return null;
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
        if (id == null)
        {
            return null;
        }

        try
        {
            string endpoint = $"cover/{id}";
            LOGGER.Debug($"MangaDex: Getting cover async from \"{_mangadexClient.BaseAddress}{endpoint}\"");

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

            mangaDexId = mangaDexId ?? data.GetProperty("id").GetString();
            string? coverImage = data.GetProperty("attributes").GetProperty("fileName").GetString();

            if (coverImage == null)
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

    /// <summary>
    /// Determines if a series is considered digital or "invalid" based on specific keyword criteria across its titles.
    /// A series is deemed invalid if its English title contains "Digital", AND
    /// neither its primary title nor its alternative English title contain
    /// "Digital", "Fan Colored", or "Official Colored" (case-insensitive).
    /// </summary>
    /// <param name="title">The primary title of the series.</param>
    /// <param name="englishTitle">The English title of the series.</param>
    /// <param name="englishAltTitle">An alternative English title for the series.</param>
    /// <returns>True if the series matches the invalid criteria; otherwise, false.</returns>
    public static bool IsSeriesDigital(string title, string englishTitle, string englishAltTitle)
    {
        if (string.IsNullOrEmpty(englishTitle) ||
            !englishTitle.Contains("Digital", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (title.AsSpan().Contains("Digital", StringComparison.OrdinalIgnoreCase) ||
            title.AsSpan().Contains("Fan Colored", StringComparison.OrdinalIgnoreCase) ||
            title.AsSpan().Contains("Official Colored", StringComparison.OrdinalIgnoreCase) ||
            englishAltTitle.AsSpan().Contains("Digital", StringComparison.OrdinalIgnoreCase) ||
            englishAltTitle.AsSpan().Contains("Official Colored", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Locates the "altTitles" JSON array element within a given root JsonElement structure.
    /// This method handles two common root structures: a direct object or an array containing objects
    /// that have the "data.attributes.altTitles" path.
    /// </summary>
    /// <param name="root">The JsonElement representing the root of the JSON structure to search within.</param>
    /// <returns>A nullable JsonElement representing the "altTitles" array if found; otherwise, null.</returns>
    private static JsonElement? FindAltTitlesElement(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in root.EnumerateArray())
            {
                if (item.TryGetProperty("data", out JsonElement data) &&
                    data.TryGetProperty("attributes", out JsonElement attrs) &&
                    attrs.TryGetProperty("altTitles", out JsonElement altTitles) &&
                    altTitles.ValueKind == JsonValueKind.Array)
                {
                    return altTitles;
                }
            }
            return null;
        }
        else if (root.ValueKind == JsonValueKind.Object &&
                 root.TryGetProperty("data", out JsonElement data) &&
                 data.TryGetProperty("attributes", out JsonElement attrs) &&
                 attrs.TryGetProperty("altTitles", out JsonElement altTitles) &&
                 altTitles.ValueKind == JsonValueKind.Array)
        {
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
    public static JsonElement[] GetAdditionalMangaDexTitleList(JsonElement root)
    {
        JsonElement? altTitlesToProcess = FindAltTitlesElement(root);

        if (altTitlesToProcess == null || altTitlesToProcess.Value.ValueKind != JsonValueKind.Array)
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
    public static bool TryGetAltTitle(string country, JsonElement[] altTitles, out string foundTitle)
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
    /// and a valid English name that maps to a known <see cref="Genre"/> enum are included.
    /// </summary>
    /// <param name="romajiTitle">The title of the series, used for logging when no genres are found.</param>
    /// <param name="tags">The JSON array of tag elements from MangaDex.</param>
    /// <returns>A <see cref="HashSet{T}"/> of <see cref="Genre"/> values parsed from the input.</returns>
    public static HashSet<Genre> ParseGenreData(string romajiTitle, JsonElement attributes)
    {
        if(attributes.ValueKind != JsonValueKind.Object || 
            !attributes.TryGetProperty("tags", out var tags) || 
            tags.ValueKind != JsonValueKind.Array)
        {
            LOGGER.Info("No genre(s) returned for {Title} from MangaDex", romajiTitle);
            return [];
        }

        HashSet<Genre> genres = [];

        foreach (JsonElement tag in tags.EnumerateArray())
        {
            if (!tag.TryGetProperty("attributes", out var attr) ||
                !attr.TryGetProperty("group", out var group) ||
                group.ValueKind != JsonValueKind.String ||
                !group.GetString().Equals("genre", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!attr.TryGetProperty("name", out var nameObj) ||
                !nameObj.TryGetProperty("en", out var enName) ||
                enName.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            if (GenreExtensions.TryGetGenre(enName.GetString(), out Genre genre))
            {
                genres.Add(genre);
            }
        }

        return genres;
    }

    // TODO - Add berserk https://mangadex.org/title/801513ba-a712-498c-8f57-cae55b38cc92/berserk?tab=art desc as a test
    public static string ParseDescription(string seriesDescription)
    {
        return MangaDexDescCleanupRegex().Replace(seriesDescription, string.Empty).TrimEnd('\n').Trim();
    }
}
