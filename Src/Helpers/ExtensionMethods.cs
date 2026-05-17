using System.Buffers;
using Avalonia.Media.Imaging;
using Microsoft.IO;

namespace Tsundoku.Helpers;

/// <summary>
/// Provides extension methods and utility functions for string manipulation, bitmap cloning, and collection comparison.
/// </summary>
public static class ExtensionMethods
{
    private static readonly SearchValues<char> SmartQuoteChars = SearchValues.Create(
    [
        '\u201C', '\u201D', '\u201E', '\u201F',
        '\u2018', '\u2019', '\u201A', '\u201B',
    ]);

    private static readonly SearchValues<char> WhitespaceChars = SearchValues.Create(
    [
        '\u0020', '\u00A0', '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u202F', '\u205F', '\u3000', '\u2028', '\u2029', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u0085',
    ]);

    /// <summary>
    /// Replaces all common smart double-quotes and single-quotes with
    /// plain ASCII " and ' in one pass over the input.
    /// </summary>
    public static string NormalizeQuotes(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (input.AsSpan().IndexOfAny(SmartQuoteChars) < 0)
            return input;

        return string.Create(input.Length, input, (span, src) =>
        {
            for (int i = 0; i < src.Length; i++)
            {
                char c = src[i];
                span[i] = c switch
                {
                    '\u201C' or '\u201D' or '\u201E' or '\u201F' => '"',
                    '\u2018' or '\u2019' or '\u201A' or '\u201B' => '\'',
                    _ => c,
                };
            }
        });
    }

    /// <summary>
    /// Creates a deep copy of the given Avalonia Bitmap by encoding it to PNG in memory
    /// and decoding it back into a new Bitmap instance.
    /// </summary>
    public static Bitmap? CloneBitmap(this Bitmap? source)
    {
        if (source is null)
        {
            return null;
        }

        using RecyclableMemoryStream stream = BitmapHelper.StreamManager.GetStream(nameof(CloneBitmap));
        source.Save(stream);
        stream.Position = 0;
        return new Bitmap(stream);
    }

    /// <summary>
    /// Computes the Levenshtein edit distance between two strings, returning -1 if it exceeds the maximum allowed distance.
    /// </summary>
    /// <param name="s">The first string to compare.</param>
    /// <param name="t">The second string to compare.</param>
    /// <param name="maxDistance">The maximum edit distance threshold; results beyond this return -1.</param>
    /// <returns>The edit distance if within the threshold; otherwise, -1.</returns>
    public static int Similar(string? s, string? t, int maxDistance)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            if (string.IsNullOrEmpty(t)) return 0;
            return t.Length <= maxDistance ? t.Length : -1;
        }

        if (string.IsNullOrWhiteSpace(t))
        {
            return s.Length <= maxDistance ? s.Length : -1;
        }

        ReadOnlySpan<char> sSpan = s;
        ReadOnlySpan<char> tSpan = t;

        // Always operate on the shorter string
        if (sSpan.Length > tSpan.Length)
        {
            ReadOnlySpan<char> tmp = sSpan;
            sSpan = tSpan;
            tSpan = tmp;
        }

        int sLen = sSpan.Length;
        int tLen = tSpan.Length;

        if (tLen - sLen > maxDistance)
        {
            return -1;
        }

        Span<int> previousRow = stackalloc int[tLen + 1];
        Span<int> currentRow = stackalloc int[tLen + 1];

        for (int j = 0; j <= tLen; j++)
        {
            previousRow[j] = j;
        }

        for (int i = 1; i <= sLen; i++)
        {
            currentRow[0] = i;
            int bestThisRow = currentRow[0];

            char sChar = char.ToLowerInvariant(sSpan[i - 1]);
            for (int j = 1; j <= tLen; j++)
            {
                char tChar = char.ToLowerInvariant(tSpan[j - 1]);

                int cost = sChar == tChar ? 0 : 1;
                int insert = currentRow[j - 1] + 1;
                int delete = previousRow[j] + 1;
                int replace = previousRow[j - 1] + cost;

                currentRow[j] = Math.Min(Math.Min(insert, delete), replace);

                bestThisRow = Math.Min(bestThisRow, currentRow[j]);
            }

            if (bestThisRow > maxDistance)
            {
                return -1;
            }

            Span<int> temp = previousRow;
            previousRow = currentRow;
            currentRow = temp;
        }

        int result = previousRow[tLen];
        return result <= maxDistance ? result : -1;
    }

    /// <summary>
    /// Computes a similarity threshold for fuzzy title matching based on the lengths of the input and comparison strings.
    /// </summary>
    /// <param name="input">The user-provided or primary string to compare.</param>
    /// <param name="compareTo">The secondary string (usually a known title) to compare against.</param>
    /// <returns>
    /// An integer threshold value used in similarity matching,
    /// typically 1/6 of the shorter string's length.
    /// </returns>
    public static int SimilarThreshold(string input, string compareTo)
    {
        if (!string.IsNullOrWhiteSpace(compareTo) && input.Length > compareTo.Length)
        {
            return compareTo.Length / 6;
        }
        else
        {
            return input.Length / 6;
        }
    }

    /// <summary>
    /// Removes all Unicode whitespace characters from the input string in a single pass.
    /// </summary>
    /// <param name="input">The string to strip whitespace from.</param>
    /// <returns>A new string with all whitespace characters removed.</returns>
    public static string RemoveInPlaceCharArray(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        ReadOnlySpan<char> src = input;
        if (src.IndexOfAny(WhitespaceChars) < 0) return input;

        const int StackThreshold = 512;
        char[]? rented = src.Length > StackThreshold ? ArrayPool<char>.Shared.Rent(src.Length) : null;
        Span<char> dst = rented is not null ? rented.AsSpan(0, src.Length) : stackalloc char[src.Length];

        int dstIdx = 0;
        foreach (char ch in src)
        {
            if (!WhitespaceChars.Contains(ch))
            {
                dst[dstIdx++] = ch;
            }
        }

        string result = new(dst[..dstIdx]);
        if (rented is not null) ArrayPool<char>.Shared.Return(rented);
        return result;
    }

    /// <summary>
    /// Returns true if both dictionaries are null, or both non-null with identical key sets
    /// and equal values for each key.
    /// </summary>
    public static bool DictionariesEqual<TKey, TValue>(
        Dictionary<TKey, TValue>? a,
        Dictionary<TKey, TValue>? b)
        where TKey : notnull
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null || (a.Count != b.Count))
        {
            return false;
        }

        // For each key in a, check that b has the same key and identical value
        foreach (KeyValuePair<TKey, TValue> kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out TValue? bValue) || !Equals(kvp.Value, bValue))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns true if both sets are null, or both non-null with the same elements.
    /// </summary>
    public static bool HashSetsEqual<T>(HashSet<T>? a, HashSet<T>? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.SetEquals(b);
    }
}
