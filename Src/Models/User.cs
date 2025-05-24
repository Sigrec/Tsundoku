using Avalonia.Media.Imaging;
using MangaAndLightNovelWebScrape.Websites;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Tsundoku.Helpers;
using Tsundoku.ViewModels;

namespace Tsundoku.Models
{
    public class User
    {
        public string UserName { get; set; }
        public uint NumVolumesCollected { get; set; }
        public uint NumVolumesToBeCollected { get; set; }
        public string CurLanguage { get; set; }
        public string Display { get; set; }
        public string MainTheme { get; set; }
        public double CurDataVersion { get; set; }
        public string Currency { get; set; }
        public decimal MeanRating { get; set; }
        public uint VolumesRead { get; set; }
        public string CollectionPrice { get; set; }
        public Region Region { get; set; }
        public Dictionary<string, bool> Memberships { get; set; }
        public string Notes { get; set; }
        public ObservableCollection<TsundokuTheme> SavedThemes { get; set; }
        public List<Series> UserCollection { get; set; }
        public byte[] UserIcon { get; set; }
        [JsonIgnore] internal static UserModelContext UserJsonModel = new UserModelContext(new JsonSerializerOptions()
        { 
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });

        public User(string UserName, string CurLanguage, string MainTheme, string Display, double CurDataVersion, string Currency, string CollectionPrice, Region Region, Dictionary<string, bool> Memberships, ObservableCollection<TsundokuTheme> SavedThemes, List<Series> UserCollection)
        {
            this.UserName = UserName;
            this.CurLanguage = CurLanguage;
            this.Display = Display;
            this.CurDataVersion = CurDataVersion;
            this.Currency = Currency;
            this.MainTheme = MainTheme;
            this.CollectionPrice = CollectionPrice;
            this.Region = Region;
            this.SavedThemes = SavedThemes;
            this.UserCollection = UserCollection;
            this.Memberships = Memberships;
            NumVolumesCollected = 0;
            NumVolumesToBeCollected = 0;
            Notes = string.Empty;
        }

        public static byte[] ImageToByteArray(Avalonia.Media.Imaging.Bitmap image)
        {
            using MemoryStream stream = new();
            image.Save(stream, 100);
            return stream.ToArray();
        }

        /// <summary>
        /// Saves the current state of this User object to UserData.json,
        /// located directly in the application's LocalState folder.
        /// </summary>
        public void SaveUserData()
        {
            string userDataFullPath = AppFileHelper.GetUserDataJsonPath();
            LOGGER.Info($"Saving \"{UserName}'s\" Collection Data to {userDataFullPath}");

            try
            {
                string jsonContent = JsonSerializer.Serialize(this, typeof(User), UserJsonModel);
                File.WriteAllText(userDataFullPath, jsonContent, Encoding.UTF8); // Write to the correct path
                LOGGER.Debug($"Successfully saved user data to: {userDataFullPath}");
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, $"Failed to save user data to {userDataFullPath}.");
            }
        }

        /// <summary>
        /// Updates the users data version
        /// </summary>
        /// <param name="userData">Users parsed Json data</param>
        /// <param name="isImport">Whether the data that needs to check for an update is an import</param>
        /// <param name="updatedCovers">Whether the covers </param>
        /// <returns></returns>
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
        public static bool UpdateSchemaVersion(JsonNode userData, bool isImport=false, bool updatedCovers=false)
        {
            bool updatedVersion = false;
            JsonObject series;
            JsonObject theme;
            JsonArray collectionJsonArray = userData[nameof(UserCollection)].AsArray();
            JsonArray themeJsonArray = userData["SavedThemes"].AsArray();
            
            // For users who did not get the older update
            if (!userData.AsObject().ContainsKey("CurDataVersion"))
            {
                userData.AsObject().Add("CurDataVersion", "1.0");
            }
            double curVersion = double.Parse(userData["CurDataVersion"].ToString());

            if (curVersion < 1.5) // 1.5 Version Upgrade changes Series Title, Staff. & Status to accomadte new changes & adds Stats related variables to Series and also changes Native to Japanese
            {
                userData.AsObject().Add("Currency", "$");
                userData.AsObject().Add("MeanRating", 0);
                userData.AsObject().Add("VolumesRead", 0);
                userData.AsObject().Add("CollectionPrice", "");

                if (userData[nameof(CurLanguage)].ToString().Equals("Native"))
                {
                    userData[nameof(CurLanguage)] = "Japanese";
                }

                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    series["Titles"] = new JsonObject
					{
						["Romaji"] = series["Titles"][0].ToString(),
                        ["English"] = series["Titles"][1].ToString(),
                        [series["Format"].ToString() switch
                        {
                            "Manhwa" => "Korean",
                            "Manhua" => "Chinese",
                            _ => "Japanese"            
                        }] = series["Titles"][2].ToString()
                    };

                    series["Staff"] = new JsonObject
					{
						["Romaji"] = series["Staff"][0].ToString(),
                        [series["Format"].ToString() switch
                        {
                            "Manhwa" => "Korean",
                            "Manhua" => "Chinese",
                            _ => "Japanese"            
                        }] = series["Staff"][1].ToString()
                    };
                    series.AsObject().Add("Score", 0);
                    
                    if (series["Titles"]["Romaji"].ToString().Equals(series["Titles"]["English"].ToString()))
                    {
                        series["Titles"].AsObject().Remove("English");
                    }

                    if (curVersion < 1.4 && series["Status"].ToString().Equals("Complete"))
                    {
                        series["Status"] = "Finished";
                    }
                }
                userData["CurDataVersion"] = 1.5;
                LOGGER.Info("Updated Users Data to v1.5", !isImport);
                updatedVersion = true;        
            }

            if (curVersion < 1.6) // 1.6 Version Upgrade fixes scores defaulting to 0 to -1 instead so it doesn't show up i stats
            {
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    if (double.Parse(series["Score"].ToString()) == 0)
                    {
                        series["Score"] = -1;
                    }
                }

                userData["CurDataVersion"] = 1.6;
                LOGGER.Info("Updated Users Data to v1.6", !isImport);
                updatedVersion = true;  
            }

            if (curVersion < 1.7) // 1.7 Version Upgrade To accomdate adding user icon
            {
                string defaultFileByte = "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";

                userData.AsObject().Add(nameof(UserIcon), defaultFileByte);
                userData["CurDataVersion"] = 1.7;
                LOGGER.Info("Updated Users Data to v1.7", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 1.8) // 1.8 Version Upgrade to rename button color identifiers for TsundokuTheme change
            {   
                for (int x = 0; x < themeJsonArray.Count; x++)
                {
                    theme = themeJsonArray.ElementAt(x).AsObject();
                    theme["SeriesButtonBGColor"] = (uint)theme["SeriesSwitchPaneButtonBGColor"];
                    theme["SeriesButtonBGHoverColor"] = (uint)theme["SeriesSwitchPaneButtonBGHoverColor"];
                    theme["SeriesButtonIconColor"] = (uint)theme["SeriesSwitchPaneButtonIconColor"];
                    theme["SeriesButtonIconHoverColor"] = (uint)theme["SeriesSwitchPaneButtonIconHoverColor"];
                }

                userData["CurDataVersion"] = 1.8;
                LOGGER.Info("Updated Users Data to v1.8", !isImport);
                updatedVersion = true;  
            }

            if (curVersion < 1.9) // 1.9 Version Upgrade resizes all bitmaps in the users cover folder
            {
                string coverPath;
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    coverPath = collectionJsonArray.ElementAt(x)["Cover"].ToString();
                    if (File.Exists(coverPath))
                    {
                        Bitmap resizedBitMap = new Bitmap(coverPath).CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                        resizedBitMap.Save(coverPath, 100);
                    }
                }
                userData["CurDataVersion"] = 1.9;
                LOGGER.Info("Updated Users Data to v1.9", !isImport);
                updatedCovers = true;
                updatedVersion = true;
            }

            if (curVersion < 2.0) // 2.0 Verion Upgrade adds Memberships for Price Analysis and changes "Score" to "Rating"
            {
                userData["Memberships"] = new JsonObject
                {
                    [BooksAMillion.WEBSITE_TITLE] = false,
                    [Indigo.WEBSITE_TITLE] = false,
                    [KinokuniyaUSA.WEBSITE_TITLE] = false
                };

                userData.AsObject()["MeanRating"] = (decimal)userData["MeanScore"];
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    series["Rating"] = decimal.Parse(series["Score"].ToString());
                }
                userData.AsObject().Remove("MeanScore");

                userData["CurDataVersion"] = 2.0;
                LOGGER.Info("Updated Users Data to v2.0", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 2.1) // 2.1 Version updates empty demographics from null or empty string to "Unknown" & removes Default thme from file
            {
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    if (string.IsNullOrWhiteSpace(series["Demographic"]?.ToString()))
                    {
                        series["Demographic"] = Demographic.Unknown.ToString();
                    }
                }
                JsonArray themes = userData["SavedThemes"].AsArray();
                for (int x = 0; x < themes.Count; x++)
                {
                    if (themes.ElementAt(x)["ThemeName"].ToString().Equals("Default"))
                    {   
                        themes.RemoveAt(x);
                    }
                }
                userData["CurDataVersion"] = 2.1;
                LOGGER.Info("Updated Users Data to v2.1", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 2.2)
            {
                userData["Memberships"].AsObject().Remove("RightStufAnime");
                userData["Memberships"].AsObject().Add(new KeyValuePair<string, JsonNode?>(Indigo.WEBSITE_TITLE, false));

                userData["CurDataVersion"] = 2.2;
                LOGGER.Info("Updated Users Data to v2.2", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 2.4) // Add Region property to User
            {
                userData.AsObject().Add(new KeyValuePair<string, JsonNode?>("Region", Region.America.ToString()));
                userData["CurDataVersion"] = 2.4;
                LOGGER.Info("Updated Users Data to v2.4", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 2.5)
            {
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    string[] coverFileName = series["Cover"].ToString().Split('_');
                    string format = series["Format"].ToString().ToUpper();
                    string oldCoverFileName = series["Cover"].ToString();
                    if (!coverFileName[1].StartsWith(format) && File.Exists(oldCoverFileName))
                    {
                        series["Cover"] = $@"{coverFileName[0]}_{format}.{oldCoverFileName.Substring(oldCoverFileName.Length - 3)}";
                        File.Move(oldCoverFileName, series["Cover"].ToString());
                        LOGGER.Debug("Updated {} Cover File Name", series["Titles"]["Romaji"].ToString());
                    }
                }
                userData["CurDataVersion"] = 2.5;
                LOGGER.Info("Updated Users Data to v2.5", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 3.0) // Update the colors in themes to hex string
            {
                for (int x = 0; x < themeJsonArray.Count; x++)
                {
                    theme = themeJsonArray.ElementAt(x).AsObject();
                    foreach (System.Reflection.PropertyInfo property in typeof(TsundokuTheme).GetProperties().Skip(1))
                    {
                        theme[property.Name] = Avalonia.Media.Color.FromUInt32(uint.Parse(theme[property.Name].ToString())).ToString();
                    }
                }
                userData["CurDataVersion"] = 3.0;
                LOGGER.Info("Updated Users Data to v3.0", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 4.0) // Remove "SeriesButtonBGColor" & SeriesButtonBGHoverColor" Colors from Themes
            {
                for (int x = 0; x < themeJsonArray.Count; x++)
                {
                    themeJsonArray.ElementAt(x).AsObject().Remove("SeriesButtonBGColor");
                    themeJsonArray.ElementAt(x).AsObject().Remove("SeriesButtonBGHoverColor");
                }
                userData["CurDataVersion"] = 4.0;
                LOGGER.Info("Updated Users Data to v4.0", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 5.0) // Update series "Value" to "Cost"
            {
                for (int x = 0; x < collectionJsonArray.Count; x++)
                {
                    series = collectionJsonArray.ElementAt(x).AsObject();
                    series.Add("Value", decimal.Parse(series["Cost"].ToString()));
                    series.Remove("Cost");
                }
                userData["CurDataVersion"] = 5.0;
                LOGGER.Info("Updated Users Data to v5.0", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 5.1) // Add new staff theme color, and remove B&N membership
            {
                for (int x = 0; x < themeJsonArray.Count; x++)
                {
                    theme = themeJsonArray.ElementAt(x).AsObject();
                    if (!theme.ContainsKey("SeriesCardPublisherColor")) theme.Add("SeriesCardPublisherColor", theme["SeriesCardStaffColor"].ToString());
                }
                userData["Memberships"].AsObject().Remove("Barnes & Noble");
                userData["CurDataVersion"] = 5.1;
                LOGGER.Info("Updated Users Data to v5.1", !isImport);
                updatedVersion = true;
            }

            if (curVersion < 5.2) // Add new staff theme color, and remove B&N membership
            {
                for (int x = 0; x < themeJsonArray.Count; x++)
                {
                    theme = themeJsonArray.ElementAt(x).AsObject();
                    if (!theme.ContainsKey("UserIconBorderColor"))
                    {
                        theme.Add("UserIconBorderColor", theme["DividerColor"].ToString());
                    }
                    else if(theme["UserIconBorderColor"] == null)
                    {
                        theme["UserIconBorderColor"] = theme["DividerColor"].ToString();
                    }
                }
                userData["CurDataVersion"] = 5.2;
                LOGGER.Info("Updated Users Data to v5.2", !isImport);
                updatedVersion = true;
            }

            if (!isImport)
            {
                File.WriteAllText(ViewModelBase.USER_DATA_FILEPATH, JsonSerializer.Serialize(userData, new JsonSerializerOptions()
                { 
                    WriteIndented = true,
                    ReadCommentHandling = JsonCommentHandling.Disallow,
                    AllowTrailingCommas = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }));
            }
            return updatedVersion;
        }
    }

    [JsonSerializable(typeof(User))]
    [JsonSourceGenerationOptions(UseStringEnumConverter = true)]
    internal partial class UserModelContext : JsonSerializerContext { }
}