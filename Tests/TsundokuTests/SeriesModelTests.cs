using System.Diagnostics;
using Tsundoku.Models;
using Tsundoku.Helpers;

namespace TsundokuTests
{
    // [Parallelizable(ParallelScope.All)]
    public class SeriesModelTests
    {
        private System.Text.Json.JsonElement bakemonogatariStaffQuery;

        [SetUp]
        public void Setup()
        {
            bakemonogatariStaffQuery = System.Text.Json.JsonDocument.Parse(new AniListQuery().GetSeries("化物語", "MANGA")).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges");
        }

        [Test]
        public void ParseDescription_BR_And_Ampersand_Test()
        {
            // Baki description to check for <br><br> & ampersand string for test
            string amerpsandAndBrDesc = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time&hellip; To lose is to die.\n<br><br>Hanma Baki (named &ldquo;Wild Fang&rdquo; by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father&rsquo;s command. For he must obey his father&rsquo;s rule that at his &ldquo;coming of age&rdquo; Baki surpass his own father, Hanma Yuujiro, &ldquo;the most powerful creature walking on earth.&rdquo; Baki&rsquo;s life has been nothing but trouble. This has given Baki a wild nature and convinced him that &ldquo;to be the most powerful&rdquo; is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become &ldquo;the strongest man on earth.&rdquo; That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever.  ";

            string expectedAmersandAndBrDesc = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time… To lose is to die.\n\nHanma Baki (named “Wild Fang” by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father’s command. For he must obey his father’s rule that at his “coming of age” Baki surpass his own father, Hanma Yuujiro, “the most powerful creature walking on earth.” Baki’s life has been nothing but trouble. This has given Baki a wild nature and convinced him that “to be the most powerful” is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become “the strongest man on earth.” That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever.";

            Assert.AreEqual(expectedAmersandAndBrDesc, Series.ParseDescription(amerpsandAndBrDesc));
        }

        [Test]
        public void ParseDescription_RemoveSource_Test()
        {
            // Mahouka Koukou no Rettousei: Koto Nairan-hen description to check for unicode characters/japanese characters being translated correctly
            string removeSourceDesc = "Baki is bored. After the conclusion of the epic battle between father and son, he continues to fight in the underground arena and train nonstop, but he always has to suppress his yawn caused by the overbearing boredom. No amount of stimulus or danger can bring excitement to him at this point.\n\nNow, with the inclusion of the Prime Minister of Japan in the loop, a massive cloning project is attempting to clone Miyamoto Musashi, one of the fathers of martial arts in Japan. Another fight of historical proportions awaits Baki!\n\n(Source: MangaHelpers)\n\nIncludes chapter 197.5";

            string expectedRemoveSourceDesc = "Baki is bored. After the conclusion of the epic battle between father and son, he continues to fight in the underground arena and train nonstop, but he always has to suppress his yawn caused by the overbearing boredom. No amount of stimulus or danger can bring excitement to him at this point.\n\nNow, with the inclusion of the Prime Minister of Japan in the loop, a massive cloning project is attempting to clone Miyamoto Musashi, one of the fathers of martial arts in Japan. Another fight of historical proportions awaits Baki!";

            Assert.AreEqual(expectedRemoveSourceDesc, Series.ParseDescription(removeSourceDesc));
        }

        [Test]
        public void ParseDescription_ExcessiveLineBreaks_Test()
        {
            string excessiveLineBreakDesc = "Average human teenage boy Tsukune accidentally enrolls at a boarding school for monsters--no, not jocks and popular kids, but bona fide werewolves, witches, and unnameables out of his wildest nightmares!<br><br>\nOn the plus side, all the girls have a monster crush on him. On the negative side, all the boys are so jealous they want to kill him! And so do the girls he spurns because he only has eyes for one of them--the far-from-average vampire Moka.<br><br>\nOn the plus side, Moka only has glowing red eyes for Tsukune. On the O-negative side, she also has a burning, unquenchable thirst for his blood...\n<br><br>\n\n(Source: Viz Media)";
            string expectedExcssiveLineBreakDesc = "Average human teenage boy Tsukune accidentally enrolls at a boarding school for monsters--no, not jocks and popular kids, but bona fide werewolves, witches, and unnameables out of his wildest nightmares!\n\nOn the plus side, all the girls have a monster crush on him. On the negative side, all the boys are so jealous they want to kill him! And so do the girls he spurns because he only has eyes for one of them--the far-from-average vampire Moka.\n\nOn the plus side, Moka only has glowing red eyes for Tsukune. On the O-negative side, she also has a burning, unquenchable thirst for his blood...";

            Assert.AreEqual(expectedExcssiveLineBreakDesc, Series.ParseDescription(excessiveLineBreakDesc));
        }

        [Test]
        public void GetCorrectComicName_Test()
        {
            Assert.Multiple(() => {
                Assert.That("Manga", Is.EqualTo(Series.GetCorrectComicName("JP")));
                Assert.That("Manhwa", Is.EqualTo(Series.GetCorrectComicName("KR")));
                Assert.That("Manhua", Is.EqualTo(Series.GetCorrectComicName("CN")));
                Assert.That("Manfra", Is.EqualTo(Series.GetCorrectComicName("FR")));
                Assert.That("Error", Is.EqualTo(Series.GetCorrectComicName("USA")));
            });
        }

        [Test]
        public void GetSeriesStatus_Test()
        {
            Assert.Multiple(() => {
                Assert.That("Ongoing", Is.EqualTo(Series.GetSeriesStatus("RELEASING")));
                Assert.That("Ongoing", Is.EqualTo(Series.GetSeriesStatus("NOT_YET_RELEASED")));
                Assert.That("Complete", Is.EqualTo(Series.GetSeriesStatus("FINISHED")));
                Assert.That("Cancelled", Is.EqualTo(Series.GetSeriesStatus("CANCELLED")));
                Assert.That("Hiatus", Is.EqualTo(Series.GetSeriesStatus("HIATUS")));
                Assert.That("Error", Is.EqualTo(Series.GetSeriesStatus("UNICORN")));
            });
        }

        [Test] // Testing with Bakemonogatari
        public void GetSeriesStaff_ToManyIllustrators_Test()
        {
            Assert.Multiple(() => {
                // Testing Full/Native Staff
                Assert.That("大暮維人 | 西尾維新 | 戴源亨 | 渡辺明夫", Is.EqualTo(Series.GetSeriesStaff(bakemonogatariStaffQuery, "native", "Manga", "Bakemonogatari")));

                // Testing Romaji/English/Non-Native Staff
                Assert.That("Ito Ogure | NISIOISIN | VOFAN | Akio Watanabe", Is.EqualTo(Series.GetSeriesStaff(bakemonogatariStaffQuery, "full", "Manga", "Bakemonogatari")));
            });
        }
    }
}