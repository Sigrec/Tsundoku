using Tsundoku.Helpers;

namespace Tsundoku.Models
{
    public enum Genre
    {
        [StringValue("Action")] Action,
        [StringValue("Adventure")] Adventure,
        [StringValue("Comedy")] Comedy,
        [StringValue("Drama")] Drama,
        [StringValue("Ecchi")] Ecchi,
        [StringValue("Fantasy")] Fantasy,
        [StringValue("Horror")] Horror,
        [StringValue("Mahou Shoujo")] MahouShoujo,
        [StringValue("Mecha")] Mecha,
        [StringValue("Music")] Music,
        [StringValue("Mystery")] Mystery,
        [StringValue("Psychological")] Psychological,
        [StringValue("Romance")] Romance,
        [StringValue("Sci-Fi")] SciFi,
        [StringValue("Slice of Life")] SliceOfLife,
        [StringValue("Sports")] Sports,
        [StringValue("Supernatural")] Supernatural,
        [StringValue("Thriller")] Thriller,
        None
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
}