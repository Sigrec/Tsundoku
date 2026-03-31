using System.Globalization;
using Tsundoku.Converters;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Tests.Converters;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class StaffLangConverterTests
{
    private static readonly StaffLangConverter Converter = StaffLangConverter.Instance;

    private static Dictionary<TsundokuLanguage, string> CreateStaff(
        string romaji = "Romaji Name",
        string? english = null,
        string? japanese = null)
    {
        Dictionary<TsundokuLanguage, string> staff = new()
        {
            { TsundokuLanguage.Romaji, romaji }
        };

        if (english is not null)
        {
            staff[TsundokuLanguage.English] = english;
        }

        if (japanese is not null)
        {
            staff[TsundokuLanguage.Japanese] = japanese;
        }

        return staff;
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
        List<object?> values = [CreateStaff()];

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
    public void Convert_RomajiLanguageEnum_ReturnsRomajiName()
    {
        Dictionary<TsundokuLanguage, string> staff = CreateStaff("Oda Eiichiro");
        List<object?> values = [staff, TsundokuLanguage.Romaji];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Oda Eiichiro"));
    }

    [Test]
    public void Convert_EnglishLanguageEnum_ReturnsEnglishName()
    {
        Dictionary<TsundokuLanguage, string> staff = CreateStaff("Oda Eiichiro", english: "Eiichiro Oda");
        List<object?> values = [staff, TsundokuLanguage.English];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Eiichiro Oda"));
    }

    [Test]
    public void Convert_MissingLanguageEnum_FallsBackToRomaji()
    {
        Dictionary<TsundokuLanguage, string> staff = CreateStaff("Oda Eiichiro");
        List<object?> values = [staff, TsundokuLanguage.Japanese];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Oda Eiichiro"));
    }

    [Test]
    public void Convert_LanguageAsString_Romaji_ReturnsRomajiName()
    {
        Dictionary<TsundokuLanguage, string> staff = CreateStaff("Kishimoto Masashi");
        List<object?> values = [staff, "Romaji"];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Kishimoto Masashi"));
    }

    [Test]
    public void Convert_LanguageAsString_English_ReturnsEnglishName()
    {
        Dictionary<TsundokuLanguage, string> staff = CreateStaff("Kishimoto Masashi", english: "Masashi Kishimoto");
        List<object?> values = [staff, "English"];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Masashi Kishimoto"));
    }

    [Test]
    public void Convert_LanguageAsString_MissingLanguage_FallsBackToRomaji()
    {
        Dictionary<TsundokuLanguage, string> staff = CreateStaff("Toriyama Akira");
        List<object?> values = [staff, "Japanese"];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Toriyama Akira"));
    }

    [Test]
    public void Convert_EmptyRomaji_ReturnsError()
    {
        Dictionary<TsundokuLanguage, string> staff = new()
        {
            { TsundokuLanguage.Romaji, string.Empty }
        };
        List<object?> values = [staff, TsundokuLanguage.Romaji];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("ERROR"));
    }

    [Test]
    public void Convert_WhitespaceRomaji_ReturnsError()
    {
        Dictionary<TsundokuLanguage, string> staff = new()
        {
            { TsundokuLanguage.Romaji, "   " }
        };
        List<object?> values = [staff, TsundokuLanguage.Romaji];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("ERROR"));
    }

    [Test]
    public void Convert_NullLanguageValue_DefaultsToRomaji()
    {
        Dictionary<TsundokuLanguage, string> staff = CreateStaff("Default Name");
        List<object?> values = [staff, null];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Default Name"));
    }

    [Test]
    public void Convert_InvalidLanguageType_ReturnsError()
    {
        Dictionary<TsundokuLanguage, string> staff = CreateStaff("Name");
        List<object?> values = [staff, 42];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("ERROR"));
    }

    [Test]
    public void Convert_ReadOnlyDictionary_Works()
    {
        IReadOnlyDictionary<TsundokuLanguage, string> staff = new Dictionary<TsundokuLanguage, string>
        {
            { TsundokuLanguage.Romaji, "Author Name" },
            { TsundokuLanguage.English, "English Author" }
        };
        List<object?> values = [staff, TsundokuLanguage.English];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("English Author"));
    }

    [Test]
    public void Convert_EmptyEnglishName_FallsBackToRomaji()
    {
        Dictionary<TsundokuLanguage, string> staff = new()
        {
            { TsundokuLanguage.Romaji, "Romaji Fallback" },
            { TsundokuLanguage.English, string.Empty }
        };
        List<object?> values = [staff, TsundokuLanguage.English];

        object? result = Converter.Convert(values, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo("Romaji Fallback"));
    }

    [Test]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            Converter.ConvertBack(null, [typeof(string)], null, CultureInfo.InvariantCulture));
    }
}
