using Tsundoku.Models;
using Tsundoku.Helpers;
using System.Text.Json;

[assembly: Description("Testing the Series class, mainly ensuring data validity.")]
namespace TsundokuTests
{
    // [Parallelizable(ParallelScope.All)]
    [TestFixture]
    [Author("Sean (Alias -> Sigrec or Prem)")]
    [TestOf(typeof(Series))]
    [Culture("en,ja-JP,fr,ko-KR")] // https://www.csharp-examples.net/culture-names/
    [SetUICulture("en")]
    public class SeriesModelTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void CreateCoverFilePath_Test()
        {
            JsonElement narutoQuery = JsonDocument.Parse(new AniListQuery().GetSeriesTitle("Naruto", "MANGA")).RootElement.GetProperty("Media");

            Assert.That("Covers\\NARUTO_MANGA.jpg", Is.EqualTo(Series.CreateCoverFilePath(narutoQuery.GetProperty("coverImage").GetProperty("extraLarge").ToString(), narutoQuery.GetProperty("title").GetProperty("romaji").ToString(), "MANGA", narutoQuery.GetProperty("synonyms"), false)));
        }

        [Test]
        public void UnnecessaryBreakRemoval_Test()
        {
            Assert.That(Series.ParseDescription("After a fierce battle between humans and vampires, a temporary peace was established, but Kaname continued to sleep within a coffin of ice… Yuki gave Kaname her heart to revive him as a human being.\n<br><br>\nThese are the stories of what happened during those 1,000 years of Kaname’s slumber and at the start of his human life.\n<br><br>\n(Source: Viz Media)"), Is.EqualTo("After a fierce battle between humans and vampires, a temporary peace was established, but Kaname continued to sleep within a coffin of ice\u2026 Yuki gave Kaname her heart to revive him as a human being.\n\nThese are the stories of what happened during those 1,000 years of Kaname\u2019s slumber and at the start of his human life."));
        }

        [Test]
        public void IdenticalSeriesNames_Test()
        {
            Assert.Multiple(() => {
                Assert.That("Getsuyoubi no Tawawa", Is.EqualTo(JsonDocument.Parse(new AniListQuery().GetSeriesID(98282, "MANGA")).RootElement.GetProperty("Media").GetProperty("title").GetProperty("romaji").ToString()));
                Assert.That("Getsuyoubi no Tawawa", Is.EqualTo(JsonDocument.Parse(new AniListQuery().GetSeriesID(125854, "MANGA")).RootElement.GetProperty("Media").GetProperty("title").GetProperty("romaji").ToString()));
            });
        }

        [Test]
        public void CreateCoverFilePath_Duplicate_Test()
        {
            if (!File.Exists("Covers\\OnePunchMan_MANGA.jpg"))
            {
                File.Create("Covers\\OnePunchMan_MANGA.jpg");
            }

            JsonElement onePunchManQuery = JsonDocument.Parse(new AniListQuery().GetSeriesTitle("One-Punch Man (Original)", "MANGA")).RootElement.GetProperty("Media");

            Assert.That("Covers\\One-PunchMan(Original)_MANGA.jpg", Is.EqualTo(Series.CreateCoverFilePath(onePunchManQuery.GetProperty("coverImage").GetProperty("extraLarge").ToString(), onePunchManQuery.GetProperty("title").GetProperty("romaji").ToString(), "MANGA", onePunchManQuery.GetProperty("synonyms"), false)));
        }

        [Test]
        public void ParseDescription_BR_And_Ampersand_Test()
        {
            // Baki description to check for <br><br> & ampersand string for test
            string amerpsandAndBrDesc = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time&hellip; To lose is to die.\n<br><br>Hanma Baki (named &ldquo;Wild Fang&rdquo; by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father&rsquo;s command. For he must obey his father&rsquo;s rule that at his &ldquo;coming of age&rdquo; Baki surpass his own father, Hanma Yuujiro, &ldquo;the most powerful creature walking on earth.&rdquo; Baki&rsquo;s life has been nothing but trouble. This has given Baki a wild nature and convinced him that &ldquo;to be the most powerful&rdquo; is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become &ldquo;the strongest man on earth.&rdquo; That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever.  ";

            string expectedAmersandAndBrDesc = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time… To lose is to die.\n\nHanma Baki (named “Wild Fang” by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father’s command. For he must obey his father’s rule that at his “coming of age” Baki surpass his own father, Hanma Yuujiro, “the most powerful creature walking on earth.” Baki’s life has been nothing but trouble. This has given Baki a wild nature and convinced him that “to be the most powerful” is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become “the strongest man on earth.” That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever.";

            Assert.That(Series.ParseDescription(amerpsandAndBrDesc), Is.EqualTo(expectedAmersandAndBrDesc));
        }

        [Test]
        public void ParseDescription_ExcessiveLineBreaks_Test()
        {
            string excessiveLineBreakDesc = "Average human teenage boy Tsukune accidentally enrolls at a boarding school for monsters--no, not jocks and popular kids, but bona fide werewolves, witches, and unnameables out of his wildest nightmares!<br><br>\nOn the plus side, all the girls have a monster crush on him. On the negative side, all the boys are so jealous they want to kill him! And so do the girls he spurns because he only has eyes for one of them--the far-from-average vampire Moka.<br><br>\nOn the plus side, Moka only has glowing red eyes for Tsukune. On the O-negative side, she also has a burning, unquenchable thirst for his blood...\n<br><br>\n\n(Source: Viz Media)";
            string expectedExcssiveLineBreakDesc = "Average human teenage boy Tsukune accidentally enrolls at a boarding school for monsters--no, not jocks and popular kids, but bona fide werewolves, witches, and unnameables out of his wildest nightmares!\n\nOn the plus side, all the girls have a monster crush on him. On the negative side, all the boys are so jealous they want to kill him! And so do the girls he spurns because he only has eyes for one of them--the far-from-average vampire Moka.\n\nOn the plus side, Moka only has glowing red eyes for Tsukune. On the O-negative side, she also has a burning, unquenchable thirst for his blood...";

            Assert.That(Series.ParseDescription(excessiveLineBreakDesc), Is.EqualTo(expectedExcssiveLineBreakDesc));
        }

        [Test]
        public void ParseDescription_Html_And_RemoveSource_Test()
        {
            string htmlDesc = "In a world where awakened beings called “Hunters” must battle deadly monsters to protect humanity, Sung Jinwoo, nicknamed “the weakest hunter of all mankind,” finds himself in a constant struggle for survival. One day, after a brutal encounter in an overpowered dungeon wipes out his party and threatens to end his life, a mysterious System chooses him as its sole player: Jinwoo has been granted the rare opportunity to level up his abilities, possibly beyond any known limits. Follow Jinwoo’s journey as he takes on ever-stronger enemies, both human and monster, to discover the secrets deep within the dungeons and the ultimate extent of his powers.\n<br><br>\n(Source: Tappytoon)\n<br><br>\n<i>Note: Chapter count includes a prologue. </i>";

            string expectedHtmlDesc = "In a world where awakened beings called “Hunters” must battle deadly monsters to protect humanity, Sung Jinwoo, nicknamed “the weakest hunter of all mankind,” finds himself in a constant struggle for survival. One day, after a brutal encounter in an overpowered dungeon wipes out his party and threatens to end his life, a mysterious System chooses him as its sole player: Jinwoo has been granted the rare opportunity to level up his abilities, possibly beyond any known limits. Follow Jinwoo’s journey as he takes on ever-stronger enemies, both human and monster, to discover the secrets deep within the dungeons and the ultimate extent of his powers.";

            Assert.That(Series.ParseDescription(htmlDesc), Is.EqualTo(expectedHtmlDesc));
        }

        [Test]
        public void GetCorrectComicName_Test()
        {
            Assert.Multiple(() => {
                Assert.That(Series.GetCorrectComicName("JP"), Is.EqualTo("Manga"));
                Assert.That(Series.GetCorrectComicName("KR"), Is.EqualTo("Manhwa"));
                Assert.That(Series.GetCorrectComicName("CN"), Is.EqualTo("Manhua"));
                Assert.That(Series.GetCorrectComicName("TW"), Is.EqualTo("Manhua"));
                Assert.That(Series.GetCorrectComicName("FR"), Is.EqualTo("Manfra"));
                Assert.That(Series.GetCorrectComicName("USA"), Is.EqualTo("Error"));
            });
        }

        [Test]
        public void GetSeriesStatus_Test()
        {
            Assert.Multiple(() => {
                Assert.That(Series.GetSeriesStatus("RELEASING"), Is.EqualTo("Ongoing"));
                Assert.That(Series.GetSeriesStatus("NOT_YET_RELEASED"), Is.EqualTo("Ongoing"));
                Assert.That(Series.GetSeriesStatus("FINISHED"), Is.EqualTo("Finished"));
                Assert.That(Series.GetSeriesStatus("CANCELLED"), Is.EqualTo("Cancelled"));
                Assert.That(Series.GetSeriesStatus("HIATUS"), Is.EqualTo("Hiatus"));
                Assert.That(Series.GetSeriesStatus("UNICORN"), Is.EqualTo("Error"));
            });
        }

        [Test] // Testing with Bakemonogatari
        public void GetSeriesStaff_ToManyIllustrators_Test()
        {
            JsonElement bakemonogatariStaffQuery = JsonDocument.Parse(new AniListQuery().GetSeriesTitle("化物語", "MANGA")).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges");

            Assert.That(Series.GetSeriesStaff(bakemonogatariStaffQuery, "full", "Manga", "Bakemonogatari"), Is.EqualTo("Ito Oogure | NISIOISIN | VOFAN | Akio Watanabe"));
        }

        [Test]
        public void GetSeriesStaff_MultplieStaffForValidRole_Test()
        {
            JsonElement soloLevelingQuery = JsonDocument.Parse(new AniListQuery().GetSeriesTitle("나 혼자만 레벨업", "MANGA")).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges");

            Assert.That(Series.GetSeriesStaff(soloLevelingQuery, "full", "Manga", "Na Honjaman Level Up"), Is.EqualTo("Seong-Rak Jang | Chu-Gong | So-Ryeong Gi | Hyeon-Gun"));
        }

        [Test]
        public void GetSeriesStaff_Anthology_Test()
        {
            //Lycoris Recoil Koushiki Comic Anthology: Repeat
            JsonElement anthologyQuery = JsonDocument.Parse(new AniListQuery().GetSeriesTitle("リコリス・リコイル 公式コミックアンソロジー リピート", "MANGA")).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges");

            Assert.That(Series.GetSeriesStaff(anthologyQuery, "full", "Manga", "Lycoris Recoil Koushiki Comic Anthology: Repeat"), Is.EqualTo("Takeshi Kojima | Mekimeki | Nyoijizai | GUNP | Itsuki Takano | Ren Sakuragi | sometime | Ryou Niina | Ginmoku | Mikaduchi | Nikomi Wakadori | Miki Morinaga | Raika Suzumi | Ree | Atto | Tiv | Sou Hamayumiba | Kanari Abe | Nachi Aono"));
        }

        [Test] // Tests if only native = null, onyl full = null, and both native and full are null
        public void GetSeriesStaff_AllNullStaffScenarios_Name_Test()
        {
            JsonElement testALQuery = JsonDocument.Parse(File.ReadAllText(@"\Tsundoku\Tests\TsundokuTests\SeriesTestData\staffNameTest.json")).RootElement.GetProperty("data").GetProperty("Media").GetProperty("staff").GetProperty("edges");

            Assert.That(Series.GetSeriesStaff(testALQuery, "full", "Novel", "86: Eighty Six"), Is.EqualTo("Asato Asato | しらび | Error"));
        }
    }
}