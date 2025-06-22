using System.Text.Json;

namespace Tsundoku.Tests.AniList;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AniListTitleParseTests
{
    [Test]
    [Category("Title")]
    public void ExtractTitlesFromAniList_ValidTitles_AllKeysExtracted()
    {
        string json = """
        {
            "title": {
                "romaji": "Youkoso Jitsuryoku Shijou Shugi no Kyoushitsu e",
                "english": "Classroom of the Elite",
                "native": "ようこそ実力至上主義の教室へ"
            }
        }
        """;

        JsonElement element = JsonDocument.Parse(json).RootElement;
        Dictionary<string, string> titles = [];

        Clients.AniList.ExtractTitlesFromAniList(element, ref titles);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(titles, Contains.Key("Romaji"));
            Assert.That(titles, Contains.Key("English"));
            Assert.That(titles, Contains.Key("Japanese"));

            Assert.That(titles["Romaji"], Is.EqualTo("Youkoso Jitsuryoku Shijou Shugi no Kyoushitsu e"));
            Assert.That(titles["English"], Is.EqualTo("Classroom of the Elite"));
            Assert.That(titles["Japanese"], Is.EqualTo("ようこそ実力至上主義の教室へ"));
        }
    }

    [Test]
    public void ExtractTitlesFromAniList_MissingSomeFields_OnlyPresentKeysAdded()
    {
        string json = """
        {
            "title": {
                "romaji": "SPY x FAMILY",
                "native": "SPY×FAMILY"
            }
        }
        """;

        JsonElement element = JsonDocument.Parse(json).RootElement;
        Dictionary<string, string> titles = new();

        Clients.AniList.ExtractTitlesFromAniList(element, ref titles);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(titles, Contains.Key("Romaji"));
            Assert.That(titles, Contains.Key("Japanese"));
            Assert.That(titles, Does.Not.ContainKey("English"));

            Assert.That(titles["Romaji"], Is.EqualTo("SPY x FAMILY"));
            Assert.That(titles["Japanese"], Is.EqualTo("SPY×FAMILY"));
        }
    }

    [Test]
    public void ExtractTitlesFromAniList_NoTitleProperty_DoesNothing()
    {
        string json = """{ "id": 12345 }""";
        JsonElement element = JsonDocument.Parse(json).RootElement;
        Dictionary<string, string> titles = new();

        Clients.AniList.ExtractTitlesFromAniList(element, ref titles);

        Assert.That(titles, Is.Empty);
    }

    [Test]
    public void ExtractTitlesFromAniList_TitleIsNotObject_DoesNothing()
    {
        string json = """{ "title": "invalid" }""";
        JsonElement element = JsonDocument.Parse(json).RootElement;
        Dictionary<string, string> titles = new();

        Clients.AniList.ExtractTitlesFromAniList(element, ref titles);

        Assert.That(titles, Is.Empty);
    }
}
