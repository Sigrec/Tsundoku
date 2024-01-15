using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Avalonia.Controls;
using System.Diagnostics;
using ReactiveUI;
using Avalonia.Platform.Storage;
using Tsundoku.Models;
using System.Text.Json.Nodes;
using System.Diagnostics.CodeAnalysis;

namespace Tsundoku.Views
{
    public partial class SettingsWindow : Window
    {
        public UserSettingsViewModel? UserSettingsVM => DataContext as UserSettingsViewModel;
        public bool IsOpen = false;
        public int currencyLength = 0;
        private static readonly FilePickerFileType fileOptions = new FilePickerFileType("JSON File")
        {
            Patterns = [ "*.json" ]
        };
        private static readonly FilePickerOpenOptions filePickerOptions =  new FilePickerOpenOptions {
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>() { fileOptions }
        };
        MainWindow CollectionWindow;
        
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new UserSettingsViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                IsOpen ^= true;

                if (Screens.Primary.WorkingArea.Height < 955)
                {
                    this.Height = 550;
                }
            };

            Closing += (s, e) =>
            {
                if (IsOpen)
                {
                    ((SettingsWindow)s).Hide();
                    Topmost = false;
                    IsOpen ^= true;
                }
                e.Cancel = true;
            };

            this.WhenAnyValue(x => x.IndigoButton.IsChecked, (member) => member != null && member == true).Subscribe(x => UserSettingsVM.IndigoMember = x);
            this.WhenAnyValue(x => x.BarnesAndNobleButton.IsChecked, (member) => member != null && member == true).Subscribe(x => UserSettingsVM.BarnesAndNobleMember = x);
            this.WhenAnyValue(x => x.BooksAMillionButton.IsChecked, (member) => member != null && member == true).Subscribe(x => UserSettingsVM.BooksAMillionMember = x);
            this.WhenAnyValue(x => x.KinokuniyaUSAButton.IsChecked, (member) => member != null && member == true).Subscribe(x => UserSettingsVM.KinokuniyaUSAMember = x);
        }

        public void CurrencyChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).IsDropDownOpen)
            {
                string newCurrency = (CurrencySelector.SelectedItem as ComboBoxItem).Content.ToString();
                currencyLength = CollectionWindow.CollectionViewModel.CurCurrency.Length;
                CollectionWindow.CollectionViewModel.CurCurrency = newCurrency;
                MainWindowViewModel.newSeriesWindow.AddNewSeriesVM.CurCurrency = newCurrency;
                MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice = $"{newCurrency}{ MainWindowViewModel.collectionStatsWindow.CollectionStatsVM.CollectionPrice[currencyLength..]}";
                ViewModelBase.MainUser.Currency = newCurrency;
                LOGGER.Info($"Currency Changed To {newCurrency}");
            }
        }


        /// <summary>
        /// Allows user to upload a new Json file to be used as their new data, it additionall creates a backup file of the users last save
        /// </summary>
        [RequiresUnreferencedCode("Calls Tsundoku.ViewModels.MainWindowViewModel.VersionUpdate(JsonNode)")]
        private async void UploadUserData(object sender, RoutedEventArgs args)
        {
            var file = await this.StorageProvider.OpenFilePickerAsync(filePickerOptions);
            if (file.Count > 0)
            {
                string uploadedFilePath = file[0].Path.LocalPath;
                try
                {
                    JsonNode uploadedUserData = JsonNode.Parse(File.ReadAllText(uploadedFilePath));
                    MainWindowViewModel.VersionUpdate(uploadedUserData, true);
                    _ = JsonSerializer.Deserialize(uploadedUserData, typeof(User), User.UserJsonModel) as User;
                }
                catch(JsonException)
                {
                    LOGGER.Info("{} File is not Valid JSON Schema", uploadedFilePath);
                    return;
                }

                ViewModelBase.isReloading = true;
                int count = 1;
                string backupFileName = @$"UserData_Backup{count}.json";
                while (File.Exists(backupFileName)) { count++; backupFileName = @$"UserData_Backup{count}.json"; }

                File.Replace(uploadedFilePath, MainWindowViewModel.USER_DATA_FILEPATH, backupFileName);
                LOGGER.Info($"Uploaded New UserData File {uploadedFilePath}");

                ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).TryShutdown();
                Process.Start(@$"{AppDomain.CurrentDomain.BaseDirectory}\Tsundoku.exe");
            }
        }

        private void OpenReleasesPage(object sender, PointerPressedEventArgs args)
        {
            Task.Run(() =>
            {
                ViewModelBase.OpenSiteLink(@"https://github.com/Sigrec/Tsundoku/releases");
            });
        }

        public void OpenAniListLink(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                ViewModelBase.OpenSiteLink(@"https://anilist.co/");
            });
        }

        public void OpenMangadexLink(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                ViewModelBase.OpenSiteLink(@"https://mangadex.org/");
            });
        }
        
        public void OpenApplicationFolder(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                Process.Start("explorer.exe", @$"{Path.GetDirectoryName(Path.GetFullPath(@"Covers"))}");
            });
        }

        public void OpenCoversFolder(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>
            {
                if (!Directory.Exists(@"Covers"))
                {
                    Directory.CreateDirectory(@"Covers");
                }
                Process.Start("explorer.exe", @"Covers");
            });
        }

        public void OpenScreenshotsFolder(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                if (!Directory.Exists(@"Screenshots"))
                {
                    Directory.CreateDirectory(@"Screenshots");
                }
                Process.Start("explorer.exe", @"Screenshots");
            });
        }

        public void OpenThemesFolder(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                if (!Directory.Exists(@"Themes"))
                {
                    Directory.CreateDirectory(@"Themes");
                }
                Process.Start("explorer.exe", @"Themes");
            });
        }

        public void ChangeUsername(object sender, RoutedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(UsernameChange.Text))
            {
                CollectionWindow.CollectionViewModel.UserName = UsernameChange.Text;
                LOGGER.Info($"Username Changed To -> {UsernameChange.Text}");
            }
            else
            {
                LOGGER.Warn("Change Username Field is Missing Input");
            }
        }

        public void OpenYoutuberSite(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                ViewModelBase.OpenSiteLink(@$"https://www.youtube.com/@{(sender as Button).Name}");
            });
        }

        public void OpenCoolorsSite(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                ViewModelBase.OpenSiteLink(@"https://coolors.co/generate");
            });
        }

        public void JoinDiscord(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                ViewModelBase.OpenSiteLink(@"https://discord.gg/QcZ5jcFPeU");
            });
        }

        public void ReportBug(object sender, RoutedEventArgs args)
        {
            Task.Run(() =>{
                ViewModelBase.OpenSiteLink(@"https://github.com/Sigrec/TsundokuApp/issues/new");
            });
        }
    }
}
