using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.Models
{
    public class TsundokuTheme : ReactiveObject, ICloneable, IComparable, IDisposable
    {
        [JsonIgnore] private bool disposedValue;
        
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

        public static readonly TsundokuTheme DEFAULT_THEME = new TsundokuTheme(
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
            hash.Add(MenuBGColor);
            hash.Add(UsernameColor);
            hash.Add(UserIconBorderColor);
            hash.Add(MenuTextColor);
            hash.Add(SearchBarBGColor);
            hash.Add(SearchBarBorderColor);
            hash.Add(SearchBarTextColor);
            hash.Add(MenuButtonBGColor);
            hash.Add(MenuButtonBGHoverColor);
            hash.Add(MenuButtonBorderColor);
            hash.Add(MenuButtonBorderHoverColor);
            hash.Add(MenuButtonTextAndIconColor);
            hash.Add(MenuButtonTextAndIconHoverColor);
            hash.Add(DividerColor);
            hash.Add(CollectionBGColor);
            hash.Add(StatusAndBookTypeBGColor);
            hash.Add(StatusAndBookTypeBGHoverColor);
            hash.Add(StatusAndBookTypeTextColor);
            hash.Add(StatusAndBookTypeTextHoverColor);
            hash.Add(SeriesCardBGColor);
            hash.Add(SeriesCardTitleColor);
            hash.Add(SeriesCardPublisherColor);
            hash.Add(SeriesCardStaffColor);
            hash.Add(SeriesCardDescColor);
            hash.Add(SeriesProgressBGColor);
            hash.Add(SeriesProgressBarColor);
            hash.Add(SeriesProgressBarBGColor);
            hash.Add(SeriesProgressBarBorderColor);
            hash.Add(SeriesProgressTextColor);
            hash.Add(SeriesProgressButtonsHoverColor);
            hash.Add(SeriesButtonIconColor);
            hash.Add(SeriesButtonIconHoverColor);
            hash.Add(SeriesEditPaneBGColor);
            hash.Add(SeriesNotesBGColor);
            hash.Add(SeriesNotesBorderColor);
            hash.Add(SeriesNotesTextColor);
            hash.Add(SeriesEditPaneButtonsBGColor);
            hash.Add(SeriesEditPaneButtonsBGHoverColor);
            hash.Add(SeriesEditPaneButtonsBorderColor);
            hash.Add(SeriesEditPaneButtonsBorderHoverColor);
            hash.Add(SeriesEditPaneButtonsIconColor);
            hash.Add(SeriesEditPaneButtonsIconHoverColor);
            return hash.ToHashCode();
        }

        public int CompareTo(object? obj)
        {
            return this.ThemeName.CompareTo((obj as TsundokuTheme).ThemeName);
        }

        public virtual TsundokuTheme Cloning()
        {
            return new TsundokuTheme(ThemeName, MenuBGColor, UsernameColor, UserIconBorderColor, MenuTextColor, SearchBarBGColor, SearchBarBorderColor, SearchBarTextColor, DividerColor, MenuButtonBGColor, MenuButtonBGHoverColor, MenuButtonBorderColor, MenuButtonBorderHoverColor, MenuButtonTextAndIconColor, MenuButtonTextAndIconHoverColor, CollectionBGColor, StatusAndBookTypeBGColor, StatusAndBookTypeBGHoverColor, StatusAndBookTypeTextColor, StatusAndBookTypeTextHoverColor, SeriesCardBGColor, SeriesCardTitleColor, SeriesCardPublisherColor, SeriesCardStaffColor, SeriesCardDescColor, SeriesProgressBGColor, SeriesProgressBarColor, SeriesProgressBarBGColor, SeriesProgressBarBorderColor, SeriesProgressTextColor, SeriesProgressButtonsHoverColor, SeriesButtonIconColor, SeriesButtonIconHoverColor, SeriesEditPaneBGColor, SeriesNotesBGColor, SeriesNotesBorderColor, SeriesNotesTextColor, SeriesEditPaneButtonsBGColor, SeriesEditPaneButtonsBGHoverColor, SeriesEditPaneButtonsBorderColor, SeriesEditPaneButtonsBorderHoverColor, SeriesEditPaneButtonsIconColor, SeriesEditPaneButtonsIconHoverColor);
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
            string.Equals(MenuBGColor, other.MenuBGColor) &&
            string.Equals(UsernameColor, other.UsernameColor) &&
            string.Equals(UserIconBorderColor, other.UserIconBorderColor) &&
            string.Equals(MenuTextColor, other.MenuTextColor) &&
            string.Equals(SearchBarBGColor, other.SearchBarBGColor) &&
            string.Equals(SearchBarBorderColor, other.SearchBarBorderColor) &&
            string.Equals(SearchBarTextColor, other.SearchBarTextColor) &&
            string.Equals(DividerColor, other.DividerColor) &&
            string.Equals(MenuButtonBGColor, other.MenuButtonBGColor) &&
            string.Equals(MenuButtonBGHoverColor, other.MenuButtonBGHoverColor) &&
            string.Equals(MenuButtonBorderColor, other.MenuButtonBorderColor) &&
            string.Equals(MenuButtonBorderHoverColor, other.MenuButtonBorderHoverColor) &&
            string.Equals(MenuButtonTextAndIconColor, other.MenuButtonTextAndIconColor) &&
            string.Equals(MenuButtonTextAndIconHoverColor, other.MenuButtonTextAndIconHoverColor) &&
            string.Equals(CollectionBGColor, other.CollectionBGColor) &&
            string.Equals(StatusAndBookTypeBGColor, other.StatusAndBookTypeBGColor) &&
            string.Equals(StatusAndBookTypeBGHoverColor, other.StatusAndBookTypeBGHoverColor) &&
            string.Equals(StatusAndBookTypeTextColor, other.StatusAndBookTypeTextColor) &&
            string.Equals(StatusAndBookTypeTextHoverColor, other.StatusAndBookTypeTextHoverColor) &&
            string.Equals(SeriesCardBGColor, other.SeriesCardBGColor) &&
            string.Equals(SeriesCardTitleColor, other.SeriesCardTitleColor) &&
            string.Equals(SeriesCardPublisherColor, other.SeriesCardPublisherColor) &&
            string.Equals(SeriesCardStaffColor, other.SeriesCardStaffColor) &&
            string.Equals(SeriesCardDescColor, other.SeriesCardDescColor) &&
            string.Equals(SeriesProgressBGColor, other.SeriesProgressBGColor) &&
            string.Equals(SeriesProgressBarColor, other.SeriesProgressBarColor) &&
            string.Equals(SeriesProgressBarBGColor, other.SeriesProgressBarBGColor) &&
            string.Equals(SeriesProgressBarBorderColor, other.SeriesProgressBarBorderColor) &&
            string.Equals(SeriesProgressTextColor, other.SeriesProgressTextColor) &&
            string.Equals(SeriesProgressButtonsHoverColor, other.SeriesProgressButtonsHoverColor) &&
            string.Equals(SeriesButtonIconColor, other.SeriesButtonIconColor) &&
            string.Equals(SeriesButtonIconHoverColor, other.SeriesButtonIconHoverColor) &&
            string.Equals(SeriesEditPaneBGColor, other.SeriesEditPaneBGColor) &&
            string.Equals(SeriesNotesBGColor, other.SeriesNotesBGColor) &&
            string.Equals(SeriesNotesBorderColor, other.SeriesNotesBorderColor) &&
            string.Equals(SeriesNotesTextColor, other.SeriesNotesTextColor) &&
            string.Equals(SeriesEditPaneButtonsBGColor, other.SeriesEditPaneButtonsBGColor) &&
            string.Equals(SeriesEditPaneButtonsBGHoverColor, other.SeriesEditPaneButtonsBGHoverColor) &&
            string.Equals(SeriesEditPaneButtonsBorderColor, other.SeriesEditPaneButtonsBorderColor) &&
            string.Equals(SeriesEditPaneButtonsBorderHoverColor, other.SeriesEditPaneButtonsBorderHoverColor) &&
            string.Equals(SeriesEditPaneButtonsIconColor, other.SeriesEditPaneButtonsIconColor) &&
            string.Equals(SeriesEditPaneButtonsIconHoverColor, other.SeriesEditPaneButtonsIconHoverColor);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
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

        public class SolidColorBrushJsonConverter : JsonConverter<SolidColorBrush>
        {
            public override SolidColorBrush? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string? hex = reader.GetString();

                if (string.IsNullOrWhiteSpace(hex))
                    return null;

                try
                {
                    Color color = Color.Parse(hex);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return null;
                }
            }

            public override void Write(Utf8JsonWriter writer, SolidColorBrush value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                    return;
                }

                writer.WriteStringValue(value.Color.ToString()); // e.g. "#FF112233"
            }
        }
    }
}