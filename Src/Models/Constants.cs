namespace Tsundoku.Models;

public static class Constants
{
	public const ushort USER_ICON_HEIGHT = 71;
	public const ushort USER_ICON_WIDTH = 71;
    public const ushort CARD_HEIGHT = 290;
	public const ushort CARD_WIDTH = 525;
    public const ushort RIGHT_SIDE_CARD_WIDTH = 355;
    public const ushort MENU_LENGTH = 400;
    public const ushort LEFT_SIDE_CARD_WIDTH = CARD_WIDTH - RIGHT_SIDE_CARD_WIDTH;
    public const ushort BOTTOM_SECTION_CARD_HEIGHT = 40;
    public const ushort TOP_SECTION_CARD_HEIGHT = CARD_HEIGHT - BOTTOM_SECTION_CARD_HEIGHT - 40;
    public const ushort USER_NOTES_WIDTH = RIGHT_SIDE_CARD_WIDTH - 74;
    public const ushort USER_NOTES_HEIGHT = TOP_SECTION_CARD_HEIGHT - 16;
    public const ushort IMAGE_HEIGHT = CARD_HEIGHT - BOTTOM_SECTION_CARD_HEIGHT;

    public static IReadOnlyList<string> ADVANCED_SEARCH_FILTERS { get; } = ["CurVolumes==", "CurVolumes>=", "CurVolumes<=", "Demographic==Josei", "Demographic==Seinen", "Demographic==Shoujo", "Demographic==Shounen", "Demographic==Unknown", "Favorite==False", "Favorite==True", "Format==Manga", "Format==Novel", "Genre==Action", "Genre==Adventure", "Genre==Comedy", "Genre==Drama", "Genre==Ecchi", "Genre==Fantasy", "Genre==Horror", "Genre==MahouShoujo", "Genre==Mecha", "Genre==Music", "Genre==Mystery", "Genre==Psychological", "Genre==Romance", "Genre==SciFi", "Genre==SliceOfLife", "Genre==Sports", "Genre==Supernatural", "Genre==Thriller", "MaxVolumes==", "MaxVolumes>=", "MaxVolumes<=", "Notes==", "Publisher==", "Read==", "Read>=", "Read<=", "Rating==", "Rating>=", "Rating<=", "Series==Complete", "Series==InComplete", "Status==Cancelled", "Status==Finished", "Status==Hiatus", "Status==Ongoing", "Value==", "Value>=", "Value<="];

	public static readonly string[] AVAILABLE_CURRENCY = ["$", "€", "£", "¥", "₣", "₹", "₱", "₩", "₽", "₺", "₫", "฿", "₸", "₼", "₾", "₻"]; // "Rp", "RM", "﷼", "د.إ", "د. ك"

	public static readonly string[] AVAILABLE_COLLECTION_FILTERS = ["None", "Query", "Favorites", "Complete", "Incomplete", "Ongoing", "Finished", "Hiatus", "Cancelled", "Shounen", "Shoujo", "Seinen", "Josei", "Manga", "Novel", "Action", "Adventure", "Comedy", "Drama", "Ecchi", "Fantsay", "Horror", "Mahou Shoujo", "Mecha", "Music", "Mystery", "Psychological", "Romance", "Sci-Fi", "Slice of Life", "Sports", "Supernatural", "Thriller", "Publisher", "Read", "Unread", "Rating", "Value"];

    public enum Site
    {
        AniList,
        MangaDex
    }
}