using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class MainWindow : Window
    {
        public string curTitle;
        private TextBlock currentTitle;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void AddVolume(object sender, RoutedEventArgs args)
        {
            /*WrapPanel test = (WrapPanel)SeriesCollection;
            foreach (var series in test.Children)
            {
                Debug.WriteLine(series);
                /*if (series.EnglishTitle.Equals(curTitle) || series.RomajiTitle.Equals(curTitle) || series.NativeTitle.Equals(curTitle))
                {
                    if (series.CurVolumeCount < series.MaxVolumeCount)
                    {
                        series.CurVolumeCount += 1;
                        MainWindowViewModel.MainUser.NumVolumesCollected += 1;
                        MainWindowViewModel.MainUser.NumVolumesToBeCollected -= 1;
                    }
                    break;
                }
            }*/
        }
        
        public void SubtractVolume(object sender, RoutedEventArgs args)
        {
            Debug.WriteLine("Removing 1 Volumes");
            /*foreach (Series series in MainWindowViewModel.Collection)
            {
               if (series.EnglishTitle.Equals(curTitle) || series.RomajiTitle.Equals(curTitle) || series.NativeTitle.Equals(curTitle))
                {
                    Debug.WriteLine("Found Series... Making Changes");
                    if (series.CurVolumeCount > 0)
                    {
                        series.CurVolumeCount -= 1;
                        MainWindowViewModel.MainUser.NumVolumesCollected -= 1;
                        MainWindowViewModel.MainUser.NumVolumesToBeCollected += 1;
                    }
                    MainWindowViewModel.UpdateCollectionNumbers(series.MaxVolumeCount, series.CurVolumeCount);
                    MainWindowViewModel.SaveUsersData();
                    break;
                }
            }*/
        }
    }
}
