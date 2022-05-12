using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using Tsundoku.ViewModels;
using System;
using Avalonia.Input;
using MessageBox.Avalonia.DTO;

namespace Tsundoku.Views
{
    public partial class AddNewSeriesWindow : Window
    {
        public string titleText;
        public string bookType;
        public string maxVolCount;
        public string curVolCount;

        public AddNewSeriesWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void GetTitleText(object sender, KeyEventArgs args)
        {
            titleText = ((TextBox)sender).Text;
        }

        public void GetMangaBookType(object sender, RoutedEventArgs args)
        {
            if ((bool)((RadioButton)sender).IsChecked)
            {
                bookType = "MANGA";
            }
        }

        public void GetNovelBookType(object sender, RoutedEventArgs args)
        {
            if ((bool)((RadioButton)sender).IsChecked)
            {
                bookType = "NOVEL";
            }
        }

        public void GetMaxVolCount(object sender, KeyEventArgs args)
        {
            maxVolCount = ((TextBox)sender).Text;
        }

        public void GetCurVolCount(object sender, KeyEventArgs args)
        {
            curVolCount = ((TextBox)sender).Text;
        }

        public void OnButtonClicked(object sender, RoutedEventArgs args)
        {
            if (String.IsNullOrEmpty(titleText) || String.IsNullOrEmpty(bookType) || String.IsNullOrEmpty(maxVolCount) || String.IsNullOrEmpty(curVolCount))
            {
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
                AddNewSeriesViewModel.GetSeriesData(titleText, bookType, Convert.ToUInt16(curVolCount), Convert.ToUInt16(maxVolCount));
            }
        }
    }
}
