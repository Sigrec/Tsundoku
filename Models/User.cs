using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        public ObservableCollection<Series> UserCollection { get; set; }

        public User() {}

        public User(string userName, char curLanguage, string mainTheme, Dictionary<string, TsundokuTheme>? savedThemes, ObservableCollection<Series> userCollection)
        {
            UserName = userName;
            CurLanguage = curLanguage;
            MainTheme = mainTheme;
            SavedThemes = savedThemes;
            UserCollection = userCollection;
        }
    }
}