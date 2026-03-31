using Avalonia.Media;

namespace Tsundoku.Clients;

/// <summary>
/// A generated color with hex value, human-readable name, and parsed <see cref="Color"/>.
/// </summary>
/// <param name="Hex">The hex representation of the color (e.g., "#FF5733").</param>
/// <param name="Name">The human-readable name of the color.</param>
/// <param name="Color">The parsed Avalonia <see cref="Color"/> value.</param>
public sealed record GeneratedColor(string Hex, string Name, Color Color);
