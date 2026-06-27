using System.Diagnostics.CodeAnalysis;

namespace Tsundoku.Helpers;

public enum SeriesUrlSource
{
    AniList,
    MangaDex
}

public sealed record SeriesUrlInfo(SeriesUrlSource Source, string Id);

/// <summary>
/// Detects supported manga/light-novel URLs and extracts the series identifier so the
/// Add-Series flow can prefill the input with the source's native ID. Uses the same
/// segment-walk pattern as <see cref="Tsundoku.Models.Series.GetLinkId"/>.
/// </summary>
public static class SeriesUrlParser
{
    public static bool TryParse(string? input, [NotNullWhen(true)] out SeriesUrlInfo? info)
    {
        info = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!Uri.TryCreate(input.Trim(), UriKind.Absolute, out Uri? uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;

        string host = uri.Host;
        if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
        {
            host = host[4..];
        }

        return host switch
        {
            "anilist.co" => TryExtract(uri, "manga", static s => uint.TryParse(s, out _), SeriesUrlSource.AniList, out info),
            "mangadex.org" => TryExtract(uri, "title", Tsundoku.Clients.MangaDex.IsMangaDexId, SeriesUrlSource.MangaDex, out info),
            _ => false
        };
    }

    private static bool TryExtract(
        Uri uri,
        string marker,
        Func<string, bool> idValidator,
        SeriesUrlSource source,
        [NotNullWhen(true)] out SeriesUrlInfo? info)
    {
        info = null;
        string[] segments = uri.Segments;
        for (int i = 0; i < segments.Length - 1; i++)
        {
            string seg = segments[i].TrimEnd('/');
            if (seg.Equals(marker, StringComparison.OrdinalIgnoreCase))
            {
                string candidate = segments[i + 1].TrimEnd('/');
                if (idValidator(candidate))
                {
                    info = new SeriesUrlInfo(source, candidate);
                    return true;
                }
                return false;
            }
        }
        return false;
    }
}
