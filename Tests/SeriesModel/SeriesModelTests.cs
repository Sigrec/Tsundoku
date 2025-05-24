using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;

[assembly: Description("Testing Series Model from Tsundoku")]
namespace Tsundoku.Tests
{
    [Author("Sean (Alias -> Prem or Sigrec)")]
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
                Console.WriteLine("Deleted Covers Folder");
                Directory.Delete(@"Covers", true);
            }
        }

        [AvaloniaTest]
        [Ignore("Weird Entry, not working")]
        public async Task NotOnMangaDex_AniList_Test()
        {
            Assert.That((await Series.CreateNewSeriesCardAsync("-0.5˚C", Constants.Format.Manga, 5, 0, [])).ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesModel\SeriesModelTestData\0.5C.json")));
        }

        [AvaloniaTest]
        public async Task Novel_AniList_Test()
        {
            Assert.That((await Series.CreateNewSeriesCardAsync("Classroom of the Elite", Constants.Format.Novel, 14, 0, [], "Unknown", Constants.Demographic.Seinen, 4, (decimal)5.6, (decimal)5000.65)).ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesModel\SeriesModelTestData\CoTE.json")));
        }

        [AvaloniaTest]
        public async Task MangaDexID_HasNativeStaff_Test()
        {
            Series series = await Series.CreateNewSeriesCardAsync("32d76d19-8a05-4db0-9fc2-e0b0648fe9d0", Constants.Format.Manga, 2, 0, []);
            Assert.That(series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesModel\SeriesModelTestData\SoloLeveling.json")));
        }

        [AvaloniaTest]
        public async Task MangaDexID_NoNativeStaff_Test()
        {
            Series series  = await Series.CreateNewSeriesCardAsync("c0189f4a-dee6-48f4-abbe-53a4359cbcfb", Constants.Format.Manga, 2, 0, []);
            Assert.That(series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesModel\SeriesModelTestData\AtTheNorthernFort.json")));
        }

        [AvaloniaTest]
        public async Task MultipleAdditionalLangTitle_AniListLink_Test()
        {
            Assert.That((await Series.CreateNewSeriesCardAsync("Rent-A-Girlfriend", Constants.Format.Manga, 18, 0, ["Arabic", "Chinese", "French", "Korean", "Russian", "Spanish"])).ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesModel\SeriesModelTestData\Rent-A-Girlfriend.json")));
        }

        [AvaloniaTest]
        public async Task SimilarNotEquals_AniList_Test()
        {
            Assert.That((await Series.CreateNewSeriesCardAsync("dont toy with me miss nagatoro", Constants.Format.Manga, 5, 0, [])).ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesModel\SeriesModelTestData\IjiranaideNagatoro-san.json")));
        }

        [AvaloniaTest]
        public async Task Radiant_MangaDexTitle_Test()
        {
            Series series = await Series.CreateNewSeriesCardAsync("Radiant", Constants.Format.Manga, 18, 0, []);
            Assert.That(series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesModel\SeriesModelTestData\Radiant.json")));
        }

        [AvaloniaTest]
        public async Task TheBeginningAfterTheEnd_NoDemographic_MangaDexTitle_Test()
        {
            Series series = await Series.CreateNewSeriesCardAsync("The Beginning After The End", Constants.Format.Manga, 5, 0, []);
            Assert.That(series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tsundoku\Tests\SeriesModel\SeriesModelTestData\TBATE.json")));
        }

        [Test]
        public async Task GetJapaneseAltTitle_Test()
        {
            var data = (await MangaDex.GetSeriesByIdAsync("32d76d19-8a05-4db0-9fc2-e0b0648fe9d0")).RootElement.GetProperty("data");
            if (data.ValueKind == JsonValueKind.Array)
            {
                Assert.That(Series.GetAltTitle("ja", data.EnumerateArray().ElementAt(0).GetProperty("attributes").GetProperty("altTitles").EnumerateArray()), Is.EqualTo("俺だけレベルアップな件"));
            }
            else
            {
                Assert.That(Series.GetAltTitle("ja", data.GetProperty("attributes").GetProperty("altTitles").EnumerateArray()), Is.EqualTo("俺だけレベルアップな件"));
            }
        }

        [Test]
        public void IdenticalSeriesNames_GetSeriesByID_Test()
        {
            Assert.Multiple(async () =>
            {
                Assert.That((await AniList.GetSeriesByIDAsync(98282, Constants.Format.Manga, 1)).RootElement.GetProperty("Media").GetProperty("title").GetProperty("romaji").GetString(), Is.EqualTo("Getsuyoubi no Tawawa"));
                Assert.That((await AniList.GetSeriesByIDAsync(125854, Constants.Format.Manga, 1)).RootElement.GetProperty("Media").GetProperty("title").GetProperty("romaji").GetString(), Is.EqualTo("Getsuyoubi no Tawawa"));
            });
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_AniList_Note_Test()
        {
            Assert.That(AniList.ParseAniListDescription("A plane is seen crashing with black smoke billowing out of it. It seems to have been caused by a \"Choujin\". However, strangely enough, the plane was not damaged and there were 200 survivors. High school students Kurobara Tokio and Higashi Azuma are on their way home from volunteering to help with the accident when they are suddenly tangled up with a delinquent they have a history with... but there is something different about him.\n<br><br>\n<i>Note: The manga chapters are irregularly released on Tonari no Young Jump. At the same time, it received a serialization in Weekly Young Jump, starting from chapter 1. Some chapters receive a version with fewer pages in the magazine, also counting with additions of new pages in certain situations. After chapter 15, the manga went back to being exclusive to Tonari no YJ, ending the serialization in the magazine."), Is.EqualTo("A plane is seen crashing with black smoke billowing out of it. It seems to have been caused by a \"Choujin\". However, strangely enough, the plane was not damaged and there were 200 survivors. High school students Kurobara Tokio and Higashi Azuma are on their way home from volunteering to help with the accident when they are suddenly tangled up with a delinquent they have a history with... but there is something different about him."));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_WinnerOfEnd_Test()
        {
            // https://mangadex.org/title/b6fed5d7-9021-4ff5-be9d-74c4925152e7/-hitogatana
            Assert.That(MangaDex.ParseMangadexDescription("As an UNLUCKY girl prepares to face her death, an UNDEAD who desperately wants to die appears before her! Vicious, violent and buck naked! An unprecedented picaresque hero appears in Shonen Jump!  \n  \n- Winner of the 2020 Tsugimanga award."), Is.EqualTo("As an UNLUCKY girl prepares to face her death, an UNDEAD who desperately wants to die appears before her! Vicious, violent and buck naked! An unprecedented picaresque hero appears in Shonen Jump!"));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_Brackets_Test()
        {
            // https://mangadex.org/title/b6fed5d7-9021-4ff5-be9d-74c4925152e7/-hitogatana
            Assert.That(MangaDex.ParseMangadexDescription("Crimes commited using manned combat-androids dubbed \"Katana\" run rampant. In an effort to maintain order, the government has organized the AKCD - \"Anti Katana Crime Division.\" Togusa of the 8th squad of the AKCD, while holding existential doubts as a Human-Katana hybrid, continually casts himself into battle…  \n  \n [Official Korean](https://ridibooks.com/books/845012944)  \n [Official Traditional Chinese](https://www.books.com.tw/products/0010625775)\n\n[Wikipedia](https://ja.wikipedia.org/wiki/-%E3%83%92%E3%83%88%E3%82%AC%E3%82%BF%E3%83%8A-)"), Is.EqualTo("Crimes commited using manned combat-androids dubbed \"Katana\" run rampant. In an effort to maintain order, the government has organized the AKCD - \"Anti Katana Crime Division.\" Togusa of the 8th squad of the AKCD, while holding existential doubts as a Human-Katana hybrid, continually casts himself into battle…"));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_Test()
        {
            Assert.That(MangaDex.ParseMangadexDescription("Seth is an apprentice sorcerer from the Pompo Hills. Like all sorcerers, he\u0027s an \u0022infected\u0022, one of the few people having survived an encounter with a Nemesis, those creatures falling from the sky and destroying everything around them. Being immune to them, Seth wants to become a Hunter and fight the Nemesis. But what Seth really wants is to find the source of all Nemesis, the Radiant. Helped by his fellow sorcerers, he will seek the Radiant, under the harsh scrutiny of the Inquisition\u2026\n\n\n---\nOfficial Release:\n- Indonesian release by Elex Media (2021)"), Is.EqualTo("Seth is an apprentice sorcerer from the Pompo Hills. Like all sorcerers, he\u0027s an \u0022infected\u0022, one of the few people having survived an encounter with a Nemesis, those creatures falling from the sky and destroying everything around them. Being immune to them, Seth wants to become a Hunter and fight the Nemesis. But what Seth really wants is to find the source of all Nemesis, the Radiant. Helped by his fellow sorcerers, he will seek the Radiant, under the harsh scrutiny of the Inquisition\u2026"));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_TripleBreak_Test()
        {
            Assert.That(MangaDex.ParseMangadexDescription("Workaholic assistant Atobe Arihito fell asleep in his company's bus that was traveling for a vacation. When he awoke he learned he had died due to the bus getting into an accident, and that he had reincarnated into a world of reincarnators - to the Labyrinth Country. To his surprise he was discovered by his slave driving manager, Igarashi Kyouka, who further explains their role in this world.  \nIn Labyrinth Country, Reincarnators are expected to choose a job and form parties to fight against monsters in Labyrinths. \"Valkyrie\" Kyouka had expected Arihito to work with her once again, but he declines due to his knowledge of her demanding nature. As they depart she vows to make him regret not choosing her. And thus starts Arihito's adventure as a \"Rear Guard\".  \n\n\n---"), Is.EqualTo("Workaholic assistant Atobe Arihito fell asleep in his company's bus that was traveling for a vacation. When he awoke he learned he had died due to the bus getting into an accident, and that he had reincarnated into a world of reincarnators - to the Labyrinth Country. To his surprise he was discovered by his slave driving manager, Igarashi Kyouka, who further explains their role in this world.  \nIn Labyrinth Country, Reincarnators are expected to choose a job and form parties to fight against monsters in Labyrinths. \"Valkyrie\" Kyouka had expected Arihito to work with her once again, but he declines due to his knowledge of her demanding nature. As they depart she vows to make him regret not choosing her. And thus starts Arihito's adventure as a \"Rear Guard\"."));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_SurroundedBreak_Test()
        {
            Assert.That(MangaDex.ParseMangadexDescription("10 years ago, after \u201Cthe Gate\u201D that connected the real world with the monster world opened, some of the ordinary, everyday people received the power to hunt monsters within the Gate. They are known as \u201CHunters\u201D. However, not all Hunters are powerful. My name is Sung Jin-Woo, an E-rank Hunter. I\u2019m someone who has to risk his life in the lowliest of dungeons, the \u201CWorld\u2019s Weakest\u201D. Having no skills whatsoever to display, I barely earned the required money by fighting in low-leveled dungeons\u2026 at least until I found a hidden dungeon with the hardest difficulty within the D-rank dungeons! In the end, as I was accepting death, I suddenly received a strange power, a quest log that only I could see, a secret to leveling up that only I know about! If I trained in accordance with my quests and hunted monsters, my level would rise. Changing from the weakest Hunter to the strongest S-rank Hunter!\n\n**Links:**\n\n- Official English Translation [\u003CPocket Comics\u003E](https://www.pocketcomics.com/comic/320) | [\u003CWebNovel\u003E](https://www.webnovel.com/comic/only-i-level-up-(solo-leveling)_15227640605485101) | [\u003CTapas\u003E](https://tapas.io/series/solo-leveling-comic/info)"), Is.EqualTo("10 years ago, after \u201Cthe Gate\u201D that connected the real world with the monster world opened, some of the ordinary, everyday people received the power to hunt monsters within the Gate. They are known as \u201CHunters\u201D. However, not all Hunters are powerful. My name is Sung Jin-Woo, an E-rank Hunter. I\u2019m someone who has to risk his life in the lowliest of dungeons, the \u201CWorld\u2019s Weakest\u201D. Having no skills whatsoever to display, I barely earned the required money by fighting in low-leveled dungeons\u2026 at least until I found a hidden dungeon with the hardest difficulty within the D-rank dungeons! In the end, as I was accepting death, I suddenly received a strange power, a quest log that only I could see, a secret to leveling up that only I know about! If I trained in accordance with my quests and hunted monsters, my level would rise. Changing from the weakest Hunter to the strongest S-rank Hunter!"));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_ValidDoubleBreak_Test()
        {
            Assert.That(MangaDex.ParseMangadexDescription("King Grey has unrivaled strength, wealth, and prestige in a world governed by martial ability. However, solitude lingers closely behind those with great power. Beneath the glamorous exterior of a powerful king lurks the shell of man, devoid of purpose and will.\n\nReincarnated into a new world filled with magic and monsters, the king has a second chance to relive his life. Correcting the mistakes of his past will not be his only challenge, however. Underneath the peace and prosperity of the new world is an undercurrent threatening to destroy everything he has worked for, questioning his role and reason for being born again.\n\n\n---\n**Contents:** 1 (Prologue) \u002B 24 (S1) \u002B 30 (S2) \u002B 28 (S3) \u002B 39 (S4) \u002B 49 (S5) = 172 chapters\n- **Season 1:** ch. 0-25\n- **Season 2:** ch. 26-56\n- **Season 3:** ch. 57-85\n- **Season 4:** ch. 86-125\n- **Season 5:** ch. 126-175\n\n---\n\n**Links:**\n- [Official Chinese (Simp) Translation](https://www.kuaikanmanhua.com/web/topic/6884/)\n- [Official Japanese Translation](https://piccoma.com/web/product/19682)\n- [Official Korean Translation](https://page.kakao.com/home?seriesId=53142176)\n- [Official Thai Translation](http://www.comico.in.th/titles/1171)\n- [Official French Translation](https://www.delitoon.com/webtoon/beginning-after-the-end)\n- [Official TBATE Discord Server](https://discord.gg/ZJHAnzF)\n- [TurtleMe Patreon Page](https://www.patreon.com/TurtleMe)"), Is.EqualTo("King Grey has unrivaled strength, wealth, and prestige in a world governed by martial ability. However, solitude lingers closely behind those with great power. Beneath the glamorous exterior of a powerful king lurks the shell of man, devoid of purpose and will.\n\nReincarnated into a new world filled with magic and monsters, the king has a second chance to relive his life. Correcting the mistakes of his past will not be his only challenge, however. Underneath the peace and prosperity of the new world is an undercurrent threatening to destroy everything he has worked for, questioning his role and reason for being born again."));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_Both_Test()
        {
            Assert.That(MangaDex.ParseMangadexDescription("Главный герой — трудоголик с большим стажем, он и его босс попадают в аварию, после чего они обнаруживают, что переместились в другой мир. Главный герой не хочет оставаться с начальницей, решая отправиться на приключения в одиночестве. Куда же его приведут эти странствия и встретятся ли они вновь?..   \n\n\n---\n\n**Links:** [Alternate Raw](https://comic-walker.com/contents/detail/KDCW_MF00000068010000_68/)"), Is.EqualTo("Главный герой — трудоголик с большим стажем, он и его босс попадают в аварию, после чего они обнаруживают, что переместились в другой мир. Главный герой не хочет оставаться с начальницей, решая отправиться на приключения в одиночестве. Куда же его приведут эти странствия и встретятся ли они вновь?.."));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_Links_Test()
        {
            Assert.That(MangaDex.ParseMangadexDescription("Kinoshita Kazuya is a regular college student who was just dumped by his girlfriend for another guy. Feeling down in the dumps, he decides to use an app called Diamond to hire Mizuhara Chizuru, a rental girlfriend, to make himself feel better. From their first meeting, she seems to be the perfect girl for him, but is there more to her than meets the eye? And how will their not quite typical relationship develop?\n\n---\n**Links:**\n\n- Alternative Official English - [K MANGA](https://kmanga.kodansha.com/title/10001/episode/312699) (U.S. Only), [Kodansha](https://kodansha.us/series/rent-a-girlfriend/)"), Is.EqualTo("Kinoshita Kazuya is a regular college student who was just dumped by his girlfriend for another guy. Feeling down in the dumps, he decides to use an app called Diamond to hire Mizuhara Chizuru, a rental girlfriend, to make himself feel better. From their first meeting, she seems to be the perfect girl for him, but is there more to her than meets the eye? And how will their not quite typical relationship develop?"));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_Link_CarriageReturn_Test()
        {
            Assert.That(MangaDex.ParseMangadexDescription("Since ancient times, rumors have abounded of man-eating demons lurking in the woods. Because of this, the local townsfolk never venture outside at night. Legend has it that a demon slayer also roams the night, hunting down these bloodthirsty demons.  \r\nEver since the death of his father, Tanjirou has taken it upon himself to support his mother and five siblings. Although their lives may be hardened by tragedy, they've found happiness. But that ephemeral warmth is shattered one day when Tanjirou finds his family slaughtered and the lone survivor, his sister Nezuko, turned into a demon. Adding to this sorrow, a demon hunter named Tomioka Giyuu arrived and was about to finish Nezuko off, but to his surprise she and Tanjiro started to protect each other. Seeing this oddity and Tanjiro's promising fighting skills, Giyuu decides to send them to his old mentor to be trained.  \r\nSo begins Tanjiro's life as a demon hunter, bound on a quest to cure his sister and find the one who murdered his entire family.  \r\n  \r\nMangaPlus: <https://mangaplus.shueisha.co.jp/titles/100009>"), Is.EqualTo("Since ancient times, rumors have abounded of man-eating demons lurking in the woods. Because of this, the local townsfolk never venture outside at night. Legend has it that a demon slayer also roams the night, hunting down these bloodthirsty demons.  \r\nEver since the death of his father, Tanjirou has taken it upon himself to support his mother and five siblings. Although their lives may be hardened by tragedy, they've found happiness. But that ephemeral warmth is shattered one day when Tanjirou finds his family slaughtered and the lone survivor, his sister Nezuko, turned into a demon. Adding to this sorrow, a demon hunter named Tomioka Giyuu arrived and was about to finish Nezuko off, but to his surprise she and Tanjiro started to protect each other. Seeing this oddity and Tanjiro's promising fighting skills, Giyuu decides to send them to his old mentor to be trained.  \r\nSo begins Tanjiro's life as a demon hunter, bound on a quest to cure his sister and find the one who murdered his entire family."));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_MangaDex_NewLineWithExtra_Test()
        {
            Assert.That(MangaDex.ParseMangadexDescription("There are six races in this world: human, beast, dragon, elf, dwarf, and demon. Humans are discriminated against and looked down on as the most inferior race. The \"Gathering of Races\" was a party with members from each race gathered to dispel such discrimination, but they also had a hidden purpose. They were ordered by the non-human countries to search for the human \"Master,\" and take it to their own country.\n\nLight, a human boy with the gift \"Infinite Gacha,\" was welcomed into the Gathering of Races as he was suspected of being a Master. But in the end, their investigation showed that Light was not a Master. Just as Light was about to be killed by his teammates, he accidentally stepped on a transfer trap and was teleported to the lowest level of the dungeon.\n\nIn the deepest dungeon, he was attacked by a mythical level 1000 demon. In a desperate situation, Light used \"Infinite Gacha\" repeatedly. Luckily, he draws the SUR level 9999 card \"Mei the Seeker Maid.\" Mei helped Light repel the demons and saved his life.\n\nAfter finally escaping from the crisis, Light decides to take revenge on his former comrades. Mei advises him to remain in the dungeon and use Infinite Gacha to increase his number of trustworthy friends. Light agreed began building up the power necessary to oppose the non-human nations.\n\nThree years later, Light had built the strongest nation in the depths of the dungeon. Once again, he will return to the surface to take revenge on his former comrades and search for the truth that nearly killed him.\n___\n[Alt Official English (US ONLY)](https://kmanga.kodansha.com/title/10040/episode/345702)"), Is.EqualTo("There are six races in this world: human, beast, dragon, elf, dwarf, and demon. Humans are discriminated against and looked down on as the most inferior race. The \"Gathering of Races\" was a party with members from each race gathered to dispel such discrimination, but they also had a hidden purpose. They were ordered by the non-human countries to search for the human \"Master,\" and take it to their own country.\n\nLight, a human boy with the gift \"Infinite Gacha,\" was welcomed into the Gathering of Races as he was suspected of being a Master. But in the end, their investigation showed that Light was not a Master. Just as Light was about to be killed by his teammates, he accidentally stepped on a transfer trap and was teleported to the lowest level of the dungeon.\n\nIn the deepest dungeon, he was attacked by a mythical level 1000 demon. In a desperate situation, Light used \"Infinite Gacha\" repeatedly. Luckily, he draws the SUR level 9999 card \"Mei the Seeker Maid.\" Mei helped Light repel the demons and saved his life.\n\nAfter finally escaping from the crisis, Light decides to take revenge on his former comrades. Mei advises him to remain in the dungeon and use Infinite Gacha to increase his number of trustworthy friends. Light agreed began building up the power necessary to oppose the non-human nations.\n\nThree years later, Light had built the strongest nation in the depths of the dungeon. Once again, he will return to the surface to take revenge on his former comrades and search for the truth that nearly killed him."));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_AniList_UnnecessaryBreakRemoval_Test()
        {
            Assert.Multiple(() =>
            {
                Assert.That(AniList.ParseAniListDescription("<i>Serialisation of the original doujin</i><br><br>\n\nSaito's never been anyone special, but his unremarkable path takes a turn when he wakes up in another world. After all, who other than the handyman could be trusted to open locked treasure chests or to repair equipment?\n<br><br>\n(Source: Yen Press)"), Is.EqualTo("Serialisation of the original doujin\n\nSaito's never been anyone special, but his unremarkable path takes a turn when he wakes up in another world. After all, who other than the handyman could be trusted to open locked treasure chests or to repair equipment?"));

                Assert.That(AniList.ParseAniListDescription("After a fierce battle between humans and vampires, a temporary peace was established, but Kaname continued to sleep within a coffin of ice… Yuki gave Kaname her heart to revive him as a human being.\n<br><br>\nThese are the stories of what happened during those 1,000 years of Kaname’s slumber and at the start of his human life.\n<br><br>\n(Source: Viz Media)"), Is.EqualTo("After a fierce battle between humans and vampires, a temporary peace was established, but Kaname continued to sleep within a coffin of ice\u2026 Yuki gave Kaname her heart to revive him as a human being.\n\nThese are the stories of what happened during those 1,000 years of Kaname\u2019s slumber and at the start of his human life."));
            });
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_AniList_BR_And_Ampersand_Test()
        {
            // Baki description to check for <br><br> & ampersand string for test
            string amerpsandAndBrDesc = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time&hellip; To lose is to die.\n<br><br>Hanma Baki (named &ldquo;Wild Fang&rdquo; by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father&rsquo;s command. For he must obey his father&rsquo;s rule that at his &ldquo;coming of age&rdquo; Baki surpass his own father, Hanma Yuujiro, &ldquo;the most powerful creature walking on earth.&rdquo; Baki&rsquo;s life has been nothing but trouble. This has given Baki a wild nature and convinced him that &ldquo;to be the most powerful&rdquo; is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become &ldquo;the strongest man on earth.&rdquo; That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever.  ";

            string expectedAmersandAndBrDesc = "BAKI: THE SEARCH OF OUR STRONGEST HERO is the official name of the 4th saga in which Baki will find strongest enemies in his path, and the way to challenge his dad once again, and this time… To lose is to die.\n\nHanma Baki (named “Wild Fang” by his father) is a boy born under an unlucky star. Since the day of his birth, Baki has been rigorously training at all kinds of martial arts, strengthening himself at his father’s command. For he must obey his father’s rule that at his “coming of age” Baki surpass his own father, Hanma Yuujiro, “the most powerful creature walking on earth.” Baki’s life has been nothing but trouble. This has given Baki a wild nature and convinced him that “to be the most powerful” is his way of life. Tenacious and fearless under all adverse conditions, Baki continues his training looking for new masters and new challenges to strengthen his personality and become “the strongest man on earth.” That road is a lonely one and Baki experienced the cruelty of his sick dad in a move that would change his life forever.";

            Assert.That(AniList.ParseAniListDescription(amerpsandAndBrDesc), Is.EqualTo(expectedAmersandAndBrDesc));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_AniList_ExcessiveLineBreaks_Test()
        {
            string excessiveLineBreakDesc = "Average human teenage boy Tsukune accidentally enrolls at a boarding school for monsters--no, not jocks and popular kids, but bona fide werewolves, witches, and unnameables out of his wildest nightmares!<br><br>\nOn the plus side, all the girls have a monster crush on him. On the negative side, all the boys are so jealous they want to kill him! And so do the girls he spurns because he only has eyes for one of them--the far-from-average vampire Moka.<br><br>\nOn the plus side, Moka only has glowing red eyes for Tsukune. On the O-negative side, she also has a burning, unquenchable thirst for his blood...\n<br><br>\n\n(Source: Viz Media)";
            string expectedExcssiveLineBreakDesc = "Average human teenage boy Tsukune accidentally enrolls at a boarding school for monsters--no, not jocks and popular kids, but bona fide werewolves, witches, and unnameables out of his wildest nightmares!\n\nOn the plus side, all the girls have a monster crush on him. On the negative side, all the boys are so jealous they want to kill him! And so do the girls he spurns because he only has eyes for one of them--the far-from-average vampire Moka.\n\nOn the plus side, Moka only has glowing red eyes for Tsukune. On the O-negative side, she also has a burning, unquenchable thirst for his blood...";

            Assert.That(AniList.ParseAniListDescription(excessiveLineBreakDesc), Is.EqualTo(expectedExcssiveLineBreakDesc));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_AniList_HtmlAndRemoveSource_Test()
        {
            string htmlDesc = "In a world where awakened beings called “Hunters” must battle deadly monsters to protect humanity, Sung Jinwoo, nicknamed “the weakest hunter of all mankind,” finds himself in a constant struggle for survival. One day, after a brutal encounter in an overpowered dungeon wipes out his party and threatens to end his life, a mysterious System chooses him as its sole player: Jinwoo has been granted the rare opportunity to level up his abilities, possibly beyond any known limits. Follow Jinwoo’s journey as he takes on ever-stronger enemies, both human and monster, to discover the secrets deep within the dungeons and the ultimate extent of his powers.\n<br><br>\n(Source: Tappytoon)\n<br><br>\n<i>Note: Chapter count includes a prologue. </i>";

            string expectedHtmlDesc = "In a world where awakened beings called “Hunters” must battle deadly monsters to protect humanity, Sung Jinwoo, nicknamed “the weakest hunter of all mankind,” finds himself in a constant struggle for survival. One day, after a brutal encounter in an overpowered dungeon wipes out his party and threatens to end his life, a mysterious System chooses him as its sole player: Jinwoo has been granted the rare opportunity to level up his abilities, possibly beyond any known limits. Follow Jinwoo’s journey as he takes on ever-stronger enemies, both human and monster, to discover the secrets deep within the dungeons and the ultimate extent of his powers.";

            Assert.That(AniList.ParseAniListDescription(htmlDesc), Is.EqualTo(expectedHtmlDesc));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void ParseDescription_AniList_Brackets_Test()
        {
            string htmlDesc = "The master spy codenamed &lt;Twilight&gt; has spent his days on undercover missions, all for the dream of a better world. But one day, he receives a particularly difficult new order from command. For his mission, he must form a temporary family and start a new life?! A Spy/Action/Comedy about a one-of-a-kind family!<br><br>\n(Source: MANGA Plus)<br><br>\n<i>Notes:<br>\n- Includes 2 \"Extra Missions\" and 9 “Short Missions”.<br>\n- Nominated for the 24th Tezuka Osamu Cultural Prize in 2020.<br>\n- Nominated for the 13th and 14th Manga Taisho Award in 2020 and 2021.<br>\n- Nominated for the 44th Kodansha Manga Award in the Shounen Category in 2020.</i>";

            string expectedHtmlDesc = "The master spy codenamed <Twilight> has spent his days on undercover missions, all for the dream of a better world. But one day, he receives a particularly difficult new order from command. For his mission, he must form a temporary family and start a new life?! A Spy/Action/Comedy about a one-of-a-kind family!";

            Assert.That(AniList.ParseAniListDescription(htmlDesc), Is.EqualTo(expectedHtmlDesc));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void GetCorrectFormat_Test()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Series.GetCorrectFormat("JP"), Is.EqualTo(Constants.Format.Manga));
                Assert.That(Series.GetCorrectFormat("KR"), Is.EqualTo(Constants.Format.Manhwa));
                Assert.That(Series.GetCorrectFormat("CN"), Is.EqualTo(Constants.Format.Manhua));
                Assert.That(Series.GetCorrectFormat("TW"), Is.EqualTo(Constants.Format.Manhua));
                Assert.That(Series.GetCorrectFormat("FR"), Is.EqualTo(Constants.Format.Manfra));
                Assert.That(Series.GetCorrectFormat("EN"), Is.EqualTo(Constants.Format.Comic));
            });
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void GetSeriesStatus_Test()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Series.GetSeriesStatus("RELEASING"), Is.EqualTo(Constants.Status.Ongoing));
                Assert.That(Series.GetSeriesStatus("NOT_YET_RELEASED"), Is.EqualTo(Constants.Status.Ongoing));
                Assert.That(Series.GetSeriesStatus("ongoing"), Is.EqualTo(Constants.Status.Ongoing));
                Assert.That(Series.GetSeriesStatus("FINISHED"), Is.EqualTo(Constants.Status.Finished));
                Assert.That(Series.GetSeriesStatus("completed"), Is.EqualTo(Constants.Status.Finished));
                Assert.That(Series.GetSeriesStatus("CANCELLED"), Is.EqualTo(Constants.Status.Cancelled));
                Assert.That(Series.GetSeriesStatus("cancelled"), Is.EqualTo(Constants.Status.Cancelled));
                Assert.That(Series.GetSeriesStatus("HIATUS"), Is.EqualTo(Constants.Status.Hiatus));
                Assert.That(Series.GetSeriesStatus("hiatus"), Is.EqualTo(Constants.Status.Hiatus));
                Assert.That(Series.GetSeriesStatus("UNICORN"), Is.EqualTo(Constants.Status.Error));
            });
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void GetSeriesDemographics_Test()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Series.GetSeriesDemographic("shounen"), Is.EqualTo(Constants.Demographic.Shounen));
                Assert.That(Series.GetSeriesDemographic("Shounen"), Is.EqualTo(Constants.Demographic.Shounen));
                Assert.That(Series.GetSeriesDemographic("shoujo"), Is.EqualTo(Constants.Demographic.Shoujo));
                Assert.That(Series.GetSeriesDemographic("Shoujo"), Is.EqualTo(Constants.Demographic.Shoujo));
                Assert.That(Series.GetSeriesDemographic("josei"), Is.EqualTo(Constants.Demographic.Josei));
                Assert.That(Series.GetSeriesDemographic("Josei"), Is.EqualTo(Constants.Demographic.Josei));
                Assert.That(Series.GetSeriesDemographic("seinen"), Is.EqualTo(Constants.Demographic.Seinen));
                Assert.That(Series.GetSeriesDemographic("Seinen"), Is.EqualTo(Constants.Demographic.Seinen));
            });
        }

        [Test] // Testing with Bungou Stray Dogs
        [Parallelizable(scope: ParallelScope.Self)]
        public async Task GetSeriesStaff_UntrimmedRole_Test()
        {
            Assert.That(AniList.GetSeriesStaff((await AniList.GetSeriesByTitleAsync("文豪ストレイドッグス", Constants.Format.Manga, 1)).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges"), "full", Constants.Format.Manga, "Bungou Stray Dogs", new System.Text.StringBuilder()), Is.EqualTo("Kafka Asagiri | Harukawa35"));
        }

        [Test] // Testing with Bakemonogatari
        [Parallelizable(scope: ParallelScope.Self)]
        public async Task GetSeriesStaff_ToManyIllustrators_Test()
        {
            Assert.That(AniList.GetSeriesStaff((await AniList.GetSeriesByTitleAsync("化物語", Constants.Format.Manga, 1)).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges"), "full", Constants.Format.Manga, "Bakemonogatari", new System.Text.StringBuilder()), Is.EqualTo("Ito Oogure | NISIOISIN | VOFAN | Akio Watanabe"));
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public async Task GetSeriesStaff_MultplieStaffForValidRole_Test()
        {
            Assert.That(AniList.GetSeriesStaff((await AniList.GetSeriesByTitleAsync("나 혼자만 레벨업", Constants.Format.Manga, 1)).RootElement.GetProperty("Media").GetProperty("staff").GetProperty("edges"), "full", Constants.Format.Manga, "Na Honjaman Level Up", new System.Text.StringBuilder()), Is.EqualTo("Chu-Gong | So-Ryeong Gi | Hyeon-Gun | Seong-Rak Jang"));
        }

        [AvaloniaTest]
        public async Task GetSeriesStaff_Anthology_Test()
        {
            //Lycoris Recoil Koushiki Comic Anthology: Repeat
            Assert.That((await Series.CreateNewSeriesCardAsync("リコリス・リコイル 公式コミックアンソロジー リピート", Constants.Format.Manga, 5, 0, [])).Staff["Romaji"], Is.EqualTo("Takeshi Kojima | Mekimeki | Nyoijizai | GUNP | Itsuki Takano | Ren Sakuragi | sometime | Ryou Niina | Ginmoku | Mikaduchi | Nikomi Wakadori | Miki Morinaga | Raika Suzumi | Ree | Itsuki Tsutsui | Utashima | Shirou Urayama | Bonryuu | Yasuka Manuma | Yuichi | Marco Nii | Nana Komado | Yuu Kimura | Sugar.Kirikanoko | Atto | Kasumi Fukagawa | Tiv | Sou Hamayumiba | Kanari Abe | Nachi Aono | Koruse"));
        }

        [Test] // Tests if only native = null, onyl full = null, and both native and full are null
        [Parallelizable(scope: ParallelScope.Self)]
        public void GetSeriesStaff_AllNullStaffScenarios_Name_Test()
        {
            Assert.That(AniList.GetSeriesStaff(JsonDocument.Parse(File.ReadAllText(@"\Tsundoku\Tests\SeriesModel\SeriesModelTestData\staffNameTest.json")).RootElement.GetProperty("data").GetProperty("Media").GetProperty("staff").GetProperty("edges"), "full", Constants.Format.Novel, "86: Eighty Six", new System.Text.StringBuilder()), Is.EqualTo("Asato Asato | しらび | Ⅰ-Ⅳ"));
        }
    }
}