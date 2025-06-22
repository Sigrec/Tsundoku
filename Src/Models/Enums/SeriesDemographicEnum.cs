using System.Collections.Frozen;

namespace Tsundoku.Models.Enums;

public static class SeriesDemographicEnum
{
    public enum SeriesDemographic
    {
        Shounen,
        Shoujo,
        Seinen,
        Josei,
        Unknown
    }

    public static readonly FrozenDictionary<SeriesDemographic, int> SERIES_DEMOGRAPHICS =
    Enum.GetValues<SeriesDemographic>()
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