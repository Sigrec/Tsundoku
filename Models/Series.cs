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
using System.Collections.Generic;

namespace Tsundoku.Models
{
	[Serializable]
	public class Series
	{
		public List<string> Titles { get; } //[Romaji, English, Native]
        public List<string> Staff { get; } //[Romaji, Native]
		public string Description { get; }
		public string Format { get; }
		public string Status { get; }
		public string Cover { get; set; }
		public string Link { get; }
		public string SeriesNotes { get; set; }
		public ushort MaxVolumeCount { get; set; }
		public ushort CurVolumeCount { get; set; }

        public Series(List<string> titles, List<string> staff, string description, string format, string status, string cover, string link, ushort maxVolumeCount, ushort curVolumeCount)
        {
			Titles = titles;
			Staff = staff;
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
			Task<JObject?> seriesDataQuery = new AniListQuery().GetSeries(title, bookType);
            seriesDataQuery.Wait();

			if (seriesDataQuery.Result != null)
            {
				JObject finalObj = JObject.Parse(seriesDataQuery.Result.ToString());
				var seriesData = finalObj["Media"];
				if (seriesData != null)
				{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
					string romajiTitle = seriesData["title"]["romaji"].ToString();
					string nativeStaff = GetSeriesStaff(seriesData["staff"]["edges"], "native");
					string fullStaff = GetSeriesStaff(seriesData["staff"]["edges"], "full");
					string filteredBookType = bookType.Equals("MANGA") ? GetCorrectComicName(seriesData["countryOfOrigin"].ToString()) : "Novel";
					Series _newSeries = new Series(
                        new List<string>()
						{
                            romajiTitle,
                            string.IsNullOrEmpty(seriesData["title"]["english"].ToString()) ? romajiTitle : seriesData["title"]["english"].ToString(),
							seriesData["title"]["native"].ToString()
						},
                        new List<string>()
						{
							fullStaff,
							nativeStaff.Equals(" | ") ? fullStaff : nativeStaff,
						},
						seriesData["description"] == null ? "" : ConvertUnicodeInDesc(Regex.Replace(seriesData["description"].ToString(), @"\(Source: [\S\s]+|\<.*?\>", "").Trim()),
						filteredBookType,
						GetSeriesStatus(seriesData["status"].ToString()),
						SaveNewCoverImage(seriesData["coverImage"]["extraLarge"].ToString(), romajiTitle, filteredBookType.ToUpper()),
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
				"JP" => "Manga",
				_ => "Error"
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
            string newPath = @$"Assets\Covers\{Regex.Replace(title, @"[^A-Za-z\d]", "")}_{bookType}.{coverLink.Substring(coverLink.Length - 3)}";

            if (!File.Exists(newPath))
            {
                try
                {
                    //DirectoryInfo newDir = Directory.CreateDirectory(@"\Tsundoku\Assets\Covers\");
                    HttpClient client = new HttpClient();
                    Task<HttpResponseMessage> response = Task.Run(async () => await client.GetAsync(new Uri(coverLink)));
                    response.Wait();
                    HttpResponseMessage clientResponse = response.Result;
                    using (var fs = new FileStream(newPath, FileMode.CreateNew))
                    {
                        Task fileResponse = Task.Run(async () => await clientResponse.Content.CopyToAsync(fs));
                        fileResponse.Wait();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
			return newPath;
        }

        public static string GetSeriesStaff(JToken staffArray, string nameType) {
			StringBuilder staffList = new StringBuilder();
			string[] validRoles = { "Story & Art", "Story", "Art", "Original Creator", "Character Design", "Illustration", "Mechanical Design", "Original Story" };
			foreach(JToken name in staffArray)
            {
				if (validRoles.Contains(Regex.Replace(name["role"].ToString(), @" \(.*\)", "")))
                {
					JToken newStaff = name["node"]["name"][nameType];
					if (!string.IsNullOrEmpty((string?)newStaff))
					{
						staffList.Append(newStaff + " | ");
					}
					else
                    {
						if (nameType.Equals("native"))
						{
							staffList.Append(name["node"]["name"]["full"] + " | ");
						}
						else // If the staff member does not have a full name entry
						{
							staffList.Append(name["node"]["name"]["native"] + " | ");
						}
                    }
                }
            }
			return staffList.ToString(0, staffList.Length - 3);
		}

		public override string ToString()
		{
			return "Series\n{\n" +
					"Titles = " + Titles[0] + " | " + Titles[1] + " | " + Titles[2] + "\n" +
					"Staff = " + Staff[0] + " | " + Staff[1] + "\n" +
					"Description = " + Description  + "\n" +
					"Format = " + Format  + "\n" +
					"Status = " + Status  + "\n" +
					"Cover = " + Cover  + "\n" +
					"Link = " + Link  + "\n" +
					"MaxVolumeCount = " + MaxVolumeCount  + "\n" +
					"CurVolumeCount = " + CurVolumeCount  + "\n" +
					'}';
		}
    }
}
