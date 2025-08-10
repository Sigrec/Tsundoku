using Tsundoku.Models.Enums;

namespace Tsundoku.Tests.Models;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class SeriesEnumTests
{
    [TestCase("KR", SeriesFormatModel.SeriesFormat.Manhwa)]
    [TestCase("ko-ro", SeriesFormatModel.SeriesFormat.Manhwa)]
    [TestCase("ko", SeriesFormatModel.SeriesFormat.Manhwa)]
    [TestCase("CN", SeriesFormatModel.SeriesFormat.Manhua)]
    [TestCase("TW", SeriesFormatModel.SeriesFormat.Manhua)]
    [TestCase("zh", SeriesFormatModel.SeriesFormat.Manhua)]
    [TestCase("zh-hk", SeriesFormatModel.SeriesFormat.Manhua)]
    [TestCase("zh-ro", SeriesFormatModel.SeriesFormat.Manhua)]
    [TestCase("FR", SeriesFormatModel.SeriesFormat.Manfra)]
    [TestCase("fr", SeriesFormatModel.SeriesFormat.Manfra)]
    [TestCase("EN", SeriesFormatModel.SeriesFormat.Comic)]
    [TestCase("en", SeriesFormatModel.SeriesFormat.Comic)]
    [TestCase("JP", SeriesFormatModel.SeriesFormat.Manga)]
    [TestCase("ja", SeriesFormatModel.SeriesFormat.Manga)]
    [TestCase("UNKNOWN", SeriesFormatModel.SeriesFormat.Manga)]
    public void SeriesFormat_Parse_WorksCorrectly(string input, SeriesFormatModel.SeriesFormat expected)
    {
        SeriesFormatModel.SeriesFormat result = SeriesFormatModel.Parse(input);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Shounen", SeriesDemographicModel.SeriesDemographic.Shounen)]
    [TestCase("shoujo", SeriesDemographicModel.SeriesDemographic.Shoujo)]
    [TestCase("Seinen", SeriesDemographicModel.SeriesDemographic.Seinen)]
    [TestCase("JOSEI", SeriesDemographicModel.SeriesDemographic.Josei)]
    [TestCase("Unknown", SeriesDemographicModel.SeriesDemographic.Unknown)]
    [TestCase("nonexistent", SeriesDemographicModel.SeriesDemographic.Unknown)]
    public void SeriesDemographic_Parse_WorksCorrectly(string input, SeriesDemographicModel.SeriesDemographic expected)
    {
        SeriesDemographicModel.SeriesDemographic result = SeriesDemographicModel.Parse(input);
        Assert.That(result, Is.EqualTo(expected));
    }


    [Test]
    public void SeriesGenre_TryParse_KnownAlias_ReturnsTrue()
    {
        bool success = SeriesGenreModel.TryParse("Magical Girls", out SeriesGenreModel.SeriesGenre genre);
        Assert.That(success, Is.True);
        Assert.That(genre, Is.EqualTo(SeriesGenreModel.SeriesGenre.MahouShoujo));
    }

    [Test]
    public void SeriesGenre_TryParse_UnknownAlias_ReturnsFalse()
    {
        bool success = SeriesGenreModel.TryParse("NoSuchGenre", out SeriesGenreModel.SeriesGenre genre);
        Assert.That(success, Is.False);
        Assert.That(genre, Is.EqualTo(SeriesGenreModel.SeriesGenre.Unknown));
    }

    [Test]
    public void SeriesGenre_Parse_KnownAlias_ReturnsCorrectGenre()
    {
        SeriesGenreModel.SeriesGenre genre = SeriesGenreModel.Parse("SciFi");
        Assert.That(genre, Is.EqualTo(SeriesGenreModel.SeriesGenre.SciFi));
    }

    [Test]
    public void SeriesGenre_GetAliases_ReturnsAliases()
    {
        IEnumerable<string> aliases = SeriesGenreModel.GetAliases(SeriesGenreModel.SeriesGenre.SliceOfLife);
        Assert.That(aliases, Contains.Item("Slice of Life"));
        Assert.That(aliases, Contains.Item("SliceOfLife"));
    }

    [TestCase("RELEASING", SeriesStatusModel.SeriesStatus.Ongoing)]
    [TestCase("NOT_YET_RELEASED", SeriesStatusModel.SeriesStatus.Ongoing)]
    [TestCase("ongoing", SeriesStatusModel.SeriesStatus.Ongoing)]
    [TestCase("FINISHED", SeriesStatusModel.SeriesStatus.Finished)]
    [TestCase("completed", SeriesStatusModel.SeriesStatus.Finished)]
    [TestCase("CANCELLED", SeriesStatusModel.SeriesStatus.Cancelled)]
    [TestCase("cancelled", SeriesStatusModel.SeriesStatus.Cancelled)]
    [TestCase("HIATUS", SeriesStatusModel.SeriesStatus.Hiatus)]
    [TestCase("hiatus", SeriesStatusModel.SeriesStatus.Hiatus)]
    [TestCase("invalid", SeriesStatusModel.SeriesStatus.Error)]
    public void SeriesStatus_Parse_ReturnsExpectedResult(string input, SeriesStatusModel.SeriesStatus expected)
    {
        SeriesStatusModel.SeriesStatus result = SeriesStatusModel.Parse(input);
        Assert.That(result, Is.EqualTo(expected));
    }
}
