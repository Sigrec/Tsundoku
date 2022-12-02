using System;
using System.Collections.Generic;
using Avalonia.Media;
using Newtonsoft.Json.Linq;

namespace Tsundoku.Models
{
    public class TsundokuTheme : ICloneable, IEquatable<TsundokuTheme?>
    {
        public string ThemeName { get; set; }
        public UInt32 MenuBGColor { get; set; }
        public UInt32 UsernameColor { get; set; }
        public UInt32 MenuTextColor { get; set; }
        public UInt32 SearchBarBGColor { get; set; }
        public UInt32 SearchBarBorderColor { get; set; }
        public UInt32 SearchBarTextColor { get; set; } 
        public UInt32 MenuButtonBGColor { get; set; } 
        public UInt32 MenuButtonBGHoverColor { get; set; }
        public UInt32 MenuButtonBorderColor { get; set; }
        public UInt32 MenuButtonBorderHoverColor { get; set; }
        public UInt32 MenuButtonTextAndIconColor { get; set; }
        public UInt32 MenuButtonTextAndIconHoverColor { get; set; }
        public UInt32 DividerColor { get; set; }
        public UInt32 CollectionBGColor { get; set; }
        public UInt32 StatusAndBookTypeBGColor { get; set; }
        public UInt32 StatusAndBookTypeBGHoverColor { get; set; }
        public UInt32 StatusAndBookTypeTextColor { get; set; }
        public UInt32 StatusAndBookTypeTextHoverColor { get; set; }


        //public static final TsundOkuTheme DEFAULT_THEME = new TsundOkuTheme("Default Theme", "rgb(44,45,66); ", "rgb(223,213,158); ", "rgb(223,213,158); ", "rgb(223,213,158); ", "rgb(18,23,29); ", "rgb(18,23,29); ", "rgb(223,213,158); ", "rgb(18,23,29); ", 10"rgb(223,213,158); ", 11"rgb(223,213,158); ", 12"rgb(223,213,158); ", 13"rgba(18,23,29,0.6); ", 14"rgba(223,213,158,0.70); ", 15"rgb(223,213,158); ", 16"rgb(18,23,29); ", 17"rgb(223,213,158); ", 18"rgb(18,23,29); ", 19"rgb(18,23,29); ", 20"rgb(223,213,158); ", 21"rgb(44,45,66); ", 22"rgba(32,35,45,0.95); ", 23"rgba(223,213,158,0.95); ", "rgb(32,35,45); ", "rgb(223,213,158); ", "rgb(223,213,158); ", "rgb(223,213,158); ",  "rgba(236,236,236,0.9); ", "rgb(44,45,66); ", "rgb(44,45,66); ","rgb(223,213,158); ", "rgb(44,45,66); ", "rgb(223,213,158); ", "rgb(223,213,158); ", "rgb(18,23,29); ", "rgb(223,213,158); ", "rgb(18,23,29); ", "rgb(18,23,29); ", "rgb(223,213,158); ", "rgb(223,213,158); ");

        public static readonly TsundokuTheme DEFAULT_THEME = new TsundokuTheme("Default", Color.FromRgb(44, 45, 66).ToUint32(), Color.FromRgb(223, 213, 158).ToUint32(), Color.FromRgb(223, 213, 158).ToUint32(), Color.FromRgb(18, 23, 29).ToUint32(), Color.FromRgb(223, 213, 158).ToUint32(), Color.FromRgb(223, 213, 158).ToUint32(), Color.FromArgb((byte)Math.Floor(0.6 * 255), 18, 23, 29).ToUint32(), Color.FromArgb((byte)Math.Floor(0.7 * 255), 223, 213, 158).ToUint32(), Color.FromRgb(223, 213, 158).ToUint32(), Color.FromRgb(18, 23, 29).ToUint32(), Color.FromRgb(223, 213, 158).ToUint32(), Color.FromRgb(18, 23, 29).ToUint32(), Color.FromRgb(223, 213, 158).ToUint32(), Color.FromRgb(18, 23, 29).ToUint32(), Color.FromArgb((byte)Math.Floor(0.95 * 255), 32, 35, 45).ToUint32(), Color.FromArgb((byte)Math.Floor(0.95 * 255), 223, 213, 158).ToUint32(), Color.FromRgb(223, 213, 158).ToUint32(), Color.FromRgb(44, 45, 66).ToUint32());

        public TsundokuTheme()
        {

        }

        public TsundokuTheme(string themeName)
        {
            ThemeName = themeName;
        }

        public TsundokuTheme(string themeName, uint menuBGColor, uint usernameColor, uint menuTextColor, uint searchBarBGColor, uint searchBarBorderColor, uint searchBarTextColor, uint menuButtonBGColor, uint menuButtonBGHoverColor, uint menuButtonBorderColor, uint menuButtonBorderHoverColor, uint menuButtonTextAndIconColor, uint menuButtonTextAndIconHoverColor, uint dividerColor, uint collectionBGColor, uint statusAndBookTypeBGColor, uint statusAndBookTypeBGHoverColor, uint statusAndBookTypeTextColor, uint statusAndBookTypeTextHoverColor) : this(themeName)
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
        }

        public virtual TsundokuTheme Cloning()
        {
            return new TsundokuTheme(ThemeName, MenuBGColor, UsernameColor, MenuTextColor, SearchBarBGColor, SearchBarBorderColor, SearchBarTextColor, MenuButtonBGColor, MenuButtonBGHoverColor, MenuButtonBorderColor, MenuButtonBorderHoverColor, MenuButtonTextAndIconColor, MenuButtonTextAndIconHoverColor, DividerColor, CollectionBGColor, StatusAndBookTypeBGColor, StatusAndBookTypeBGHoverColor, StatusAndBookTypeTextColor, StatusAndBookTypeTextHoverColor);
        }
        object ICloneable.Clone()
        {
            return Cloning();
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
                   StatusAndBookTypeTextHoverColor == other.StatusAndBookTypeTextHoverColor;
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