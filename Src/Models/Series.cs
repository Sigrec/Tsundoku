using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Tsundoku.Helpers;
using Avalonia.Media.Imaging;
using MangaAndLightNovelWebScrape;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Tsundoku.Models
{
    public partial class Series : IDisposable, IEquatable<Series?>
    {
		[GeneratedRegex(@"[a-z0-9]")] private static partial Regex AlphaNumericOnlyRegex();
        [GeneratedRegex(@" \(.*\)")] private static partial Regex StaffRegex();
        [GeneratedRegex(@" \((.*)\)")] private static partial Regex NativeStaffRegex();
        [GeneratedRegex(@"^.*(?= \(.*\))")] private static partial Regex FullStaffRegex();
		[GeneratedRegex(@"[a-z\d]{8}-[a-z\d]{4}-[a-z\d]{4}-[a-z\d]{4}-[a-z\d]{11,}")] private static partial Regex MangaDexIDRegex();

        private static SeriesModelContext SeriesJsonModel = new SeriesModelContext(new JsonSerializerOptions()
        { 
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });

		[JsonIgnore] private bool disposedValue;
		[JsonIgnore] public string Synonyms { get; }
		[JsonIgnore] public Bitmap CoverBitMap { get; set; }
		public Dictionary<string, string> Titles { get; }
        public Dictionary<string, string> Staff { get; }
		public string Description { get; }

		/// <summary>
		/// The format of the series (Manga, Manhwa, Manhua, Manfra, Comic, or Novel)
		/// </summary>
		public Format Format { get; }

		/// <summary>
		/// The serialization status of the series (Finished, Ongoing, Hiatus, Cancelled, or Error)
		/// </summary>
		public Status Status { get; }

		/// <summary>
		/// Path to the cover for a series
		/// </summary>
		public string Cover { get; set; }

        /// <summary>
        /// Link to the AniList or MangaDex page for this series
        /// </summary>
        /// <value>String</value>
		public Uri Link { get; }
		public string SeriesNotes { get; set; }
		public ushort MaxVolumeCount { get; set; }
		public ushort CurVolumeCount { get; set; }
		public uint VolumesRead { get; set; }
		public decimal Cost { get; set; }
		public decimal Rating { get; set; }
		public Demographic Demographic { get; set; }
		public bool IsFavorite { get; set; } = false;
        [JsonIgnore] public bool IsStatsPaneOpen { get; set; } = false;
        [JsonIgnore] public bool IsEditPaneOpen { get; set; } = false;

        [JsonConstructor]
		public Series(Dictionary<string, string> Titles, Dictionary<string, string> Staff, string Description, Format Format, Status Status, string Cover, Uri Link, ushort MaxVolumeCount, ushort CurVolumeCount, decimal Rating, uint VolumesRead, decimal Cost, Demographic Demographic, Bitmap CoverBitMap)
        {
            this.Titles = Titles;
			this.Staff = Staff;
			this.Description = Description;
            this.Format = Format;
            this.Status = Status;
            this.Cover = Cover;
            this.Link = Link;
            this.MaxVolumeCount = MaxVolumeCount;
            this.CurVolumeCount = CurVolumeCount;
			this.Rating = Rating;
            this.VolumesRead = VolumesRead;
            this.Cost = Cost;
			this.Demographic = Demographic;
			this.CoverBitMap = CoverBitMap;
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
        public static async Task<Series?> CreateNewSeriesCardAsync(string title, Format bookType, ushort maxVolCount, ushort minVolCount, ObservableCollection<string> additionalLanguages, string customImageUrl = "", Demographic demographic = Demographic.Unknown, uint volumesRead = 0, decimal rating = -1, decimal cost = 0)
        {
			JsonDocument? seriesDataDoc;
			int pageNum = 1;
			bool isAniListID = false, isMangaDexId = false;
            string curMangaDexId = string.Empty;
			if (int.TryParse(title, out int seriesId))
			{
				seriesDataDoc = await AniListQuery.GetSeriesByIDAsync(seriesId, bookType, pageNum);
				isAniListID = true;
			}
            else if (MangaDexIDRegex().IsMatch(title))
            {
                seriesDataDoc = await MangadexQuery.GetSeriesByIdAsync(title);
				curMangaDexId = title;
                isMangaDexId = true;
            }
			else
			{
				seriesDataDoc = await AniListQuery.GetSeriesByTitleAsync(title, bookType, pageNum);
			}

			string countryOfOrigin = string.Empty, nativeTitle = string.Empty, japaneseTitle = string.Empty, romajiTitle = string.Empty, englishTitle = string.Empty, coverPath = string.Empty;
            Format filteredBookType;
			JsonElement.ArrayEnumerator mangaDexAltTitles = [];
			Dictionary<string, string> newTitles = [];

			// AniList Query Check
			Restart:
			if ((!isMangaDexId || isAniListID) && seriesDataDoc != null)
			{
				LOGGER.Debug("Valid AniList Query");
                string nativeStaff = string.Empty, fullStaff = string.Empty;
				JsonElement seriesData = seriesDataDoc.RootElement.GetProperty("Media");
				nativeTitle = seriesData.GetProperty("title").GetProperty("native").GetString();
				romajiTitle = seriesData.GetProperty("title").GetProperty("romaji").GetString();
				englishTitle = seriesData.GetProperty("title").GetProperty("english").GetString();
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

				bool hasNextPage = seriesData.GetProperty("staff").GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean();
				countryOfOrigin = seriesData.GetProperty("countryOfOrigin").GetString();
				filteredBookType = bookType == Format.Manga ? GetCorrectFormat(countryOfOrigin) : Format.Novel;
				nativeStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "native", filteredBookType, romajiTitle, new StringBuilder());
				fullStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "full", filteredBookType, romajiTitle, new StringBuilder());
				coverPath = CreateCoverFilePath(seriesData.GetProperty("coverImage").GetProperty("extraLarge").GetString(), romajiTitle, filteredBookType, seriesData.GetProperty("id").GetUInt64().ToString());

				// If available on AniList and is not a Japanese series get the Japanese title from Mangadex
				if (!bookType.Equals("NOVEL") && (countryOfOrigin.Equals("KR") || countryOfOrigin.Equals("CW") || countryOfOrigin.Equals("TW")))
				{
                    LOGGER.Info("Getting Japanese Title for Non-Japanese Series");
					mangaDexAltTitles = MangadexQuery.GetAdditionalMangaDexTitleList((await MangadexQuery.GetSeriesByTitleAsync(romajiTitle)).RootElement.GetProperty("data"));
					japaneseTitle = GetAltTitle("ja", mangaDexAltTitles);
				}

				// Loop while there are still staff to check
				while(hasNextPage)
				{
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

				newTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
					if (mangaDexAltTitles.Any())
					{
						AddAdditionalLanguages(ref newTitles, additionalLanguages, mangaDexAltTitles.ToList());
					}
					else
					{
                        AddAdditionalLanguages(ref newTitles, additionalLanguages, MangadexQuery.GetAdditionalMangaDexTitleList((await MangadexQuery.GetSeriesByTitleAsync(romajiTitle)).RootElement.GetProperty("data")).ToList());
					}
				}

				Dictionary<string, string> newStaff = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
						new Uri(seriesData.GetProperty("siteUrl").GetString()),
						maxVolCount,
						minVolCount,
						rating,
                        volumesRead,
                        cost,
						demographic,
                        string.IsNullOrWhiteSpace(customImageUrl) ? await ViewModels.AddNewSeriesViewModel.SaveCoverAsync(coverPath, seriesData.GetProperty("coverImage").GetProperty("extraLarge").GetString(), customImageUrl) : await ViewModels.AddNewSeriesViewModel.SaveCoverAsync(coverPath, seriesData.GetProperty("coverImage").GetProperty("extraLarge").GetString(), customImageUrl)
					);
			}
			else if (isMangaDexId || bookType == Format.Manga) // MangadexQuery
			{
				bool notFoundCondition = true;
				if (!isMangaDexId)
                {
                    seriesDataDoc = await MangadexQuery.GetSeriesByTitleAsync(title);
                }

				if (seriesDataDoc != null)
				{
					JsonElement data = seriesDataDoc.RootElement.GetProperty("data");
					JsonElement attributes = data;
                    Status Status;
					string description = string.Empty, link = string.Empty, englishAltTitle = string.Empty, japaneseAltTitle = string.Empty;
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
							if (!IsSeriesInvalid(title, englishTitle, englishAltTitle)
                                && (
                                    data.GetArrayLength() == 1
                                    || (
                                            EntryModel.Similar(title, englishTitle, !string.IsNullOrWhiteSpace(englishTitle) && title.Length > englishTitle.Length ? englishTitle.Length / 6 : title.Length / 6) != -1 
                                            || EntryModel.Similar(title, englishAltTitle, !string.IsNullOrWhiteSpace(englishAltTitle) && title.Length > englishAltTitle.Length ? englishAltTitle.Length / 6 : title.Length / 6) != -1 
                                            || EntryModel.Similar(title, japaneseAltTitle, !string.IsNullOrWhiteSpace(japaneseAltTitle) && title.Length > japaneseAltTitle.Length ? japaneseAltTitle.Length / 6 : title.Length / 6) != -1
                                    )
                                )
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
                        englishTitle = attributes.GetProperty("title").GetProperty("en").GetString();
						englishAltTitle = GetAltTitle("en", altTitles);

                        if (IsSeriesInvalid(title, englishTitle, englishAltTitle))
                        {
                            LOGGER.Warn("{} is a Invalid Series", title);
                            return null;
                        }
					}

					relationships = data.GetProperty("relationships").EnumerateArray();
					curMangaDexId = string.IsNullOrWhiteSpace(curMangaDexId) ? data.GetProperty("id").GetString() : curMangaDexId;
					romajiTitle = attributes.GetProperty("title").GetProperty("en").GetString();
					description = MangadexQuery.ParseMangadexDescription(attributes.GetProperty("description").GetProperty("en").GetString());
					Status = GetSeriesStatus(attributes.GetProperty("status").GetString());
					link = !attributes.GetProperty("links").TryGetProperty("al", out JsonElement aniListId) ? @$"https://mangadex.org/title/{curMangaDexId}" : @$"https://anilist.co/manga/{aniListId}";
					countryOfOrigin = attributes.GetProperty("originalLanguage").GetString();
					string coverLink = @$"https://uploads.mangadex.org/covers/{curMangaDexId}/{await MangadexQuery.GetCoverAsync(relationships.Single(x => x.GetProperty("type").GetString().Equals("cover_art")).GetProperty("id").GetString())}";
                    LOGGER.Debug("COVER LINK = {}", coverLink);
					coverPath = CreateCoverFilePath(coverLink, romajiTitle, GetCorrectFormat(countryOfOrigin), curMangaDexId);
					demographic = GetSeriesDemographic(attributes.GetProperty("publicationDemographic").GetString());

					if (altTitles.Any())
					{
						LOGGER.Debug("Series has no Alternate Titles");
					}

					newTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
					{
						{ "Romaji", romajiTitle },
						{ "English",  englishTitle.Equals(englishAltTitle) || string.IsNullOrWhiteSpace(englishAltTitle) || countryOfOrigin.Equals("en") ? englishTitle : englishAltTitle }
					};

					// Get the Japanese title if the series is not a Japanese Comic
					if (!countryOfOrigin.Equals("ja"))
					{
						newTitles.Add("Japanese", GetAltTitle("ja", altTitles));
					}

					// Check to see if the user wants additional languages
					if (additionalLanguages != null && additionalLanguages.Any())
					{
						AddAdditionalLanguages(ref newTitles, additionalLanguages, altTitles.ToList());
					}

					// Get the staff for a series
                    string? staffName = string.Empty;
                    StringBuilder nativeStaffBuilder = new StringBuilder(), fullStaffBuilder = new StringBuilder();
					foreach(string staff in relationships.Where(staff => staff.GetProperty("type").GetString().Equals("author") || staff.GetProperty("type").GetString().Equals("artist")).Select(staff => staff.GetProperty("id").GetString()).Distinct())
					{
						staffName = await MangadexQuery.GetAuthorAsync(staff);
						if (!string.IsNullOrWhiteSpace(staffName))
                        {
                            if (NativeStaffRegex().IsMatch(staffName))
                            {
                                string native = NativeStaffRegex().Match(staffName).Groups[1].ToString();
                                string full = FullStaffRegex().Match(staffName).Value;
                                if (fullStaffBuilder.ToString().Contains(full, StringComparison.OrdinalIgnoreCase) || nativeStaffBuilder.ToString().Contains(native, StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                                nativeStaffBuilder.AppendFormat("{0} | ", native);
                                fullStaffBuilder.AppendFormat("{0} | ", full);
                            }
                            else if (!fullStaffBuilder.ToString().Contains(staffName))
                            {
                                fullStaffBuilder.AppendFormat("{0} | ", staffName);
                            }
                        }
					}

					Dictionary<string, string> newStaff = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
					{
						{ "Romaji", fullStaffBuilder.Remove(fullStaffBuilder.Length - 3, 3).ToString() }
					};

                    // If country of origin is not English based add the english title and staff
					if (!countryOfOrigin.Equals("en"))
					{
						newTitles.Add(MANGADEX_LANG_CODES[countryOfOrigin], GetAltTitle(countryOfOrigin, altTitles));
						newStaff.Add(MANGADEX_LANG_CODES[countryOfOrigin], nativeStaffBuilder.Length != 0 ? nativeStaffBuilder.Remove(nativeStaffBuilder.Length - 3, 3).ToString() : string.Empty);
					}

					// Remove empty titles
					foreach (var x in newTitles)
					{
						if (string.IsNullOrWhiteSpace(x.Value))
						{
							newTitles.Remove(x.Key);
						}
					}

					// Remove empty titles
					foreach (var x in newStaff)
					{
						if (string.IsNullOrWhiteSpace(x.Value))
						{
							newStaff.Remove(x.Key);
						}
					}

                    return new Series(
                        newTitles,
                        newStaff,
                        description,
                        GetCorrectFormat(countryOfOrigin),
                        Status,
                        coverPath,
                        new Uri(link),
                        maxVolCount,
                        minVolCount,
                        rating,
                        volumesRead,
                        cost,
                        demographic,
						string.IsNullOrWhiteSpace(customImageUrl) ? await ViewModels.AddNewSeriesViewModel.SaveCoverAsync(coverPath, coverLink, string.Empty) : await ViewModels.AddNewSeriesViewModel.SaveCoverAsync(coverPath, coverLink, customImageUrl)
					);
				}
			}

            LOGGER.Warn($"User Input Invalid Series Title or ID or Can't Determine Series Needs to be more Specific for {title}");
			return null;
        }

        private static bool IsSeriesInvalid(string title, string englishTitle, string englishAltTitle)
        {
            return 
            (
                !(
                    title.Contains("Digital", StringComparison.OrdinalIgnoreCase)
                    || title.Contains("Fan Colored", StringComparison.OrdinalIgnoreCase)
                    || englishAltTitle.Contains("Digital", StringComparison.OrdinalIgnoreCase)
                    || englishAltTitle.Contains("Official Colored", StringComparison.OrdinalIgnoreCase)
                    || title.Contains("Official Colored", StringComparison.OrdinalIgnoreCase)
                )
                && englishTitle.Contains("Digital", StringComparison.OrdinalIgnoreCase)
            );
        }        
        
		private static void AddAdditionalLanguages(ref Dictionary<string, string> newTitles, ObservableCollection<string> additionalLanguages, List<JsonElement> altTitles)
		{
            Span<string> mangaDexLang = CollectionsMarshal.AsSpan(MANGADEX_LANG_CODES.Where(lang => additionalLanguages.Contains(lang.Value)).Select(lang => lang.Key).ToList());

            foreach (string langKey in mangaDexLang)
			{
				foreach (JsonElement mdKey in altTitles)
				{
					if (!newTitles.ContainsKey(MANGADEX_LANG_CODES[langKey]) && mdKey.TryGetProperty(langKey, out JsonElement foundTitle))
					{
						LOGGER.Debug($"Added [{MANGADEX_LANG_CODES[langKey]} : {foundTitle.GetString()}]");
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

		public static Format GetCorrectFormat(string jsonCountryOfOrigin)
		{
			return jsonCountryOfOrigin switch
			{
				"KR" or "ko" => Format.Manhwa,
				"CN" or "TW" or "zh" or "zh-hk"=> Format.Manhua,
				"FR" or "fr" => Format.Manfra,
				"EN" or "en" => Format.Comic,
				_ or "JP" or "ja" => Format.Manga
			};
		}

		public static Status GetSeriesStatus(string jsonStatus)
		{
			return jsonStatus switch
			{
				"RELEASING" or "NOT_YET_RELEASED" or "ongoing" => Status.Ongoing,
				"FINISHED"  or "completed" => Status.Finished,
				"CANCELLED" or "cancelled" => Status.Cancelled,
				"HIATUS" or "hiatus" => Status.Hiatus,
				_  => Status.Error
			};
		}

        public static Demographic GetSeriesDemographic(string demographic)
        {
            return demographic switch
            {
                "shounen" or "Shounen" => Demographic.Shounen,
                "shoujo" or "Shoujo" => Demographic.Shoujo,
                "josei" or "Josei"=> Demographic.Josei,
                "seinen" or "Seinen" => Demographic.Seinen,
                _ => Demographic.Unknown
            };
        }

        public static string GetSeriesStaff(JsonElement staffArray, string nameType, Format bookType, string title, StringBuilder staffList) {
			foreach(JsonElement name in staffArray.EnumerateArray())
            {
				string staffRole = StaffRegex().Replace(name.GetProperty("role").GetString(), string.Empty).Trim();
				JsonElement nameProperty = name.GetProperty("node").GetProperty("name");

				// Don't include "Illustration" staff for manga that are not anthologies
				if (
					VALID_STAFF_ROLES.Contains(staffRole, StringComparer.OrdinalIgnoreCase) 
					&& !(
							bookType != Format.Novel 
							&& (
									staffRole.Equals("Illustration") 
									|| staffRole.Equals("Cover Illustration")
								) 
							&& !title.Contains("Anthology")
						)
					)
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

		public static string CreateCoverFilePath(string coverLink, string title, Format bookType, string seriesId)
		{
            Directory.CreateDirectory(@"Covers");
            string coverName = ExtensionMethods.RemoveInPlaceCharArray(string.Concat(title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))).Replace(",", string.Empty);
            string format = bookType.ToString().ToUpper();
			string newPath = @$"Covers\{coverName}_{format}.{coverLink[^3..]}";
            LOGGER.Debug("(0) {}", newPath);

            if (File.Exists(@$"Covers\{coverName}_{format}.jpg") || File.Exists(@$"Covers\{coverName}_{format}.png"))
            {
                newPath = @$"Covers\{coverName}_{seriesId}_{format}.{coverLink[^3..]}";
            }
            LOGGER.Debug("(1) {}", newPath);

			return newPath;
		}

        public override string ToString()
        {
            if (this != null)
			{
				return JsonSerializer.Serialize(this, typeof(Series), SeriesJsonModel);
			}
			return "Null Series";
        }

        // public override string ToString()
        // {
        //     if (this != null)
		// 	{
		// 		return JsonSerializer.Serialize(this, ViewModels.ViewModelBase.options);
		// 	}
		// 	return "Null Series";
        // }UseStringEnumConverter = true

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

        public bool Equals(Series other)
        {
            return other is not null &&
                   Titles["Romaji"].Equals(other.Titles["Romaji"]) &&
                   Format == other.Format;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Titles, Staff, Description, Format, Status, Cover, Link);
        }

        public static bool operator ==(Series? left, Series? right)
        {
            return EqualityComparer<Series>.Default.Equals(left, right);
        }

        public static bool operator !=(Series? left, Series? right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj) => Equals(obj as Series);
    }

    [JsonSerializable(typeof(Series))]
    [JsonSourceGenerationOptions(UseStringEnumConverter = true)]
    internal partial class SeriesModelContext : JsonSerializerContext
    {
    }

    class SeriesComparer(string curLang) : IComparer<Series>
    {
        private readonly string curLang = curLang;
        private readonly StringComparer SeriesTitleComparer = StringComparer.Create(new CultureInfo(CULTURE_LANG_CODES[curLang]), false);

        public int Compare(Series? x, Series? y)
        {
            return SeriesTitleComparer.Compare(x.Titles.TryGetValue(curLang, out string? xValue) ? xValue : x.Titles["Romaji"], y.Titles.TryGetValue(curLang, out string? yValue) ? yValue : y.Titles["Romaji"]);
        }
    }
}
