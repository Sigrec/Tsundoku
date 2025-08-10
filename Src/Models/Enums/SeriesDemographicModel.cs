using System.Collections.Frozen;

namespace Tsundoku.Models.Enums;

public static class SeriesDemographicModel
{
    public enum SeriesDemographic
    {
        Shounen,
        Shoujo,
        Seinen,
        Josei,
        Unknown
    }

    public static readonly FrozenSet<SeriesDemographic> SERIES_DEMOGRAPHIC_SET = Enum.GetValues<SeriesDemographic>().ToFrozenSet();

    public static readonly FrozenDictionary<SeriesDemographic, int> SERIES_DEMOGRAPHICS_DICT =
    SERIES_DEMOGRAPHIC_SET
        .AsValueEnumerable()
        .Select((value, index) => new KeyValuePair<SeriesDemographic, int>(value, index))
        .ToFrozenDictionary(pair => pair.Key, pair => pair.Value);

    public static SeriesDemographic Parse(string demographicString)
    {
        if (Enum.TryParse(demographicString, true, out SeriesDemographic result))
        {
            return result;
        }
        return SeriesDemographic.Unknown;
    }
}