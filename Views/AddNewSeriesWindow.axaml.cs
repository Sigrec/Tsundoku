using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using Tsundoku.ViewModels;
using System;
using Avalonia.Input;
using MessageBox.Avalonia.DTO;
using Tsundoku.Models;

namespace Tsundoku.Views
{
    public partial class AddNewSeriesWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public AddNewSeriesWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
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
                AddNewSeriesViewModel.GetSeriesData(TitleBox.Text, (bool)MangaButton.IsChecked ? "MANGA" : "NOVEL", Convert.ToUInt16(CurVolCount.Text), Convert.ToUInt16(MaxVolCount.Text));
            }
        }
    }
}
