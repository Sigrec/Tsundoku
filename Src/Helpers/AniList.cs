using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Helpers
{
    public partial class AniList : IDisposable
	{
		private static readonly GraphQLHttpClient AniListClient;
        private bool disposedValue;
        [GeneratedRegex(@"\(Source: [\S\s]+|\<.*?\>|Note:.*")] private static partial Regex AniListDescRegex();
        [GeneratedRegex(@" \(.*\)")] public static partial Regex StaffRegex();

		static AniList()
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
                              genres
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
                              genres
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

        public static HashSet<Genre> ParseGenreArray(string romajiTitle, JsonElement genres)
        {
            if (genres.ValueKind != JsonValueKind.Null)
            {
                HashSet<Genre> newGenres = [];
                foreach (JsonElement genreElement in genres.EnumerateArray())
                {
                    Genre genre = GenreExtensions.GetGenreFromString(genreElement.GetString());
                    if (genre != Genre.None)
                    {
                        newGenres.Add(genre);
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
        
        public static void ExtractTitlesFromAniList(JsonElement seriesData, ref Dictionary<string, string> newTitles)
        {
            string nativeTitle = seriesData.GetProperty("title").GetProperty("native").GetString();
            string romajiTitle = seriesData.GetProperty("title").GetProperty("romaji").GetString();
            string englishTitle = seriesData.GetProperty("title").GetProperty("english").GetString();

            newTitles.Add("Romaji", romajiTitle);
            newTitles.Add("English", englishTitle);
            newTitles.Add("Japanese", nativeTitle);
        }

        public static void ExtractStaffFromAniList(JsonElement seriesData, ref string nativeStaff, ref string fullStaff)
        {
            nativeStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "native", Format.Manga, "", new StringBuilder());
            fullStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "full", Format.Manga, "", new StringBuilder());
        }
        
        public static string GetSeriesStaff(JsonElement staffArray, string nameType, Format bookType, string title, StringBuilder staffList) {
			foreach(JsonElement name in staffArray.EnumerateArray())
            {
				string staffRole = StaffRegex().Replace(name.GetProperty("role").GetString(), string.Empty).Trim();
				JsonElement nameProperty = name.GetProperty("node").GetProperty("name");

				// Don't include "Illustration" staff for manga that are not anthologies
				if (
					VALID_STAFF_ROLES.Contains(staffRole, StringComparer.OrdinalIgnoreCase) 
					&& !(bookType != Format.Novel 
                        && (
                                staffRole.Equals("Illustration") 
                                || staffRole.Equals("Cover Illustration")
                            ) 
                        && !title.Contains("Anthology")))
                {
					string newStaff = nameProperty.GetProperty(nameType).GetString();
					string newStaffOther = nameProperty.GetProperty(nameType.Equals("native") ? "full" : "native").GetString();
					if (string.IsNullOrWhiteSpace(newStaff) || !staffList.ToString().Contains(newStaff)) // Check to see if this staff member has multiple roles to only add them once
					{
						if (!string.IsNullOrWhiteSpace(newStaff))
						{
							staffList.AppendFormat("{0} | ", newStaff.Trim());
						}
						else if (!string.IsNullOrWhiteSpace(newStaffOther))
						{
							staffList.AppendFormat("{0} | ", newStaffOther.Trim());
						}
						else if (nameProperty.GetProperty("alternative").GetArrayLength() > 0) // If the staff member does not have a full or native name entry
						{
							staffList.AppendFormat("{0} | ", nameProperty.GetProperty("alternative")[0].GetString().Trim());
						}
					}
					else
					{
						LOGGER.Info($"Duplicate Staff Entry For {newStaff}");
					}
                }
            }
			return staffList.ToString(0, staffList.Length - 3);
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

        ~AniList()
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