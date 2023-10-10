namespace Tsundoku.Models
{
    public class TsundokuTheme : ICloneable, IComparable
    {
        public string ThemeName { get; set; }
        public uint MenuBGColor { get; set; }
        public uint UsernameColor { get; set; }
        public uint MenuTextColor { get; set; }
        public uint SearchBarBGColor { get; set; }
        public uint SearchBarBorderColor { get; set; }
        public uint SearchBarTextColor { get; set; }
        public uint DividerColor { get; set; }
        public uint MenuButtonBGColor { get; set; } 
        public uint MenuButtonBGHoverColor { get; set; }
        public uint MenuButtonBorderColor { get; set; }
        public uint MenuButtonBorderHoverColor { get; set; }
        public uint MenuButtonTextAndIconColor { get; set; }
        public uint MenuButtonTextAndIconHoverColor { get; set; }
        public uint CollectionBGColor { get; set; }
        public uint StatusAndBookTypeBGColor { get; set; }
        public uint StatusAndBookTypeBGHoverColor { get; set; }
        public uint StatusAndBookTypeTextColor { get; set; }
        public uint StatusAndBookTypeTextHoverColor { get; set; }
        public uint SeriesCardBGColor { get; set; }
        public uint SeriesCardTitleColor { get; set; }
        public uint SeriesCardStaffColor { get; set; }
        public uint SeriesCardDescColor { get; set; }
        public uint SeriesProgressBGColor { get; set; }
        public uint SeriesProgressBarColor { get; set; }
        public uint SeriesProgressBarBGColor { get; set; }
        public uint SeriesProgressBarBorderColor { get; set; }
        public uint SeriesProgressTextColor { get; set; }
        public uint SeriesProgressButtonsHoverColor { get; set; }
        public uint SeriesButtonBGColor { get; set; }
        public uint SeriesButtonBGHoverColor { get; set; }
        public uint SeriesButtonIconColor { get; set; }
        public uint SeriesButtonIconHoverColor { get; set; }
        public uint SeriesEditPaneBGColor { get; set; }
        public uint SeriesNotesBGColor  { get; set; }
        public uint SeriesNotesBorderColor { get; set; }
        public uint SeriesNotesTextColor { get; set; }
        public uint SeriesEditPaneButtonsBGColor { get; set; }
        public uint SeriesEditPaneButtonsBGHoverColor { get; set; }
        public uint SeriesEditPaneButtonsBorderColor { get; set; }
        public uint SeriesEditPaneButtonsBorderHoverColor { get; set; }
        public uint SeriesEditPaneButtonsIconColor { get; set; }
        public uint SeriesEditPaneButtonsIconHoverColor { get; set; }

        public static readonly TsundokuTheme DEFAULT_THEME = new TsundokuTheme(
            "Default", //ThemeName
            4280296237,
            4290035163,
            4290035163,
            4284638304,
            4292859294,
            4290035163,
            4292859294,
            4284638304,
            4281085250,
            4292859294,
            4292859294,
            4290035163,
            4284638304,
            4281085250,
            4284638304,
            4292859294,
            4293717228,
            4284638304,
            4280296237,
            4292859294,
            4290035163,
            4293717228,
            4284638304,
            4292859294,
            4280296237,
            4293717228,
            4293717228,
            4280296237,
            4284638304,
            4284638304,
            4293717228,
            4292859294,
            4280296237,
            4284638304,
            4292859294,
            4290035163,
            4281085250,
            4284638304,
            4292859294,
            4292859294,
            4284638304,
            4290035163
        );

        public TsundokuTheme()
        {

        }

        public TsundokuTheme(string themeName)
        {
            ThemeName = themeName;
        }

        [JsonConstructor]
        public TsundokuTheme(string themeName, uint menuBGColor, uint usernameColor, uint menuTextColor, uint searchBarBGColor, uint searchBarBorderColor, uint searchBarTextColor, uint dividerColor, uint menuButtonBGColor, uint menuButtonBGHoverColor, uint menuButtonBorderColor, uint menuButtonBorderHoverColor, uint menuButtonTextAndIconColor, uint menuButtonTextAndIconHoverColor, uint collectionBGColor, uint statusAndBookTypeBGColor, uint statusAndBookTypeBGHoverColor, uint statusAndBookTypeTextColor, uint statusAndBookTypeTextHoverColor, uint seriesCardBGColor, uint seriesCardTitleColor, uint seriesCardStaffColor, uint seriesCardDescColor, uint seriesProgressBGColor, uint seriesProgressBarColor, uint seriesProgressBarBGColor, uint seriesProgressBarBorderColor, uint seriesProgressTextColor, uint seriesProgressButtonsHoverColor, uint seriesButtonBGColor, uint seriesButtonBGHoverColor, uint seriesButtonIconColor, uint seriesButtonIconHoverColor, uint seriesEditPaneBGColor, uint seriesNotesBGColor, uint seriesNotesBorderColor, uint seriesNotesTextColor, uint seriesEditPaneButtonsBGColor, uint seriesEditPaneButtonsBGHoverColor, uint seriesEditPaneButtonsBorderColor, uint seriesEditPaneButtonsBorderHoverColor, uint seriesEditPaneButtonsIconColor, uint seriesEditPaneButtonsIconHoverColor) : this(themeName)
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
            StatusAndBookTypeBGHoverColor = statusAndBookTypeBGHoverColor;
            StatusAndBookTypeTextColor = statusAndBookTypeTextColor;
            StatusAndBookTypeTextHoverColor = statusAndBookTypeTextHoverColor;
            SeriesCardBGColor = seriesCardBGColor;
            SeriesCardTitleColor = seriesCardTitleColor;
            SeriesCardStaffColor = seriesCardStaffColor;
            SeriesCardDescColor = seriesCardDescColor;
            SeriesProgressBGColor = seriesProgressBGColor;
            SeriesProgressBarColor = seriesProgressBarColor;
            SeriesProgressBarBGColor = seriesProgressBarBGColor;
            SeriesProgressBarBorderColor = seriesProgressBarBorderColor;
            SeriesProgressTextColor = seriesProgressTextColor;
            SeriesProgressButtonsHoverColor = seriesProgressButtonsHoverColor;
            SeriesButtonBGColor = seriesButtonBGColor;
            SeriesButtonBGHoverColor = seriesButtonBGHoverColor;
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
            hash.Add(SeriesCardStaffColor);
            hash.Add(SeriesCardDescColor);
            hash.Add(SeriesProgressBGColor);
            hash.Add(SeriesProgressBarColor);
            hash.Add(SeriesProgressBarBGColor);
            hash.Add(SeriesProgressBarBorderColor);
            hash.Add(SeriesProgressTextColor);
            hash.Add(SeriesProgressButtonsHoverColor);
            hash.Add(SeriesButtonBGColor);
            hash.Add(SeriesButtonBGHoverColor);
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
            return new TsundokuTheme(ThemeName, MenuBGColor, UsernameColor, MenuTextColor,SearchBarBGColor,SearchBarBorderColor,SearchBarTextColor, DividerColor, MenuButtonBGColor, MenuButtonBGHoverColor, MenuButtonBorderColor, MenuButtonBorderHoverColor, MenuButtonTextAndIconColor, MenuButtonTextAndIconHoverColor, CollectionBGColor,StatusAndBookTypeBGColor,StatusAndBookTypeBGHoverColor,StatusAndBookTypeTextColor,StatusAndBookTypeTextHoverColor,SeriesCardBGColor,SeriesCardTitleColor,SeriesCardStaffColor,SeriesCardDescColor,SeriesProgressBGColor,SeriesProgressBarColor,SeriesProgressBarBGColor,SeriesProgressBarBorderColor,SeriesProgressTextColor,SeriesProgressButtonsHoverColor,SeriesButtonBGColor,SeriesButtonBGHoverColor,SeriesButtonIconColor,SeriesButtonIconHoverColor,SeriesEditPaneBGColor,SeriesNotesBGColor,SeriesNotesBorderColor,SeriesNotesTextColor,SeriesEditPaneButtonsBGColor,SeriesEditPaneButtonsBGHoverColor,SeriesEditPaneButtonsBorderColor,SeriesEditPaneButtonsBorderHoverColor,SeriesEditPaneButtonsIconColor,SeriesEditPaneButtonsIconHoverColor);
        }

        object ICloneable.Clone()
        {
            return Cloning();
        }
    }
}