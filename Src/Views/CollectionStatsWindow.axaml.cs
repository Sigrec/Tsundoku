using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Tsundoku.ViewModels;
using Avalonia.ReactiveUI;
using System.Reactive.Subjects;
using Avalonia.Media;
using SkiaSharp;
using Avalonia.Controls.ApplicationLifetimes;

namespace Tsundoku.Views
{
    public partial class CollectionStatsWindow : ReactiveWindow<CollectionStatsViewModel>
    {
        public bool IsOpen = false;
        public bool CanUpdate = true; // On First Update
        private MainWindow CollectionWindow;
        Subject<SolidColorBrush> UnknownRectangleColorSource = new Subject<SolidColorBrush>();
        Subject<SolidColorBrush> ManfraRectangleColorSource = new Subject<SolidColorBrush>();
        Subject<SolidColorBrush> ComicRectangleColorSource = new Subject<SolidColorBrush>();
        Subject<SolidColorBrush> DataBGColorSource = new Subject<SolidColorBrush>();

        public CollectionStatsWindow()
        {
            InitializeComponent();
            DataContext = new CollectionStatsViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                
                _ = UnknownRectangle.Bind(Avalonia.Controls.Shapes.Shape.FillProperty, UnknownRectangleColorSource);
                UnknownRectangleColorSource.OnNext(SolidColorBrush.Parse(ViewModelBase.CurrentTheme.SeriesCardDescColor == ViewModelBase.CurrentTheme.MenuTextColor ? ViewModelBase.CurrentTheme.SeriesCardTitleColor.ToString() : ViewModelBase.CurrentTheme.SeriesCardDescColor.ToString()));

                _ = ComicRectangle.Bind(Avalonia.Controls.Shapes.Shape.FillProperty, ComicRectangleColorSource);
                ComicRectangleColorSource.OnNext(SolidColorBrush.Parse(ViewModelBase.CurrentTheme.SeriesCardDescColor == ViewModelBase.CurrentTheme.MenuTextColor  ? ViewModelBase.CurrentTheme.SeriesCardTitleColor.ToString() : ViewModelBase.CurrentTheme.SeriesCardDescColor.ToString()));

                _ = ManfraRectangle.Bind(Avalonia.Controls.Shapes.Shape.FillProperty, ManfraRectangleColorSource);
                ManfraRectangleColorSource.OnNext(SolidColorBrush.Parse(ViewModelBase.CurrentTheme.SeriesCardStaffColor == ViewModelBase.CurrentTheme.SeriesCardTitleColor  ? ViewModelBase.CurrentTheme.SeriesEditPaneButtonsIconColor.ToString() : ViewModelBase.CurrentTheme.SeriesCardStaffColor.ToString()));

                _ = StatsBG.Bind(Avalonia.Controls.Border.BackgroundProperty, DataBGColorSource);
                DataBGColorSource.OnNext(SolidColorBrush.Parse(ViewModelBase.CurrentTheme.SeriesCardBGColor == ViewModelBase.CurrentTheme.MenuBGColor ? ViewModelBase.CurrentTheme.MenuButtonBGColor.ToString() : ViewModelBase.CurrentTheme.SeriesCardBGColor.ToString()));

                _ = DistBGOne.Bind(Avalonia.Controls.Border.BackgroundProperty, DataBGColorSource);
                DataBGColorSource.OnNext(SolidColorBrush.Parse(ViewModelBase.CurrentTheme.SeriesCardBGColor == ViewModelBase.CurrentTheme.MenuBGColor ? ViewModelBase.CurrentTheme.MenuButtonBGColor.ToString() : ViewModelBase.CurrentTheme.SeriesCardBGColor.ToString()));

                _ = DistBGTwo.Bind(Avalonia.Controls.Border.BackgroundProperty, DataBGColorSource);
                DataBGColorSource.OnNext(SolidColorBrush.Parse(ViewModelBase.CurrentTheme.SeriesCardBGColor == ViewModelBase.CurrentTheme.MenuBGColor ? ViewModelBase.CurrentTheme.MenuButtonBGColor.ToString() : ViewModelBase.CurrentTheme.SeriesCardBGColor.ToString()));

                if (CanUpdate) { UpdateChartColors(); }
                CanUpdate = false;
                IsOpen ^= true;

                if (Screens.Primary.WorkingArea.Height < 1250)
                {
                    this.Height = 550;
                }
            };

            Closing += (s, e) =>
            {
                if (IsOpen)
                {
                    MainWindow.ResetMenuButton(CollectionWindow.StatsButton);
                    ((CollectionStatsWindow)s).Hide();
                    IsOpen ^= true;
                    Topmost = false;
                }
                e.Cancel = true;
            };
        }

        public void UpdateChartColors()
        {
            PieSeries<ObservableValue> ShounenObject = (PieSeries<ObservableValue>)ViewModel.Demographics[0];
            ShounenObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuBGColor.ToString()));
            ShounenObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> SeinenObject = (PieSeries<ObservableValue>)ViewModel.Demographics[1];
            SeinenObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuButtonBGColor.ToString()));
            SeinenObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> ShoujoObject = (PieSeries<ObservableValue>)ViewModel.Demographics[2];
            ShoujoObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            ShoujoObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> JoseiObject = (PieSeries<ObservableValue>)ViewModel.Demographics[3];
            JoseiObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));
            JoseiObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> UnknownObject = (PieSeries<ObservableValue>)ViewModel.Demographics[4];
            UnknownObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.SeriesCardDescColor == ViewModelBase.CurrentTheme.MenuTextColor ? ViewModelBase.CurrentTheme.SeriesCardTitleColor.ToString() : ViewModelBase.CurrentTheme.SeriesCardDescColor.ToString()));
            UnknownObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> OngoingObject = (PieSeries<ObservableValue>)ViewModel.StatusDistribution[0];
            OngoingObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuBGColor.ToString()));;
            OngoingObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));
            
            PieSeries<ObservableValue> FinishedObject = (PieSeries<ObservableValue>)ViewModel.StatusDistribution[1];
            FinishedObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuButtonBGColor.ToString()));
            FinishedObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> CancelledObject = (PieSeries<ObservableValue>)ViewModel.StatusDistribution[2];
            CancelledObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));
            CancelledObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> HiatusObject = (PieSeries<ObservableValue>)ViewModel.StatusDistribution[3];
            HiatusObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            HiatusObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            // Format pie chart themeing
            PieSeries<ObservableValue> MangaObject = (PieSeries<ObservableValue>)ViewModel.Formats[0];
            MangaObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuBGColor.ToString()));;
            MangaObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> ManhwaObject = (PieSeries<ObservableValue>)ViewModel.Formats[1];
            ManhwaObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuButtonBGColor.ToString()));
            ManhwaObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> Manhuabject = (PieSeries<ObservableValue>)ViewModel.Formats[2];
            Manhuabject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            Manhuabject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> NovelObject = (PieSeries<ObservableValue>)ViewModel.Formats[3];
            NovelObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));
            NovelObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> ComicObject = (PieSeries<ObservableValue>)ViewModel.Formats[4];
            ComicObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.SeriesCardDescColor == ViewModelBase.CurrentTheme.MenuTextColor ? ViewModelBase.CurrentTheme.SeriesCardTitleColor.ToString() : ViewModelBase.CurrentTheme.SeriesCardDescColor.ToString()));
            ComicObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            PieSeries<ObservableValue> ManfraObject = (PieSeries<ObservableValue>)ViewModel.Formats[5];
            ManfraObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.SeriesCardDescColor == ViewModelBase.CurrentTheme.MenuTextColor ? ViewModelBase.CurrentTheme.SeriesCardTitleColor.ToString() : ViewModelBase.CurrentTheme.SeriesCardDescColor.ToString()));
            ManfraObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            // Distribution themeing
            Color behindBarColor = ViewModelBase.CurrentTheme.MenuButtonBGColor.Color;
            SolidColorPaint BehindBarColor = new SolidColorPaint(new SKColor(behindBarColor.R, behindBarColor.G, behindBarColor.B, 120));

            // Rating Bar Chart themeing
            ColumnSeries<ObservableValue> RatingBarBehindObject = (ColumnSeries<ObservableValue>)ViewModel.RatingDistribution[0];
            RatingBarBehindObject.Fill = BehindBarColor;

            ColumnSeries<ObservableValue> RatingBarObject = (ColumnSeries<ObservableValue>)ViewModel.RatingDistribution[1];
            RatingBarObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuBGColor.ToString()));;
            RatingBarObject.DataLabelsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            RatingBarObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            Axis RatingXAxisObject = ViewModel.RatingXAxes[0];
            RatingXAxisObject.LabelsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            RatingXAxisObject.TicksPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));

            ViewModel.RatingYAxes[0].SeparatorsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            // Volume Count Bar Chart themeing
            ColumnSeries<ObservableValue> VolumeCountBarBehindObject = (ColumnSeries<ObservableValue>)ViewModel.VolumeCountDistribution[0];
            VolumeCountBarBehindObject.Fill = BehindBarColor;

            ColumnSeries<ObservableValue> VolumeCountBarObject = (ColumnSeries<ObservableValue>)ViewModel.VolumeCountDistribution[1];
            VolumeCountBarObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuBGColor.ToString()));;
            VolumeCountBarObject.DataLabelsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            VolumeCountBarObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            Axis VolumeCountXAxisObject = ViewModel.VolumeCountXAxes[0];
            VolumeCountXAxisObject.LabelsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            VolumeCountXAxisObject.TicksPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));

            ViewModel.VolumeCountYAxes[0].SeparatorsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            // Genre Bar Chart themeing
            var GenreBarObject = (RowSeries<KeyValuePair<string, int>>)ViewModel.GenreDistribution[0];
            GenreBarObject.Fill = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuBGColor.ToString()));;
            GenreBarObject.DataLabelsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            GenreBarObject.Stroke = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));

            Axis GenreYAxisObject = ViewModel.GenreYAxes[0];
            GenreYAxisObject.LabelsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            GenreYAxisObject.TicksPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));

            Axis GenreXAxisObject = ViewModel.GenreXAxes[0];
            GenreXAxisObject.TicksPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            GenreXAxisObject.LabelsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.MenuTextColor.ToString()));
            GenreXAxisObject.SeparatorsPaint = new SolidColorPaint(SKColor.Parse(ViewModelBase.CurrentTheme.DividerColor.ToString()));
        }

        private async void CopyTextAsync(object sender, PointerPressedEventArgs args)
        {
            string curText = $"{(sender as Controls.ValueStat).Text} {(sender as Controls.ValueStat).Title}";
            LOGGER.Info($"Copying {curText} to Clipboard");
            await TextCopy.ClipboardService.SetTextAsync($"{curText}");
        }
    }
}
