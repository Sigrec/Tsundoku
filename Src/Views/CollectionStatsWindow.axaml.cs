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
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public bool IsOpen = false;
        public bool CanUpdate = true; // On First Update
        Subject<SolidColorBrush> UnknownRectangleColorSource = new Subject<SolidColorBrush>();
        Subject<SolidColorBrush> ManfraRectangleColorSource = new Subject<SolidColorBrush>();
        Subject<SolidColorBrush> ComicRectangleColorSource = new Subject<SolidColorBrush>();
        Subject<SolidColorBrush> DataBGColorSource = new Subject<SolidColorBrush>();

        public CollectionStatsWindow(CollectionStatsViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

            Opened += (s, e) =>
            {
                // _ = UnknownRectangle.Bind(Avalonia.Controls.Shapes.Shape.FillProperty, UnknownRectangleColorSource);
                // UnknownRectangleColorSource.OnNext(SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesCardDescColor == ViewModel.CurrentTheme.MenuTextColor ? ViewModel.CurrentTheme.SeriesCardTitleColor.ToString() : ViewModel.CurrentTheme.SeriesCardDescColor.ToString()));

                // _ = ComicRectangle.Bind(Avalonia.Controls.Shapes.Shape.FillProperty, ComicRectangleColorSource);
                // ComicRectangleColorSource.OnNext(SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesCardDescColor == ViewModel.CurrentTheme.MenuTextColor  ? ViewModel.CurrentTheme.SeriesCardTitleColor.ToString() : ViewModel.CurrentTheme.SeriesCardDescColor.ToString()));

                // _ = ManfraRectangle.Bind(Avalonia.Controls.Shapes.Shape.FillProperty, ManfraRectangleColorSource);
                // ManfraRectangleColorSource.OnNext(SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesCardStaffColor == ViewModel.CurrentTheme.SeriesCardTitleColor  ? ViewModel.CurrentTheme.SeriesEditPaneButtonsIconColor.ToString() : ViewModel.CurrentTheme.SeriesCardStaffColor.ToString()));

                // _ = StatsBG.Bind(Avalonia.Controls.Border.BackgroundProperty, DataBGColorSource);
                // DataBGColorSource.OnNext(SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesCardBGColor == ViewModel.CurrentTheme.MenuBGColor ? ViewModel.CurrentTheme.MenuButtonBGColor.ToString() : ViewModel.CurrentTheme.SeriesCardBGColor.ToString()));

                // _ = DistBGOne.Bind(Avalonia.Controls.Border.BackgroundProperty, DataBGColorSource);
                // DataBGColorSource.OnNext(SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesCardBGColor == ViewModel.CurrentTheme.MenuBGColor ? ViewModel.CurrentTheme.MenuButtonBGColor.ToString() : ViewModel.CurrentTheme.SeriesCardBGColor.ToString()));

                // _ = DistBGTwo.Bind(Avalonia.Controls.Border.BackgroundProperty, DataBGColorSource);
                // DataBGColorSource.OnNext(SolidColorBrush.Parse(ViewModel.CurrentTheme.SeriesCardBGColor == ViewModel.CurrentTheme.MenuBGColor ? ViewModel.CurrentTheme.MenuButtonBGColor.ToString() : ViewModel.CurrentTheme.SeriesCardBGColor.ToString()));

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
                    this.Hide();
                    IsOpen ^= true;
                    Topmost = false;
                }
                e.Cancel = true;
            };
        }

        private async void CopyTextAsync(object sender, PointerPressedEventArgs args)
        {
            string curText = $"{(sender as Controls.ValueStat).Text} {(sender as Controls.ValueStat).Title}";
            LOGGER.Info($"Copying {curText} to Clipboard");
            await TextCopy.ClipboardService.SetTextAsync($"{curText}");
        }
    }
}
