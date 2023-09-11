using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tsundoku.Helpers
{
    public partial class MangadexQuery
    {
        private static readonly HttpClient MangadexClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        })
        {
            BaseAddress = new Uri("https://api.mangadex.org/"),
            DefaultRequestVersion = HttpVersion.Version30,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        public static readonly JsonSerializerOptions options = new()
        { 
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        private static readonly string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.62";
        [GeneratedRegex("\\n\\n\\n---[\\S\\s.]*|\\n\\n\\*\\*[\\S\\s.]*")] private static partial Regex MangaDexDescRegex();

        static MangadexQuery()
        {
            MangadexClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
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
                //LOGGER.Debug($"{MangadexClient.BaseAddress}manga?title={title.Replace(" ", "%20")}");
                var response = await MangadexClient.GetStringAsync($"manga?title={title.Replace(" ", "%20")}");
                // File.WriteAllText(@"MangadexTitleTest.json", JsonSerializer.Serialize(JsonDocument.Parse(response), options));
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
                var response = await MangadexClient.GetStringAsync($"manga/{id}");
                // File.WriteAllText(@"MangadexIdTest.json", JsonSerializer.Serialize(JsonDocument.Parse(response), options));
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
                var response = await MangadexClient.GetStringAsync($"author/{id}");
                // File.WriteAllText(@"MangadexAuthorTest.json", JsonSerializer.Serialize(response, options));
                return JsonDocument.Parse(response).RootElement.GetProperty("data").GetProperty("attributes").GetProperty("name").ToString();
            }
            catch (Exception e)
            {
                LOGGER.Error($"MangaDex GetAuthor w/ {id} Request Failed {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// ttp request to get cover image for a MangaDex series by ID
        /// </summary>
        /// <param name="id">The id used to query the cover</param>
        /// <returns></returns>
        public static async Task<string?> GetCoverAsync(string id)
        {
            try
            {
                var response = await MangadexClient.GetStringAsync($"cover/{id}");
                JsonElement data = JsonDocument.Parse(response).RootElement.GetProperty("data");
                // File.WriteAllText(@"MangadexCoverTest.json", JsonSerializer.Serialize(JsonDocument.Parse(response), options));
                if (data.ValueKind == JsonValueKind.Array)
                {
                    return data.EnumerateArray().ElementAt(0).GetProperty("attributes").GetProperty("fileName").ToString();
                }
                {
                    return data.GetProperty("attributes").GetProperty("fileName").ToString();
                }
            }
            catch (Exception e)
            {
                LOGGER.Error($"MangaDex GetCover w/ {id} Request Failed {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Gets the additional/alternative titles for a MangaDex series
        /// </summary>
        /// <param name="data">The JSON data of the series from MangaDex</param>
        /// <returns></returns>
        public static List<JsonElement> GetAdditionalMangaDexTitleList(JsonElement data)
		{
			if (data.ValueKind == JsonValueKind.Array) // Collection
			{
				return data.EnumerateArray().ElementAt(0).GetProperty("attributes").GetProperty("altTitles").EnumerateArray().ToList();
			}
			else
			{
				return data.GetProperty("attributes").GetProperty("altTitles").EnumerateArray().ToList();
			}
		}

        /// <summary>
        /// Parses MangaDex series descriptions to remove all of the fluff
        /// </summary>
        /// <param name="seriesDescription">The string containing the description of the series</param>
        /// <returns></returns>
        public static string ParseMangadexDescription(string seriesDescription)
		{
			return MangaDexDescRegex().Replace(seriesDescription, "").Trim();
		}
    }
}
