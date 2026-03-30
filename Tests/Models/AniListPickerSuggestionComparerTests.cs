namespace Tsundoku.Tests.Models;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AniListPickerSuggestionComparerTests
{
    private readonly AniListPickerSuggestionComparer _comparer = new();

    [Test]
    public void Compare_BothNull_ReturnsZero()
    {
        int result = _comparer.Compare(null, null);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Compare_LeftNull_ReturnsNegative()
    {
        AniListPickerSuggestion suggestion = new("1", "Title", "MANGA");
        int result = _comparer.Compare(null, suggestion);
        Assert.That(result, Is.LessThan(0));
    }

    [Test]
    public void Compare_RightNull_ReturnsPositive()
    {
        AniListPickerSuggestion suggestion = new("1", "Title", "MANGA");
        int result = _comparer.Compare(suggestion, null);
        Assert.That(result, Is.GreaterThan(0));
    }

    [Test]
    public void Compare_MangaBeforeNovel()
    {
        AniListPickerSuggestion manga = new("1", "Title A", "MANGA");
        AniListPickerSuggestion novel = new("2", "Title A", "NOVEL");

        int result = _comparer.Compare(manga, novel);
        Assert.That(result, Is.LessThan(0), "MANGA should sort before NOVEL");
    }

    [Test]
    public void Compare_NovelAfterManga()
    {
        AniListPickerSuggestion manga = new("1", "Title A", "MANGA");
        AniListPickerSuggestion novel = new("2", "Title A", "NOVEL");

        int result = _comparer.Compare(novel, manga);
        Assert.That(result, Is.GreaterThan(0), "NOVEL should sort after MANGA");
    }

    [Test]
    public void Compare_MangaBeforeNovel_RegardlessOfDisplayOrder()
    {
        AniListPickerSuggestion manga = new("1", "Zebra", "MANGA");
        AniListPickerSuggestion novel = new("2", "Alpha", "NOVEL");

        int result = _comparer.Compare(manga, novel);
        Assert.That(result, Is.LessThan(0), "MANGA should sort before NOVEL even when display name is later alphabetically");
    }

    [Test]
    public void Compare_SameFormat_SortsByDisplayAlphabetically()
    {
        AniListPickerSuggestion alpha = new("1", "Alpha", "MANGA");
        AniListPickerSuggestion beta = new("2", "Beta", "MANGA");

        int result = _comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0), "Same format should sort alphabetically by Display");
    }

    [Test]
    public void Compare_SameFormat_SortsByDisplayAlphabetically_Reverse()
    {
        AniListPickerSuggestion alpha = new("1", "Alpha", "MANGA");
        AniListPickerSuggestion beta = new("2", "Beta", "MANGA");

        int result = _comparer.Compare(beta, alpha);
        Assert.That(result, Is.GreaterThan(0));
    }

    [Test]
    public void Compare_SameFormatNovel_SortsByDisplayAlphabetically()
    {
        AniListPickerSuggestion alpha = new("1", "Alpha", "NOVEL");
        AniListPickerSuggestion beta = new("2", "Beta", "NOVEL");

        int result = _comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0), "Same NOVEL format should sort alphabetically by Display");
    }

    [Test]
    public void Compare_IdenticalEntries_ReturnsZero()
    {
        AniListPickerSuggestion a = new("1", "Same Title", "MANGA");
        AniListPickerSuggestion b = new("2", "Same Title", "MANGA");

        int result = _comparer.Compare(a, b);
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Compare_CaseInsensitiveDisplay()
    {
        AniListPickerSuggestion lower = new("1", "alpha", "MANGA");
        AniListPickerSuggestion upper = new("2", "ALPHA", "MANGA");

        int result = _comparer.Compare(lower, upper);
        Assert.That(result, Is.EqualTo(0), "Display comparison should be case-insensitive");
    }

    [Test]
    public void Sort_MixedFormats_MangaGroupedFirst()
    {
        List<AniListPickerSuggestion> list =
        [
            new("1", "Naruto", "NOVEL"),
            new("2", "Bleach", "MANGA"),
            new("3", "One Piece", "MANGA"),
            new("4", "Overlord", "NOVEL"),
        ];

        list.Sort(_comparer);

        Assert.That(list[0].Format, Is.EqualTo("MANGA"));
        Assert.That(list[1].Format, Is.EqualTo("MANGA"));
        Assert.That(list[2].Format, Is.EqualTo("NOVEL"));
        Assert.That(list[3].Format, Is.EqualTo("NOVEL"));
    }

    [Test]
    public void Sort_MixedFormats_AlphabeticalWithinGroup()
    {
        List<AniListPickerSuggestion> list =
        [
            new("1", "Naruto", "NOVEL"),
            new("2", "Bleach", "MANGA"),
            new("3", "One Piece", "MANGA"),
            new("4", "Attack on Titan", "NOVEL"),
        ];

        list.Sort(_comparer);

        Assert.That(list[0].Display, Is.EqualTo("Bleach"));
        Assert.That(list[1].Display, Is.EqualTo("One Piece"));
        Assert.That(list[2].Display, Is.EqualTo("Attack on Titan"));
        Assert.That(list[3].Display, Is.EqualTo("Naruto"));
    }

    [Test]
    public void Compare_EmptyDisplayStrings_ReturnsZero()
    {
        AniListPickerSuggestion a = new("1", string.Empty, "MANGA");
        AniListPickerSuggestion b = new("2", string.Empty, "MANGA");

        int result = _comparer.Compare(a, b);
        Assert.That(result, Is.EqualTo(0));
    }
}
