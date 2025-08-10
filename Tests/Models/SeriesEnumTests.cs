using Tsundoku.Models.Enums;

namespace Tsundoku.Tests.Models;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class SeriesEnumTests
{
    [TestCase("KR", SeriesFormatEnum.SeriesFormat.Manhwa)]
    [TestCase("ko-ro", SeriesFormatEnum.SeriesFormat.Manhwa)]
    [TestCase("ko", SeriesFormatEnum.SeriesFormat.Manhwa)]
    [TestCase("CN", SeriesFormatEnum.SeriesFormat.Manhua)]
    [TestCase("TW", SeriesFormatEnum.SeriesFormat.Manhua)]
    [TestCase("zh", SeriesFormatEnum.SeriesFormat.Manhua)]
    [TestCase("zh-hk", SeriesFormatEnum.SeriesFormat.Manhua)]
    [TestCase("zh-ro", SeriesFormatEnum.SeriesFormat.Manhua)]
    [TestCase("FR", SeriesFormatEnum.SeriesFormat.Manfra)]
    [TestCase("fr", SeriesFormatEnum.SeriesFormat.Manfra)]
    [TestCase("EN", SeriesFormatEnum.SeriesFormat.Comic)]
    [TestCase("en", SeriesFormatEnum.SeriesFormat.Comic)]
    [TestCase("JP", SeriesFormatEnum.SeriesFormat.Manga)]
    [TestCase("ja", SeriesFormatEnum.SeriesFormat.Manga)]
    [TestCase("UNKNOWN", SeriesFormatEnum.SeriesFormat.Manga)]
    public void SeriesFormat_Parse_WorksCorrectly(string input, SeriesFormatEnum.SeriesFormat expected)
    {
        SeriesFormatEnum.SeriesFormat result = SeriesFormatEnum.Parse(input);
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

    [TestCase("RELEASING", SeriesStatusEnum.SeriesStatus.Ongoing)]
    [TestCase("NOT_YET_RELEASED", SeriesStatusEnum.SeriesStatus.Ongoing)]
    [TestCase("ongoing", SeriesStatusEnum.SeriesStatus.Ongoing)]
    [TestCase("FINISHED", SeriesStatusEnum.SeriesStatus.Finished)]
    [TestCase("completed", SeriesStatusEnum.SeriesStatus.Finished)]
    [TestCase("CANCELLED", SeriesStatusEnum.SeriesStatus.Cancelled)]
    [TestCase("cancelled", SeriesStatusEnum.SeriesStatus.Cancelled)]
    [TestCase("HIATUS", SeriesStatusEnum.SeriesStatus.Hiatus)]
    [TestCase("hiatus", SeriesStatusEnum.SeriesStatus.Hiatus)]
    [TestCase("invalid", SeriesStatusEnum.SeriesStatus.Error)]
    public void SeriesStatus_Parse_ReturnsExpectedResult(string input, SeriesStatusEnum.SeriesStatus expected)
    {
        SeriesStatusEnum.SeriesStatus result = SeriesStatusEnum.Parse(input);
        Assert.That(result, Is.EqualTo(expected));
    }
}
