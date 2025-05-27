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

        // TODO - Split this function into multiple so one to update cover from a new path, another to update cover from a url
        /// <summary>
        /// Downloads an image from a URL if it's a customImage or the new path does not exist,
        /// and saves it as a PNG Avalonia Bitmap to a local path.
        /// </summary>
        /// <param name="newPath">The path to save the image file in the "Covers" directory on the user's machine.</param>
        /// <param name="httpClient">The HttpClient instance to use for downloading images.</param>
        /// <param name="coverLink">Original image URL from an API call (fallback).</param>
        /// <param name="isCustomImage">Web image URL provided by the user (takes precedence).</param>
        /// <returns>The generated and scaled Avalonia Bitmap, or null if any operation fails.</returns>
        public async Task<Bitmap?> GenerateAvaloniaBitmapAsync(
            string coverPath,
            string newPath = "",
            string coverLink = "",
            bool isCustomImageUrl = false)
        {
            if ((isCustomImageUrl && !string.IsNullOrWhiteSpace(coverLink)) || !File.Exists(coverPath))
            {
                Uri? imageUri = null;

                // Only try coverLink if custom failed or was not provided
                if (imageUri == null && !string.IsNullOrWhiteSpace(coverLink))
                {
                    if (Uri.TryCreate(coverLink, UriKind.Absolute, out Uri coverUri) &&
                        (coverUri.Scheme == Uri.UriSchemeHttp || coverUri.Scheme == Uri.UriSchemeHttps))
                    {
                        imageUri = coverUri;
                        LOGGER.Debug("Using Image Link for Cover {CoverLink}", coverLink);
                    }
                    else
                    {
                        LOGGER.Warn("Cover link '{CoverLink}' is not a valid absolute URI or has an unsupported scheme. Cannot download image.", coverLink);
                    }
                }

                if (imageUri == null)
                {
                    LOGGER.Error("No valid image URL could be resolved from coverLink ('{CoverLink}').", coverLink);
                    return null; // No valid URL to download from
                }
                else
                {
                    LOGGER.Debug("Attempting to download image from: {ResolvedImageUrl}", imageUri.OriginalString);
                }

                byte[] imageByteArray;
                try
                {
                    // Create HttpClient instance internally using the injected factory
                    // Using the named client 'AddCoverClient' as configured in Program.cs
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
                    // 3. Create Bitmap from bytes and scale
                    using (var imageStream = new MemoryStream(imageByteArray))
                    {
                        originalBitmap = new Bitmap(imageStream);
                    }
                    LOGGER.Debug("Original bitmap created from downloaded data. Dimensions: {Width}x{Height}", originalBitmap.PixelSize.Width, originalBitmap.PixelSize.Height);

                    Bitmap newCover = originalBitmap.CreateScaledBitmap(
                        new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT),
                        BitmapInterpolationMode.HighQuality);

                    // 4. Save the scaled bitmap to file
                    newCover.Save(coverPath, 100);
                    LOGGER.Debug("Saved Image File from Url to {file}", coverPath);

                    // 5. Return the newly created Bitmap
                    return newCover;
                }
                catch (ArgumentException argEx)
                {
                    LOGGER.Error(argEx, "Invalid image format detected when creating bitmap from downloaded data from {ResolvedImageUrl}.", imageUri.OriginalString);
                    return null;
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex, "Error processing or saving image downloaded from {ResolvedImageUrl} to {NewPath}.", imageUri.OriginalString, coverPath);
                    return null;
                }
                finally
                {
                    // Ensure originalBitmap is disposed to free unmanaged resources
                    originalBitmap?.Dispose();
                    LOGGER.Debug("Original bitmap (if created) disposed.");
                }
            }
            else
            {
                LOGGER.Debug("Saving Image File from Path {file}", newPath);
                Bitmap loadedBitmap = new Bitmap(string.IsNullOrWhiteSpace(newPath) ? coverPath : newPath);
                if (loadedBitmap.Size.Width != LEFT_SIDE_CARD_WIDTH || loadedBitmap.Size.Height != IMAGE_HEIGHT)
                {
                    Bitmap scaledBitmap = loadedBitmap.CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                    scaledBitmap.Save(coverPath, 100);
                    loadedBitmap.Dispose();
                    return scaledBitmap;
                }

                loadedBitmap.Save(coverPath, 100);
                return loadedBitmap;
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