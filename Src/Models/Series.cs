using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Text.Encodings.Web;
using Tsundoku.Clients;
using Tsundoku.Helpers;
using Tsundoku.Models.Enums;
using static Tsundoku.Models.Enums.SeriesDemographicEnum;
using static Tsundoku.Models.Enums.SeriesFormatEnum;
using static Tsundoku.Models.Enums.SeriesGenreEnum;
using static Tsundoku.Models.Enums.SeriesStatusEnum;
using static Tsundoku.Models.TsundokuLanguageModel;

namespace Tsundoku.Models;

public sealed partial class Series : ReactiveObject, IDisposable, IEquatable<Series?>
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
    [JsonIgnore][Reactive] public Bitmap? CoverBitMap { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    [Reactive] public string Publisher { get; set; }
    [Reactive] public Dictionary<TsundokuLanguage, string> Titles { get; set; }
    [Reactive] public Dictionary<TsundokuLanguage, string> Staff { get; set; }
    [Reactive] public uint DuplicateIndex { get; set; }
    [Reactive] public string Description { get; set; }
    [Reactive] public SeriesFormat Format { get; set; }
    [Reactive] public SeriesStatus Status { get; set; }
    [Reactive] public string Cover { get; set; }
    public Uri Link { get; set; }
    [Reactive] public HashSet<SeriesGenre>? Genres { get; set; }
    [Reactive] public string SeriesNotes { get; set; }
    [Reactive] public ushort MaxVolumeCount { get; set; }
    [Reactive] public ushort CurVolumeCount { get; set; }
    [Reactive] public uint VolumesRead { get; set; }
    [Reactive] public decimal Value { get; set; }
    [Reactive] public decimal Rating { get; set; }
    [Reactive] public SeriesDemographic Demographic { get; set; }
    [Reactive] public bool IsFavorite { get; set; } = false;

    [JsonConstructor]
    public Series(
        Dictionary<TsundokuLanguage, string> Titles,
        Dictionary<TsundokuLanguage, string> Staff,
        string Description,
        SeriesFormat Format,
        SeriesStatus Status,
        string Cover,
        Uri Link,
        HashSet<SeriesGenre> Genres,
        ushort MaxVolumeCount,
        ushort CurVolumeCount,
        decimal Rating,
        uint VolumesRead,
        decimal Value,
        SeriesDemographic Demographic,
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
    /// <param name="input">Title or ID of the series the user wants to add to their collection</param>
    /// <param name="bookType">Booktype of the series the user wants to add, either Manga or Light Novel</param>
    /// <param name="maxVolCount">Current max volume count of the series</param>
    /// <param name="minVolCount">Current volume count the user has for the series</param>
    /// <param name="ALQuery">AniList object for the AniList HTTP Client</param>
    /// <param name="MD_Query">MangaDex object for the MangaDex HTTP client</param>
    /// <param name="additionalLanguages">List of additional languages to query for</param>
    /// <returns></returns>
    // TODO - Make it so it doesn't download the image until it's confirmed that the series is not a dupe
    public static async Task<Series?> CreateNewSeriesCardAsync(
        BitmapHelper bitmapHelper,
        MangaDex mangaDex,
        AniList aniList,
        string input,
        SeriesFormat bookType,
        ushort maxVolCount,
        ushort minVolCount,
        TsundokuLanguage[] additionalLanguages,
        string publisher = "Unknown",
        SeriesDemographic demographic = SeriesDemographic.Unknown,
        uint volumesRead = 0,
        decimal rating = -1,
        decimal value = 0,
        string customImageUrl = "",
        bool allowDuplicate = false,
        bool isRefresh = false
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
            if (int.TryParse(input, out int seriesId)) // Check AniList ID
            {
                // Numeric title is treated as AniList ID
                mediaDataDoc = await aniList.GetSeriesByIDAsync(seriesId, bookType, pageNum);
                isAniList = true;
                isTitleId = true;
            }
            else if (MangaDex.IsMangaDexId(input)) // Check MangaDex Id
            {
                mediaDataDoc = await mangaDex.GetSeriesByIdAsync(input);
                curMangaDexId = input;
                isMangaDex = true;
                isTitleId = true;
            }
            else
            {
                mediaDataDoc = await aniList.GetSeriesByTitleAsync(input, bookType, pageNum); // Check AniList Title
                if (mediaDataDoc == null)
                {
                    mediaDataDoc = await mangaDex.GetSeriesByTitleAsync(input); // Check MangaDex Title
                    isMangaDex = true;
                }
                else
                {
                    isAniList = true;
                }
            }

            // Predeclare variables for extraction later
            string countryOfOrigin = string.Empty;
            string nativeTitle = string.Empty;
            string japaneseTitle = string.Empty;
            string romajiTitle = string.Empty;
            string englishTitle = string.Empty;
            string coverPath = string.Empty;

            JsonElement[] mangaDexAltTitles = [];
            Dictionary<TsundokuLanguage, string> newTitles = [];

            BuildContext context = new BuildContext();

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
                    maxVolCount,
                    minVolCount,
                    rating,
                    volumesRead,
                    value,
                    demographic,
                    publisher,
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
                        maxVolCount: maxVolCount,
                        minVolCount: minVolCount,
                        rating: rating,
                        volumesRead: volumesRead,
                        value: value,
                        demographic: demographic,
                        publisher: publisher,
                        bitmapHelper: bitmapHelper,
                        context: context
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
                    maxVolCount: maxVolCount,
                    minVolCount: minVolCount,
                    rating: rating,
                    volumesRead: volumesRead,
                    value: value,
                    demographic: demographic,
                    publisher: publisher,
                    bitmapHelper: bitmapHelper,
                    context: context
                );
            }
        }
        catch (Exception ex)
        {
            // TODO - Add some of the input variables here
            LOGGER.Error(ex, "Error Creating new Series");
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
        TsundokuLanguage[] additionalLanguages,
        bool allowDuplicate,
        bool isRefresh,
        ushort maxVolCount,
        ushort minVolCount,
        decimal rating,
        uint volumesRead,
        decimal value,
        SeriesDemographic demographic,
        string? publisher,
        BitmapHelper bitmapHelper,
        BuildContext context)
    {
        void AddIfNotEmpty(string? title, TsundokuLanguage lang)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                context.NewTitles[lang] = title;
            }
        }

        void AddStaffIfValid(string? staff, TsundokuLanguage lang)
        {
            if (!string.IsNullOrWhiteSpace(staff) && staff != " | ")
            {
                context.NewStaff[lang] = staff;
            }
        }

        int pageNum = 1;

        string nativeStaff = string.Empty;
        string fullStaff = string.Empty;

        JsonElement mediaData = mediaDataDoc.RootElement.GetProperty("Media");
        JsonElement titleElement = mediaData.GetProperty("title");

        context.NativeTitle = titleElement.GetProperty("native").GetString();
        context.RomajiTitle = titleElement.GetProperty("romaji").GetString();
        context.EnglishTitle = titleElement.GetProperty("english").GetString();

        bool isSimilar = false;
        if (!string.IsNullOrWhiteSpace(context.EnglishTitle))
        {
            isSimilar |= ExtensionMethods.Similar(input, context.EnglishTitle, ExtensionMethods.SimilarThreshold(input, context.EnglishTitle)) != -1;
        }
        if (!string.IsNullOrWhiteSpace(context.RomajiTitle))
        {
            isSimilar |= ExtensionMethods.Similar(input, context.RomajiTitle, ExtensionMethods.SimilarThreshold(input, context.RomajiTitle)) != -1;
        }
        if (!string.IsNullOrWhiteSpace(context.NativeTitle))
        {
            isSimilar |= ExtensionMethods.Similar(input, context.NativeTitle, ExtensionMethods.SimilarThreshold(input, context.NativeTitle)) != -1;
        }

        if (!isAniListId && !isSimilar)
        {
            LOGGER.Info("Not on AniList or Incorrect Entry -> Trying MangaDex");
            return null;
        }

        context.CountryOfOrigin = mediaData.GetProperty("countryOfOrigin").GetString();
        context.FilteredBookType = bookType == SeriesFormat.Manga
            ? SeriesFormatEnum.Parse(context.CountryOfOrigin)
            : SeriesFormat.Novel;

        if (!isRefresh)
        {
            string imageUrl = mediaData
                .GetProperty("coverImage")
                .GetProperty("extraLarge")
                .GetString();

            (context.CoverPath, _) = AppFileHelper.CreateUniqueCoverFileName(
                imageUrl,
                context.RomajiTitle,
                context.FilteredBookType,
                allowDuplicate,
                ref context.DupeIndex
            );
        }

        // Fallback to MangaDex to fetch Japanese title for KR/TW/CW origin
        if (bookType != SeriesFormat.Novel && IsAsianNonJapanese(context.CountryOfOrigin))
        {
            LOGGER.Info("Getting Japanese Title for Non-Japanese Series");

            JsonDocument? altTitleDoc = await mangaDex.GetSeriesByTitleAsync(context.RomajiTitle);
            if (altTitleDoc is null)
            {
                LOGGER.Warn("Unable to get MangaDex alt titles for \"{RomajiTitle}\"", context.RomajiTitle);
                return null;
            }

            context.MangaDexAltTitles = MangaDex.GetAdditionalMangaDexTitleList(altTitleDoc.RootElement.GetProperty("data"), context.EnglishTitle, context.NativeTitle);

            if (!MangaDex.TryGetAltTitle("ja", context.MangaDexAltTitles, out context.JapaneseTitle))
            {
                LOGGER.Warn("{RomajiTitle} at MangaDex has no japanese title", context.RomajiTitle);
            }
        }

        bool disallowIllustrationStaff = input.Contains("Anthology", StringComparison.OrdinalIgnoreCase);
        bool hasNextPage = AniList.ExtractStaffFromAniList(mediaData, ref nativeStaff, ref fullStaff, disallowIllustrationStaff);
        while (hasNextPage) // Continue fetching paged staff
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

        // Populate titles
        AddIfNotEmpty(context.RomajiTitle, TsundokuLanguage.Romaji);
        AddIfNotEmpty(context.EnglishTitle, TsundokuLanguage.English);
        AddIfNotEmpty(context.JapaneseTitle, TsundokuLanguage.Japanese);
        AddIfNotEmpty(context.NativeTitle, ANILIST_LANG_CODES[context.CountryOfOrigin]);

        // Add additional languages from MangaDex if requested
        if (additionalLanguages.Length > 0)
        {
            JsonElement[] altTitles = context.MangaDexAltTitles;

            if (altTitles.Length == 0)
            {
                JsonDocument? altTitlesDoc = await mangaDex.GetSeriesByTitleAsync(context.RomajiTitle);
                if (altTitlesDoc == null)
                {
                    LOGGER.Warn("Unable to get MangaDex info for {Title}", context.RomajiTitle);
                    return null;
                }

                altTitles = MangaDex.GetAdditionalMangaDexTitleList(
                    altTitlesDoc.RootElement.GetProperty("data"),
                    context.EnglishTitle,
                    context.NativeTitle
                );
            }

            AddAdditionalLanguages(ref context.NewTitles, additionalLanguages, altTitles);
        }

        // Build staff dictionary
        AddStaffIfValid(fullStaff, TsundokuLanguage.Romaji);
        AddStaffIfValid(nativeStaff, ANILIST_LANG_CODES[context.CountryOfOrigin]);

        // Download cover image if needed
        string coverImageUrl = string.IsNullOrWhiteSpace(customImageUrl)
            ? mediaData.GetProperty("coverImage").GetProperty("extraLarge").GetString()
            : customImageUrl;

        Bitmap? coverImage = null;

        if (!isRefresh && (allowDuplicate || !string.IsNullOrWhiteSpace(context.CoverPath)))
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
            Description: AniList.ParseSeriesDescription(mediaData.GetProperty("description").GetString()),
            Format: context.FilteredBookType,
            Status: SeriesStatusEnum.Parse(mediaData.GetProperty("status").GetString()),
            Cover: context.CoverPath,
            Link: new Uri(mediaData.GetProperty("siteUrl").GetString()),
            Genres: AniList.ParseGenreArray(context.RomajiTitle, mediaData.GetProperty("genres")),
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
        TsundokuLanguage[] additionalLanguages,
        bool allowDuplicate,
        bool isRefresh,
        ushort maxVolCount,
        ushort minVolCount,
        decimal rating,
        uint volumesRead,
        decimal value,
        SeriesDemographic demographic,
        string? publisher,
        BitmapHelper bitmapHelper,
        BuildContext context)
    {
        JsonElement data = mediaDataDoc.RootElement.GetProperty("data");
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

        if (string.IsNullOrEmpty(curMangaDexId) && data.TryGetProperty("id", out JsonElement idElement))
        {
            curMangaDexId = idElement.GetString();
        }

        // attributes
        if (!data.TryGetProperty("attributes", out JsonElement attributes))
        {
            LOGGER.Warn("Series {Input} missing 'attributes' block", input);
            return null;
        }
        else if (!attributes.TryGetProperty("title", out JsonElement titleBlock))
        {
            LOGGER.Warn("Series {Input} missing 'title' block", input);
            return null;
        }

        curMangaDexId = string.IsNullOrEmpty(curMangaDexId) ? data.GetProperty("id").GetString() : curMangaDexId;

        JsonElement attributesBlock = data.GetProperty("attributes");
        JsonElement[] altTitleList = [.. attributesBlock.GetProperty("altTitles").EnumerateArray()];

        context.EnglishTitle = attributesBlock.GetProperty("title").GetProperty("en").GetString();
        context.CountryOfOrigin = attributesBlock.GetProperty("originalLanguage").GetString();
        context.RomajiTitle = MangaDex.GetAltTitle("ja-ro", altTitleList) ?? context.EnglishTitle;
        context.NativeTitle = MangaDex.GetAltTitle(context.CountryOfOrigin, altTitleList) ?? context.RomajiTitle;
        context.FilteredBookType = SeriesFormatEnum.Parse(context.CountryOfOrigin);

        if (!MangaDex.TryGetAltTitle("ja", altTitleList, out context.JapaneseTitle))
        {
            context.JapaneseTitle = string.Empty;
        }

        // Update cover path
        JsonElement[] relationships = [.. data.GetProperty("relationships").EnumerateArray()];
        string? coverId = relationships
            .AsValueEnumerable()
            .FirstOrDefault(static x =>
                x.TryGetProperty("type", out var typeProp) &&
                typeProp.ValueEquals("cover_art"))
            .GetProperty("id")
            .GetString();

        // Resolve cover URL (prefer custom, fallback to generated)
        string coverUrl = string.IsNullOrWhiteSpace(customImageUrl)
            ? await mangaDex.GetCoverLinkAsync(
                coverId,
                curMangaDexId ?? data.GetProperty("id").GetString(),
                context.RomajiTitle)
            : customImageUrl;

        // Conditionally generate cover file path
        (context.CoverPath, bool isDupe) = !isRefresh
            ? AppFileHelper.CreateUniqueCoverFileName(
                coverUrl,
                context.RomajiTitle,
                context.FilteredBookType,
                allowDuplicate,
                ref context.DupeIndex)
            : (string.Empty, false);

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
            MangaDex.TryGetAltTitle(context.CountryOfOrigin, altTitleList, out string nativeAlt))
        {
            titles[MANGADEX_LANG_CODES[context.CountryOfOrigin]] = nativeAlt;
        }

        // Additional languages (only if specified)
        if (additionalLanguages.Length > 0)
        {
            AddAdditionalLanguages(ref titles, additionalLanguages, altTitleList);
        }

        // Staff
        (StringBuilder fullStaff, StringBuilder nativeStaff) = await mangaDex.GetStaffAsync(relationships, context.RomajiTitle);
        if (fullStaff.Length > 0)
        {
            context.NewStaff[TsundokuLanguage.Romaji] = fullStaff.ToString();
        }

        if (nativeStaff.Length > 0)
        {
            context.NewStaff[MANGADEX_LANG_CODES[context.CountryOfOrigin]] = nativeStaff.ToString();
        }

        // Download cover
        Bitmap? coverImage = null;
        if (!isRefresh && (!isDupe || allowDuplicate))
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
            Description: MangaDex.ParseDescription(attributesBlock.GetProperty("description").GetProperty("en").GetString()),
            Format: context.FilteredBookType,
            Status: SeriesStatusEnum.Parse(attributesBlock.GetProperty("status").GetString()),
            Cover: context.CoverPath,
            Link: new Uri(attributesBlock.GetProperty("links").TryGetProperty("al", out JsonElement alId)
                ? $"https://anilist.co/manga/{alId.GetString()}"
                : $"https://mangadex.org/title/{curMangaDexId ?? data.GetProperty("id").GetString()}"),
            Genres: MangaDex.ParseGenreData(context.RomajiTitle, attributesBlock.GetProperty("tags")),
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
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            this.CoverBitMap.Dispose();
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

    public override string ToString()
    {
        if (this != null)
        {
            return JsonSerializer.Serialize(this, typeof(Series), SeriesJsonModel);
        }
        return "Null Series";
    }

    public void UpdateFrom(Series other)
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
    }

    private void Dispose(bool disposing)
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
            this.CoverBitMap = null;
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
    }
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
        foreach (KeyValuePair<TsundokuLanguage, string> title in obj.Titles)
            hash.Add(title);
        foreach (KeyValuePair<TsundokuLanguage, string> staff in obj.Staff)
            hash.Add(staff);
        return hash.ToHashCode();
    }
}
