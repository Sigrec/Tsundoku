using System.Collections.ObjectModel;
using LiveChartsCore;
using Avalonia.Controls.Primitives;

namespace Tsundoku.Controls;

public sealed class TsundokuPieChart : TemplatedControl
{
    public static readonly StyledProperty<string> ChartTitleProperty = AvaloniaProperty.Register<TsundokuPieChart, string>(nameof(ChartTitle), "Pie Chart Title");

    public string ChartTitle
    {
        get => GetValue(ChartTitleProperty);
        set => SetValue(ChartTitleProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<ISeries>> ChartDataProperty = AvaloniaProperty.Register<TsundokuPieChart, ObservableCollection<ISeries>>(nameof(ChartData));

    public ObservableCollection<ISeries> ChartData
    {
        get => GetValue(ChartDataProperty);
        set => SetValue(ChartDataProperty, value);
    }
}
