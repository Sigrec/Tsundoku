using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string GetTitle(object sender, RoutedEventArgs args)
        {
            return ((TextBlock)sender).Text;
        }

        public void AddVolume(object sender, RoutedEventArgs args)
        {
            foreach (Series series in MainWindowViewModel.Collection)
            {
                if (series.EnglishTitle.Equals(GetTitle))
                {
                    series.CurVolumeCount += 1;
                }
            }
        }

        public void SubtractVolume(object sender, RoutedEventArgs args)
        {
            foreach (Series series in MainWindowViewModel.Collection)
            {
                if (series.EnglishTitle.Equals(GetTitle))
                {
                    series.CurVolumeCount -= 1;
                }
            }
        }
    }
}
