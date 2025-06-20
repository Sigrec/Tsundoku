using System.Reflection;

namespace Tsundoku.Models;

/// <summary>
/// Attribute for specifying one or more string aliases for a Genre enum value.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class GenreAliasesAttribute : Attribute
{
    public string[] Aliases { get; }

    public GenreAliasesAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }
}

/// <summary>
/// Enum representing possible genres associated with anime/manga.
/// </summary>
public enum Genre
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
    [GenreAliases("Thriller")] Thriller
}

/// <summary>
/// Extension methods and utilities for working with Genre enum values and aliases.
/// </summary>
public static class GenreExtensions
{
    private static readonly Dictionary<string, Genre> GenreMap;

    static GenreExtensions()
    {
        GenreMap = new Dictionary<string, Genre>(StringComparer.OrdinalIgnoreCase);

        foreach (Genre genre in Enum.GetValues<Genre>())
        {
            MemberInfo member = typeof(Genre).GetMember(genre.ToString()).First();
            GenreAliasesAttribute? attribute = member.GetCustomAttribute<GenreAliasesAttribute>();
            if (attribute != null)
            {
                foreach (string alias in attribute.Aliases)
                {
                    GenreMap[alias] = genre;
                }
            }
        }
    }

    /// <summary>
    /// Attempts to map a genre string to its corresponding enum value.
    /// </summary>
    public static bool TryGetGenre(string genre, out Genre result)
    {
        return GenreMap.TryGetValue(genre, out result);
    }
}