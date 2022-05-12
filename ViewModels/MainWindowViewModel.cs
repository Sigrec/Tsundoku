using ReactiveUI;
using Tsundoku.Models;
using System.Collections.ObjectModel;
using System.IO;
using Tsundoku.Views;
using System.Windows.Input;
using System.ComponentModel;
using System;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;

namespace Tsundoku.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string? _searchText;
        private bool _isBusy;
        private Series? _selectedSeries;
        private static string filePath = @"\Tsundoku\UserData\Collection.bin";
        private Bitmap? _cover;
        public Bitmap? Cover
        {
            get => _cover;
            private set => this.RaiseAndSetIfChanged(ref _cover, value);
        }

        public ICommand OpenAddNewSeriesWindow { get; }

        public static ObservableCollection<Series> Collection { get; set; } = new();
        private User MainUser = new();
        private AddNewSeriesWindow newSeriesWindow;

        public MainWindowViewModel()
        {
            RetriveUserData();
            OpenAddNewSeriesWindow = ReactiveCommand.CreateFromTask(() => {
                newSeriesWindow = new AddNewSeriesWindow();
                newSeriesWindow.Show();
                return Task.CompletedTask;
            });
        }

        protected virtual void MainWindowCloseEvent(CancelEventArgs e)
        {
            SaveUsersData();
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
            }
        }

        public void SaveUsersData(bool append = false){
            MainUser.UserCollection = Collection;
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                 binaryFormatter.Serialize(stream, MainUser);
            }
        }

        // Binding event to track what the current search text is in a users collection
        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        // Binding event to check if the user is searching for anything in their list
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        public Series? SelectedSeries
        {
            get => _selectedSeries;
            set => this.RaiseAndSetIfChanged(ref _selectedSeries, value);
        }
    }
}
