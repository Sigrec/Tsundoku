using System.Text.Json;
using static Tsundoku.Models.Enums.SeriesGenreModel;

namespace Tsundoku.Tests.AniList;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AniListDescParseTests
{
    [Test]
    public void ParseGenreArray_InvalidValueKind_ReturnsEmptySet()
    {
        using JsonDocument doc = JsonDocument.Parse(@"{ ""notArray"": 123 }");
        JsonElement notArray = doc.RootElement.GetProperty("notArray");

        HashSet<SeriesGenre> result = Clients.AniList.ParseGenreArray("Bad Kind", notArray);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseDescription_WithUnnecessaryBreaksAndSource_RemovesBreaksAndSource()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Clients.AniList.ParseSeriesDescription("<i>Serialisation of the original doujin</i><br><br>\n\nSaito's never been anyone special, but his unremarkable path takes a turn when he wakes up in another world. After all, who other than the handyman could be trusted to open locked treasure chests or to repair equipment?\n<br><br>\n(Source: Yen Press)"),
                Is.EqualTo("Serialisation of the original doujin\n\nSaito's never been anyone special, but his unremarkable path takes a turn when he wakes up in another world. After all, who other than the handyman could be trusted to open locked treasure chests or to repair equipment?"));

            Assert.That(Clients.AniList.ParseSeriesDescription("After a fierce battle between humans and vampires, a temporary peace was established, but Kaname continued to sleep within a coffin of ice� Yuki gave Kaname her heart to revive him as a human being.\n<br><br>\nThese are the stories of what happened during those 1,000 years of Kaname's slumber and at the start of his human life.\n<br><br>\n(Source: Viz Media)"),
                Is.EqualTo("After a fierce battle between humans and vampires, a temporary peace was established, but Kaname continued to sleep within a coffin of ice� Yuki gave Kaname her heart to revive him as a human being.\n\nThese are the stories of what happened during those 1,000 years of Kaname's slumber and at the start of his human life."));
        }
    }

    [Test]
    public void ParseDescription_WithNoteTag_RemovesNote()
    {
        string input = "...<i>Note: The manga chapters are irregularly released...</i>";
        string expected = "...";

        Assert.That(Clients.AniList.ParseSeriesDescription(input), Is.EqualTo(expected));
    }

    [Test]
    public void ParseDescription_WithHtmlEntitiesAndBreaks_DecodesEntitiesAndNormalizesBreaks()
    {
        string input = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time&hellip; To lose is to die.<br><br>\nHanma Baki (named &ldquo;Wild Fang&rdquo; by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father&rsquo;s command. For he must obey his father&rsquo;s rule that at his &ldquo;coming of age&rdquo; Baki surpass his own father, Hanma Yuujiro, &ldquo;the most powerful creature walking on earth.&rdquo; Baki&rsquo;s life has been nothing but trouble. This has given Baki a wild nature and convinced him that &ldquo;to be the most powerful&rdquo; is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become &ldquo;the strongest man on earth.&rdquo; That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever. ";
        string expected = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time… To lose is to die.\n\nHanma Baki (named \"Wild Fang\" by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father's command. For he must obey his father's rule that at his \"coming of age\" Baki surpass his own father, Hanma Yuujiro, \"the most powerful creature walking on earth.\" Baki's life has been nothing but trouble. This has given Baki a wild nature and convinced him that \"to be the most powerful\" is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become \"the strongest man on earth.\" That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever.";

        Assert.That(Clients.AniList.ParseSeriesDescription(input), Is.EqualTo(expected));
    }

    [Test]
    public void ParseDescription_WithExcessiveBreaksAndSource_RemovesBreaksAndSource()
    {
        string input = "A sentence...\n<br><br>\n\n(Source: Viz Media)";
        string expected = "A sentence...";

        Assert.That(Clients.AniList.ParseSeriesDescription(input), Is.EqualTo(expected));
    }

    [Test]
    public void ParseDescription_WithHtmlTagsAndNote_RemovesMetadata()
    {
        string input = "...Jinwoo's journey...\n<br><br>\n(Source: Tappytoon)\n<br><br>\n<i>Note: Chapter count includes a prologue. </i>";
        string expected = "...Jinwoo's journey...";

        Assert.That(Clients.AniList.ParseSeriesDescription(input), Is.EqualTo(expected));
    }

    [Test]
    public void ParseDescription_WithEscapedAngleBracketsAndSource_CleansAndDecodes()
    {
        string input = "The master spy codenamed &lt;Twilight&gt; has spent his days on undercover missions, all for the dream of a better world. But one day, he receives a particularly difficult new order from command. For his mission, he must form a temporary family and start a new life?! A Spy/Action/Comedy about a one-of-a-kind family!<br><br>\n(Source: MANGA Plus)<br><br>\n<i>Notes:<br>\n- Includes 2 \"Extra Missions\" and 11 \"Short Missions\".<br>\n- Nominated for the 24th Tezuka Osamu Cultural Prize in 2020.<br>\n- Nominated for the 13th and 14th Manga Taisho Award in 2020 and 2021.<br>\n- Nominated for the 44th Kodansha Manga Award in the Shounen Category in 2020.</i>";
        string expected = "The master spy codenamed <Twilight> has spent his days on undercover missions, all for the dream of a better world. But one day, he receives a particularly difficult new order from command. For his mission, he must form a temporary family and start a new life?! A Spy/Action/Comedy about a one-of-a-kind family!";

        Assert.That(Clients.AniList.ParseSeriesDescription(input), Is.EqualTo(expected));
    }

    [Test]
    public void ParseDescription_WhenInputIsNullOrWhitespace_ReturnsEmpty()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Clients.AniList.ParseSeriesDescription(null!), Is.EqualTo(string.Empty));
            Assert.That(Clients.AniList.ParseSeriesDescription(""), Is.EqualTo(string.Empty));
            Assert.That(Clients.AniList.ParseSeriesDescription("   "), Is.EqualTo(string.Empty));
        }
    }

    [Test]
    public void ParseDescription_WhenNoFormattingNeeded_ReturnsOriginalText()
    {
        const string input = "Just a plain description.";
        const string expected = input;

        Assert.That(Clients.AniList.ParseSeriesDescription(input), Is.EqualTo(expected));
    }

    [Test]
    public void ParseDescription_WithStackedTagsAndSource_RemovesAllUnwantedContent()
    {
        string input = "<i>\"I'm the strongest.\"</i><br><br><br><br><br>\n<br><br><br>\n(Source: Nothing)";
        string expected = "\"I'm the strongest.\"";

        Assert.That(Clients.AniList.ParseSeriesDescription(input), Is.EqualTo(expected));
    }
}
