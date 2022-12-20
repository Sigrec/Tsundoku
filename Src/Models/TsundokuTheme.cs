using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace Tsundoku.Models
{
    public class TsundokuTheme : ICloneable, IEquatable<TsundokuTheme?>
    {
        public string ThemeName { get; set; }
        public uint MenuBGColor { get; set; }
        public uint UsernameColor { get; set; }
        public uint MenuTextColor { get; set; }
        public uint SearchBarBGColor { get; set; }
        public uint SearchBarBorderColor { get; set; }
        public uint SearchBarTextColor { get; set; } 
        public uint MenuButtonBGColor { get; set; } 
        public uint MenuButtonBGHoverColor { get; set; }
        public uint MenuButtonBorderColor { get; set; }
        public uint MenuButtonBorderHoverColor { get; set; }
        public uint MenuButtonTextAndIconColor { get; set; }
        public uint MenuButtonTextAndIconHoverColor { get; set; }
        public uint DividerColor { get; set; }
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
        public uint SeriesSwitchPaneButtonBGColor { get; set; }
        public uint SeriesSwitchPaneButtonBGHoverColor { get; set; }
        public uint SeriesSwitchPaneButtonIconColor { get; set; }
        public uint SeriesSwitchPaneButtonIconHoverColor { get; set; }
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


        //public static final TsundOkuTheme DEFAULT_THEME = new TsundOkuTheme(1"Default Theme", 2"rgb(44,45,66); ", 3"rgb(223,213,158); ", 4"rgb(223,213,158); ", 5"rgb(223,213,158); ", 6"rgb(18,23,29); ", 7"rgb(18,23,29); ", 8"rgb(223,213,158); ", 9"rgb(18,23,29); ", 10"rgb(223,213,158); ", 11"rgb(223,213,158); ", 12"rgb(223,213,158); ", 13"rgba(18,23,29,0.6); ", 14"rgba(223,213,158,0.70); ", 15"rgb(223,213,158); ", 16"rgb(18,23,29); ", 17"rgb(223,213,158); ", 18"rgb(18,23,29); ", 19"rgb(18,23,29); ", 20"rgb(223,213,158); ", 21"rgb(44,45,66); ", 22"rgba(32,35,45,0.95); ", 23"rgba(223,213,158,0.95); ", 24"rgb(32, 35, 45); ", 25"rgb(223,213,158); ", 26"rgb(223,213,158); ", 27"rgb(223,213,158); ",  28"rgba(236,236,236,0.9); ", 29"rgb(44,45,66); ", 30"rgb(44, 45, 66); ",31"rgb(223,213,158); ", 32"rgb(44,45,66); ", 33"rgb(223,213,158); ", 34"rgb(223,213,158); ", 35"rgb(18, 23, 29); ", 36"rgb(223,213,158); ", 37"rgb(18, 23, 29); ", 38"rgb(18,23,29); ", 39"rgb(223,213,158); ", 40"rgb(223,213,158); ");



        public static readonly TsundokuTheme DEFAULT_THEME = new TsundokuTheme(
        "Default", 
        Color.FromRgb(44, 45, 66).ToUint32(), 
        Color.FromRgb(223, 213, 158).ToUint32(), 
        Color.FromRgb(223, 213, 158).ToUint32(), 
        Color.FromRgb(18, 23, 29).ToUint32(), 
        Color.FromRgb(223, 213, 158).ToUint32(), //SearchBarBGColor
        Color.FromRgb(223, 213, 158).ToUint32(), 
        Color.FromArgb((byte)Math.Floor(0.6 * 255), 18, 23, 29).ToUint32(), 
        Color.FromArgb((byte)Math.Floor(0.7 * 255), 223, 213, 158).ToUint32(), 
        Color.FromRgb(223, 213, 158).ToUint32(), 
        Color.FromRgb(18, 23, 29).ToUint32(), //MenuButtonBorderHoverColor
        Color.FromRgb(223, 213, 158).ToUint32(), 
        Color.FromRgb(18, 23, 29).ToUint32(), 
        Color.FromRgb(223, 213, 158).ToUint32(), 
        Color.FromRgb(18, 23, 29).ToUint32(), 
        Color.FromRgb(32, 35, 45).ToUint32(), //StatusAndBookTypeBGColor
        Color.FromRgb(223, 213, 158).ToUint32(), //StatusAndBookTypeBGHoverColor
        Color.FromRgb(223, 213, 158).ToUint32(), //StatusAndBookTypeTextColor
        Color.FromRgb(32, 35, 45).ToUint32(), //StatusAndBookTypeTextHoverColor
        Color.FromRgb(32, 35, 45).ToUint32(), //SeriesCardBGColor
        Color.FromRgb(223, 213, 158).ToUint32(), //20
        Color.FromRgb(223, 213, 158).ToUint32(), 
        Color.FromArgb((byte)Math.Floor(0.9 * 255), 236, 236, 236).ToUint32(), 
        Color.FromRgb(44, 45, 66).ToUint32(), //SeriesProgressBGColor
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesProgressBarColor
        Color.FromRgb(18, 23, 29).ToUint32(), //SeriesProgressBarBGColor
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesProgressBarBorderColor
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesProgressTextColor
        Color.FromRgb(18, 23, 29).ToUint32(), //SeriesProgressButtonsHoverColor
        Color.FromRgb(44, 45, 66).ToUint32(), //SeriesSwitchPaneButtonBGColor
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesSwitchPaneButtonBGHoverColor
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesSwitchPaneButtonIconColor
        Color.FromRgb(18, 23, 29).ToUint32(), //SeriesSwitchPaneButtonIconHoverColor
        Color.FromRgb(32, 35, 45).ToUint32(), //SeriesEditPaneBGColor 
        Color.FromRgb(18, 23, 29).ToUint32(), //SeriesNotesBGColor
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesNotesBorderColor
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesNotesTextColor
        Color.FromRgb(44, 45, 66).ToUint32(), //SeriesEditPaneButtonsBGColor
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesEditPaneButtonsBGHoverColor 
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesEditPaneButtonsBorderColor
        Color.FromRgb(44, 45, 66).ToUint32(), //SeriesEditPaneButtonsBorderHoverColor
        Color.FromRgb(223, 213, 158).ToUint32(), //SeriesEditPaneButtonsIconColor
        Color.FromRgb(44, 45, 66).ToUint32() //SeriesEditPaneButtonsIconHoverColor
        );

        public TsundokuTheme()
        {

        }

        public TsundokuTheme(string themeName)
        {
            ThemeName = themeName;
        }

        public TsundokuTheme(string themeName, uint menuBGColor, uint usernameColor, uint menuTextColor, uint searchBarBGColor, uint searchBarBorderColor, uint searchBarTextColor, uint menuButtonBGColor, uint menuButtonBGHoverColor, uint menuButtonBorderColor, uint menuButtonBorderHoverColor, uint menuButtonTextAndIconColor, uint menuButtonTextAndIconHoverColor, uint dividerColor, uint collectionBGColor, uint statusAndBookTypeBGColor, uint statusAndBookTypeBGHoverColor, uint statusAndBookTypeTextColor, uint statusAndBookTypeTextHoverColor, uint seriesCardBGColor, uint seriesCardTitleColor, uint seriesCardStaffColor, uint seriesCardDescColor, uint seriesProgressBGColor, uint seriesProgressBarColor, uint seriesProgressBarBGColor, uint seriesProgressBarBorderColor, uint seriesProgressTextColor, uint seriesProgressButtonsHoverColor, uint seriesSwitchPaneButtonBGColor, uint seriesSwitchPaneButtonBGHoverColor, uint seriesSwitchPaneButtonIconColor, uint seriesSwitchPaneButtonIconHoverColor, uint seriesEditPaneBGColor, uint seriesNotesBGColor, uint seriesNotesBorderColor, uint seriesNotesTextColor, uint seriesEditPaneButtonsBGColor, uint seriesEditPaneButtonsBGHoverColor, uint seriesEditPaneButtonsBorderColor, uint seriesEditPaneButtonsBorderHoverColor, uint seriesEditPaneButtonsIconColor, uint seriesEditPaneButtonsIconHoverColor) : this(themeName)
        {
            MenuBGColor = menuBGColor;
            UsernameColor = usernameColor;
            MenuTextColor = menuTextColor;
            SearchBarBGColor = searchBarBGColor;
            SearchBarBorderColor = searchBarBorderColor;
            SearchBarTextColor = searchBarTextColor;
            MenuButtonBGColor = menuButtonBGColor;
            MenuButtonBGHoverColor = menuButtonBGHoverColor;
            MenuButtonBorderColor = menuButtonBorderColor;
            MenuButtonBorderHoverColor = menuButtonBorderHoverColor;
            MenuButtonTextAndIconColor = menuButtonTextAndIconColor;
            MenuButtonTextAndIconHoverColor = menuButtonTextAndIconHoverColor;
            DividerColor = dividerColor;
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
            SeriesSwitchPaneButtonBGColor = seriesSwitchPaneButtonBGColor;
            SeriesSwitchPaneButtonBGHoverColor = seriesSwitchPaneButtonBGHoverColor;
            SeriesSwitchPaneButtonIconColor = seriesSwitchPaneButtonIconColor;
            SeriesSwitchPaneButtonIconHoverColor = seriesSwitchPaneButtonIconHoverColor;
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

        public virtual TsundokuTheme Cloning()
        {
            return new TsundokuTheme(ThemeName, MenuBGColor, UsernameColor, MenuTextColor, SearchBarBGColor, SearchBarBorderColor, SearchBarTextColor, MenuButtonBGColor, MenuButtonBGHoverColor, MenuButtonBorderColor, MenuButtonBorderHoverColor, MenuButtonTextAndIconColor, MenuButtonTextAndIconHoverColor, DividerColor, CollectionBGColor, StatusAndBookTypeBGColor, StatusAndBookTypeBGHoverColor, StatusAndBookTypeTextColor, StatusAndBookTypeTextHoverColor, SeriesCardBGColor, SeriesCardTitleColor, SeriesCardStaffColor, SeriesCardDescColor, SeriesProgressBGColor, SeriesProgressBarColor, SeriesProgressBarBGColor, SeriesProgressBarBorderColor, SeriesProgressTextColor, SeriesProgressButtonsHoverColor, SeriesSwitchPaneButtonBGColor, SeriesSwitchPaneButtonBGHoverColor, SeriesSwitchPaneButtonIconColor, SeriesSwitchPaneButtonIconHoverColor, SeriesEditPaneBGColor, SeriesNotesBGColor, SeriesNotesBorderColor, SeriesNotesTextColor, SeriesEditPaneButtonsBGColor, SeriesEditPaneButtonsBGHoverColor, SeriesEditPaneButtonsBorderColor, SeriesEditPaneButtonsBorderHoverColor, SeriesEditPaneButtonsIconColor, SeriesEditPaneButtonsIconHoverColor);
        }

        object ICloneable.Clone()
        {
            return Cloning();
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
            hash.Add(SeriesSwitchPaneButtonBGColor);
            hash.Add(SeriesSwitchPaneButtonBGHoverColor);
            hash.Add(SeriesSwitchPaneButtonIconColor);
            hash.Add(SeriesSwitchPaneButtonIconHoverColor);
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as TsundokuTheme);
        }

        public bool Equals(TsundokuTheme? other)
        {
            return other is not null &&
                   ThemeName == other.ThemeName &&
                   MenuBGColor == other.MenuBGColor &&
                   UsernameColor == other.UsernameColor &&
                   MenuTextColor == other.MenuTextColor &&
                   SearchBarBGColor == other.SearchBarBGColor &&
                   SearchBarBorderColor == other.SearchBarBorderColor &&
                   SearchBarTextColor == other.SearchBarTextColor &&
                   MenuButtonBGColor == other.MenuButtonBGColor &&
                   MenuButtonBGHoverColor == other.MenuButtonBGHoverColor &&
                   MenuButtonBorderColor == other.MenuButtonBorderColor &&
                   MenuButtonBorderHoverColor == other.MenuButtonBorderHoverColor &&
                   MenuButtonTextAndIconColor == other.MenuButtonTextAndIconColor &&
                   MenuButtonTextAndIconHoverColor == other.MenuButtonTextAndIconHoverColor &&
                   DividerColor == other.DividerColor &&
                   CollectionBGColor == other.CollectionBGColor &&
                   StatusAndBookTypeBGColor == other.StatusAndBookTypeBGColor &&
                   StatusAndBookTypeBGHoverColor == other.StatusAndBookTypeBGHoverColor &&
                   StatusAndBookTypeTextColor == other.StatusAndBookTypeTextColor &&
                   StatusAndBookTypeTextHoverColor == other.StatusAndBookTypeTextHoverColor &&
                   SeriesCardBGColor == other.SeriesCardBGColor &&
                   SeriesCardTitleColor == other.SeriesCardTitleColor &&
                   SeriesCardStaffColor == other.SeriesCardStaffColor &&
                   SeriesCardDescColor == other.SeriesCardDescColor &&
                   SeriesProgressBGColor == other.SeriesProgressBGColor &&
                   SeriesProgressBarColor == other.SeriesProgressBarColor &&
                   SeriesProgressBarBGColor == other.SeriesProgressBarBGColor &&
                   SeriesProgressBarBorderColor == other.SeriesProgressBarBorderColor &&
                   SeriesProgressTextColor == other.SeriesProgressTextColor &&
                   SeriesProgressButtonsHoverColor == other.SeriesProgressButtonsHoverColor &&
                   SeriesSwitchPaneButtonBGColor == other.SeriesSwitchPaneButtonBGColor &&
                   SeriesSwitchPaneButtonBGHoverColor == other.SeriesSwitchPaneButtonBGHoverColor &&
                   SeriesSwitchPaneButtonIconColor == other.SeriesSwitchPaneButtonIconColor &&
                   SeriesSwitchPaneButtonIconHoverColor == other.SeriesSwitchPaneButtonIconHoverColor &&
                   SeriesEditPaneBGColor == other.SeriesEditPaneBGColor &&
                   SeriesNotesBGColor == other.SeriesNotesBGColor &&
                   SeriesNotesBorderColor == other.SeriesNotesBorderColor &&
                   SeriesNotesTextColor == other.SeriesNotesTextColor &&
                   SeriesEditPaneButtonsBGColor == other.SeriesEditPaneButtonsBGColor &&
                   SeriesEditPaneButtonsBGHoverColor == other.SeriesEditPaneButtonsBGHoverColor &&
                   SeriesEditPaneButtonsBorderColor == other.SeriesEditPaneButtonsBorderColor &&
                   SeriesEditPaneButtonsBorderHoverColor == other.SeriesEditPaneButtonsBorderHoverColor &&
                   SeriesEditPaneButtonsIconColor == other.SeriesEditPaneButtonsIconColor &&
                   SeriesEditPaneButtonsIconHoverColor == other.SeriesEditPaneButtonsIconHoverColor;
        }

        public static bool operator ==(TsundokuTheme? left, TsundokuTheme? right)
        {
            return EqualityComparer<TsundokuTheme>.Default.Equals(left, right);
        }

        public static bool operator !=(TsundokuTheme? left, TsundokuTheme? right)
        {
            return !(left == right);
        }
    }
}