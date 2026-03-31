using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Text.Encodings.Web;
using Tsundoku.Clients;
using Tsundoku.Helpers;
using Tsundoku.Models.Enums;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using static Tsundoku.Models.Enums.SeriesStatusModel;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;
using static Tsundoku.Models.Enums.TsundokuSortModel;

namespace Tsundoku.Models;

[method: JsonConstructor]
public sealed partial class Series(
    Dictionary<TsundokuLanguage, string> Titles,
    Dictionary<TsundokuLanguage, string> Staff,
    string Description,
    SeriesFormat Format,
    SeriesStatus Status,
    string Cover,
    Uri Link,
    HashSet<SeriesGenre> Genres,
    uint MaxVolumeCount,
    uint CurVolumeCount,
    decimal Rating,
    uint VolumesRead,
    decimal Value,
    SeriesDemographic Demographic,
    Bitmap CoverBitMap,
    string Publisher = "Unknown",
    uint DuplicateIndex = 0) : ReactiveObject, IDisposable, IEquatable<Series?>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private static readonly SeriesModelContext SeriesJsonModel = new SeriesModelContext(new JsonSerializerOptions()
    {
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    [JsonIgnore] private bool disposedValue;
    [JsonIgnore][Reactive] public partial Bitmap? CoverBitMap { get; set; } = CoverBitMap;
    public Guid Id { get; set; } = Guid.NewGuid();
    [Reactive] public partial string Publisher { get; set; } = Publisher;
    [Reactive] public partial Dictionary<TsundokuLanguage, string> Titles { get; set; } = Titles;
    [Reactive] public partial Dictionary<TsundokuLanguage, string> Staff { get; set; } = Staff;
    [Reactive] public partial uint DuplicateIndex { get; set; } = DuplicateIndex;
    [Reactive] public partial string Description { get; set; } = Description;
    [Reactive] public partial SeriesFormat Format { get; set; } = Format;
    [Reactive] public partial SeriesStatus Status { get; set; } = Status;
    [Reactive] public partial string Cover { get; set; } = Cover;
    public Uri Link { get; set; } = Link;
    [Reactive] public partial HashSet<SeriesGenre> Genres { get; set; } = Genres;
    [Reactive] public partial string SeriesNotes { get; set; } = string.Empty;
    [Reactive] public partial uint MaxVolumeCount { get; set; } = MaxVolumeCount;
    [Reactive] public partial uint CurVolumeCount { get; set; } = CurVolumeCount;
    [Reactive] public partial uint VolumesRead { get; set; } = VolumesRead;
    [Reactive] public partial decimal Value { get; set; } = Value;
    [Reactive] public partial decimal Rating { get; set; } = Rating;
    [Reactive] public partial SeriesDemographic Demographic { get; set; } = Demographic;
    [Reactive] public partial bool IsFavorite { get; set; } = false;

    /// <summary>
    /// All Chinese or Taiwanese series use "Chinese (Simplified)" and go to "Chinese"
    /// </summary>
    /// <param name="input">Title or ID of the series the user wants to add to their collection</param>
    /// <param name="bookType">Booktype of the series the user wants to add, either Manga or Light Novel</param>
    /// <param name="maxVolCount">Current max volume count of the series</param>
    /// <param name="minVolCount">Current volume count the user has for the series</param>
    /// <param name="ALQuery">AniList object for the AniList HTTP Client</param>
    /// <param name="MD_Query">MangaDex object for the MangaDex HTTP client</param>
    /// <param name="additionalLanguages">List of additional languages to query for</param>
    /// <returns></returns>
    public static async Task<Series?> CreateNewSeriesCardAsync(
        BitmapHelper bitmapHelper,
        MangaDex mangaDex,
        AniList aniList,
        string input,
        SeriesFormat bookType,
        uint maxVolCount,
        uint minVolCount,
        TsundokuLanguage[]? additionalLanguages,
        string publisher = "Unknown",
        SeriesDemographic demographic = SeriesDemographic.Unknown,
        uint volumesRead = 0,
        decimal rating = -1,
        decimal value = 0,
        string customImageUrl = "",
        bool allowDuplicate = false,
        bool isRefresh = false,
        bool isCoverImageRefresh = false,
        string? coverPath = null
    )
    {
        Series? newSeries = null;
        JsonDocument? mediaDataDoc = null;

        int pageNum = 1;
        string curMangaDexId = string.Empty;

        bool isAniList = false;
        bool isMangaDex = false;
        bool isTitleId = false;

        try
        {
            switch (input)
            {
                case string s when int.TryParse(s, out var seriesId):
                    mediaDataDoc = await aniList
                        .GetSeriesByIDAsync(seriesId, bookType, pageNum);
                    isAniList  = true;
                    isTitleId  = true;
                    break;

                case string s when MangaDex.IsMangaDexId(s):
                    mediaDataDoc = await mangaDex
                        .GetSeriesByIdAsync(s);
                    curMangaDexId = s;
                    isMangaDex = true;
                    isTitleId  = true;
                    break;

                default:
                    mediaDataDoc = null;
                    break;
            }

            // If neither ID-case matched, try “by title” on AniList, then MangaDex:
            if (!isTitleId)
            {
                mediaDataDoc = await aniList.GetSeriesByTitleAsync(input, bookType, pageNum);

                if (mediaDataDoc is not null)
                {
                    isAniList = true;
                }
                else
                {
                    mediaDataDoc = await mangaDex.GetSeriesByTitleAsync(input);
                    isMangaDex = true;
                }
            }

            // Predeclare variables for extraction later
            string countryOfOrigin = string.Empty;
            string nativeTitle = string.Empty;
            string japaneseTitle = string.Empty;
            string romajiTitle = string.Empty;
            string englishTitle = string.Empty;

            JsonElement[] mangaDexAltTitles = [];
            Dictionary<TsundokuLanguage, string> newTitles = [];

            BuildContext context = new();

            // AniList Query Check
            if (isAniList)
            {
                newSeries = await TryCreateAniListSeriesAsync(
                    aniList,
                    mangaDex,
                    input,
                    customImageUrl,
                    bookType,
                    isTitleId,
                    mediaDataDoc,
                    additionalLanguages,
                    allowDuplicate,
                    isRefresh,
                    isCoverImageRefresh,
                    maxVolCount,
                    minVolCount,
                    rating,
                    volumesRead,
                    value,
                    demographic,
                    publisher,
                    coverPath,
                    bitmapHelper,
                    context
                );

                if (!isTitleId && newSeries is null && bookType != SeriesFormat.Novel)
                {
                    mediaDataDoc = await mangaDex.GetSeriesByTitleAsync(input);
                    newSeries = await TryCreateMangaDexSeriesAsync(
                        mediaDataDoc: mediaDataDoc,
                        mangaDex: mangaDex,
                        input: input,
                        customImageUrl: customImageUrl,
                        curMangaDexId: curMangaDexId,
                        additionalLanguages: additionalLanguages,
                        allowDuplicate: allowDuplicate,
                        isRefresh: isRefresh,
                        isCoverImageRefresh: isCoverImageRefresh,
                        maxVolCount: maxVolCount,
                        minVolCount: minVolCount,
                        rating: rating,
                        volumesRead: volumesRead,
                        value: value,
                        demographic: demographic,
                        publisher: publisher,
                        bitmapHelper: bitmapHelper,
                        context: context,
                        coverPath: coverPath
                    );
                }
            }
            else if (isMangaDex && bookType != SeriesFormat.Novel) // MangaDex
            {
                newSeries = await TryCreateMangaDexSeriesAsync(
                    mediaDataDoc: mediaDataDoc,
                    mangaDex: mangaDex,
                    input: input,
                    customImageUrl: customImageUrl,
                    curMangaDexId: curMangaDexId,
                    additionalLanguages: additionalLanguages,
                    allowDuplicate: allowDuplicate,
                    isRefresh: isRefresh,
                    isCoverImageRefresh: isCoverImageRefresh,
                    maxVolCount: maxVolCount,
                    minVolCount: minVolCount,
                    rating: rating,
                    volumesRead: volumesRead,
                    value: value,
                    demographic: demographic,
                    publisher: publisher,
                    bitmapHelper: bitmapHelper,
                    context: context,
                    coverPath: coverPath
                );
            }
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Error Creating new Series for {Input}", input);
        }
        finally
        {
            mediaDataDoc?.Dispose();
        }
        return newSeries;
    }

    private static async Task<Series?> TryCreateAniListSeriesAsync(
        AniList aniList,
        MangaDex mangaDex,
        string input,
        string? customImageUrl,
        SeriesFormat bookType,
        bool isAniListId,
        JsonDocument mediaDataDoc,
        TsundokuLanguage[]? additionalLanguages,
        bool allowDuplicate,
        bool isRefresh,
        bool isCoverImageRefresh,
        uint maxVolCount,
        uint minVolCount,
        decimal rating,
        uint volumesRead,
        decimal value,
        SeriesDemographic demographic,
        string? publisher,
        string coverPath,
        BitmapHelper bitmapHelper,
        BuildContext context)
    {
        // Parse the root "Media" element
        if (!mediaDataDoc.RootElement.TryGetProperty("Media", out JsonElement mediaData))
        {
            LOGGER.Warn("AniList response missing 'Media' root element for {Input}", input);
            return null;
        }

        // Extract titles
        context.NativeTitle = GetStringOrEmpty(mediaData, "title", "native");
        context.RomajiTitle = GetStringOrEmpty(mediaData, "title", "romaji");
        context.EnglishTitle = GetStringOrEmpty(mediaData, "title", "english");

        if (!isAniListId && !IsTitleSimilar(input, context.EnglishTitle, context.RomajiTitle, context.NativeTitle))
        {
            LOGGER.Info("Not on AniList or Incorrect Entry -> Trying MangaDex");
            return null;
        }

        context.CountryOfOrigin = GetStringOrEmpty(mediaData, "countryOfOrigin");
        context.FilteredBookType = bookType == SeriesFormat.Manga
            ? SeriesFormatModel.Parse(context.CountryOfOrigin)
            : SeriesFormat.Novel;

        // Resolve cover path
        if (!isRefresh)
        {
            string imageUrl = GetStringOrEmpty(mediaData, "coverImage", "extraLarge");
            (context.CoverPath, _) = AppFileHelper.CreateUniqueCoverFileName(
                imageUrl,
                context.RomajiTitle,
                context.FilteredBookType,
                allowDuplicate,
                ref context.DupeIndex
            );
        }
        else
        {
            context.CoverPath = coverPath;
        }

        // Fallback to MangaDex to fetch Japanese title for KR/TW/CW origin
        if (bookType != SeriesFormat.Novel && IsAsianNonJapanese(context.CountryOfOrigin))
        {
            LOGGER.Info("Getting Japanese Title for Non-Japanese Series");

            using JsonDocument? altTitleDoc = await mangaDex.GetSeriesByTitleAsync(context.RomajiTitle);
            if (altTitleDoc is null)
            {
                LOGGER.Warn("Unable to get MangaDex alt titles for \"{RomajiTitle}\"", context.RomajiTitle);
                return null;
            }

            if (altTitleDoc.RootElement.TryGetProperty("data", out JsonElement altData))
            {
                context.MangaDexAltTitles = MangaDex.GetAdditionalMangaDexTitleList(altData, context.EnglishTitle, context.NativeTitle);
            }

            if (!MangaDex.TryGetAltTitle("ja", context.MangaDexAltTitles, out context.JapaneseTitle))
            {
                LOGGER.Warn("{RomajiTitle} at MangaDex has no japanese title", context.RomajiTitle);
            }
        }

        // Extract staff with pagination
        bool disallowIllustrationStaff = input.Contains("Anthology", StringComparison.OrdinalIgnoreCase);
        (string fullStaff, string nativeStaff) = await ExtractAllAniListStaffAsync(
            aniList, mediaData, input, bookType, isAniListId, disallowIllustrationStaff);

        // Populate titles
        context.AddTitle(context.RomajiTitle, TsundokuLanguage.Romaji);
        context.AddTitle(context.EnglishTitle, TsundokuLanguage.English);
        context.AddTitle(context.JapaneseTitle, TsundokuLanguage.Japanese);
        if (ANILIST_LANG_CODES.TryGetValue(context.CountryOfOrigin, out TsundokuLanguage nativeLang))
        {
            context.AddTitle(context.NativeTitle, nativeLang);
        }

        // Add additional languages from MangaDex if requested
        if (additionalLanguages is not null && additionalLanguages.Length > 0)
        {
            JsonElement[] altTitles = context.MangaDexAltTitles;

            if (altTitles.Length == 0)
            {
                using JsonDocument? altTitlesDoc = await mangaDex.GetSeriesByTitleAsync(context.RomajiTitle);
                if (altTitlesDoc is null)
                {
                    LOGGER.Warn("Unable to get MangaDex info for {Title}", context.RomajiTitle);
                    return null;
                }

                if (altTitlesDoc.RootElement.TryGetProperty("data", out JsonElement altData))
                {
                    altTitles = MangaDex.GetAdditionalMangaDexTitleList(altData, context.EnglishTitle, context.NativeTitle);
                }
            }

            AddAdditionalLanguages(ref context.NewTitles, additionalLanguages, altTitles);
        }

        // Build staff dictionary
        context.AddStaff(fullStaff, TsundokuLanguage.Romaji);
        if (ANILIST_LANG_CODES.TryGetValue(context.CountryOfOrigin, out TsundokuLanguage staffLang))
        {
            context.AddStaff(nativeStaff, staffLang);
        }

        // Extract remaining metadata
        string coverImageUrl = string.IsNullOrWhiteSpace(customImageUrl)
            ? GetStringOrEmpty(mediaData, "coverImage", "extraLarge")
            : customImageUrl;

        string desc = mediaData.TryGetProperty("description", out JsonElement descriptionProp)
            ? AniList.ParseSeriesDescription(descriptionProp.GetString())
            : string.Empty;

        SeriesStatus status = SeriesStatusModel.Parse(GetStringOrEmpty(mediaData, "status"));
        Uri link = new Uri(GetStringOrEmpty(mediaData, "siteUrl"));

        HashSet<SeriesGenre> genres = mediaData.TryGetProperty("genres", out JsonElement genresElement)
            ? AniList.ParseGenreArray(context.RomajiTitle, genresElement)
            : [];

        Bitmap? coverImage = null;

        if (isCoverImageRefresh || (!isRefresh && (allowDuplicate || !string.IsNullOrWhiteSpace(context.CoverPath))))
        {
            coverImage = await bitmapHelper.UpdateCoverFromUrlAsync(
                coverImageUrl,
                AppFileHelper.GetFullCoverPath(context.CoverPath)
            );

            if (coverImage is null)
            {
                LOGGER.Warn("Expected cover image generation to succeed, but it returned null");
            }
        }

        return new Series(
            Titles: context.NewTitles,
            Staff: context.NewStaff,
            Description: desc,
            Format: context.FilteredBookType,
            Status: status,
            Cover: context.CoverPath,
            Link: link,
            Genres: genres,
            MaxVolumeCount: maxVolCount,
            CurVolumeCount: minVolCount,
            Rating: rating,
            VolumesRead: volumesRead,
            Value: value,
            Demographic: demographic,
            CoverBitMap: coverImage,
            Publisher: publisher ?? "Unknown",
            DuplicateIndex: context.DupeIndex
        );
    }

    private static async Task<Series?> TryCreateMangaDexSeriesAsync(
        JsonDocument mediaDataDoc,
        MangaDex mangaDex,
        string input,
        string? customImageUrl,
        string? curMangaDexId,
        TsundokuLanguage[]? additionalLanguages,
        bool allowDuplicate,
        bool isRefresh,
        bool isCoverImageRefresh,
        uint maxVolCount,
        uint minVolCount,
        decimal rating,
        uint volumesRead,
        decimal value,
        SeriesDemographic demographic,
        string? publisher,
        string coverPath,
        BitmapHelper bitmapHelper,
        BuildContext context)
    {
        if (!mediaDataDoc.RootElement.TryGetProperty("data", out JsonElement data))
        {
            LOGGER.Warn("MangaDex response missing 'data' for {Input}", input);
            return null;
        }

        if (data.ValueKind == JsonValueKind.Array)
        {
            LOGGER.Debug("Multiple series entries returned");
            data = MangaDex.TryFindValidMangaDexSeries(data, input);
        }

        if (data.ValueKind != JsonValueKind.Object)
        {
            LOGGER.Warn("Series {Input}, is not on MangaDex, returned {Type}", input, data.ValueKind);
            return null;
        }

        if (string.IsNullOrEmpty(curMangaDexId))
        {
            curMangaDexId = GetStringOrEmpty(data, "id");
        }

        if (!data.TryGetProperty("attributes", out JsonElement attributesBlock))
        {
            LOGGER.Warn("Series {Input} missing 'attributes' block", input);
            return null;
        }

        if (!attributesBlock.TryGetProperty("title", out _))
        {
            LOGGER.Warn("Series {Input} missing 'title' block", input);
            return null;
        }

        JsonElement[] altTitleList = attributesBlock.TryGetProperty("altTitles", out JsonElement altTitlesEl)
            ? [.. altTitlesEl.EnumerateArray()]
            : [];

        context.EnglishTitle = GetStringOrEmpty(attributesBlock, "title", "en");
        context.CountryOfOrigin = GetStringOrEmpty(attributesBlock, "originalLanguage");
        context.RomajiTitle = MangaDex.GetAltTitle("ja-ro", altTitleList) ?? context.EnglishTitle;
        context.NativeTitle = MangaDex.GetAltTitle(context.CountryOfOrigin, altTitleList) ?? context.RomajiTitle;
        context.FilteredBookType = SeriesFormatModel.Parse(context.CountryOfOrigin);

        if (!MangaDex.TryGetAltTitle("ja", altTitleList, out context.JapaneseTitle))
        {
            context.JapaneseTitle = string.Empty;
        }

        // Extract cover art ID from relationships
        JsonElement[] relationships = data.TryGetProperty("relationships", out JsonElement relsEl)
            ? [.. relsEl.EnumerateArray()]
            : [];

        JsonElement coverArtRel = relationships
            .AsValueEnumerable()
            .FirstOrDefault(static x =>
                x.TryGetProperty("type", out var typeProp) &&
                typeProp.ValueEquals("cover_art"));

        string coverId = coverArtRel.ValueKind == JsonValueKind.Object
            ? GetStringOrEmpty(coverArtRel, "id")
            : string.Empty;

        string seriesId = string.IsNullOrEmpty(curMangaDexId) ? GetStringOrEmpty(data, "id") : curMangaDexId;

        // Resolve cover URL (prefer custom, fallback to generated)
        string coverUrl = string.IsNullOrWhiteSpace(customImageUrl)
            ? await mangaDex.GetCoverLinkAsync(coverId, seriesId, context.RomajiTitle)
            : customImageUrl;

        // Conditionally generate cover file path
        (context.CoverPath, bool isDupe) = !isRefresh
            ? AppFileHelper.CreateUniqueCoverFileName(
                coverUrl,
                context.RomajiTitle,
                context.FilteredBookType,
                allowDuplicate,
                ref context.DupeIndex)
            : (coverPath, false);

        // Titles
        Dictionary<TsundokuLanguage, string> titles = context.NewTitles;
        titles[TsundokuLanguage.Romaji] = context.RomajiTitle;
        titles[TsundokuLanguage.English] = context.EnglishTitle;

        // Japanese title only for non-Japanese-origin works
        if (!string.IsNullOrWhiteSpace(context.JapaneseTitle))
        {
            titles[TsundokuLanguage.Japanese] = context.JapaneseTitle;
        }

        // Native language title (e.g., "ko", "zh-hk") if not English
        if (!"en".Equals(context.CountryOfOrigin, StringComparison.Ordinal) &&
            MANGADEX_LANG_CODES.TryGetValue(context.CountryOfOrigin, out TsundokuLanguage mdNativeLang) &&
            MangaDex.TryGetAltTitle(context.CountryOfOrigin, altTitleList, out string nativeAlt))
        {
            titles[mdNativeLang] = nativeAlt;
        }

        // Additional languages (only if specified)
        if (additionalLanguages is not null && additionalLanguages.Length > 0)
        {
            AddAdditionalLanguages(ref titles, additionalLanguages, altTitleList);
        }

        // Staff
        (StringBuilder fullStaff, StringBuilder nativeStaff) = await mangaDex.GetStaffAsync(relationships, context.RomajiTitle);
        if (fullStaff.Length > 0)
        {
            context.NewStaff[TsundokuLanguage.Romaji] = fullStaff.ToString();
        }

        if (nativeStaff.Length > 0 && MANGADEX_LANG_CODES.TryGetValue(context.CountryOfOrigin, out TsundokuLanguage mdStaffLang))
        {
            context.NewStaff[mdStaffLang] = nativeStaff.ToString();
        }

        string desc = 
            attributesBlock.TryGetProperty("description", out JsonElement descObj) &&
            descObj.ValueKind == JsonValueKind.Object
                ? MangaDex.ParseDescription(
                    // pick "en", or fallback to original language, or empty
                    (descObj.TryGetProperty("en", out JsonElement e) ? e :
                    descObj.TryGetProperty(context.CountryOfOrigin, out JsonElement f) ? f :
                    default)
                    .GetString() ?? string.Empty
                )
                : string.Empty;
            
        SeriesStatus status = SeriesStatusModel.Parse(GetStringOrEmpty(attributesBlock, "status"));

        Uri link = MangaDex.ConstructMangaLink(attributesBlock, data, curMangaDexId);
        HashSet<SeriesGenre> genres = attributesBlock.TryGetProperty("tags", out JsonElement tagsElement)
            ? MangaDex.ParseGenreData(context.RomajiTitle, tagsElement)
            : [];

        Bitmap? coverImage = null;
        if (isCoverImageRefresh || (!isRefresh && (!isDupe || allowDuplicate)))
        {
            coverImage = await bitmapHelper.UpdateCoverFromUrlAsync(coverUrl, AppFileHelper.GetFullCoverPath(context.CoverPath));
            if (coverImage is null)
            {
                LOGGER.Warn("Expected cover image generation to succeed, but it returned null");
            }
        }

        return new Series(
            Titles: context.NewTitles,
            Staff: context.NewStaff,
            Description: desc,
            Format: context.FilteredBookType,
            Status: status,
            Cover: context.CoverPath,
            Link: link,
            Genres: genres,
            MaxVolumeCount: maxVolCount,
            CurVolumeCount: minVolCount,
            Rating: rating,
            VolumesRead: volumesRead,
            Value: value,
            Demographic: demographic,
            CoverBitMap: coverImage,
            Publisher: publisher ?? "Unknown",
            DuplicateIndex: context.DupeIndex
        );
    }

    public void IncrementCurVolumeCount()
    {
        if (this.CurVolumeCount < this.MaxVolumeCount)
        {
            this.CurVolumeCount++;
        }
    }

    public void DecrementCurVolumeCount()
    {
        if (this.CurVolumeCount > 0)
        {
            this.CurVolumeCount--;
        }
    }

    public void UpdateVolumeCounts(uint newCur, uint newMax, Action? action = null)
    {
        if (newMax >= newCur)
        {
            uint oldCur = this.CurVolumeCount;
            uint oldMax = this.MaxVolumeCount;
            this.CurVolumeCount = newCur;
            this.MaxVolumeCount = newMax;

            LOGGER.Info(
                "Changed Series Volume Counts For {RomajiTitle} From {OldCur}/{OldMax} -> {NewCur}/{NewMax}",
                this.Titles[TsundokuLanguage.Romaji], oldCur, oldMax, newCur, newMax
            );

            action();
        }
        else
        {
            LOGGER.Warn("{NewCur} cannot be greater than {NewMax}", newCur, newMax);
        }
    }

    public void UpdateCurVolumeCount(uint newCur, Action? action = null)
    {
        if (newCur <= this.MaxVolumeCount)
        {
            uint oldCur = this.CurVolumeCount;
            this.CurVolumeCount = newCur;

            LOGGER.Info(
                "Updated Series Current Volume Count For {RomajiTitle} From {OldCur} to {NewCur}",
                this.Titles[TsundokuLanguage.Romaji], oldCur, newCur
            );

            action();
        }
        else
        {
            LOGGER.Warn("{NewCur} cannot be greater than {MaxVolumeCount}", newCur, this.MaxVolumeCount);
        }
    }

    public void UpdateMaxVolumeCount(uint newMax, Action? action = null)
    {
        if (newMax >= this.CurVolumeCount)
        {
            uint oldMax = this.MaxVolumeCount;
            this.MaxVolumeCount = newMax;

            LOGGER.Info(
                "Updated Series Max Volume Count For {RomajiTitle} From {OldMax} to {NewMax}",
                this.Titles[TsundokuLanguage.Romaji], oldMax, newMax
            );

            action();
        }
        else
        {
            LOGGER.Warn("{NewMax} cannot be less than {CurVolumeCount}", newMax, this.CurVolumeCount);
        }
    }

    /// <summary>
    /// Determines whether the country of origin is an Asian region other than Japan (e.g., Korea, Taiwan, China).
    /// </summary>
    /// <param name="origin">The country code (e.g., "JP", "KR", "TW", "CW").</param>
    /// <returns>
    /// <c>true</c> if the origin is Korean, Taiwanese, or Chinese (Simplified/Traditional); otherwise, <c>false</c>.
    /// </returns>
    private static bool IsAsianNonJapanese(string origin)
    {
        return origin == "KR" || origin == "CW" || origin == "TW";
    }

    /// <summary>
    /// Adds additional titles to the provided dictionary based on the specified alternate languages
    /// and available altTitles from MangaDex. Only languages not already present in the dictionary
    /// will be added.
    /// </summary>
    /// <param name="newTitles">Reference to the dictionary of titles to update.</param>
    /// <param name="additionalLanguages">Array of requested additional languages to consider.</param>
    /// <param name="altTitles">Flat list of MangaDex altTitle JsonElements to search through.</param>
    public static void AddAdditionalLanguages(
        ref Dictionary<TsundokuLanguage, string> newTitles,
        TsundokuLanguage[] additionalLanguages,
        JsonElement[] altTitles)
    {
        // Create lookup for faster contains checks
        HashSet<TsundokuLanguage> requestedLangs = [.. additionalLanguages];

        foreach ((string mdLangCode, TsundokuLanguage tsuLang) in MANGADEX_LANG_CODES)
        {
            if (!requestedLangs.Contains(tsuLang) || newTitles.ContainsKey(tsuLang))
            {
                continue;
            }

            foreach (JsonElement entry in altTitles)
            {
                if (entry.TryGetProperty(mdLangCode, out JsonElement altValue) &&
                    altValue.ValueKind == JsonValueKind.String)
                {
                    string? value = altValue.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        LOGGER.Debug("Added [{Lang} : {Value}]", tsuLang, value);
                        newTitles[tsuLang] = value;
                        break;
                    }
                }
            }
        }
    }

    public void DeleteCover()
    {
        AppFileHelper.DeleteCoverFile(this.Cover);
    }

    public void UpdateCover(Bitmap newCover)
    {
        RxSchedulers.MainThreadScheduler.Schedule(() =>
        {
            this.CoverBitMap?.Dispose();
            this.CoverBitMap = newCover;
        });
    }

    public TsundokuLanguage[] SeriesContainsAdditionalLanagues()
    {
        return LANGUAGES
            .AsValueEnumerable()
            .Skip(3)
            .Where(Titles.ContainsKey)
            .ToArray();
    }

    public bool HasGenre(SeriesGenre genre)
    {
        return this.Genres is not null && this.Genres.Contains(genre);
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, typeof(Series), SeriesJsonModel);
    }

    public bool IsCoverImageEmpty()
    {
        return this.CoverBitMap is null || this.CoverBitMap.PixelSize.Width == 0 || this.CoverBitMap.PixelSize.Height == 0;
    }

    public void UpdateFrom(Series other, bool isCoverEmpty)
    {
        // 1. Titles (Dictionary<TsundokuLanguage, string>) – deep‐compare by count + per‐key/value
        if (!ExtensionMethods.DictionariesEqual(Titles, other.Titles))
        {
            Titles = new Dictionary<TsundokuLanguage, string>(other.Titles);
        }

        // 2. Staff (Dictionary<TsundokuLanguage, string>) – deep‐compare
        if (!ExtensionMethods.DictionariesEqual(Staff, other.Staff))
        {
            Staff = new Dictionary<TsundokuLanguage, string>(other.Staff);
        }

        // 3. Description (string)
        if (Description != other.Description)
        {
            Description = other.Description;
        }

        // 4. Format (enum or struct)
        if (Format != other.Format)
        {
            Format = other.Format;
        }

        // 5. Status (enum or struct)
        if (Status != other.Status)
        {
            Status = other.Status;
        }

        // 6. Cover (string)
        if (Cover != other.Cover)
        {
            Cover = other.Cover;
        }

        // 7. Link (Uri)
        if (!Equals(Link, other.Link))
        {
            Link = other.Link;
        }

        // 8. Genres (HashSet<Genre>) – deep‐compare via SetEquals
        if (!ExtensionMethods.HashSetsEqual(Genres, other.Genres))
        {
            Genres = [.. other.Genres];
        }

        // 9. MaxVolumeCount (ushort)
        if (MaxVolumeCount != other.MaxVolumeCount)
        {
            MaxVolumeCount = other.MaxVolumeCount;
        }

        // 10. CurVolumeCount (ushort)
        if (CurVolumeCount != other.CurVolumeCount)
        {
            CurVolumeCount = other.CurVolumeCount;
        }

        // 11. Rating (decimal)
        if (Rating != other.Rating)
        {
            Rating = other.Rating;
        }

        // 12. VolumesRead (uint)
        if (VolumesRead != other.VolumesRead)
        {
            VolumesRead = other.VolumesRead;
        }

        // 13. Value (decimal)
        if (Value != other.Value)
        {
            Value = other.Value;
        }

        // 14. Demographic (enum or struct)
        if (Demographic != other.Demographic)
        {
            Demographic = other.Demographic;
        }

        // 15. Publisher (string)
        if (Publisher != other.Publisher)
        {
            Publisher = other.Publisher;
        }

        // 16. DuplicateIndex (uint)
        if (DuplicateIndex != other.DuplicateIndex)
        {
            DuplicateIndex = other.DuplicateIndex;
        }

        // 17. Cover Image
        if (isCoverEmpty)
        {
            Bitmap? newCover = other.CoverBitMap?.CloneBitmap();
            if (newCover is null)
            {
                // Fallback: load from file if the bitmap wasn't in memory
                string fullPath = AppFileHelper.GetFullCoverPath(other.Cover);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        newCover = new Bitmap(fullPath);
                    }
                    catch (Exception ex)
                    {
                        LOGGER.Warn(ex, "Failed to load cover from file {Path}", fullPath);
                    }
                }
            }
            if (newCover is not null)
            {
                UpdateCover(newCover);
            }
        }
    }

    private void Dispose(bool disposing, bool deleteCover = true)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.CoverBitMap?.Dispose();
                if (deleteCover)
                {
                    DeleteCover();
                }
            }

            this.CoverBitMap = null;
            this.disposedValue = true;
        }
    }

    public void Dispose(bool deleteCover)
    {
        Dispose(disposing: true, deleteCover);
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
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

    /// <summary>
    /// Safely retrieves a string from a nested JSON property path, returning empty string if any key is missing.
    /// </summary>
    private static string GetStringOrEmpty(JsonElement root, params ReadOnlySpan<string> path)
    {
        JsonElement current = root;
        foreach (string key in path)
        {
            if (!current.TryGetProperty(key, out current))
            {
                return string.Empty;
            }
        }
        return current.GetString() ?? string.Empty;
    }

    /// <summary>
    /// Checks whether the input title is similar to any of the provided candidate titles.
    /// </summary>
    private static bool IsTitleSimilar(string input, params ReadOnlySpan<string> candidates)
    {
        foreach (string title in candidates)
        {
            if (string.IsNullOrWhiteSpace(title)) continue;
            if (title.Contains(input, StringComparison.OrdinalIgnoreCase)) return true;

            string normalized = title.Replace("and", "&");
            if (ExtensionMethods.Similar(input, normalized, ExtensionMethods.SimilarThreshold(input, title)) != -1)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Extracts all staff pages from AniList, handling pagination.
    /// </summary>
    private static async Task<(string FullStaff, string NativeStaff)> ExtractAllAniListStaffAsync(
        AniList aniList,
        JsonElement firstMediaData,
        string input,
        SeriesFormat bookType,
        bool isAniListId,
        bool disallowIllustrationStaff)
    {
        string nativeStaff = string.Empty;
        string fullStaff = string.Empty;
        int pageNum = 1;

        bool hasNextPage = AniList.ExtractStaffFromAniList(firstMediaData, ref nativeStaff, ref fullStaff, disallowIllustrationStaff);
        while (hasNextPage)
        {
            pageNum++;
            LOGGER.Debug("Getting staff for page {num}", pageNum);
            using JsonDocument? moreStaffDoc = isAniListId
                ? await aniList.GetSeriesByIDAsync(int.Parse(input), bookType, pageNum)
                : await aniList.GetSeriesByTitleAsync(input, bookType, pageNum);

            if (moreStaffDoc is null)
            {
                LOGGER.Warn("AniList returned null when fetching additional staff (page {Page}) for {Title}", pageNum, input);
                break;
            }

            JsonElement moreMedia = moreStaffDoc.RootElement.GetProperty("Media");
            hasNextPage = AniList.ExtractStaffFromAniList(moreMedia, ref nativeStaff, ref fullStaff, disallowIllustrationStaff);
        }

        return (fullStaff, nativeStaff);
    }

    private sealed class BuildContext
    {
        public string NativeTitle = string.Empty;
        public string RomajiTitle = string.Empty;
        public string EnglishTitle = string.Empty;
        public string JapaneseTitle = string.Empty;
        public string CountryOfOrigin = string.Empty;
        public string CoverPath = string.Empty;

        public SeriesFormat FilteredBookType;
        public uint DupeIndex;

        public JsonElement[] MangaDexAltTitles = [];
        public Dictionary<TsundokuLanguage, string> NewTitles = [];
        public Dictionary<TsundokuLanguage, string> NewStaff = [];

        public void AddTitle(string? title, TsundokuLanguage lang)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                NewTitles[lang] = title;
            }
        }

        public void AddStaff(string? staff, TsundokuLanguage lang)
        {
            if (!string.IsNullOrWhiteSpace(staff) && staff != " | ")
            {
                NewStaff[lang] = staff;
            }
        }
    }
}