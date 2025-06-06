using System.Globalization;

namespace Tsundoku.Helpers
{
    public static class StaticExtensionMethods
    {
        
    }

    public class ExtensionMethods
    {
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
}