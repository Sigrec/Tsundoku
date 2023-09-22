using Avalonia.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Tsundoku.ViewModels;
using Avalonia.ReactiveUI;
using Avalonia.Platform;
using System.Drawing;
using Avalonia.Logging;

namespace Tsundoku.Views
{
    public partial class CollectionStatsWindow : ReactiveWindow<CollectionStatsViewModel>
    {
        public CollectionStatsViewModel? CollectionStatsVM => DataContext as CollectionStatsViewModel;
        public bool IsOpen = false;
        public bool CanUpdate = true; // On First Update
        MainWindow CollectionWindow;

        // TODO Clicking stats isn't copying
        public CollectionStatsWindow()
        {
            InitializeComponent();
            DataContext = new CollectionStatsViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                CollectionStatsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                if (CanUpdate) { UpdateChartColors(); }
                CanUpdate = false;
                IsOpen ^= true;

                if (Screens.Primary.WorkingArea.Height < 1010)
                {
                    this.Height = 550;
                }
            };

            Closing += (s, e) =>
            {
                ((CollectionStatsWindow)s).Hide();
                IsOpen ^= true;
                Topmost = false;
                e.Cancel = true;
            };
        }

        public void UpdateChartColors()
        {
            PieSeries<ObservableValue> ShounenObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[0];
            ShounenObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuBGColor));
            ShounenObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> SeinenObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[1];
            SeinenObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuButtonBGColor));
            SeinenObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> ShoujoObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[2];
            ShoujoObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));
            ShoujoObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> JoseiObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[3];
            JoseiObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));
            JoseiObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> OngoingObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[0];
            OngoingObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuBGColor));
            OngoingObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));
            
            PieSeries<ObservableValue> FinishedObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[1];
            FinishedObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuButtonBGColor));
            FinishedObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> CancelledObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[2];
            CancelledObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));
            CancelledObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            PieSeries<ObservableValue> HiatusObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[3];
            HiatusObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));
            HiatusObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            Color behindBarColor = Color.FromArgb((int)CollectionStatsVM.CurrentTheme.MenuButtonBGColor);
            ColumnSeries<ObservableValue> BarBehindObject = (ColumnSeries<ObservableValue>)CollectionStatsVM.ScoreDistribution[0];
            BarBehindObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(behindBarColor.R, behindBarColor.G, behindBarColor.B, 120));

            ColumnSeries<ObservableValue> BarObject = (ColumnSeries<ObservableValue>)CollectionStatsVM.ScoreDistribution[1];
            BarObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuBGColor));
            BarObject.DataLabelsPaint = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));
            BarObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            Axis XAxisObject = CollectionStatsVM.ScoreXAxes[0];
            XAxisObject.LabelsPaint = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));
            XAxisObject.TicksPaint = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));

            CollectionStatsVM.ScoreYAxes[0].SeparatorsPaint = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            LOGGER.Info("Updated Chart Colors ");
        }

        private async void CopyTextAsync(object sender, PointerPressedEventArgs args)
        {
            string curText = $"{(sender as Controls.ValueStat).Title} {(sender as Controls.ValueStat).Text}";
            LOGGER.Info($"Copying {curText} to Clipboard");
            await TextCopy.ClipboardService.SetTextAsync($"{curText}");
        }
    }
}
