using System.Text.Json;

namespace Tsundoku.Tests.MangaDex;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class MangaDexAltTitleTests
{
    private static JsonDocument document;
    private static JsonElement altTitles;
    private static JsonElement[] altTitlesArray;

    private static JsonElement CreateJsonElement(string json)
    {
        return JsonDocument.Parse(json).RootElement;
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        string json = File.ReadAllText(@"MangaDex\MangaDexTestData\SeriesTestData.json");
        document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        altTitles = root
            .GetProperty("data")
            .GetProperty("attributes")
            .GetProperty("altTitles");
        altTitlesArray = [.. altTitles.EnumerateArray()];
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        document?.Dispose();
    }

    [Test]
    public void GetAdditionalMangaDexTitleList_FullDocument_ParsesAltTitlesFromAttributes()
    {
        JsonElement[] results = Clients.MangaDex.GetAdditionalMangaDexTitleList(document.RootElement, "The Bugle Call", "戦奏教室");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Has.Length.EqualTo(5), "Expected 5 alternative titles in the list.");
            Assert.That(results.Any(e => e.TryGetProperty("ja", out JsonElement j) && j.GetString().Equals("戦奏教室")), "Expected 'ja' title '戦奏教室' to be present.");
            Assert.That(results.Any(e => e.TryGetProperty("ja-ro", out JsonElement jro) && jro.GetString().Equals("Sensou Kyoushitsu")), "Expected 'ja-ro' title 'Sensou Kyoushitsu' to be present.");
            Assert.That(results.Any(e => e.TryGetProperty("fr", out JsonElement fr) && fr.GetString().Equals("The Bugle Call")), "Expected 'fr' title 'The Bugle Call' to be present.");
            Assert.That(results.Any(e => e.TryGetProperty("ru", out JsonElement ru) && ru.GetString().Equals("Зов горна")), "Expected 'ru' title 'Зов горна' to be present.");
            Assert.That(results.Any(e => e.TryGetProperty("pt-br", out JsonElement ptbr) && ptbr.GetString().Equals("Uma Canção de Guerra")), "Expected 'pt-br' title 'Uma Canção de Guerra' to be present.");
        }
    }

    [Test]
    public void GetAltTitle_ShouldReturnCorrectTitle_WhenFound()
    {
        string country = "fr"; // Corresponds to {"fr": "The Bugle Call"}
        string expectedTitle = "The Bugle Call";

        string? actualTitle = Clients.MangaDex.GetAltTitle(country, altTitlesArray);

        Assert.That(actualTitle, Is.EqualTo(expectedTitle));
    }

    [Test]
    public void GetAltTitle_ShouldReturnFirstMatch_WhenMultipleExist()
    {
        string country = "ja";
        // The first "ja" entry in the JSON is "戦奏教室"
        string expectedTitle = "戦奏教室";

        string? actualTitle = Clients.MangaDex.GetAltTitle(country, altTitlesArray);

        Assert.That(actualTitle, Is.EqualTo(expectedTitle));
    }


    [Test]
    public void GetAltTitle_ShouldReturnNull_WhenCountryNotFound()
    {
        string country = "es-la"; // This language is in availableTranslatedLanguages, but not in altTitles
                                  // (only "pt-br", "ru", "fr", "ja", "ja-ro" are in altTitles)

        string? actualTitle = Clients.MangaDex.GetAltTitle(country, altTitles.EnumerateArray().ToArray());

        Assert.That(actualTitle, Is.Null);
    }

    [Test]
    public void GetAltTitle_ShouldReturnNull_WhenAltTitlesIsEmpty()
    {
        // Arrange - Create an empty JsonElement array explicitly
        JsonElement[] emptyAltTitles = [];
        string country = "en";

        string? actualTitle = Clients.MangaDex.GetAltTitle(country, emptyAltTitles);

        Assert.That(actualTitle, Is.Null);
    }

    [Test]
    public void GetAltTitle_ShouldReturnNull_WhenAltTitlesContainsNonObjectElements()
    {
        // Arrange - A custom array with invalid JSON elements for robustness
        JsonElement[] customAltTitles =
        [
            CreateJsonElement("\"just a string\""), // Not an object
            CreateJsonElement("123"),               // Not an object
            CreateJsonElement("true"),              // Not an object
            CreateJsonElement("{\"en\": \"Valid Title Here\"}") // Valid object
        ];
        string country = "en";
        string expectedTitle = "Valid Title Here";

        string? actualTitle = Clients.MangaDex.GetAltTitle(country, customAltTitles);

        Assert.That(actualTitle, Is.EqualTo(expectedTitle)); // Should still find the valid one
    }

    [Test]
    public void GetAltTitle_ShouldReturnNull_WhenTitleValueIsNullJson()
    {
        // Arrange - Create a custom array with an explicitly null JSON value
        JsonElement[] customAltTitles =
        [
            CreateJsonElement("{\"en\": \"English Title\"}"),
            CreateJsonElement("{\"custom-null\": null}") // Property exists, but value is JSON null
        ];
        string country = "custom-null";

        string? actualTitle = Clients.MangaDex.GetAltTitle(country, customAltTitles);

        Assert.That(actualTitle, Is.Null); // JsonElement.GetString() correctly returns null for JSON null
    }

    [Test]
    public void GetAltTitle_ShouldReturnNull_WhenCountryIsEmptyString()
    {
        string country = ""; // Empty string as country code

        string? actualTitle = Clients.MangaDex.GetAltTitle(country, altTitlesArray);

        Assert.That(actualTitle, Is.Null); // An empty country code should not match any valid property name
    }

    [Test]
    public void TryGetAltTitle_ShouldReturnTrueAndCorrectTitle_WhenFound()
    {
        // Arrange
        string json = File.ReadAllText(@"MangaDex\MangaDexTestData\SeriesTestData.json");
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        JsonElement altTitles = root
            .GetProperty("data")
            .GetProperty("attributes")
            .GetProperty("altTitles");

        string country = "ru"; // Corresponds to {"ru": "Зов горна"}
        string expectedTitle = "Зов горна";

        bool result = Clients.MangaDex.TryGetAltTitle(country, altTitles.EnumerateArray().ToArray(), out string? actualTitle);

        Assert.That(result, Is.True);
        Assert.That(actualTitle, Is.EqualTo(expectedTitle));
    }

    [Test]
    public void TryGetAltTitle_ShouldReturnTrueAndFirstMatch_WhenMultipleExist()
    {
        // Arrange (using the provided JSON which has two "ja" entries)
        string json = File.ReadAllText(@"MangaDex\MangaDexTestData\SeriesTestData.json");
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        JsonElement altTitles = root
            .GetProperty("data")
            .GetProperty("attributes")
            .GetProperty("altTitles");

        string country = "ja";
        // The first "ja" entry in the JSON is "戦奏教室"
        string expectedTitle = "戦奏教室";

        bool result = Clients.MangaDex.TryGetAltTitle(country, altTitles.EnumerateArray().ToArray(), out string? actualTitle);

        Assert.That(result, Is.True);
        Assert.That(actualTitle, Is.EqualTo(expectedTitle));
    }


    [Test]
    public void TryGetAltTitle_ShouldReturnFalseAndNull_WhenCountryNotFound()
    {
        // Arrange
        string json = File.ReadAllText(@"MangaDex\MangaDexTestData\SeriesTestData.json");
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        JsonElement altTitles = root
            .GetProperty("data")
            .GetProperty("attributes")
            .GetProperty("altTitles");

        string country = "de"; // Does not exist in the altTitles of the JSON

        bool result = Clients.MangaDex.TryGetAltTitle(country, altTitles.EnumerateArray().ToArray(), out string? actualTitle);

        Assert.That(result, Is.False);
        Assert.That(actualTitle, Is.Null);
    }

    [Test]
    public void TryGetAltTitle_ShouldReturnFalseAndNull_WhenAltTitlesIsEmpty()
    {
        JsonElement[] emptyAltTitles = [];
        string country = "en";

        bool result = Clients.MangaDex.TryGetAltTitle(country, emptyAltTitles, out string? actualTitle);

        Assert.That(result, Is.False);
        Assert.That(actualTitle, Is.Null);
    }

    [Test]
    public void TryGetAltTitle_ShouldReturnTrueAndCorrectTitle_WhenMixedElements()
    {
        // Arrange - Custom array with invalid JSON elements for robustness
        JsonElement[] customAltTitles =
        [
            CreateJsonElement("123"),               // Not an object
            CreateJsonElement("{\"pt-br\": \"Título em Português\"}"), // Valid object
            CreateJsonElement("null")               // Not an object
        ];
        string country = "pt-br";
        string expectedTitle = "Título em Português";

        bool result = Clients.MangaDex.TryGetAltTitle(country, customAltTitles, out string? actualTitle);

        Assert.That(result, Is.True);
        Assert.That(actualTitle, Is.EqualTo(expectedTitle));
    }

    [Test]
    public void TryGetAltTitle_ShouldReturnTrueAndNull_WhenTitleValueIsNullJson()
    {
        // Arrange - Custom array with an explicitly null JSON value
        JsonElement[] customAltTitles =
        [
            CreateJsonElement("{\"en\": \"Another Title\"}"),
            CreateJsonElement("{\"jpn\": null}") // Property exists, but value is JSON null
        ];
        string country = "jpn";

        bool result = Clients.MangaDex.TryGetAltTitle(country, customAltTitles, out string? actualTitle);

        Assert.That(result, Is.True); // It found the property
        Assert.That(actualTitle, Is.Null); // But the string value is null
    }

    [Test]
    public void TryGetAltTitle_ShouldReturnFalseAndNull_WhenCountryIsEmptyString()
    {
        string country = ""; // Empty string as country code

        bool result = Clients.MangaDex.TryGetAltTitle(country, altTitles.EnumerateArray().ToArray(), out string? actualTitle);

        Assert.That(result, Is.False);
        Assert.That(actualTitle, Is.Null);
    }
}
