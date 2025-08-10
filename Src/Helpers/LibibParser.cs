using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;
using static Tsundoku.Models.Enums.SeriesFormatModel;

namespace Tsundoku.Helpers;

public static partial class LibibParser
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    [GeneratedRegex(@"\s+,?(?:Omnibus|(?:Complete\s+)?Box\s+Set|Volume|Season|Special\s+Edition|(?:Deluxe|Special)\s+Complete|Vol|\d{1,3}:|\d{1,3}.*Anniversary|\([^()]*\)).*$|,[^,]*$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex TitleCleanRegex();

    [GeneratedRegex(@"\s*:\s.*$")]
    private static partial Regex TitleColonCleanRegex();

    [GeneratedRegex(@"(?<=(?:VIZ Media|Kodansha|Dark Horse|Vertical|Yen Press|Seven Seas|Ghost Ship|Titan|Ponent Mon|Square Enix))(?:,\s*|\s+?).*$", RegexOptions.IgnoreCase)]
    private static partial Regex PublisherCleanRegex();

    [GeneratedRegex(@"^\d{1,3}$")]
    private static partial Regex NumberCleanRegex();

    public static async Task<Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>?> ExtractUniqueTitles(string[]? csvFilePaths = null)
    {
        LOGGER.Info("Attempting to parse Libib csv files...");
        if (csvFilePaths is null)
        {
            LOGGER.Warn("User tried to import non existent file paths");
            return null;
        }

        Dictionary<(string Title, SeriesFormat Format, string Publisher), uint> result = new(new TitleFormatComparer());
        await Task.Run(() =>
        {
            foreach (string path in csvFilePaths)
            {
                using StreamReader reader = new(path);
                CsvConfiguration config = new(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    TrimOptions = TrimOptions.Trim,
                    DetectColumnCountChanges = false,
                    BadDataFound = null
                };

                using CsvReader csv = new CsvReader(reader, config);
                if (csv.Read()) // Read the first row (which should be the header)
                {
                    csv.ReadHeader(); // Parse the header names
                }
                else
                {
                    // If the file is empty (no rows at all), skip to the next file
                    LOGGER.Warn("User imported csv file {File} is empty", path);
                    continue;
                }

                while (csv.Read())
                {
                    string? rawTitle = csv.GetField("title");
                    if (string.IsNullOrWhiteSpace(rawTitle)) continue;

                    string description = csv.ParseCsvString("description", string.Empty);
                    string publisher = csv.ParseCsvString("publisher", "Unknown");

                    ReadOnlySpan<char> titleSpan = rawTitle.AsSpan();
                    ReadOnlySpan<char> descSpan = description.AsSpan();

                    SeriesFormat format = (
                        titleSpan.Contains("Light Novel", StringComparison.OrdinalIgnoreCase) ||
                        titleSpan.Contains("(Novel)", StringComparison.OrdinalIgnoreCase) ||
                        titleSpan.Contains("(LN)", StringComparison.OrdinalIgnoreCase) ||
                        descSpan.Contains("light novel", StringComparison.OrdinalIgnoreCase))
                        ? SeriesFormat.Novel
                        : SeriesFormat.Manga;

                    if (result.AsValueEnumerable().Any(k =>
                        k.Key.Title.Contains(rawTitle, StringComparison.OrdinalIgnoreCase) &&
                        k.Key.Format == format))
                    {
                        continue;
                    }

                    if (!publisher.Equals("Unknown"))
                    {
                        ReadOnlySpan<char> publisherSpan = publisher.AsSpan();
                        if (publisherSpan.Contains("Tokyopop", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "TOKYOPOP";
                        }
                        else if (publisherSpan.Contains("JNovel", StringComparison.OrdinalIgnoreCase) || publisherSpan.Contains("J-Novel", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "J-Novel Club";
                        }
                        else if (publisherSpan.Contains("VIZ Media", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "Viz Media";
                        }
                        else if (publisherSpan.Contains("Yen On", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "Yen Press";
                        }
                        else if (publisherSpan.Contains("Denpa", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "DENPA";
                        }
                        else if (publisherSpan.Contains("ComicsOne", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = publisher.Replace("ComicsOne Corporation", "ComicsOne");
                        }
                        else if (publisherSpan.Contains("Kodama", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "Kodama";
                        }
                    }

                    string cleanedTitle = TitleCleanRegex().Replace(rawTitle, string.Empty).Trim();
                    if (cleanedTitle.Length > 0)
                    {
                        int firstColonIndex = titleSpan.IndexOf(':');
                        int lastColonIndex = titleSpan.LastIndexOf(':');
                        if (firstColonIndex != -1 && lastColonIndex != -1 && (firstColonIndex != lastColonIndex || titleSpan.IndexOf("Vol", StringComparison.OrdinalIgnoreCase) > firstColonIndex || titleSpan.IndexOf("Volume", StringComparison.OrdinalIgnoreCase) > firstColonIndex))
                        {
                            cleanedTitle = TitleColonCleanRegex().Replace(cleanedTitle, string.Empty);
                        }

                        int lastSpaceIndex = cleanedTitle.LastIndexOf(' ');
                        if (lastSpaceIndex != -1 && lastSpaceIndex < cleanedTitle.Length - 1)
                        {
                            string potentialNumberPart = cleanedTitle[(lastSpaceIndex + 1)..];
                            // Check if the part after the last space is a number (1 to 3 digits)
                            if (NumberCleanRegex().IsMatch(potentialNumberPart))
                            {
                                cleanedTitle = cleanedTitle[..lastSpaceIndex].Trim();
                            }
                        }
                    }

                    cleanedTitle = System.Web.HttpUtility.HtmlDecode(cleanedTitle.TrimEnd(':'));
                    publisher = PublisherCleanRegex().Replace(System.Web.HttpUtility.HtmlDecode(publisher), string.Empty).Trim();

                    (string Title, SeriesFormat format, string Publisher) entry = (cleanedTitle, format, publisher);
                    if (result.TryGetValue(entry, out uint count))
                    {
                        result[entry] = count + 1;
                    }
                    else
                    {
                        result[entry] = 1;
                    }
                }
            }
        });

        if (result.Count == 0)
        {
            LOGGER.Warn("User tried to import libib data from file(s) [{Files}] but none was returned from parse", string.Join(',', csvFilePaths));
            return null;
        }

        return result;
    }

    private sealed class TitleFormatComparer : IEqualityComparer<(string Title, SeriesFormat Format, string Publisher)>
    {
        public bool Equals((string Title, SeriesFormat Format, string Publisher) x, (string Title, SeriesFormat Format, string Publisher) y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(x.Title, y.Title) && x.Format == y.Format;
        }

        public int GetHashCode((string Title, SeriesFormat Format, string Publisher) obj)
        {
            int hashTitle = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Title);
            int hashFormat = obj.Format.GetHashCode();
            return HashCode.Combine(hashTitle, hashFormat);
        }
    }
}