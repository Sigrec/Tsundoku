using Tsundoku.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Tsundoku.Models; // Assuming Tsundoku.Models contains User, Format, etc.

namespace Tsundoku.Helpers
{
    /// <summary>
    /// Provides static helper methods for resolving application-specific file paths,
    /// and for common file operations especially for MSIX-packaged applications.
    /// </summary>
    public static class AppFileHelper
    {
        private static readonly HashSet<string> ValidImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png"
        };

        /// <summary>
        /// Gets the full, writable path for application-specific files,
        /// ensuring the directory exists and is compliant with MSIX sandbox security.
        /// </summary>
        /// <param name="fileName">The name of the file (e.g., "UserData.json").</param>
        /// <param name="appSpecificSubfolder">A subfolder name *within* Tsundoku (e.g., "Logs", "Covers").
        /// Pass an empty string for files directly under the "Tsundoku" folder (like UserData.json).</param>
        /// <returns>The full path to the specified file in a writable location.</returns>
        public static string GetFilePath(string fileName, string appSpecificSubfolder)
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
            return GetFilePath(ViewModelBase.USER_DATA_FILEPATH, "");
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
        public static string CreateUniqueCoverFilePath(string coverLink, string title, Format bookType, ref uint dupeIndex)
        {
            string coversFolderPath = GetCoversFolderPath();

            string safeTitle = ExtensionMethods.RemoveInPlaceCharArray(string.Concat(title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))).Replace(",", string.Empty);

            string extension = Path.GetExtension(coverLink).ToLowerInvariant();

            if (!extension.StartsWith('.'))
            {
                extension = "." + extension.TrimStart('.');
            }

            if (!ValidImageExtensions.Contains(extension))
            {
                LOGGER.Warn("Derived extension '{DerivedExtension}' from cover link '{CoverLink}' is not a recognized image format. Defaulting to '.png'.", extension, coverLink);
                extension = ".png";
            }

            string format = bookType.ToString().ToUpper();
            string baseFileName = $"{safeTitle}_{format}";
            string newPath = Path.Combine(coversFolderPath, $"{baseFileName}{extension}");

            if (File.Exists(newPath))
            {
                uint count = (dupeIndex == 0) ? 2 : dupeIndex;

                string potentialPath;
                while (true)
                {
                    potentialPath = Path.Combine(coversFolderPath, $"{baseFileName}_{count}{extension}");
                    if (!File.Exists(potentialPath))
                    {
                        newPath = potentialPath;
                        dupeIndex = count;
                        LOGGER.Info("Duplicate detected. New cover path created: {NewPath}", newPath);
                        break;
                    }
                    LOGGER.Info("Path {PotentialPath} already exists. Trying next index.", potentialPath);
                    count++;
                }
            }
            return newPath;
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

            if (!extension.StartsWith("."))
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
    }
}