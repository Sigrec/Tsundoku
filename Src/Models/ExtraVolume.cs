namespace Tsundoku.Models;

/// <summary>
/// A named book in a series that has no volume number, such as a fanbook or a box set bonus.
/// Shown after the numbered volumes and counted alongside specials.
/// </summary>
public sealed record ExtraVolume(string Name, bool Owned);
