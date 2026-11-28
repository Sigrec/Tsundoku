using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
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
    Bitmap? CoverBitMap,
    string Publisher = "Unknown",
    uint DuplicateIndex = 0) : ReactiveObject, IDisposable, IEquatable<Series?>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private static readonly SeriesModelContext SeriesJsonModel = new(new JsonSerializerOptions()
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
    private uint _maxVolumeCount = MaxVolumeCount;
    private uint _curVolumeCount = CurVolumeCount;
    private List<decimal> _ownedVolumes = VolumeMath.Sequence(CurVolumeCount);
    private List<decimal> _specialVolumes = [];

    public uint MaxVolumeCount
    {
        get => _maxVolumeCount;
        set
        {
            if (_maxVolumeCount == value) return;
            this.RaiseAndSetIfChanged(ref _maxVolumeCount, value);
            RaiseVolumeDerivedChanged();
        }
    }

    /// <summary>
    /// Number of main volumes owned (whole numbers 1 and up in <see cref="OwnedVolumes"/>). Setting it
    /// adds the next volumes after the highest owned, or removes the highest owned, to match.
    /// </summary>
    public uint CurVolumeCount
    {
        get => _curVolumeCount;
        set => SetMainOwnedCount(value);
    }

    /// <summary>Every owned volume number, sorted, including specials like 18.5.</summary>
    public List<decimal> OwnedVolumes
    {
        get => _ownedVolumes;
        set => ApplyOwnedVolumes(VolumeMath.Normalize(value));
    }

    /// <summary>Special volume numbers (0, 18.5, …) this series has, owned or not.</summary>
    public List<decimal> SpecialVolumes
    {
        get => _specialVolumes;
        set
        {
            _specialVolumes = VolumeMath.NormalizeSpecials(value);
            this.RaisePropertyChanged();
            RaiseVolumeDerivedChanged();
        }
    }

    private List<ExtraVolume> _extraVolumes = [];

    /// <summary>Named books with no volume number (fanbooks, box set bonuses), shown after the numbered volumes.</summary>
    public List<ExtraVolume> ExtraVolumes
    {
        get => _extraVolumes;
        set
        {
            _extraVolumes = value ?? [];
            this.RaisePropertyChanged();
            RaiseVolumeDerivedChanged();
        }
    }

    /// <summary>Card progress text, e.g. <c>20 / 20</c>, or <c>20 / 20 +2</c> with owned specials or extras.</summary>
    [JsonIgnore]
    public string ProgressText
    {
        get
        {
            int ownedBeyondMain = OwnedSpecialCount + OwnedExtraCount;
            return ownedBeyondMain > 0
                ? $"{_curVolumeCount} / {_maxVolumeCount} +{ownedBeyondMain}"
                : $"{_curVolumeCount} / {_maxVolumeCount}";
        }
    }

    /// <summary>Card count, e.g. <c>10 / 12</c>.</summary>
    [JsonIgnore] public string MainCountText => $"{_curVolumeCount} / {_maxVolumeCount}";

    /// <summary>Owned specials and extras on the card, e.g. <c>+2</c>; empty when there are none.</summary>
    [JsonIgnore]
    public string BeyondMainText
    {
        get
        {
            int ownedBeyondMain = OwnedSpecialCount + OwnedExtraCount;
            return ownedBeyondMain > 0 ? $"+{ownedBeyondMain}" : string.Empty;
        }
    }

    /// <summary>Text after the card count, e.g. <c>· 83%</c>; empty when the max is unknown.</summary>
    [JsonIgnore]
    public string CountSuffixText => _maxVolumeCount > 0
        ? $"· {(int)Math.Round(_curVolumeCount * 100.0 / _maxVolumeCount)}%"
        : string.Empty;

    [JsonIgnore] public int OwnedSpecialCount => VolumeMath.CountOwnedSpecials(_ownedVolumes, _specialVolumes);

    [JsonIgnore] public int OwnedExtraCount => _extraVolumes.Count(static e => e.Owned);

    /// <summary>Main volumes missing from 1..max plus listed specials that aren't owned.</summary>
    [JsonIgnore]
    public List<decimal> MissingVolumes =>
        [.. VolumeMath.MissingMain(_ownedVolumes, _maxVolumeCount), .. VolumeMath.MissingSpecials(_ownedVolumes, _specialVolumes)];

    /// <summary>Names of extras that aren't owned.</summary>
    [JsonIgnore] public List<string> MissingExtraNames => [.. _extraVolumes.Where(static e => !e.Owned).Select(static e => e.Name)];

    [JsonIgnore]
    public int MissingVolumeCount =>
        VolumeMath.CountMissingMain(_ownedVolumes, _maxVolumeCount)
        + (_specialVolumes.Count - VolumeMath.CountOwnedSpecials(_ownedVolumes, _specialVolumes))
        + _extraVolumes.Count(static e => !e.Owned);

    /// <summary>Whether a volume or special below the highest owned volume is missing.</summary>
    [JsonIgnore] public bool HasGaps => VolumeMath.HasGaps(_ownedVolumes, _specialVolumes);

    /// <summary>Card gap line, numbered part: e.g. <c>Missing 3–4, 7–9</c>; empty without gaps.</summary>
    [JsonIgnore]
    public string GapMainText
    {
        get
        {
            if (!HasGaps) return string.Empty;
            string[] rangeParts = SplitRanges(VolumeMath.FormatRanges(VolumeMath.MissingMain(_ownedVolumes, _maxVolumeCount), "–"));
            if (rangeParts.Length == 0) return "Missing ";

            const int MaxRangesShown = 4;
            string shown = string.Join(", ", rangeParts.Take(MaxRangesShown));
            string more = rangeParts.Length > MaxRangesShown ? $", +{rangeParts.Length - MaxRangesShown} more" : string.Empty;
            return $"Missing {shown}{more}";
        }
    }

    /// <summary>Card gap line, specials part: e.g. <c> · 3.5, Fanbook</c>; empty without gaps or missing specials.</summary>
    [JsonIgnore]
    public string GapSpecialText
    {
        get
        {
            if (!HasGaps) return string.Empty;
            List<string> missing = [.. VolumeMath.MissingSpecials(_ownedVolumes, _specialVolumes).Select(VolumeMath.Format), .. MissingExtraNames];
            if (missing.Count == 0) return string.Empty;
            bool hasMainPart = VolumeMath.CountMissingMain(_ownedVolumes, _maxVolumeCount) > 0;
            return (hasMainPart ? " · " : string.Empty) + string.Join(", ", missing);
        }
    }

    private static string[] SplitRanges(string ranges) => ranges.Length == 0 ? [] : ranges.Split(", ");

    /// <summary>Card line listing missing volumes, shown only when the series has gaps.</summary>
    [JsonIgnore]
    public string GapText
    {
        get
        {
            if (!HasGaps) return string.Empty;

            string mainRanges = VolumeMath.FormatRanges(VolumeMath.MissingMain(_ownedVolumes, _maxVolumeCount), "–");
            string[] rangeParts = mainRanges.Length == 0 ? [] : mainRanges.Split(", ");
            List<decimal> missingSpecials = VolumeMath.MissingSpecials(_ownedVolumes, _specialVolumes);

            StringBuilder text = new("Missing ");
            const int MaxRangesShown = 4;
            text.AppendJoin(", ", rangeParts.Take(MaxRangesShown));
            if (rangeParts.Length > MaxRangesShown)
            {
                text.Append($", +{rangeParts.Length - MaxRangesShown} more");
            }
            if (missingSpecials.Count > 0)
            {
                text.Append(rangeParts.Length > 0 ? " · special " : "special ");
                text.AppendJoin(", ", missingSpecials.Select(VolumeMath.Format));
            }
            List<string> missingExtras = MissingExtraNames;
            if (missingExtras.Count > 0)
            {
                text.Append(" · ").AppendJoin(", ", missingExtras);
            }
            return text.ToString();
        }
    }
    [Reactive] public partial uint VolumesRead { get; set; } = VolumesRead;
    [Reactive] public partial decimal Value { get; set; } = Value;
    [Reactive] public partial decimal Rating { get; set; } = Rating;
    [Reactive] public partial SeriesDemographic Demographic { get; set; } = Demographic;
    [Reactive] public partial bool IsFavorite { get; set; } = false;

    /// <summary>Whether the card is part of the main window's bulk-edit selection. Never persisted.</summary>
    [JsonIgnore][Reactive] public partial bool IsSelected { get; set; }

    /// <summary>
    /// All Chinese or Taiwanese series use "Chinese (Simplified)" and go to "Chinese"
    /// </summary>
    /// <param name="bitmapHelper">Helper used to download and save the cover image</param>
    /// <param name="mangaDex">MangaDex client</param>
    /// <param name="aniList">AniList client</param>
    /// <param name="input">Title or ID of the series the user wants to add to their collection</param>
    /// <param name="bookType">Booktype of the series the user wants to add, either Manga or Light Novel</param>
    /// <param name="maxVolCount">Current max volume count of the series</param>
    /// <param name="minVolCount">Current volume count the user has for the series</param>
    /// <param name="additionalLanguages">List of additional languages to query for</param>
    /// <param name="publisher">Publisher of the series</param>
    /// <param name="demographic">Demographic of the series</param>
    /// <param name="volumesRead">Number of volumes the user has read</param>
    /// <param name="rating">The user's rating, or -1 for none</param>
    /// <param name="value">Total value of the user's volumes</param>
    /// <param name="customImageUrl">Cover image URL to use instead of the one from the API</param>
    /// <param name="allowDuplicate">Whether the series may be added when it is already in the collection</param>
    /// <param name="isRefresh">Whether this call refreshes an existing series</param>
    /// <param name="isCoverImageRefresh">Whether the cover image should be downloaded again on refresh</param>
    /// <param name="coverPath">Existing cover file name, used when refreshing</param>
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
                case string s when int.TryParse(s, out int seriesId):
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
        JsonDocument? mediaDataDoc,
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
        string? coverPath,
        BitmapHelper bitmapHelper,
        BuildContext context)
    {
        if (mediaDataDoc is null)
        {
            LOGGER.Warn("AniList returned no data for {Input}", input);
            return null;
        }

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
            context.CoverPath = coverPath ?? string.Empty;
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
            ? AniList.ParseSeriesDescription(descriptionProp.GetString() ?? string.Empty)
            : string.Empty;

        SeriesStatus status = SeriesStatusModel.Parse(GetStringOrEmpty(mediaData, "status"));
        Uri link = new(GetStringOrEmpty(mediaData, "siteUrl"));

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
        JsonDocument? mediaDataDoc,
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
        string? coverPath,
        BitmapHelper bitmapHelper,
        BuildContext context)
    {
        if (mediaDataDoc is null)
        {
            LOGGER.Warn("MangaDex returned no data for {Input}", input);
            return null;
        }

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
                x.TryGetProperty("type", out JsonElement typeProp) &&
                typeProp.ValueEquals("cover_art"));

        string coverId = coverArtRel.ValueKind == JsonValueKind.Object
            ? GetStringOrEmpty(coverArtRel, "id")
            : string.Empty;

        string seriesId = string.IsNullOrEmpty(curMangaDexId) ? GetStringOrEmpty(data, "id") : curMangaDexId;

        // Resolve cover URL (prefer custom, fallback to generated)
        string coverUrl = (string.IsNullOrWhiteSpace(customImageUrl)
            ? await mangaDex.GetCoverLinkAsync(coverId, seriesId, context.RomajiTitle)
            : customImageUrl) ?? string.Empty;

        // Conditionally generate cover file path
        (context.CoverPath, bool isDupe) = !isRefresh
            ? AppFileHelper.CreateUniqueCoverFileName(
                coverUrl,
                context.RomajiTitle,
                context.FilteredBookType,
                allowDuplicate,
                ref context.DupeIndex)
            : (coverPath ?? string.Empty, false);

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
            MangaDex.TryGetAltTitle(context.CountryOfOrigin, altTitleList, out string? nativeAlt) &&
            nativeAlt is not null)
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

    #region Owned volumes

    /// <summary>Adds the next volume after the highest owned, or fills the lowest gap once at the max.</summary>
    public void IncrementCurVolumeCount()
    {
        if (this.CurVolumeCount < this.MaxVolumeCount && VolumeMath.NextToAdd(_ownedVolumes, _maxVolumeCount) is decimal next)
        {
            ApplyOwnedVolumes(VolumeMath.Normalize([.. _ownedVolumes, next]));
        }
    }

    /// <summary>Removes the highest owned main volume.</summary>
    public void DecrementCurVolumeCount()
    {
        uint highest = VolumeMath.HighestMain(_ownedVolumes);
        if (highest > 0)
        {
            List<decimal> owned = [.. _ownedVolumes];
            owned.Remove(highest);
            ApplyOwnedVolumes(owned);
        }
    }

    /// <summary>Marks one volume owned or not owned. A special number is added to <see cref="SpecialVolumes"/> if new.</summary>
    public void ToggleVolume(decimal volume)
    {
        volume = decimal.Round(volume, 2);
        List<decimal> owned = [.. _ownedVolumes];
        if (!owned.Remove(volume))
        {
            if (!VolumeMath.IsMain(volume) && !_specialVolumes.Contains(volume))
            {
                SpecialVolumes = [.. _specialVolumes, volume];
            }
            owned.Add(volume);
        }
        ApplyOwnedVolumes(VolumeMath.Normalize(owned));
    }

    /// <summary>Sets every main volume in <paramref name="volumes"/> to <paramref name="owned"/>.</summary>
    public void SetVolumesOwned(IEnumerable<decimal> volumes, bool owned)
    {
        HashSet<decimal> set = [.. _ownedVolumes];
        foreach (decimal volume in volumes)
        {
            if (owned) set.Add(volume);
            else set.Remove(volume);
        }
        ApplyOwnedVolumes(VolumeMath.Normalize(set));
    }

    /// <summary>
    /// Replaces the owned volumes. Specials in <paramref name="volumes"/> that the series doesn't list
    /// yet are added to <see cref="SpecialVolumes"/>.
    /// </summary>
    public void SetOwnedVolumes(IEnumerable<decimal> volumes)
    {
        List<decimal> normalized = VolumeMath.Normalize(volumes);
        List<decimal> newSpecials = [];
        foreach (decimal volume in normalized)
        {
            if (!VolumeMath.IsMain(volume) && !_specialVolumes.Contains(volume))
            {
                newSpecials.Add(volume);
            }
        }
        if (newSpecials.Count > 0)
        {
            SpecialVolumes = [.. _specialVolumes, .. newSpecials];
        }
        ApplyOwnedVolumes(normalized);
    }

    /// <summary>Lists a special volume for this series and marks it owned.</summary>
    /// <returns><c>false</c> when the number is a main volume or already listed.</returns>
    public bool AddSpecialVolume(decimal volume)
    {
        volume = decimal.Round(volume, 2);
        if (volume < 0m || VolumeMath.IsMain(volume) || _specialVolumes.Contains(volume))
        {
            return false;
        }

        SpecialVolumes = [.. _specialVolumes, volume];
        ApplyOwnedVolumes(VolumeMath.Normalize([.. _ownedVolumes, volume]));
        return true;
    }

    /// <summary>Removes a special volume from this series, owned or not.</summary>
    public void RemoveSpecialVolume(decimal volume)
    {
        volume = decimal.Round(volume, 2);
        if (!_specialVolumes.Contains(volume)) return;

        SpecialVolumes = [.. _specialVolumes.Where(s => s != volume)];
        List<decimal> owned = [.. _ownedVolumes];
        owned.Remove(volume);
        ApplyOwnedVolumes(owned);
    }

    /// <summary>Longest name accepted for an extra volume.</summary>
    public const int MaxExtraNameLength = 40;

    /// <summary>Adds a named extra (fanbook, box set bonus, …) and marks it owned.</summary>
    /// <returns><c>false</c> when the name is blank, too long, or already listed (ignoring case).</returns>
    public bool AddExtraVolume(string name)
    {
        string trimmed = name.Trim();
        if (trimmed.Length == 0 || trimmed.Length > MaxExtraNameLength || FindExtraIndex(trimmed) >= 0)
        {
            return false;
        }

        ExtraVolumes = [.. _extraVolumes, new ExtraVolume(trimmed, Owned: true)];
        return true;
    }

    /// <summary>Flips whether the named extra is owned.</summary>
    public void ToggleExtraVolume(string name)
    {
        int index = FindExtraIndex(name);
        if (index < 0) return;

        List<ExtraVolume> extras = [.. _extraVolumes];
        extras[index] = extras[index] with { Owned = !extras[index].Owned };
        ExtraVolumes = extras;
    }

    /// <summary>Marks every extra owned or not owned.</summary>
    public void SetAllExtrasOwned(bool owned)
    {
        if (_extraVolumes.All(e => e.Owned == owned)) return;
        ExtraVolumes = [.. _extraVolumes.Select(e => e with { Owned = owned })];
    }

    /// <summary>Removes the named extra from this series.</summary>
    public void RemoveExtraVolume(string name)
    {
        int index = FindExtraIndex(name);
        if (index < 0) return;

        List<ExtraVolume> extras = [.. _extraVolumes];
        extras.RemoveAt(index);
        ExtraVolumes = extras;
    }

    private int FindExtraIndex(string name) =>
        _extraVolumes.FindIndex(e => string.Equals(e.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Owns every main volume 1..max. Specials are left as they are.</summary>
    public void MarkMainRunComplete()
    {
        if (_maxVolumeCount == 0) return;
        ApplyOwnedVolumes(VolumeMath.Normalize([.. _ownedVolumes, .. VolumeMath.Sequence(_maxVolumeCount)]));
    }

    /// <summary>Adds or removes main volumes until <paramref name="count"/> are owned. Specials are kept.</summary>
    private void SetMainOwnedCount(uint count)
    {
        List<decimal> owned = [.. _ownedVolumes];
        uint current = VolumeMath.CountMain(owned);
        while (current < count)
        {
            decimal next = VolumeMath.NextToAdd(owned, _maxVolumeCount) ?? (VolumeMath.HighestMain(owned) + 1);
            owned = VolumeMath.Normalize([.. owned, next]);
            current++;
        }
        while (current > count)
        {
            owned.Remove(VolumeMath.HighestMain(owned));
            current--;
        }
        ApplyOwnedVolumes(owned);
    }

    private void ApplyOwnedVolumes(List<decimal> owned)
    {
        _ownedVolumes = owned;
        this.RaiseAndSetIfChanged(ref _curVolumeCount, VolumeMath.CountMain(owned), nameof(CurVolumeCount));
        this.RaisePropertyChanged(nameof(OwnedVolumes));
        RaiseVolumeDerivedChanged();
    }

    private void RaiseVolumeDerivedChanged()
    {
        this.RaisePropertyChanged(nameof(ProgressText));
        this.RaisePropertyChanged(nameof(MainCountText));
        this.RaisePropertyChanged(nameof(BeyondMainText));
        this.RaisePropertyChanged(nameof(CountSuffixText));
        this.RaisePropertyChanged(nameof(OwnedSpecialCount));
        this.RaisePropertyChanged(nameof(OwnedExtraCount));
        this.RaisePropertyChanged(nameof(MissingExtraNames));
        this.RaisePropertyChanged(nameof(MissingVolumes));
        this.RaisePropertyChanged(nameof(MissingVolumeCount));
        this.RaisePropertyChanged(nameof(HasGaps));
        this.RaisePropertyChanged(nameof(GapText));
        this.RaisePropertyChanged(nameof(GapMainText));
        this.RaisePropertyChanged(nameof(GapSpecialText));
    }

    #endregion

    public void UpdateVolumeCounts(uint newCur, uint newMax, Action? action = null)
    {
        if (newMax >= newCur)
        {
            uint oldCur = this.CurVolumeCount;
            uint oldMax = this.MaxVolumeCount;
            if (newMax > 0 && VolumeMath.HighestMain(_ownedVolumes) > newMax)
            {
                ApplyOwnedVolumes([.. _ownedVolumes.Where(v => !VolumeMath.IsMain(v) || v <= newMax)]);
            }
            this.MaxVolumeCount = newMax;
            this.CurVolumeCount = newCur;

            LOGGER.Info(
                "Changed Series Volume Counts For {RomajiTitle} From {OldCur}/{OldMax} -> {NewCur}/{NewMax}",
                this.Titles[TsundokuLanguage.Romaji], oldCur, oldMax, newCur, newMax
            );

            action?.Invoke();
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

            action?.Invoke();
        }
        else
        {
            LOGGER.Warn("{NewCur} cannot be greater than {MaxVolumeCount}", newCur, this.MaxVolumeCount);
        }
    }

    /// <summary>Changes the max volume count unless an owned main volume is above it.</summary>
    /// <returns><c>false</c> when <paramref name="newMax"/> is below the highest owned volume.</returns>
    public bool UpdateMaxVolumeCount(uint newMax, Action? action = null)
    {
        if (newMax >= VolumeMath.HighestMain(_ownedVolumes))
        {
            uint oldMax = this.MaxVolumeCount;
            this.MaxVolumeCount = newMax;

            LOGGER.Info(
                "Updated Series Max Volume Count For {RomajiTitle} From {OldMax} to {NewMax}",
                this.Titles[TsundokuLanguage.Romaji], oldMax, newMax
            );

            action?.Invoke();
            return true;
        }

        LOGGER.Warn("{NewMax} cannot be less than the highest owned volume {HighestOwned}", newMax, VolumeMath.HighestMain(_ownedVolumes));
        return false;
    }

    /// <summary>
    /// Determines whether the country of origin is an Asian region other than Japan (e.g., Korea, Taiwan, China).
    /// </summary>
    /// <param name="origin">The country code (e.g., "JP", "KR", "TW", "CW").</param>
    /// <returns>
    /// <c>true</c> if the origin is Korean, Taiwanese, or Chinese (Simplified/Traditional); otherwise, <c>false</c>.
    /// </returns>
    private static bool IsAsianNonJapanese(string origin) => origin is "KR" or "CW" or "TW";

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

    public void DeleteCover() => AppFileHelper.DeleteCoverFile(this.Cover);

    public void UpdateCover(Bitmap newCover)
    {
        TsundokuSchedulers.MainThread.Schedule(() =>
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

    public bool HasGenre(SeriesGenre genre) => this.Genres is not null && this.Genres.Contains(genre);

    public override string ToString() => JsonSerializer.Serialize(this, typeof(Series), SeriesJsonModel);

    /// <summary>
    /// Extracts the series identifier (AniList numeric ID or MangaDex UUID) from the
    /// stored <see cref="Link"/> URI.  AniList URLs may include a trailing slug
    /// (e.g. <c>/manga/86635/Jujutsu-Kaisen</c>), so the last segment is not
    /// always the ID.  This method finds the segment immediately after
    /// <c>manga/</c> or <c>title/</c> and strips any trailing slash.
    /// </summary>
    public string GetLinkId()
    {
        string[] segments = Link.Segments;
        for (int i = 0; i < segments.Length - 1; i++)
        {
            string seg = segments[i].TrimEnd('/');
            if (seg.Equals("manga", StringComparison.OrdinalIgnoreCase) ||
                seg.Equals("title", StringComparison.OrdinalIgnoreCase))
            {
                return segments[i + 1].TrimEnd('/');
            }
        }

        return segments[^1].TrimEnd('/');
    }

    public bool IsCoverImageEmpty() => this.CoverBitMap is null || this.CoverBitMap.PixelSize.Width == 0 || this.CoverBitMap.PixelSize.Height == 0;

    /// <summary>
    /// Copies refreshed data from <paramref name="other"/> into this series. When
    /// <paramref name="isCoverEmpty"/> is true, this series takes ownership of
    /// <paramref name="other"/>'s cover bitmap and clears it there, so disposing
    /// <paramref name="other"/> afterwards leaves the moved cover intact.
    /// </summary>
    public void UpdateFrom(Series other, bool isCoverEmpty)
    {
        // 1. Titles (Dictionary<TsundokuLanguage, string>) – deep‐compare by count + per‐key/value
        if (!ExtensionMethods.DictionariesEqual(Titles, other.Titles))
        {
            Titles = [with(other.Titles)];
        }

        // 2. Staff (Dictionary<TsundokuLanguage, string>) – deep‐compare
        if (!ExtensionMethods.DictionariesEqual(Staff, other.Staff))
        {
            Staff = [with(other.Staff)];
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

        // 9. MaxVolumeCount — never below a volume the user owns
        if (MaxVolumeCount != other.MaxVolumeCount && other.MaxVolumeCount >= VolumeMath.HighestMain(_ownedVolumes))
        {
            MaxVolumeCount = other.MaxVolumeCount;
        }

        // 10. Owned and special volumes are the user's own data and are kept as they are. The refreshed
        // series was built from the count alone, so copying its list would erase gaps and specials.

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
            Bitmap? newCover = other.CoverBitMap;
            other.CoverBitMap = null;
            if (newCover is null)
            {
                // Fallback: load from file if the bitmap wasn't in memory
                string fullPath = AppFileHelper.GetFullCoverPath(other.Cover);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        using FileStream coverStream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, options: FileOptions.SequentialScan);
                        newCover = Bitmap.DecodeToWidth(coverStream, LEFT_SIDE_CARD_WIDTH * BITMAP_SCALE, BitmapInterpolationMode.HighQuality);
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

    public bool Equals(Series? other) => other is not null && this.Id.Equals(other.Id);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Series? left, Series? right) => EqualityComparer<Series>.Default.Equals(left, right);

    public static bool operator !=(Series? left, Series? right) => !(left == right);

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
        public string? JapaneseTitle = string.Empty;
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