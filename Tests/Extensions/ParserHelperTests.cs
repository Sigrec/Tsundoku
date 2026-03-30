using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Tsundoku.Tests.Extensions;

[TestFixture]
public class ParserHelperTests
{
    #region ContainsAny

    [Test]
    public void ContainsAny_InputContainsOneOfTheValues_ReturnsTrue()
    {
        bool result = "Hello World".ContainsAny(["World", "Mars"]);
        Assert.That(result, Is.True);
    }

    [Test]
    public void ContainsAny_InputContainsNoneOfTheValues_ReturnsFalse()
    {
        bool result = "Hello World".ContainsAny(["Mars", "Jupiter"]);
        Assert.That(result, Is.False);
    }

    [Test]
    public void ContainsAny_CaseInsensitiveMatch_ReturnsTrue()
    {
        bool result = "Hello World".ContainsAny(["HELLO"]);
        Assert.That(result, Is.True);
    }

    [Test]
    public void ContainsAny_EmptyValues_ReturnsFalse()
    {
        bool result = "Hello World".ContainsAny([]);
        Assert.That(result, Is.False);
    }

    [Test]
    public void ContainsAny_SubstringMatch_ReturnsTrue()
    {
        bool result = "Manga Collection".ContainsAny(["Coll"]);
        Assert.That(result, Is.True);
    }

    #endregion

    #region ParseCsvString

    private static CsvReader CreateCsvReader(string csvContent)
    {
        StringReader stringReader = new StringReader(csvContent);
        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };
        CsvReader csv = new CsvReader(stringReader, config);
        csv.Read();
        csv.ReadHeader();
        csv.Read();
        return csv;
    }

    [Test]
    public void ParseCsvString_FieldHasValue_ReturnsTrimmedValue()
    {
        using CsvReader csv = CreateCsvReader("Title\n  Berserk  ");
        string result = csv.ParseCsvString("Title", "Unknown");
        Assert.That(result, Is.EqualTo("Berserk"));
    }

    [Test]
    public void ParseCsvString_FieldIsEmpty_ReturnsNullValue()
    {
        using CsvReader csv = CreateCsvReader("Title\n\"\"");
        string result = csv.ParseCsvString("Title", "Unknown");
        Assert.That(result, Is.EqualTo("Unknown"));
    }

    [Test]
    public void ParseCsvString_FieldIsWhitespace_ReturnsNullValue()
    {
        using CsvReader csv = CreateCsvReader("Title\n\"   \"");
        string result = csv.ParseCsvString("Title", "N/A");
        Assert.That(result, Is.EqualTo("N/A"));
    }

    [Test]
    public void ParseCsvString_FieldHasLeadingAndTrailingSpaces_ReturnsTrimmed()
    {
        using CsvReader csv = CreateCsvReader("Title\n\"  One Piece  \"");
        string result = csv.ParseCsvString("Title", "Default");
        Assert.That(result, Is.EqualTo("One Piece"));
    }

    #endregion
}
