using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using static Tsundoku.Models.Enums.SeriesStatusModel;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Tests.Models;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class SeriesValueComparerTests
{
    private readonly SeriesValueComparer _comparer = new SeriesValueComparer();

    [AvaloniaTest]
    public void Equals_BothNull_ReturnsFalse()
    {
        bool result = _comparer.Equals(null, null);
        Assert.That(result, Is.False);
    }

    [AvaloniaTest]
    public void Equals_LeftNull_ReturnsFalse()
    {
        Series series = CreateSeries("Title", SeriesFormat.Manga);
        bool result = _comparer.Equals(null, series);
        Assert.That(result, Is.False);
    }

    [AvaloniaTest]
    public void Equals_RightNull_ReturnsFalse()
    {
        Series series = CreateSeries("Title", SeriesFormat.Manga);
        bool result = _comparer.Equals(series, null);
        Assert.That(result, Is.False);
    }

    [AvaloniaTest]
    public void Equals_SameFormatTitlesAndStaff_ReturnsTrue()
    {
        Series a = CreateSeries("Title", SeriesFormat.Manga, "Author A");
        Series b = CreateSeries("Title", SeriesFormat.Manga, "Author A");

        bool result = _comparer.Equals(a, b);
        Assert.That(result, Is.True);
    }

    [AvaloniaTest]
    public void Equals_DifferentFormat_ReturnsFalse()
    {
        Series manga = CreateSeries("Title", SeriesFormat.Manga, "Author A");
        Series novel = CreateSeries("Title", SeriesFormat.Novel, "Author A");

        bool result = _comparer.Equals(manga, novel);
        Assert.That(result, Is.False);
    }

    [AvaloniaTest]
    public void Equals_DifferentTitles_ReturnsFalse()
    {
        Series a = CreateSeries("Alpha", SeriesFormat.Manga, "Author A");
        Series b = CreateSeries("Beta", SeriesFormat.Manga, "Author A");

        bool result = _comparer.Equals(a, b);
        Assert.That(result, Is.False);
    }

    [AvaloniaTest]
    public void Equals_DifferentStaff_ReturnsFalse()
    {
        Series a = CreateSeries("Title", SeriesFormat.Manga, "Author A");
        Series b = CreateSeries("Title", SeriesFormat.Manga, "Author B");

        bool result = _comparer.Equals(a, b);
        Assert.That(result, Is.False);
    }

    [AvaloniaTest]
    public void Equals_DifferentTitleLanguages_ReturnsFalse()
    {
        Series a = CreateSeries("Title", SeriesFormat.Manga);
        Series b = CreateSeriesWithMultipleTitles("Title", "Titre", SeriesFormat.Manga);

        bool result = _comparer.Equals(a, b);
        Assert.That(result, Is.False);
    }

    [AvaloniaTest]
    public void Equals_IgnoresNonComparedProperties()
    {
        Series a = CreateSeries("Title", SeriesFormat.Manga, "Author A", rating: 3.0m, value: 10.0m);
        Series b = CreateSeries("Title", SeriesFormat.Manga, "Author A", rating: 9.0m, value: 100.0m);

        bool result = _comparer.Equals(a, b);
        Assert.That(result, Is.True, "Rating and value should not affect equality");
    }

    [AvaloniaTest]
    public void GetHashCode_EqualSeries_ReturnsSameHash()
    {
        Series a = CreateSeries("Title", SeriesFormat.Manga, "Author A");
        Series b = CreateSeries("Title", SeriesFormat.Manga, "Author A");

        Assert.That(_comparer.GetHashCode(a), Is.EqualTo(_comparer.GetHashCode(b)));
    }

    [AvaloniaTest]
    public void GetHashCode_DifferentFormat_ReturnsDifferentHash()
    {
        Series manga = CreateSeries("Title", SeriesFormat.Manga, "Author A");
        Series novel = CreateSeries("Title", SeriesFormat.Novel, "Author A");

        Assert.That(_comparer.GetHashCode(manga), Is.Not.EqualTo(_comparer.GetHashCode(novel)));
    }

    [AvaloniaTest]
    public void GetHashCode_DifferentTitles_IsConsistent()
    {
        Series a = CreateSeries("Alpha", SeriesFormat.Manga, "Author A");

        Assert.That(_comparer.GetHashCode(a), Is.EqualTo(_comparer.GetHashCode(a)));
    }

    [AvaloniaTest]
    public void GetHashCode_DifferentStaff_IsConsistent()
    {
        Series a = CreateSeries("Title", SeriesFormat.Manga, "Author A");

        Assert.That(_comparer.GetHashCode(a), Is.EqualTo(_comparer.GetHashCode(a)));
    }

    private static Series CreateSeries(
        string title,
        SeriesFormat format,
        string staff = "Staff",
        decimal rating = 5.0m,
        decimal value = 49.99m)
    {
        WriteableBitmap emptyBitmap = new WriteableBitmap(
            new PixelSize(1, 1),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul
        );

        return new Series(
            Titles: new() { [TsundokuLanguage.English] = title },
            Staff: new() { [TsundokuLanguage.English] = staff },
            Description: string.Empty,
            Format: format,
            Status: SeriesStatus.Ongoing,
            Cover: string.Empty,
            Link: new Uri("https://example.com"),
            Genres: [SeriesGenre.Action],
            MaxVolumeCount: 10,
            CurVolumeCount: 5,
            Rating: rating,
            VolumesRead: 3,
            Value: value,
            Demographic: SeriesDemographic.Shounen,
            CoverBitMap: emptyBitmap
        );
    }

    private static Series CreateSeriesWithMultipleTitles(
        string englishTitle,
        string frenchTitle,
        SeriesFormat format)
    {
        WriteableBitmap emptyBitmap = new WriteableBitmap(
            new PixelSize(1, 1),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul
        );

        return new Series(
            Titles: new()
            {
                [TsundokuLanguage.English] = englishTitle,
                [TsundokuLanguage.French] = frenchTitle
            },
            Staff: new() { [TsundokuLanguage.English] = "Staff" },
            Description: string.Empty,
            Format: format,
            Status: SeriesStatus.Ongoing,
            Cover: string.Empty,
            Link: new Uri("https://example.com"),
            Genres: [SeriesGenre.Action],
            MaxVolumeCount: 10,
            CurVolumeCount: 5,
            Rating: 5.0m,
            VolumesRead: 3,
            Value: 49.99m,
            Demographic: SeriesDemographic.Shounen,
            CoverBitMap: emptyBitmap
        );
    }
}
