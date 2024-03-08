using System.Net;
using System.Text.RegularExpressions;

namespace Tsundoku.Helpers
{
    public partial class MangadexQuery
    {
        private static readonly HttpClient MangadexClient;
        [GeneratedRegex(@"- Winner.*$|\n\n\n---[\S\s.]*|\n\n\*\*[\S\s.]*|\[Official.*?\].*|\[Wikipedia.*?\].*|\n\n---\n\*\*Links:\*\*\n\n.*")] private static partial Regex MangaDexDescRegex();

        static MangadexQuery()
        {
            MangadexClient = new HttpClient(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            })
            {
                BaseAddress = new Uri("https://api.mangadex.org/"),
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
            };
            MangadexClient.DefaultRequestHeaders.Add("User-Agent", ViewModels.ViewModelBase.USER_AGENT);
        }

        /// <summary>
        /// REST request to get series data for a MangaDex series by title
        /// </summary>
        /// <param name="title">A string used for getting series data from MangaDex</param>
        /// <returns></returns>
        public static async Task<JsonDocument?> GetSeriesByTitleAsync(string title)
        {
            try
            {
                LOGGER.Debug($"MangaDex Getting Series By Title Async \"{MangadexClient.BaseAddress}manga?title={title.Replace(" ", "%20")}\"");
                var response = await MangadexClient.GetStringAsync($"manga?title={title.Replace(" ", "%20")}");
                return JsonDocument.Parse(response);
            }
            catch (HttpRequestException e)
            {
                LOGGER.Error($"MangaDex GetSeriesByTitle w/ {title} Request Failed HttpRequestException {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Http request to get series data for a MangaDex series by ID
        /// </summary>
        /// <param name="id">A unique string ID used for getting series data from MangaDex</param>
        /// <returns></returns>
        public static async Task<JsonDocument?> GetSeriesByIdAsync(string id)
        {
            try
            {
                LOGGER.Debug($"MangaDex Getting Series Async \"{MangadexClient.BaseAddress}manga/{id}\"");
                var response = await MangadexClient.GetStringAsync($"manga/{id}");
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
        public static async Task<string?> GetAuthorAsync(string id)
        {
            try
            {
                LOGGER.Debug($"MangaDex Getting Author Async \"{MangadexClient.BaseAddress}author/{id}\"");
                var response = await MangadexClient.GetStringAsync($"author/{id}");
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
        public static async Task<string?> GetCoverAsync(string id)
        {
            try
            {
                LOGGER.Debug($"MangaDex Getting Cover Async \"{MangadexClient.BaseAddress}cover/{id}\"");
                var response = await MangadexClient.GetStringAsync($"cover/{id}");
                JsonElement data = JsonDocument.Parse(response).RootElement.GetProperty("data");
                if (data.ValueKind == JsonValueKind.Array)
                {
                    return data.EnumerateArray().ElementAt(0).GetProperty("attributes").GetProperty("fileName").GetString();
                }
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

        /// <summary>
        /// Gets the additional/alternative titles for a MangaDex series
        /// </summary>
        /// <param name="data">The JSON data of the series from MangaDex</param>
        /// <returns></returns>
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

        /// <summary>
        /// Parses MangaDex series descriptions to remove all of the fluff
        /// </summary>
        /// <param name="seriesDescription">The string containing the description of the series</param>
        /// <returns></returns>
        public static string ParseMangadexDescription(string seriesDescription)
		{
			return MangaDexDescRegex().Replace(seriesDescription, "").TrimEnd('\n').Trim();
		}
    }
}
