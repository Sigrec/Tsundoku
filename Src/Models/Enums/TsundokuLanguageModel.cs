using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Serialization;

namespace Tsundoku.Models.Enums;

public static class TsundokuLanguageModel
{
    public static readonly FrozenDictionary<string, TsundokuLanguage> TsundokuLanguageStringValueToLanguageMap;
    public static readonly FrozenDictionary<TsundokuLanguage, string> TsundokuLanguageLanguageToStringValueMap;

    public static readonly ImmutableArray<TsundokuLanguage> LANGUAGES = [.. Enum.GetValues<TsundokuLanguage>()];

    public static readonly FrozenDictionary<TsundokuLanguage, int> INDEXED_LANGUAGES =
        LANGUAGES.Select((lang, index) => (lang, index))
            .ToFrozenDictionary(x => x.lang, x => x.index);

    public static readonly FrozenDictionary<string, TsundokuLanguage> ANILIST_LANG_CODES;
    public static readonly FrozenDictionary<TsundokuLanguage, string> CULTURE_LANG_CODES;
    public static readonly FrozenDictionary<string, TsundokuLanguage> MANGADEX_LANG_CODES;

    static TsundokuLanguageModel()
    {
        Dictionary<string, TsundokuLanguage> stringToLang = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<TsundokuLanguage, string> langToString = [];
        Dictionary<string, TsundokuLanguage> aniList = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<TsundokuLanguage, string> culture = [];
        Dictionary<string, TsundokuLanguage> mangaDex = new(StringComparer.OrdinalIgnoreCase);

        foreach (TsundokuLanguage lang in Enum.GetValues<TsundokuLanguage>())
        {
            string name = Enum.GetName(lang)!;
            FieldInfo? field = typeof(TsundokuLanguage).GetField(name);

            // EnumMember → display string mapping
            string stringValue = field?.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name;
            stringToLang[stringValue] = lang;
            langToString[lang] = stringValue;

            // LanguageCode attribute → culture, AniList, MangaDex mappings
            LanguageCodeAttribute? code = field?.GetCustomAttribute<LanguageCodeAttribute>();
            if (code is null) continue;

            culture[lang] = code.Culture;

            if (!string.IsNullOrEmpty(code.AniList))
            {
                foreach (string aniListCode in code.AniList.Split(','))
                {
                    aniList[aniListCode.Trim()] = lang;
                }
            }

            if (!string.IsNullOrEmpty(code.MangaDex))
            {
                foreach (string mdCode in code.MangaDex.Split(','))
                {
                    mangaDex[mdCode.Trim()] = lang;
                }
            }
        }

        TsundokuLanguageStringValueToLanguageMap = stringToLang.ToFrozenDictionary();
        TsundokuLanguageLanguageToStringValueMap = langToString.ToFrozenDictionary();
        ANILIST_LANG_CODES = aniList.ToFrozenDictionary();
        CULTURE_LANG_CODES = culture.ToFrozenDictionary();
        MANGADEX_LANG_CODES = mangaDex.ToFrozenDictionary();
    }

    public enum TsundokuLanguage
    {
        [EnumMember(Value = "Romaji")]
        [LanguageCode("ex-US", MangaDex = "ja-ro,ko-ro,zh-ro")]
        Romaji,

        [EnumMember(Value = "English")]
        [LanguageCode("en-US", AniList = "EN", MangaDex = "en")]
        English,

        [EnumMember(Value = "Japanese")]
        [LanguageCode("ja-JP", AniList = "JP", MangaDex = "ja")]
        Japanese,

        [EnumMember(Value = "Albanian")]
        [LanguageCode("sq-AL", MangaDex = "sq")]
        Albanian,

        [EnumMember(Value = "Arabic")]
        [LanguageCode("ar-SA", MangaDex = "ar")]
        Arabic,

        [EnumMember(Value = "Azerbaijan")]
        [LanguageCode("az-Latn-AZ", MangaDex = "az")]
        Azerbaijan,

        [EnumMember(Value = "Belarusian")]
        [LanguageCode("be-BY", MangaDex = "be")]
        Belarusian,

        [EnumMember(Value = "Bengali")]
        [LanguageCode("bn-BD", MangaDex = "bn")]
        Bengali,

        [EnumMember(Value = "Bosnian")]
        [LanguageCode("bs-Latn-BA", MangaDex = "bs")]
        Bosnian,

        [EnumMember(Value = "Bulgarian")]
        [LanguageCode("bg-BG", MangaDex = "bg")]
        Bulgarian,

        [EnumMember(Value = "Burmese")]
        [LanguageCode("my-MM", MangaDex = "my")]
        Burmese,

        [EnumMember(Value = "Catalan")]
        [LanguageCode("ca-ES", MangaDex = "ca")]
        Catalan,

        [EnumMember(Value = "Chinese")]
        [LanguageCode("zh-CN", AniList = "CN,TW", MangaDex = "zh,zh-hk")]
        Chinese,

        [EnumMember(Value = "Croatian")]
        [LanguageCode("hr-HR", MangaDex = "hr")]
        Croatian,

        [EnumMember(Value = "Czech")]
        [LanguageCode("cs-CZ", MangaDex = "cs")]
        Czech,

        [EnumMember(Value = "Danish")]
        [LanguageCode("da-DK", MangaDex = "da")]
        Danish,

        [EnumMember(Value = "Dutch")]
        [LanguageCode("nl-NL", MangaDex = "nl")]
        Dutch,

        [EnumMember(Value = "Esperanto")]
        [LanguageCode("eo-001", MangaDex = "eo")]
        Esperanto,

        [EnumMember(Value = "Estonian")]
        [LanguageCode("et-EE", MangaDex = "et")]
        Estonian,

        [EnumMember(Value = "Filipino")]
        [LanguageCode("fil-PH", MangaDex = "fil")]
        Filipino,

        [EnumMember(Value = "Finnish")]
        [LanguageCode("fi-FI", MangaDex = "fi")]
        Finnish,

        [EnumMember(Value = "French")]
        [LanguageCode("fr-FR", AniList = "FR", MangaDex = "fr")]
        French,

        [EnumMember(Value = "Galician")]
        [LanguageCode("gl-ES", MangaDex = "gl")]
        Galician,

        [EnumMember(Value = "German")]
        [LanguageCode("de-DE", MangaDex = "de")]
        German,

        [EnumMember(Value = "Greek")]
        [LanguageCode("el-GR", MangaDex = "el")]
        Greek,

        [EnumMember(Value = "Gujarati")]
        [LanguageCode("gu-IN", MangaDex = "gu")]
        Gujarati,

        [EnumMember(Value = "Hebrew")]
        [LanguageCode("he-IL", MangaDex = "he")]
        Hebrew,

        [EnumMember(Value = "Hindi")]
        [LanguageCode("hi-IN", MangaDex = "hi")]
        Hindi,

        [EnumMember(Value = "Hungarian")]
        [LanguageCode("hu-HU", MangaDex = "hu")]
        Hungarian,

        [EnumMember(Value = "Icelandic")]
        [LanguageCode("is-IS", MangaDex = "is")]
        Icelandic,

        [EnumMember(Value = "Indonesian")]
        [LanguageCode("id-ID", MangaDex = "id")]
        Indonesian,

        [EnumMember(Value = "Italian")]
        [LanguageCode("it-IT", MangaDex = "it")]
        Italian,

        [EnumMember(Value = "Kannada")]
        [LanguageCode("kn-IN", MangaDex = "kn")]
        Kannada,

        [EnumMember(Value = "Kazakh")]
        [LanguageCode("kk-KZ", MangaDex = "kk")]
        Kazakh,

        [EnumMember(Value = "Korean")]
        [LanguageCode("ko-KR", AniList = "KR", MangaDex = "ko")]
        Korean,

        [EnumMember(Value = "Latin")]
        [LanguageCode("la-001", MangaDex = "la")]
        Latin,

        [EnumMember(Value = "Latvian")]
        [LanguageCode("lv-LV", MangaDex = "lv")]
        Latvian,

        [EnumMember(Value = "Lithuanian")]
        [LanguageCode("lt-LT", MangaDex = "lt")]
        Lithuanian,

        [EnumMember(Value = "Malay")]
        [LanguageCode("ms-MY", MangaDex = "ms")]
        Malay,

        [EnumMember(Value = "Malayalam")]
        [LanguageCode("ml-IN", MangaDex = "ml")]
        Malayalam,

        [EnumMember(Value = "Macedonian")]
        [LanguageCode("mk-MK", MangaDex = "mk")]
        Macedonian,

        [EnumMember(Value = "Marathi")]
        [LanguageCode("mr-IN", MangaDex = "mr")]
        Marathi,

        [EnumMember(Value = "Mongolian")]
        [LanguageCode("mn-MN", MangaDex = "mn")]
        Mongolian,

        [EnumMember(Value = "Nepali")]
        [LanguageCode("ne-NP", MangaDex = "ne")]
        Nepali,

        [EnumMember(Value = "Norwegian")]
        [LanguageCode("nb-NO", MangaDex = "no")]
        Norwegian,

        [EnumMember(Value = "Persian")]
        [LanguageCode("fa-IR", MangaDex = "fa")]
        Persian,

        [EnumMember(Value = "Polish")]
        [LanguageCode("pl-PL", MangaDex = "pl")]
        Polish,

        [EnumMember(Value = "Portuguese")]
        [LanguageCode("pt-BR", MangaDex = "pt,pt-br")]
        Portuguese,

        [EnumMember(Value = "Punjabi")]
        [LanguageCode("pa-IN", MangaDex = "pa")]
        Punjabi,

        [EnumMember(Value = "Romanian")]
        [LanguageCode("ro-RO", MangaDex = "ro")]
        Romanian,

        [EnumMember(Value = "Russian")]
        [LanguageCode("ru-RU", MangaDex = "ru")]
        Russian,

        [EnumMember(Value = "Serbian")]
        [LanguageCode("sr-Latn-RS", MangaDex = "sr")]
        Serbian,

        [EnumMember(Value = "Slovak")]
        [LanguageCode("sk-SK", MangaDex = "sk")]
        Slovak,

        [EnumMember(Value = "Slovenian")]
        [LanguageCode("sl-SI", MangaDex = "sl")]
        Slovenian,

        [EnumMember(Value = "Spanish")]
        [LanguageCode("es-ES", MangaDex = "es,es-la")]
        Spanish,

        [EnumMember(Value = "Swedish")]
        [LanguageCode("sv-SE", MangaDex = "sv")]
        Swedish,

        [EnumMember(Value = "Tamil")]
        [LanguageCode("ta-IN", MangaDex = "ta")]
        Tamil,

        [EnumMember(Value = "Telugu")]
        [LanguageCode("te-IN", MangaDex = "te")]
        Telugu,

        [EnumMember(Value = "Thai")]
        [LanguageCode("th-TH", MangaDex = "th")]
        Thai,

        [EnumMember(Value = "Turkish")]
        [LanguageCode("tr-TR", MangaDex = "tr")]
        Turkish,

        [EnumMember(Value = "Ukrainian")]
        [LanguageCode("uk-UA", MangaDex = "uk")]
        Ukrainian,

        [EnumMember(Value = "Urdu")]
        [LanguageCode("ur-PK", MangaDex = "ur")]
        Urdu,

        [EnumMember(Value = "Vietnamese")]
        [LanguageCode("vi-VN", MangaDex = "vi")]
        Vietnamese
    }
}
