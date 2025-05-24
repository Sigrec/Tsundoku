namespace Tsundoku.Models
{
    public class Constants
    {
		public const ushort USER_ICON_HEIGHT = 71;
		public const ushort USER_ICON_WIDTH = 71;
        public const ushort CARD_HEIGHT = 290;
		public const ushort CARD_WIDTH = 525;
        public const ushort RIGHT_SIDE_CARD_WIDTH = 355;
        public const ushort LEFT_SIDE_CARD_WIDTH = CARD_WIDTH - RIGHT_SIDE_CARD_WIDTH;
        public const ushort BOTTOM_SECTION_CARD_HEIGHT = 40;
        public const ushort TOP_SECTION_CARD_HEIGHT = CARD_HEIGHT - BOTTOM_SECTION_CARD_HEIGHT - 40;
        public const ushort USER_NOTES_WIDTH = RIGHT_SIDE_CARD_WIDTH - 74;
        public const ushort USER_NOTES_HEIGHT = TOP_SECTION_CARD_HEIGHT - 16;
        public const ushort IMAGE_HEIGHT = CARD_HEIGHT - BOTTOM_SECTION_CARD_HEIGHT;
        public static IReadOnlyList<string> ADVANCED_SEARCH_FILTERS { get; } = ["CurVolumes==", "CurVolumes>=", "CurVolumes<=", "Demographic==Josei", "Demographic==Seinen", "Demographic==Shoujo", "Demographic==Shounen", "Demographic==Unknown", "Favorite==False", "Favorite==True", "Format==Manga", "Format==Novel", "Genre==Action", "Genre==Adventure", "Genre==Comedy", "Genre==Drama", "Genre==Ecchi", "Genre==Fantasy", "Genre==Horror", "Genre==MahouShoujo", "Genre==Mecha", "Genre==Music", "Genre==Mystery", "Genre==Psychological", "Genre==Romance", "Genre==SciFi", "Genre==SliceOfLife", "Genre==Sports", "Genre==Supernatural", "Genre==Thriller", "MaxVolumes==", "MaxVolumes>=", "MaxVolumes<=", "Notes==", "Publisher==", "Read==", "Read>=", "Read<=", "Rating==", "Rating>=", "Rating<=", "Series==Complete", "Series==InComplete", "Status==Cancelled", "Status==Finished", "Status==Hiatus", "Status==Ongoing", "Value==", "Value>=", "Value<="];
        public const ushort MENU_LENGTH = 400;
		public static readonly string[] VALID_STAFF_ROLES = ["Story & Art", "Story", "Art", "Original Creator", "Character Design", "Cover Illustration", "Illustration", "Mechanical Design", "Original Story", "Original Character Design", "Original Story"];
		public static readonly string[] AVAILABLE_LANGUAGES = ["Romaji", "English", "Japanese", "Arabic", "Azerbaijan", "Bengali", "Bulgarian", "Burmese", "Catalan", "Chinese", "Croatian", "Czech", "Danish", "Dutch", "Esperanto", "Estonian", "Filipino", "Finnish", "French", "German", "Greek", "Hebrew", "Hindi", "Hungarian", "Indonesian", "Italian", "Kazakh", "Korean", "Latin", "Lithuanian", "Malay", "Mongolian", "Nepali", "Norwegian", "Persian", "Polish", "Portuguese", "Romanian", "Russian", "Serbian", "Slovak", "Spanish", "Swedish", "Tamil", "Thai", "Turkish", "Ukrainian", "Vietnamese"];
		public static readonly string[] AVAILABLE_CURRENCY = ["$", "€", "£", "¥", "₣", "₹", "₱", "₩", "₽", "₺", "₫", "฿", "₸", "₼", "₾", "₻"]; // "Rp", "RM", "﷼", "د.إ", "د. ك"
		public static readonly string[] AVAILABLE_COLLECTION_FILTERS = ["None", "Query", "Favorites", "Complete", "Incomplete", "Ongoing", "Finished", "Hiatus", "Cancelled", "Shounen", "Shoujo", "Seinen", "Josei", "Manga", "Novel", "Action", "Adventure", "Comedy", "Drama", "Ecchi", "Fantsay", "Horror", "Mahou Shoujo", "Mecha", "Music", "Mystery", "Psychological", "Romance", "Sci-Fi", "Slice of Life", "Sports", "Supernatural", "Thriller", "Publisher", "Read", "Unread", "Rating", "Value"];

        public enum Site
        {
            AniList,
            MangaDex
        }

        public enum Status
        {
            Finished,
            Ongoing,
            Hiatus,
            Cancelled,
            Error
        }
        public enum Format
        {
            Manga,
            Manhwa,
            Manhua,
            Manfra,
            Comic,
            Novel
        }
        public enum Demographic
        {
            Shounen,
            Shoujo,
            Seinen,
            Josei,
            Unknown
        }
        public static readonly Demographic[] DEMOGRAPHICS = Enum.GetValues<Demographic>();

        // public enum CollectionFilter
        // {
        //     Ongoing,
        //     Finished,
        //     Hiatus,
        //     Cancelled,
        //     Complete,
        //     Incomplete,
        //     Favorites,
        //     Manga,
        //     Novel,
        //     Shounen,
        //     Shoujo,
        //     Seinen,
        //     Josei,
        //     Read,
        //     Unread,
        //     Rating,
        //     Value,
        //     Query,
        //     None
        // }

        public readonly static Dictionary<string, string> ANILIST_LANG_CODES = new()
		{
			{ "JP", "Japanese" },
			{ "FR" , "French" },
			{ "EN" , "English" },
			{ "KR" , "Korean" },
			{ "CN" , "Chinese" },
			{ "TW" , "Chinese" }
		};

		public readonly static Dictionary<string, string> CULTURE_LANG_CODES = new()
		{
            { "Japanese" , "ja-JP" },
            { "Korean" , "ko-kr" },
            { "Arabic" , "ar-SA" },
            { "Azerbaijan" , "az-Latn-AZ" },
            { "Bengali" , "bn-BD" },
            { "Bulgarian" , "bg-BG" },
            { "Burmese" , "my-MM" },
            { "Catalan" , "ca-ES" },
            { "Chinese" , "zh-CN" },
            { "Croatian" , "hr-HR" },
            { "Czech" , "cs-CZ" },
            { "Danish" , "da-DK" },
            { "Esperanto" , "eo-001" },
            { "Estonian" , "et-EE" },
            { "Filipino" , "fil-PH" },
            { "Finnish" , "fi-FI" },
            { "French" , "fr-FR" },
            { "German" , "de-DE" },
            { "Greek" , "el-GR" },
            { "Hebrew" , "he-IL" },
            { "Hindi" , "hi-IN" },
            { "Hungarian" , "hu-HU" },
            { "Indonesian" , "id-ID" },
            { "Italian" , "it-IT" },
            { "Kazakh" , "kk-KZ" },
            { "Latin" , "la-001" },
            { "Lithuanian" , "lt-LT" },
            { "Malay" , "ms-MY" },
            { "Mongolian" , "mn-MN" },
            { "Nepali" , "ne-NP" },
            { "Norwegian" , "nb-NO" },
            { "Persian" , "fa-IR" },
            { "Polish" , "pl-PL" },
            { "Portuguese" , "pt-BR" }, // Portuguese (Brazil)
            { "Romanian" , "ro-RO" },
            { "Russian" , "ru-RU" },
            { "Serbian" , "sr-Latn-RS" },
            { "Slovak" , "sk-SK" },
            { "Spanish" , "es-ES" },
            { "Swedish" , "sv-SE" },
            { "Tamil" , "ta-IN" },
            { "Thai" , "th-TH" },
            { "Turkish" , "tr-TR" },
            { "Ukrainian" , "uk-UA" },
            { "Vietnamese" , "vi-VN" },
            { "English" , "en-US" },
            { "Romaji" , "en-US" }
		};

		public readonly static Dictionary<string, string> MANGADEX_LANG_CODES = new()
		{
			{ "ja", "Japanese" },
            { "ja-ro", "Romaji" },
			{ "ko" , "Korean" },
            { "ko-ro", "Romanized Korean"},
			{ "fr" , "French" },
			{ "en" , "English" },
			{ "ar" , "Arabic" },
			{ "az" , "Azerbaijani" },
			{ "bn" , "Bengali" },
			{ "bg" , "Bulgarian" },
			{ "my" , "Burmese" },
			{ "ca" , "Catalan" },
			{ "zh" , "Chinese" }, // Simplified Chinese
			{ "zh-hk" , "Chinese" }, // Traditional Chinese
            { "zh-ro", "Romanized Chinese" },
			{ "hr" , "Croatian" },
			{ "cs" , "Czech" },
			{ "da" , "Danish" },
			{ "nl" , "Dutch" },
			{ "eo" , "Esperanto" },
			{ "et" , "Estonian" },
			{ "fil" , "Filipino" },
			{ "fi" , "Finnish" },
			{ "de" , "German" },
			{ "el" , "Greek" },
			{ "he" , "Hebrew" },
			{ "hi" , "Hindi" },
			{ "hu" , "Hungarian" },
			{ "id" , "Indonesian" },
			{ "it" , "Italian" },
			{ "kk" , "Kazakh" },
			{ "la" , "Latin" },
			{ "lt" , "Lithuanian" },
			{ "ms" , "Malay" },
			{ "mn" , "Mongolian" },
			{ "ne" , "Nepali" },
			{ "no" , "Norwegian" },
			{ "fa" , "Persian" },
			{ "pl" , "Polish" },
			{ "pt" , "Portuguese" },
			{ "pt-br" , "Portuguese" }, // Brazilian Portuguese
			{ "ro" , "Romanian" },
			{ "ru" , "Russian" },
			{ "sr" , "Serbian" },
			{ "sk" , "Slovak" },
			{ "es" , "Spanish" }, // Castilian Spanish
			{ "es-la" , "Spanish" },
			{ "sv" , "Swedish" },
			{ "ta" , "Tamil" },
			{ "th" , "Thai" },
			{ "tr" , "Turkish" },
			{ "uk" , "Ukrainian" },
			{ "vi" , "Vietnamese" }
		};
    }
}