using System.Reflection;

namespace Tsundoku.Models.Enums;

public static class SeriesGenreEnum
{
    /// <summary>
    /// Attribute for specifying one or more string aliases for a Genre enum value.
    /// This attribute is nested to clearly indicate its relation to this enum.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class GenreAliasesAttribute : Attribute
    {
        public string[] Aliases { get; }

        public GenreAliasesAttribute(params string[] aliases)
        {
            Aliases = aliases ?? []; // Ensure aliases is not null
        }
    }

    /// <summary>
    /// Enum representing possible genres associated with anime/manga.
    /// Includes a default 'Unknown' value for robustness.
    /// </summary>
    public enum SeriesGenre
    {
        [GenreAliases("Action")] Action,
        [GenreAliases("Adventure")] Adventure,
        [GenreAliases("Comedy")] Comedy,
        [GenreAliases("Drama")] Drama,
        [GenreAliases("Ecchi")] Ecchi,
        [GenreAliases("Fantasy")] Fantasy,
        [GenreAliases("Hentai")] Hentai,
        [GenreAliases("Horror")] Horror,
        [GenreAliases("Mahou Shoujo", "Magical Girls")] MahouShoujo,
        [GenreAliases("Mecha")] Mecha,
        [GenreAliases("Music")] Music,
        [GenreAliases("Mystery")] Mystery,
        [GenreAliases("Psychological")] Psychological,
        [GenreAliases("Romance")] Romance,
        [GenreAliases("Sci-Fi", "SciFi")] SciFi,
        [GenreAliases("Slice of Life", "SliceOfLife")] SliceOfLife,
        [GenreAliases("Sports")] Sports,
        [GenreAliases("Supernatural")] Supernatural,
        [GenreAliases("Thriller")] Thriller,
        [GenreAliases("Unknown")] Unknown // Add a default/fallback genre
    }

    // A static readonly array to easily access all enum values, consistent with other enums.
    public static readonly SeriesGenre[] AllSeriesGenres = Enum.GetValues<SeriesGenre>();

    // Private static dictionary to store the mapping from alias/string to enum value.
    // It uses StringComparer.OrdinalIgnoreCase for case-insensitive lookups.
    private static readonly Dictionary<string, SeriesGenre> GenreMap;

    // Static constructor: This code runs once, the first time any member of SeriesGenreEnums is accessed.
    static SeriesGenreEnum()
    {
        GenreMap = new Dictionary<string, SeriesGenre>(StringComparer.OrdinalIgnoreCase);

        // Iterate through each enum value to build the map
        foreach (SeriesGenre genre in Enum.GetValues<SeriesGenre>())
        {
            string enumName = genre.ToString();

            // 1. Add the enum's own name (e.g., "Action") to the map.
            // This ensures that if the direct enum name is used as input, it's recognized.
            // We use TryAdd to avoid issues if an alias somehow matches the enum's name.
            GenreMap.TryAdd(enumName, genre);

            // 2. Get the custom GenreAliasesAttribute for the current enum member
            // Using typeof(SeriesGenre).GetMember(enumName).FirstOrDefault()
            // is more robust than .First() in case of unexpected scenarios.
            MemberInfo? member = typeof(SeriesGenre).GetMember(enumName).FirstOrDefault();

            // If the member info is found and it has the attribute
            if (member != null)
            {
                GenreAliasesAttribute? attribute = member.GetCustomAttribute<GenreAliasesAttribute>();
                if (attribute != null)
                {
                    // 3. Add all defined aliases for this genre to the map
                    foreach (string alias in attribute.Aliases)
                    {
                        // If an alias conflicts with a previous entry, the last one processed wins.
                        // For aliases, overwriting is often acceptable.
                        GenreMap[alias] = genre;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attempts to parse a genre string (including its aliases) into a SeriesGenre value.
    /// The lookup is case-insensitive. This is the safer method as it doesn't throw exceptions.
    /// </summary>
    /// <param name="genreString">The genre string or alias to look up.</param>
    /// <param name="result">When this method returns, contains the SeriesGenre equivalent to the genreString, if the conversion succeeded, or SeriesGenre.Unknown if the conversion failed.</param>
    /// <returns>True if the genreString was successfully mapped; otherwise, false.</returns>
    public static bool TryParse(string genreString, out SeriesGenre result)
    {
        // The dictionary's StringComparer.OrdinalIgnoreCase handles case-insensitivity.
        if (GenreMap.TryGetValue(genreString, out result))
        {
            return true;
        }
        // If not found, set result to Unknown and return false
        result = SeriesGenre.Unknown;
        return false;
    }

    /// <summary>
    /// Parses a genre string (including its aliases) into a SeriesGenre value.
    /// The lookup is case-insensitive. Throws an exception if the genre string is not recognized.
    /// Use TryParse for non-throwing behavior.
    /// </summary>
    /// <param name="genreString">The genre string or alias to look up.</param>
    /// <returns>The corresponding SeriesGenre value.</returns>
    /// <exception cref="ArgumentException">Thrown if the genre string is not a recognized genre or alias.</exception>
    public static SeriesGenre Parse(string genreString)
    {
        if (TryParse(genreString, out SeriesGenre result))
        {
            return result;
        }
        return SeriesGenre.Unknown;
    }

    /// <summary>
    /// Gets the primary aliases associated with a SeriesGenre value.
    /// </summary>
    /// <param name="genre">The SeriesGenre value.</param>
    /// <returns>An array of aliases for the genre. Returns the enum's name as a string if no aliases are defined.</returns>
    public static IEnumerable<string> GetAliases(SeriesGenre genre)
    {
        MemberInfo? member = typeof(SeriesGenre).GetMember(genre.ToString()).FirstOrDefault();
        if (member != null)
        {
            GenreAliasesAttribute? attribute = member.GetCustomAttribute<GenreAliasesAttribute>();
            if (attribute != null && attribute.Aliases.Length > 0)
            {
                return attribute.Aliases;
            }
        }
        // If no aliases attribute or no aliases defined, return the enum's own name
        return [genre.ToString()];
    }
}