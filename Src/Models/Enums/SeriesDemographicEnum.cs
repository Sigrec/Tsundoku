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

    public static readonly SeriesDemographic[] SERIES_DEMOGRAPHICS = Enum.GetValues<SeriesDemographic>();

    public static SeriesDemographic Parse(string demographicString)
    {
        if (Enum.TryParse(demographicString, true, out SeriesDemographic result))
        {
            return result;
        }
        return SeriesDemographic.Unknown;
    }
}