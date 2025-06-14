using Avalonia.Media.Imaging;

namespace Tsundoku.Helpers
{
    public class BitmapHelper
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private readonly IHttpClientFactory _httpClientFactory; // New: To be injected

        // Constructor for Dependency Injection
        public BitmapHelper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Updates the cover image from a local file path. It loads the existing image,
        /// scales it if necessary, and saves it to the specified destination path.
        /// </summary>
        /// <param name="sourceFilePath">The existing local path to the image file.</param>
        /// <param name="destinationCoverPath">The path where the (potentially scaled) image should be saved (e.g., in the "Covers" directory).</param>
        /// <returns>The generated and scaled Avalonia Bitmap, or null if any operation fails.</returns>
        public static Bitmap? UpdateCoverFromFilePath(string sourceFilePath, string destinationCoverPath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                LOGGER.Error("Provided source file path is null/empty or does not exist: {SourceFilePath}", sourceFilePath);
                return null;
            }

            try
            {
                LOGGER.Debug("Attempting to load image from local path: {SourceFilePath}", sourceFilePath);
                // Avalonia Bitmap constructor can load directly from a file path
                using (Bitmap loadedBitmap = new Bitmap(sourceFilePath))
                {
                    LOGGER.Debug("Image loaded from local path. Dimensions: {Width}x{Height}", loadedBitmap.PixelSize.Width, loadedBitmap.PixelSize.Height);
                    // ProcessAndSaveBitmap is synchronous, so it runs within this Task.Run context
                    return ProcessAndSaveBitmap(loadedBitmap, destinationCoverPath, sourceFilePath);
                }
            }
            catch (ArgumentException argEx)
            {
                LOGGER.Error(argEx, "Invalid image format detected when loading from local path: {SourceFilePath}.", sourceFilePath);
                return null;
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Failed to load or process image from local path: {SourceFilePath}.", sourceFilePath);
                return null;
            }
        }

        /// <summary>
        /// Downloads an image from a URL, processes it, and saves it as a PNG Avalonia Bitmap
        /// to a local path.
        /// </summary>
        /// <param name="imageUrl">The URL of the image to download.</param>
        /// <param name="destinationCoverPath">The path to save the image file in the "Covers" directory on the user's machine.</param>
        /// <returns>The generated and scaled Avalonia Bitmap, or null if any operation fails.</returns>
        public async Task<Bitmap?> UpdateCoverFromUrlAsync(string imageUrl, string destinationCoverPath)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                LOGGER.Error("Provided image URL is null or empty.");
                return null;
            }

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri? imageUri) ||
                (imageUri.Scheme != Uri.UriSchemeHttp && imageUri.Scheme != Uri.UriSchemeHttps))
            {
                LOGGER.Warn("Cover URL '{ImageUrl}' is not a valid absolute URI or has an unsupported scheme. Cannot download image.", imageUrl);
                return null;
            }

            LOGGER.Debug("Attempting to download image from: {ResolvedImageUrl}", imageUri.OriginalString);
            byte[] imageByteArray;
            try
            {
                // Use the named HttpClient client from the factory for specific configurations
                using (HttpClient httpClient = _httpClientFactory.CreateClient("AddCoverClient"))
                {
                    imageByteArray = await httpClient.GetByteArrayAsync(imageUri);
                }
                LOGGER.Info("Successfully downloaded image from {ResolvedImageUrl}. Size: {Size} bytes.", imageUri.OriginalString, imageByteArray.Length);
            }
            catch (HttpRequestException httpEx)
            {
                LOGGER.Error(httpEx, "HTTP error downloading image from {ResolvedImageUrl}. Status: {HttpStatus}", imageUri.OriginalString, httpEx.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Failed to download image from {ResolvedImageUrl}.", imageUri.OriginalString);
                return null;
            }

            Bitmap? originalBitmap = null;
            try
            {
                using (MemoryStream imageStream = new MemoryStream(imageByteArray))
                {
                    originalBitmap = new Bitmap(imageStream);
                    LOGGER.Debug("Original bitmap created from downloaded data. Dimensions: {Width}x{Height}", originalBitmap.PixelSize.Width, originalBitmap.PixelSize.Height);
                    return ProcessAndSaveBitmap(originalBitmap, destinationCoverPath, imageUri.OriginalString);
                }
            }
            catch (ArgumentException argEx)
            {
                LOGGER.Error(argEx, "Invalid image format detected when creating bitmap from downloaded data from {ResolvedImageUrl}.", imageUri.OriginalString);
                return null;
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Error processing or saving image downloaded from {ResolvedImageUrl} to {NewPath}.", imageUri.OriginalString, destinationCoverPath);
                return null;
            }
            finally
            {
                // Ensure originalBitmap is disposed if it was created here
                originalBitmap?.Dispose();
                LOGGER.Debug("Original bitmap (if created) disposed.");
            }
        }

        /// <summary>
        /// Private helper method to scale an Avalonia Bitmap to a standard size and save it as a PNG.
        /// </summary>
        /// <param name="originalBitmap">The bitmap to process and save.</param>
        /// <param name="savePath">The path where the scaled image should be saved.</param>
        /// <param name="sourceIdentifier">A string identifying the source (e.g., file path or URL) for logging purposes.</param>
        /// <returns>The generated and scaled Avalonia Bitmap, or null if scaling or saving fails.</returns>
        private static Bitmap? ProcessAndSaveBitmap(Bitmap originalBitmap, string savePath, string sourceIdentifier)
        {
            Bitmap? scaledBitmap = null;
            try
            {
                if (originalBitmap.PixelSize.Width != LEFT_SIDE_CARD_WIDTH || originalBitmap.PixelSize.Height != IMAGE_HEIGHT)
                {
                    LOGGER.Debug("Scaling bitmap from {Width}x{Height} to {TargetWidth}x{TargetHeight} for {Source}.",
                        originalBitmap.PixelSize.Width, originalBitmap.PixelSize.Height,
                        LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT,
                        sourceIdentifier);

                    scaledBitmap = originalBitmap.CreateScaledBitmap(
                        new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT),
                        BitmapInterpolationMode.HighQuality);
                }
                else
                {
                    LOGGER.Debug("Bitmap already at target size for {Source}. No scaling needed.", sourceIdentifier);
                    scaledBitmap = originalBitmap; // Use original if no scaling needed
                }

                // Ensure the directory exists
                string? directory = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    LOGGER.Debug("Created directory: {DirectoryPath}", directory);
                }

                // Save the bitmap (whether scaled or original)
                // Avalonia Bitmap.Save handles various image formats; 100 usually means best quality for PNG.
                scaledBitmap.Save(savePath, 100);
                LOGGER.Info("Successfully saved cover image to: {SavePath} from {Source}.", savePath, sourceIdentifier);

                return scaledBitmap;
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Failed to scale or save bitmap from {SourceIdentifier} to {SavePath}.", sourceIdentifier, savePath);
                scaledBitmap?.Dispose(); // Dispose if creation or save failed
                return null;
            }
        }

        /// <summary>
        /// Converts an Avalonia Bitmap to a byte array asynchronously, always saving it in PNG format.
        /// This method offloads the operation to a background thread to prevent UI freezing.
        /// </summary>
        /// <param name="image">The Avalonia Bitmap to convert.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation.
        /// The Task's Result is a byte array representing the image data in PNG format,
        /// or an empty array if the input bitmap is null.
        /// </returns>
        /// <exception cref="System.Exception">Throws an exception if the bitmap saving process fails.</exception>
        public static async Task<byte[]> ImageToPngByteArrayAsync(Bitmap image)
        {
            if (image == null)
            {
                LOGGER.Warn("Attempted to convert a null Avalonia Bitmap to PNG byte array asynchronously. Returning an empty array.");
                return [];
            }

            // Offload the potentially long-running operation to a background thread.
            return await Task.Run(() =>
            {
                using MemoryStream stream = new();
                try
                {
                    // 3. Core Logic (executed on a background thread): Save the Avalonia Bitmap.
                    // Avalonia's Bitmap.Save(Stream) inherently saves as PNG.
                    image.Save(stream);

                    // 4. Conversion (executed on a background thread): Convert the stream's content to a byte array.
                    return stream.ToArray();
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex, "Failed to convert Avalonia Bitmap to PNG byte array in background task.");
                    return [];
                }
            });
        }
    }
}