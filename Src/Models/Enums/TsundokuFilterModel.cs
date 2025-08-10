using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using Tsundoku.Helpers;

namespace Tsundoku.Models.Enums;

public static class TsundokuFilterModel
{
    public static readonly ImmutableArray<string> TSUNDOKU_FILTER_LIST = Enum.GetValues<TsundokuFilter>().AsValueEnumerable().Select(filter => filter.GetEnumMemberValue()).ToImmutableArray();
    public static readonly FrozenDictionary<TsundokuFilter, int> TSUNDOKU_FILTER_DICT =
        Enum.GetValues<TsundokuFilter>().AsValueEnumerable().Select((filter, index) => (filter, index))
            .ToFrozenDictionary(x => x.filter, x => x.index);

    public enum TsundokuFilter
    {
        [EnumMember(Value = "None")] None,
        [EnumMember(Value = "Query")] Query,
        [EnumMember(Value = "Favorites")] Favorites,
        [EnumMember(Value = "Complete")] Complete,
        [EnumMember(Value = "Incomplete")] Incomplete,
        [EnumMember(Value = "Ongoing")] Ongoing,
        [EnumMember(Value = "Finished")] Finished,
        [EnumMember(Value = "Hiatus")] Hiatus,
        [EnumMember(Value = "Cancelled")] Cancelled,
        [EnumMember(Value = "Shounen")] Shounen,
        [EnumMember(Value = "Shoujo")] Shoujo,
        [EnumMember(Value = "Seinen")] Seinen,
        [EnumMember(Value = "Josei")] Josei,
        [EnumMember(Value = "Manga")] Manga,
        [EnumMember(Value = "Manhwa")] Manhwa,
        [EnumMember(Value = "Manhua")] Manhua,
        [EnumMember(Value = "Manfra")] Manfra,
        [EnumMember(Value = "Comic")] Comic,
        [EnumMember(Value = "Novel")] Novel,
        [EnumMember(Value = "Action")] Action,
        [EnumMember(Value = "Adventure")] Adventure,
        [EnumMember(Value = "Comedy")] Comedy,
        [EnumMember(Value = "Drama")] Drama,
        [EnumMember(Value = "Ecchi")] Ecchi,
        [EnumMember(Value = "Fantasy")] Fantasy,
        [EnumMember(Value = "Hentai")] Hentai,
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
        [EnumMember(Value = "Read")] Read,
        [EnumMember(Value = "Unread")] Unread,
    }
}