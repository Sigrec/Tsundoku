namespace Tsundoku.Models.Enums;

public static class SeriesStatusModel
{
    public enum SeriesStatus
    {
        Finished,
        Ongoing,
        Hiatus,
        Cancelled,
        Error
    }

    /// <summary>
    /// Parses a string representation of a series status into the corresponding SeriesStatus enum.
    /// Handles various common API strings and provides a default 'Error' status for unmatched inputs.
    /// </summary>
    /// <param name="jsonStatus">The status string obtained from a JSON source (e.g., MangaDex API).</param>
    /// <returns>The corresponding SeriesStatus enum value, or SeriesStatus.Error if no match is found.</returns>
    public static SeriesStatus Parse(string jsonStatus)
    {
        // The switch expression is excellent for mapping multiple input strings to enum values.
        // It inherently handles the exact string comparisons as defined.
        // If 'jsonStatus' could have arbitrary casing for "RELEASING", etc.,
        // you might want to use 'jsonStatus.ToUpperInvariant()' or 'jsonStatus.ToLowerInvariant()'
        // before the switch, but your current setup covers common variations.
        return jsonStatus switch
        {
            "RELEASING" or "NOT_YET_RELEASED" or "ongoing" => SeriesStatus.Ongoing,
            "FINISHED" or "completed" => SeriesStatus.Finished,
            "CANCELLED" or "cancelled" => SeriesStatus.Cancelled,
            "HIATUS" or "hiatus" => SeriesStatus.Hiatus,
            _ => SeriesStatus.Error // Default for any unhandled or unknown status string
        };
    }
}