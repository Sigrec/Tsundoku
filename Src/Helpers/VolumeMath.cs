using System.Globalization;

namespace Tsundoku.Helpers;

/// <summary>
/// Volume-number logic for owned-volume tracking. Main volumes are whole numbers from 1 up; every
/// other number (0, 18.5, 0.5) is a special volume that sits outside the main count.
/// </summary>
public static class VolumeMath
{
    /// <summary>Above this many main volumes the card strip draws without gaps between segments.</summary>
    public const int DenseStripThreshold = 40;

    /// <summary>Whether <paramref name="volume"/> is a main volume (a whole number of 1 or more).</summary>
    public static bool IsMain(decimal volume) => volume >= 1m && decimal.Truncate(volume) == volume;

    /// <summary>Volumes 1 through <paramref name="count"/>.</summary>
    public static List<decimal> Sequence(uint count)
    {
        List<decimal> volumes = [with((int)count)];
        for (uint v = 1; v <= count; v++)
        {
            volumes.Add(v);
        }
        return volumes;
    }

    /// <summary>Sorted, de-duplicated, non-negative volume numbers rounded to two decimals.</summary>
    public static List<decimal> Normalize(IEnumerable<decimal>? volumes)
    {
        if (volumes is null)
        {
            return [];
        }

        SortedSet<decimal> set = [];
        foreach (decimal volume in volumes)
        {
            if (volume >= 0m)
            {
                set.Add(decimal.Round(volume, 2));
            }
        }
        return [.. set];
    }

    /// <summary>Only the special (non-main) numbers from <paramref name="volumes"/>, normalized.</summary>
    public static List<decimal> NormalizeSpecials(IEnumerable<decimal>? volumes)
    {
        List<decimal> normalized = Normalize(volumes);
        normalized.RemoveAll(IsMain);
        return normalized;
    }

    public static uint CountMain(IReadOnlyList<decimal> owned)
    {
        uint count = 0;
        foreach (decimal volume in owned)
        {
            if (IsMain(volume))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>The highest owned main volume, or 0 when none are owned.</summary>
    public static uint HighestMain(IReadOnlyList<decimal> owned)
    {
        for (int i = owned.Count - 1; i >= 0; i--)
        {
            if (IsMain(owned[i]))
            {
                return (uint)owned[i];
            }
        }
        return 0;
    }

    /// <summary>
    /// The last main volume the series is expected to have: <paramref name="maxVolumeCount"/>, or the
    /// highest owned volume when the max is unknown (0).
    /// </summary>
    public static uint MainRunEnd(IReadOnlyList<decimal> owned, uint maxVolumeCount) =>
        maxVolumeCount > 0 ? maxVolumeCount : HighestMain(owned);

    /// <summary>Main volumes 1..max that aren't owned.</summary>
    public static List<decimal> MissingMain(IReadOnlyList<decimal> owned, uint maxVolumeCount)
    {
        HashSet<decimal> ownedSet = [.. owned];
        uint end = MainRunEnd(owned, maxVolumeCount);
        List<decimal> missing = [];
        for (uint v = 1; v <= end; v++)
        {
            if (!ownedSet.Contains(v))
            {
                missing.Add(v);
            }
        }
        return missing;
    }

    /// <summary>Special volumes listed for the series that aren't owned.</summary>
    /// <summary>
    /// How many main volumes in 1..<see cref="MainRunEnd"/> are not owned; the count of
    /// <see cref="MissingMain"/> without building the list. <paramref name="owned"/> must be
    /// normalized (no duplicates), as <see cref="Normalize"/> returns it.
    /// </summary>
    public static int CountMissingMain(IReadOnlyList<decimal> owned, uint maxVolumeCount)
    {
        uint end = MainRunEnd(owned, maxVolumeCount);
        uint ownedInRun = 0;
        foreach (decimal volume in owned)
        {
            if (IsMain(volume) && volume <= end)
            {
                ownedInRun++;
            }
        }
        return (int)(end - ownedInRun);
    }

    public static List<decimal> MissingSpecials(IReadOnlyList<decimal> owned, IReadOnlyList<decimal> specials)
    {
        HashSet<decimal> ownedSet = [.. owned];
        List<decimal> missing = [];
        foreach (decimal special in specials)
        {
            if (!ownedSet.Contains(special))
            {
                missing.Add(special);
            }
        }
        return missing;
    }

    public static int CountOwnedSpecials(IReadOnlyList<decimal> owned, IReadOnlyList<decimal> specials)
    {
        HashSet<decimal> ownedSet = [.. owned];
        int count = 0;
        foreach (decimal special in specials)
        {
            if (ownedSet.Contains(special))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Whether something below the highest owned main volume is missing: a skipped main volume or a
    /// listed special. A series owned straight through from 1 has no gaps even if it isn't complete.
    /// </summary>
    public static bool HasGaps(IReadOnlyList<decimal> owned, IReadOnlyList<decimal> specials)
    {
        uint highest = HighestMain(owned);
        if (highest == 0)
        {
            return false;
        }

        HashSet<decimal> ownedSet = [.. owned];
        for (uint v = 1; v < highest; v++)
        {
            if (!ownedSet.Contains(v))
            {
                return true;
            }
        }
        foreach (decimal special in specials)
        {
            if (special < highest && !ownedSet.Contains(special))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// The volume the card's + button adds: the one after the highest owned, or the lowest gap once the
    /// highest owned is already the last volume. <c>null</c> when every main volume is owned.
    /// </summary>
    public static decimal? NextToAdd(IReadOnlyList<decimal> owned, uint maxVolumeCount)
    {
        uint highest = HighestMain(owned);
        if (maxVolumeCount == 0 || highest < maxVolumeCount)
        {
            return highest + 1;
        }

        List<decimal> missing = MissingMain(owned, maxVolumeCount);
        return missing.Count > 0 ? missing[0] : null;
    }

    public static string Format(decimal volume) => volume.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// Formats volumes for display or editing: consecutive main volumes collapse into ranges and
    /// specials are listed on their own, e.g. <c>1-46, 48-100, 18.5</c>.
    /// </summary>
    public static string FormatRanges(IEnumerable<decimal> volumes, string rangeSeparator = "-")
    {
        List<decimal> sorted = Normalize(volumes);
        List<decimal> mains = [.. sorted.Where(IsMain)];
        List<string> parts = [];
        int i = 0;
        while (i < mains.Count)
        {
            decimal start = mains[i];
            decimal end = start;
            while (i + 1 < mains.Count && mains[i + 1] == end + 1m)
            {
                i++;
                end = mains[i];
            }
            parts.Add(start == end ? Format(start) : $"{Format(start)}{rangeSeparator}{Format(end)}");
            i++;
        }

        foreach (decimal volume in sorted)
        {
            if (!IsMain(volume))
            {
                parts.Add(Format(volume));
            }
        }
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Parses text like <c>1-46, 48-100, 18.5</c>. Ranges are whole numbers within 1..max (any
    /// upper bound when max is 0); single entries may be specials such as 18.5 or 0.
    /// </summary>
    /// <returns><c>null</c> on success, otherwise a message describing the first bad entry.</returns>
    public static string? TryParseRanges(string text, uint maxVolumeCount, out List<decimal> volumes)
    {
        volumes = [];
        List<decimal> parsed = [];
        foreach (string rawPart in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            int dash = rawPart.IndexOfAny(['-', '–']);
            if (dash > 0)
            {
                if (!uint.TryParse(rawPart.AsSpan(0, dash).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out uint start)
                    || !uint.TryParse(rawPart.AsSpan(dash + 1).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out uint end))
                {
                    return $"Couldn't read \"{rawPart}\". Ranges use whole numbers, like 1-46";
                }
                if (start < 1 || start > end || (maxVolumeCount > 0 && end > maxVolumeCount))
                {
                    return maxVolumeCount > 0
                        ? $"\"{rawPart}\" is outside 1–{maxVolumeCount}"
                        : $"\"{rawPart}\" isn't a valid range";
                }
                for (uint v = start; v <= end; v++)
                {
                    parsed.Add(v);
                }
                continue;
            }

            if (!decimal.TryParse(rawPart, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal single)
                || decimal.Round(single, 2) != single)
            {
                return $"Couldn't read \"{rawPart}\". Use numbers, ranges like 1-46, or specials like 18.5";
            }
            if (IsMain(single) && maxVolumeCount > 0 && single > maxVolumeCount)
            {
                return $"\"{rawPart}\" is outside 1–{maxVolumeCount}";
            }
            parsed.Add(single);
        }

        volumes = Normalize(parsed);
        return null;
    }
}
