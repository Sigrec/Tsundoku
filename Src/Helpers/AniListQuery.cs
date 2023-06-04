using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tsundoku.Models;

namespace Tsundoku.Helpers
{
	public class AniListQuery
	{
		private static readonly string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.66 Safari/537.36";

		private static GraphQLHttpClient AniListClient = new GraphQLHttpClient("https://graphql.anilist.co", new SystemTextJsonSerializer());

		public void Dispose()
		{
			AniListClient.Dispose();
		}

        public async Task<JsonDocument?> GetSeriesByTitleAsync(string title, string format, int pageNum)
		{
			try
			{
				AniListClient.HttpClient.DefaultRequestHeaders.Add("RequestType", "POST");
				AniListClient.HttpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
				AniListClient.HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
				AniListClient.HttpClient.DefaultRequestHeaders.Add("UserAgent", USER_AGENT);

				GraphQLRequest queryRequest = new()
				{
					Query = @"
						query ($title: String, $type: MediaFormat, $pageNum: Int) {
						  Media(search: $title, format: $type) {
							  countryOfOrigin
							  title {
							    romaji
							    english
							    native
							  }
							  synonyms
							  staff(sort: RELEVANCE, perPage: 25, page: $pageNum) {
							    pageInfo {
								  hasNextPage
							    }
							    edges {
								  role
								  node {
								    name {
									  full
									  native
									  alternative
								    }
								  }
							    }
							  }
							  description(asHtml: false)
							  status(version: 2)
							  siteUrl
							  coverImage {
							    extraLarge
							  }
						    }
						  }",
					Variables = new
					{
						title = title,
						type = format,
						pageNum = pageNum
					}
				};
				var response = await AniListClient.SendQueryAsync<JsonDocument?>(queryRequest);
				return response.Data;
			}
			catch(Exception e)
			{
				Constants.Logger.Warn($"{title} Request Failed {e.ToString()}");
			}
			return null;
		}

		public async Task<JsonDocument?> GetSeriesByIDAsync(int seriesId, string format, int pageNum)
		{
			try
			{
				AniListClient.HttpClient.DefaultRequestHeaders.Add("RequestType", "POST");
				AniListClient.HttpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
				AniListClient.HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
				AniListClient.HttpClient.DefaultRequestHeaders.Add("UserAgent", USER_AGENT);

				GraphQLRequest queryRequest = new()
				{
					Query = @"
						query ($seriesId: Int, $type: MediaFormat, $pageNum: Int) {
						  Media(id: $seriesId, format: $type) {
							  countryOfOrigin
							  title {
							    romaji
							    english
							    native
							  }
							  synonyms
							  staff(sort: RELEVANCE, perPage: 25, page: $pageNum) {
							    pageInfo {
								  hasNextPage
							    }
							    edges {
								  role
								  node {
								    name {
									  full
									  native
									  alternative
								    }
								  }
							    }
							  }
							  description(asHtml: false)
							  status(version: 2)
							  siteUrl
							  coverImage {
							    extraLarge
							  }
						    }
						  }",
					Variables = new
					{
						seriesId = seriesId,
						type = format,
						pageNum = pageNum
					}
				};

				var response = await AniListClient.SendQueryAsync<JsonDocument?>(queryRequest);
				return response.Data;
			}
			catch(Exception e)
			{
				Constants.Logger.Warn($"{seriesId} Request Failed {e.ToString()}");
			}
			return null;
		}

		public static string ParseAniListDescription(string seriesDescription)
		{
			return string.IsNullOrWhiteSpace(seriesDescription) ? "" : System.Web.HttpUtility.HtmlDecode(Regex.Replace(new StringBuilder(seriesDescription).Replace("\n<br><br>\n", "\n\n").Replace("<br><br>\n\n", "\n\n").Replace("<br><br>", "\n").ToString(), @"\(Source: [\S\s]+|\<.*?\>", "").Trim().TrimEnd('\n'));
		}
	}
}