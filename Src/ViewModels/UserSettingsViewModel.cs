using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;
using Tsundoku.Models;
using MangaAndLightNovelWebScrape.Websites;
using static Tsundoku.Models.Enums.TsundokuLanguageEnums;
using Tsundoku.Helpers;
using System.Globalization;
using static Tsundoku.Models.Enums.SeriesFormatEnum;
using Avalonia.Controls;

namespace Tsundoku.ViewModels;

public sealed class UserSettingsViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    [Reactive] public bool IsChangeUsernameButtonEnabled { get; set; }
    [Reactive] public bool IndigoMember { get; set; }
    [Reactive] public bool BooksAMillionMember { get; set; }
    [Reactive] public bool KinokuniyaUSAMember { get; set; }
    [Reactive] public int SelectedCurrencyIndex { get; set; } = 0;

    private readonly AddNewSeriesViewModel _addNewSeriesViewModel;
    private readonly ILoadingDialogService _loadingDialogService;

    public UserSettingsViewModel(IUserService userService, AddNewSeriesViewModel addNewSeriesViewModel, ILoadingDialogService loadingDialogService) : base(userService)
    {
        _addNewSeriesViewModel = addNewSeriesViewModel;
        _loadingDialogService = loadingDialogService;

        this.WhenAnyValue(x => x.CurrentUser)
            .Select(user => user?.Currency)
            .DistinctUntilChanged()
            .Where(currency =>
                !string.IsNullOrWhiteSpace(currency) &&
                AVAILABLE_CURRENCY_WITH_CULTURE.ContainsKey(currency))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(currency =>
            {
                SelectedCurrencyIndex = AVAILABLE_CURRENCY_WITH_CULTURE[currency].Index;
            });

        BooksAMillionMember = CurrentUser.Memberships[BooksAMillion.WEBSITE_TITLE];
        this.WhenAnyValue(x => x.BooksAMillionMember)
            .Skip(1)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(isMember => UpdateMembership(BooksAMillion.WEBSITE_TITLE, isMember));

        IndigoMember = CurrentUser.Memberships[Indigo.WEBSITE_TITLE];
        this.WhenAnyValue(x => x.IndigoMember)
            .Skip(1)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(isMember => UpdateMembership(Indigo.WEBSITE_TITLE, isMember));

        KinokuniyaUSAMember = CurrentUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE];
        this.WhenAnyValue(x => x.KinokuniyaUSAMember)
            .Skip(1)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(isMember => UpdateMembership(KinokuniyaUSA.WEBSITE_TITLE, isMember));
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

    public void ImportUserDataFromJson(string filePath, Window owner)
    {
        _loadingDialogService.Show("Importing User Data from Json", vm =>
        {
            vm.IsLoadingIndeterminate = true;
            _userService.ImportUserDataFromJson(filePath);
        }, owner);
    }

    public async Task ImportLibibDataFromCsv(string[] filePaths, Window owner)
    {
        await _loadingDialogService.ShowAsync("Importing Libib Data", async vm =>
        {
            Dictionary<(string Title, SeriesFormat Format, string Publisher), uint>? result = await LibibParser.ExtractUniqueTitles(filePaths);
            if (result is not null)
            {
                vm.ProgressMaximum = result.Count - 1;
                foreach (KeyValuePair<(string Title, SeriesFormat Format, string Publisher), uint> entry in result)
                {
                    KeyValuePair<bool, string> addSeriesResult = await _addNewSeriesViewModel.GetSeriesDataAsync(
                        input: entry.Key.Title,
                        bookType: entry.Key.Format,
                        publisher: entry.Key.Publisher,
                        curVolCount: entry.Value,
                        maxVolCount: entry.Value
                    );
                    vm.ProgressValue++;

                    if (addSeriesResult.Key)
                    {
                        LOGGER.Info("Successfully added series {Series} | {Format} | {Publisher} | {Count}", entry.Key.Title, entry.Key.Format, entry.Key.Publisher, entry.Value);
                    }
                    else
                    {
                        LOGGER.Info("Unable to add series {Series} from Libib", entry.Key.Title);
                    }
                }
            }
        }, owner);
    }

    public async Task ExportToSpreadSheetAsync(Window owner)
    {
        await _loadingDialogService.ShowAsync("Exporting Data to CSV", async vm =>
        {
            vm.IsLoadingIndeterminate = true;
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
        }, owner);
    }
}
