using System.Text.Json;
using static Tsundoku.Models.Enums.SeriesGenreEnum;

namespace Tsundoku.Tests.AniList;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AniListGenreParseTests
{

    [Test]
    public void ParseGenreArray_AllKnownGenres_AreParsedCorrectly()
    {
        string[] knownGenres =
        [
            "Action", "Adventure", "Comedy", "Drama", "Ecchi",
            "Fantasy", "Hentai", "Horror", "Mahou Shoujo", "Mecha",
            "Music", "Mystery", "Psychological", "Romance", "Sci-Fi",
            "Slice of Life", "Sports", "Supernatural", "Thriller"
        ];

        string json = JsonSerializer.Serialize(knownGenres);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement genresArray = doc.RootElement;

        HashSet<SeriesGenre> result = Clients.AniList.ParseGenreArray("Test Title", genresArray);

        Assert.That(result, Has.Count.EqualTo(knownGenres.Length));
    }

    [Test]
    public void ParseGenreArray_EmptyArray_ReturnsEmptySet()
    {
        using JsonDocument doc = JsonDocument.Parse("[]");
        JsonElement genresArray = doc.RootElement;

        HashSet<SeriesGenre> result = Clients.AniList.ParseGenreArray("Empty Test", genresArray);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseGenreArray_InvalidGenres_ReturnsEmptySet()
    {
        string[] invalid = ["Gibberish", "NonExistentGenre", "🎲 Board Games"];
        string json = JsonSerializer.Serialize(invalid);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement genresArray = doc.RootElement;

        HashSet<SeriesGenre> result = Clients.AniList.ParseGenreArray("Invalid Test", genresArray);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseGenreArray_MixedGenres_OnlyValidGenresParsed()
    {
        string[] mixed = ["Action", "FakeGenre", "Horror", "?", "Mecha"];
        string json = JsonSerializer.Serialize(mixed);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement genresArray = doc.RootElement;

        HashSet<SeriesGenre> result = Clients.AniList.ParseGenreArray("Mixed Test", genresArray);

        Assert.That(result, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain(SeriesGenre.Action));
            Assert.That(result, Does.Contain(SeriesGenre.Horror));
            Assert.That(result, Does.Contain(SeriesGenre.Mecha));
        }
    }

    [Test]
    public void ParseGenreArray_InvalidValueKind_ReturnsEmptySet()
    {
        using JsonDocument doc = JsonDocument.Parse(@"{ ""notArray"": 123 }");
        JsonElement notArray = doc.RootElement.GetProperty("notArray");

        HashSet<SeriesGenre> result = Clients.AniList.ParseGenreArray("Bad Kind", notArray);

        Assert.That(result, Is.Empty);
    }
}
