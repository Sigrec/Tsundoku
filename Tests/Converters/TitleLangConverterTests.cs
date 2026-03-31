using System.Globalization;
using Tsundoku.Converters;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Tests.Converters;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class TitleLangConverterTests
{
    private static readonly TitleLangConverter Converter = TitleLangConverter.Instance;

    private static Dictionary<TsundokuLanguage, string> CreateTitles(
        string romaji = "Romaji Title",
        string? english = null,
        string? japanese = null)
    {
        Dictionary<TsundokuLanguage, string> titles = new()
        {
            { TsundokuLanguage.Romaji, romaji }
        };

        if (english is not null)
        {
            titles[TsundokuLanguage.English] = english;
        }

        if (japanese is not null)
        {
            titles[TsundokuLanguage.Japanese] = japanese;
        }

        return titles;
    }

    [Test]
    public void Convert_NullValues_ReturnsError()
    {
        object? result = Converter.Convert(null!, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("ERROR"));
    }

    [Test]
    public void Convert_EmptyValues_ReturnsError()
    {
        List<object?> values = [];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("ERROR"));
    }

    [Test]
    public void Convert_SingleValue_ReturnsError()
    {
        List<object?> values = [CreateTitles()];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("ERROR"));
    }

    [Test]
    public void Convert_NonDictionaryFirstValue_ReturnsError()
    {
        List<object?> values = ["not a dict", TsundokuLanguage.Romaji];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("ERROR"));
    }

    [Test]
    public void Convert_RomajiLanguage_ReturnsRomajiTitle()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("Shingeki no Kyojin");
        List<object?> values = [titles, TsundokuLanguage.Romaji];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Shingeki no Kyojin"));
    }

    [Test]
    public void Convert_EnglishLanguage_ReturnsEnglishTitle()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("Shingeki no Kyojin", english: "Attack on Titan");
        List<object?> values = [titles, TsundokuLanguage.English];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Attack on Titan"));
    }

    [Test]
    public void Convert_MissingLanguage_FallsBackToRomaji()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("Shingeki no Kyojin");
        List<object?> values = [titles, TsundokuLanguage.Japanese];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Shingeki no Kyojin"));
    }

    [Test]
    public void Convert_EmptyRomaji_ReturnsError()
    {
        Dictionary<TsundokuLanguage, string> titles = new()
        {
            { TsundokuLanguage.Romaji, string.Empty }
        };
        List<object?> values = [titles, TsundokuLanguage.Romaji];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("ERROR"));
    }

    [Test]
    public void Convert_WithDuplicateIndex_Zero_ReturnsPlainTitle()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("One Piece");
        List<object?> values = [titles, TsundokuLanguage.Romaji, (uint)0];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("One Piece"));
    }

    [Test]
    public void Convert_WithDuplicateIndex_NonZero_AppendsSuffix()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("One Piece");
        List<object?> values = [titles, TsundokuLanguage.Romaji, (uint)2];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("One Piece (2)"));
    }

    [Test]
    public void Convert_WithDuplicateIndex_AsInt_AppendsSuffix()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("Naruto");
        List<object?> values = [titles, TsundokuLanguage.Romaji, 3];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Naruto (3)"));
    }

    [Test]
    public void Convert_WithDuplicateIndex_AsString_AppendsSuffix()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("Bleach");
        List<object?> values = [titles, TsundokuLanguage.Romaji, "5"];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Bleach (5)"));
    }

    [Test]
    public void Convert_WithDuplicateIndex_InvalidString_ReturnsError()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("Bleach");
        List<object?> values = [titles, TsundokuLanguage.Romaji, "abc"];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("ERROR"));
    }

    [Test]
    public void Convert_WithNullDuplicateIndex_ReturnsPlainTitle()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("Naruto");
        List<object?> values = [titles, TsundokuLanguage.Romaji, null];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Naruto"));
    }

    [Test]
    public void Convert_WithLargeDuplicateIndex_AppendsSuffix()
    {
        Dictionary<TsundokuLanguage, string> titles = CreateTitles("Title");
        List<object?> values = [titles, TsundokuLanguage.Romaji, (uint)12345];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Title (12345)"));
    }

    [Test]
    public void Convert_ReadOnlyDictionary_Works()
    {
        IReadOnlyDictionary<TsundokuLanguage, string> titles = new Dictionary<TsundokuLanguage, string>
        {
            { TsundokuLanguage.Romaji, "Test Title" },
            { TsundokuLanguage.English, "English Title" }
        };
        List<object?> values = [titles, TsundokuLanguage.English];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("English Title"));
    }

    [Test]
    public void Convert_EmptyEnglishTitle_FallsBackToRomaji()
    {
        Dictionary<TsundokuLanguage, string> titles = new()
        {
            { TsundokuLanguage.Romaji, "Romaji Fallback" },
            { TsundokuLanguage.English, string.Empty }
        };
        List<object?> values = [titles, TsundokuLanguage.English];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Romaji Fallback"));
    }
}
