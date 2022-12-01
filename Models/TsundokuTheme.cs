using System;
using Avalonia.Media;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.Models
{
    [Serializable]
    public class TsundokuTheme
    {
        public string ThemeName { get; set; }
        public UInt32 MenuBGColor { get; set; }
        public UInt32 UsernameColor { get; set; }
        public UInt32 MenuTextColor { get; set; }
        public UInt32 SearchBarBGColor { get; set; }
        public UInt32 SearchBarBorderColor { get; set; }
        public UInt32 SearchBarTextColor { get; set; }
        public UInt32 Seperator { get; set; }
        public UInt32 CollectionBGColor { get; set; }

        public TsundokuTheme()
        {

        }

        public TsundokuTheme(string themeName)
        {
            ThemeName = themeName;
        }
    }
}