using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Tsundoku.Source;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Platform;

namespace Tsundoku.Models
{
    [Serializable]
    public class Series
    {
		public string EnglishTitle { get; }
		public string RomajiTitle { get; }
		public string NativeTitle { get; }
		public string RomajiStaff { get; }
		public string NativeStaff { get; }
		public string Description { get; }
		public string Format { get; }
		public string Status { get; }
		public string Cover { get; set; }
		public string Link { get; }
		public string UserNotes { get; set; }
		public ushort MaxVolumeCount { get; set; }
		public ushort CurVolumeCount { get; set; }

		public Series(string englishTitle, string romajiTitle, string nativeTitle, string romajiStaff, string nativeStaff, string description, string format, string status, string cover, string link, ushort maxVolumeCount, ushort curVolumeCount)
        {
            EnglishTitle = englishTitle;
            RomajiTitle = romajiTitle;
            NativeTitle = nativeTitle;
            RomajiStaff = romajiStaff;
            NativeStaff = nativeStaff;
            Description = description;
            Format = format;
            Status = status;
            Cover = cover;
            Link = link;
            MaxVolumeCount = maxVolumeCount;
            CurVolumeCount = curVolumeCount;
        }

		public static Series? CreateNewSeriesCard(string title, string bookType, ushort maxVolCount, ushort minVolCount)
        {
			Task<JObject?> seriesDataQuery = new GraphQLQuery().GetSeries(title, bookType);
            seriesDataQuery.Wait();

			if (seriesDataQuery.Result != null)
            {
				JObject finalObj = JObject.Parse(seriesDataQuery.Result.ToString());
				var seriesData = finalObj["Media"];
				if (seriesData != null)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
					string romajiTitle = seriesData["title"]["romaji"].ToString();
					Series _newSeries = new Series(
                        string.IsNullOrEmpty(seriesData["title"]["english"].ToString()) ? seriesData["title"]["romaji"].ToString() : seriesData["title"]["english"].ToString(),
						romajiTitle,
                        seriesData["title"]["native"].ToString(),
                        GetSeriesStaff(seriesData["staff"]["edges"], "full"),
						GetSeriesStaff(seriesData["staff"]["edges"], "native"),
						seriesData["description"] == null ? "" : ConvertUnicodeInDesc(Regex.Replace(seriesData["description"].ToString(), @"\(Source: [\S\s]+|\<.*?\>", "").Trim()),
						bookType.Equals("MANGA") ? GetCorrectComicName(seriesData["countryOfOrigin"].ToString()) : "Novel",
						GetSeriesStatus(seriesData["status"].ToString()),
						SaveNewCoverImage(seriesData["coverImage"]["extraLarge"].ToString(), romajiTitle, bookType),
						seriesData["siteUrl"].ToString(),
						maxVolCount,
						minVolCount);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
					return _newSeries;
				}
			}
            else
            {
				Debug.WriteLine("Invalid Series Type And/OR BookType");
            }
            return null;
        }

		private static string ConvertUnicodeInDesc(string seriesDesc)
		{
			foreach (Match unicodeMatch in Regex.Matches(seriesDesc, @"(&#(\d+);)"))
			{
				seriesDesc = seriesDesc.Replace(unicodeMatch.Groups[1].Value, Convert.ToChar(Convert.ToUInt16(unicodeMatch.Groups[2].Value)).ToString());
			}
			return seriesDesc;
		}

		public static string GetCorrectComicName(string jsonCountryOfOrigin)
		{
			return jsonCountryOfOrigin switch
			{
				"KR" => "Manhwa",
				"CN" => "Manhua",
				"FR" => "Manfra",
				_ => "Manga"
			};
		}

		public static String GetSeriesStatus(String jsonStatus)
		{
			return jsonStatus switch
			{
				"RELEASING" => "Ongoing",
				"FINISHED" => "Complete",
				"CANCELLED" => "Cancelled",
				"HIATUS" => "Hiatus",
				"NOT_YET_RELEASED" => "Coming Soon",
				_ => "Error",
			};
		}

        public static string SaveNewCoverImage(String coverLink, String title, String bookType)
        {
            string newPath = @"\Tsundoku\Assets\Covers\" + Regex.Replace(title, @"[^A-Za-z\d]", "") + "_" + bookType + ".png"; //C:\Tsundoku\Assets\Covers\Ottoman_Manga.png
            Debug.WriteLine(newPath);

            if (!File.Exists(newPath))
            {
                try
                {
                    DirectoryInfo newDir = Directory.CreateDirectory(@"\Tsundoku\Covers\");
                    HttpClient client = new HttpClient();
                    var response = Task.Run(async () => await client.GetAsync(new Uri(coverLink)));
                    response.Wait();
                    var clientResponse = response.Result;
                    using (var fs = new FileStream(newPath, FileMode.CreateNew))
                    {
                        var fileResponse = Task.Run(async () => await clientResponse.Content.CopyToAsync(fs));
                        fileResponse.Wait();
                    }
                }
                catch (Exception e)
                {
                    Console.Error.Write(e.ToString());
                }
            }
			return newPath;
        }

        public static string GetSeriesStaff(JToken staffArray, string nameType) {
			StringBuilder staffList = new StringBuilder();
			string[] validRoles = { "Story & Art", "Story", "Art", "Original Creator", "Character Design", "Illustration", "Mechanical Design" };
			foreach(JToken name in staffArray)
            {
				if (validRoles.Contains(name["role"].ToString()))
                {
					if (nameType.Equals("full"))
                    {
						staffList.Append(name["node"]["name"]["full"] + " | ");
                    }
					else if (nameType.Equals("native"))
                    {
						staffList.Append(name["node"]["name"]["native"] + " | ");
					}
                }
            }
			return staffList.ToString(0, staffList.Length - 3);
		}

		public override string ToString()
		{
			return "Series{" + "\n" +
					"EnglishTitle=" + EnglishTitle  + "\n" +
					"RomajiTitle=" + RomajiTitle  + "\n" +
					"NativeTitle=" + NativeTitle  + "\n" +
					"RomajiStaff=" + RomajiStaff  + "\n" +
					"NativeStaff=" + NativeStaff  + "\n" +
					"Description=" + Description  + "\n" +
					"Format=" + Format  + "\n" +
					"Status=" + Status  + "\n" +
					"Cover=" + Cover  + "\n" +
					"Link=" + Link  + "\n" +
					"MaxVolumeCount=" + MaxVolumeCount  + "\n" +
					"CurVolumeCount=" + CurVolumeCount  + "\n" +
					'}';
		}
	}
}
