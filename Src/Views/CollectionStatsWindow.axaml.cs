using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
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
            };

            Closing += (s, e) =>
            {
                ((CollectionStatsWindow)s).Hide();
                IsOpen ^= true;
                Topmost = false;
                e.Cancel = true;
            };
        }
    }
}
