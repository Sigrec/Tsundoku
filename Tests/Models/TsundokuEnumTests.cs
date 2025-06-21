using NLog.Filters;
using System.Globalization;
using static Tsundoku.Models.TsundokuFilterModel;
using static Tsundoku.Models.TsundokuLanguageModel;

namespace Tsundoku.Tests.Models;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class TsundokuEnumTests
{
    [Test]
    public void All_EnumMembers_HaveStringMappings()
    {
        foreach (TsundokuLanguage lang in LANGUAGES)
        {
            Assert.That(TsundokuLanguageLanguageToStringValueMap.ContainsKey(lang), Is.True, $"Missing string mapping for {lang}");
            string stringValue = TsundokuLanguageLanguageToStringValueMap[lang];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(TsundokuLanguageStringValueToLanguageMap.ContainsKey(stringValue), Is.True, $"Missing reverse mapping for string value '{stringValue}'");
                Assert.That(TsundokuLanguageStringValueToLanguageMap[stringValue], Is.EqualTo(lang), $"Reverse mapping mismatch for {lang}");
            }
        }
    }

    [Test]
    public void All_LanguageCodes_Have_ValidEnum()
    {
        foreach (KeyValuePair<string, TsundokuLanguage> entry in MANGADEX_LANG_CODES)
        {
            Assert.That(Enum.IsDefined(entry.Value), Is.True, $"Invalid enum value in MangaDex map: {entry.Key}");
        }

        foreach (KeyValuePair<string, TsundokuLanguage> entry in ANILIST_LANG_CODES)
        {
            Assert.That(Enum.IsDefined(entry.Value), Is.True, $"Invalid enum value in AniList map: {entry.Key}");
        }
    }

    [Test]
    public void Culture_Lang_Codes_ResolveToValidCultureInfo()
    {
        foreach (KeyValuePair<TsundokuLanguage, string> entry in CULTURE_LANG_CODES)
        {
            Assert.That(() => new CultureInfo(entry.Value), Throws.Nothing, $"Culture code '{entry.Value}' is invalid for language {entry.Key}");
        }
    }

    [Test]
    public void Indexed_Languages_HaveUniqueAndAccurateIndexes()
    {
        HashSet<int> seenIndexes = new();

        foreach ((TsundokuLanguage lang, int index) in INDEXED_LANGUAGES)
        {
            Assert.That(index, Is.GreaterThanOrEqualTo(0));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(index, Is.LessThan(LANGUAGES.Count));
                Assert.That(seenIndexes, Does.Not.Contain(index), $"Duplicate index {index} for language {lang}");
            }
            seenIndexes.Add(index);
            Assert.That(LANGUAGES[index], Is.EqualTo(lang), $"Index mismatch: LANGUAGES[{index}] != {lang}");
        }
    }

    [Test]
    public void TsundokuLanguageStringValueToLanguageMap_UnknownKey_ThrowsKeyNotFound()
    {
        Assert.That(() => _ = TsundokuLanguageStringValueToLanguageMap["NonexistentLang"], Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void TsundokuLanguageLanguageToStringValueMap_InvalidEnum_ThrowsKeyNotFound()
    {
        const TsundokuLanguage invalidEnum = (TsundokuLanguage)(-999);
        Assert.That(() => _ = TsundokuLanguageLanguageToStringValueMap[invalidEnum], Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void CultureLangCodes_MissingEnum_ThrowsKeyNotFound()
    {
        const TsundokuLanguage missingLang = (TsundokuLanguage)(-500);
        Assert.That(() => _ = CULTURE_LANG_CODES[missingLang], Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void MangaDexLangCodes_CaseInsensitive_Match()
    {
        bool hasMatch = MANGADEX_LANG_CODES.TryGetValue("EN", out TsundokuLanguage lang);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(hasMatch, Is.True);
            Assert.That(lang, Is.EqualTo(TsundokuLanguage.English));
        }
    }

    [Test]
    public void IndexedLanguages_UnknownEnum_ThrowsKeyNotFound()
    {
        const TsundokuLanguage unknownLang = (TsundokuLanguage)(9999);
        Assert.That(() => _ = INDEXED_LANGUAGES[unknownLang], Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void All_TsundokuFilterEnumMembers_AreIncludedInFrozenDictionary()
    {
        foreach (TsundokuFilter filter in Enum.GetValues<TsundokuFilter>())
        {
            Assert.That(FILTERS.ContainsKey(filter), Is.True, $"Missing frozen dictionary entry for filter: {filter}");
        }
    }

    [Test]
    public void FilterDictionary_Indexes_AreUniqueAndSequential()
    {
        HashSet<int> seenIndexes = new();

        foreach ((TsundokuFilter filter, int index) in FILTERS)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(index, Is.GreaterThanOrEqualTo(0));
                Assert.That(seenIndexes, Does.Not.Contain(index), $"Duplicate index {index} for filter {filter}");
            }
            seenIndexes.Add(index);
        }
    }

    [Test]
    public void FilterDictionary_InvalidKey_Throws()
    {
        const TsundokuFilter invalid = (TsundokuFilter)(-100);
        Assert.That(() => _ = FILTERS[invalid], Throws.TypeOf<KeyNotFoundException>());
    }
}