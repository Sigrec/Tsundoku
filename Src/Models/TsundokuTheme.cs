using System.Globalization;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Models;

public sealed partial class TsundokuTheme : ReactiveObject, ICloneable, IComparable
{
    [Reactive] public partial string ThemeName { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush MenuBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush UsernameColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush MenuTextColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SearchBarBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SearchBarBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SearchBarTextColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush DividerColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush MenuButtonBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush MenuButtonBGHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush MenuButtonBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush MenuButtonBorderHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush MenuButtonTextAndIconColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush MenuButtonTextAndIconHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush CollectionBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush StatusAndBookTypeBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush StatusAndBookTypeTextColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesCardBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesCardTitleColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesCardPublisherColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesCardStaffColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesCardDescColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesCardBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesProgressBarColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesProgressBarBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesProgressBarBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesProgressTextColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesButtonIconColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesButtonIconHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush StatusAndBookTypeBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesCoverBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesCardButtonBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public partial SolidColorBrush SeriesCardDividerColor { get; set; }

    public static readonly TsundokuTheme DEFAULT_THEME = new(
        "Default",
        new SolidColorBrush(Color.Parse("#ff20232d")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ff2c2d42")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ff2c2d42")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ff20232d")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ff20232d")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ff20232d")),
        new SolidColorBrush(Color.Parse("#ffdfd59e"))
    );

    public TsundokuTheme()
    {

    }

    public TsundokuTheme(string themeName)
    {
        ThemeName = themeName;
    }

    [JsonConstructor]
    public TsundokuTheme(string themeName, SolidColorBrush menuBGColor, SolidColorBrush usernameColor, SolidColorBrush menuTextColor, SolidColorBrush searchBarBGColor, SolidColorBrush searchBarBorderColor, SolidColorBrush searchBarTextColor, SolidColorBrush dividerColor, SolidColorBrush menuButtonBGColor, SolidColorBrush menuButtonBGHoverColor, SolidColorBrush menuButtonBorderColor, SolidColorBrush menuButtonBorderHoverColor, SolidColorBrush menuButtonTextAndIconColor, SolidColorBrush menuButtonTextAndIconHoverColor, SolidColorBrush collectionBGColor, SolidColorBrush statusAndBookTypeBGColor, SolidColorBrush statusAndBookTypeTextColor, SolidColorBrush seriesCardBGColor, SolidColorBrush seriesCardTitleColor, SolidColorBrush seriesCardPublisherColor, SolidColorBrush seriesCardStaffColor, SolidColorBrush seriesCardDescColor, SolidColorBrush seriesCardBorderColor, SolidColorBrush seriesProgressBarColor, SolidColorBrush seriesProgressBarBGColor, SolidColorBrush seriesProgressBarBorderColor, SolidColorBrush seriesProgressTextColor, SolidColorBrush seriesButtonIconColor, SolidColorBrush seriesButtonIconHoverColor, SolidColorBrush statusAndBookTypeBorderColor, SolidColorBrush seriesCoverBGColor, SolidColorBrush seriesCardButtonBGColor, SolidColorBrush seriesCardDividerColor) : this(themeName)
    {
        MenuBGColor = menuBGColor;
        UsernameColor = usernameColor;
        MenuTextColor = menuTextColor;
        SearchBarBGColor = searchBarBGColor;
        SearchBarBorderColor = searchBarBorderColor;
        SearchBarTextColor = searchBarTextColor;
        DividerColor = dividerColor;
        MenuButtonBGColor = menuButtonBGColor;
        MenuButtonBGHoverColor = menuButtonBGHoverColor;
        MenuButtonBorderColor = menuButtonBorderColor;
        MenuButtonBorderHoverColor = menuButtonBorderHoverColor;
        MenuButtonTextAndIconColor = menuButtonTextAndIconColor;
        MenuButtonTextAndIconHoverColor = menuButtonTextAndIconHoverColor;
        CollectionBGColor = collectionBGColor;
        StatusAndBookTypeBGColor = statusAndBookTypeBGColor;
        StatusAndBookTypeTextColor = statusAndBookTypeTextColor;
        SeriesCardBGColor = seriesCardBGColor;
        SeriesCardTitleColor = seriesCardTitleColor;
        SeriesCardPublisherColor = seriesCardPublisherColor;
        SeriesCardStaffColor = seriesCardStaffColor;
        SeriesCardDescColor = seriesCardDescColor;
        SeriesCardBorderColor = seriesCardBorderColor;
        SeriesProgressBarColor = seriesProgressBarColor;
        SeriesProgressBarBGColor = seriesProgressBarBGColor;
        SeriesProgressBarBorderColor = seriesProgressBarBorderColor;
        SeriesProgressTextColor = seriesProgressTextColor;
        SeriesButtonIconColor = seriesButtonIconColor;
        SeriesButtonIconHoverColor = seriesButtonIconHoverColor;
        StatusAndBookTypeBorderColor = statusAndBookTypeBorderColor;
        SeriesCoverBGColor = seriesCoverBGColor;
        SeriesCardButtonBGColor = seriesCardButtonBGColor;
        SeriesCardDividerColor = seriesCardDividerColor;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(ThemeName);
        hash.Add(MenuBGColor.Color.ToUInt32());
        hash.Add(UsernameColor.Color.ToUInt32());
        hash.Add(MenuTextColor.Color.ToUInt32());
        hash.Add(SearchBarBGColor.Color.ToUInt32());
        hash.Add(SearchBarBorderColor.Color.ToUInt32());
        hash.Add(SearchBarTextColor.Color.ToUInt32());
        hash.Add(DividerColor.Color.ToUInt32());
        hash.Add(MenuButtonBGColor.Color.ToUInt32());
        hash.Add(MenuButtonBGHoverColor.Color.ToUInt32());
        hash.Add(MenuButtonBorderColor.Color.ToUInt32());
        hash.Add(MenuButtonBorderHoverColor.Color.ToUInt32());
        hash.Add(MenuButtonTextAndIconColor.Color.ToUInt32());
        hash.Add(MenuButtonTextAndIconHoverColor.Color.ToUInt32());
        hash.Add(CollectionBGColor.Color.ToUInt32());
        hash.Add(StatusAndBookTypeBGColor.Color.ToUInt32());
        hash.Add(StatusAndBookTypeTextColor.Color.ToUInt32());
        hash.Add(SeriesCardBGColor.Color.ToUInt32());
        hash.Add(SeriesCardTitleColor.Color.ToUInt32());
        hash.Add(SeriesCardPublisherColor.Color.ToUInt32());
        hash.Add(SeriesCardStaffColor.Color.ToUInt32());
        hash.Add(SeriesCardDescColor.Color.ToUInt32());
        hash.Add(SeriesCardBorderColor.Color.ToUInt32());
        hash.Add(SeriesProgressBarColor.Color.ToUInt32());
        hash.Add(SeriesProgressBarBGColor.Color.ToUInt32());
        hash.Add(SeriesProgressBarBorderColor.Color.ToUInt32());
        hash.Add(SeriesProgressTextColor.Color.ToUInt32());
        hash.Add(SeriesButtonIconColor.Color.ToUInt32());
        hash.Add(SeriesButtonIconHoverColor.Color.ToUInt32());
        hash.Add(StatusAndBookTypeBorderColor.Color.ToUInt32());
        hash.Add(SeriesCoverBGColor.Color.ToUInt32());
        hash.Add(SeriesCardButtonBGColor.Color.ToUInt32());
        hash.Add(SeriesCardDividerColor.Color.ToUInt32());
        return hash.ToHashCode();
    }

    public int CompareTo(object? obj)
    {
        if (obj is not TsundokuTheme other) return 1;
        return this.ThemeName.CompareTo(other.ThemeName);
    }

    public TsundokuTheme Cloning()
    {
        return new TsundokuTheme(
            ThemeName,
            CloneBrush(MenuBGColor),
            CloneBrush(UsernameColor),
            CloneBrush(MenuTextColor),
            CloneBrush(SearchBarBGColor),
            CloneBrush(SearchBarBorderColor),
            CloneBrush(SearchBarTextColor),
            CloneBrush(DividerColor),
            CloneBrush(MenuButtonBGColor),
            CloneBrush(MenuButtonBGHoverColor),
            CloneBrush(MenuButtonBorderColor),
            CloneBrush(MenuButtonBorderHoverColor),
            CloneBrush(MenuButtonTextAndIconColor),
            CloneBrush(MenuButtonTextAndIconHoverColor),
            CloneBrush(CollectionBGColor),
            CloneBrush(StatusAndBookTypeBGColor),
            CloneBrush(StatusAndBookTypeTextColor),
            CloneBrush(SeriesCardBGColor),
            CloneBrush(SeriesCardTitleColor),
            CloneBrush(SeriesCardPublisherColor),
            CloneBrush(SeriesCardStaffColor),
            CloneBrush(SeriesCardDescColor),
            CloneBrush(SeriesCardBorderColor),
            CloneBrush(SeriesProgressBarColor),
            CloneBrush(SeriesProgressBarBGColor),
            CloneBrush(SeriesProgressBarBorderColor),
            CloneBrush(SeriesProgressTextColor),
            CloneBrush(SeriesButtonIconColor),
            CloneBrush(SeriesButtonIconHoverColor),
            CloneBrush(StatusAndBookTypeBorderColor),
            CloneBrush(SeriesCoverBGColor),
            CloneBrush(SeriesCardButtonBGColor),
            CloneBrush(SeriesCardDividerColor)
        );
    }

    private static SolidColorBrush? CloneBrush(SolidColorBrush? brush)
    {
        if (brush is null) return null;
        return new SolidColorBrush(brush.Color);
    }

    object ICloneable.Clone()
    {
        return Cloning();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        TsundokuTheme other = (TsundokuTheme)obj;

        // Compare all relevant properties, string.Equals with OrdinalIgnoreCase or Ordinal is better for colors
        return string.Equals(ThemeName, other.ThemeName, StringComparison.OrdinalIgnoreCase) &&
        BrushEquals(MenuBGColor, other.MenuBGColor) &&
        BrushEquals(UsernameColor, other.UsernameColor) &&
        BrushEquals(MenuTextColor, other.MenuTextColor) &&
        BrushEquals(SearchBarBGColor, other.SearchBarBGColor) &&
        BrushEquals(SearchBarBorderColor, other.SearchBarBorderColor) &&
        BrushEquals(SearchBarTextColor, other.SearchBarTextColor) &&
        BrushEquals(DividerColor, other.DividerColor) &&
        BrushEquals(MenuButtonBGColor, other.MenuButtonBGColor) &&
        BrushEquals(MenuButtonBGHoverColor, other.MenuButtonBGHoverColor) &&
        BrushEquals(MenuButtonBorderColor, other.MenuButtonBorderColor) &&
        BrushEquals(MenuButtonBorderHoverColor, other.MenuButtonBorderHoverColor) &&
        BrushEquals(MenuButtonTextAndIconColor, other.MenuButtonTextAndIconColor) &&
        BrushEquals(MenuButtonTextAndIconHoverColor, other.MenuButtonTextAndIconHoverColor) &&
        BrushEquals(CollectionBGColor, other.CollectionBGColor) &&
        BrushEquals(StatusAndBookTypeBGColor, other.StatusAndBookTypeBGColor) &&
        BrushEquals(StatusAndBookTypeTextColor, other.StatusAndBookTypeTextColor) &&
        BrushEquals(SeriesCardBGColor, other.SeriesCardBGColor) &&
        BrushEquals(SeriesCardTitleColor, other.SeriesCardTitleColor) &&
        BrushEquals(SeriesCardPublisherColor, other.SeriesCardPublisherColor) &&
        BrushEquals(SeriesCardStaffColor, other.SeriesCardStaffColor) &&
        BrushEquals(SeriesCardDescColor, other.SeriesCardDescColor) &&
        BrushEquals(SeriesCardBorderColor, other.SeriesCardBorderColor) &&
        BrushEquals(SeriesProgressBarColor, other.SeriesProgressBarColor) &&
        BrushEquals(SeriesProgressBarBGColor, other.SeriesProgressBarBGColor) &&
        BrushEquals(SeriesProgressBarBorderColor, other.SeriesProgressBarBorderColor) &&
        BrushEquals(SeriesProgressTextColor, other.SeriesProgressTextColor) &&
        BrushEquals(SeriesButtonIconColor, other.SeriesButtonIconColor) &&
        BrushEquals(SeriesButtonIconHoverColor, other.SeriesButtonIconHoverColor) &&
        BrushEquals(StatusAndBookTypeBorderColor, other.StatusAndBookTypeBorderColor) &&
        BrushEquals(SeriesCoverBGColor, other.SeriesCoverBGColor) &&
        BrushEquals(SeriesCardButtonBGColor, other.SeriesCardButtonBGColor) &&
        BrushEquals(SeriesCardDividerColor, other.SeriesCardDividerColor);
    }

    private static bool BrushEquals(SolidColorBrush? a, SolidColorBrush? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Color.Equals(b.Color);
    }

    public sealed class SolidColorBrushJsonConverter : JsonConverter<SolidColorBrush>
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger(); // Logger for the converter

        public override SolidColorBrush? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? hex = reader.GetString();

            if (string.IsNullOrWhiteSpace(hex))
            {
                LOGGER.Warn("SolidColorBrushJsonConverter: Hex string is null or whitespace, returning null brush.");
                return null;
            }

            try
            {
                Color color = Color.Parse(hex);
                return new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, $"SolidColorBrushJsonConverter: Error parsing hex '{hex}': {ex.Message}. Returning null brush.");
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, SolidColorBrush value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                LOGGER.Warn("SolidColorBrushJsonConverter: Writing null value for SolidColorBrush.");
                return;
            }

            writer.WriteStringValue(value.Color.ToString());
        }
    }
}

public sealed class TsundokuThemeComparer : IComparer<TsundokuTheme>
{
    private readonly TsundokuLanguage _curLang;
    private readonly StringComparer _themeNameComparer;

    public TsundokuThemeComparer(TsundokuLanguage curLang)
    {
        _curLang = curLang;
        // Use the culture info from your constants
        _themeNameComparer = StringComparer.Create(new CultureInfo(CULTURE_LANG_CODES[_curLang]), false);
    }

    public int Compare(TsundokuTheme? x, TsundokuTheme? y)
    {
        // Handle nulls as per IComparer contract
        if (x is null && y is null) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        // Assuming you want to compare by ThemeName
        return _themeNameComparer.Compare(x.ThemeName, y.ThemeName);
    }
}

[JsonSerializable(typeof(TsundokuTheme))]
[JsonSourceGenerationOptions(
    UseStringEnumConverter = true,
    WriteIndented = true,
    ReadCommentHandling = JsonCommentHandling.Disallow,
    AllowTrailingCommas = true,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    IncludeFields = false,
    NumberHandling = JsonNumberHandling.AllowReadingFromString
)]
internal partial class TsundokuThemeModelContext : JsonSerializerContext { }