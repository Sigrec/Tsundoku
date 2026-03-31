using System.Collections.Frozen;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using static Tsundoku.Models.Enums.SeriesStatusModel;

namespace Tsundoku.ViewModels;

public sealed partial class FilterChipViewModel : ReactiveObject
{
    public static readonly string[] AllFields =
    [
        "Rating", "Value", "Read", "CurVolumes", "MaxVolumes",
        "Format", "Status", "Demographic", "Series", "Favorite", "Genre",
        "Notes", "Publisher"
    ];

    private static readonly FrozenSet<string> NumericFields =
        new[] { "Rating", "Value", "Read", "CurVolumes", "MaxVolumes" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, string[]> EnumValues =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Format"] = Enum.GetValues<SeriesFormat>().Select(e => e.ToString()).ToArray(),
            ["Status"] = Enum.GetValues<SeriesStatus>().Select(e => e.ToString()).ToArray(),
            ["Demographic"] = Enum.GetValues<SeriesDemographic>().Select(e => e.ToString()).ToArray(),
            ["Genre"] = Enum.GetValues<SeriesGenre>().Select(e => e.ToString()).ToArray(),
            ["Series"] = ["Complete", "InComplete"],
            ["Favorite"] = ["True", "False"],
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly string[] NumericOperators = ["==", ">=", "<=", ">", "<"];
    private static readonly string[] EqualityOnlyOperator = ["=="];

    [Reactive] public partial string SelectedField { get; set; }
    [Reactive] public partial string SelectedOperator { get; set; }
    [Reactive] public partial string Value { get; set; }
    [Reactive] public partial bool IsConnectorOr { get; set; }
    [Reactive] public partial bool ShowConnector { get; set; }

    // Computed from SelectedField
    [Reactive] public partial string[] AvailableOperators { get; private set; }
    [Reactive] public partial string[] AvailableValues { get; private set; }
    [Reactive] public partial bool IsValueFreeText { get; private set; }

    public FilterChipViewModel(string field = "Rating", string op = ">=", string value = "0")
    {
        SelectedField = field;
        SelectedOperator = op;
        Value = value;
        ShowConnector = true;

        UpdateFieldDependencies(field);

        this.WhenAnyValue(x => x.SelectedField)
            .DistinctUntilChanged()
            .Subscribe(UpdateFieldDependencies);
    }

    private void UpdateFieldDependencies(string field)
    {
        bool isNumeric = NumericFields.Contains(field);
        AvailableOperators = isNumeric ? NumericOperators : EqualityOnlyOperator;
        IsValueFreeText = isNumeric || field.Equals("Notes", StringComparison.OrdinalIgnoreCase) || field.Equals("Publisher", StringComparison.OrdinalIgnoreCase);
        AvailableValues = EnumValues.TryGetValue(field, out string[]? vals) ? vals : [];

        // Reset operator to == if current isn't available
        if (!AvailableOperators.Contains(SelectedOperator))
        {
            SelectedOperator = "==";
        }

        // Reset value if switching between numeric/enum
        if (!IsValueFreeText && AvailableValues.Length > 0 && !AvailableValues.Contains(Value))
        {
            Value = AvailableValues[0];
        }
        else if (IsValueFreeText && !isNumeric)
        {
            Value = string.Empty;
        }
    }

    public void ToggleConnector()
    {
        IsConnectorOr = !IsConnectorOr;
    }

    public string ToQuerySegment()
    {
        string val = Value;
        if (IsValueFreeText && !NumericFields.Contains(SelectedField) && val.Contains(' '))
        {
            val = $"\"{val}\"";
        }
        return $"{SelectedField}{SelectedOperator}{val}";
    }
}
