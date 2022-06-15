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
        public char CurLanguage { get; set; }
        public string MainTheme { get; set; }
        public Dictionary<string, TsundokuTheme>? SavedThemes { get; set; }
        public Collection<Series> UserCollection { get; set; }

        public User() {}

        public User(string userName, char curLanguage, string mainTheme, Dictionary<string, TsundokuTheme>? savedThemes, Collection<Series> userCollection)
        {
            UserName = userName;
            NumVolumesCollected = 0;
            NumVolumesToBeCollected = 0;
            CurLanguage = curLanguage;
            MainTheme = mainTheme;
            SavedThemes = savedThemes;
            UserCollection = userCollection;
        }
    }
}