using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class AddNewSeriesWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public AddNewSeriesViewModel? AddNewSeriesVM => DataContext as AddNewSeriesViewModel;
        public bool IsOpen = false;

        public AddNewSeriesWindow()
        {
            InitializeComponent();
            DataContext = new AddNewSeriesViewModel();
            Opened += (s, e) =>
            {
                AddNewSeriesVM.CurrentTheme = ((MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow).CollectionViewModel.CurrentTheme;
                IsOpen ^= true;
            };
            
            Closing += (s, e) =>
            {
                ((AddNewSeriesWindow)s).Hide();
                IsOpen ^= true;
                e.Cancel = true;
            };
// #if DEBUG
//             this.AttachDevTools();
// #endif
        }

        private void IsMangaButtonClicked(object sender, RoutedEventArgs args)
        {
            NovelButton.IsChecked = false;
        }


        private void IsNovelButtonClicked(object sender, RoutedEventArgs args)
        {
            MangaButton.IsChecked = false;
        }

        public void OnButtonClicked(object sender, RoutedEventArgs args)
        {
            ushort cur = 0;
            ushort max = 0;
            if (string.IsNullOrWhiteSpace(TitleBox.Text) || (!(bool)MangaButton.IsChecked && !(bool)NovelButton.IsChecked) || string.IsNullOrWhiteSpace(CurVolCount.Text.Replace("_", "")) || string.IsNullOrWhiteSpace(MaxVolCount.Text.Replace("_", "")) || !ushort.TryParse(CurVolCount.Text.Replace("_", ""), out cur) || !ushort.TryParse(MaxVolCount.Text.Replace("_", ""), out max) || cur > max)
            {
                Logger.Warn("Fields Missing Input");
                // var errorBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                // new MessageBoxStandardParams
                // {
                //     ContentTitle = "Error!",
                //     ContentMessage = "One or More Fields are Empty or CurVolumes > MaxVolumes\nPlease Enter Data For All Fields",
                //     WindowIcon = new WindowIcon(@"Assets\Icons\Tsundoku-Logo.ico")
                // });
                // errorBox.Show();
            }
            else
            {
                AddNewSeriesViewModel.GetSeriesData(TitleBox.Text, (bool)MangaButton.IsChecked ? "MANGA" : "NOVEL", cur, max);
            }
        }
    }
}
