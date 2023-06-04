using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using Tsundoku.Helpers;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Tsundoku.Models.Constants;

namespace Tsundoku.Models
{
	public class Series : IDisposable
	{
		[JsonIgnore] private bool disposedValue;
		[JsonIgnore] public string Synonyms { get; }
		[JsonIgnore] public Bitmap CoverBitMap { get; set; }
		public Dictionary<string, string> Titles { get; }
        public Dictionary<string, string> Staff { get; }
		public string Description { get; }
		public string Format { get; }
		public string Status { get; set; }
		public string Cover { get; set; }
		public string Link { get; }
		public string SeriesNotes { get; set; }
		public ushort MaxVolumeCount { get; set; }
		public ushort CurVolumeCount { get; set; }
		public uint VolumesRead { get; set; }
		public decimal Cost { get; set; }
		public decimal Score { get; set; }
		public string Demographic { get; set; }
		public bool IsFavorite { get; set; } = false;

		public Series(Dictionary<string, string> titles, Dictionary<string, string> staff, string description, string format, string status, string cover, string link, ushort maxVolumeCount, ushort curVolumeCount, decimal score, string demographic)
        {
			Titles = titles;
			Staff = staff;
			Description = description;
            Format = format;
            Status = status;
            Cover = cover;
            Link = link;
            MaxVolumeCount = maxVolumeCount;
            CurVolumeCount = curVolumeCount;
			Score = score;
			Demographic = demographic;
        }

		/// <summary>
		/// All Chinese or Taiwanese series use "Chinese (Simplified)" and go to "Chinese"
		/// </summary>
		/// <param name="title"></param>
		/// <param name="bookType"></param>
		/// <param name="maxVolCount"></param>
		/// <param name="minVolCount"></param>
		/// <param name="ALQuery"></param>
		/// <param name="MD_Query"></param>
		/// <param name="additionalLanguages"></param>
		/// <returns></returns>
		public static async Task<Series?> CreateNewSeriesCard(string title, string bookType, ushort maxVolCount, ushort minVolCount, Helpers.AniListQuery ALQuery, MangadexQuery MD_Query, ObservableCollection<string> additionalLanguages)
        {
			JsonDocument? seriesDataDoc;
			int pageNum = 1;
			if (int.TryParse(title, out int seriesId))
			{
				seriesDataDoc = await ALQuery.GetSeriesByIDAsync(seriesId, bookType, pageNum);
			}
			else
			{
				seriesDataDoc = await ALQuery.GetSeriesByTitleAsync(title, bookType, pageNum);
			}

			string countryOfOrigin = "", nativeTitle = "", japaneseTitle = "", romajiTitle = "", englishTitle = "", filteredBookType = "", nativeStaff = "", fullStaff = "", coverPath = "";
			JsonDocument seriesJson;
			List<JsonElement> mangaDexAltTitles = new List<JsonElement>();
			Dictionary<string, string> newTitles = new Dictionary<string, string>();

			// AniList Query Check
			Restart:
			if (seriesDataDoc != null)
			{
				Constants.Logger.Debug("AniList Query");
				JsonElement seriesData = seriesDataDoc.RootElement.GetProperty("Media");
				nativeTitle = seriesData.GetProperty("title").GetProperty("native").ToString();
				romajiTitle = seriesData.GetProperty("title").GetProperty("romaji").ToString();
				englishTitle = seriesData.GetProperty("title").GetProperty("english").ToString();
				
				if (!nativeTitle.Equals(title, StringComparison.OrdinalIgnoreCase) && !englishTitle.Equals(title, StringComparison.OrdinalIgnoreCase) && !romajiTitle.Equals(title, StringComparison.OrdinalIgnoreCase))
				{
					Constants.Logger.Info("Not on AniList or Incorrect Entry -> Trying Mangadex");
					seriesDataDoc = null;
					goto Restart;
				}

				bool hasNextPage = seriesData.GetProperty("staff").GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean();
				countryOfOrigin = seriesData.GetProperty("countryOfOrigin").ToString();
				filteredBookType = bookType.Equals("MANGA") ? GetCorrectComicName(countryOfOrigin) : "Novel";
				nativeStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "native", filteredBookType, romajiTitle, new StringBuilder());
				fullStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "full", filteredBookType, romajiTitle, new StringBuilder());
				coverPath = SaveNewCoverImage(CreateCoverFilePath(seriesData.GetProperty("coverImage").GetProperty("extraLarge").ToString(), romajiTitle, filteredBookType.ToUpper(), seriesData.GetProperty("synonyms").EnumerateArray().ToList(), Constants.Site.AniList), seriesData.GetProperty("coverImage").GetProperty("extraLarge").ToString());

				// If available on AniList and is not a Japanese series get the Japanese title from Mangadex
				if (!bookType.Equals("NOVEL") && (countryOfOrigin.Equals("KR") || countryOfOrigin.Equals("CW") || countryOfOrigin.Equals("TW")))
				{
					mangaDexAltTitles = MangadexQuery.GetAdditionalMangaDexTitleList((await MD_Query.GetSeriesByTitleAsync(romajiTitle)).RootElement.GetProperty("data"));
					japaneseTitle = GetAltTitle("ja", mangaDexAltTitles);
				}

				// Loop while there are still staff to check
				while(hasNextPage)
				{
					Constants.Logger.Info($"{romajiTitle} has More Staff");
					JsonDocument? moreStaffQuery;
					if (int.TryParse(title, out seriesId))
					{
						moreStaffQuery = await ALQuery.GetSeriesByIDAsync(seriesId, bookType, ++pageNum);
					}
					else
					{
						moreStaffQuery = await ALQuery.GetSeriesByTitleAsync(title, bookType, ++pageNum);
					}
					JsonElement moreStaff = moreStaffQuery.RootElement.GetProperty("Media").GetProperty("staff");
					nativeStaff = GetSeriesStaff(moreStaff.GetProperty("edges"), "native", filteredBookType, romajiTitle, new StringBuilder(nativeStaff + " | "));
					fullStaff = GetSeriesStaff(moreStaff.GetProperty("edges"), "full", filteredBookType, romajiTitle, new StringBuilder(fullStaff + " | "));
					hasNextPage = moreStaff.GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean();
				}


				newTitles = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
				if (!string.IsNullOrWhiteSpace(romajiTitle))
				{
					newTitles.Add("Romaji", romajiTitle);
				}
				if (!string.IsNullOrWhiteSpace(englishTitle))
				{
					newTitles.Add("English", englishTitle);
				}
				if (!string.IsNullOrWhiteSpace(japaneseTitle))
				{
					newTitles.Add("Japanese", japaneseTitle);
				}
				if (!string.IsNullOrWhiteSpace(nativeTitle))
				{
					newTitles.Add(Constants.ANILIST_LANG_CODES[countryOfOrigin], nativeTitle);
				}

				if (additionalLanguages.Count != 0)
				{
					if (mangaDexAltTitles.Count == 0)
					{
						AddAdditionalLanguages(newTitles, additionalLanguages, romajiTitle, MangadexQuery.GetAdditionalMangaDexTitleList((await MD_Query.GetSeriesByTitleAsync(romajiTitle)).RootElement.GetProperty("data")));
					}
					else
					{
						AddAdditionalLanguages(newTitles, additionalLanguages, romajiTitle, mangaDexAltTitles);
					}
				}

				Dictionary<string, string> newStaff = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
				if (!string.IsNullOrWhiteSpace(fullStaff))
				{
					newStaff.Add("Romaji", fullStaff);
				}
				if (nativeStaff != " | " && !string.IsNullOrWhiteSpace(nativeStaff))
				{
					newStaff.Add(Constants.ANILIST_LANG_CODES[countryOfOrigin], nativeStaff);
				}
				
				return new Series(
					newTitles,
					newStaff,
					AniListQuery.ParseAniListDescription(seriesData.GetProperty("description").ToString()),
					filteredBookType,
					GetSeriesStatus(seriesData.GetProperty("status").ToString()),
					coverPath,
					seriesData.GetProperty("siteUrl").ToString(),
					maxVolCount,
					minVolCount,
					-1,
					""
				);
			}
			else if (bookType.Equals("MANGA")) // MangadexQuery
			{
				string curId = "";
				bool notFoundCondition = false;
				if (Regex.IsMatch(title, @"[a-z\d]{8}-[a-z\d]{4}-[a-z\d]{4}-[a-z\d]{4}-[a-z\d]{11,}"))
				{
					seriesJson = await MD_Query.GetSeriesByIdAsync(title);
					curId = title;
				}
				else
				{
					seriesJson = await MD_Query.GetSeriesByTitleAsync(title);
				}

				if (!string.IsNullOrWhiteSpace(seriesJson.ToString()))
				{
					JsonElement data = seriesJson.RootElement.GetProperty("data");
					JsonElement attributes = data;
					string description = "", seriesStatus = "", link = "", demographic = "";
					List<JsonElement> altTitles = null, relationships = null;

					if (data.ValueKind == JsonValueKind.Array)
					{
						foreach (JsonElement series in data.EnumerateArray())
						{
							attributes = series.GetProperty("attributes");
							if (attributes.GetProperty("title").GetProperty("en").ToString().Equals(title, StringComparison.OrdinalIgnoreCase) || data.GetArrayLength() == 1)
							{
								data = series;
								notFoundCondition = false;
								break;
							}
							else
							{
								notFoundCondition = true;
							}
						}
					}
					else
					{
						attributes = data.GetProperty("attributes");
					}

					if(notFoundCondition)
					{
						Constants.Logger.Warn("User Input Invalid Series Title or ID or Can't Determine Series Needs to be more Specific");
						return null;
					}

					altTitles = attributes.GetProperty("altTitles").EnumerateArray().ToList();
					relationships = data.GetProperty("relationships").EnumerateArray().ToList();
					curId = string.IsNullOrWhiteSpace(curId) ? data.GetProperty("id").ToString() : curId;
					romajiTitle = attributes.GetProperty("title").GetProperty("en").ToString();
					description = MangadexQuery.ParseMangadexDescription(attributes.GetProperty("description").GetProperty("en").ToString());
					seriesStatus = GetSeriesStatus(attributes.GetProperty("status").ToString());
					link = @$"https://mangadex.org/title/{curId}";
					countryOfOrigin = attributes.GetProperty("originalLanguage").ToString();
					string coverLink = @$"https://uploads.mangadex.org/covers/{curId}/{(await MD_Query.GetCover(relationships.Single(x => x.GetProperty("type").ToString().Equals("cover_art")).GetProperty("id").ToString()))}";
					Constants.Logger.Debug(coverLink);
					coverPath = SaveNewCoverImage(CreateCoverFilePath(coverLink, romajiTitle, bookType, altTitles, Constants.Site.MangaDex), coverLink);
					demographic = attributes.GetProperty("publicationDemographic").ToString();
					if (!string.IsNullOrWhiteSpace(demographic))
					{
						demographic = $"{Char.ToUpper(demographic[0])}{demographic.Substring(1)}";
					}

					if (altTitles.Count == 0)
					{
						Constants.Logger.Warn("Series has no Alternate Titles");
					}
					
					newTitles = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
					{
						{ "Romaji", romajiTitle },
						{ "English", !countryOfOrigin.Equals("en") ? GetAltTitle("en", altTitles) : romajiTitle }
					};

					// Get the Japanese title if the series is not a Japanese Comic
					if (!countryOfOrigin.Equals("ja"))
					{
						newTitles.Add("Japanese", GetAltTitle("ja", altTitles));
					}

					// Check to see if the user wants additional languages
					if (additionalLanguages.Count != 0)
					{
						AddAdditionalLanguages(newTitles, additionalLanguages, romajiTitle, altTitles);
					}

					// Get the staff for a series
					string staffName = "";
					foreach(JsonElement staff in relationships.Where(staff => staff.GetProperty("type").ToString().Equals("author") || staff.GetProperty("type").ToString().Equals("artist")))
					{
						staffName = await MD_Query.GetAuthor(staff.GetProperty("id").ToString());
						if (Regex.IsMatch(staffName, @"\(.*\)"))
						{
							string native = Regex.Match(staffName, @"\((.*)\)").Groups[1].ToString();
							string full = Regex.Match(staffName, @"^.*(?= \(.*\))").Value;
							if (fullStaff.Contains(full, StringComparison.OrdinalIgnoreCase) || nativeStaff.Contains(native, StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}
							nativeStaff += native + " | ";
							fullStaff += full + " | ";
						}
						else if (!fullStaff.Contains(staffName, StringComparison.OrdinalIgnoreCase))
						{
							fullStaff += staffName + " | ";
						}
					}
					nativeStaff = !string.IsNullOrWhiteSpace(nativeStaff) ? nativeStaff.Substring(0, nativeStaff.Length - 3) : "";
					fullStaff = fullStaff.Substring(0, fullStaff.Length - 3);

					Dictionary<string, string> newStaff = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
					{
						{ "Romaji", fullStaff }
					};

					if (!countryOfOrigin.Equals("en"))
					{
						newTitles.Add(Constants.MANGADEX_LANG_CODES[countryOfOrigin], GetAltTitle(countryOfOrigin, altTitles));
						newStaff.Add(Constants.MANGADEX_LANG_CODES[countryOfOrigin], nativeStaff);
					}

					// Remove empty titles
					foreach (var x in newTitles)
					{
						// Constants.Logger.Debug(x.Key + " | " + x.Value);
						if (string.IsNullOrWhiteSpace(x.Value))
						{
							newTitles.Remove(x.Key);
						}
					}

					// Remove empty titles
					foreach (var x in newStaff)
					{
						// Constants.Logger.Debug(x.Key + " | " + x.Value);
						if (string.IsNullOrWhiteSpace(x.Value))
						{
							newStaff.Remove(x.Key);
						}
					}

					return new Series(
						newTitles,
						newStaff,
						description,
						GetCorrectComicName(countryOfOrigin),
						seriesStatus,
						coverPath,
						link,
						maxVolCount,
						minVolCount,
						-1,
						demographic
					);
				}
			}

			Constants.Logger.Warn("User Input Invalid Series Title or ID or Can't Determine Series Needs to be more Specific");
			return null;
        }

		private static void AddAdditionalLanguages(Dictionary<string, string> newTitles, ObservableCollection<string> additionalLanguages, string romajiTitle, List<JsonElement> altTitles)
		{
			JsonElement foundTitle;
			foreach (string language in additionalLanguages)
			{
				Restart:
				if (language.Equals("Chinese") || language.Equals("Spanish") || language.Equals("Portugese")) // If a language has multiple codes need to loop again
				{
					var mangaDexLang = Constants.MANGADEX_LANG_CODES.Where(lang => lang.Value.Equals(language)).Select(lang => lang.Key);
					for (int x = 0; x < altTitles.Count; x++)
					{
						if (!newTitles.ContainsKey(language))
						{
							foreach (string lang in mangaDexLang)
							{
								if (altTitles[x].TryGetProperty(lang, out foundTitle))
								{
									// Constants.Logger.Debug($"Added [{language} : {foundTitle.ToString()}]");
									newTitles.Add(language, foundTitle.ToString());
									goto Restart;
								}
							}
						}
					}
				}
				else
				{
					var mangaDexLang = Constants.MANGADEX_LANG_CODES.FirstOrDefault(lang => lang.Value.Equals(language)).Key;
					for (int x = 0; x < altTitles.Count; x++)
					{
						if (!newTitles.ContainsKey(language) && altTitles[x].TryGetProperty(mangaDexLang, out foundTitle))
						{
							// Constants.Logger.Debug($"Added [{language} : {foundTitle.ToString()}]");
							newTitles.Add(language, foundTitle.ToString());
							break;
						}
					}
				}
			}
		}

		public static string GetAltTitle(string country, List<JsonElement> altTitles)
		{
			JsonElement altTitle;
			foreach (JsonElement titles in altTitles)
			{
				if (titles.TryGetProperty(country, out altTitle))
				{
					return altTitle.ToString();
				}
			}
			return "";
		}

		public static string GetCorrectComicName(string jsonCountryOfOrigin)
		{
			return jsonCountryOfOrigin switch
			{
				"KR" or "ko" => "Manhwa",
				"CN" or "TW" or "zh" or "zh-hk"=> "Manhua",
				"FR" or "fr" => "Manfra",
				"EN" or "en" => "Comic",
				_ or "JP" or "ja" => "Manga"
			};
		}

		public static String GetSeriesStatus(String jsonStatus)
		{
			return jsonStatus switch
			{
				"RELEASING" or "NOT_YET_RELEASED" or "ongoing" => "Ongoing",
				"FINISHED"  or "completed" => "Finished",
				"CANCELLED" or "cancelled" => "Cancelled",
				"HIATUS" or "hiatus" => "Hiatus",
				_  => "Error"
			};
		}

        public static string GetSeriesStaff(JsonElement staffArray, string nameType, string bookType, string title, StringBuilder staffList) {
			string[] validRoles = { "Story & Art", "Story", "Art", "Original Creator", "Character Design", "Cover Illustration", "Illustration", "Mechanical Design", "Original Story", "Original Character Design", "Original Story"};
			foreach(JsonElement name in staffArray.EnumerateArray())
            {
				string staffRole = Regex.Replace(name.GetProperty("role").ToString(), @" \(.*\)", "").Trim();

				// Don't include "Illustration" staff for manga that are not anthologies
				if (validRoles.Contains(staffRole, StringComparer.OrdinalIgnoreCase) && !(bookType.Equals("Manga") && (staffRole.Equals("Illustration") || staffRole.Equals("Cover Illustration")) && !title.Contains("Anthology")))
                {
					if (name.GetProperty("node").GetProperty("name").GetProperty("native").ValueKind == JsonValueKind.Null && name.GetProperty("node").GetProperty("name").GetProperty("full").ValueKind == JsonValueKind.Null)
					{
						staffList.Append("Error" + " | ");
					}
					else
					{
						string newStaff = name.GetProperty("node").GetProperty("name").GetProperty(nameType).ToString().Trim();
						string newStaffOther = name.GetProperty("node").GetProperty("name").GetProperty(nameType.Equals("native") ? "full" : "native").ToString().Trim();
						bool hasAlternatives = (name.GetProperty("node").GetProperty("name").GetProperty("alternative").GetArrayLength() > 0);
						if (!staffList.ToString().Contains(newStaff) || string.IsNullOrWhiteSpace(newStaff)) // Check to see if this staff member has multiple roles to only add them once
						{
							if (!string.IsNullOrWhiteSpace(newStaff))
							{
								staffList.Append(newStaff + " | ");
							}
							else if (!string.IsNullOrWhiteSpace(newStaffOther))
							{
								staffList.Append(newStaffOther + " | ");
							}
							else if (hasAlternatives) // If the staff member does not have a full name entry
							{
								staffList.Append(name.GetProperty("node").GetProperty("name").GetProperty("alternative")[0].ToString().Trim() + " | ");
							}
						}
						else
						{
							Constants.Logger.Info($"Duplicate Staff Entry For {newStaff}");
						}
					}
                }
            }
			return staffList.ToString(0, staffList.Length - 3);
		}

		public static string CreateCoverFilePath(string coverLink, string title, string bookType, List<JsonElement> synonyms, Site queryType)
		{
			string newPath = @$"Covers\{Helpers.ExtensionMethods.RemoveInPlaceCharArray(string.Concat(title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))).Replace(",", "")}_{bookType}.{coverLink.Substring(coverLink.Length - 3)}";
            Directory.CreateDirectory(@"Covers");

			// Check and see if the series will generate a duplicate file name
            if (File.Exists(newPath))
            {
                switch (queryType)
				{
					case Site.AniList:
						foreach (JsonElement newTitle in synonyms)
						{
							if (Regex.IsMatch(newTitle.ToString(), @"[\w]"))
							{
								newPath = @$"Covers\{Helpers.ExtensionMethods.RemoveInPlaceCharArray(string.Concat(newTitle.ToString().Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))).Replace(",", "")}_{bookType}.{coverLink.Substring(coverLink.Length - 3)}";
								break;
							}
						}
						break;
					case Site.MangaDex:
						Constants.Logger.Debug("Check #1");
						JsonElement altTitle;
						foreach (JsonElement newTitle in synonyms)
						{
							if (newTitle.TryGetProperty("en", out altTitle))
							{
								Constants.Logger.Debug(coverLink);
								newPath = @$"Covers\{Helpers.ExtensionMethods.RemoveInPlaceCharArray(string.Concat(altTitle.ToString().Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))).Replace(",", "")}_{bookType}.{coverLink.Substring(coverLink.Length - 3)}";
								break;
							}
						}
						break;
				}
            }
			return newPath;
		}
		public static string SaveNewCoverImage(string newPath, string coverLink)
        {
            try
            {
                HttpClient client = new HttpClient();
                Task<HttpResponseMessage> response = Task.Run(async () => await client.GetAsync(new Uri(coverLink)));
                response.Wait();
                HttpResponseMessage clientResponse = response.Result;
                using (FileStream fs = new FileStream(newPath, FileMode.Create, FileAccess.Write))
                {
                    Task fileResponse = Task.Run(async () => await clientResponse.Content.CopyToAsync(fs));
                    fileResponse.Wait();
                }
            }
            catch (Exception e)
            {
                Constants.Logger.Debug(e.ToString());
            }
			return newPath;
        }

		public string ToJsonString(JsonSerializerOptions options)
		{
			return JsonSerializer.Serialize(this, options);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// dispose managed state (managed objects)
					CoverBitMap.Dispose();
				}

				// free unmanaged resources (unmanaged objects) and override finalizer
				// set large fields to null
				disposedValue = true;
			}
		}

		// Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~Series()
		// {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

    class SeriesComparer : IComparer<Series>
    {
		string curLang;
        System.Globalization.CultureInfo curCul;

        public SeriesComparer(string curLang)
        {
            this.curLang = curLang;
			curCul = System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(Constants.CULTURE_LANG_CODES[curLang], false);
        }
        public int Compare(Series? x, Series? y)
        {
            if (!x.Titles.ContainsKey(curLang) && !y.Titles.ContainsKey(curLang))
			{
				return x.Titles["Romaji"].CompareTo(y.Titles["Romaji"]);
			}
			else if (x.Titles.ContainsKey(curLang) && !y.Titles.ContainsKey(curLang))
			{
				return x.Titles[curLang].CompareTo(y.Titles["Romaji"]);
			}
			else if (!x.Titles.ContainsKey(curLang) && y.Titles.ContainsKey(curLang))
			{
				return x.Titles["Romaji"].CompareTo(y.Titles[curLang]);
			}
			else
			{
				return x.Titles[curLang].CompareTo(y.Titles[curLang]);
			}
        }
    }
}
