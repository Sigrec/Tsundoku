using System.Collections.ObjectModel;
using Avalonia;
using LiveChartsCore;
using Avalonia.Controls.Primitives;

namespace Tsundoku.Controls
{
    public class TsundokuPieChart : TemplatedControl
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
}
