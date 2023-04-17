using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Tsundoku.Helpers
{
    public class ExtensionMethods
    {    
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

		private void PrintCultures()
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
                list.Add(String.Format("{0,-12}{1,-12}{2}", ci.Name, specName, ci.EnglishName));
            }

            list.Sort();  // sort by name

            using (StreamWriter outputFile = new StreamWriter(@"CurrentCultures.txt"))
            {
                // write to console
                outputFile.WriteLine("CULTURE   SPEC.CULTURE  ENGLISH NAME");
                outputFile.WriteLine("--------------------------------------------------------------");
                foreach (string str in list)
                {
                    outputFile.WriteLine(str);
                }
            } 
        }
    }
}