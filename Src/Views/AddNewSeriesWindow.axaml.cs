using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class AddNewSeriesWindow : Window
    {
        public AddNewSeriesViewModel? AddNewSeriesVM => DataContext as AddNewSeriesViewModel;
        public bool IsOpen = false;
        MainWindow CollectionWindow;

        public AddNewSeriesWindow()
        {
            InitializeComponent();
            DataContext = new AddNewSeriesViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                AddNewSeriesVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                IsOpen ^= true;
            };

            Closing += (s, e) =>
            {
                ((AddNewSeriesWindow)s).Hide();
                NovelButton.IsChecked = false;
                MangaButton.IsChecked = false;
                TitleBox.Text = String.Empty;
                CurVolCount.Text = String.Empty;
                MaxVolCount.Text = String.Empty;
                IsOpen ^= true;
                Topmost = false;
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
            foreach (var x in AddNewSeriesVM.SelectedAdditionalLanguages)
            {
                Constants.Logger.Debug(x);
            }
            bool validResponse = true;
            string errorMessage = "";
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                errorMessage += "Title Field is Empty\n";
                validResponse = false;
            }

            if ((!MangaButton.IsChecked & !NovelButton.IsChecked) == true)
            {
                errorMessage += "Series Book Type (Manga or Novel) Not Checked\n";
                validResponse = false;
            }

            if (string.IsNullOrWhiteSpace(CurVolCount.Text.Replace("_", "")))
            {
                errorMessage += "Series Current Volume Count is Empty\n";
                validResponse = false;
            }

            if (string.IsNullOrWhiteSpace(MaxVolCount.Text.Replace("_", "")))
            {
                errorMessage += "Series Current Max Volume Count is Empty\n";
                validResponse = false;
            }

            ushort cur = 0;
            ushort max = 0;
            if (!ushort.TryParse(CurVolCount.Text.Replace("_", ""), out cur))
            {
                errorMessage += "Current Volume Count Inputted is not a Number\n";
                validResponse = false;
            }

            if (!ushort.TryParse(MaxVolCount.Text.Replace("_", ""), out max))
            {
                errorMessage += "Max Volumes Count Inpuuted is not a Number\n";
                validResponse = false;
            }

            if (cur > max)
            {
                errorMessage += "Current Volume Count is Greater than the Max Volume Count\n";
                validResponse = false;
            }

            if (!validResponse)
            {
                Constants.Logger.Warn("User Input to Add New Series is Invalid");
                var errorBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBox.Avalonia.DTO.MessageBoxStandardParams
                {
                    ContentTitle = "Error Adding Series",
                    ContentMessage = errorMessage
                });
                errorBox.Show();
            }
            else if (!AddNewSeriesVM.GetSeriesData(TitleBox.Text.Trim(), (MangaButton.IsChecked == true) ? "MANGA" : "NOVEL", cur, max, AddNewSeriesVM.SelectedAdditionalLanguages)) // Boolean returns whether the series added is a duplicate
            {
                CollectionWindow.CollectionViewModel.UsersNumVolumesCollected += cur;
                CollectionWindow.CollectionViewModel.UsersNumVolumesToBeCollected += (uint)(max - cur);
                CollectionWindow.CollectionViewModel.SearchText = "";
                CollectionWindow.CollectionViewModel.collectionStatsWindow.CollectionStatsVM.SeriesCount = (uint)MainWindowViewModel.Collection.Count;
            }
        }
    }
}
