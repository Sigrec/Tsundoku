using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Tsundoku.ViewModels;

namespace Tsundoku.Helpers
{
    public partial class AniListQuery : IDisposable
	{
		private static readonly GraphQLHttpClient AniListClient;
        private bool disposedValue;
        [GeneratedRegex(@"\(Source: [\S\s]+|\<.*?\>")] private static partial Regex AniListDescRegex();

		static AniListQuery()
		{
            AniListClient = new GraphQLHttpClient("https://graphql.anilist.co", new SystemTextJsonSerializer());
			AniListClient.HttpClient.DefaultRequestHeaders.Add("RequestType", "POST");
			AniListClient.HttpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
			AniListClient.HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			AniListClient.HttpClient.DefaultRequestHeaders.Add("UserAgent", ViewModelBase.USER_AGENT);
		}


		/// <summary>
		/// Gets a AniList series by a title
		/// </summary>
		/// <param name="seriesId">The title of the series as a string, this can be any title including ones under synonyms</param>
		/// <param name="format">The format of the series either manga or light novel</param>
		/// <param name="pageNum">The current page number of the GraphQL query</param>
		/// <returns></returns>
        public static async Task<JsonDocument?> GetSeriesByTitleAsync(string title, Format format, int pageNum)
		{
			try
			{
				GraphQLRequest queryRequest = new()
				{
					Query = @"
						query ($title: String, $format: MediaFormat, $pageNum: Int) {
						  Media(search: $title, format: $format) {
                              id
							  countryOfOrigin
							  title {
							    romaji
							    english
							    native
							  }
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
                GraphQLResponse<JsonDocument?> response = await AniListClient.SendQueryAsync<JsonDocument?>(queryRequest);
                short rateCheck = RateLimitCheck(response.AsGraphQLHttpResponse().ResponseHeaders);
				if (rateCheck != -1)
                {
                    LOGGER.Info($"Waiting {rateCheck} Seconds for Rate Limit To Reset");
                    await Task.Delay(TimeSpan.FromSeconds(rateCheck));
                    response = await AniListClient.SendQueryAsync<JsonDocument?>(queryRequest);
                }
                return response.Data;
			
			}
			catch(Exception e)
			{
				LOGGER.Error("AniList GetSeriesByTitle w/ {} Request Failed -> {}", title, e.Message);
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
		public static async Task<JsonDocument?> GetSeriesByIDAsync(int seriesId, Format format, int pageNum)
		{
			try
			{
				GraphQLRequest queryRequest = new()
				{
					Query = @"
						query ($seriesId: Int, $format: MediaFormat, $pageNum: Int) {
						  Media(id: $seriesId, format: $format) {
                              id
							  countryOfOrigin
							  title {
							    romaji
							    english
							    native
							  }
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

				GraphQLResponse<JsonDocument?> response = await AniListClient.SendQueryAsync<JsonDocument?>(queryRequest);
                short rateCheck = RateLimitCheck(response.AsGraphQLHttpResponse().ResponseHeaders);
				if (rateCheck != -1)
                {
                    LOGGER.Info($"Waiting {rateCheck} Seconds for Rate Limit To Reset");
                    await Task.Delay(TimeSpan.FromSeconds(rateCheck));
                    response = await AniListClient.SendQueryAsync<JsonDocument?>(queryRequest);
                }
                return response.Data;
			}
			catch(Exception e)
			{
				LOGGER.Error("AniList GetSeriesById w/ {} Request Failed -> {}", seriesId, e.Message);
			}
			return null;
		}

        private static short RateLimitCheck(HttpResponseHeaders responseHeaders)
        {
            responseHeaders.TryGetValues("X-RateLimit-Remaining", out var rateRemainingValues);
            _ = short.TryParse(rateRemainingValues?.FirstOrDefault(), out var rateRemaining);
            LOGGER.Info($"AniList Rate Remaining = {rateRemaining}");
            if (rateRemaining > 0)
            {
                return -1;
            }
            else
            {
                responseHeaders.TryGetValues("Retry-After", out var retryAfter);
                _ = short.TryParse(retryAfter?.FirstOrDefault(), out var retryAfterInSeconds);
                return retryAfterInSeconds;
            }
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

        ~AniListQuery()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}