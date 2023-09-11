using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tsundoku.Helpers
{
    public class ExtensionMethods
    {    
		/// <summary>
		/// Last Modified: 06 March, 2023
		/// Author: Sigrec (Sean. N)
		/// Compares the titles of various entries for a series against each other to determine if they are similar enough to be cosnidered equal,
		/// allowing comparisons of titles who are technically the same but there websites have slightly different formats.
		/// It determine if they are similar enough by a threshold of 1/4 the size of longest title being the same, meaning based on the
		/// number of characters that do not match if it's is less than 1/4 the size of the longest title string.
		/// </summary>
		/// <param name="titleOne">The first title in the comparison and is used for determining when to stop traversing</param>
		/// <param name="titleTwo">The 2nd title in the comparison</param>
		/// <returns></returns>
        public static bool Similar(string titleOne, string titleTwo)
        {
            int count = titleOne.Length == 0 || titleTwo.Length == 0 ? -1 : 0; // The amount of times that the characters and there "alignment" don't match
            int titleOnePointer = 0, titleTwoPointer = 0; // Pointers for the characters in both strings
            titleOne = RemoveInPlaceCharArray(titleOne.ToLower());
            titleTwo = RemoveInPlaceCharArray(titleTwo.ToLower());

            while (titleOnePointer < titleOne.Length && titleTwoPointer < titleTwo.Length) // Keep traversing until u reach the end of titleOne's string
            {
                if (titleOne[titleOnePointer] != titleTwo[titleTwoPointer]) // Checks to see if the characters match
                {
                    count++; // There is 1 additional character difference
                    for (int z = titleOnePointer; z < titleOne.Length; z++) // Start at the index of where the characters were not the same, then traverse the other string to see if it matches
                    {
                        if (titleOne[z] == titleTwo[titleTwoPointer]) // Checks to see if the character is present in the other string and is in a similar position
                        {
                            break;
                        }
                    }
                } 
                else // Characters do match so just move to the next set of characters to compare in the strings
                {
                    titleOnePointer++;
                }
                titleTwoPointer++;
            }
            return count != -1 && count <= (titleOne.Length > titleTwo.Length ? titleTwo.Length / 5 : titleOne.Length / 5); // Determine if they are similar enough by a threshold of 1/5 the size of longest title
        }

		public static string RemoveInPlaceCharArray(string input)
		{
			var len = input.Length;
			var src = input.ToCharArray();
			int dstIdx = 0;
			for (int i = 0; i < len; i++)
			{
				var ch = src[i];
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
            List<string> list = new List<string>();
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
    }
}