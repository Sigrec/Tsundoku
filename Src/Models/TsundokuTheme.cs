using System.Globalization;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Tsundoku.Models.Enums.TsundokuLanguageEnums;

namespace Tsundoku.Models;

public sealed class TsundokuTheme : ReactiveObject, ICloneable, IComparable
{
    [Reactive] public string ThemeName { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush MenuBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush UsernameColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush UserIconBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush MenuTextColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SearchBarBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SearchBarBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SearchBarTextColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush DividerColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush MenuButtonBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush MenuButtonBGHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush MenuButtonBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush MenuButtonBorderHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush MenuButtonTextAndIconColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush MenuButtonTextAndIconHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush CollectionBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush StatusAndBookTypeBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush StatusAndBookTypeBGHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush StatusAndBookTypeTextColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush StatusAndBookTypeTextHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesCardBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesCardTitleColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesCardPublisherColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesCardStaffColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesCardDescColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesProgressBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesProgressBarColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesProgressBarBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesProgressBarBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesProgressTextColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesProgressButtonsHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesButtonIconColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesButtonIconHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesEditPaneBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesNotesBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesNotesBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesNotesTextColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesEditPaneButtonsBGColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesEditPaneButtonsBGHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesEditPaneButtonsBorderColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesEditPaneButtonsBorderHoverColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesEditPaneButtonsIconColor { get; set; }

    [JsonConverter(typeof(SolidColorBrushJsonConverter))]
    [Reactive] public SolidColorBrush SeriesEditPaneButtonsIconHoverColor { get; set; }

    public static readonly TsundokuTheme DEFAULT_THEME = new(
        "Default",
        new SolidColorBrush(Color.Parse("#ff20232d")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ff626460")),
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
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ff20232d")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ff20232d")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ff20232d")),
        new SolidColorBrush(Color.Parse("#ffececec")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ff20232d")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffb4bddb")),
        new SolidColorBrush(Color.Parse("#ff2c2d42")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ffdfd59e")),
        new SolidColorBrush(Color.Parse("#ff626460")),
        new SolidColorBrush(Color.Parse("#ffb4bddb"))
    );

    public TsundokuTheme()
    {

    }

    public TsundokuTheme(string themeName)
    {
        ThemeName = themeName;
    }

    [JsonConstructor]
    public TsundokuTheme(string themeName, SolidColorBrush menuBGColor, SolidColorBrush usernameColor, SolidColorBrush userIconBorderColor, SolidColorBrush menuTextColor, SolidColorBrush searchBarBGColor, SolidColorBrush searchBarBorderColor, SolidColorBrush searchBarTextColor, SolidColorBrush dividerColor, SolidColorBrush menuButtonBGColor, SolidColorBrush menuButtonBGHoverColor, SolidColorBrush menuButtonBorderColor, SolidColorBrush menuButtonBorderHoverColor, SolidColorBrush menuButtonTextAndIconColor, SolidColorBrush menuButtonTextAndIconHoverColor, SolidColorBrush collectionBGColor, SolidColorBrush statusAndBookTypeBGColor, SolidColorBrush statusAndBookTypeBGHoverColor, SolidColorBrush statusAndBookTypeTextColor, SolidColorBrush statusAndBookTypeTextHoverColor, SolidColorBrush seriesCardBGColor, SolidColorBrush seriesCardTitleColor, SolidColorBrush seriesCardPublisherColor, SolidColorBrush seriesCardStaffColor, SolidColorBrush seriesCardDescColor, SolidColorBrush seriesProgressBGColor, SolidColorBrush seriesProgressBarColor, SolidColorBrush seriesProgressBarBGColor, SolidColorBrush seriesProgressBarBorderColor, SolidColorBrush seriesProgressTextColor, SolidColorBrush seriesProgressButtonsHoverColor, SolidColorBrush seriesButtonIconColor, SolidColorBrush seriesButtonIconHoverColor, SolidColorBrush seriesEditPaneBGColor, SolidColorBrush seriesNotesBGColor, SolidColorBrush seriesNotesBorderColor, SolidColorBrush seriesNotesTextColor, SolidColorBrush seriesEditPaneButtonsBGColor, SolidColorBrush seriesEditPaneButtonsBGHoverColor, SolidColorBrush seriesEditPaneButtonsBorderColor, SolidColorBrush seriesEditPaneButtonsBorderHoverColor, SolidColorBrush seriesEditPaneButtonsIconColor, SolidColorBrush seriesEditPaneButtonsIconHoverColor) : this(themeName)
    {
        MenuBGColor = menuBGColor;
        UsernameColor = usernameColor;
        UserIconBorderColor = userIconBorderColor;
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
        StatusAndBookTypeBGHoverColor = statusAndBookTypeBGHoverColor;
        StatusAndBookTypeTextColor = statusAndBookTypeTextColor;
        StatusAndBookTypeTextHoverColor = statusAndBookTypeTextHoverColor;
        SeriesCardBGColor = seriesCardBGColor;
        SeriesCardTitleColor = seriesCardTitleColor;
        SeriesCardPublisherColor = seriesCardPublisherColor;
        SeriesCardStaffColor = seriesCardStaffColor;
        SeriesCardDescColor = seriesCardDescColor;
        SeriesProgressBGColor = seriesProgressBGColor;
        SeriesProgressBarColor = seriesProgressBarColor;
        SeriesProgressBarBGColor = seriesProgressBarBGColor;
        SeriesProgressBarBorderColor = seriesProgressBarBorderColor;
        SeriesProgressTextColor = seriesProgressTextColor;
        SeriesProgressButtonsHoverColor = seriesProgressButtonsHoverColor;
        SeriesButtonIconColor = seriesButtonIconColor;
        SeriesButtonIconHoverColor = seriesButtonIconHoverColor;
        SeriesEditPaneBGColor = seriesEditPaneBGColor;
        SeriesNotesBGColor = seriesNotesBGColor;
        SeriesNotesBorderColor = seriesNotesBorderColor;
        SeriesNotesTextColor = seriesNotesTextColor;
        SeriesEditPaneButtonsBGColor = seriesEditPaneButtonsBGColor;
        SeriesEditPaneButtonsBGHoverColor = seriesEditPaneButtonsBGHoverColor;
        SeriesEditPaneButtonsBorderColor = seriesEditPaneButtonsBorderColor;
        SeriesEditPaneButtonsBorderHoverColor = seriesEditPaneButtonsBorderHoverColor;
        SeriesEditPaneButtonsIconColor = seriesEditPaneButtonsIconColor;
        SeriesEditPaneButtonsIconHoverColor = seriesEditPaneButtonsIconHoverColor;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(ThemeName);
        hash.Add(MenuBGColor.Color.ToUInt32());
        hash.Add(UsernameColor.Color.ToUInt32());
        hash.Add(UserIconBorderColor.Color.ToUInt32());
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
        hash.Add(StatusAndBookTypeBGHoverColor.Color.ToUInt32());
        hash.Add(StatusAndBookTypeTextColor.Color.ToUInt32());
        hash.Add(StatusAndBookTypeTextHoverColor.Color.ToUInt32());
        hash.Add(SeriesCardBGColor.Color.ToUInt32());
        hash.Add(SeriesCardTitleColor.Color.ToUInt32());
        hash.Add(SeriesCardPublisherColor.Color.ToUInt32());
        hash.Add(SeriesCardStaffColor.Color.ToUInt32());
        hash.Add(SeriesCardDescColor.Color.ToUInt32());
        hash.Add(SeriesProgressBGColor.Color.ToUInt32());
        hash.Add(SeriesProgressBarColor.Color.ToUInt32());
        hash.Add(SeriesProgressBarBGColor.Color.ToUInt32());
        hash.Add(SeriesProgressBarBorderColor.Color.ToUInt32());
        hash.Add(SeriesProgressTextColor.Color.ToUInt32());
        hash.Add(SeriesProgressButtonsHoverColor.Color.ToUInt32());
        hash.Add(SeriesButtonIconColor.Color.ToUInt32());
        hash.Add(SeriesButtonIconHoverColor.Color.ToUInt32());
        hash.Add(SeriesEditPaneBGColor.Color.ToUInt32());
        hash.Add(SeriesNotesBGColor.Color.ToUInt32());
        hash.Add(SeriesNotesBorderColor.Color.ToUInt32());
        hash.Add(SeriesNotesTextColor.Color.ToUInt32());
        hash.Add(SeriesEditPaneButtonsBGColor.Color.ToUInt32());
        hash.Add(SeriesEditPaneButtonsBGHoverColor.Color.ToUInt32());
        hash.Add(SeriesEditPaneButtonsBorderColor.Color.ToUInt32());
        hash.Add(SeriesEditPaneButtonsBorderHoverColor.Color.ToUInt32());
        hash.Add(SeriesEditPaneButtonsIconColor.Color.ToUInt32());
        hash.Add(SeriesEditPaneButtonsIconHoverColor.Color.ToUInt32());
        return hash.ToHashCode();
    }

    public int CompareTo(object? obj)
    {
        return this.ThemeName.CompareTo((obj as TsundokuTheme).ThemeName);
    }

    public TsundokuTheme Cloning()
    {
        return new TsundokuTheme(
            ThemeName,
            CloneBrush(MenuBGColor),
            CloneBrush(UsernameColor),
            CloneBrush(UserIconBorderColor),
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
            CloneBrush(StatusAndBookTypeBGHoverColor),
            CloneBrush(StatusAndBookTypeTextColor),
            CloneBrush(StatusAndBookTypeTextHoverColor),
            CloneBrush(SeriesCardBGColor),
            CloneBrush(SeriesCardTitleColor),
            CloneBrush(SeriesCardPublisherColor),
            CloneBrush(SeriesCardStaffColor),
            CloneBrush(SeriesCardDescColor),
            CloneBrush(SeriesProgressBGColor),
            CloneBrush(SeriesProgressBarColor),
            CloneBrush(SeriesProgressBarBGColor),
            CloneBrush(SeriesProgressBarBorderColor),
            CloneBrush(SeriesProgressTextColor),
            CloneBrush(SeriesProgressButtonsHoverColor),
            CloneBrush(SeriesButtonIconColor),
            CloneBrush(SeriesButtonIconHoverColor),
            CloneBrush(SeriesEditPaneBGColor),
            CloneBrush(SeriesNotesBGColor),
            CloneBrush(SeriesNotesBorderColor),
            CloneBrush(SeriesNotesTextColor),
            CloneBrush(SeriesEditPaneButtonsBGColor),
            CloneBrush(SeriesEditPaneButtonsBGHoverColor),
            CloneBrush(SeriesEditPaneButtonsBorderColor),
            CloneBrush(SeriesEditPaneButtonsBorderHoverColor),
            CloneBrush(SeriesEditPaneButtonsIconColor),
            CloneBrush(SeriesEditPaneButtonsIconHoverColor)
        );
    }

    private static SolidColorBrush CloneBrush(SolidColorBrush? brush)
    {
        if (brush is null) return null!;
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
        BrushEquals(UserIconBorderColor, other.UserIconBorderColor) &&
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
        BrushEquals(StatusAndBookTypeBGHoverColor, other.StatusAndBookTypeBGHoverColor) &&
        BrushEquals(StatusAndBookTypeTextColor, other.StatusAndBookTypeTextColor) &&
        BrushEquals(StatusAndBookTypeTextHoverColor, other.StatusAndBookTypeTextHoverColor) &&
        BrushEquals(SeriesCardBGColor, other.SeriesCardBGColor) &&
        BrushEquals(SeriesCardTitleColor, other.SeriesCardTitleColor) &&
        BrushEquals(SeriesCardPublisherColor, other.SeriesCardPublisherColor) &&
        BrushEquals(SeriesCardStaffColor, other.SeriesCardStaffColor) &&
        BrushEquals(SeriesCardDescColor, other.SeriesCardDescColor) &&
        BrushEquals(SeriesProgressBGColor, other.SeriesProgressBGColor) &&
        BrushEquals(SeriesProgressBarColor, other.SeriesProgressBarColor) &&
        BrushEquals(SeriesProgressBarBGColor, other.SeriesProgressBarBGColor) &&
        BrushEquals(SeriesProgressBarBorderColor, other.SeriesProgressBarBorderColor) &&
        BrushEquals(SeriesProgressTextColor, other.SeriesProgressTextColor) &&
        BrushEquals(SeriesProgressButtonsHoverColor, other.SeriesProgressButtonsHoverColor) &&
        BrushEquals(SeriesButtonIconColor, other.SeriesButtonIconColor) &&
        BrushEquals(SeriesButtonIconHoverColor, other.SeriesButtonIconHoverColor) &&
        BrushEquals(SeriesEditPaneBGColor, other.SeriesEditPaneBGColor) &&
        BrushEquals(SeriesNotesBGColor, other.SeriesNotesBGColor) &&
        BrushEquals(SeriesNotesBorderColor, other.SeriesNotesBorderColor) &&
        BrushEquals(SeriesNotesTextColor, other.SeriesNotesTextColor) &&
        BrushEquals(SeriesEditPaneButtonsBGColor, other.SeriesEditPaneButtonsBGColor) &&
        BrushEquals(SeriesEditPaneButtonsBGHoverColor, other.SeriesEditPaneButtonsBGHoverColor) &&
        BrushEquals(SeriesEditPaneButtonsBorderColor, other.SeriesEditPaneButtonsBorderColor) &&
        BrushEquals(SeriesEditPaneButtonsBorderHoverColor, other.SeriesEditPaneButtonsBorderHoverColor) &&
        BrushEquals(SeriesEditPaneButtonsIconColor, other.SeriesEditPaneButtonsIconColor) &&
        BrushEquals(SeriesEditPaneButtonsIconHoverColor, other.SeriesEditPaneButtonsIconHoverColor);
    }

    private static bool BrushEquals(SolidColorBrush? a, SolidColorBrush? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Color.Equals(b.Color);
    }

    public class SolidColorBrushJsonConverter : JsonConverter<SolidColorBrush>
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

public class TsundokuThemeComparer : IComparer<TsundokuTheme>
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