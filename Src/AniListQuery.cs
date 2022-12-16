using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tsundoku.Source
{
	internal class AniListQuery
	{
		private static readonly string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.66 Safari/537.36";

		private static GraphQLHttpClient AniListClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());


		public AniListQuery()
		{
			
		}

        public string GetSeries(string title, string format)
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
						query ($title: String, $type: MediaFormat) {
						  Media(search: $title, format: $type, sort: TITLE_NATIVE) {
							countryOfOrigin
							title {
							  romaji
							  english
							  native
							}
							synonyms
							staff(sort: RELEVANCE) {
							  edges {
								role
								node {
								  name {
									full
									native
								  }
								}
							  }
							}
							description
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
						type = format
					}
				};
				var response = Task.Run(async () => await AniListClient.SendQueryAsync<JObject?>(queryRequest));
				response.Wait();
				return response.Result.Data.ToString();
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.ToString());
			}

			return "";
		}
	}
}