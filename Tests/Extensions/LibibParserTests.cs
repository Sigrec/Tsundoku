using System.Security.Policy;
using static Tsundoku.Helpers.LibibParser;
using static Tsundoku.Models.Enums.SeriesFormatEnum;

namespace Tsundoku.Tests.Extensions
{
    [TestFixture]
    [Parallelizable(scope: ParallelScope.All)]
    public class LibibParserTests
    {
        // Helper method to create a CSV string and return its file path
        private static string CreateTempCsvFile(string content)
        {
            string filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, content);
            return filePath;
        }

        [TearDown]
        public void Teardown()
        {
            // Clean up any temporary files created by tests
            string tempPath = Path.GetTempPath();
            string[] files = Directory.GetFiles(tempPath, "tmp*.tmp");
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                    // Ignore if file is in use, as it might be relevant for subsequent tests
                }
            }
        }

        [Test]
        public async Task ExtractUniqueTitles_NullFilePaths_ReturnsEmptyDictionary()
        {
            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(null);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ExtractUniqueTitles_EmptyFilePaths_ReturnsEmptyDictionary()
        {
            string[] emptyPaths = [];
            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(emptyPaths);
            Assert.That(result, Is.Null);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public async Task ExtractUniqueTitles_EmptyCsvFile_ReturnsEmptyDictionary()
        {
            string emptyCsvPath = CreateTempCsvFile("title,description,publisher\n"); // Just headers
            string[] csvPaths = [emptyCsvPath];
            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);
            Assert.That(result, Is.Null);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public async Task ExtractUniqueTitles_SingleValidEntry_ExtractsCorrectly()
        {
            string csvContent = "title,description,publisher\n" +
                                "My Awesome Manga Vol. 5,A great read,Viz Media";
            string csvPath = CreateTempCsvFile(csvContent);
            string[] csvPaths = [csvPath];

            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!, Has.Count.EqualTo(1));
            Assert.That(result!.ContainsKey(("My Awesome Manga", SeriesFormat.Manga, "Viz Media")), Is.True);
            Assert.That(result[("My Awesome Manga", SeriesFormat.Manga, "Viz Media")], Is.EqualTo(1));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public async Task ExtractUniqueTitles_MultipleFormats_DistinguishesCorrectly()
        {
            string csvContent = "title,description,publisher\n" +
                                "My Awesome Manga Vol. 5,A great read,Viz Media\n" +
                                "My Awesome Light Novel (LN),An amazing novel,J-Novel Club";
            string csvPath = CreateTempCsvFile(csvContent);
            string[] csvPaths = [csvPath];

            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);

            Assert.That(result!, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Contains.Key(("My Awesome Manga", SeriesFormat.Manga, "Viz Media")));
                Assert.That(result[("My Awesome Manga", SeriesFormat.Manga, "Viz Media")], Is.EqualTo(1));
                Assert.That(result, Contains.Key(("My Awesome Light Novel", SeriesFormat.Novel, "J-Novel Club")));
                Assert.That(result[("My Awesome Light Novel", SeriesFormat.Novel, "J-Novel Club")], Is.EqualTo(1));
            }
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public async Task ExtractUniqueTitles_TitleCleaning_RemovesVolumeAndNumbers()
        {
            string csvContent = "title,description,publisher\n" +
                                "Series A Volume 10,desc,pub\n" +
                                "Series B: Omnibus,desc,pub\n" +
                                "Series C (Light Novel) Vol. 3,desc,pub\n" +
                                "Series D, Complete,desc,pub\n" +
                                "Series E Season 2,desc,pub\n" +
                                "Series G (Another thing) Vol. 1,desc,pub\n" +
                                "Series H (Something else),desc,pub\n" + // Should remove (Something else)
                                "Series I, Vol 9,desc,pub\n" +
                                "Title with only a number 123,desc,pub\n" + // Number part at end should be removed
                                "Title with no number,desc,pub"; // No number at end, no change
            string csvPath = CreateTempCsvFile(csvContent);
            string[] csvPaths = [csvPath];

            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Contains.Key(("Series A", SeriesFormat.Manga, "pub")));
                Assert.That(result[("Series A", SeriesFormat.Manga, "pub")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Series B", SeriesFormat.Manga, "pub")));
                Assert.That(result[("Series B", SeriesFormat.Manga, "pub")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Series C", SeriesFormat.Novel, "pub")));
                Assert.That(result[("Series C", SeriesFormat.Novel, "pub")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Series D", SeriesFormat.Manga, "pub")));
                Assert.That(result[("Series D", SeriesFormat.Manga, "pub")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Series E", SeriesFormat.Manga, "pub")));
                Assert.That(result[("Series E", SeriesFormat.Manga, "pub")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Series G", SeriesFormat.Manga, "pub")));
                Assert.That(result[("Series G", SeriesFormat.Manga, "pub")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Series H", SeriesFormat.Manga, "pub")));
                Assert.That(result[("Series H", SeriesFormat.Manga, "pub")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Series I", SeriesFormat.Manga, "pub")));
                Assert.That(result[("Series I", SeriesFormat.Manga, "pub")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title with only a number", SeriesFormat.Manga, "pub")));
                Assert.That(result[("Title with only a number", SeriesFormat.Manga, "pub")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title with no number", SeriesFormat.Manga, "pub")));
                Assert.That(result[("Title with no number", SeriesFormat.Manga, "pub")], Is.EqualTo(1));
            }
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public async Task ExtractUniqueTitles_PublisherCleaning_RemovesSuffixes()
        {
            string csvContent = "title,description,publisher\n" +
                                "Title A,desc,VIZ Media, LLC\n" +
                                "Title B,desc,Kodansha USA\n" +
                                "Title C,desc,Dark Horse Comics\n" +
                                "Title D,desc,Vertical, Inc.\n" +
                                "Title E,desc,Yen Press, a division of Hachette Book Group\n" +
                                "Title F,desc,Seven Seas Entertainment\n" +
                                "Title G,desc,Ghost Ship\n" +
                                "Title H,desc,Titan Books\n" +
                                "Title I,desc,Ponent Mon S.L.\n" +
                                "Title J,desc,Square Enix Manga & Books";
            string csvPath = CreateTempCsvFile(csvContent);
            string[] csvPaths = [csvPath];

            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Contains.Key(("Title A", SeriesFormat.Manga, "Viz Media")));
                Assert.That(result[("Title A", SeriesFormat.Manga, "Viz Media")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title B", SeriesFormat.Manga, "Kodansha")));
                Assert.That(result[("Title B", SeriesFormat.Manga, "Kodansha")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title C", SeriesFormat.Manga, "Dark Horse")));
                Assert.That(result[("Title C", SeriesFormat.Manga, "Dark Horse")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title D", SeriesFormat.Manga, "Vertical")));
                Assert.That(result[("Title D", SeriesFormat.Manga, "Vertical")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title E", SeriesFormat.Manga, "Yen Press")));
                Assert.That(result[("Title E", SeriesFormat.Manga, "Yen Press")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title F", SeriesFormat.Manga, "Seven Seas")));
                Assert.That(result[("Title F", SeriesFormat.Manga, "Seven Seas")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title G", SeriesFormat.Manga, "Ghost Ship")));
                Assert.That(result[("Title G", SeriesFormat.Manga, "Ghost Ship")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title H", SeriesFormat.Manga, "Titan")));
                Assert.That(result[("Title H", SeriesFormat.Manga, "Titan")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title I", SeriesFormat.Manga, "Ponent Mon")));
                Assert.That(result[("Title I", SeriesFormat.Manga, "Ponent Mon")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title J", SeriesFormat.Manga, "Square Enix")));
                Assert.That(result[("Title J", SeriesFormat.Manga, "Square Enix")], Is.EqualTo(1));
            }
        }
        
        [Test]
        [Parallelizable(ParallelScope.None)]
        public async Task ExtractUniqueTitles_PublisherRemapping_AppliesCorrectly()
        {
            string csvContent = "title,description,publisher\n" +
                                "Title A,desc,Tokyopop\n" +
                                "Title B,desc,J-Novel\n" +
                                "Title C,desc,VIZ MEDIA LLC\n" + // Test case-insensitivity and suffix
                                "Title D,desc,Yen On\n" +
                                "Title E,desc,Denpa Books\n" +
                                "Title F,desc,ComicsOne Corporation";
            string csvPath = CreateTempCsvFile(csvContent);
            string[] csvPaths = [csvPath];

            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Contains.Key(("Title A", SeriesFormat.Manga, "TOKYOPOP")));
                Assert.That(result[("Title A", SeriesFormat.Manga, "TOKYOPOP")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title B", SeriesFormat.Manga, "J-Novel Club")));
                Assert.That(result[("Title B", SeriesFormat.Manga, "J-Novel Club")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title C", SeriesFormat.Manga, "Viz Media")));
                Assert.That(result[("Title C", SeriesFormat.Manga, "Viz Media")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title D", SeriesFormat.Manga, "Yen Press")));
                Assert.That(result[("Title D", SeriesFormat.Manga, "Yen Press")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title E", SeriesFormat.Manga, "DENPA")));
                Assert.That(result[("Title E", SeriesFormat.Manga, "DENPA")], Is.EqualTo(1));

                Assert.That(result, Contains.Key(("Title F", SeriesFormat.Manga, "ComicsOne")));
                Assert.That(result[("Title F", SeriesFormat.Manga, "ComicsOne")], Is.EqualTo(1));
            }
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public async Task ExtractUniqueTitles_HtmlDecode_AppliesCorrectly()
        {
            string csvContent = "title,description,publisher\n" +
                                "My &amp; Awesome Title &quot;Test&quot;,desc,Viz Media";
            string csvPath = CreateTempCsvFile(csvContent);
            string[] csvPaths = [csvPath];

            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);

            using (Assert.EnterMultipleScope())
            {
                (string Title, SeriesFormat Format, string Publisher) key = ("My & Awesome Title \"Test\"", SeriesFormat.Manga, "Viz Media");
                Assert.That(result, Contains.Key(key));
                Assert.That(result[key], Is.EqualTo(1));
            }
        }

        [Test]
        public async Task ExtractUniqueTitles_SkipsInvalidEntries()
        {
            string csvContent = "title,description,publisher\n" +
                                ",desc,pub\n" + // Empty title
                                "Title,description,\n" + // Empty publisher
                                "Title,desc,Title\n" + // Title contains publisher
                                "Valid Title,desc,Valid Publisher";
            string csvPath = CreateTempCsvFile(csvContent);
            string[] csvPaths = [csvPath];

            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);

            Assert.That(result, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Contains.Key(("Valid Title", SeriesFormat.Manga, "Valid Publisher")));
                Assert.That(result[("Valid Title", SeriesFormat.Manga, "Valid Publisher")], Is.EqualTo(1));
            }
        }

        [Test]
        public async Task ExtractUniqueTitles_DuplicateTitles_HandledByComparer()
        {
            string csvContent = "title,description,publisher\n" +
                                "Duplicate Title Vol. 1,desc,Pub1\n" +
                                "Duplicate Title: Vol. 2,desc,Pub2\n" + // Same cleaned title, same format
                                "Another Title (Light Novel),desc,Pub3\n" +
                                "Another Title (Novel),desc,Pub4"; // Same cleaned title, same format
            string csvPath = CreateTempCsvFile(csvContent);
            string[] csvPaths = [csvPath];

            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);

            Assert.That(result, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Contains.Key(("Duplicate Title", SeriesFormat.Manga, "Pub1")));
                Assert.That(result[("Duplicate Title", SeriesFormat.Manga, "Pub1")], Is.EqualTo(2));
                Assert.That(result, Does.ContainKey(("Duplicate Title", SeriesFormat.Manga, "Pub2")));

                Assert.That(result, Contains.Key(("Another Title", SeriesFormat.Novel, "Pub3")));
                Assert.That(result[("Another Title", SeriesFormat.Novel, "Pub3")], Is.EqualTo(2));
                Assert.That(result, Does.ContainKey(("Another Title", SeriesFormat.Novel, "Pub4")));
            }
        }

        [Test]
        [TestCase("Demon Slayer Complete Box Set", "Demon Slayer", SeriesFormat.Manga)]
        [TestCase("Bleach Box Set 1: Vols. 1-21", "Bleach", SeriesFormat.Manga)]
        [TestCase("Nichijou 15th Anniversary", "Nichijou", SeriesFormat.Manga)]
        [TestCase("Battle Angel Alita Deluxe Complete Series Box Set", "Battle Angel Alita", SeriesFormat.Manga)]
        [TestCase("Lone Wolf and Cub 6: Lanterns for the Dead", "Lone Wolf and Cub", SeriesFormat.Manga)]
        [TestCase("The Fox &amp; Little Tanuki, Volume 5", "The Fox & Little Tanuki", SeriesFormat.Manga)]
        [TestCase("The Demon Sword Master of Excalibur Academy, Vol. 8 (light Novel)", "The Demon Sword Master of Excalibur Academy", SeriesFormat.Novel)]
        [TestCase("Monster, Vol. 1: The Perfect Edition", "Monster", SeriesFormat.Manga)]
        [TestCase("BLAME!, 6", "BLAME!", SeriesFormat.Manga)]
        [TestCase("Blue Giant Omnibus Vols. 1-2", "Blue Giant", SeriesFormat.Manga)]
        [TestCase("Lone Wolf and Cub 19 : The Moon: in Our Hearts", "Lone Wolf and Cub", SeriesFormat.Manga)] // No regex match for this one to remove
        [TestCase("APOSIMZ, Volume 7", "APOSIMZ", SeriesFormat.Manga)]
        [TestCase("Freezing Vol. 15-16", "Freezing", SeriesFormat.Manga)]
        [TestCase("Don't Call It Mystery (Omnibus)", "Don't Call It Mystery", SeriesFormat.Manga)]
        [TestCase("The Rampage of Haruhi Suzumiya (The Haruhi Suzumiya Series)", "The Rampage of Haruhi Suzumiya", SeriesFormat.Manga)] // (.*) regex match
        [TestCase("Goblin Slayer Side Story: Year One", "Goblin Slayer Side Story: Year One", SeriesFormat.Manga)] // No regex match for this one to remove
        [TestCase("Durarara!! (LN)", "Durarara!!", SeriesFormat.Novel)]
        [TestCase("I Had That Same Dream Again (Novel)", "I Had That Same Dream Again", SeriesFormat.Novel)]
        [Parallelizable(ParallelScope.None)]
        public async Task ExtractUniqueTitles_ExampleTitles_CleanTitlesAndFormatsCorrectly(string rawTitle, string expectedCleanTitle, SeriesFormat expectedFormat)
        {
            string csvContent = $"title,description,publisher\n" +
                                $"{rawTitle},Some Description,Some Publisher";
            string csvPath = CreateTempCsvFile(csvContent);
            string[] csvPaths = [csvPath];

            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(csvPaths);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));

            // Retrieve the single item from the HashSet and assert its properties
            var actualEntry = result.Single();
            (string actualTitle, SeriesFormat actualFormat, _) = actualEntry.Key;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(actualTitle, Is.EqualTo(expectedCleanTitle), $"Title mismatch for raw title: '{rawTitle}'");
                Assert.That(actualFormat, Is.EqualTo(expectedFormat), $"Format mismatch for raw title: '{rawTitle}'");
                Assert.That(actualEntry.Value, Is.EqualTo(1), $"Volume count mismatch for raw title: '{rawTitle}'");
            }
        }
    }
}