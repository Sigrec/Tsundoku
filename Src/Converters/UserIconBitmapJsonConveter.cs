using Avalonia.Media.Imaging;

namespace Tsundoku.Converters;

public sealed class UserIconBitmapJsonConverter : JsonConverter<Bitmap>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    public override Bitmap? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? base64String = reader.GetString();

        if (string.IsNullOrWhiteSpace(base64String))
        {
            LOGGER.Warn("Attempted to read null or empty Base64 string for Bitmap.");
            return null;
        }

        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);

            // 1. Convert bytes to the original (unscaled) Bitmap
            Bitmap? originalBitmap = BitmapConverter.BytesToBitmap(imageBytes);

            if (originalBitmap is null)
            {
                LOGGER.Error("Failed to create original Bitmap from byte array.");
                return null;
            }

            // 2. Apply the scaling
            // Ensure PixelSize and BitmapInterpolationMode are accessible
            Bitmap? scaledBitmap = originalBitmap.CreateScaledBitmap(
                new PixelSize(USER_ICON_WIDTH, USER_ICON_HEIGHT),
                BitmapInterpolationMode.HighQuality
            );

            // Dispose of the original bitmap if it's no longer needed
            // This is important for memory management, especially if the original is large.
            originalBitmap.Dispose();

            return scaledBitmap;
        }
        catch (FormatException ex)
        {
            LOGGER.Error(ex, "Failed to convert Base64 string to byte array (invalid format).");
            return null;
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to read and scale Bitmap from JSON (conversion or scaling error).");
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, Bitmap value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            LOGGER.Warn("Writing null Bitmap to JSON.");
            return;
        }

        try
        {
            // When writing, you usually want to serialize the *original* quality/size
            // unless you specifically want to save the scaled version.
            // Assuming you want to save the original for maximum fidelity.
            byte[]? pngBytes = BitmapConverter.BitmapToPngBytes(value);

            if (pngBytes is null)
            {
                writer.WriteNullValue();
                LOGGER.Error("Failed to convert Bitmap to PNG bytes for serialization.");
                return;
            }

            writer.WriteStringValue(Convert.ToBase64String(pngBytes));
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to write Bitmap to JSON as Base64 string.");
            writer.WriteNullValue();
        }
    }
}