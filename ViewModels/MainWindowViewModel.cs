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

namespace Tsundoku.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string? _searchText;
        private bool _isBusy;
        private Series? _selectedSeries;
        private static string filePath = @"\Tsundoku\UserData\Collection.bin";

        public ICommand OpenAddNewSeriesWindow { get; }

        public static Collection<Series> Collection { get; set; } = new();
        public static ObservableCollection<Series> SearchedCollection { get; set; } = new();
        public static User MainUser { get; set; } = new();
        private AddNewSeriesWindow newSeriesWindow;
        private static List<string> Languages = new List<string> { "Romaji", "English", "Native" };

        // Binding event to track what the current search text is in a users collection
        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public Series? SelectedSeries
        {
            get => _selectedSeries;
            set => this.RaiseAndSetIfChanged(ref _selectedSeries, value);
        }

        public MainWindowViewModel()
        {
            RetriveUserData();
            this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(500)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(SearchCollection!);
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
                Parallel.ForEach(Collection.Where(series => series.EnglishTitle.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)), series =>
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
            if (new FileInfo(filePath).Length != 0)
            {
                GetUserData();
            }
            else
            {
                MainUser = new User("UserName", 'N', "Rustic", null, Collection);
                SaveUsersData();
                GetUserData();
            }
        }

        private void GetUserData()
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                MainUser = (User)binaryFormatter.Deserialize(stream);
                Collection = MainUser.UserCollection;
                Parallel.ForEach(Collection, series =>
                {
                    SearchedCollection.Add(series);
                });
            }
        }

        public static void SaveUsersData(bool append = false){
            MainUser.UserCollection = Collection;
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                 binaryFormatter.Serialize(stream, MainUser);
            }
        }
    }
}
