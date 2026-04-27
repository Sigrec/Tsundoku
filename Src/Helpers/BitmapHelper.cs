using Avalonia.Media.Imaging;
using Microsoft.IO;

namespace Tsundoku.Helpers;

/// <summary>
/// Provides helper methods for loading, scaling, downloading, and converting Avalonia bitmaps for cover images.
/// </summary>
public sealed class BitmapHelper
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private static readonly RecyclableMemoryStreamManager StreamManager = new();
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="BitmapHelper"/> with the specified HTTP client factory.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create HTTP clients for image downloads.</param>
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
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            LOGGER.Error("Provided source file path is null or empty: {SourceFilePath}", sourceFilePath);
            return null;
        }

        try
        {
            LOGGER.Debug("Attempting to load image from local path: {SourceFilePath}", sourceFilePath);
            int targetWidth = LEFT_SIDE_CARD_WIDTH * BITMAP_SCALE;
            using FileStream fileStream = new(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, options: FileOptions.SequentialScan);
            Bitmap decoded = Bitmap.DecodeToWidth(fileStream, targetWidth, BitmapInterpolationMode.HighQuality);
            LOGGER.Debug("Image decoded at target width. Dimensions: {Width}x{Height}", decoded.PixelSize.Width, decoded.PixelSize.Height);
            return SaveBitmap(decoded, destinationCoverPath, sourceFilePath);
        }
        catch (FileNotFoundException)
        {
            LOGGER.Error("Source file does not exist: {SourceFilePath}", sourceFilePath);
            return null;
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
            LOGGER.Warn("Provided image URL {url} is null or empty.", imageUrl);
            return null;
        }

        if (!AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(Path.GetExtension(imageUrl)))
        {
            LOGGER.Warn("Provided image URL {url} is not a '.png', 'jpg' or '.jpeg'", imageUrl);
            return null;
        }

        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri? imageUri) ||
                (imageUri.Scheme != Uri.UriSchemeHttp && imageUri.Scheme != Uri.UriSchemeHttps))
        {
            LOGGER.Warn("Cover URL '{ImageUrl}' is not a valid absolute URI or has an unsupported scheme. Cannot download image.", imageUrl);
            return null;
        }

        LOGGER.Debug("Attempting to download image from: {ResolvedImageUrl}", imageUri.OriginalString);

        try
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient("AddCoverClient");
            using HttpResponseMessage response = await httpClient.GetAsync(imageUri, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                LOGGER.Error("Failed to download image. Status: {StatusCode} from {Url}", response.StatusCode, imageUri.OriginalString);
                return null;
            }

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            await using RecyclableMemoryStream buffer = StreamManager.GetStream(nameof(UpdateCoverFromUrlAsync));
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;

            int targetWidth = LEFT_SIDE_CARD_WIDTH * BITMAP_SCALE;
            Bitmap decoded = Bitmap.DecodeToWidth(buffer, targetWidth, BitmapInterpolationMode.HighQuality);
            LOGGER.Debug("Bitmap decoded at target width. Dimensions: {Width}x{Height}", decoded.PixelSize.Width, decoded.PixelSize.Height);
            return SaveBitmap(decoded, destinationCoverPath, imageUri.OriginalString);
        }
        catch (ArgumentException argEx)
        {
            LOGGER.Error(argEx, "Invalid image format detected when creating bitmap from downloaded data from {ResolvedImageUrl}.", imageUri.OriginalString);
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Error processing or saving image downloaded from {ResolvedImageUrl} to {NewPath}.", imageUri.OriginalString, destinationCoverPath);
        }

        return null;
    }

    /// <summary>
    /// Persists a bitmap to disk as a PNG. The bitmap is assumed to already be at the desired dimensions
    /// (decoded via <see cref="Bitmap.DecodeToWidth(Stream, int, BitmapInterpolationMode)"/>).
    /// </summary>
    /// <param name="bitmap">The bitmap to save. Ownership transfers to the caller on success.</param>
    /// <param name="savePath">The path where the image should be saved.</param>
    /// <param name="sourceIdentifier">A string identifying the source (e.g., file path or URL) for logging purposes.</param>
    /// <returns>The same <paramref name="bitmap"/> instance on success, or null on failure (the bitmap is disposed).</returns>
    private static Bitmap? SaveBitmap(Bitmap bitmap, string savePath, string sourceIdentifier)
    {
        try
        {
            string? directory = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                LOGGER.Debug("Created directory: {DirectoryPath}", directory);
            }

            bitmap.Save(savePath, 100);
            LOGGER.Info("Successfully saved cover image to: {SavePath} from {Source}.", savePath, sourceIdentifier);
            return bitmap;
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to save bitmap from {SourceIdentifier} to {SavePath}.", sourceIdentifier, savePath);
            bitmap.Dispose();
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
        if (image is null)
        {
            LOGGER.Warn("Attempted to convert a null Avalonia Bitmap to PNG byte array asynchronously. Returning an empty array.");
            return [];
        }

        // Offload the potentially long-running operation to a background thread.
        return await Task.Run(() =>
        {
            using RecyclableMemoryStream stream = StreamManager.GetStream(nameof(ImageToPngByteArrayAsync));
            try
            {
                // Avalonia's Bitmap.Save(Stream) inherently saves as PNG.
                image.Save(stream);
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