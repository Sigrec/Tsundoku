using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.Serialization;

namespace Tsundoku.Models;

public static class TsundokuLanguageModel
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
    public static readonly FrozenDictionary<TsundokuLanguage, int> INDEXED_LANGUAGES = 
        LANGUAGES.Select((lang, index) => (lang, index))
            .ToFrozenDictionary(x => x.lang, x => x.index);

    public enum TsundokuLanguage
    {
        [EnumMember(Value = "Romaji")] Romaji,
        [EnumMember(Value = "English")] English,
        [EnumMember(Value = "Japanese")] Japanese,
        [EnumMember(Value = "Albanian")] Albanian,
        [EnumMember(Value = "Arabic")] Arabic,
        [EnumMember(Value = "Azerbaijan")] Azerbaijan,
        [EnumMember(Value = "Belarusian")] Belarusian,
        [EnumMember(Value = "Bengali")] Bengali,
        [EnumMember(Value = "Bosnian")] Bosnian,
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
        [EnumMember(Value = "Galician")] Galician,
        [EnumMember(Value = "German")] German,
        [EnumMember(Value = "Greek")] Greek,
        [EnumMember(Value = "Gujarati")] Gujarati,
        [EnumMember(Value = "Hebrew")] Hebrew,
        [EnumMember(Value = "Hindi")] Hindi,
        [EnumMember(Value = "Hungarian")] Hungarian,
        [EnumMember(Value = "Icelandic")] Icelandic,
        [EnumMember(Value = "Indonesian")] Indonesian,
        [EnumMember(Value = "Italian")] Italian,
        [EnumMember(Value = "Kannada")] Kannada,
        [EnumMember(Value = "Kazakh")] Kazakh,
        [EnumMember(Value = "Korean")] Korean,
        [EnumMember(Value = "Latin")] Latin,
        [EnumMember(Value = "Latvian")] Latvian,
        [EnumMember(Value = "Lithuanian")] Lithuanian,
        [EnumMember(Value = "Malay")] Malay,
        [EnumMember(Value = "Malayalam")] Malayalam,
        [EnumMember(Value = "Macedonian")] Macedonian,
        [EnumMember(Value = "Marathi")] Marathi,
        [EnumMember(Value = "Mongolian")] Mongolian,
        [EnumMember(Value = "Nepali")] Nepali,
        [EnumMember(Value = "Norwegian")] Norwegian,
        [EnumMember(Value = "Persian")] Persian,
        [EnumMember(Value = "Polish")] Polish,
        [EnumMember(Value = "Portuguese")] Portuguese,
        [EnumMember(Value = "Punjabi")] Punjabi,
        [EnumMember(Value = "Romanian")] Romanian,
        [EnumMember(Value = "Russian")] Russian,
        [EnumMember(Value = "Serbian")] Serbian,
        [EnumMember(Value = "Slovak")] Slovak,
        [EnumMember(Value = "Slovenian")] Slovenian,
        [EnumMember(Value = "Spanish")] Spanish,
        [EnumMember(Value = "Swedish")] Swedish,
        [EnumMember(Value = "Tamil")] Tamil,
        [EnumMember(Value = "Telugu")] Telugu,
        [EnumMember(Value = "Thai")] Thai,
        [EnumMember(Value = "Turkish")] Turkish,
        [EnumMember(Value = "Ukrainian")] Ukrainian,
        [EnumMember(Value = "Urdu")] Urdu,
        [EnumMember(Value = "Vietnamese")] Vietnamese
    }

    public static readonly FrozenDictionary<string, TsundokuLanguage> ANILIST_LANG_CODES = new Dictionary<string, TsundokuLanguage>
    {
        { "JP", TsundokuLanguage.Japanese },
        { "FR", TsundokuLanguage.French },
        { "EN", TsundokuLanguage.English },
        { "KR", TsundokuLanguage.Korean },
        { "CN", TsundokuLanguage.Chinese },
        { "TW", TsundokuLanguage.Chinese }
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static readonly FrozenDictionary<TsundokuLanguage, string> CULTURE_LANG_CODES = new Dictionary<TsundokuLanguage, string>
    {
        { TsundokuLanguage.Japanese, "ja-JP" },
        { TsundokuLanguage.Korean, "ko-KR" },
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
        { TsundokuLanguage.Dutch, "nl-NL" },
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
        { TsundokuLanguage.Icelandic, "is-IS" },
        { TsundokuLanguage.Indonesian, "id-ID" },
        { TsundokuLanguage.Italian, "it-IT" },
        { TsundokuLanguage.Kannada, "kn-IN" },
        { TsundokuLanguage.Kazakh, "kk-KZ" },
        { TsundokuLanguage.Latin, "la-001" },
        { TsundokuLanguage.Latvian, "lv-LV" },
        { TsundokuLanguage.Lithuanian, "lt-LT" },
        { TsundokuLanguage.Malay, "ms-MY" },
        { TsundokuLanguage.Malayalam, "ml-IN" },
        { TsundokuLanguage.Marathi, "mr-IN" },
        { TsundokuLanguage.Macedonian, "mk-MK" },
        { TsundokuLanguage.Mongolian, "mn-MN" },
        { TsundokuLanguage.Nepali, "ne-NP" },
        { TsundokuLanguage.Norwegian, "nb-NO" },
        { TsundokuLanguage.Persian, "fa-IR" },
        { TsundokuLanguage.Polish, "pl-PL" },
        { TsundokuLanguage.Portuguese, "pt-BR" },
        { TsundokuLanguage.Punjabi, "pa-IN" },
        { TsundokuLanguage.Romanian, "ro-RO" },
        { TsundokuLanguage.Russian, "ru-RU" },
        { TsundokuLanguage.Serbian, "sr-Latn-RS" },
        { TsundokuLanguage.Slovak, "sk-SK" },
        { TsundokuLanguage.Slovenian, "sl-SI" },
        { TsundokuLanguage.Spanish, "es-ES" },
        { TsundokuLanguage.Swedish, "sv-SE" },
        { TsundokuLanguage.Tamil, "ta-IN" },
        { TsundokuLanguage.Telugu, "te-IN" },
        { TsundokuLanguage.Thai, "th-TH" },
        { TsundokuLanguage.Turkish, "tr-TR" },
        { TsundokuLanguage.Ukrainian, "uk-UA" },
        { TsundokuLanguage.Urdu, "ur-PK" },
        { TsundokuLanguage.Vietnamese, "vi-VN" },
        { TsundokuLanguage.Albanian, "sq-AL" },
        { TsundokuLanguage.Belarusian, "be-BY" },
        { TsundokuLanguage.Bosnian, "bs-Latn-BA" },
        { TsundokuLanguage.Galician, "gl-ES" },
        { TsundokuLanguage.Gujarati, "gu-IN" },
        { TsundokuLanguage.English, "en-US" },
        { TsundokuLanguage.Romaji, "ex-US" } // Placeholder for Romanized forms
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<string, TsundokuLanguage> MANGADEX_LANG_CODES = new Dictionary<string, TsundokuLanguage>
    {
        { "ja", TsundokuLanguage.Japanese },
        { "ja-ro", TsundokuLanguage.Romaji },
        { "ko", TsundokuLanguage.Korean },
        { "ko-ro", TsundokuLanguage.Romaji },
        { "zh", TsundokuLanguage.Chinese },
        { "zh-hk", TsundokuLanguage.Chinese },
        { "zh-ro", TsundokuLanguage.Romaji },
        { "en", TsundokuLanguage.English },
        { "fr", TsundokuLanguage.French },
        { "pt", TsundokuLanguage.Portuguese },
        { "pt-br", TsundokuLanguage.Portuguese },
        { "es", TsundokuLanguage.Spanish },
        { "es-la", TsundokuLanguage.Spanish },
        { "de", TsundokuLanguage.German },
        { "it", TsundokuLanguage.Italian },
        { "ru", TsundokuLanguage.Russian },
        { "ar", TsundokuLanguage.Arabic },
        { "az", TsundokuLanguage.Azerbaijan },
        { "bn", TsundokuLanguage.Bengali },
        { "bg", TsundokuLanguage.Bulgarian },
        { "my", TsundokuLanguage.Burmese },
        { "ca", TsundokuLanguage.Catalan },
        { "hr", TsundokuLanguage.Croatian },
        { "cs", TsundokuLanguage.Czech },
        { "da", TsundokuLanguage.Danish },
        { "nl", TsundokuLanguage.Dutch },
        { "eo", TsundokuLanguage.Esperanto },
        { "et", TsundokuLanguage.Estonian },
        { "fil", TsundokuLanguage.Filipino },
        { "fi", TsundokuLanguage.Finnish },
        { "el", TsundokuLanguage.Greek },
        { "he", TsundokuLanguage.Hebrew },
        { "hi", TsundokuLanguage.Hindi },
        { "hu", TsundokuLanguage.Hungarian },
        { "id", TsundokuLanguage.Indonesian },
        { "is", TsundokuLanguage.Icelandic },
        { "kk", TsundokuLanguage.Kazakh },
        { "la", TsundokuLanguage.Latin },
        { "lt", TsundokuLanguage.Lithuanian },
        { "lv", TsundokuLanguage.Latvian },
        { "ml", TsundokuLanguage.Malayalam },
        { "mr", TsundokuLanguage.Marathi },
        { "ms", TsundokuLanguage.Malay },
        { "mn", TsundokuLanguage.Mongolian },
        { "ne", TsundokuLanguage.Nepali },
        { "no", TsundokuLanguage.Norwegian },
        { "pa", TsundokuLanguage.Punjabi },
        { "fa", TsundokuLanguage.Persian },
        { "pl", TsundokuLanguage.Polish },
        { "ro", TsundokuLanguage.Romanian },
        { "sr", TsundokuLanguage.Serbian },
        { "sk", TsundokuLanguage.Slovak },
        { "sl", TsundokuLanguage.Slovenian },
        { "sq", TsundokuLanguage.Albanian },
        { "sv", TsundokuLanguage.Swedish },
        { "ta", TsundokuLanguage.Tamil },
        { "te", TsundokuLanguage.Telugu },
        { "th", TsundokuLanguage.Thai },
        { "tr", TsundokuLanguage.Turkish },
        { "uk", TsundokuLanguage.Ukrainian },
        { "ur", TsundokuLanguage.Urdu },
        { "vi", TsundokuLanguage.Vietnamese },
        { "be", TsundokuLanguage.Belarusian },
        { "gl", TsundokuLanguage.Galician },
        { "gu", TsundokuLanguage.Gujarati },
        { "kn", TsundokuLanguage.Kannada },
        { "mk", TsundokuLanguage.Macedonian },
        { "bs", TsundokuLanguage.Bosnian }
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}