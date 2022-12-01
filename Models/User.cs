using System.Collections.ObjectModel;
using System;
namespace Tsundoku.Models
{
    [Serializable]
    public class User
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public string UserName { get; set; }
        public uint NumVolumesCollected { get; set; }
        public uint NumVolumesToBeCollected { get; set; }
        public string CurLanguage { get; set; }
        public TsundokuTheme MainTheme { get; set; }
        public string Display { get; set; }
        public ObservableCollection<TsundokuTheme> SavedThemes { get; set; }
        public ObservableCollection<Series> UserCollection { get; set; }

        public User(string userName, string curLanguage, TsundokuTheme mainTheme, string display, ObservableCollection<TsundokuTheme> savedThemes, ObservableCollection<Series> userCollection)
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

        public void AddNewTheme(ObservableCollection<TsundokuTheme> curThemes, TsundokuTheme newTheme)
        {
            bool duplicateCheck = false;
            for (int x = 0; x < curThemes.Count; x++)
            {
                if (newTheme.ThemeName.Equals(curThemes[x].ThemeName))
                {
                    duplicateCheck = true;
                    curThemes[x] = newTheme;
                    Logger.Info($"{newTheme.ThemeName} Already Exists Replacing Color Values");
                    break;
                }
            }

            if (!duplicateCheck)
            {
                curThemes.Insert(0, newTheme);
                Logger.Info($"Added New Theme {newTheme.ThemeName} to Saved Themes");
            }
        }
    }
}