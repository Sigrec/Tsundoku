using System.Text.Json;

namespace Tsundoku.Tests.MangaDex;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class MangaDexGenreParseTests
{
    [Test]
    public void ParseGenreData_ExtractsOnlyValidGenresFromJson()
    {
        string json = File.ReadAllText(@"MangaDex\MangaDexTestData\SeriesTestData.json");

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        HashSet<Genre> result = Clients.MangaDex.ParseGenreData("Test Manga", root.GetProperty("data").GetProperty("attributes"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result, Does.Contain(Genre.Action));
            Assert.That(result, Does.Contain(Genre.Psychological));
            Assert.That(result, Does.Contain(Genre.Fantasy));
            Assert.That(result, Does.Contain(Genre.Mystery));
        }
    }

    [Test]
    public void ParseGenreData_NullJson_ReturnsEmptySet()
    {
        JsonElement nullElement = JsonDocument.Parse("null").RootElement;
        HashSet<Genre> result = Clients.MangaDex.ParseGenreData("Empty Genre", nullElement);
        Assert.That(result, Is.Empty);
    }
}