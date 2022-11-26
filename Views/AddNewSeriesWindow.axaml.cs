using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using System;
using MessageBox.Avalonia.DTO;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.Views
{
    public partial class AddNewSeriesWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private MainWindow CollectionWindow => DataContext as MainWindow;

        public AddNewSeriesWindow()
        {
            InitializeComponent();
            this.Closing += (s, e) =>
            {
                ((AddNewSeriesWindow)s).Hide();
                e.Cancel = true;
            };
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void MangaCheck(object sender, RoutedEventArgs args)
        {
            NovelButton.IsChecked = false;
        }

        private void NovelCheck(object sender, RoutedEventArgs args)
        {
            MangaButton.IsChecked = false;
        }

        public void OnButtonClicked(object sender, RoutedEventArgs args)
        {
            if (string.IsNullOrEmpty(TitleBox.Text) || (!(bool)MangaButton.IsChecked && !(bool)NovelButton.IsChecked) || string.IsNullOrEmpty(MaxVolCount.Text) || string.IsNullOrEmpty(CurVolCount.Text))
            {
                Logger.Warn("Fields Missing Input");
                var errorBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams
                {
                    ContentTitle = "Error!",
                    ContentMessage = "One or More Fields are Empty... Please Enter Data For All Fields",
                    WindowIcon = new WindowIcon(@"\Tsundoku\Assets\tsundoku-logo.ico")
                });
                errorBox.Show();
            }
            else
            {
                AddNewSeriesViewModel.GetSeriesData(TitleBox.Text, (bool)MangaButton.IsChecked ? "MANGA" : "NOVEL", Convert.ToUInt16(CurVolCount.Text.Replace("_", "")), Convert.ToUInt16(MaxVolCount.Text.Replace("_", "")));
            }
        }
    }
}
