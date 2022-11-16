using ReactiveUI;
using Tsundoku.Models;
using System.Collections.ObjectModel;
using System.IO;
using Tsundoku.Views;
using System.Windows.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Avalonia.Metadata;
using System.ComponentModel;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Reactive;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Tsundoku.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private string? _searchText;
        private Series? _selectedSeries;
        private static string filePath = @"\Tsundoku\UserData\UserData.dat";
        public ICommand OpenAddNewSeriesWindow { get; }
        public ICommand ShowEditSeriesPane { get; }
        public static ObservableCollection<Series> SearchedCollection { get; set; }
        public static ObservableCollection<Series> Collection { get; set; } = new();
        public static User MainUser { get; set; }
        private AddNewSeriesWindow newSeriesWindow;
        public static string _curLanguage, _curDisplay;
        public static uint _curVolumesCollected, _curVolumesToBeCollected;
        public string[] AvailableLanguages { get; } = new string[] {"Romaji", "English", "Native"};
        public string[] AvailableDisplays { get; } = new string[] {"Card", "Mini-Card"};

        [Reactive]
        public string SearchText { get; set; }

        [Reactive]
        public uint UsersNumVolumesCollected { get; set; }

        [Reactive]
        public uint UsersNumVolumesToBeCollected { get; set; }
        private ReactiveCommand<Unit, uint> IncrementVolumeCount { get; }
        
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

        public MainWindowViewModel()
        {
            RetriveUserData();
            this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(300)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(SearchCollection!);
            OpenAddNewSeriesWindow = ReactiveCommand.CreateFromTask(() =>
            {
                newSeriesWindow = new AddNewSeriesWindow();
                newSeriesWindow.Show();
                return Task.CompletedTask;
            });

            IncrementVolumeCount = ReactiveCommand.Create(() => UsersNumVolumesCollected += 1);
            this.WhenAnyValue(x => x.UsersNumVolumesCollected).Subscribe(x => MainUser.NumVolumesCollected = x);
            this.WhenAnyValue(x => x.UsersNumVolumesToBeCollected).Subscribe(x => MainUser.NumVolumesToBeCollected = x);
            Collection.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    UsersNumVolumesCollected += Collection[Collection.Count - 1].CurVolumeCount;
                    UsersNumVolumesToBeCollected += (uint)Collection[Collection.Count - 1].MaxVolumeCount - Collection[Collection.Count - 1].CurVolumeCount;
                }
            };
        }

        public static void SortCollection()
        {
            SearchedCollection.Clear();
            switch (_curLanguage)
            {
                case "Native":
                    foreach (Series x in Collection.OrderBy(x => x.Titles[2], StringComparer.CurrentCulture))
                    {
                        SearchedCollection.Add(x);
                    }
                    break;
                case "English":
                    foreach (Series x in Collection.OrderBy(x => x.Titles[1], StringComparer.CurrentCulture))
                    {
                        SearchedCollection.Add(x);
                    }
                    break;
                default:
                    foreach (Series x in Collection.OrderBy(x => x.Titles[0], StringComparer.CurrentCulture))
                    {
                        SearchedCollection.Add(x);
                    }
                    break;
            }
            //Collection = SearchedCollection;
            Logger.Info($"Sorting {_curLanguage}");
        }

        private void SearchCollection(string searchText)
        {
            SearchedCollection.Clear();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var series in Collection.Where(x => x.Titles[1].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) | x.Titles[2].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) | x.Titles[0].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) | x.Staff[0].Contains(searchText, StringComparison.CurrentCultureIgnoreCase) | x.Staff[1].Contains(searchText, StringComparison.CurrentCultureIgnoreCase)))
                {
                    SearchedCollection.Add(series);
                }
            }
            else
            {
                // foreach (var series in Collection)
                // {
                //     SearchedCollection.Add(series);
                // }
                SortCollection();
            }
        }

        public void RetriveUserData(){
            Logger.Info("Starting TsundOku");
            if (!File.Exists(filePath))
            {
                Logger.Info("Creating New User");
                MainUser = new User("UserName", "Native", "Rustic", "Card", null, Collection);
                _curLanguage = MainUser.CurLanguage;
                _curDisplay = MainUser.Display;
                UsersNumVolumesCollected = 0;
                UsersNumVolumesToBeCollected = 0;
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
            }
            Collection = MainUser.UserCollection;
            SearchedCollection = new ObservableCollection<Series>(Collection);
            _curLanguage = MainUser.CurLanguage;
            _curDisplay = MainUser.Display;
            UsersNumVolumesCollected = MainUser.NumVolumesCollected;
            UsersNumVolumesToBeCollected = MainUser.NumVolumesToBeCollected;
            Logger.Info($"Loading {MainUser.UserName}'s Data");
        }

        public static async void SaveUsersData(bool append = false){

            //Collection = SearchedCollection;
            MainUser.UserCollection = Collection;
            MainUser.CurLanguage = _curLanguage;
            MainUser.Display = _curDisplay;
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, MainUser);
            }
        }
    }
}
