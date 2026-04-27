using System.Collections.Frozen;
using System.Reflection;

namespace Tsundoku.Models;

/// <summary>
/// A single version's changelog with feature changes and recommended user actions.
/// </summary>
public sealed record ChangelogEntry(string[] Changes, string[] Actions);

[JsonSerializable(typeof(Dictionary<string, ChangelogEntry>))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
internal partial class ChangelogModelContext : JsonSerializerContext { }

/// <summary>
/// Provides access to embedded changelog entries and version-based display logic.
/// </summary>
public static class Changelog
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    /// <summary>Gets the loaded changelog entries keyed by version string.</summary>
    public static readonly FrozenDictionary<string, ChangelogEntry> Entries = LoadEntries();

    private static FrozenDictionary<string, ChangelogEntry> LoadEntries()
    {
        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().AsValueEnumerable()
                .First(n => n.EndsWith("changelog.json", StringComparison.Ordinal));

            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            Dictionary<string, ChangelogEntry>? data = JsonSerializer.Deserialize(stream, ChangelogModelContext.Default.DictionaryStringChangelogEntry);
            return data?.AsValueEnumerable().ToFrozenDictionary(StringComparer.Ordinal)
                ?? FrozenDictionary<string, ChangelogEntry>.Empty;
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to load changelog.json from embedded resources");
            return FrozenDictionary<string, ChangelogEntry>.Empty;
        }
    }

    /// <summary>
    /// Checks whether a changelog should be shown for the current version.
    /// </summary>
    public static bool ShouldShow(string currentVersion, string lastSeenVersion)
    {
        return !string.Equals(currentVersion, lastSeenVersion, StringComparison.Ordinal)
            && Entries.ContainsKey(currentVersion);
    }
}
