using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;
using Tsundoku.Models;
using System.Windows.Input;
using MangaAndLightNovelWebScrape.Websites;
using static Tsundoku.Models.TsundokuLanguageModel;
using Tsundoku.Helpers;
using System.Globalization;
using DynamicData;
namespace Tsundoku.ViewModels
{
    public class UserSettingsViewModel : ViewModelBase
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        [Reactive] public bool IsChangeUsernameButtonEnabled { get; set; }
        [Reactive] public bool IndigoMember { get; set; }
        [Reactive] public bool BooksAMillionMember { get; set; }
        [Reactive] public bool KinokuniyaUSAMember { get; set; }
        [Reactive] public int SelectedCurrencyIndex { get; set; } = 0;
        public ICommand ExportToSpreadSheetAsyncCommand { get; }
        public UserSettingsViewModel(IUserService userService) : base(userService)
        {
            this.WhenAnyValue(x => x.CurrentUser)
                .Where(user => user != null)
                .Select(user => user!.Currency)
                .DistinctUntilChanged()
                .Subscribe(currency =>
                {
                    SelectedCurrencyIndex = AVAILABLE_CURRENCY.IndexOf(currency);
                });

            BooksAMillionMember = CurrentUser.Memberships[BooksAMillion.WEBSITE_TITLE];
            this.WhenAnyValue(x => x.BooksAMillionMember)
                .Skip(1)
                .Subscribe(isMember => UpdateMembership(BooksAMillion.WEBSITE_TITLE, isMember));

            IndigoMember = CurrentUser.Memberships[Indigo.WEBSITE_TITLE];
            this.WhenAnyValue(x => x.IndigoMember)
                .Skip(1)
                .Subscribe(isMember => UpdateMembership(Indigo.WEBSITE_TITLE, isMember));

            KinokuniyaUSAMember = CurrentUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE];
            this.WhenAnyValue(x => x.KinokuniyaUSAMember)
                .Skip(1)
                .Subscribe(isMember => UpdateMembership(KinokuniyaUSA.WEBSITE_TITLE, isMember));

            ExportToSpreadSheetAsyncCommand = ReactiveCommand.CreateFromTask(ExportToSpreadSheetAsync);
        }

        private void UpdateMembership(string websiteTitle, bool isMember)
        {
            _userService.UpdateUser(user => user.Memberships[websiteTitle] = isMember);
            LOGGER.Debug("Updated {site} Membership to {isMember}", websiteTitle, isMember);
        }

        public void UpdateUserCurrency(string newCurrency)
        {
            _userService.UpdateUser(user => user.Currency = newCurrency);
        }

        public void UpdateUserName(string newUsername)
        {
            _userService.UpdateUser(user => user.UserName = newUsername);
        }

        public void ImportUserDataFromJson(string filePath)
        {
            _userService.ImportUserDataFromJson(filePath);
        }

        private async Task ExportToSpreadSheetAsync()
        {
            LOGGER.Info("Exporting User Collection to CSV Spreadhseet");
            string filePath = AppFileHelper.GetFilePath("TsundokuCollection.csv");
            string userNotes = CurrentUser.Notes;
            TsundokuLanguage userLanguage = CurrentUser.Language;
            string userCurrency = CurrentUser.Currency;

            IReadOnlyList<Series> seriesToExport = _userService.GetUserCollection();

            await Task.Run(async () =>
            {
                StringBuilder output = new();

                if (!string.IsNullOrWhiteSpace(userNotes))
                {
                    output.Append($"\"{userNotes.Replace("\"", "\"\"")}\"").AppendLine().AppendLine();
                }

                string[] headers = ["Title", "Staff", "Format", "Status", "Cur Volumes", "Max Volumes", "Demographic", "Value", "Rating", "Volumes Read", "Genres", "Notes"];
                output.AppendLine(string.Join(",", headers));

                foreach (Series curSeries in seriesToExport)
                {
                    string title = curSeries.Titles.TryGetValue(userLanguage, out string? preferredTitle) ? preferredTitle : title = curSeries.Titles[TsundokuLanguage.Romaji];
                    string romajiTitle = curSeries.Titles[TsundokuLanguage.Romaji]; // Get Romaji title regardless

                    string staff = curSeries.Staff.TryGetValue(userLanguage, out string? preferredStaff) ? preferredStaff : curSeries.Staff[TsundokuLanguage.Romaji];
                    string romajiStaff = curSeries.Staff[TsundokuLanguage.Romaji]; // Get Romaji staff regardless

                    string formattedTitle = !userLanguage.Equals(TsundokuLanguage.Romaji) && !title.Equals(romajiTitle, StringComparison.OrdinalIgnoreCase) ? title : $"{title} ({romajiTitle})";

                    string formattedStaff = !userLanguage.Equals(TsundokuLanguage.Romaji) && !staff.Equals(romajiStaff, StringComparison.OrdinalIgnoreCase) ? staff : $"{staff} ({romajiStaff})";

                    output.Append($"\"{formattedTitle.Replace("\"", "\"\"")}\",");
                    output.Append($"\"{formattedStaff.Replace("\"", "\"\"")}\",");
                    output.Append($"{curSeries.Format},");
                    output.Append($"{curSeries.Status},");
                    output.Append($"{curSeries.CurVolumeCount},");
                    output.Append($"{curSeries.MaxVolumeCount},");
                    output.Append($"{curSeries.Demographic},");
                    output.Append($"{userCurrency}{curSeries.Value.ToString(CultureInfo.InvariantCulture)},");
                    output.Append($"{(curSeries.Rating != -1 ? curSeries.Rating.ToString(CultureInfo.InvariantCulture) : string.Empty)},");
                    output.Append($"{curSeries.VolumesRead},");
                    output.Append($"{(curSeries.Genres.Count != 0 ? $"\"{string.Join(" | ", curSeries.Genres)}\"" : string.Empty)},");
                    output.Append($"\"{(!string.IsNullOrWhiteSpace(curSeries.SeriesNotes) ? curSeries.SeriesNotes.Replace("\"", "\"\"") : string.Empty)}\"\n");
                }

                try
                {
                    await File.WriteAllTextAsync(filePath, output.ToString(), Encoding.UTF8);
                    LOGGER.Info("Exported Spreadsheet Data to -> {filePath}", filePath);
                    await OpenSiteLink(filePath);
                }
                catch (Exception ex)
                {
                    LOGGER.Warn("Could not Export Collection Spreadsheet Data to -> {filePath}\n{ex.Message}", filePath, ex.Message);
                }
            });
        }
    }
}
