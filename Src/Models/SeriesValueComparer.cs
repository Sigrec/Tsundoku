using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Models;

public sealed class SeriesValueComparer : IEqualityComparer<Series>
{
    public bool Equals(Series? x, Series? y)
    {
        if (x is null || y is null)
            return false;

        return x.Format == y.Format
            && x.Titles.SequenceEqual(y.Titles)
            && x.Staff.SequenceEqual(y.Staff);
    }

    public int GetHashCode(Series obj)
    {
        HashCode hash = new HashCode();
        hash.Add(obj.Format);
        foreach (KeyValuePair<TsundokuLanguage, string> title in obj.Titles)
        {
            hash.Add(title);
        }
        foreach (KeyValuePair<TsundokuLanguage, string> staff in obj.Staff)
        {
            hash.Add(staff);
        }
        return hash.ToHashCode();
    }
}
