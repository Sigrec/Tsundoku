namespace Tsundoku.Tests;

/// <summary>
/// NUnit category names. CI runs everything except <see cref="Network"/> on every push, and runs the
/// network tests in a separate job that may fail without blocking a build.
/// </summary>
public static class TestCategories
{
    /// <summary>Tests that call AniList or MangaDex over the internet, so they depend on those services being up.</summary>
    public const string Network = "Network";
}
