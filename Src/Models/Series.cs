using Avalonia.Media.Imaging;
using MangaAndLightNovelWebScrape;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using Tsundoku.Helpers;
using static Tsundoku.Models.TsundokuLanguageModel;

namespace Tsundoku.Models
{
    public partial class Series : ReactiveObject, IDisposable, IEquatable<Series?>
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private static SeriesModelContext SeriesJsonModel = new SeriesModelContext(new JsonSerializerOptions()
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });

        [JsonIgnore] private bool disposedValue;
        [JsonIgnore][Reactive] public Bitmap CoverBitMap { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        [Reactive] public string Publisher { get; set; }
        [Reactive] public Dictionary<TsundokuLanguage, string> Titles { get; set; }
        [Reactive] public Dictionary<TsundokuLanguage, string> Staff { get; set; }
        [Reactive] public uint DuplicateIndex { get; set; }
        [Reactive] public string Description { get; set; }
        [Reactive] public Format Format { get; set; }
        [Reactive] public Status Status { get; set; }
        [Reactive] public string Cover { get; set; }
        public Uri Link { get; set; }
        [Reactive] public HashSet<Genre> Genres { get; set; }
        [Reactive] public string SeriesNotes { get; set; }
        [Reactive] public ushort MaxVolumeCount { get; set; }
        [Reactive] public ushort CurVolumeCount { get; set; }
        [Reactive] public uint VolumesRead { get; set; }
        [Reactive] public decimal Value { get; set; }
        [Reactive] public decimal Rating { get; set; }
        [Reactive] public Demographic Demographic { get; set; }
        [Reactive] public bool IsFavorite { get; set; } = false;

        [JsonConstructor]
        public Series(
            Dictionary<TsundokuLanguage, string> Titles,
            Dictionary<TsundokuLanguage, string> Staff,
            string Description,
            Format Format,
            Status Status,
            string Cover,
            Uri Link,
            HashSet<Genre> Genres,
            ushort MaxVolumeCount,
            ushort CurVolumeCount,
            decimal Rating,
            uint VolumesRead,
            decimal Value,
            Demographic Demographic,
            Bitmap CoverBitMap,
            string Publisher = "Unknown",
            uint DuplicateIndex = 0)
        {
            this.Publisher = Publisher;
            this.Titles = Titles;
            this.Staff = Staff;
            this.Description = Description;
            this.Format = Format;
            this.Status = Status;
            this.Cover = Cover;
            this.Link = Link;
            this.Genres = Genres;
            this.MaxVolumeCount = MaxVolumeCount;
            this.CurVolumeCount = CurVolumeCount;
            this.Rating = Rating;
            this.VolumesRead = VolumesRead;
            this.Value = Value;
            this.Demographic = Demographic;
            this.CoverBitMap = CoverBitMap;
            this.DuplicateIndex = DuplicateIndex;
        }

        /// <summary>
        /// All Chinese or Taiwanese series use "Chinese (Simplified)" and go to "Chinese"
        /// </summary>
        /// <param name="title">Title or ID of the series the user wants to add to their collection</param>
        /// <param name="bookType">Booktype of the series the user wants to add, either Manga or Light Novel</param>
        /// <param name="maxVolCount">Current max volume count of the series</param>
        /// <param name="minVolCount">Current volume count the user has for the series</param>
        /// <param name="ALQuery">AniList object for the AniList HTTP Client</param>
        /// <param name="MD_Query">MangaDex object for the MangaDex HTTP client</param>
        /// <param name="additionalLanguages">List of additional languages to query for</param>
        /// <returns></returns>
        /// TODO - Make it so it doesn't download the image until it's confirmed that the series is not a dupe
        public static async Task<Series?> CreateNewSeriesCardAsync(
            BitmapHelper bitmapHelper,
            MangaDex mangaDex,
            AniList aniList,
            string title,
            Format bookType,
            ushort maxVolCount,
            ushort minVolCount,
            List<TsundokuLanguage> additionalLanguages,
            string publisher = "Unknown",
            Demographic demographic = Demographic.Unknown,
            uint volumesRead = 0,
            decimal rating = -1,
            decimal value = 0,
            string customImageUrl = "",
            bool allowDuplicate = false,
            bool isRefresh = false
        )
        {
            try
            {
                JsonDocument? seriesDataDoc;
                int pageNum = 1;
                bool isAniListID = false, isMangaDexId = false;
                string curMangaDexId = string.Empty;
                uint dupeIndex = 0;
                ;
                if (int.TryParse(title, out int seriesId))
                {
                    LOGGER.Debug("Getting AniList ID Data");
                    seriesDataDoc = await aniList.GetSeriesByIDAsync(seriesId, bookType, pageNum);
                    isAniListID = true;
                }
                else if (MangaDex.MangaDexIDRegex().IsMatch(title))
                {
                    LOGGER.Debug("Getting MangaDex ID Data");
                    seriesDataDoc = await mangaDex.GetSeriesByIdAsync(title);
                    curMangaDexId = title;
                    isMangaDexId = true;
                }
                else
                {
                    seriesDataDoc = await aniList.GetSeriesByTitleAsync(title, bookType, pageNum);
                    if (seriesDataDoc == null)
                    {
                        LOGGER.Debug("Getting MangaDex Title Data since AniList is Null");
                        seriesDataDoc = await mangaDex.GetSeriesByTitleAsync(title);
                        isMangaDexId = true;
                    }
                }

                string countryOfOrigin = string.Empty, nativeTitle = string.Empty, japaneseTitle = string.Empty, romajiTitle = string.Empty, englishTitle = string.Empty, coverPath = string.Empty;
                Format filteredBookType;
                JsonElement.ArrayEnumerator mangaDexAltTitles = [];
                Dictionary<TsundokuLanguage, string> newTitles = [];

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
                    nativeStaff = AniList.GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "native", filteredBookType, romajiTitle, new StringBuilder());
                    fullStaff = AniList.GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "full", filteredBookType, romajiTitle, new StringBuilder());
                    (coverPath, bool isDuplicateCover) = !isRefresh ? AppFileHelper.CreateUniqueCoverFileName(seriesData.GetProperty("coverImage").GetProperty("extraLarge").GetString(), romajiTitle, filteredBookType, allowDuplicate, ref dupeIndex) : (string.Empty, false);


                    // If available on AniList and is not a Japanese series get the Japanese title from Mangadex
                    if (!bookType.Equals("NOVEL") && (countryOfOrigin.Equals("KR") || countryOfOrigin.Equals("CW") || countryOfOrigin.Equals("TW")))
                    {
                        LOGGER.Info("Getting Japanese Title for Non-Japanese Series");
                        JsonDocument? getJapnAltTitle = await mangaDex.GetSeriesByTitleAsync(romajiTitle);
                        if (getJapnAltTitle == null)
                        {
                            LOGGER.Warn($"Unable to get MangaDex alt titles for \"{romajiTitle}\"");
                            return null;
                        }
                        mangaDexAltTitles = MangaDex.GetAdditionalMangaDexTitleList(getJapnAltTitle.RootElement.GetProperty("data"));
                        japaneseTitle = GetAltTitle("ja", mangaDexAltTitles);
                    }

                    // Loop while there are still staff to check
                    while (hasNextPage)
                    {
                        JsonDocument? moreStaffQuery;
                        if (int.TryParse(title, out seriesId))
                        {
                            moreStaffQuery = await aniList.GetSeriesByIDAsync(seriesId, bookType, ++pageNum);
                        }
                        else
                        {
                            moreStaffQuery = await aniList.GetSeriesByTitleAsync(title, bookType, ++pageNum);
                        }
                        JsonElement moreStaff = moreStaffQuery.RootElement.GetProperty("Media").GetProperty("staff");
                        nativeStaff = AniList.GetSeriesStaff(moreStaff.GetProperty("edges"), "native", filteredBookType, romajiTitle, new StringBuilder(nativeStaff + " | "));
                        fullStaff = AniList.GetSeriesStaff(moreStaff.GetProperty("edges"), "full", filteredBookType, romajiTitle, new StringBuilder(fullStaff + " | "));
                        hasNextPage = moreStaff.GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean();
                    }

                    newTitles = new Dictionary<TsundokuLanguage, string>();
                    if (!string.IsNullOrWhiteSpace(romajiTitle))
                    {
                        newTitles.Add(TsundokuLanguage.Romaji, romajiTitle);
                    }
                    if (!string.IsNullOrWhiteSpace(englishTitle))
                    {
                        newTitles.Add(TsundokuLanguage.English, englishTitle);
                    }
                    if (!string.IsNullOrWhiteSpace(japaneseTitle))
                    {
                        newTitles.Add(TsundokuLanguage.Japanese, japaneseTitle);
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
                            LOGGER.Info("Alt Titles Empty so Making Call to Mangadex");
                            JsonDocument? altTitlesAgain = await mangaDex.GetSeriesByTitleAsync(romajiTitle);
                            if (altTitlesAgain == null)
                            {
                                LOGGER.Warn($"Unable to get MangaDex info for \"{romajiTitle}\"");
                                return null;
                            }
                            AddAdditionalLanguages(ref newTitles, additionalLanguages, MangaDex.GetAdditionalMangaDexTitleList(altTitlesAgain.RootElement.GetProperty("data")).ToList());
                        }
                    }

                    Dictionary<TsundokuLanguage, string> newStaff = new Dictionary<TsundokuLanguage, string>();
                    if (!string.IsNullOrWhiteSpace(fullStaff))
                    {
                        newStaff.Add(TsundokuLanguage.Romaji, fullStaff);
                    }
                    if (nativeStaff != " | " && !string.IsNullOrWhiteSpace(nativeStaff))
                    {
                        newStaff.Add(ANILIST_LANG_CODES[countryOfOrigin], nativeStaff);
                    }

                    Bitmap? coverImage = !isRefresh && (!isDuplicateCover || allowDuplicate) ? await bitmapHelper.GenerateAvaloniaBitmapAsync(AppFileHelper.GetFullCoverPath(coverPath), string.Empty, string.IsNullOrWhiteSpace(customImageUrl) ? seriesData.GetProperty("coverImage").GetProperty("extraLarge").GetString() : customImageUrl) : null;

                    if (!isRefresh && coverImage is null)
                    {
                        throw new InvalidOperationException("Expected cover image generation to succeed, but it returned null");
                    }

                    return new Series(
                        newTitles,
                        newStaff,
                        AniList.ParseAniListDescription(seriesData.GetProperty("description").GetString()),
                        filteredBookType,
                        GetSeriesStatus(seriesData.GetProperty("status").GetString()),
                        coverPath,
                        new Uri(seriesData.GetProperty("siteUrl").GetString()),
                        AniList.ParseGenreArray(romajiTitle, seriesData.GetProperty("genres")),
                        maxVolCount,
                        minVolCount,
                        rating,
                        volumesRead,
                        value,
                        demographic,
                        coverImage,
                        publisher,
                        dupeIndex
                    );
                }
                else if (isMangaDexId || bookType == Format.Manga) // MangaDex
                {
                    bool notFoundCondition = true;
                    if (!isMangaDexId)
                    {
                        seriesDataDoc = await mangaDex.GetSeriesByTitleAsync(title);
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
                                englishAltTitle = GetAltTitle("en", altTitles);
                                japaneseAltTitle = GetAltTitle("ja", altTitles);
                                englishTitle = attributes.GetProperty("title").TryGetProperty("en", out JsonElement englishElement) ? englishElement.GetString() : englishAltTitle;

                                if (!MangaDex.IsSeriesInvalid(title, englishTitle, englishAltTitle)
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

                            if (notFoundCondition)
                            {
                                LOGGER.Warn($"User Input Invalid Series Title or ID \"{title}\" or Can't Determine Series Needs to be more Specific");
                                return null;
                            }
                        }
                        else
                        {
                            attributes = data.GetProperty("attributes");
                            altTitles = attributes.GetProperty("altTitles").EnumerateArray();
                            englishTitle = attributes.GetProperty("title").GetProperty("en").GetString();
                            englishAltTitle = GetAltTitle("en", altTitles);

                            if (MangaDex.IsSeriesInvalid(title, englishTitle, englishAltTitle))
                            {
                                LOGGER.Warn("{} is a Invalid Series", title);
                                return null;
                            }
                        }

                        relationships = data.GetProperty("relationships").EnumerateArray();
                        curMangaDexId = string.IsNullOrWhiteSpace(curMangaDexId) ? data.GetProperty("id").GetString() : curMangaDexId;
                        romajiTitle = GetAltTitle("ja-ro", altTitles); romajiTitle = string.IsNullOrWhiteSpace(romajiTitle) ? englishTitle : romajiTitle;
                        description = MangaDex.ParseMangadexDescription(attributes.GetProperty("description").GetProperty("en").GetString());
                        Status = GetSeriesStatus(attributes.GetProperty("status").GetString());
                        link = !attributes.GetProperty("links").TryGetProperty("al", out JsonElement aniListId) ? @$"https://mangadex.org/title/{curMangaDexId}" : @$"https://anilist.co/manga/{aniListId}";
                        countryOfOrigin = attributes.GetProperty("originalLanguage").GetString();

                        string? mangadexCover = await mangaDex.GetCoverAsync(relationships.Single(x => x.GetProperty("type").GetString().Equals("cover_art")).GetProperty("id").GetString());
                        if (mangadexCover == null)
                        {
                            LOGGER.Warn($"Unable to get MangaDex cover for \"{romajiTitle}\"");
                            return null;
                        }
                        string coverLink = string.IsNullOrWhiteSpace(customImageUrl) ? @$"https://uploads.mangadex.org/covers/{curMangaDexId}/{mangadexCover}" : customImageUrl;
                        (coverPath, bool isDuplicateCover) = !isRefresh ? AppFileHelper.CreateUniqueCoverFileName(coverLink, romajiTitle, GetCorrectFormat(countryOfOrigin), allowDuplicate, ref dupeIndex) : (string.Empty, false);
                        demographic = GetSeriesDemographic(attributes.GetProperty("publicationDemographic").GetString());

                        if (altTitles.Any())
                        {
                            LOGGER.Debug("Series has no Alternate Titles");
                        }

                        newTitles = new Dictionary<TsundokuLanguage, string>
                        {
                            { TsundokuLanguage.Romaji, romajiTitle },
                            { TsundokuLanguage.English,  englishTitle.Equals(englishAltTitle) || string.IsNullOrWhiteSpace(englishAltTitle) || countryOfOrigin.Equals("en") ? englishTitle : englishAltTitle }
                        };

                        // Get the Japanese title if the series is not a Japanese Comic
                        if (!countryOfOrigin.Equals("ja"))
                        {
                            newTitles.Add(TsundokuLanguage.Japanese, GetAltTitle("ja", altTitles));
                        }

                        // Check to see if the user wants additional languages
                        if (additionalLanguages != null && additionalLanguages.Any())
                        {
                            AddAdditionalLanguages(ref newTitles, additionalLanguages, altTitles.ToList());
                        }

                        // Get the staff for a series
                        string? staffName = string.Empty;
                        StringBuilder nativeStaffBuilder = new StringBuilder(), fullStaffBuilder = new StringBuilder();
                        foreach (string staff in relationships.Where(staff => staff.GetProperty("type").GetString().Equals("author") || staff.GetProperty("type").GetString().Equals("artist")).Select(staff => staff.GetProperty("id").GetString()).Distinct())
                        {
                            staffName = await mangaDex.GetAuthorAsync(staff);
                            if (!string.IsNullOrWhiteSpace(staffName))
                            {
                                if (MangaDex.NativeStaffRegex().IsMatch(staffName))
                                {
                                    string native = MangaDex.NativeStaffRegex().Match(staffName).Groups[1].ToString();
                                    string full = MangaDex.FullStaffRegex().Match(staffName).Value;
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
                            else
                            {
                                LOGGER.Warn($"Unable to get MangaDex author info for \"{romajiTitle}\"");
                                return null;
                            }
                        }

                        Dictionary<TsundokuLanguage, string> newStaff = new Dictionary<TsundokuLanguage, string>()
                        {
                            { TsundokuLanguage.Romaji, fullStaffBuilder.Remove(fullStaffBuilder.Length - 3, 3).ToString() }
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

                        Bitmap? coverImage = !isRefresh && (!isDuplicateCover || allowDuplicate) ? await bitmapHelper.GenerateAvaloniaBitmapAsync(AppFileHelper.GetFullCoverPath(coverPath), string.Empty, coverLink) : null;

                        if (!isRefresh && coverImage is null)
                        {
                            throw new InvalidOperationException("Expected cover image generation to succeed, but it returned null");
                        }

                        return new Series(
                            newTitles,
                            newStaff,
                            description,
                            GetCorrectFormat(countryOfOrigin),
                            Status,
                            coverPath,
                            new Uri(link),
                            MangaDex.ParseGenreData(romajiTitle, attributes.GetProperty("tags")),
                            maxVolCount,
                            minVolCount,
                            rating,
                            volumesRead,
                            value,
                            demographic,
                            coverImage,
                            publisher,
                            dupeIndex
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error("Error Creating Series Card\n{msg}", ex.StackTrace);
            }

            LOGGER.Warn($"User Input Invalid Series Title or ID \"{title}\" or Can't Determine Series Needs to be more Specific");
            return null;
        }

        public static void AddAdditionalLanguages(ref Dictionary<TsundokuLanguage, string> newTitles, List<TsundokuLanguage> additionalLanguages, List<JsonElement> altTitles)
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

        public static Format GetCorrectFormat(string jsonCountryOfOrigin)
        {
            return jsonCountryOfOrigin switch
            {
                "KR" or "ko" => Format.Manhwa,
                "CN" or "TW" or "zh" or "zh-hk" => Format.Manhua,
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
                "FINISHED" or "completed" => Status.Finished,
                "CANCELLED" or "cancelled" => Status.Cancelled,
                "HIATUS" or "hiatus" => Status.Hiatus,
                _ => Status.Error
            };
        }

        public static Demographic GetSeriesDemographic(string demographic)
        {
            return demographic switch
            {
                "shounen" or "Shounen" => Demographic.Shounen,
                "shoujo" or "Shoujo" => Demographic.Shoujo,
                "josei" or "Josei" => Demographic.Josei,
                "seinen" or "Seinen" => Demographic.Seinen,
                _ => Demographic.Unknown
            };
        }

        public static string? GetAltTitle(string country, JsonElement.ArrayEnumerator altTitles)
        {
            while (altTitles.MoveNext())
            {
                if (altTitles.Current.TryGetProperty(country, out JsonElement altTitle))
                {
                    return altTitle.GetString();
                }
            }
            return string.Empty;
        }

        public void DeleteCover()
        {
            AppFileHelper.DeleteCoverFile(this.Cover);
        }

        public void UpdateCover(Bitmap newCover)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                this.CoverBitMap.Dispose();
                this.CoverBitMap = newCover;
            });
        }

        public List<TsundokuLanguage> SeriesContainsAdditionalLanagues()
        {
            List<TsundokuLanguage> additionalLangs = [];
            foreach (TsundokuLanguage lang in LANGUAGES.Skip(3))
            {
                if (this.Titles.ContainsKey(lang))
                {
                    additionalLangs.Add(lang);
                }
            }
            return additionalLangs;
        }

        public override string ToString()
        {
            if (this != null)
            {
                return JsonSerializer.Serialize(this, typeof(Series), SeriesJsonModel);
            }
            return "Null Series";
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    this.CoverBitMap?.Dispose();
                    DeleteCover();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                this.disposedValue = true;
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

        public bool Equals(Series? other)
        {
            return other is not null && this.Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Series? left, Series? right)
        {
            return EqualityComparer<Series>.Default.Equals(left, right);
        }

        public static bool operator !=(Series? left, Series? right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj) => Equals(obj as Series);
    }

    [JsonSerializable(typeof(Series))]
    [JsonSourceGenerationOptions(UseStringEnumConverter = true)]
    internal partial class SeriesModelContext : JsonSerializerContext
    {
    }

    public class SeriesComparer : IComparer<Series>
    {
        private readonly TsundokuLanguage _curLang; // Changed to match common C# naming convention
        private readonly StringComparer _seriesTitleComparer; // Changed to match common C# naming convention

        public SeriesComparer(TsundokuLanguage curLang)
        {
            _curLang = curLang;
            _seriesTitleComparer = StringComparer.Create(new CultureInfo(CULTURE_LANG_CODES[curLang]), false);
        }

        public int Compare(Series? x, Series? y)
        {
            // Handle nulls gracefully, as IComparer expects
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Safely get titles, falling back to "Romaji" if preferred language isn't available
            string xTitle = x.Titles.TryGetValue(_curLang, out string? xValue) ? xValue : x.Titles[TsundokuLanguage.Romaji];
            string yTitle = y.Titles.TryGetValue(_curLang, out string? yValue) ? yValue : y.Titles[TsundokuLanguage.Romaji];

            int titleComparison = _seriesTitleComparer.Compare(xTitle, yTitle);

            // If titles are the same, compare by DuplicateIndex
            return titleComparison != 0 ? titleComparison : x.DuplicateIndex.CompareTo(y.DuplicateIndex);
        }
    }
    
    public class SeriesValueComparer : IEqualityComparer<Series>
    {
        public bool Equals(Series? x, Series? y)
        {
            if (x == null || y == null)
                return false;

            return x.Format == y.Format
                && x.Titles.SequenceEqual(y.Titles)
                && x.Staff.SequenceEqual(y.Staff);
        }

        public int GetHashCode(Series obj)
        {
            HashCode hash = new HashCode();
            hash.Add(obj.Format);
            foreach (var title in obj.Titles)
                hash.Add(title);
            foreach (var staff in obj.Staff)
                hash.Add(staff);
            return hash.ToHashCode();
        }
    }
}
