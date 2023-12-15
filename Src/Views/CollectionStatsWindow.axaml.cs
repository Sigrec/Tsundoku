using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Tsundoku.ViewModels;
using Avalonia.ReactiveUI;
using System.Reactive.Subjects;
using Avalonia.Media;
using SkiaSharp;

namespace Tsundoku.Views
{
    public partial class CollectionStatsWindow : ReactiveWindow<CollectionStatsViewModel>
    {
        public CollectionStatsViewModel? CollectionStatsVM => DataContext as CollectionStatsViewModel;
        public bool IsOpen = false;
        public bool CanUpdate = true; // On First Update
        Subject<SolidColorBrush> UnknownRectangleColorSource = new Subject<SolidColorBrush>();

        public CollectionStatsWindow()
        {
            InitializeComponent();
            DataContext = new CollectionStatsViewModel();
            Opened += (s, e) =>
            {
                var sub = UnknownRectangle.Bind(Avalonia.Controls.Shapes.Rectangle.FillProperty, UnknownRectangleColorSource);
                if (CanUpdate) { UpdateChartColors(); }
                CanUpdate = false;
                IsOpen ^= true;

                if (Screens.Primary.WorkingArea.Height < 1025)
                {
                    this.Height = 550;
                }
            };

            Closing += (s, e) =>
            {
                if (IsOpen)
                {
                    ((CollectionStatsWindow)s).Hide();
                    IsOpen ^= true;
                    Topmost = false;
                }
                e.Cancel = true;
            };
        }

        public void UpdateChartColors()
        {
            PieSeries<ObservableValue> ShounenObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[0];
            ShounenObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuBGColor));
            ShounenObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> SeinenObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[1];
            SeinenObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuButtonBGColor));
            SeinenObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> ShoujoObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[2];
            ShoujoObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuTextColor));
            ShoujoObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> JoseiObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[3];
            JoseiObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));
            JoseiObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> UnknownObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[4];
            UnknownObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.SeriesCardDescColor == CollectionStatsVM.CurrentTheme.MenuTextColor ? CollectionStatsVM.CurrentTheme.SeriesCardTitleColor : CollectionStatsVM.CurrentTheme.SeriesCardDescColor));
            UnknownRectangleColorSource.OnNext(SolidColorBrush.Parse(CollectionStatsVM.CurrentTheme.SeriesCardDescColor == CollectionStatsVM.CurrentTheme.MenuTextColor ? CollectionStatsVM.CurrentTheme.SeriesCardTitleColor : CollectionStatsVM.CurrentTheme.SeriesCardDescColor));
            UnknownObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> OngoingObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[0];
            OngoingObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuBGColor));
            OngoingObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));
            
            PieSeries<ObservableValue> FinishedObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[1];
            FinishedObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuButtonBGColor));
            FinishedObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> CancelledObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[2];
            CancelledObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));
            CancelledObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> HiatusObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[3];
            HiatusObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuTextColor));
            HiatusObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));

            // Color behindBarColor = Color.Parse(CollectionStatsVM.CurrentTheme.MenuButtonBGColor);
            ColumnSeries<ObservableValue> BarBehindObject = (ColumnSeries<ObservableValue>)CollectionStatsVM.RatingDistribution[0];
            BarBehindObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuButtonBGColor));

            ColumnSeries<ObservableValue> BarObject = (ColumnSeries<ObservableValue>)CollectionStatsVM.RatingDistribution[1];
            BarObject.Fill = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuBGColor));
            BarObject.DataLabelsPaint = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuTextColor));
            BarObject.Stroke = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));

            Axis XAxisObject = CollectionStatsVM.RatingXAxes[0];
            XAxisObject.LabelsPaint = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuTextColor));
            XAxisObject.TicksPaint = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.MenuTextColor));

            CollectionStatsVM.RatingYAxes[0].SeparatorsPaint = new SolidColorPaint(SKColor.Parse(CollectionStatsVM.CurrentTheme.DividerColor));
        }

        private async void CopyTextAsync(object sender, PointerPressedEventArgs args)
        {
            string curText = $"{(sender as Controls.ValueStat).Text} {(sender as Controls.ValueStat).Title}";
            LOGGER.Info($"Copying {curText} to Clipboard");
            await TextCopy.ClipboardService.SetTextAsync($"{curText}");
        }
    }
}
