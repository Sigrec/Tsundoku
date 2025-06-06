using System.Reflection;
using System.Runtime.Serialization;

namespace Tsundoku.Models
{
    public class TsundokuLanguageModel
    {
        public static readonly IReadOnlyDictionary<string, TsundokuLanguage> TsundokuLanguageStringValueToLanguageMap;
        public static readonly IReadOnlyDictionary<TsundokuLanguage, string> TsundokuLanguageLanguageToStringValueMap;

        static TsundokuLanguageModel()
        {
            Dictionary<string, TsundokuLanguage> stringToLangBuilder = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<TsundokuLanguage, string> langToStringBuilder = [];

            foreach (TsundokuLanguage langEnum in Enum.GetValues<TsundokuLanguage>())
            {
                // Get the FieldInfo for the current enum member
                string name = Enum.GetName(typeof(TsundokuLanguage), langEnum)!;
                FieldInfo? field = typeof(TsundokuLanguage).GetField(name);

                // Get the EnumMemberAttribute value, or fallback to the enum's name
                string stringValue = field?.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name;

                // Populate both dictionaries
                stringToLangBuilder[stringValue] = langEnum;
                langToStringBuilder[langEnum] = stringValue;
            }

            TsundokuLanguageStringValueToLanguageMap = stringToLangBuilder;
            TsundokuLanguageLanguageToStringValueMap = langToStringBuilder;
        }
        
        public static readonly IReadOnlyList<TsundokuLanguage> LANGUAGES = Enum.GetValues<TsundokuLanguage>();
        public static readonly IReadOnlyDictionary<TsundokuLanguage, int> INDEXED_LANGUAGES = 
            LANGUAGES.Select((lang, index) => (lang, index))
                .ToDictionary(x => x.lang, x => x.index);
        public enum TsundokuLanguage
        {
            [EnumMember(Value = "Romaji")] Romaji,
            [EnumMember(Value = "English")] English,
            [EnumMember(Value = "Japanese")] Japanese,
            [EnumMember(Value = "Arabic")] Arabic,
            [EnumMember(Value = "Azerbaijan")] Azerbaijan,
            [EnumMember(Value = "Bengali")] Bengali,
            [EnumMember(Value = "Bulgarian")] Bulgarian,
            [EnumMember(Value = "Burmese")] Burmese,
            [EnumMember(Value = "Catalan")] Catalan,
            [EnumMember(Value = "Chinese")] Chinese,
            [EnumMember(Value = "Croatian")] Croatian,
            [EnumMember(Value = "Czech")] Czech,
            [EnumMember(Value = "Danish")] Danish,
            [EnumMember(Value = "Dutch")] Dutch,
            [EnumMember(Value = "Esperanto")] Esperanto,
            [EnumMember(Value = "Estonian")] Estonian,
            [EnumMember(Value = "Filipino")] Filipino,
            [EnumMember(Value = "Finnish")] Finnish,
            [EnumMember(Value = "French")] French,
            [EnumMember(Value = "German")] German,
            [EnumMember(Value = "Greek")] Greek,
            [EnumMember(Value = "Hebrew")] Hebrew,
            [EnumMember(Value = "Hindi")] Hindi,
            [EnumMember(Value = "Hungarian")] Hungarian,
            [EnumMember(Value = "Indonesian")] Indonesian,
            [EnumMember(Value = "Italian")] Italian,
            [EnumMember(Value = "Kazakh")] Kazakh,
            [EnumMember(Value = "Korean")] Korean,
            [EnumMember(Value = "Latin")] Latin,
            [EnumMember(Value = "Lithuanian")] Lithuanian,
            [EnumMember(Value = "Malay")] Malay,
            [EnumMember(Value = "Mongolian")] Mongolian,
            [EnumMember(Value = "Nepali")] Nepali,
            [EnumMember(Value = "Norwegian")] Norwegian,
            [EnumMember(Value = "Persian")] Persian,
            [EnumMember(Value = "Polish")] Polish,
            [EnumMember(Value = "Portuguese")] Portuguese,
            [EnumMember(Value = "Romanian")] Romanian,
            [EnumMember(Value = "Russian")] Russian,
            [EnumMember(Value = "Serbian")] Serbian,
            [EnumMember(Value = "Slovak")] Slovak,
            [EnumMember(Value = "Spanish")] Spanish,
            [EnumMember(Value = "Swedish")] Swedish,
            [EnumMember(Value = "Tamil")] Tamil,
            [EnumMember(Value = "Thai")] Thai,
            [EnumMember(Value = "Turkish")] Turkish,
            [EnumMember(Value = "Ukrainian")] Ukrainian,
            [EnumMember(Value = "Vietnamese")] Vietnamese
        }

        public static readonly IReadOnlyDictionary<string, TsundokuLanguage> ANILIST_LANG_CODES = new Dictionary<string, TsundokuLanguage>(StringComparer.OrdinalIgnoreCase)
        {
            { "JP", TsundokuLanguage.Japanese },
            { "FR", TsundokuLanguage.French },
            { "EN", TsundokuLanguage.English },
            { "KR", TsundokuLanguage.Korean },
            { "CN", TsundokuLanguage.Chinese },
            { "TW", TsundokuLanguage.Chinese }
        };

        public static readonly IReadOnlyDictionary<TsundokuLanguage, string> CULTURE_LANG_CODES = new Dictionary<TsundokuLanguage, string>
        {
            { TsundokuLanguage.Japanese, "ja-JP" },
            { TsundokuLanguage.Korean, "ko-kr" },
            { TsundokuLanguage.Arabic, "ar-SA" },
            { TsundokuLanguage.Azerbaijan, "az-Latn-AZ" },
            { TsundokuLanguage.Bengali, "bn-BD" },
            { TsundokuLanguage.Bulgarian, "bg-BG" },
            { TsundokuLanguage.Burmese, "my-MM" },
            { TsundokuLanguage.Catalan, "ca-ES" },
            { TsundokuLanguage.Chinese, "zh-CN" },
            { TsundokuLanguage.Croatian, "hr-HR" },
            { TsundokuLanguage.Czech, "cs-CZ" },
            { TsundokuLanguage.Danish, "da-DK" },
            { TsundokuLanguage.Esperanto, "eo-001" },
            { TsundokuLanguage.Estonian, "et-EE" },
            { TsundokuLanguage.Filipino, "fil-PH" },
            { TsundokuLanguage.Finnish, "fi-FI" },
            { TsundokuLanguage.French, "fr-FR" },
            { TsundokuLanguage.German, "de-DE" },
            { TsundokuLanguage.Greek, "el-GR" },
            { TsundokuLanguage.Hebrew, "he-IL" },
            { TsundokuLanguage.Hindi, "hi-IN" },
            { TsundokuLanguage.Hungarian, "hu-HU" },
            { TsundokuLanguage.Indonesian, "id-ID" },
            { TsundokuLanguage.Italian, "it-IT" },
            { TsundokuLanguage.Kazakh, "kk-KZ" },
            { TsundokuLanguage.Latin, "la-001" },
            { TsundokuLanguage.Lithuanian, "lt-LT" },
            { TsundokuLanguage.Malay, "ms-MY" },
            { TsundokuLanguage.Mongolian, "mn-MN" },
            { TsundokuLanguage.Nepali, "ne-NP" },
            { TsundokuLanguage.Norwegian, "nb-NO" },
            { TsundokuLanguage.Persian, "fa-IR" },
            { TsundokuLanguage.Polish, "pl-PL" },
            { TsundokuLanguage.Portuguese, "pt-BR" },
            { TsundokuLanguage.Romanian, "ro-RO" },
            { TsundokuLanguage.Russian, "ru-RU" },
            { TsundokuLanguage.Serbian, "sr-Latn-RS" },
            { TsundokuLanguage.Slovak, "sk-SK" },
            { TsundokuLanguage.Spanish, "es-ES" },
            { TsundokuLanguage.Swedish, "sv-SE" },
            { TsundokuLanguage.Tamil, "ta-IN" },
            { TsundokuLanguage.Thai, "th-TH" },
            { TsundokuLanguage.Turkish, "tr-TR" },
            { TsundokuLanguage.Ukrainian, "uk-UA" },
            { TsundokuLanguage.Vietnamese, "vi-VN" },
            { TsundokuLanguage.English, "en-US" },
            { TsundokuLanguage.Romaji, "en-US" }
        };

        public static readonly IReadOnlyDictionary<string, TsundokuLanguage> MANGADEX_LANG_CODES = new Dictionary<string, TsundokuLanguage>(StringComparer.OrdinalIgnoreCase)
        {
            { "ja", TsundokuLanguage.Japanese },
            { "ja-ro", TsundokuLanguage.Romaji },
            { "ko" , TsundokuLanguage.Korean },
            // { "ko-ro", TsundokuLanguage.RomanizedKorean}, // Omitted: "Romanized Korean" not in enum
            { "fr" , TsundokuLanguage.French },
            { "en" , TsundokuLanguage.English },
            { "ar" , TsundokuLanguage.Arabic },
            { "az" , TsundokuLanguage.Azerbaijan },
            { "bn" , TsundokuLanguage.Bengali },
            { "bg" , TsundokuLanguage.Bulgarian },
            { "my" , TsundokuLanguage.Burmese },
            { "ca" , TsundokuLanguage.Catalan },
            { "zh" , TsundokuLanguage.Chinese },
            { "zh-hk" , TsundokuLanguage.Chinese },
            // { "zh-ro", TsundokuLanguage.RomanizedChinese }, // Omitted: "Romanized Chinese" not in enum
            { "hr" , TsundokuLanguage.Croatian },
            { "cs" , TsundokuLanguage.Czech },
            { "da" , TsundokuLanguage.Danish },
            { "nl" , TsundokuLanguage.Dutch },
            { "eo" , TsundokuLanguage.Esperanto },
            { "et" , TsundokuLanguage.Estonian },
            { "fil" , TsundokuLanguage.Filipino },
            { "fi" , TsundokuLanguage.Finnish },
            { "de" , TsundokuLanguage.German },
            { "el" , TsundokuLanguage.Greek },
            { "he" , TsundokuLanguage.Hebrew },
            { "hi" , TsundokuLanguage.Hindi },
            { "hu" , TsundokuLanguage.Hungarian },
            { "id" , TsundokuLanguage.Indonesian },
            { "it" , TsundokuLanguage.Italian },
            { "kk" , TsundokuLanguage.Kazakh },
            { "la" , TsundokuLanguage.Latin },
            { "lt" , TsundokuLanguage.Lithuanian },
            { "ms" , TsundokuLanguage.Malay },
            { "mn" , TsundokuLanguage.Mongolian },
            { "ne" , TsundokuLanguage.Nepali },
            { "no" , TsundokuLanguage.Norwegian },
            { "fa" , TsundokuLanguage.Persian },
            { "pl" , TsundokuLanguage.Polish },
            { "pt" , TsundokuLanguage.Portuguese },
            { "pt-br" , TsundokuLanguage.Portuguese },
            { "ro" , TsundokuLanguage.Romanian },
            { "ru" , TsundokuLanguage.Russian },
            { "sr" , TsundokuLanguage.Serbian },
            { "sk" , TsundokuLanguage.Slovak },
            { "es" , TsundokuLanguage.Spanish },
            { "es-la" , TsundokuLanguage.Spanish },
            { "sv" , TsundokuLanguage.Swedish },
            { "ta" , TsundokuLanguage.Tamil },
            { "th" , TsundokuLanguage.Thai },
            { "tr" , TsundokuLanguage.Turkish },
            { "uk" , TsundokuLanguage.Ukrainian },
            { "vi" , TsundokuLanguage.Vietnamese }
        };
    }
}