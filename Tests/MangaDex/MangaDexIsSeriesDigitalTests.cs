namespace Tsundoku.Tests.MangaDex;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class MangaDexIsDigitalSeriesTests
{
    [Test]
    public void InvalidSeries_EnglishTitleContainsDigital_NoKeywordsInOthers_ReturnsTrue()
    {
        Assert.That(Clients.MangaDex.IsSeriesDigital("Normal Title", "Digital Version", "Alt Title"), Is.True);
    }

    [Test]
    public void ValidSeries_TitleContainsDigital_ReturnsFalse()
    {
        Assert.That(Clients.MangaDex.IsSeriesDigital("This is Digital", "Digital Release", "Alt Title"), Is.False);
    }

    [Test]
    public void ValidSeries_TitleContainsFanColored_ReturnsFalse()
    {
        Assert.That(Clients.MangaDex.IsSeriesDigital("Fan Colored Edition", "Digital Edition", "Alt Title"), Is.False);
    }

    [Test]
    public void ValidSeries_TitleContainsOfficialColored_ReturnsFalse()
    {
        Assert.That(Clients.MangaDex.IsSeriesDigital("Official Colored", "Digital", "Alt Title"), Is.False);
    }

    [Test]
    public void ValidSeries_AltTitleContainsDigital_ReturnsFalse()
    {
        Assert.That(Clients.MangaDex.IsSeriesDigital("Normal Title", "Digital Copy", "Digital Alt"), Is.False);
    }

    [Test]
    public void ValidSeries_AltTitleContainsOfficialColored_ReturnsFalse()
    {
        Assert.That(Clients.MangaDex.IsSeriesDigital("Normal Title", "Digital", "Official Colored Extra"), Is.False);
    }

    [Test]
    public void ValidSeries_EnglishTitleDoesNotContainDigital_ReturnsFalse()
    {
        Assert.That(Clients.MangaDex.IsSeriesDigital("Normal", "Physical Edition", "Some Alt"), Is.False);
    }

    [Test]
    public void CaseInsensitiveCheck_MixedCasing_ReturnsFalse()
    {
        Assert.That(Clients.MangaDex.IsSeriesDigital("fan colored", "DIGITAL!", "alt"), Is.False);
    }

    [Test]
    public void AllEmptyStrings_ReturnsFalse()
    {
        Assert.That(Clients.MangaDex.IsSeriesDigital("", "", ""), Is.False);
    }
}
