using System.Collections.Frozen;
using System.Reflection;

namespace Tsundoku.Models;

public static class Changelog
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    public static readonly FrozenDictionary<string, string[]> Entries = LoadEntries();

    private static FrozenDictionary<string, string[]> LoadEntries()
    {
        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .First(n => n.EndsWith("changelog.json", StringComparison.Ordinal));

            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            Dictionary<string, string[]>? data = JsonSerializer.Deserialize<Dictionary<string, string[]>>(stream);
            return data?.ToFrozenDictionary(StringComparer.Ordinal)
                ?? FrozenDictionary<string, string[]>.Empty;
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to load changelog.json from embedded resources");
            return FrozenDictionary<string, string[]>.Empty;
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
