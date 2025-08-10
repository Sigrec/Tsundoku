using CsvHelper;

namespace Tsundoku.Helpers;

public static class ParserHelper
{
    public static string ParseCsvString(this CsvReader csv, string field, string nullValue)
    {
        string? fieldVal = csv.GetField(field);
        return string.IsNullOrWhiteSpace(fieldVal) ? nullValue : fieldVal.Trim();
    }

    public static bool ContainsAny(this string input, IEnumerable<string> values)
    {
        return values.AsValueEnumerable().Any(val => input.Contains(val, StringComparison.OrdinalIgnoreCase));
    }
}