namespace Tsundoku.Clients;

/// <summary>
/// Color scheme generation modes supported by The Color API.
/// </summary>
public enum ColorSchemeMode
{
    /// <summary>Single-hue variations.</summary>
    Monochrome,
    /// <summary>Single-hue dark variations.</summary>
    MonochromeDark,
    /// <summary>Single-hue light variations.</summary>
    MonochromeLight,
    /// <summary>Adjacent hues on the color wheel.</summary>
    Analogic,
    /// <summary>Opposite hues on the color wheel.</summary>
    Complement,
    /// <summary>Analogic colors combined with their complement.</summary>
    AnalogicComplement,
    /// <summary>Three evenly spaced hues.</summary>
    Triad,
    /// <summary>Four evenly spaced hues.</summary>
    Quad
}
