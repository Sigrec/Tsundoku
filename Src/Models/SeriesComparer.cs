using System.Globalization;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;
using static Tsundoku.Models.Enums.TsundokuSortModel;

namespace Tsundoku.Models;

public sealed class SeriesComparer(TsundokuLanguage curLang, TsundokuSort sort = TsundokuSort.TitleAZ) : IComparer<Series>
{
    private readonly TsundokuLanguage _curLang = curLang;
    private readonly TsundokuSort _sort = sort;
    private readonly StringComparer _seriesTitleComparer = StringComparer.Create(new CultureInfo(CULTURE_LANG_CODES[curLang]), false);

    public int Compare(Series? x, Series? y)
    {
        if (x is null && y is null) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        int result = _sort switch
        {
            TsundokuSort.TitleZA => -CompareByTitle(x, y),
            TsundokuSort.Rating => CompareByRating(x, y),
            TsundokuSort.Unread => CompareByUnread(x, y),
            TsundokuSort.Read => CompareByRead(x, y),
            TsundokuSort.Value => CompareByValue(x, y),
            TsundokuSort.VolumeCount => CompareByVolumeCount(x, y),
            _ => CompareByTitle(x, y),
        };

        return result != 0 ? result : CompareByTitle(x, y);
    }

    private int CompareByTitle(Series x, Series y)
    {
        string xTitle = x.Titles.TryGetValue(_curLang, out string? xValue) ? xValue : x.Titles[TsundokuLanguage.Romaji];
        string yTitle = y.Titles.TryGetValue(_curLang, out string? yValue) ? yValue : y.Titles[TsundokuLanguage.Romaji];

        int titleComparison = _seriesTitleComparer.Compare(xTitle, yTitle);
        return titleComparison != 0 ? titleComparison : x.DuplicateIndex.CompareTo(y.DuplicateIndex);
    }

    private static int CompareByRating(Series x, Series y)
    {
        return y.Rating.CompareTo(x.Rating);
    }

    private static int CompareByUnread(Series x, Series y)
    {
        long xUnread = (long)x.MaxVolumeCount - x.CurVolumeCount;
        long yUnread = (long)y.MaxVolumeCount - y.CurVolumeCount;
        return yUnread.CompareTo(xUnread);
    }

    private static int CompareByRead(Series x, Series y)
    {
        return y.VolumesRead.CompareTo(x.VolumesRead);
    }

    private static int CompareByValue(Series x, Series y)
    {
        return y.Value.CompareTo(x.Value);
    }

    private static int CompareByVolumeCount(Series x, Series y)
    {
        return y.CurVolumeCount.CompareTo(x.CurVolumeCount);
    }
}
