using System.Text.Json;

[assembly: Description("Testing Series Model from Tsundoku")]
namespace Tsundoku.Tests.SeriesModel
{
    [Author("Sean (Alias -> Prem or Sigrec)")]
    [Parallelizable(scope: ParallelScope.All)]
    [TestOf(typeof(Series))]
    [Description("Testing Series Model")]
    public class SeriesModelTests
    {
        public static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        [OneTimeSetUp]
        public void Setup()
        {
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (Directory.Exists("Covers"))
            {
                Directory.Delete(@"Covers", true);
            }
        }

        [AvaloniaTest]
        public async Task Novel_AniList_Test()
        {
            Assert.That((await Series.CreateNewSeriesCardAsync("Classroom of the Elite", "NOVEL", 14, 0, [])).ToJsonString(options), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesTests\SeriesTestData\CoTE.json")));
        }

        [AvaloniaTest]
        public async Task MultipleAdditionalLangTitle_MangaDexID_AniListLink_Test()
        {
            Assert.That((await Series.CreateNewSeriesCardAsync("32fdfe9b-6e11-4a13-9e36-dcd8ea77b4e4", "MANGA", 18, 0, ["Arabic", "Chinese", "French", "Korean", "Russian", "Spanish"])).ToJsonString(options), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesTests\SeriesTestData\Rent-A-Girlfriend.json")));
        }

        [AvaloniaTest]
        public async Task SimilarNotEquals_AniList_Test()
        {
            Assert.That((await Series.CreateNewSeriesCardAsync("dont toy with me miss nagatoro", "MANGA", 5, 0, [])).ToJsonString(options), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesTests\SeriesTestData\IjiranaideNagatoro-san.json")));
        }

        [AvaloniaTest]
        public async Task Radiant_MangaDexTitle_Test()
        {
            Assert.That((await Series.CreateNewSeriesCardAsync("Radiant", "MANGA", 18, 0, [])).ToJsonString(options), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesTests\SeriesTestData\Radiant.json")));
        }

        [AvaloniaTest]
        public async Task TheBeginningAfterTheEnd_NoDemographic_MangaDexTitle_Test()
        {
            Assert.That((await Series.CreateNewSeriesCardAsync("The Beginning After The End", "MANGA", 5, 0, [])).ToJsonString(options), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesTests\SeriesTestData\TBATE.json")));
        }

        [Test]
        public async Task GetJapaneseAltTitle_Test()
        {
            Assert.That(Series.GetAltTitle("ja", (await MangadexQuery.GetSeriesByTitleAsync("나 혼자만 레벨업")).RootElement.GetProperty("data").EnumerateArray().ElementAt(0).GetProperty("attributes").GetProperty("altTitles").EnumerateArray()), Is.EqualTo("俺だけレベルアップな件"));
        }

        [Test]
        public async Task CreateCoverFilePath_Test()
        {
            JsonElement narutoQuery = (await AniListQuery.GetSeriesByTitleAsync("Naruto", "MANGA", 1)).RootElement.GetProperty("Media");

            Assert.That(Series.CreateCoverFilePath(narutoQuery.GetProperty("coverImage").GetProperty("extraLarge").GetString(), narutoQuery.GetProperty("title").GetProperty("romaji").GetString(), "MANGA", narutoQuery.GetProperty("synonyms").EnumerateArray(), Constants.Site.AniList), Is.EqualTo("Covers\\NARUTO_MANGA.jpg"));
        }

        [Test]
        public void IdenticalSeriesNames_GetSeriesByID_Test()
        {
            Assert.Multiple(async () =>
            {
                Assert.That((await AniListQuery.GetSeriesByIDAsync(98282, "MANGA", 1)).RootElement.GetProperty("Media").GetProperty("title").GetProperty("romaji").GetString(), Is.EqualTo("Getsuyoubi no Tawawa"));
                Assert.That((await AniListQuery.GetSeriesByIDAsync(125854, "MANGA", 1)).RootElement.GetProperty("Media").GetProperty("title").GetProperty("romaji").GetString(), Is.EqualTo("Getsuyoubi no Tawawa"));
            });
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_Test()
        {
            Assert.That(MangadexQuery.ParseMangadexDescription("Seth is an apprentice sorcerer from the Pompo Hills. Like all sorcerers, he\u0027s an \u0022infected\u0022, one of the few people having survived an encounter with a Nemesis, those creatures falling from the sky and destroying everything around them. Being immune to them, Seth wants to become a Hunter and fight the Nemesis. But what Seth really wants is to find the source of all Nemesis, the Radiant. Helped by his fellow sorcerers, he will seek the Radiant, under the harsh scrutiny of the Inquisition\u2026\n\n\n---\nOfficial Release:\n- Indonesian release by Elex Media (2021)"), Is.EqualTo("Seth is an apprentice sorcerer from the Pompo Hills. Like all sorcerers, he\u0027s an \u0022infected\u0022, one of the few people having survived an encounter with a Nemesis, those creatures falling from the sky and destroying everything around them. Being immune to them, Seth wants to become a Hunter and fight the Nemesis. But what Seth really wants is to find the source of all Nemesis, the Radiant. Helped by his fellow sorcerers, he will seek the Radiant, under the harsh scrutiny of the Inquisition\u2026"));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_TripleBreak_Test()
        {
            Assert.That(MangadexQuery.ParseMangadexDescription("Workaholic assistant Atobe Arihito fell asleep in his company's bus that was traveling for a vacation. When he awoke he learned he had died due to the bus getting into an accident, and that he had reincarnated into a world of reincarnators - to the Labyrinth Country. To his surprise he was discovered by his slave driving manager, Igarashi Kyouka, who further explains their role in this world.  \nIn Labyrinth Country, Reincarnators are expected to choose a job and form parties to fight against monsters in Labyrinths. \"Valkyrie\" Kyouka had expected Arihito to work with her once again, but he declines due to his knowledge of her demanding nature. As they depart she vows to make him regret not choosing her. And thus starts Arihito's adventure as a \"Rear Guard\".  \n\n\n---"), Is.EqualTo("Workaholic assistant Atobe Arihito fell asleep in his company's bus that was traveling for a vacation. When he awoke he learned he had died due to the bus getting into an accident, and that he had reincarnated into a world of reincarnators - to the Labyrinth Country. To his surprise he was discovered by his slave driving manager, Igarashi Kyouka, who further explains their role in this world.  \nIn Labyrinth Country, Reincarnators are expected to choose a job and form parties to fight against monsters in Labyrinths. \"Valkyrie\" Kyouka had expected Arihito to work with her once again, but he declines due to his knowledge of her demanding nature. As they depart she vows to make him regret not choosing her. And thus starts Arihito's adventure as a \"Rear Guard\"."));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_SurroundedBreak_Test()
        {
            Assert.That(MangadexQuery.ParseMangadexDescription("10 years ago, after \u201Cthe Gate\u201D that connected the real world with the monster world opened, some of the ordinary, everyday people received the power to hunt monsters within the Gate. They are known as \u201CHunters\u201D. However, not all Hunters are powerful. My name is Sung Jin-Woo, an E-rank Hunter. I\u2019m someone who has to risk his life in the lowliest of dungeons, the \u201CWorld\u2019s Weakest\u201D. Having no skills whatsoever to display, I barely earned the required money by fighting in low-leveled dungeons\u2026 at least until I found a hidden dungeon with the hardest difficulty within the D-rank dungeons! In the end, as I was accepting death, I suddenly received a strange power, a quest log that only I could see, a secret to leveling up that only I know about! If I trained in accordance with my quests and hunted monsters, my level would rise. Changing from the weakest Hunter to the strongest S-rank Hunter!\n\n**Links:**\n\n- Official English Translation [\u003CPocket Comics\u003E](https://www.pocketcomics.com/comic/320) | [\u003CWebNovel\u003E](https://www.webnovel.com/comic/only-i-level-up-(solo-leveling)_15227640605485101) | [\u003CTapas\u003E](https://tapas.io/series/solo-leveling-comic/info)"), Is.EqualTo("10 years ago, after \u201Cthe Gate\u201D that connected the real world with the monster world opened, some of the ordinary, everyday people received the power to hunt monsters within the Gate. They are known as \u201CHunters\u201D. However, not all Hunters are powerful. My name is Sung Jin-Woo, an E-rank Hunter. I\u2019m someone who has to risk his life in the lowliest of dungeons, the \u201CWorld\u2019s Weakest\u201D. Having no skills whatsoever to display, I barely earned the required money by fighting in low-leveled dungeons\u2026 at least until I found a hidden dungeon with the hardest difficulty within the D-rank dungeons! In the end, as I was accepting death, I suddenly received a strange power, a quest log that only I could see, a secret to leveling up that only I know about! If I trained in accordance with my quests and hunted monsters, my level would rise. Changing from the weakest Hunter to the strongest S-rank Hunter!"));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_ValidDoubleBreak_Test()
        {
            Assert.That(MangadexQuery.ParseMangadexDescription("King Grey has unrivaled strength, wealth, and prestige in a world governed by martial ability. However, solitude lingers closely behind those with great power. Beneath the glamorous exterior of a powerful king lurks the shell of man, devoid of purpose and will.\n\nReincarnated into a new world filled with magic and monsters, the king has a second chance to relive his life. Correcting the mistakes of his past will not be his only challenge, however. Underneath the peace and prosperity of the new world is an undercurrent threatening to destroy everything he has worked for, questioning his role and reason for being born again.\n\n\n---\n**Contents:** 1 (Prologue) \u002B 24 (S1) \u002B 30 (S2) \u002B 28 (S3) \u002B 39 (S4) \u002B 49 (S5) = 172 chapters\n- **Season 1:** ch. 0-25\n- **Season 2:** ch. 26-56\n- **Season 3:** ch. 57-85\n- **Season 4:** ch. 86-125\n- **Season 5:** ch. 126-175\n\n---\n\n**Links:**\n- [Official Chinese (Simp) Translation](https://www.kuaikanmanhua.com/web/topic/6884/)\n- [Official Japanese Translation](https://piccoma.com/web/product/19682)\n- [Official Korean Translation](https://page.kakao.com/home?seriesId=53142176)\n- [Official Thai Translation](http://www.comico.in.th/titles/1171)\n- [Official French Translation](https://www.delitoon.com/webtoon/beginning-after-the-end)\n- [Official TBATE Discord Server](https://discord.gg/ZJHAnzF)\n- [TurtleMe Patreon Page](https://www.patreon.com/TurtleMe)"), Is.EqualTo("King Grey has unrivaled strength, wealth, and prestige in a world governed by martial ability. However, solitude lingers closely behind those with great power. Beneath the glamorous exterior of a powerful king lurks the shell of man, devoid of purpose and will.\n\nReincarnated into a new world filled with magic and monsters, the king has a second chance to relive his life. Correcting the mistakes of his past will not be his only challenge, however. Underneath the peace and prosperity of the new world is an undercurrent threatening to destroy everything he has worked for, questioning his role and reason for being born again."));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_Both_Test()
        {
            Assert.That(MangadexQuery.ParseMangadexDescription("Главный герой — трудоголик с большим стажем, он и его босс попадают в аварию, после чего они обнаруживают, что переместились в другой мир. Главный герой не хочет оставаться с начальницей, решая отправиться на приключения в одиночестве. Куда же его приведут эти странствия и встретятся ли они вновь?..   \n\n\n---\n\n**Links:** [Alternate Raw](https://comic-walker.com/contents/detail/KDCW_MF00000068010000_68/)"), Is.EqualTo("Главный герой — трудоголик с большим стажем, он и его босс попадают в аварию, после чего они обнаруживают, что переместились в другой мир. Главный герой не хочет оставаться с начальницей, решая отправиться на приключения в одиночестве. Куда же его приведут эти странствия и встретятся ли они вновь?.."));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_UnnecessaryBreakRemoval_Test()
        {
            Assert.Multiple(() =>
            {
                Assert.That(AniListQuery.ParseAniListDescription("<i>Serialisation of the original doujin</i><br><br>\n\nSaito's never been anyone special, but his unremarkable path takes a turn when he wakes up in another world. After all, who other than the handyman could be trusted to open locked treasure chests or to repair equipment?\n<br><br>\n(Source: Yen Press)"), Is.EqualTo("Serialisation of the original doujin\n\nSaito's never been anyone special, but his unremarkable path takes a turn when he wakes up in another world. After all, who other than the handyman could be trusted to open locked treasure chests or to repair equipment?"));

                Assert.That(AniListQuery.ParseAniListDescription("After a fierce battle between humans and vampires, a temporary peace was established, but Kaname continued to sleep within a coffin of ice… Yuki gave Kaname her heart to revive him as a human being.\n<br><br>\nThese are the stories of what happened during those 1,000 years of Kaname’s slumber and at the start of his human life.\n<br><br>\n(Source: Viz Media)"), Is.EqualTo("After a fierce battle between humans and vampires, a temporary peace was established, but Kaname continued to sleep within a coffin of ice\u2026 Yuki gave Kaname her heart to revive him as a human being.\n\nThese are the stories of what happened during those 1,000 years of Kaname\u2019s slumber and at the start of his human life."));
            });
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_BR_And_Ampersand_Test()
        {
            // Baki description to check for <br><br> & ampersand string for test
            string amerpsandAndBrDesc = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time&hellip; To lose is to die.\n<br><br>Hanma Baki (named &ldquo;Wild Fang&rdquo; by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father&rsquo;s command. For he must obey his father&rsquo;s rule that at his &ldquo;coming of age&rdquo; Baki surpass his own father, Hanma Yuujiro, &ldquo;the most powerful creature walking on earth.&rdquo; Baki&rsquo;s life has been nothing but trouble. This has given Baki a wild nature and convinced him that &ldquo;to be the most powerful&rdquo; is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become &ldquo;the strongest man on earth.&rdquo; That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever.  ";

            string expectedAmersandAndBrDesc = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time… To lose is to die.\n\nHanma Baki (named “Wild Fang” by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father’s command. For he must obey his father’s rule that at his “coming of age” Baki surpass his own father, Hanma Yuujiro, “the most powerful creature walking on earth.” Baki’s life has been nothing but trouble. This has given Baki a wild nature and convinced him that “to be the most powerful” is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become “the strongest man on earth.” That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever.";

            Assert.That(AniListQuery.ParseAniListDescription(amerpsandAndBrDesc), Is.EqualTo(expectedAmersandAndBrDesc));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_ExcessiveLineBreaks_Test()
        {
            string excessiveLineBreakDesc = "Average human teenage boy Tsukune accidentally enrolls at a boarding school for monsters--no, not jocks and popular kids, but bona fide werewolves, witches, and unnameables out of his wildest nightmares!<br><br>\nOn the plus side, all the girls have a monster crush on him. On the negative side, all the boys are so jealous they want to kill him! And so do the girls he spurns because he only has eyes for one of them--the far-from-average vampire Moka.<br><br>\nOn the plus side, Moka only has glowing red eyes for Tsukune. On the O-negative side, she also has a burning, unquenchable thirst for his blood...\n<br><br>\n\n(Source: Viz Media)";
            string expectedExcssiveLineBreakDesc = "Average human teenage boy Tsukune accidentally enrolls at a boarding school for monsters--no, not jocks and popular kids, but bona fide werewolves, witches, and unnameables out of his wildest nightmares!\n\nOn the plus side, all the girls have a monster crush on him. On the negative side, all the boys are so jealous they want to kill him! And so do the girls he spurns because he only has eyes for one of them--the far-from-average vampire Moka.\n\nOn the plus side, Moka only has glowing red eyes for Tsukune. On the O-negative side, she also has a burning, unquenchable thirst for his blood...";

            Assert.That(AniListQuery.ParseAniListDescription(excessiveLineBreakDesc), Is.EqualTo(expectedExcssiveLineBreakDesc));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_Html_And_RemoveSource_Test()
        {
            string htmlDesc = "In a world where awakened beings called “Hunters” must battle deadly monsters to protect humanity, Sung Jinwoo, nicknamed “the weakest hunter of all mankind,” finds himself in a constant struggle for survival. One day, after a brutal encounter in an overpowered dungeon wipes out his party and threatens to end his life, a mysterious System chooses him as its sole player: Jinwoo has been granted the rare opportunity to level up his abilities, possibly beyond any known limits. Follow Jinwoo’s journey as he takes on ever-stronger enemies, both human and monster, to discover the secrets deep within the dungeons and the ultimate extent of his powers.\n<br><br>\n(Source: Tappytoon)\n<br><br>\n<i>Note: Chapter count includes a prologue. </i>";

            string expectedHtmlDesc = "In a world where awakened beings called “Hunters” must battle deadly monsters to protect humanity, Sung Jinwoo, nicknamed “the weakest hunter of all mankind,” finds himself in a constant struggle for survival. One day, after a brutal encounter in an overpowered dungeon wipes out his party and threatens to end his life, a mysterious System chooses him as its sole player: Jinwoo has been granted the rare opportunity to level up his abilities, possibly beyond any known limits. Follow Jinwoo’s journey as he takes on ever-stronger enemies, both human and monster, to discover the secrets deep within the dungeons and the ultimate extent of his powers.";

            Assert.That(AniListQuery.ParseAniListDescription(htmlDesc), Is.EqualTo(expectedHtmlDesc));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_Brackets_Test()
        {
            string htmlDesc = "The master spy codenamed &lt;Twilight&gt; has spent his days on undercover missions, all for the dream of a better world. But one day, he receives a particularly difficult new order from command. For his mission, he must form a temporary family and start a new life?! A Spy/Action/Comedy about a one-of-a-kind family!<br><br>\n(Source: MANGA Plus)<br><br>\n<i>Notes:<br>\n- Includes 2 \"Extra Missions\" and 9 “Short Missions”.<br>\n- Nominated for the 24th Tezuka Osamu Cultural Prize in 2020.<br>\n- Nominated for the 13th and 14th Manga Taisho Award in 2020 and 2021.<br>\n- Nominated for the 44th Kodansha Manga Award in the Shounen Category in 2020.</i>";

            string expectedHtmlDesc = "The master spy codenamed <Twilight> has spent his days on undercover missions, all for the dream of a better world. But one day, he receives a particularly difficult new order from command. For his mission, he must form a temporary family and start a new life?! A Spy/Action/Comedy about a one-of-a-kind family!";

            Assert.That(AniListQuery.ParseAniListDescription(htmlDesc), Is.EqualTo(expectedHtmlDesc));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void GetCorrectComicName_Test()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Series.GetCorrectComicName("JP"), Is.EqualTo("Manga"));
                Assert.That(Series.GetCorrectComicName("KR"), Is.EqualTo("Manhwa"));
                Assert.That(Series.GetCorrectComicName("CN"), Is.EqualTo("Manhua"));
                Assert.That(Series.GetCorrectComicName("TW"), Is.EqualTo("Manhua"));
                Assert.That(Series.GetCorrectComicName("FR"), Is.EqualTo("Manfra"));
                Assert.That(Series.GetCorrectComicName("EN"), Is.EqualTo("Comic"));
            });
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void GetSeriesStatus_Test()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Series.GetSeriesStatus("RELEASING"), Is.EqualTo("Ongoing"));
                Assert.That(Series.GetSeriesStatus("NOT_YET_RELEASED"), Is.EqualTo("Ongoing"));
                Assert.That(Series.GetSeriesStatus("ongoing"), Is.EqualTo("Ongoing"));
                Assert.That(Series.GetSeriesStatus("FINISHED"), Is.EqualTo("Finished"));
                Assert.That(Series.GetSeriesStatus("completed"), Is.EqualTo("Finished"));
                Assert.That(Series.GetSeriesStatus("CANCELLED"), Is.EqualTo("Cancelled"));
                Assert.That(Series.GetSeriesStatus("cancelled"), Is.EqualTo("Cancelled"));
                Assert.That(Series.GetSeriesStatus("HIATUS"), Is.EqualTo("Hiatus"));
                Assert.That(Series.GetSeriesStatus("hiatus"), Is.EqualTo("Hiatus"));
                Assert.That(Series.GetSeriesStatus("UNICORN"), Is.EqualTo("Error"));
            });
        }

        [Test] // Testing with Bungou Stray Dogs
        public async Task GetSeriesStaff_UntrimmedRole_Test()
        {
            Assert.That(Series.GetSeriesStaff((await AniListQuery.GetSeriesByTitleAsync("文豪ストレイドッグス", "MANGA", 1)).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges"), "full", "Manga", "Bungou Stray Dogs", new System.Text.StringBuilder()), Is.EqualTo("Kafka Asagiri | Harukawa35"));
        }

        [Test] // Testing with Bakemonogatari
        public async Task GetSeriesStaff_ToManyIllustrators_Test()
        {
            Assert.That(Series.GetSeriesStaff((await AniListQuery.GetSeriesByTitleAsync("化物語", "MANGA", 1)).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges"), "full", "Manga", "Bakemonogatari", new System.Text.StringBuilder()), Is.EqualTo("Ito Oogure | NISIOISIN | VOFAN | Akio Watanabe"));
        }

        [Test]
        public async Task GetSeriesStaff_MultplieStaffForValidRole_Test()
        {
            Assert.That(Series.GetSeriesStaff((await AniListQuery.GetSeriesByTitleAsync("나 혼자만 레벨업", "MANGA", 1)).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges"), "full", "Manga", "Na Honjaman Level Up", new System.Text.StringBuilder()), Is.EqualTo("Chu-Gong | So-Ryeong Gi | Hyeon-Gun | Seong-Rak Jang"));
        }

        [AvaloniaTest]
        public async Task GetSeriesStaff_Anthology_Test()
        {
            //Lycoris Recoil Koushiki Comic Anthology: Repeat
            Assert.That((await Series.CreateNewSeriesCardAsync("リコリス・リコイル 公式コミックアンソロジー リピート", "MANGA", 5, 0, [])).Staff["Romaji"], Is.EqualTo("Takeshi Kojima | Mekimeki | Nyoijizai | GUNP | Itsuki Takano | Ren Sakuragi | sometime | Ryou Niina | Ginmoku | Mikaduchi | Nikomi Wakadori | Miki Morinaga | Raika Suzumi | Ree | Itsuki Tsutsui | Utashima | Shirou Urayama | Bonryuu | Yasuka Manuma | Yuichi | Marco Nii | Nana Komado | Yuu Kimura | Sugar.Kirikanoko | Atto | Kasumi Fukagawa | Tiv | Sou Hamayumiba | Kanari Abe | Nachi Aono | Koruse"));
        }

        [Test] // Tests if only native = null, onyl full = null, and both native and full are null
        public void GetSeriesStaff_AllNullStaffScenarios_Name_Test()
        {
            Assert.That(Series.GetSeriesStaff(JsonDocument.Parse(File.ReadAllText(@"\Tsundoku\Tests\SeriesTests\SeriesTestData\staffNameTest.json")).RootElement.GetProperty("data").GetProperty("Media").GetProperty("staff").GetProperty("edges"), "full", "Novel", "86: Eighty Six", new System.Text.StringBuilder()), Is.EqualTo("Asato Asato | しらび | Ⅰ-Ⅳ"));
        }
    }
}