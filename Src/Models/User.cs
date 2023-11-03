using System.Collections.ObjectModel;
using static MangaAndLightNovelWebScrape.Models.Constants;

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
        public decimal MeanRating { get; set; }
        public uint VolumesRead { get; set; }
        public string CollectionPrice { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))] public Region Region { get; set; }
        public Dictionary<string, bool> Memberships { get; set; }
        public byte[] UserIcon { get; set; }
        public ObservableCollection<TsundokuTheme> SavedThemes { get; set; }
        public List<Series> UserCollection { get; set; }

        public User(string UserName, string CurLanguage, string MainTheme, string Display, double CurDataVersion, string Currency, string CollectionPrice, Region Region, Dictionary<string, bool> Memberships, ObservableCollection<TsundokuTheme> SavedThemes, List<Series> UserCollection)
        {
            this.UserName = UserName;
            this.CurLanguage = CurLanguage;
            this.Display = Display;
            this.CurDataVersion = CurDataVersion;
            this.Currency = Currency;
            this.MainTheme = MainTheme;
            this.CollectionPrice = CollectionPrice;
            this.Region = Region;
            this.SavedThemes = SavedThemes;
            this.UserCollection = UserCollection;
            this.Memberships = Memberships;
            NumVolumesCollected = 0;
            NumVolumesToBeCollected = 0;
        }

        public static byte[] ImageToByteArray(Avalonia.Media.Imaging.Bitmap image)
        {
            using MemoryStream stream = new();
            image.Save(stream, 100);
            return stream.ToArray();
        }
    }

    // [JsonSerializable(typeof(User))]
    // [JsonSourceGenerationOptions(WriteIndented = true, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Disallow)]
    // internal partial class Context : JsonSerializerContext
    // {
        
    // }
}