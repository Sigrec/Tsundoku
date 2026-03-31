using Avalonia.Media.Imaging;

namespace Tsundoku.Converters;

public static class BitmapConverter
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Converts an Avalonia Bitmap to a byte array in PNG format.
    /// PNG is a lossless format, good for images with sharp edges or transparency.
    /// </summary>
    /// <param name="bitmap">The Avalonia Bitmap to convert.</param>
    /// <returns>A byte array representing the image in PNG format, or null on failure.</returns>
    public static byte[]? BitmapToPngBytes(Bitmap? bitmap)
    {
        if (bitmap is null)
        {
            LOGGER.Warn("Attempted to convert a null Bitmap to PNG bytes.");
            return null;
        }

        try
        {
            using MemoryStream stream = new();
            bitmap.Save(stream, 100);
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to convert Bitmap to PNG bytes.");
            return null;
        }
    }

    /// <summary>
    /// Converts a byte array (image data) back to an Avalonia Bitmap.
    /// </summary>
    /// <param name="imageBytes">The byte array containing image data (e.g., PNG, JPEG, BMP).</param>
    /// <returns>An Avalonia Bitmap object, or null on failure.</returns>
    public static Bitmap? BytesToBitmap(byte[]? imageBytes)
    {
        if (imageBytes is null || imageBytes.Length == 0)
        {
            LOGGER.Warn("Attempted to convert a null or empty byte array to Bitmap.");
            return null;
        }

        try
        {
            using MemoryStream stream = new(imageBytes);
            return new Bitmap(stream);
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to convert bytes to Bitmap.");
            return null;
        }
    }
}