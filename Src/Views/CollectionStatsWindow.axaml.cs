using System.Drawing;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class CollectionStatsWindow : Window
    {
        public CollectionStatsViewModel? CollectionStatsVM => DataContext as CollectionStatsViewModel;
        public bool IsOpen = false;
        public bool CanUpdate = true; // On First Update
        MainWindow CollectionWindow;

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
            ShounenObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));

            PieSeries<ObservableValue> SeinenObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[1];
            SeinenObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.SearchBarBGColor));
            SeinenObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));

            PieSeries<ObservableValue> ShoujoObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[2];
            ShoujoObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.UsernameColor));
            ShoujoObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));

            PieSeries<ObservableValue> JoseiObject = (PieSeries<ObservableValue>)CollectionStatsVM.Demographics[3];
            JoseiObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));
            JoseiObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));

            PieSeries<ObservableValue> OngoingObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[0];
            OngoingObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuBGColor));
            OngoingObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));
            
            PieSeries<ObservableValue> FinishedObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[1];
            FinishedObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.SearchBarBGColor));
            FinishedObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));

            PieSeries<ObservableValue> CancelledObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[2];
            CancelledObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.UsernameColor));
            CancelledObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));

            PieSeries<ObservableValue> HiatusObject = (PieSeries<ObservableValue>)CollectionStatsVM.StatusDistribution[3];
            HiatusObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));
            HiatusObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));

            ColumnSeries<ObservableValue> BarObject = (ColumnSeries<ObservableValue>)CollectionStatsVM.ScoreDistribution[0];
            BarObject.Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuBGColor));
            BarObject.DataLabelsPaint =new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.UsernameColor));
            BarObject.Stroke = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));

            Axis XAxisObject = CollectionStatsVM.ScoreXAxes[0];
            XAxisObject.LabelsPaint = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor));
            XAxisObject.TicksPaint = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuBGColor));

            CollectionStatsVM.ScoreYAxes[0].SeparatorsPaint = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.DividerColor));

            Constants.Logger.Info("Updated Chart Colors ");
        }

        private async void CopyTextAsync(object sender, PointerPressedEventArgs args)
        {
            string curText = (sender as Controls.ValueStat).Text;
            Constants.Logger.Info($"Copying {curText} to Clipboard");
            await TextCopy.ClipboardService.SetTextAsync(curText);
        }
    }
}
