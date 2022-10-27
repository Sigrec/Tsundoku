using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Debug.WriteLine(SeriesCollection.GetLogicalChildren().ToString());
        }

        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            switch (text)
            {
                case "Native":
                    MainWindowViewModel._curLanguage = "Native";
                    break;
                case "English":
                    MainWindowViewModel._curLanguage = "English";
                    break;
                default:
                    MainWindowViewModel._curLanguage = "Romaji";
                    break;
            }
            Debug.WriteLine("Current Lang = " + MainWindowViewModel._curLanguage);
        }

        private void DisplayChanged(object sender, SelectionChangedEventArgs e)
        {
            string display = (sender as ComboBox).SelectedItem as string;
            if (display.Equals("Card"))
            {
                MainWindowViewModel._curDisplay = "Card";
            }
            else if (display.Equals("Mini-Card"))
            {
                MainWindowViewModel._curDisplay = "Mini-Card";
            }
            Debug.WriteLine("Current Display = " + MainWindowViewModel._curDisplay);
        }

        private void SaveOnClose(object sender, CancelEventArgs e)
        {
            MainWindowViewModel.SaveUsersData();
        }

        private void OpenSiteLink(object sender, PointerPressedEventArgs args)
        {
            string link = ((Canvas)sender).Name;
            Debug.WriteLine("Opening Site " + link);
            try
            {
                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch (Win32Exception noBrowser)
            {
                Debug.WriteLine(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Debug.WriteLine(other.Message);
            }
        }

        private async void CopySeriesTitleAsync(object sender, PointerPressedEventArgs args)
        {
            string title = ((TextBlock)sender).Text;
            Debug.WriteLine("Copying " + title + " to Clipboard");
            await Application.Current.Clipboard.SetTextAsync(title);
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
            Series curSeries = ((Series)((Button)sender).DataContext);
            Debug.WriteLine("Removing 1 Volume from " + curSeries.Titles[0]);
            curSeries.CurVolumeCount -= 1;
            MainWindowViewModel._numCollected -= 1;
            MainWindowViewModel.MainUser.NumVolumesToBeCollected += 1;
        }
    }
}
