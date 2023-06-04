using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tsundoku.Models;

namespace Tsundoku.Helpers
{
    public class MangadexQuery
    {
        private static readonly HttpClient MangadexClient = new HttpClient();
        public static readonly JsonSerializerOptions options = new JsonSerializerOptions { 
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        /// <summary>
        /// Http request to get series data for a MangaDex series by title
        /// </summary>
        /// <param name="title">A string used for getting series data from MangaDex</param>
        /// <returns></returns>
        public async Task<JsonDocument?> GetSeriesByTitleAsync(string title)
        {
            try
            {
                var response = await MangadexClient.GetStringAsync(@$"https://api.mangadex.org/manga?title='{title}'");
                // File.WriteAllText(@"MangadexTitleTest.json", JsonSerializer.Serialize(JsonDocument.Parse(response), options));
                return JsonDocument.Parse(response);
            }
            catch (Exception e)
            {
                Constants.Logger.Warn($"{title} Request Failed {e.ToString()}");
            }
            return null;
        }

        /// <summary>
        /// Http request to get series data for a MangaDex series by ID
        /// </summary>
        /// <param name="id">A unique string ID used for getting series data from MangaDex</param>
        /// <returns></returns>
        public async Task<JsonDocument?> GetSeriesByIdAsync(string id)
        {
            try
            {
                var response = await MangadexClient.GetStringAsync(@$"https://api.mangadex.org/manga/{id}");
                // File.WriteAllText(@"MangadexIdTest.json", JsonSerializer.Serialize(JsonDocument.Parse(response), options));
                return JsonDocument.Parse(response);
            }
            catch (Exception e)
            {
                Constants.Logger.Warn($"{id} Request Failed {e.ToString()}");
            }
            return null;
        }

        /// <summary>
        /// Http request to get author names for a MangaDex series by ID
        /// </summary>
        /// <param name="id">The string id used to query the author data for a MangaDex series</param>
        /// <returns></returns>
        public async Task<string?> GetAuthor(string id)
        {
            try
            {
                var response = await MangadexClient.GetStringAsync(@$"https://api.mangadex.org/author/{id}");
                // File.WriteAllText(@"MangadexAuthorTest.json", JsonSerializer.Serialize(response, options));
                return JsonDocument.Parse(response).RootElement.GetProperty("data").GetProperty("attributes").GetProperty("name").ToString();
            }
            catch (Exception e)
            {
                Constants.Logger.Warn($"{id} Request Failed {e.ToString()}");
            }
            return null;
        }

        /// <summary>
        /// ttp request to get cover image for a MangaDex series by ID
        /// </summary>
        /// <param name="id">The id used to query the cover</param>
        /// <returns></returns>
        public async Task<string?> GetCover(string id)
        {
            try
            {
                var response = await MangadexClient.GetStringAsync(@$"https://api.mangadex.org/cover/{id}");
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
                Constants.Logger.Warn($"{id} Request Failed {e.ToString()}");
            }
            return null;
        }

        /// <summary>
        /// Gets the additional/alternative titles for a MangaDex series
        /// </summary>
        /// <param name="data"></param>
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
			return Regex.Replace(seriesDescription, @"\n\n\n---[\S\s.]*|\n\n\*\*[\S\s.]*", "").Trim();
		}

        public void Dispose()
        {
            MangadexClient.Dispose();
        }
    }
}
