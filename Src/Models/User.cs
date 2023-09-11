using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;

namespace Tsundoku.Models
{
    public class User
    {
        public string UserName { get; set; }
        public uint NumVolumesCollected { get; set; }
        public uint NumVolumesToBeCollected { get; set; }
        public string CurLanguage { get; set; }
        public string Display { get; set; }
        public string MainTheme { get; set; }
        public double CurDataVersion { get; set; }
        public string Currency { get; set; }
        public decimal MeanScore { get; set; }
        public uint VolumesRead { get; set; }
        public string CollectionPrice { get; set; }
        public Dictionary<string, bool> Memberships { get; set; }
        public byte[] UserIcon { get; set; }
        public ObservableCollection<TsundokuTheme> SavedThemes { get; set; }
        public ObservableCollection<Series> UserCollection { get; set; }

        public User(string userName, string curLanguage, string mainTheme, string display, double curDataVersion, string currency, string collectionPrice,  Dictionary<string, bool> memberships, ObservableCollection<TsundokuTheme> savedThemes, ObservableCollection<Series> userCollection)
        {
            UserName = userName;
            CurLanguage = curLanguage;
            Display = display;
            CurDataVersion = curDataVersion;
            Currency = currency;
            MainTheme = mainTheme;
            CollectionPrice = collectionPrice;
            SavedThemes = savedThemes;
            UserCollection = userCollection;
            Memberships = memberships;
            NumVolumesCollected = 0;
            NumVolumesToBeCollected = 0;
        }

        public static byte[] ImageToByteArray(Avalonia.Media.Imaging.Bitmap image)
        {
            using (MemoryStream stream = new())
            {
                image.Save(stream, 100);
                return stream.ToArray();
            }
        }
    }
}