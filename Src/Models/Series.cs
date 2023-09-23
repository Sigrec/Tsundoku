using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using Tsundoku.Helpers;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System;
using System.IO;
using MangaLightNovelWebScrape;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices;

namespace Tsundoku.Models
{
    public partial class Series : IDisposable
	{
		[GeneratedRegex("[\\w]")] private static partial Regex AlphaNumericOnlyRegex();
        [GeneratedRegex(" \\(.*\\)")] private static partial Regex StaffRegex();
        [GeneratedRegex("\\((.*)\\)")] private static partial Regex NativeStaffRegex();
        [GeneratedRegex("^.*(?= \\(.*\\))")] private static partial Regex FullStaffRegex();
        [GeneratedRegex("\\(.*\\)")] private static partial Regex FindStaffRegex();
		[GeneratedRegex("[a-z\\d]{8}-[a-z\\d]{4}-[a-z\\d]{4}-[a-z\\d]{4}-[a-z\\d]{11,}")] private static partial Regex MangaDexIDRegex();

		[JsonIgnore] private bool disposedValue;
		[JsonIgnore] public string Synonyms { get; }
		[JsonIgnore] public Bitmap CoverBitMap { get; set; }
		public Dictionary<string, string> Titles { get; }
        public Dictionary<string, string> Staff { get; }
		public string Description { get; }

		/// <summary>
		/// The format of the series (Manga, Manhwa, Manhua, Manfra, Comic, or Novel)
		/// </summary>
		public string Format { get; }

		/// <summary>
		/// The serialization status of the series (Finished, Ongoing, Hiatus, Cancelled, or Error)
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		/// Path to the cover for a series
		/// </summary>
		public string Cover { get; set; }
		public string Link { get; }
		public string SeriesNotes { get; set; }
		public ushort MaxVolumeCount { get; set; }
		public ushort CurVolumeCount { get; set; }
		public uint VolumesRead { get; set; }
		public decimal Cost { get; set; }
		public decimal Rating { get; set; }
		public string Demographic { get; set; }
		public bool IsFavorite { get; set; } = false;

		public Series(Dictionary<string, string> Titles, Dictionary<string, string> Staff, string description, string format, string status, string cover, string link, ushort maxVolumeCount, ushort curVolumeCount, decimal rating, string demographic)
        {
            this.Titles = Titles;
			this.Staff = Staff;
			Description = description;
            Format = format;
            Status = status;
            Cover = cover;
            Link = link;
            MaxVolumeCount = maxVolumeCount;
            CurVolumeCount = curVolumeCount;
			Rating = rating;
			Demographic = demographic;
        }

		/// <summary>
		/// All Chinese or Taiwanese series use "Chinese (Simplified)" and go to "Chinese"
		/// </summary>
		/// <param name="title">Title or ID of the series the user wants to add to their collection</param>
		/// <param name="bookType">Booktype of the series the user wants to add, either Manga or Light Novel</param>
		/// <param name="maxVolCount">Current max volume count of the series</param>
		/// <param name="minVolCount">Current volume count the user has for the series</param>
		/// <param name="ALQuery">AniListQuery object for the AniList HTTP Client</param>
		/// <param name="MD_Query">MangaDexQuery object for the MangaDex HTTP client</param>
		/// <param name="additionalLanguages">List of additional languages to query for</param>
		/// <returns></returns>
		public static async Task<Series?> CreateNewSeriesCardAsync(string title, string bookType, ushort maxVolCount, ushort minVolCount, ObservableCollection<string> additionalLanguages)
        {
			JsonDocument? seriesDataDoc;
			int pageNum = 1;
			bool isAniListID = false;
			if (int.TryParse(title, out int seriesId))
			{
				seriesDataDoc = await AniListQuery.GetSeriesByIDAsync(seriesId, bookType, pageNum);
				isAniListID = true;
			}
			else
			{
				seriesDataDoc = await AniListQuery.GetSeriesByTitleAsync(title, bookType, pageNum);
			}

			string countryOfOrigin = string.Empty, nativeTitle = string.Empty, japaneseTitle = string.Empty, romajiTitle = string.Empty, englishTitle = string.Empty, filteredBookType = string.Empty, nativeStaff = string.Empty, fullStaff = string.Empty, coverPath = string.Empty;
			JsonDocument seriesJson;
			JsonElement.ArrayEnumerator mangaDexAltTitles = [];
			Dictionary<string, string> newTitles = [];

			// AniList Query Check
			Restart:
			if (isAniListID || seriesDataDoc != null)
			{
				LOGGER.Info("Valid AniList Query");
				JsonElement seriesData = seriesDataDoc.RootElement.GetProperty("Media");
				nativeTitle = seriesData.GetProperty("title").GetProperty("native").GetString();
				romajiTitle = seriesData.GetProperty("title").GetProperty("romaji").GetString();
				englishTitle = seriesData.GetProperty("title").GetProperty("english").GetString();
				LOGGER.Info("Check");
				if (!isAniListID 
						&& !(
								EntryModel.Similar(title, englishTitle, !string.IsNullOrWhiteSpace(englishTitle) && title.Length > englishTitle.Length ? englishTitle.Length / 6 : title.Length / 6) != -1 
								|| EntryModel.Similar(title, romajiTitle, !string.IsNullOrWhiteSpace(romajiTitle) && title.Length > romajiTitle.Length ? romajiTitle.Length / 6 : title.Length / 6) != -1 
								|| EntryModel.Similar(title, nativeTitle, !string.IsNullOrWhiteSpace(nativeTitle) && title.Length > nativeTitle.Length ? nativeTitle.Length / 6 : title.Length / 6) != -1
							)
					)
				{
                    LOGGER.Info("Not on AniList or Incorrect Entry -> Trying Mangadex");
					seriesDataDoc = null;
					goto Restart;
				}
				LOGGER.Info("Check2");

				bool hasNextPage = seriesData.GetProperty("staff").GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean();
				countryOfOrigin = seriesData.GetProperty("countryOfOrigin").GetString();
				filteredBookType = bookType.Equals("MANGA") ? GetCorrectComicName(countryOfOrigin) : "Novel";
				nativeStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "native", filteredBookType, romajiTitle, new StringBuilder());
				fullStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "full", filteredBookType, romajiTitle, new StringBuilder());
				coverPath = SaveNewCoverImage(CreateCoverFilePath(seriesData.GetProperty("coverImage").GetProperty("extraLarge").GetString(), romajiTitle, filteredBookType.ToUpper(), seriesData.GetProperty("synonyms").EnumerateArray(), Site.AniList), seriesData.GetProperty("coverImage").GetProperty("extraLarge").GetString());

				// If available on AniList and is not a Japanese series get the Japanese title from Mangadex
				if (!bookType.Equals("NOVEL") && (countryOfOrigin.Equals("KR") || countryOfOrigin.Equals("CW") || countryOfOrigin.Equals("TW")))
				{
					mangaDexAltTitles = MangadexQuery.GetAdditionalMangaDexTitleList((await MangadexQuery.GetSeriesByTitleAsync(romajiTitle)).RootElement.GetProperty("data"));
					japaneseTitle = GetAltTitle("ja", mangaDexAltTitles);
				}

				// Loop while there are still staff to check
				while(hasNextPage)
				{
                    LOGGER.Info($"{romajiTitle} has More Staff");
					JsonDocument? moreStaffQuery;
					if (int.TryParse(title, out seriesId))
					{
						moreStaffQuery = await AniListQuery.GetSeriesByIDAsync(seriesId, bookType, ++pageNum);
					}
					else
					{
						moreStaffQuery = await AniListQuery.GetSeriesByTitleAsync(title, bookType, ++pageNum);
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
					newTitles.Add(ANILIST_LANG_CODES[countryOfOrigin], nativeTitle);
				}

				if (additionalLanguages.Count != 0)
				{
					if (mangaDexAltTitles.Count() == 0)
					{
						AddAdditionalLanguages(ref newTitles, additionalLanguages, MangadexQuery.GetAdditionalMangaDexTitleList((await MangadexQuery.GetSeriesByTitleAsync(romajiTitle)).RootElement.GetProperty("data")).ToList());
					}
					else
					{
						AddAdditionalLanguages(ref newTitles, additionalLanguages, mangaDexAltTitles.ToList());
					}
				}

				Dictionary<string, string> newStaff = new(StringComparer.InvariantCultureIgnoreCase);
				if (!string.IsNullOrWhiteSpace(fullStaff))
				{
					newStaff.Add("Romaji", fullStaff);
				}
				if (nativeStaff != " | " && !string.IsNullOrWhiteSpace(nativeStaff))
				{
					newStaff.Add(ANILIST_LANG_CODES[countryOfOrigin], nativeStaff);
				}

				return new Series(
					newTitles,
					newStaff,
					AniListQuery.ParseAniListDescription(seriesData.GetProperty("description").GetString()),
					filteredBookType,
					GetSeriesStatus(seriesData.GetProperty("status").GetString()),
					coverPath,
					seriesData.GetProperty("siteUrl").GetString(),
					maxVolCount,
					minVolCount,
					-1,
					string.Empty
				);
			}
			else if (bookType.Equals("MANGA")) // MangadexQuery
			{
				string curId = string.Empty;
				bool notFoundCondition = true;
				if (MangaDexIDRegex().IsMatch(title))
				{
					seriesJson = await MangadexQuery.GetSeriesByIdAsync(title);
					curId = title;
				}
				else
				{
					seriesJson = await MangadexQuery.GetSeriesByTitleAsync(title);
				}

				if (!string.IsNullOrWhiteSpace(seriesJson.ToString()))
				{
					JsonElement data = seriesJson.RootElement.GetProperty("data");
					JsonElement attributes = data;
					string description = string.Empty, seriesStatus = string.Empty, link = string.Empty, demographic = string.Empty, englishAltTitle = string.Empty, japaneseAltTitle = string.Empty;
					JsonElement.ArrayEnumerator altTitles = new(), relationships = new();

					if (data.ValueKind == JsonValueKind.Array)
					{
						foreach (JsonElement series in data.EnumerateArray())
						{
							attributes = series.GetProperty("attributes");
							altTitles = attributes.GetProperty("altTitles").EnumerateArray();
							englishTitle = attributes.GetProperty("title").GetProperty("en").GetString();
							englishAltTitle = GetAltTitle("en", altTitles);
							japaneseAltTitle = GetAltTitle("ja", altTitles);
							if (
								data.GetArrayLength() == 1 
								|| EntryModel.Similar(title, englishTitle, !string.IsNullOrWhiteSpace(englishTitle) && title.Length > englishTitle.Length ? englishTitle.Length / 6 : title.Length / 6) != -1 
								|| EntryModel.Similar(title, englishAltTitle, !string.IsNullOrWhiteSpace(englishAltTitle) && title.Length > englishAltTitle.Length ? englishAltTitle.Length / 6 : title.Length / 6) != -1 
								|| EntryModel.Similar(title, japaneseAltTitle, !string.IsNullOrWhiteSpace(japaneseAltTitle) && title.Length > japaneseAltTitle.Length ? japaneseAltTitle.Length / 6 : title.Length / 6) != -1
								)
							{
								data = series;
								notFoundCondition = false;
								break;
							}
						}

						if(notFoundCondition)
						{
							LOGGER.Warn("User Input Invalid Series Title or ID or Can't Determine Series Needs to be more Specific");
							return null;
						}
					}
					else
					{
						attributes = data.GetProperty("attributes");
						altTitles = attributes.GetProperty("altTitles").EnumerateArray();
					}

					relationships = data.GetProperty("relationships").EnumerateArray();
					curId = string.IsNullOrWhiteSpace(curId) ? data.GetProperty("id").GetString() : curId;
					romajiTitle = attributes.GetProperty("title").GetProperty("en").GetString();
					description = MangadexQuery.ParseMangadexDescription(attributes.GetProperty("description").GetProperty("en").GetString());
					seriesStatus = GetSeriesStatus(attributes.GetProperty("status").GetString());
					link = !attributes.GetProperty("links").TryGetProperty("al", out JsonElement aniListId) ? @$"https://mangadex.org/title/{curId}" : @$"https://anilist.co/manga/{aniListId}";
					countryOfOrigin = attributes.GetProperty("originalLanguage").GetString();
					string coverLink = @$"https://uploads.mangadex.org/covers/{curId}/{await MangadexQuery.GetCoverAsync(relationships.Single(x => x.GetProperty("type").GetString().Equals("cover_art")).GetProperty("id").GetString())}";
					LOGGER.Debug(coverLink);
					coverPath = SaveNewCoverImage(CreateCoverFilePath(coverLink, romajiTitle, bookType, altTitles, Site.MangaDex), coverLink);
					demographic = attributes.GetProperty("publicationDemographic").GetString();
					if (!string.IsNullOrWhiteSpace(demographic))
					{
						demographic = $"{char.ToUpper(demographic[0])}{demographic[1..]}";
					}

					if (altTitles.Count() == 0)
					{
						LOGGER.Warn("Series has no Alternate Titles");
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
					if (!additionalLanguages.IsNullOrEmpty())
					{
						AddAdditionalLanguages(ref newTitles, additionalLanguages, altTitles.ToList());
					}
					Console.WriteLine(string.Join(Environment.NewLine, newTitles));

					// Get the staff for a series
					string staffName = string.Empty;
					foreach(JsonElement staff in relationships.Where(staff => staff.GetProperty("type").GetString().Equals("author") || staff.GetProperty("type").GetString().Equals("artist")))
					{
						staffName = await MangadexQuery.GetAuthorAsync(staff.GetProperty("id").GetString());
						if (FindStaffRegex().IsMatch(staffName))
						{
							string native = NativeStaffRegex().Match(staffName).Groups[1].ToString();
							string full = FullStaffRegex().Match(staffName).Value;
							if (fullStaff.Contains(full, StringComparison.OrdinalIgnoreCase) || nativeStaff.Contains(native, StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}
							nativeStaff += $"{native} | ";
							fullStaff += $"{full} | ";
						}
						else if (!fullStaff.Contains(staffName, StringComparison.OrdinalIgnoreCase))
						{
							fullStaff += $"{staffName} | ";
						}
					}
					nativeStaff = !string.IsNullOrWhiteSpace(nativeStaff) ? nativeStaff[..^3] : string.Empty;
					fullStaff = fullStaff[..^3];

					Dictionary<string, string> newStaff = new(StringComparer.InvariantCultureIgnoreCase)
					{
						{ "Romaji", fullStaff }
					};

					if (!countryOfOrigin.Equals("en"))
					{
						newTitles.Add(MANGADEX_LANG_CODES[countryOfOrigin], GetAltTitle(countryOfOrigin, altTitles));
						newStaff.Add(MANGADEX_LANG_CODES[countryOfOrigin], nativeStaff);
					}

					// Remove empty titles
					foreach (var x in newTitles)
					{
						// LOGGER.Debug(x.Key + " | " + x.Value);
						if (string.IsNullOrWhiteSpace(x.Value))
						{
							newTitles.Remove(x.Key);
						}
					}

					// Remove empty titles
					foreach (var x in newStaff)
					{
						// LOGGER.Debug(x.Key + " | " + x.Value);
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

            LOGGER.Warn("User Input Invalid Series Title or ID or Can't Determine Series Needs to be more Specific");
			return null;
        }

		private static void AddAdditionalLanguages(ref Dictionary<string, string> newTitles, ObservableCollection<string> additionalLanguages, List<JsonElement> altTitles)
		{
			JsonElement foundTitle;
			Span<string> mangaDexLang = CollectionsMarshal.AsSpan(MANGADEX_LANG_CODES.Where(lang => additionalLanguages.Contains(lang.Value)).Select(lang => lang.Key).ToList());

			foreach (string langKey in mangaDexLang)
			{
				foreach (JsonElement mdKey in altTitles)
				{
					if (!newTitles.ContainsKey(MANGADEX_LANG_CODES[langKey]) && mdKey.TryGetProperty(langKey, out foundTitle))
					{
						Console.WriteLine($"Added [{MANGADEX_LANG_CODES[langKey]} : {foundTitle.GetString()}]");
						newTitles.Add(MANGADEX_LANG_CODES[langKey], foundTitle.GetString());
						break;
					}
				}
			}
		}

		public static string? GetAltTitle(string country, JsonElement.ArrayEnumerator altTitles)
		{
            while(altTitles.MoveNext())
            {
                if (altTitles.Current.TryGetProperty(country, out JsonElement altTitle))
                {
                    return altTitle.GetString();
                }
            }
            return string.Empty;
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

		public static string GetSeriesStatus(string jsonStatus)
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
			foreach(JsonElement name in staffArray.EnumerateArray())
            {
				string staffRole = StaffRegex().Replace(name.GetProperty("role").GetString(), string.Empty).Trim();
				JsonElement nameProperty = name.GetProperty("node").GetProperty("name");
				Console.WriteLine("Check1");

				// Don't include "Illustration" staff for manga that are not anthologies
				if (
					VALID_STAFF_ROLES.Contains(staffRole, StringComparer.OrdinalIgnoreCase) 
					&& !(
							bookType.Equals("Manga") 
							&& (
									staffRole.Equals("Illustration") 
									|| staffRole.Equals("Cover Illustration")
								) 
							&& !title.Contains("Anthology")
						)
					)
                {
					Console.WriteLine("Check2");
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
						Console.WriteLine("Check3");
					}
					else
					{
						LOGGER.Info($"Duplicate Staff Entry For {newStaff}");
					}
                }
            }
			return staffList.ToString(0, staffList.Length - 3);
		}

		public static string CreateCoverFilePath(string coverLink, string title, string bookType, JsonElement.ArrayEnumerator synonyms, Site queryType)
		{
			string newPath = @$"Covers\{ExtensionMethods.RemoveInPlaceCharArray(string.Concat(title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))).Replace(",", string.Empty)}_{bookType}.{coverLink[^3..]}";
            Directory.CreateDirectory(@"Covers");

			// Check and see if the series will generate a duplicate file name
            if (File.Exists(newPath))
            {
                switch (queryType)
				{
					case Site.AniList:
						while(synonyms.MoveNext())
						{
							if (AlphaNumericOnlyRegex().IsMatch(synonyms.Current.GetString()))
							{
								newPath = @$"Covers\{ExtensionMethods.RemoveInPlaceCharArray(string.Concat(synonyms.Current.GetString().Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))).Replace(",", string.Empty)}_{bookType}.{coverLink[^3..]}";
								break;
							}
						}
						break;
					case Site.MangaDex:
						LOGGER.Debug("Check #1");
						JsonElement altTitle;
						while(synonyms.MoveNext())
						{
							if (synonyms.Current.TryGetProperty("en", out altTitle))
							{
								LOGGER.Debug(coverLink);
								newPath = @$"Covers\{ExtensionMethods.RemoveInPlaceCharArray(string.Concat(altTitle.GetString().Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))).Replace(",", string.Empty)}_{bookType}.{coverLink[^3..]}";
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
                HttpClient client = new();
                Task<HttpResponseMessage> response = Task.Run(async () => await client.GetAsync(new Uri(coverLink)));
                response.Wait();
                HttpResponseMessage clientResponse = response.Result;
                using (FileStream fs = new(newPath, FileMode.Create, FileAccess.Write))
                {
                    Task fileResponse = Task.Run(async () => await clientResponse.Content.CopyToAsync(fs));
                    fileResponse.Wait();
                }
            }
            catch (Exception e)
            {
                LOGGER.Debug(e);
            }
			return newPath;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public string? ToJsonString(JsonSerializerOptions options)
        {
            if (this != null)
			{
				return JsonSerializer.Serialize<Series?>(this, options);
			}
			return "Null Series";
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
        private readonly string curLang;

        public SeriesComparer(string curLang)
        {
            this.curLang = curLang;
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
