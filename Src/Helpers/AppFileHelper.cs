using Tsundoku.ViewModels;
using Tsundoku.Models;
using System.Text.Json.Nodes; // Assuming Tsundoku.Models contains User, Format, etc.

namespace Tsundoku.Helpers
{
    /// <summary>
    /// Provides static helper methods for resolving application-specific file paths,
    /// and for common file operations especially for MSIX-packaged applications.
    /// </summary>
    public static class AppFileHelper
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public static readonly HashSet<string> ValidImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg"
        };

        /// <summary>
        /// Gets the full, writable path for application-specific files,
        /// ensuring the directory exists and is compliant with MSIX sandbox security.
        /// </summary>
        /// <param name="fileName">The name of the file (e.g., "UserData.json").</param>
        /// <param name="appSpecificSubfolder">A subfolder name *within* Tsundoku (e.g., "Logs", "Covers").
        /// Pass an empty string for files directly under the "Tsundoku" folder (like UserData.json).</param>
        /// <returns>The full path to the specified file in a writable location.</returns>
        public static string GetFilePath(string fileName, string appSpecificSubfolder = "")
        {
            // This now calls GetFolderPath directly to ensure "Tsundoku" is always in the path,
            // and passes the specific subfolder name (which can be empty).
            string baseWritablePath = GetFolderPath(appSpecificSubfolder);

            return Path.Combine(baseWritablePath, fileName);
        }

        /// <summary>
        /// Gets the full path for UserData.json, now located directly within the "Tsundoku\" folder.
        /// </summary>
        /// <returns>The full path to UserData.json.</returns>
        public static string GetUserDataJsonPath()
        {
            // Assuming ViewModelBase.USER_DATA_FILEPATH holds "UserData.json"
            // Pass an EMPTY string for the subfolder to place it directly under Tsundoku\
            return GetFilePath(ViewModelBase.USER_DATA_FILEPATH);
        }

        /// <summary>
        /// Creates a unique file path for a cover image within the "Covers" subfolder,
        /// handling invalid characters, duplicate filenames, and validating the file extension.
        /// </summary>
        /// <param name="coverLink">The URL or link from which the cover image is sourced, used to derive the file extension.</param>
        /// <param name="title">The title of the book/item to base the cover name on.</param>
        /// <param name="bookType">The format/type of the book (e.g., "Manga", "Novel").</param>
        /// <param name="dupeIndex">A reference parameter that will hold the duplicate index if a duplicate was found (e.g., 2 for _2).</param>
        /// <returns>A unique, full file path for the cover image.</returns>
        public static (string, bool) CreateUniqueCoverFileName(string coverLink, string title, Format bookType, bool allowDuplicate, ref uint dupeIndex)
        {
            string safeTitle = ExtensionMethods.RemoveInPlaceCharArray(string.Concat(title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))).Replace(",", string.Empty);

            string extension = Path.GetExtension(coverLink).ToLowerInvariant();

            if (!ValidImageExtensions.Contains(extension))
            {
                LOGGER.Warn("Derived extension '{DerivedExtension}' from cover link '{CoverLink}' is not a recognized image format. Defaulting to '.png'.", extension, coverLink);
            }
            extension = ".png";

            string baseFileName = $"{safeTitle}_{bookType.ToString().ToUpper()}";
            bool isDuplicate = false;
            if (CheckCoverFileExists(baseFileName + extension) && allowDuplicate)
            {
                isDuplicate = true;
                uint count = (dupeIndex == 0) ? 2 : dupeIndex;
                while (true)
                {
                    string newFileName = $"{baseFileName}_{count}{extension}";
                    if (!CheckCoverFileExists(newFileName))
                    {
                        baseFileName = newFileName;
                        dupeIndex = count;
                        break;
                    }
                    count++;
                }
                LOGGER.Debug("{name} DupeIndex is Now {count}", title, dupeIndex);
            }
            else
            {
                baseFileName += extension;
            }
            return (baseFileName, isDuplicate);
        }

        /// <summary>
        /// Gets the full path for a specific subfolder within the app's local data.
        /// It ensures the directory exists and is compliant with MSIX sandbox security.
        /// All folders created by this method will be under 'LocalApplicationData\Tsundoku\[subfolderName]\'.
        /// If subfolderName is empty, it returns 'LocalApplicationData\Tsundoku\'.
        /// </summary>
        /// <param name="subfolderName">The name of the subfolder (e.g., "Logs", "Covers"). Can be empty.</param>
        /// <returns>The full path to the specified subfolder or the Tsundoku root.</returns>
        public static string GetFolderPath(string subfolderName)
        {
            string baseWritablePath;
            string fullFolderPath;
#if DEBUG
            // In Debug configuration, use the current application directory
            baseWritablePath = AppContext.BaseDirectory;
            fullFolderPath = Path.Combine(baseWritablePath, subfolderName);
#else
            // In Release (or any other non-Debug) configuration, use Windows.Storage.ApplicationData.Current.LocalCacheFolder
            baseWritablePath = ApplicationData.Current.LocalCacheFolder.Path;
            fullFolderPath = Path.Combine(baseWritablePath, "Local", "Tsundoku", subfolderName);
#endif

            try
            {
                Directory.CreateDirectory(fullFolderPath);
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, $"Failed to create folder: {fullFolderPath}");
                throw;
            }
            return fullFolderPath;
        }

        /// <summary>
        /// Gets the full path for the "Covers\" subfolder.
        /// </summary>
        /// <returns>The full path to the Covers folder.</returns>
        public static string GetCoversFolderPath()
        {
            return GetFolderPath("Covers");
        }

        /// <summary>
        /// Gets the full path for log files, specifically in the "Logs\" subfolder.
        /// </summary>
        /// <param name="logFileName">The name of the log file (e.g., "AppLog.log").</param>
        /// <returns>The full path to the log file in the Logs subfolder.</returns>
        public static string GetLogFolderPath(string logFileName)
        {
            return GetFolderPath("Logs");
        }

        /// <summary>
        /// Gets the full path for the "Themes\" subfolder.
        /// </summary>
        /// <returns>The full path to the Themes folder.</returns>
        public static string GetThemesFolderPath()
        {
            return GetFolderPath("Themes");
        }

        /// <summary>
        /// Gets the full path for the "Screenshots\" subfolder.
        /// </summary>
        /// <returns>The full path to the Screenshots folder.</returns>
        public static string GetScreenshotsFolderPath()
        {
            return GetFolderPath("Screenshots");
        }

        /// <summary>
        /// Generates a unique file path for a screenshot within the "Screenshots" subfolder.
        /// Handles invalid filename characters and appends a number if a file with the same base name already exists.
        /// </summary>
        /// <param name="baseFileNameWithoutExtension">The desired base name for the screenshot file (e.g., "User-Collection-Screenshot-Theme-Language").</param>
        /// <param name="extension">The desired file extension for the screenshot (e.g., ".jpg", ".png").</param>
        /// <returns>A unique, full file path for the screenshot.</returns>
        public static string CreateUniqueScreenshotPath(string baseFileNameWithoutExtension, string extension)
        {
            string screenshotsFolderPath = GetScreenshotsFolderPath();

            string safeBaseFileName = ExtensionMethods.RemoveInPlaceCharArray(
                string.Concat(baseFileNameWithoutExtension.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)))
                .Replace(",", string.Empty);

            if (!extension.StartsWith('.'))
            {
                extension = "." + extension;
            }

            string newPath = Path.Combine(screenshotsFolderPath, $"{safeBaseFileName}{extension}");

            uint count = 1;
            while (File.Exists(newPath))
            {
                count++;
                newPath = Path.Combine(screenshotsFolderPath, $"{safeBaseFileName}_{count}{extension}");
                LOGGER.Info("Screenshot path {PotentialPath} already exists. Trying next index.", newPath);
            }

            return newPath;
        }

        /// <summary>
        /// Serializes the provided User object to the UserData.json file using source-generated JSON.
        /// </summary>
        /// <param name="userData">The User object to serialize.</param>
        public static void WriteUserDataToFile(User userData)
        {
            string filePath = GetUserDataJsonPath();
            try
            {
                // Use the Default instance of your UserModelContext.
                string jsonString = userData.Serialize();
                File.WriteAllText(filePath, jsonString);
                LOGGER.Info("User data successfully written to {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Failed to write user data to {FilePath}\n{ErrorMsg}", filePath, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Serializes the provided JsonNode to the UserData.json file.
        /// This method uses the default JsonSerializerOptions from UserModelContext.
        /// </summary>
        /// <param name="jsonDataNode">The JsonNode object to serialize.</param>
        public static void WriteUserDataToFile(JsonNode jsonDataNode)
        {
            string filePath = GetUserDataJsonPath();
            try
            {
                string jsonString = jsonDataNode.ToJsonString(User.JSON_SERIALIZATION_OPTIONS);
                File.WriteAllText(filePath, jsonString);
                LOGGER.Debug("JsonNode data successfully written to {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Failed to write json user data to {FilePath}\n{ErrorMsg}", filePath, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Deserializes User data from the UserData.json file using source-generated JSON.
        /// </summary>
        /// <returns>The deserialized User object, or null if the file does not exist or deserialization fails.</returns>
        public static User? ReadUserDataFromFile()
        {
            string filePath = GetUserDataJsonPath();
            if (!File.Exists(filePath))
            {
                LOGGER.Error("User data file not found at {FilePath}.", filePath);
                return null;
            }

            try
            {
                string jsonString = File.ReadAllText(filePath);
                User? user = JsonSerializer.Deserialize(jsonString, UserModelContext.Default.User);

                if (user != null)
                {
                    LOGGER.Info("User data successfully deserialized from {FilePath} for user: {Username}", filePath, user.UserName);
                }
                else
                {
                    // This scenario might occur if the JSON content is literally "null" or an empty object that maps to null User.
                    LOGGER.Warn("Deserialization from {FilePath} completed, but resulted in a null User object. JSON content: {JsonContent}", filePath, jsonString);
                }

                return user;
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Failed to read or deserialize user data from {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// Checks if a file with the given name exists in the "Covers" folder.
        /// </summary>
        /// <param name="fileName">The specific file name to check (e.g., "MySeries_MANGA.png").</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        public static bool CheckCoverFileExists(string fileName)
        {
            string coversFolderPath = GetCoversFolderPath();
            string fullPath = Path.Combine(coversFolderPath, fileName);
            bool exists = File.Exists(fullPath);
            if (exists)
            {
                LOGGER.Debug("Cover file {FileName} found at {FullPath}", fileName, fullPath);
            }
            else
            {
                LOGGER.Debug("Cover file {FileName} not found at {FullPath}", fileName, fullPath);
            }
            return exists;
        }

        /// <summary>
        /// Gets the full path to an existing cover file if it has a valid image extension
        /// and exists in the "Covers" folder.
        /// </summary>am>
        /// <returns>The full path to the existing cover file if found, otherwise null.</returns>
        public static string GetFullCoverPath(string coverFileName)
        {
            string fullPath = Path.Combine(GetCoversFolderPath(), coverFileName);
            string extension = Path.GetExtension(coverFileName).ToLowerInvariant();

            if (ValidImageExtensions.Contains(extension))
            {
                LOGGER.Trace("Cover file {FileName} has valid extension {extension}: {FullPath}", coverFileName, extension, fullPath);
                return fullPath;
            }

            if (!ValidImageExtensions.Contains(extension))
            {
                LOGGER.Warn("Provided file name {FileName} has an invalid or unrecognized extension '{Extension}'", coverFileName, extension);
            }
            else
            {
                LOGGER.Warn("Cover file {FileName} with valid extension '{Extension}' not found at {FullPath}", coverFileName, extension, fullPath);
            }
            return fullPath;
        }

        /// <summary>
        /// Removes the extension from a given file name.
        /// </summary>
        /// <param name="fileNameWithExtension">The file name including its extension (e.g., "MyImage.png").</param>
        /// <returns>The file name without its extension (e.g., "MyImage"). Returns an empty string if the input is null or empty.</returns>
        public static string GetFileNameWithoutExtension(string fileNameWithExtension)
        {
            if (string.IsNullOrEmpty(fileNameWithExtension))
            {
                return string.Empty; // Return an empty string for null or empty input
            }
            return Path.GetFileNameWithoutExtension(fileNameWithExtension);
        }

        /// <summary>
        /// Finds the full path to an existing cover file in the "Covers" folder
        /// by iterating through known valid image extensions, given only the base filename.
        /// </summary>
        /// <param name="baseFileNameWithoutExtension">The base name of the cover file without any extension (e.g., "MySeries_MANGA").</param>
        /// <returns>The full path to the first found existing cover file with a valid extension, or null if no such file is found.</returns>
        public static string? FindExistingCoverByBaseName(string baseFileNameWithExtension)
        {
            string coversFolderPath = GetCoversFolderPath();
            string baseFileNameWithoutExtension = GetFileNameWithoutExtension(baseFileNameWithExtension);

            foreach (string ext in ValidImageExtensions)
            {
                string potentialPath = Path.Combine(coversFolderPath, baseFileNameWithoutExtension + ext);
                if (File.Exists(potentialPath))
                {
                    LOGGER.Debug("Found existing cover file by base name: {FullPath}", potentialPath);
                    return potentialPath;
                }
            }

            LOGGER.Warn("No existing cover file found for base name {BaseName} with any valid extensions.", baseFileNameWithoutExtension);
            return null; // No valid cover found with any known extension
        }

        public static void DeleteCoverFile(string coverName)
        {
            string filePath = GetFullCoverPath(coverName);
            if (File.Exists(filePath))
            {
                try
                {
                    FileAttributes attributes = File.GetAttributes(filePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        // Remove ONLY the ReadOnly attribute, leave others (Hidden, Archive, etc.) intact
                        File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                        LOGGER.Trace("Removed ReadOnly attribute from: {filePath}", filePath);
                    }

                    File.Delete(filePath);
                    LOGGER.Info("Successfully deleted: {filePath}", filePath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    LOGGER.Error("Error deleting {filePath}: Access denied. (Details: {ex.Message})", filePath, ex.Message);
                    // This could still happen if permissions are truly insufficient beyond ReadOnly
                    // or if the file is in use by another process despite the error message.
                }
                catch (IOException ex)
                {
                    LOGGER.Info("Error deleting {filePath}: I/O error (e.g., file in use). (Details: {ex.Message})", filePath, ex.Message);
                }
                catch (Exception ex)
                {
                    LOGGER.Info("An unexpected error occurred while deleting {filePath}: {ex.Message}", filePath, ex.Message);
                }
            }
            else
            {
                LOGGER.Warn("File not found, skipping deletion: {filePath}", filePath);
            }
        }
    }
}