using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

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

        public JsonDocument GetSeriesTitleAsync(string title)
        {
            JsonDocument data = JsonDocument.Parse(MangadexClient.GetStringAsync(@$"https://api.mangadex.org/manga?title='{title}'").Result);
            // File.WriteAllText(@"MangadexTitleTest.json", JsonSerializer.Serialize(data, options));
            return data;
        }

        public JsonDocument GetSeriesIdAsync(string id)
        {
            JsonDocument data = JsonDocument.Parse(MangadexClient.GetStringAsync(@$"https://api.mangadex.org/manga/{id}").Result);
            // File.WriteAllText(@"MangadexIdTest.json", JsonSerializer.Serialize(data, options));
            return data;
        }

        public string GetAuthor(string id)
        {
            JsonDocument data = JsonDocument.Parse(MangadexClient.GetStringAsync(@$"https://api.mangadex.org/author/{id}").Result);
            // File.WriteAllText(@"MangadexAuthorTest.json", JsonSerializer.Serialize(data, options));
            return data.RootElement.GetProperty("data").GetProperty("attributes").GetProperty("name").ToString();
        }

        public string GetCover(string id)
        {
            JsonElement data = JsonDocument.Parse(MangadexClient.GetStringAsync(@$"https://api.mangadex.org/cover/{id}").Result).RootElement.GetProperty("data");
            File.WriteAllText(@"MangadexCoverTest.json", JsonSerializer.Serialize(data, options));
            if (data.ValueKind == JsonValueKind.Array)
            {
                return data.EnumerateArray().ElementAt(0).GetProperty("attributes").GetProperty("fileName").ToString();
            }
            {
                return data.GetProperty("attributes").GetProperty("fileName").ToString();
            }
        }

        public void Dispose()
        {
            MangadexClient.Dispose();
        }
    }
}
