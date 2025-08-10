using System.Text.Json;
using static Tsundoku.Models.Enums.SeriesGenreModel;

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

        HashSet<SeriesGenre> result = Clients.MangaDex.ParseGenreData("Test Manga", root.GetProperty("data").GetProperty("attributes"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result, Does.Contain(SeriesGenre.Action));
            Assert.That(result, Does.Contain(SeriesGenre.Psychological));
            Assert.That(result, Does.Contain(SeriesGenre.Fantasy));
            Assert.That(result, Does.Contain(SeriesGenre.Mystery));
        }
    }

    [Test]
    public void ParseGenreData_NullJson_ReturnsEmptySet()
    {
        JsonElement nullElement = JsonDocument.Parse("null").RootElement;
        HashSet<SeriesGenre> result = Clients.MangaDex.ParseGenreData("Empty Genre", nullElement);
        Assert.That(result, Is.Empty);
    }
}