using System.Collections.Generic;

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
        public static readonly string[] ADVANCED_SEARCH_FILTERS = ["Rating==", "Rating>=", "Rating<=", "Status==Finished", "Status==Ongoing", "Status==Cancelled", "Status==Hiatus", "Format==Manga", "Format==Novel", "Demographic==Shounen", "Demographic==Shoujo", "Demographic==Seinen", "Demographic==Josei", "Notes==", "Cost==", "Cost>=", "Cost<=", "Read==", "Read>=", "Read<=", "CurVolumes==", "CurVolumes>=", "CurVolumes<=", "MaxVolumes==", "MaxVolumes>=", "MaxVolumes<=", "Series==Complete", "Series==InComplete", "Favorite==True", "Favorite==False"];
        public const ushort MENU_LENGTH = 400;
		public static readonly string[] VALID_STAFF_ROLES = ["Story & Art", "Story", "Art", "Original Creator", "Character Design", "Cover Illustration", "Illustration", "Mechanical Design", "Original Story", "Original Character Design", "Original Story"];
		public static readonly string[] DEMOGRAPHICS = ["Shounen", "Shoujo", "Seinen", "Josei"];
		public static readonly string[] AVAILABLE_LANGUAGES = ["Romaji", "English", "Japanese", "Arabic", "Azerbaijan", "Bengali", "Bulgarian", "Burmese", "Catalan", "Chinese", "Croatian", "Czech", "Danish", "Dutch", "Esperanto", "Estonian", "Filipino", "Finnish", "French", "German", "Greek", "Hebrew", "Hindi", "Hungarian", "Indonesian", "Italian", "Kazakh", "Korean", "Latin", "Lithuanian", "Malay", "Mongolian", "Nepali", "Norwegian", "Persian", "Polish", "Portuguese", "Romanian", "Russian", "Serbian", "Slovak", "Spanish", "Swedish", "Tamil", "Thai", "Turkish", "Ukrainian", "Vietnamese"];
		public static readonly string[] AVAILABLE_CURRENCY = ["$", "€", "£", "¥", "₣", "₹", "₱", "₩", "₽", "₺", "₫", "฿", "₸", "₼", "₾", "₻", "Rp", "RM", "﷼", "د.إ", "د. ك"];
		public static readonly string[] AVAILABLE_COLLECTION_FILTERS = ["None", "Favorites", "Complete", "Incomplete", "Ongoing", "Finished", "Hiatus", "Cancelled", "Shounen", "Shoujo", "Seinen", "Josei", "Manga", "Novel", "Read", "Unread", "Rating", "Cost", "Query"];

        public enum Site
        {
            AniList,
            MangaDex
        }

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
                { "Portugese" , "pt-BR" }, // Portuguese (Brazil)
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
			{ "ko" , "Korean" },
			{ "fr" , "French" },
			{ "en" , "English" },
			{ "ar" , "Arabic" },
			{ "az" , "Azerbaijani" },
			{ "bn" , "Bengali" },
			{ "bg" , "Bulgarian" },
			{ "my" , "Burmese" },
			{ "ca" , "Catalan" },
			{ "zh" , "Chinese" },
			{ "zh-hk" , "Chinese" },
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
			{ "pt" , "Portugese" },
			{ "pt-br" , "Portugese" },
			{ "ro" , "Romanian" },
			{ "ru" , "Russian" },
			{ "sr" , "Serbian" },
			{ "sk" , "Slovak" },
			{ "es" , "Spanish" },
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