using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// TODO Add rate limiting
namespace Tsundoku.Helpers
{
    public partial class AniListQuery : IDisposable
	{
		private static readonly string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.66 Safari/537.36";
		private static GraphQLHttpClient AniListClient = new GraphQLHttpClient("https://graphql.anilist.co", new SystemTextJsonSerializer());
        private bool disposedValue;
        [GeneratedRegex("\\(Source: [\\S\\s]+|\\<.*?\\>")] private static partial Regex AniListDescRegex();

		static AniListQuery()
		{
			AniListClient.HttpClient.DefaultRequestHeaders.Add("RequestType", "POST");
			AniListClient.HttpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
			AniListClient.HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			AniListClient.HttpClient.DefaultRequestHeaders.Add("UserAgent", USER_AGENT);
		}

		/// <summary>
		/// Gets a AniList series by a title
		/// </summary>
		/// <param name="seriesId">The title of the series as a string, this can be any title including ones under synonyms</param>
		/// <param name="format">The format of the series either manga or light novel</param>
		/// <param name="pageNum">The current page number of the GraphQL query</param>
		/// <returns></returns>
        public static async Task<JsonDocument?> GetSeriesByTitleAsync(string title, string format, int pageNum)
		{
			try
			{
				GraphQLRequest queryRequest = new()
				{
					Query = @"
						query ($title: String, $format: MediaFormat, $pageNum: Int) {
						  Media(search: $title, format: $format) {
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
						title,
						format,
						pageNum
					}
				};
				var response = await AniListClient.SendQueryAsync<JsonDocument?>(queryRequest);
				return response.Data;
			}
			catch(Exception e)
			{
				LOGGER.Error($"AniList GetSeriesByTitle w/ {title} Request Failed {e.Message}");
			}
			return null;
		}

		/// <summary>
		/// Gets a AniList Series by its ID
		/// </summary>
		/// <param name="seriesId">The id of the series as a integer, should be 6 digits long</param>
		/// <param name="format">The format of the series either manga or light novel</param>
		/// <param name="pageNum">The current page number of the GraphQL query</param>
		/// <returns></returns>
		public static async Task<JsonDocument?> GetSeriesByIDAsync(int seriesId, string format, int pageNum)
		{
			try
			{
				GraphQLRequest queryRequest = new()
				{
					Query = @"
						query ($seriesId: Int, $format: MediaFormat, $pageNum: Int) {
						  Media(id: $seriesId, format: $format) {
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
                        seriesId,
						format,
                        pageNum
                    }
				};

				var response = await AniListClient.SendQueryAsync<JsonDocument?>(queryRequest);
				return response.Data;
			}
			catch(Exception e)
			{
				LOGGER.Error($"AniList GetSeriesById w/ {seriesId} Request Failed {e.Message}");
			}
			return null;
		}

		public static string ParseAniListDescription(string seriesDescription)
		{
			return string.IsNullOrWhiteSpace(seriesDescription) ? "" : System.Web.HttpUtility.HtmlDecode(AniListDescRegex().Replace(new StringBuilder(seriesDescription).Replace("\n<br><br>\n", "\n\n").Replace("<br><br>\n\n", "\n\n").Replace("<br><br>", "\n").ToString(), "").Trim().TrimEnd('\n'));
		}

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null
				AniListClient.Dispose();
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AniListQuery()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}