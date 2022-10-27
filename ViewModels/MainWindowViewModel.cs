using ReactiveUI;
using Tsundoku.Models;
using System.Collections.ObjectModel;
using System.IO;
using Tsundoku.Views;
using System.Windows.Input;
using System.ComponentModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using Avalonia.Animation;
using DynamicData;
using DynamicData.Binding;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace Tsundoku.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string? _searchText;
        private Series? _selectedSeries;
        private static string filePath = @"\Tsundoku\UserData\Collection.bin";
        public ICommand OpenAddNewSeriesWindow { get; }
        public static Collection<Series> Collection { get; set; } = new();
        public static ObservableCollection<Series> SearchedCollection { get; set; } = new();
        public static User MainUser { get; set; } = new();
        private AddNewSeriesWindow newSeriesWindow;
        public static string? _curLanguage, _curDisplay;
        public static ushort _numCollected;
        public string[] AvailableLanguages { get; } = new string[]
        {
            "Romaji", "English", "Native"
        };
        public string[] AvailableDisplays { get; } = new string[]
        {
            "Card", "Mini-Card"
        };

        // Binding event to track what the current search text is in a users collection
        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        // Binding event to track what the current language is in a users collection
        public string CurLanguage
        {
            get => _curLanguage;
            set => this.RaiseAndSetIfChanged(ref _curLanguage, value);
        }

        public string CurDisplay
        {
            get => _curDisplay;
            set => this.RaiseAndSetIfChanged(ref _curDisplay, value);
        }

        public Series? SelectedSeries
        {
            get => _selectedSeries;
            set => this.RaiseAndSetIfChanged(ref _selectedSeries, value);
        }

        public ushort CurNumVolumesCollected
        {
            get => _numCollected;
            set => this.RaiseAndSetIfChanged(ref _numCollected, value);
        }

        public MainWindowViewModel()
        {
            RetriveUserData();
            this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(500)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(SearchCollection!);
            //this.WhenValueChanged(x => x.CurLanguage).Throttle(TimeSpan.FromMilliseconds(500)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(GetCurTitle!);
            // Need to disable the add new series while this window is open
            OpenAddNewSeriesWindow = ReactiveCommand.CreateFromTask(() =>
            {
                newSeriesWindow = new AddNewSeriesWindow();
                newSeriesWindow.Show();
                return Task.CompletedTask;
            });
        }

        public static void UpdateCollectionNumbers(ushort maxVolumes, ushort curVolumes){
            MainUser.NumVolumesCollected += curVolumes;
            MainUser.NumVolumesToBeCollected += (ushort)(maxVolumes - curVolumes);
        }

        private void SearchCollection(string searchText)
        {
            SearchedCollection.Clear();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                Parallel.ForEach(Collection.Where(series => series.Titles[1].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) | series.Titles[2].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) | series.Titles[0].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) | series.Staff[0].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) | series.Staff[1].Contains(searchText, StringComparison.CurrentCultureIgnoreCase)), series =>
                {
                    SearchedCollection.Add(series);
                });
            }
            else
            {
                Parallel.ForEach(Collection, series =>
                {
                    SearchedCollection.Add(series);
                });
            }
        }

        public void RetriveUserData(){
            if (new FileInfo(filePath).Length == 0)
            {
                MainUser = new User("UserName", "Native", "Rustic", "Card", null, Collection);
                SaveUsersData();
            }
            GetUserData();
        }

        private void GetUserData()
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                MainUser = (User)binaryFormatter.Deserialize(stream);
                Collection = MainUser.UserCollection;
                _curLanguage = MainUser.CurLanguage;
                _curDisplay = MainUser.Display;
                _numCollected = MainUser.NumVolumesCollected;
                Parallel.ForEach(Collection, series =>
                {
                    SearchedCollection.Add(series);
                });
            }
        }

        public static void SaveUsersData(bool append = false){
            MainUser.UserCollection = Collection;
            MainUser.CurLanguage = _curLanguage;
            MainUser.Display = _curDisplay;
            MainUser.NumVolumesCollected = _numCollected;
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                 binaryFormatter.Serialize(stream, MainUser);
            }
        }
    }
}
