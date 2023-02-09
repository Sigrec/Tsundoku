using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using Tsundoku.Helpers;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Tsundoku.Models
{
	public class Series : IDisposable
	{
		[JsonIgnore]
		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
		[JsonIgnore]
		private bool disposedValue;
		public List<string> Titles { get; } //[Romaji, English, Native]
        public List<string> Staff { get; } //[Romaji, Native]
		public string Description { get; }
		public string Format { get; }
		public string Status { get; set; }
		public string Cover { get; set; }
		public string Link { get; }
		public string SeriesNotes { get; set; }
		public ushort MaxVolumeCount { get; set; }
		public ushort CurVolumeCount { get; set; }

		[JsonIgnore]
		public string Synonyms { get; }

		[JsonIgnore]
		public Bitmap CoverBitMap { get; set; }

        public Series(List<string> titles, List<string> staff, string description, string format, string status, string cover, string link, ushort maxVolumeCount, ushort curVolumeCount, Bitmap coverBitMap)
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
			CoverBitMap = coverBitMap;
        }

		public static Series? CreateNewSeriesCard(string title, string bookType, ushort maxVolCount, ushort minVolCount)
        {
			int seriesId;
			string seriesDataQuery;
			if (int.TryParse(title, out seriesId))
			{
				seriesDataQuery = new AniListQuery().GetSeriesID(seriesId, bookType);
			}
			else
			{
				seriesDataQuery = new AniListQuery().GetSeriesTitle(title, bookType);
			}


			if (!string.IsNullOrWhiteSpace(seriesDataQuery))
            {
				bool extraSeriesCheck = false;
				JsonElement seriesData = JsonDocument.Parse(seriesDataQuery).RootElement.GetProperty("Media");
				string romajiTitle = seriesData.GetProperty("title").GetProperty("romaji").ToString();
				string nativeTitle = seriesData.GetProperty("title").GetProperty("native").ToString();
				JsonElement englishTitle = seriesData.GetProperty("title").GetProperty("english");
				JsonElement synonyms = seriesData.GetProperty("synonyms");

				// Checks to see if the series is available on AniList if not check ExtraSeries json
				string[] ExtraSeriesList = { "RADIANT" };
				if (ExtraSeriesList.Contains(Helpers.ExtensionMethods.RemoveInPlaceCharArray(title.ToUpper())) && File.Exists(@"ExtraSeries.json"))
				{
					Logger.Info($"AniList Does Not Have {title}");
					JsonElement.ArrayEnumerator extraSeriesList = JsonDocument.Parse(File.ReadAllText(@"ExtraSeries.json")).RootElement.GetProperty("ExtraSeries").EnumerateArray();

					// Traverse ExtraSeries
					foreach (JsonElement series in extraSeriesList)
					{
						seriesData = series.GetProperty("Media");
						romajiTitle = seriesData.GetProperty("title").GetProperty("romaji").ToString();
						nativeTitle = seriesData.GetProperty("title").GetProperty("native").ToString();
						englishTitle = seriesData.GetProperty("title").GetProperty("english");

						// Check to see if the title is a valid entry in the ExtraSeries.json
						if (romajiTitle.Equals(title, StringComparison.OrdinalIgnoreCase) || nativeTitle.Equals(title, StringComparison.OrdinalIgnoreCase) || englishTitle.ToString().Equals(title, StringComparison.OrdinalIgnoreCase))
						{
							extraSeriesCheck = true;
							break;
						}
					}

					if (!extraSeriesCheck)
					{
						Logger.Warn($"Need To Add Series {title} To ExtraSeries Json");
					}
				}

				string filteredBookType = bookType.Equals("MANGA") ? GetCorrectComicName(seriesData.GetProperty("countryOfOrigin").ToString()) : "Novel";
				string nativeStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "native", filteredBookType, romajiTitle);
				string fullStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "full", filteredBookType, romajiTitle);
				string coverPath = SaveNewCoverImage(CreateCoverFilePath(seriesData.GetProperty("coverImage").GetProperty("extraLarge").ToString(), romajiTitle, filteredBookType.ToUpper(), synonyms, extraSeriesCheck), seriesData.GetProperty("coverImage").GetProperty("extraLarge").ToString());

				//Logger.Debug(Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(seriesData.GetProperty("description").ToString(), false));

				return new Series(
						new List<string>()
						{
							romajiTitle,
							englishTitle.ValueKind == JsonValueKind.Null ? romajiTitle : englishTitle.ToString(),
							nativeTitle
						},
						new List<string>()
						{
							fullStaff,
							nativeStaff.Equals(" | ") ? fullStaff : nativeStaff,
						},
						ParseDescription(seriesData.GetProperty("description").ToString()),
						filteredBookType,
						GetSeriesStatus(seriesData.GetProperty("status").ToString()),
						coverPath,
						seriesData.GetProperty("siteUrl").ToString(),
						maxVolCount,
						minVolCount,
						new Bitmap(coverPath).CreateScaledBitmap(new Avalonia.PixelSize(Constants.LEFT_SIDE_CARD_WIDTH, Constants.IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality)
						);
			}
            else
            {
				Logger.Warn("User Input Invalid Series Type And/Or BookType");
            }
            return null;
        }

		public static string ParseDescription(string seriesDescription)
		{
			return string.IsNullOrWhiteSpace(seriesDescription) ? "" : System.Web.HttpUtility.HtmlDecode(Regex.Replace(new StringBuilder(seriesDescription).Replace("\n<br><br>\n", "\n\n").Replace("<br><br>\n\n", "\n\n").Replace("<br><br>", "\n").ToString(), @"\(Source: [\S\s]+|\<.*?\>", "").Trim().TrimEnd('\n'));
		}

		public static string GetCorrectComicName(string jsonCountryOfOrigin)
		{
			return jsonCountryOfOrigin switch
			{
				"JP" => "Manga",
				"KR" => "Manhwa",
				"CN" or "TW" => "Manhua",
				"FR" => "Manfra",
				_ => "Error"
			};
		}

		public static String GetSeriesStatus(String jsonStatus)
		{
			return jsonStatus switch
			{
				"RELEASING" or "NOT_YET_RELEASED" => "Ongoing",
				"FINISHED" => "Finished",
				"CANCELLED" => "Cancelled",
				"HIATUS" => "Hiatus",
				_  => "Error"
			};
		}

        public static string GetSeriesStaff(JsonElement staffArray, string nameType, string bookType, string title) {
			StringBuilder staffList = new StringBuilder();
			string[] validRoles = { "Story & Art", "Story", "Art", "Original Creator", "Character Design", "Illustration", "Mechanical Design", "Original Story", "Original Character Design", "Original Story"};
			foreach(JsonElement name in staffArray.EnumerateArray())
            {
				string staffRole = Regex.Replace(name.GetProperty("role").ToString(), @" \(.*\)", "").Trim();

				// Don't include "Illustration" staff for manga that are not anthologies
				if (validRoles.Contains(staffRole) && !(bookType.Equals("Manga") && staffRole.Equals("Illustration") && !title.Contains("Anthology")))
                {
					if (name.GetProperty("node").GetProperty("name").GetProperty("native").ValueKind == JsonValueKind.Null && name.GetProperty("node").GetProperty("name").GetProperty("full").ValueKind == JsonValueKind.Null)
					{
						staffList.Append("Error" + " | ");
					}
					else
					{
						string newStaff = name.GetProperty("node").GetProperty("name").GetProperty(nameType).ToString().Trim();
						string newStaffOther = name.GetProperty("node").GetProperty("name").GetProperty(nameType.Equals("native") ? "full" : "native").ToString().Trim();
						bool hasAlternatives = (name.GetProperty("node").GetProperty("name").GetProperty("alternative").GetArrayLength() > 0);
						if (!staffList.ToString().Contains(newStaff) || string.IsNullOrWhiteSpace(newStaff)) // Check to see if this staff member has multiple roles to only add them once
						{
							if (!string.IsNullOrWhiteSpace(newStaff))
							{
								staffList.Append(newStaff + " | ");
							}
							else if (!string.IsNullOrWhiteSpace(newStaffOther))
							{
								staffList.Append(newStaffOther + " | ");
							}
							else if (hasAlternatives) // If the staff member does not have a full name entry
							{
								staffList.Append(name.GetProperty("node").GetProperty("name").GetProperty("alternative")[0].ToString().Trim() + " | ");
							}
						}
						else
						{
							Logger.Info($"Duplicate Staff Entry For {newStaff}");
						}
					}
                }
            }
			return staffList.ToString(0, staffList.Length - 3);
		}

		public static string CreateCoverFilePath(string coverLink, string title, string bookType, JsonElement synonyms, bool extraSeriesCheck)
		{
			string newPath = @$"Covers\{Helpers.ExtensionMethods.RemoveInPlaceCharArray(string.Concat(title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)))}_{bookType}.{coverLink.Substring(coverLink.Length - 3)}";
            Directory.CreateDirectory(@"Covers");

			// Check and see if the series will generate a duplicate file name
            if (!extraSeriesCheck && File.Exists(newPath))
            {
                foreach (JsonElement newTitle in synonyms.EnumerateArray())
                {
                    if (Regex.IsMatch(newTitle.ToString(), @"[\w]"))
                    {
                        newPath = @$"Covers\{Helpers.ExtensionMethods.RemoveInPlaceCharArray(string.Concat(newTitle.ToString().Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)))}_{bookType}.{coverLink.Substring(coverLink.Length - 3)}";
                        break;
                    }
                }
            }
			return newPath;
		}
		public static string SaveNewCoverImage(string newPath, string coverLink)
        {
            try
            {
                HttpClient client = new HttpClient();
                Task<HttpResponseMessage> response = Task.Run(async () => await client.GetAsync(new Uri(coverLink)));
                response.Wait();
                HttpResponseMessage clientResponse = response.Result;
                using (FileStream fs = new FileStream(newPath, FileMode.Create, FileAccess.Write))
                {
                    Task fileResponse = Task.Run(async () => await clientResponse.Content.CopyToAsync(fs));
                    fileResponse.Wait();
                }
            }
            catch (Exception e)
            {
                Logger.Debug(e.ToString());
            }
			return newPath;
        }

		public string ToJsonString(JsonSerializerOptions options)
		{
			return "\n" + JsonSerializer.Serialize(this, options);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
					CoverBitMap.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~Series()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

    class RomajiComparer : IComparer<Series>
    {
        public int Compare(Series? x, Series? y)
        {
            if (x.Titles[0] == null && y.Titles[0] == null)
			{
				return 0;
			}
			else
			{
				return x.Titles[0].CompareTo(y.Titles[0]);
			}
        }
    }

	class EnglishComparer : IComparer<Series>
    {
        public int Compare(Series? x, Series? y)
        {
            if (x.Titles[1] == null && y.Titles[1] == null)
			{
				return 0;
			}
			else
			{
				return x.Titles[1].CompareTo(y.Titles[1]);
			}
        }
    }

	class NativeComparer : IComparer<Series>
    {
        public int Compare(Series? x, Series? y)
        {
            if (x.Titles[2] == null && y.Titles[2] == null)
			{
				return 0;
			}
			else
			{
				return x.Titles[2].CompareTo(y.Titles[2]);
			}
        }
    }
}
