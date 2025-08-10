using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using static Tsundoku.Models.Enums.SeriesFormatModel;

namespace Tsundoku.Helpers;

public static partial class GoodreadsParser
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    [GeneratedRegex(@"(?:(?=, Vol)|:|Omnibus|Deluxe|Vol\.|Volume|(?<!By.*) -|Manga|, Tome|Tome|Shonan|, Master Edition|Black Edition|\[.*\]|\d{1,3} by|Box Set|\s*\d+\s*$|Complete Box Set|Perfect Collection|\(.*\)).*", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex TitleCleanRegex();

    [GeneratedRegex(@"^.*-|(?:(?=, Vol)|:|Omnibus|Deluxe|Vol\.|Volume|Manga|, Tome|Tome|Shonan|, Master Edition|Black Edition|\[.*\]|\d{1,3} by|Box Set|\s*\d+\s*$|Complete Box Set|Perfect Collection|\(.*\)).*", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex PrefixTitleCleanRegex();

    [GeneratedRegex(@"llc", RegexOptions.IgnoreCase)]
    private static partial Regex PublisherCleanRegex();

    public static async Task<Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>?> ExtractUniqueTitles(string[]? csvFilePaths = null)
    {
        LOGGER.Info("Attempting to parse Libib csv files...");
        if (csvFilePaths is null)
        {
            LOGGER.Warn("User tried to import non existent file paths");
            return null;
        }

        Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint> result = new(new TitleFormatComparer());
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

                using CsvReader csv = new(reader, config);
                if (csv.Read()) csv.ReadHeader(); // Read the first row (which should be the header)
                else
                {
                    // If the file is empty (no rows at all), skip to the next file
                    LOGGER.Warn("User imported csv file {File} is empty", path);
                    continue;
                }

                while (csv.Read())
                {
                    string? rawTitle = csv.GetField("Title");
                    if (string.IsNullOrWhiteSpace(rawTitle)) continue;

                    string binding = csv.ParseCsvString("Binding", string.Empty);
                    ReadOnlySpan<char> bindingSpan = binding.AsSpan();
                    if (binding.Contains("Kindle", StringComparison.OrdinalIgnoreCase)) continue;

                    if (!decimal.TryParse(csv.ParseCsvString("My Rating", "0"), out decimal rating)) rating = 0m;
                    string publisher = csv.ParseCsvString("Publisher", "Unknown");
                    // uint curVols = uint.Parse(ParserHelpers.ParseCsvString("Owned Copies", "0"));

                    ReadOnlySpan<char> titleSpan = rawTitle.AsSpan();

                    // NOTE: Currently there is no way to actually determine if a series is a manga or novel
                    SeriesFormat format = titleSpan.Contains("light novel", StringComparison.OrdinalIgnoreCase) ? SeriesFormat.Novel : SeriesFormat.Manga;

                    if (result.AsValueEnumerable().Any(k =>
                        k.Key.Title.Contains(rawTitle, StringComparison.OrdinalIgnoreCase) &&
                        k.Key.Format == format))
                    {
                        continue;
                    }

                    if (!publisher.Equals("Unknown"))
                    {
                        ReadOnlySpan<char> publisherSpan = publisher.AsSpan();
                        if (publisherSpan.Contains("VIZ Media", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "Viz Media";
                        }
                        else if (publisherSpan.Contains("Dark Horse", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "Dark Horse";
                        }
                        else if (publisherSpan.Contains("Kodansha", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "Kodansha";
                        }
                        else if (publisherSpan.Contains("Tokyopop", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "TOKYOPOP";
                        }
                        else if (publisherSpan.Contains("JNovel", StringComparison.OrdinalIgnoreCase) || publisherSpan.Contains("J-Novel", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "J-Novel Club";
                        }
                        else if (publisherSpan.Contains("Vertical", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "Vertical Comics";
                        }
                        else if (publisherSpan.Contains("Kana", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "Kana";
                        }
                        else if (publisherSpan.Contains("Kodama", StringComparison.OrdinalIgnoreCase))
                        {
                            publisher = "Kodama";
                        }
                    }

                    string cleanedTitle;
                    if (!titleSpan.StartsWith("By", StringComparison.OrdinalIgnoreCase))
                    {
                        cleanedTitle = TitleCleanRegex().Replace(rawTitle, string.Empty);
                    }
                    else
                    {
                        cleanedTitle = PrefixTitleCleanRegex().Replace(rawTitle, string.Empty);
                    }

                    (string Title, SeriesFormat Format, string Publisher, decimal Rating) entry = (HttpUtility.HtmlDecode(cleanedTitle).Trim(), format, PublisherCleanRegex().Replace(publisher, string.Empty).Trim(), rating);
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
            LOGGER.Warn("User tried to import goodreads data from file(s) [{Files}] but none was returned from parse", string.Join(',', csvFilePaths));
            return null;
        }

        return result;
    }

    private sealed class TitleFormatComparer : IEqualityComparer<(string Title, SeriesFormat Format, string Publisher, decimal Rating)>
    {
        public bool Equals((string Title, SeriesFormat Format, string Publisher, decimal Rating) x, (string Title, SeriesFormat Format, string Publisher, decimal Rating) y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(x.Title, y.Title) && x.Format == y.Format;
        }

        public int GetHashCode((string Title, SeriesFormat Format, string Publisher, decimal Rating) obj)
        {
            int hashTitle = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Title);
            int hashFormat = obj.Format.GetHashCode();
            return HashCode.Combine(hashTitle, hashFormat);
        }
    }
}