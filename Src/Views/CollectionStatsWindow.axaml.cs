using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using LiveChartsCore;
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
        MainWindow CollectionWindow;
        public CollectionStatsWindow()
        {
            InitializeComponent();
            DataContext = new CollectionStatsViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                CollectionStatsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                IsOpen ^= true;
                // UpdateChartColors();
            };

            Closing += (s, e) =>
            {
                ((CollectionStatsWindow)s).Hide();
                IsOpen ^= true;
                Topmost = false;
                e.Cancel = true;
            };
        }

        private void UpdateChartColors()
        {
            // Constants.Logger.Info("Updating Chart Colors");
            // CollectionStatsVM.CountryDistribution.Add( new PieSeries<double> 
            //     { 
            //         Values = new ObservableCollection<double> { 1 }, 
            //         Name = "Test",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuButtonBGColor))
            //     });
            // CollectionStatsVM.CountryDistribution = new ObservableCollection<ISeries>
            // {
            //     new PieSeries<double> 
            //     { 
            //         Values = new ObservableCollection<double> { 1 }, 
            //         Name = "Japan",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuButtonBGColor))
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new ObservableCollection<double> { 1 }, 
            //         Name = "Korea",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuButtonBorderColor))
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new ObservableCollection<double> { 1 }, 
            //         Name = "America",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuButtonTextAndIconColor))
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new ObservableCollection<double> { 1 }, 
            //         Name = "China",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.CollectionBGColor))
            //     },
            //     new PieSeries<double>
            //     { 
            //         Values = new ObservableCollection<double> { 1 }, 
            //         Name = "France",
            //         Fill = new SolidColorPaint(new SkiaSharp.SKColor(CollectionStatsVM.CurrentTheme.MenuTextColor))
            //     }
            // };
        }

        private async void CopyTextAsync(object sender, PointerPressedEventArgs args)
        {
            string curText = (sender as Controls.ValueStat).Text;
            Constants.Logger.Info($"Copying {curText} to Clipboard");
            await Application.Current.Clipboard.SetTextAsync(curText);
        }
    }
}
