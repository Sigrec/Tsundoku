using CsvHelper;

namespace Tsundoku.Helpers;

public static class ParserHelper
{
    /// <summary>Gets a field by header name, or null when the file has no such column.</summary>
    public static string? GetOptionalField(this CsvReader csv, string field) =>
        csv.TryGetFieldIndex(field, out int index) ? csv.GetString(index) : null;

    public static string ParseCsvString(this CsvReader csv, string field, string nullValue)
    {
        string? fieldVal = csv.GetOptionalField(field);
        return string.IsNullOrWhiteSpace(fieldVal) ? nullValue : fieldVal.Trim();
    }

    public static bool ContainsAny(this string input, IEnumerable<string> values) => values.AsValueEnumerable().Any(val => input.Contains(val, StringComparison.OrdinalIgnoreCase));
}