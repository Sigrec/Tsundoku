using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;
using Tsundoku.Models;
using System.Windows.Input;
using MangaAndLightNovelWebScrape.Websites;
using System.Text.Json.Nodes;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Platform.Storage;
namespace Tsundoku.ViewModels
{
    public class UserSettingsViewModel : ViewModelBase
    {
        [Reactive] public string UsernameText { get; set; }
        [Reactive] public bool IsChangeUsernameButtonEnabled { get; set; }
        [Reactive] public int CurrencyIndex { get; set; }
        [Reactive] public bool IndigoMember { get; set; } = MainUser.Memberships[Indigo.WEBSITE_TITLE];
        [Reactive] public bool BooksAMillionMember { get; set; } = MainUser.Memberships[BooksAMillion.WEBSITE_TITLE];
        [Reactive] public bool KinokuniyaUSAMember { get; set; } = MainUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE];
        public ICommand ExportToSpreadSheetAsyncCommand { get; }

        public UserSettingsViewModel()
        {
            ExportToSpreadSheetAsyncCommand = ReactiveCommand.CreateFromTask(ExportToSpreadSheetAsync);
            this.WhenAnyValue(x => x.CurCurrencyInstance).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => CurrencyIndex = Array.IndexOf(AVAILABLE_CURRENCY, Uri.UnescapeDataString(x)));
            this.WhenAnyValue(x => x.CurCurrencyInstance).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => MainUser.Currency = CurCurrencyInstance);
            this.WhenAnyValue(x => x.UsernameText, x => !string.IsNullOrWhiteSpace(x)).Subscribe(x => IsChangeUsernameButtonEnabled = x);
            this.WhenAnyValue(x => x.IndigoMember).Subscribe(x => MainUser.Memberships[Indigo.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.BooksAMillionMember).Subscribe(x => MainUser.Memberships[BooksAMillion.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.KinokuniyaUSAMember).Subscribe(x => MainUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE] = x);
        }

        private static async Task ExportToSpreadSheetAsync()
        {
            await Task.Run(async () =>
            {
                string COLLECTION_FILE = @"TsundokuCollection.csv";
                StringBuilder output = new();
                if (!string.IsNullOrWhiteSpace(MainUser.Notes))
                {
                    output.AppendFormat("\"{0}\"", MainUser.Notes).AppendLine();
                }
                string[] headers = ["Title", "Staff", "Format", "Status", "Cur Volumes", "Max Volumes", "Demographic", "Value", "Rating", "Volumes Read", "Genres", "Notes"];
                output.AppendLine(string.Join(",", headers));

                foreach (Series curSeries in MainWindowViewModel.UserCollection)
                {
                    string titleLang = curSeries.Titles.ContainsKey(MainUser.CurLanguage) ? MainUser.CurLanguage : "Romaji";
                    string staffLang = curSeries.Staff.ContainsKey(MainUser.CurLanguage) ? MainUser.CurLanguage : "Romaji";

                    output.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}\n",
                        $"\"{curSeries.Titles[titleLang]}{(!titleLang.Equals("Romaji") && !curSeries.Titles[titleLang].Equals(curSeries.Titles["Romaji"], StringComparison.OrdinalIgnoreCase) ? $" ({curSeries.Titles["Romaji"]})\"" : "\"")}",
                        $"\"{curSeries.Staff[staffLang]}{(!staffLang.Equals("Romaji") && !curSeries.Staff[staffLang].Equals(curSeries.Staff["Romaji"], StringComparison.OrdinalIgnoreCase) ? $" ({curSeries.Staff["Romaji"]})\"" : "\"")}", 
                        curSeries.Format.ToString(), 
                        curSeries.Status.ToString(), 
                        curSeries.CurVolumeCount, 
                        curSeries.MaxVolumeCount, 
                        curSeries.Demographic.ToString(), 
                        $"{MainUser.Currency}{curSeries.Value}", 
                        curSeries.Rating != -1 ? curSeries.Rating : string.Empty, 
                        curSeries.VolumesRead,
                        curSeries.Genres != null ? $"\"{string.Join("\n", curSeries.Genres)}\"" : string.Empty,
                        $"\"{curSeries.SeriesNotes}\""
                    );
                }

                try
                {
                    await File.WriteAllTextAsync(COLLECTION_FILE, output.ToString(), Encoding.UTF8);
                    LOGGER.Info($"Exported {MainUser.UserName}'s Data To -> TsundokuCollection.csv");
                    await OpenSiteLink(@"TsundokuCollection.csv");
                }
                catch (Exception ex)
                {
                    LOGGER.Warn($"Could not Export {MainUser.UserName}'s Data To -> TsundokuCollection.csv \n{ex}");
                }
            });
        }

        /// <summary>
        /// Allows user to upload a new Json file to be used as their new data, it additionall creates a backup file of the users last save
        /// </summary>
        [RequiresUnreferencedCode("Calls UpdateVersion(JsonNode)")]
        public static void ImportUserData(IReadOnlyList<IStorageFile>? file)
        {
            string uploadedFilePath = file[0].Path.LocalPath;
            try
            {
                JsonNode uploadedUserData = JsonNode.Parse(File.ReadAllText(uploadedFilePath));
                User.UpdateSchemaVersion(uploadedUserData, true);
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
        }
    }
}
