using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using Tsundoku.Helpers;

namespace Tsundoku.Models.Enums;

public static class TsundokuSortModel
{
    public static readonly ImmutableArray<string> TSUNDOKU_SORT_LIST = Enum.GetValues<TsundokuSort>().AsValueEnumerable().Select(sort => sort.GetEnumMemberValue()).ToImmutableArray();

    public static readonly FrozenDictionary<TsundokuSort, int> TSUNDOKU_SORT_DICT =
        Enum.GetValues<TsundokuSort>().AsValueEnumerable().Select((sort, index) => (sort, index))
            .ToFrozenDictionary(x => x.sort, x => x.index);

    public enum TsundokuSort
    {
        [EnumMember(Value = "Title A-Z")] TitleAZ,
        [EnumMember(Value = "Title Z-A")] TitleZA,
        [EnumMember(Value = "Rating")] Rating,
        [EnumMember(Value = "Unread")] Unread,
        [EnumMember(Value = "Read")] Read,
        [EnumMember(Value = "Value")] Value,
        [EnumMember(Value = "Volume Count")] VolumeCount,
    }
}
