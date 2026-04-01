using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Tsundoku.ViewModels;

public sealed partial class FilterBuilderViewModel : ReactiveObject, IDisposable
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly CompositeDisposable _disposables = [];

    public ObservableCollection<FilterChipViewModel> Chips { get; } = [];

    [Reactive] public partial string SynthesizedQuery { get; private set; }

    public FilterBuilderViewModel()
    {
        SynthesizedQuery = string.Empty;

        // Observe any change to the Chips collection or any chip's properties
        Chips.ToObservableChangeSet()
            .AutoRefresh(c => c.SelectedField)
            .AutoRefresh(c => c.SelectedOperator)
            .AutoRefresh(c => c.Value)
            .AutoRefresh(c => c.IsConnectorOr)
            .Throttle(TimeSpan.FromMilliseconds(150))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => RebuildQuery())
            .DisposeWith(_disposables);
    }

    public void AddChip()
    {
        FilterChipViewModel chip = new();
        Chips.Add(chip);
        UpdateConnectorVisibility();
    }

    public void RemoveChip(FilterChipViewModel chip)
    {
        Chips.Remove(chip);
        UpdateConnectorVisibility();

        if (Chips.Count == 0)
        {
            SynthesizedQuery = string.Empty;
        }
    }

    public void ClearAll()
    {
        Chips.Clear();
        SynthesizedQuery = string.Empty;
    }

    private void UpdateConnectorVisibility()
    {
        for (int i = 0; i < Chips.Count; i++)
        {
            Chips[i].ShowConnector = i < Chips.Count - 1;
        }
    }

    private const string AndConnector = " & ";
    private const string OrConnector = " | ";

    private void RebuildQuery()
    {
        if (Chips.Count == 0)
        {
            SynthesizedQuery = string.Empty;
            return;
        }

        StringBuilder sb = new();
        for (int i = 0; i < Chips.Count; i++)
        {
            FilterChipViewModel chip = Chips[i];
            if (string.IsNullOrWhiteSpace(chip.Value) && chip.IsValueFreeText)
            {
                continue; // Skip chips with empty free-text values
            }

            if (sb.Length > 0 && i > 0)
            {
                sb.Append(Chips[i - 1].IsConnectorOr ? OrConnector : AndConnector);
            }

            sb.Append(chip.ToQuerySegment());
        }

        string query = sb.ToString();
        if (!query.Equals(SynthesizedQuery, StringComparison.Ordinal))
        {
            SynthesizedQuery = query;
            LOGGER.Debug("Filter query synthesized: {Query}", query);
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
