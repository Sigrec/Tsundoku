using System.Text.RegularExpressions;
using System.Web;
using Tsundoku.Models;

namespace Tsundoku.Helpers
{
    public partial class MangaDex(IHttpClientFactory httpClientFactory)
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private readonly HttpClient _mangadexClient = httpClientFactory.CreateClient("MangaDexClient");
        [GeneratedRegex(@"(?:\n\n---\n\*\*Links:\*\*|\n\n\n---|\n\n\*\*|\[(?:Official|Wikipedia).*?\]|\n___\n|\r\n\s+\r\n)[\S\s]*|- Winner.*$")] private static partial Regex MangaDexDescRegex();
        [GeneratedRegex(@"[a-z\d]{8}-[a-z\d]{4}-[a-z\d]{4}-[a-z\d]{4}-[a-z\d]{11,}")] public static partial Regex MangaDexIDRegex();
        [GeneratedRegex(@" \((.*)\)")] public static partial Regex NativeStaffRegex();
        [GeneratedRegex(@"^.*(?= \(.*\))")] public static partial Regex FullStaffRegex();

        // <summary>
        /// REST request to get series data for a MangaDex series by title
        /// </summary>
        /// <param name="title">A string used for getting series data from MangaDex</param>
        /// <returns></returns>
        // Make the method non-static
        public async Task<JsonDocument?> GetSeriesByTitleAsync(string title)
        {
            try
            {
                title = HttpUtility.UrlEncode(title);
                LOGGER.Debug($"MangaDex Getting Series By Title Async \"{_mangadexClient.BaseAddress}manga?title={title}\"");
                var response = await _mangadexClient.GetStringAsync($"manga?title={title}");
                return JsonDocument.Parse(response);
            }
            catch (HttpRequestException e)
            {
                LOGGER.Error($"MangaDex GetSeriesByTitle w/ \"{title}\" Request Failed HttpRequestException {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Http request to get series data for a MangaDex series by ID
        /// </summary>
        /// <param name="id">A unique string ID used for getting series data from MangaDex</param>
        /// <returns></returns>
        // Make the method non-static
        public async Task<JsonDocument?> GetSeriesByIdAsync(string id)
        {
            try
            {
                LOGGER.Debug($"MangaDex Getting Series Async \"{_mangadexClient.BaseAddress}manga/{id}\"");
                var response = await _mangadexClient.GetStringAsync($"manga/{id}");
                return JsonDocument.Parse(response);
            }
            catch (Exception e)
            {
                LOGGER.Error($"MangaDex GetSeriesById w/ {id} Request Failed {e} {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Http request to get author names for a MangaDex series by ID
        /// </summary>
        /// <param name="id">The string id used to query the author data for a MangaDex series</param>
        /// <returns></returns>
        // Make the method non-static
        public async Task<string?> GetAuthorAsync(string id)
        {
            try
            {
                LOGGER.Debug($"MangaDex Getting Author Async \"{_mangadexClient.BaseAddress}author/{id}\"");
                var response = await _mangadexClient.GetStringAsync($"author/{id}");
                return JsonDocument.Parse(response).RootElement.GetProperty("data").GetProperty("attributes").GetProperty("name").GetString();
            }
            catch (Exception e)
            {
                LOGGER.Error($"MangaDex GetAuthor w/ {id} Request Failed {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// http request to get cover image for a MangaDex series by ID
        /// </summary>
        /// <param name="id">The id used to query the cover</param>
        /// <returns></returns>
        // Make the method non-static
        public async Task<string?> GetCoverAsync(string id)
        {
            try
            {
                LOGGER.Debug($"MangaDex Getting Cover Async \"{_mangadexClient.BaseAddress}cover/{id}\"");
                var response = await _mangadexClient.GetStringAsync($"cover/{id}");
                JsonElement data = JsonDocument.Parse(response).RootElement.GetProperty("data");
                if (data.ValueKind == JsonValueKind.Array) // Collection
                {
                    return data.EnumerateArray().ElementAt(0).GetProperty("attributes").GetProperty("fileName").GetString();
                }
                else
                {
                    return data.GetProperty("attributes").GetProperty("fileName").GetString();
                }
            }
            catch (Exception e)
            {
                LOGGER.Error($"MangaDex Get Cover w/ {id} Request Failed {e.Message}");
            }
            return null;
        }

        // These methods don't use HttpClient, so they can remain static if you prefer,
        // or become instance methods if MangaDex is meant to be entirely an instance class.
        // For now, I'll leave them static if they truly have no instance dependency.
        public static bool IsSeriesInvalid(string title, string englishTitle, string englishAltTitle)
        {
            return
            (
                !(
                    title.Contains("Digital", StringComparison.OrdinalIgnoreCase)
                    || title.Contains("Fan Colored", StringComparison.OrdinalIgnoreCase)
                    || englishAltTitle.Contains("Digital", StringComparison.OrdinalIgnoreCase)
                    || englishAltTitle.Contains("Official Colored", StringComparison.OrdinalIgnoreCase)
                    || title.Contains("Official Colored", StringComparison.OrdinalIgnoreCase)
                )
                && englishTitle.Contains("Digital", StringComparison.OrdinalIgnoreCase)
            );
        }

        public static JsonElement.ArrayEnumerator GetAdditionalMangaDexTitleList(JsonElement data)
        {
            if (data.ValueKind == JsonValueKind.Array) // Collection
            {
                if (data.EnumerateArray().Any())
                {
                    return data.EnumerateArray().ElementAt(0).GetProperty("attributes").GetProperty("altTitles").EnumerateArray();
                }
                else
                {
                    return new JsonElement.ArrayEnumerator();
                }
            }
            else
            {
                return data.GetProperty("attributes").GetProperty("altTitles").EnumerateArray();
            }
        }

        public static HashSet<Genre> ParseGenreData(string romajiTitle, JsonElement tags)
        {
            if (tags.ValueKind != JsonValueKind.Null)
            {
                HashSet<Genre> newGenres = [];
                foreach (JsonElement tagElement in tags.EnumerateArray())
                {
                    JsonElement attribute = tagElement.GetProperty("attributes");
                    if (attribute.GetProperty("group").GetString().Equals("genre"))
                    {
                        Genre genre = GenreExtensions.GetGenreFromString(attribute.GetProperty("name").GetProperty("en").GetString());
                        if (genre != Genre.None)
                        {
                            newGenres.Add(genre);
                        }
                    }
                }
                return newGenres;
            }
            else
            {
                LOGGER.Debug($"No genre(s) returned for \"{romajiTitle}\" from AniList");
                return [];
            }
        }

        public static string ParseMangadexDescription(string seriesDescription)
        {
            return MangaDexDescRegex().Replace(seriesDescription, "").TrimEnd('\n').Trim();
        }
    }
}
