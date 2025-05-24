using Tsundoku.Helpers;

namespace Tsundoku.Models
{
    public enum TsundokuFilter
    {
        [StringValue("Ongoing")] Ongoing,
        [StringValue("Finished")] Finished,
        [StringValue("Hiatus")] Hiatus,
        [StringValue("Cancelled")] Cancelled,
        [StringValue("Complete")] Complete,
        [StringValue("Incomplete")] Incomplete,
        [StringValue("Favorites")] Favorites,
        [StringValue("Manga")] Manga,
        [StringValue("Novel")] Novel,
        [StringValue("Shounen")] Shounen,
        [StringValue("Shoujo")] Shoujo,
        [StringValue("Seinen")] Seinen,
        [StringValue("Josei")] Josei,
        [StringValue("Publisher")] Publisher,
        [StringValue("Read")] Read,
        [StringValue("Unread")] Unread,
        [StringValue("Rating")] Rating,
        [StringValue("Value")] Value,
        [StringValue("Query")] Query,
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
        [StringValue("None")] None,
    }

    public class TsundokuFilterExtensions
    {
        public static TsundokuFilter GetFilterFromString(string filter)
        {
            return filter switch
            {
                "Ongoing" => TsundokuFilter.Ongoing,
                "Finished" => TsundokuFilter.Finished,
                "Hiatus" => TsundokuFilter.Hiatus,
                "Cancelled" => TsundokuFilter.Cancelled,
                "Complete" => TsundokuFilter.Complete,
                "Incomplete" => TsundokuFilter.Incomplete,
                "Favorites" => TsundokuFilter.Favorites,
                "Manga" => TsundokuFilter.Manga,
                "Novel" => TsundokuFilter.Novel,
                "Shounen" => TsundokuFilter.Shounen,
                "Shoujo" => TsundokuFilter.Shoujo,
                "Seinen" => TsundokuFilter.Seinen,
                "Josei" => TsundokuFilter.Josei,
                "Publisher" => TsundokuFilter.Publisher,
                "Read" => TsundokuFilter.Read,
                "Unread" => TsundokuFilter.Unread,
                "Rating" => TsundokuFilter.Rating,
                "Value" => TsundokuFilter.Value,
                "Query" => TsundokuFilter.Query,
                "Action" => TsundokuFilter.Action,
                "Adventure" => TsundokuFilter.Adventure,
                "Comedy" => TsundokuFilter.Comedy,
                "Drama" => TsundokuFilter.Drama,
                "Ecchi" => TsundokuFilter.Ecchi,
                "Fantasy" => TsundokuFilter.Fantasy,
                "Horror" => TsundokuFilter.Horror,
                "Mahou Shoujo" or "Magical Girls" => TsundokuFilter.MahouShoujo,
                "Mecha" => TsundokuFilter.Mecha,
                "Music" => TsundokuFilter.Music,
                "Mystery" => TsundokuFilter.Mystery,
                "Psychologicl" => TsundokuFilter.Psychological,
                "Romance" => TsundokuFilter.Romance,
                "Sci-Fi" or "SciFi" => TsundokuFilter.SciFi,
                "Slice of Life" or "Slice Of Life" or "SliceOfLife" => TsundokuFilter.SliceOfLife,
                "Sports" => TsundokuFilter.Sports,
                "Supernatural" => TsundokuFilter.Supernatural,
                "Thriller" => TsundokuFilter.Thriller,
                _ => TsundokuFilter.None,
            };
        }
    }
}