using System.Globalization;

namespace Tsundoku.Helpers;

public static class StaticExtensionMethods
{
    
}

public static class ExtensionMethods
{
    public static int Similar(string s, string t, int maxDistance)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return string.IsNullOrEmpty(t) || t.Length <= maxDistance ? t.Length : -1;
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

    public static string RemoveInPlaceCharArray(string input)
    {
        int len = input.Length;
        char[] src = input.ToCharArray();
        int dstIdx = 0;
        for (int i = 0; i < len; i++)
        {
            char ch = src[i];
            switch (ch)
            {
                case '\u0020':
                case '\u00A0':
                case '\u1680':
                case '\u2000':
                case '\u2001':
                case '\u2002':
                case '\u2003':
                case '\u2004':
                case '\u2005':
                case '\u2006':
                case '\u2007':
                case '\u2008':
                case '\u2009':
                case '\u200A':
                case '\u202F':
                case '\u205F':
                case '\u3000':
                case '\u2028':
                case '\u2029':
                case '\u0009':
                case '\u000A':
                case '\u000B':
                case '\u000C':
                case '\u000D':
                case '\u0085':
                    continue;
                default:
                    src[dstIdx++] = ch;
                    break;
            }
        }
        return new string(src, 0, dstIdx);
    }

    public static void PrintCultures()
    {
        List<string> list = [];
        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
        {
            string specName = "(none)";
            try
            {
                specName = CultureInfo.CreateSpecificCulture(ci.Name).Name;
            }
            catch { }
            list.Add(string.Format("{0,-12}{1,-12}{2}", ci.Name, specName, ci.EnglishName));
        }

        list.Sort();  // sort by name

        using (StreamWriter outputFile = new StreamWriter(@"CurrentCultures.txt"))
        {
            // write to file
            outputFile.WriteLine("CULTURE   SPEC.CULTURE  ENGLISH NAME");
            outputFile.WriteLine("--------------------------------------------------------------");
            foreach (string str in list)
            {
                outputFile.WriteLine(str);
            }
        }
    }

    public static void PrintCurrencySymbols()
    {
        string[] symbols = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(ci => ci.Name).Distinct().Select(id => new RegionInfo(id)).Select(r => r.CurrencySymbol).Distinct().ToArray();
        using (StreamWriter outputFile = new StreamWriter(@"CurrencySymbols.txt"))
        {
            // write to file
            foreach (string str in symbols)
            {
                outputFile.WriteLine(str);
            }
        }
    }
    
    /// <summary>
    /// Returns true if both dictionaries are null, or both non-null with identical key sets 
    /// and equal values for each key.
    /// </summary>
    public static bool DictionariesEqual<TKey, TValue>(
        Dictionary<TKey, TValue>? a,
        Dictionary<TKey, TValue>? b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        if (a.Count != b.Count)
            return false;

        // For each key in a, check that b has the same key and identical value
        foreach (KeyValuePair<TKey, TValue> kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out TValue? bValue))
                return false;

            if (!Equals(kvp.Value, bValue))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if both sets are null, or both non-null with the same elements.
    /// </summary>
    public static bool HashSetsEqual<T>(HashSet<T>? a, HashSet<T>? b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.SetEquals(b);
    }
}