using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tsundoku.Src;

namespace Tsundoku.Models
{
    [Serializable]
    public class User
    {
        public string UserName { get; set; }
        public ushort NumVolumesCollected { get; set; }
        public ushort NumVolumesToBeCollected { get; set; }
        public string CurLanguage { get; set; }
        public string MainTheme { get; set; }
        public string Display { get; set; }
        public Dictionary<string, TsundokuTheme>? SavedThemes { get; set; }
        public Collection<Series> UserCollection { get; set; }

        public User() { }

        public User(string userName, string curLanguage, string mainTheme, string display, Dictionary<string, TsundokuTheme>? savedThemes, Collection<Series> userCollection)
        {
            UserName = userName;
            NumVolumesCollected = 0;
            NumVolumesToBeCollected = 0;
            CurLanguage = curLanguage;
            Display = display;
            MainTheme = mainTheme;
            SavedThemes = savedThemes;
            UserCollection = userCollection;
        }
    }
}