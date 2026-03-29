using Avalonia.Media;

namespace Tsundoku.Clients;

/// <summary>
/// Client for The Color API (thecolorapi.com) — generates harmonious color palettes.
/// Free, no API key required.
/// </summary>
public sealed class ColorApi
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.thecolorapi.com";

    public ColorApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ColorApiClient");
    }

    /// <summary>
    /// Generates a harmonious color palette from a seed color.
    /// </summary>
    /// <param name="seedHex">Seed color hex without # (e.g., "20232d").</param>
    /// <param name="mode">The color harmony algorithm to use.</param>
    /// <param name="count">Number of colors to generate (1+).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of generated colors, or empty array on failure.</returns>
    public async Task<GeneratedColor[]> GenerateSchemeAsync(
        string seedHex,
        ColorSchemeMode mode,
        int count,
        CancellationToken cancellationToken = default)
    {
        string modeParam = ConvertMode(mode);
        string cleanHex = seedHex.TrimStart('#');
        string url = $"{BaseUrl}/scheme?hex={cleanHex}&mode={modeParam}&count={count}&format=json";

        LOGGER.Debug("Requesting color scheme: {Url}", url);

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                LOGGER.Warn("Color API returned {StatusCode} for seed {Hex}", response.StatusCode, cleanHex);
                return [];
            }

            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!json.RootElement.TryGetProperty("colors", out JsonElement colorsElement) ||
                colorsElement.ValueKind != JsonValueKind.Array)
            {
                LOGGER.Warn("Color API response missing 'colors' array");
                return [];
            }

            List<GeneratedColor> colors = new(count);
            foreach (JsonElement colorElement in colorsElement.EnumerateArray())
            {
                string hex = colorElement.GetProperty("hex").GetProperty("value").GetString() ?? string.Empty;
                string name = colorElement.GetProperty("name").GetProperty("value").GetString() ?? string.Empty;

                Color parsedColor = Color.Parse(hex);
                colors.Add(new GeneratedColor(hex, name, parsedColor));
            }

            LOGGER.Info("Generated {Count} colors from seed #{Hex} using {Mode} mode", colors.Count, cleanHex, modeParam);
            return [.. colors];
        }
        catch (TaskCanceledException)
        {
            LOGGER.Debug("Color scheme request cancelled");
            return [];
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to generate color scheme from seed #{Hex}", cleanHex);
            return [];
        }
    }

    /// <summary>
    /// Generates a palette from a seed color and converts results to SolidColorBrushes.
    /// </summary>
    public async Task<SolidColorBrush[]> GenerateBrushesAsync(
        string seedHex,
        ColorSchemeMode mode,
        int count,
        CancellationToken cancellationToken = default)
    {
        GeneratedColor[] colors = await GenerateSchemeAsync(seedHex, mode, count, cancellationToken);
        return [.. colors.AsValueEnumerable().Select(static c => new SolidColorBrush(c.Color))];
    }

    private static string ConvertMode(ColorSchemeMode mode)
    {
        return mode switch
        {
            ColorSchemeMode.Monochrome => "monochrome",
            ColorSchemeMode.MonochromeDark => "monochrome-dark",
            ColorSchemeMode.MonochromeLight => "monochrome-light",
            ColorSchemeMode.Analogic => "analogic",
            ColorSchemeMode.Complement => "complement",
            ColorSchemeMode.AnalogicComplement => "analogic-complement",
            ColorSchemeMode.Triad => "triad",
            ColorSchemeMode.Quad => "quad",
            _ => "analogic"
        };
    }
}
