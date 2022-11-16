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
        public uint NumVolumesCollected { get; set; }
        public uint NumVolumesToBeCollected { get; set; }
        public string CurLanguage { get; set; }
        public string MainTheme { get; set; }
        public string Display { get; set; }
        public Dictionary<string, TsundokuTheme>? SavedThemes { get; set; }
        public ObservableCollection<Series> UserCollection { get; set; }

        public User(string userName, string curLanguage, string mainTheme, string display, Dictionary<string, TsundokuTheme>? savedThemes, ObservableCollection<Series> userCollection)
        {
            UserName = userName;
            CurLanguage = curLanguage;
            Display = display;
            MainTheme = mainTheme;
            SavedThemes = savedThemes;
            UserCollection = userCollection;
            NumVolumesCollected = 0;
            NumVolumesToBeCollected = 0;
        }
    }
}