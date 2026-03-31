using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using static Tsundoku.Models.Enums.SeriesStatusModel;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;
using static Tsundoku.Models.Enums.TsundokuSortModel;

namespace Tsundoku.Tests.Models;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class SeriesComparerTests
{
    [AvaloniaTest]
    public void Compare_BothNull_ReturnsZero()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.TitleAZ);
        int result = comparer.Compare(null, null);
        Assert.That(result, Is.EqualTo(0));
    }

    [AvaloniaTest]
    public void Compare_LeftNull_ReturnsNegative()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.TitleAZ);
        Series series = CreateSeries("Alpha");
        int result = comparer.Compare(null, series);
        Assert.That(result, Is.LessThan(0));
    }

    [AvaloniaTest]
    public void Compare_RightNull_ReturnsPositive()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.TitleAZ);
        Series series = CreateSeries("Alpha");
        int result = comparer.Compare(series, null);
        Assert.That(result, Is.GreaterThan(0));
    }

    [AvaloniaTest]
    public void TitleAZ_SortsAlphabetically()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.TitleAZ);
        Series alpha = CreateSeries("Alpha");
        Series beta = CreateSeries("Beta");

        int result = comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0));
    }

    [AvaloniaTest]
    public void TitleAZ_EqualTitles_ComparesByDuplicateIndex()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.TitleAZ);
        Series first = CreateSeries("Same", duplicateIndex: 1);
        Series second = CreateSeries("Same", duplicateIndex: 2);

        int result = comparer.Compare(first, second);
        Assert.That(result, Is.LessThan(0));
    }

    [AvaloniaTest]
    public void TitleZA_SortsReverseAlphabetically()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.TitleZA);
        Series alpha = CreateSeries("Alpha");
        Series beta = CreateSeries("Beta");

        int result = comparer.Compare(alpha, beta);
        Assert.That(result, Is.GreaterThan(0));
    }

    [AvaloniaTest]
    public void Rating_HigherRatingComesFirst()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.Rating);
        Series low = CreateSeries("Low", rating: 3.0m);
        Series high = CreateSeries("High", rating: 9.0m);

        int result = comparer.Compare(low, high);
        Assert.That(result, Is.GreaterThan(0), "Higher rating should sort before lower rating");
    }

    [AvaloniaTest]
    public void Rating_EqualRatings_FallsBackToTitle()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.Rating);
        Series alpha = CreateSeries("Alpha", rating: 5.0m);
        Series beta = CreateSeries("Beta", rating: 5.0m);

        int result = comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0), "Equal ratings should fall back to title A-Z");
    }

    [AvaloniaTest]
    public void Unread_MoreUnreadComesFirst()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.Unread);
        Series fewUnread = CreateSeries("Few", maxVolCount: 10, curVolCount: 8);
        Series manyUnread = CreateSeries("Many", maxVolCount: 10, curVolCount: 2);

        int result = comparer.Compare(fewUnread, manyUnread);
        Assert.That(result, Is.GreaterThan(0), "More unread should sort before fewer unread");
    }

    [AvaloniaTest]
    public void Unread_EqualUnread_FallsBackToTitle()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.Unread);
        Series alpha = CreateSeries("Alpha", maxVolCount: 10, curVolCount: 5);
        Series beta = CreateSeries("Beta", maxVolCount: 10, curVolCount: 5);

        int result = comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0), "Equal unread should fall back to title A-Z");
    }

    [AvaloniaTest]
    public void Read_MoreReadComesFirst()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.Read);
        Series fewRead = CreateSeries("Few", volumesRead: 2);
        Series manyRead = CreateSeries("Many", volumesRead: 8);

        int result = comparer.Compare(fewRead, manyRead);
        Assert.That(result, Is.GreaterThan(0), "More read should sort before fewer read");
    }

    [AvaloniaTest]
    public void Read_EqualRead_FallsBackToTitle()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.Read);
        Series alpha = CreateSeries("Alpha", volumesRead: 5);
        Series beta = CreateSeries("Beta", volumesRead: 5);

        int result = comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0), "Equal read should fall back to title A-Z");
    }

    [AvaloniaTest]
    public void Value_HigherValueComesFirst()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.Value);
        Series cheap = CreateSeries("Cheap", value: 10.0m);
        Series expensive = CreateSeries("Expensive", value: 100.0m);

        int result = comparer.Compare(cheap, expensive);
        Assert.That(result, Is.GreaterThan(0), "Higher value should sort before lower value");
    }

    [AvaloniaTest]
    public void Value_EqualValue_FallsBackToTitle()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.Value);
        Series alpha = CreateSeries("Alpha", value: 50.0m);
        Series beta = CreateSeries("Beta", value: 50.0m);

        int result = comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0), "Equal value should fall back to title A-Z");
    }

    [AvaloniaTest]
    public void VolumeCount_HigherCountComesFirst()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.VolumeCount);
        Series few = CreateSeries("Few", curVolCount: 3);
        Series many = CreateSeries("Many", curVolCount: 20);

        int result = comparer.Compare(few, many);
        Assert.That(result, Is.GreaterThan(0), "Higher volume count should sort before lower");
    }

    [AvaloniaTest]
    public void VolumeCount_EqualCount_FallsBackToTitle()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English, TsundokuSort.VolumeCount);
        Series alpha = CreateSeries("Alpha", curVolCount: 10);
        Series beta = CreateSeries("Beta", curVolCount: 10);

        int result = comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0), "Equal volume count should fall back to title A-Z");
    }

    [AvaloniaTest]
    public void TitleAZ_UsesRomajiFallback_WhenLanguageMissing()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.Japanese, TsundokuSort.TitleAZ);

        Series alpha = CreateSeriesWithRomaji("Romaji Alpha");
        Series beta = CreateSeriesWithRomaji("Romaji Beta");

        int result = comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0), "Should fall back to Romaji titles when requested language is missing");
    }

    [AvaloniaTest]
    public void DefaultSort_UsesTitleAZ()
    {
        SeriesComparer comparer = new SeriesComparer(TsundokuLanguage.English);
        Series alpha = CreateSeries("Alpha");
        Series beta = CreateSeries("Beta");

        int result = comparer.Compare(alpha, beta);
        Assert.That(result, Is.LessThan(0), "Default sort should be Title A-Z");
    }

    private static Series CreateSeries(
        string title,
        decimal rating = 5.0m,
        uint maxVolCount = 10,
        uint curVolCount = 5,
        uint volumesRead = 3,
        decimal value = 49.99m,
        uint duplicateIndex = 0)
    {
        WriteableBitmap emptyBitmap = new WriteableBitmap(
            new PixelSize(1, 1),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul
        );

        return new Series(
            Titles: new() { [TsundokuLanguage.English] = title, [TsundokuLanguage.Romaji] = title },
            Staff: new() { [TsundokuLanguage.English] = "Staff" },
            Description: string.Empty,
            Format: SeriesFormat.Manga,
            Status: SeriesStatus.Ongoing,
            Cover: string.Empty,
            Link: new Uri("https://example.com"),
            Genres: [SeriesGenre.Action],
            MaxVolumeCount: maxVolCount,
            CurVolumeCount: curVolCount,
            Rating: rating,
            VolumesRead: volumesRead,
            Value: value,
            Demographic: SeriesDemographic.Shounen,
            CoverBitMap: emptyBitmap,
            DuplicateIndex: duplicateIndex
        );
    }

    private static Series CreateSeriesWithRomaji(string romajiTitle)
    {
        WriteableBitmap emptyBitmap = new WriteableBitmap(
            new PixelSize(1, 1),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul
        );

        return new Series(
            Titles: new() { [TsundokuLanguage.Romaji] = romajiTitle },
            Staff: new() { [TsundokuLanguage.English] = "Staff" },
            Description: string.Empty,
            Format: SeriesFormat.Manga,
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
