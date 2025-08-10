using static Tsundoku.Models.Enums.SeriesFormatModel;

namespace Tsundoku.Tests.Extensions.Goodreads;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public class GoodreadsParserTests
{
    private static readonly List<string> _files = [];

    private static string CreateTempCsvFile(string content)
    {
        string path = Path.Combine(Path.GetTempPath(), $"goodreads_{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content);
        _files.Add(path);
        return path;
    }

    [TearDown]
    public void Teardown()
    {
        foreach (string file in _files.ToArray())
        {
            try { if (File.Exists(file)) File.Delete(file); }
            catch { /* ignore */ }
            finally { _files.Remove(file); }
        }
    }

    [Test]
    public async Task ExtractUniqueTitles_NullFilePaths_ReturnsNull()
    {
        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles(null);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExtractUniqueTitles_EmptyFilePaths_ReturnsNull()
    {
        string[] paths = Array.Empty<string>();
        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles(paths);
        Assert.That(result, Is.Null);
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task ExtractUniqueTitles_EmptyCsvFile_ReturnsNull()
    {
        string csv = "Title,Binding,My Rating,Publisher,Private Notes\n";
        string path = CreateTempCsvFile(csv);

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles([path]);

        Assert.That(result, Is.Null);
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task ExtractUniqueTitles_SingleValidEntry_ExtractsAndCleans()
    {
        // Note the quoted Title because it contains a comma.
        string csv =
            "Title,Binding,My Rating,Publisher,Private Notes\n" +
            "\"My Awesome Title, Vol. 5\",Paperback,4,VIZ MEDIA LLC,Some note";
        string path = CreateTempCsvFile(csv);

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles([path]);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!, Has.Count.EqualTo(1));

        KeyValuePair<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint> kvp = result!.Single();
        (string title, SeriesFormat format, string publisher, decimal rating) = kvp.Key;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(title, Is.EqualTo("My Awesome Title"));
            Assert.That(format, Is.EqualTo(SeriesFormat.Manga));
            Assert.That(publisher, Is.EqualTo("Viz Media"));
            Assert.That(rating, Is.EqualTo(4m));
            Assert.That(kvp.Value, Is.EqualTo(1u));
        }
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task ExtractUniqueTitles_TitleParsing_CoversNewSuffixes_And_ByPrefix()
    {
        // Titles with commas are quoted per CSV rules.
        string csv =
            "Title,Binding,My Rating,Publisher,Private Notes\n" +
            "Series A: Subtitle,Paperback,0,Viz Media,..\n" +
            "\"Series B, Vol. 3\",Paperback,0,Viz Media,..\n" +
            "\"Series C Omnibus\",Paperback,0,Viz Media,..\n" +
            "Series D Deluxe Edition,Paperback,0,Viz Media,..\n" +
            "Series E Volume 10,Paperback,0,Viz Media,..\n" +
            "\"Series F, Tome 2\",Paperback,0,Viz Media,..\n" +
            "\"Series H, Master Edition\",Paperback,0,Viz Media,..\n" +
            "Series I Black Edition,Paperback,0,Viz Media,..\n" +
            "Series J [Limited Edition],Paperback,0,Viz Media,..\n" +
            "Series K 3 by John Smith,Paperback,0,Viz Media,..\n" +
            "Series L Box Set,Paperback,0,Viz Media,..\n" +
            "Series M 12,Paperback,0,Viz Media,..\n" +
            "Series N Complete Box Set,Paperback,0,Viz Media,..\n" +
            "Series O Perfect Collection,Paperback,0,Viz Media,..\n" +
            "Series P (Hardcover),Paperback,0,Viz Media,..\n" +
            "\"By Jane Doe - Series Q, Vol. 1\",Paperback,0,Viz Media,..\n" +
            "Series R - Side Story,Paperback,0,Viz Media,..\n";

        string path = CreateTempCsvFile(csv);

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles([path]);

        Assert.That(result, Is.Not.Null);
        // Each line should collapse to a unique cleaned title below
        Assert.That(result!, Has.Count.EqualTo(17));

        // Expected cleaned titles after parsing
        string[] expectedTitles =
        [
            "Series A",
            "Series B",
            "Series C",
            "Series D",
            "Series E",
            "Series F",
            "Series H",
            "Series I",
            "Series J",
            "Series K",
            "Series L",
            "Series M",
            "Series N",
            "Series O",
            "Series P",
            "Series Q", // from: "By Jane Doe - Series Q, Vol. 1"
            "Series R"  // hyphen suffix removed for non-By titles
        ];

        foreach (string t in expectedTitles)
        {
            bool exists = result!.Any(k => k.Key.Title == t);
            Assert.That(exists, Is.True, $"Missing cleaned title '{t}'");
        }
    }

    [Test]
    public async Task ExtractUniqueTitles_SkipsKindleBinding()
    {
        string csv =
            "Title,Binding,My Rating,Publisher,Private Notes\n" +
            "Should Skip,Kindle Edition,5,Viz Media,..\n" +
            "Should Keep,Paperback,5,Viz Media,..\n";
        string path = CreateTempCsvFile(csv);

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles([path]);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!, Has.Count.EqualTo(1));
        Assert.That(result!.Single().Key.Title, Is.EqualTo("Should Keep"));
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task ExtractUniqueTitles_PublisherRemapping_AndCleanup()
    {
        string csv =
            "Title,Binding,My Rating,Publisher,Private Notes\n" +
            "Viz Title,Paperback,0,VIZ Media, LLC,..\n" +
            "Dark Horse Title,Paperback,0,Dark Horse Comics,..\n" +
            "Kodan Title,Paperback,0,Kodansha USA,..\n" +
            "Tokyo Title,Paperback,0,Tokyopop,..\n" +
            "JNovel Title,Paperback,0,J-Novel,..\n" +
            "Vertical Title,Paperback,0,Vertical, Inc.,..\n" +
            "Kana Title,Paperback,0,Kana,..\n" +
            "Kodama Title,Paperback,0,Kodama,..\n";
        string path = CreateTempCsvFile(csv);

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles(new[] { path });

        Assert.That(result, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.Any(k => k.Key.Title == "Viz Title" && k.Key.Publisher == "Viz Media"));
            Assert.That(result!.Any(k => k.Key.Title == "Dark Horse Title" && k.Key.Publisher == "Dark Horse"));
            Assert.That(result!.Any(k => k.Key.Title == "Kodan Title" && k.Key.Publisher == "Kodansha"));
            Assert.That(result!.Any(k => k.Key.Title == "Tokyo Title" && k.Key.Publisher == "TOKYOPOP"));
            Assert.That(result!.Any(k => k.Key.Title == "JNovel Title" && k.Key.Publisher == "J-Novel Club"));
            Assert.That(result!.Any(k => k.Key.Title == "Vertical Title" && k.Key.Publisher == "Vertical Comics"));
            Assert.That(result!.Any(k => k.Key.Title == "Kana Title" && k.Key.Publisher == "Kana"));
            Assert.That(result!.Any(k => k.Key.Title == "Kodama Title" && k.Key.Publisher == "Kodama"));
        }
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task ExtractUniqueTitles_HtmlDecode_Applies()
    {
        string csv =
            "Title,Binding,My Rating,Publisher,Private Notes\n" +
            "My &amp; Title: Test,Paperback,0,VIZ MEDIA LLC,Note &quot;X&quot;";
        string path = CreateTempCsvFile(csv);

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles([path]);

        KeyValuePair<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint> kv =
            result!.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(kv.Key.Title, Is.EqualTo("My & Title"));
            Assert.That(kv.Key.Publisher, Is.EqualTo("Viz Media"));
        }
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task ExtractUniqueTitles_RatingAndNotes_Preserved()
    {
        string csv =
            "Title,Binding,My Rating,Publisher,Private Notes\n" +
            "T1,Paperback,3.5,Viz Media,Some Notes";
        string path = CreateTempCsvFile(csv);

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles([path]);

        KeyValuePair<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint> kv =
            result!.Single();

        Assert.That(kv.Key.Rating, Is.EqualTo(3.5m));
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task ExtractUniqueTitles_DuplicateTitles_MergedByTitleAndFormat()
    {
        // Same cleaned title; both Titles contain commas, so each is quoted.
        string csv =
            "Title,Binding,My Rating,Publisher,Private Notes\n" +
            "\"Dup Series, Vol. 1\",Paperback,5,Viz Media,A\n" +
            "\"Dup Series, Vol. 2\",Paperback,4,Kodansha,B\n";
        string path = CreateTempCsvFile(csv);

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles([path]);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!, Has.Count.EqualTo(1));

        KeyValuePair<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint> kv =
            result!.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(kv.Key.Title, Is.EqualTo("Dup Series"));
            Assert.That(kv.Key.Format, Is.EqualTo(SeriesFormat.Manga));
            Assert.That(kv.Value, Is.EqualTo(2u));
        }
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task ExtractUniqueTitles_Defaults_WhenPublisherOrRatingMissingOrEmpty()
    {
        // Titles contain commas → wrap them in quotes.
        string csv =
            "Title,Binding,My Rating,Publisher,Private Notes\n" +
            "\"Missing Both, Vol. 1\",Paperback,,,\n" +                 // both missing → Unknown / 0
            "\"Missing Publisher, Vol. 3\",Paperback,4,,\n" +           // missing publisher → Unknown, rating kept
            "\"Missing Rating, Vol. 2\",Paperback,,Kodansha,Note";      // missing rating → 0, publisher kept

        string path = CreateTempCsvFile(csv);

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result =
            await GoodreadsParser.ExtractUniqueTitles([path]);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!, Has.Count.EqualTo(3));

        // 1) Both missing → Unknown / 0
        KeyValuePair<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint> both =
            result!.Single(kvp => kvp.Key.Title == "Missing Both" && kvp.Key.Format == SeriesFormat.Manga);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(both.Key.Publisher, Is.EqualTo("Unknown"));
            Assert.That(both.Key.Rating, Is.EqualTo(0m));
            Assert.That(both.Value, Is.EqualTo(1u));
        }

        // 2) Publisher missing only → Unknown, rating preserved
        KeyValuePair<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint> noPub =
            result!.Single(kvp => kvp.Key.Title == "Missing Publisher" && kvp.Key.Format == SeriesFormat.Manga);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(noPub.Key.Publisher, Is.EqualTo("Unknown"));
            Assert.That(noPub.Key.Rating, Is.EqualTo(4m));
            Assert.That(noPub.Value, Is.EqualTo(1u));
        }

        // 3) Rating missing only → 0, publisher preserved (no remap applied here)
        KeyValuePair<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint> noRating =
            result!.Single(kvp => kvp.Key.Title == "Missing Rating" && kvp.Key.Format == SeriesFormat.Manga);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(noRating.Key.Publisher, Is.EqualTo("Kodansha"));
            Assert.That(noRating.Key.Rating, Is.EqualTo(0m));
            Assert.That(noRating.Value, Is.EqualTo(1u));
        }
    }
}