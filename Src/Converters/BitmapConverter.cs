using Avalonia.Media.Imaging;

namespace Tsundoku.Converters
{
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
            if (bitmap == null)
            {
                LOGGER.Warn("Attempted to convert a null Bitmap to PNG bytes.");
                return null;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, 100);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Failed to convert Bitmap to PNG bytes.");
                return null;
            }
        }

        /// <summary>
        /// Converts an Avalonia Bitmap to a byte array in JPEG format.
        /// JPEG is a lossy format, good for photographs where file size is a concern.
        /// You can specify the quality (0-100).
        /// </summary>
        /// <param name="bitmap">The Avalonia Bitmap to convert.</param>
        /// <param name="quality">The JPEG quality (0-100). 90 is a good default for reasonable quality and size.</param>
        /// <returns>A byte array representing the image in JPEG format, or null on failure.</returns>
        public static byte[]? BitmapToJpegBytes(Bitmap? bitmap, int quality = 90)
        {
            if (bitmap == null)
            {
                LOGGER.Warn("Attempted to convert a null Bitmap to JPEG bytes.");
                return null;
            }
            if (quality < 0 || quality > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(quality), "JPEG quality must be between 0 and 100.");
            }

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    // Avalonia's Bitmap.Save() can take a quality parameter for JPEG
                    bitmap.Save(stream, quality);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Failed to convert Bitmap to JPEG bytes.");
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
            if (imageBytes == null || imageBytes.Length == 0)
            {
                LOGGER.Warn("Attempted to convert a null or empty byte array to Bitmap.");
                return null;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(imageBytes))
                {
                    // Avalonia's Bitmap constructor can load directly from a stream
                    return new Bitmap(stream);
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "Failed to convert bytes to Bitmap.");
                return null;
            }
        }
    }
}