using System.Collections.ObjectModel;

namespace Tsundoku.Models
{
    public class User
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public string UserName { get; set; }
        public uint NumVolumesCollected { get; set; }
        public uint NumVolumesToBeCollected { get; set; }
        public string CurLanguage { get; set; }
        public string Display { get; set; }
        public string MainTheme { get; set; }
        public ObservableCollection<TsundokuTheme> SavedThemes { get; set; }
        public ObservableCollection<Series> UserCollection { get; set; }

        public User(string userName, string curLanguage, string mainTheme, string display, ObservableCollection<TsundokuTheme> savedThemes, ObservableCollection<Series> userCollection)
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