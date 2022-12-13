using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using Tsundoku.Source;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Tsundoku.Models
{
	public class Series : IDisposable, IEquatable<Series?>
	{
		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
		[JsonIgnore]
		private bool disposedValue;
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
			string seriesDataQuery = new AniListQuery().GetSeries(title, bookType);

			if (!string.IsNullOrWhiteSpace(seriesDataQuery))
            {
				JsonElement seriesData = JsonDocument.Parse(seriesDataQuery).RootElement.GetProperty("Media"); // "Media"
				string romajiTitle = seriesData.GetProperty("title").GetProperty("romaji").ToString();
				string filteredBookType = bookType.Equals("MANGA") ? GetCorrectComicName(seriesData.GetProperty("countryOfOrigin").ToString()) : "Novel";
				string nativeStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "native", filteredBookType, romajiTitle);
				string fullStaff = GetSeriesStaff(seriesData.GetProperty("staff").GetProperty("edges"), "full", filteredBookType, romajiTitle);
				string coverPath = SaveNewCoverImage(seriesData.GetProperty("coverImage").GetProperty("extraLarge").ToString(), romajiTitle, filteredBookType.ToUpper());
				Series _newSeries = new Series(
					new List<string>()
					{
						romajiTitle,
						string.IsNullOrWhiteSpace(seriesData.GetProperty("title").GetProperty("english").ToString().ToString()) ? romajiTitle : seriesData.GetProperty("title").GetProperty("english").ToString(),
						seriesData.GetProperty("title").GetProperty("native").ToString()
					},
					new List<string>()
					{
						fullStaff,
						nativeStaff.Equals(" | ") ? fullStaff : nativeStaff,
					},
					seriesData.GetProperty("description").ValueKind == JsonValueKind.Null ? "" : ConvertUnicodeInDesc(Regex.Replace(seriesData.GetProperty("description").ToString(), @"\(Source: [\S\s]+|\<.*?\>", "").Trim()),
					filteredBookType,
					GetSeriesStatus(seriesData.GetProperty("status").ToString()),
					coverPath,
					seriesData.GetProperty("siteUrl").ToString(),
					maxVolCount,
					minVolCount,
					new Bitmap(coverPath).CreateScaledBitmap(new Avalonia.PixelSize(Constants.LEFT_SIDE_CARD_WIDTH, Constants.IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality));
				return _newSeries;
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
			
			switch(jsonStatus)
			{
				case "RELEASING":
				case "NOT_YET_RELEASED" :
					return "Ongoing";
				case "FINISHED":
					return "Complete";
				case "CANCELLED":
					return "Cancelled";
				case "HIATUS":
					return "Hiatus";
				default:
					return "Error";
			}
		}

        public static string SaveNewCoverImage(String coverLink, String title, String bookType)
        {
            string newPath = @$"Covers\{Regex.Replace(title, @"[^A-Za-z\d]", "")}_{bookType}.{coverLink.Substring(coverLink.Length - 3)}";
            Directory.CreateDirectory(@$"Covers");

            if (!File.Exists(newPath))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    Task<HttpResponseMessage> response = Task.Run(async () => await client.GetAsync(new Uri(coverLink)));
                    response.Wait();
                    HttpResponseMessage clientResponse = response.Result;
                    using (FileStream fs = new FileStream(newPath, FileMode.OpenOrCreate, FileAccess.Write))
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

        public static string GetSeriesStaff(JsonElement staffArray, string nameType, string bookType, string title) {
			StringBuilder staffList = new StringBuilder();
			string[] validRoles = { "Story & Art", "Story", "Art", "Original Creator", "Character Design", "Illustration", "Mechanical Design", "Original Story"};
			foreach(JsonElement name in staffArray.EnumerateArray())
            {
				string staffRole = Regex.Replace(name.GetProperty("role").ToString(), @" \(.*\)", "");

				// Don't include staff for manga that are illustrators
				if (bookType.Equals("Manga") && staffRole.Equals("Illustration") && !title.Contains("Anthology"))
				{
					break;
				}
				else if (validRoles.Contains(staffRole))
                {
					String newStaff = name.GetProperty("node").GetProperty("name").GetProperty(nameType).ToString().Trim();
					if (!staffList.ToString().Contains(newStaff) || string.IsNullOrWhiteSpace(newStaff)) // Check to see if this staff member has multiple roles to only add them once
					{
						if (!string.IsNullOrWhiteSpace(newStaff))
						{
							staffList.Append(newStaff + " | ");
						}
						else if (nameType.Equals("native"))
						{
							staffList.Append(name.GetProperty("node").GetProperty("name").GetProperty("full").ToString().Trim() + " | ");
						}
						else // If the staff member does not have a full name entry
						{
							staffList.Append(name.GetProperty("node").GetProperty("name").GetProperty("native").ToString().Trim() + " | ");
						}
					}
					else
					{
						Logger.Info($"Duplicate Staff Entry For {newStaff}");
					}
                }
            }
			return staffList.ToString(0, staffList.Length - 3);
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

		public override bool Equals(object? obj)
		{
			return Equals(obj as Series);
		}

		public bool Equals(Series? other)
		{
			return other is not null &&
				   disposedValue == other.disposedValue &&
				   EqualityComparer<List<string>>.Default.Equals(Titles, other.Titles) &&
				   EqualityComparer<List<string>>.Default.Equals(Staff, other.Staff) &&
				   Description == other.Description &&
				   Format == other.Format &&
				   Status == other.Status &&
				   Cover == other.Cover &&
				   Link == other.Link &&
				   SeriesNotes == other.SeriesNotes &&
				   MaxVolumeCount == other.MaxVolumeCount &&
				   CurVolumeCount == other.CurVolumeCount;
		}

		public override int GetHashCode()
		{
			HashCode hash = new HashCode();
			hash.Add(disposedValue);
			hash.Add(Titles);
			hash.Add(Staff);
			hash.Add(Description);
			hash.Add(Format);
			hash.Add(Status);
			hash.Add(Cover);
			hash.Add(Link);
			hash.Add(SeriesNotes);
			hash.Add(MaxVolumeCount);
			hash.Add(CurVolumeCount);
			return hash.ToHashCode();
		}

		public static bool operator ==(Series? left, Series? right)
		{
			return EqualityComparer<Series>.Default.Equals(left, right);
		}

		public static bool operator !=(Series? left, Series? right)
		{
			return !(left == right);
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
