using System.Runtime.Serialization;

namespace Tsundoku.Models
{
    public enum Genre
    {
        [EnumMember(Value = "Action")] Action,
        [EnumMember(Value = "Adventure")] Adventure,
        [EnumMember(Value = "Comedy")] Comedy,
        [EnumMember(Value = "Drama")] Drama,
        [EnumMember(Value = "Ecchi")] Ecchi,
        [EnumMember(Value = "Fantasy")] Fantasy,
        [EnumMember(Value = "Horror")] Horror,
        [EnumMember(Value = "Mahou Shoujo")] MahouShoujo,
        [EnumMember(Value = "Mecha")] Mecha,
        [EnumMember(Value = "Music")] Music,
        [EnumMember(Value = "Mystery")] Mystery,
        [EnumMember(Value = "Psychological")] Psychological,
        [EnumMember(Value = "Romance")] Romance,
        [EnumMember(Value = "Sci-Fi")] SciFi,
        [EnumMember(Value = "Slice of Life")] SliceOfLife,
        [EnumMember(Value = "Sports")] Sports,
        [EnumMember(Value = "Supernatural")] Supernatural,
        [EnumMember(Value = "Thriller")] Thriller,
        [EnumMember(Value = "None")] None
    }

    public class GenreExtensions
    {
        public static Genre GetGenreFromString(string genre)
        {
            return genre switch
            {
                "Action" => Genre.Action,
                "Adventure" => Genre.Adventure,
                "Comedy" => Genre.Comedy,
                "Drama" => Genre.Drama,
                "Ecchi" => Genre.Ecchi,
                "Fantasy" => Genre.Fantasy,
                "Horror" => Genre.Horror,
                "Mahou Shoujo" or "Magical Girls" => Genre.MahouShoujo,
                "Mecha" => Genre.Mecha,
                "Music" => Genre.Music,
                "Mystery" => Genre.Mystery,
                "Psychological" => Genre.Psychological,
                "Romance" => Genre.Romance,
                "Sci-Fi" or "SciFi" => Genre.SciFi,
                "Slice of Life" or "Slice Of Life" or "SliceOfLife" => Genre.SliceOfLife,
                "Sports" => Genre.Sports,
                "Supernatural" => Genre.Supernatural,
                "Thriller" => Genre.Thriller,
                _ => Genre.None,
            };
        }
    }

    public sealed class GenreEntry
    {
        public Genre Genre { get; }

        public GenreEntry(Genre genre)
        {
            Genre = genre;
        }
    }
}