using ReactiveUI.SourceGenerators;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Tsundoku.Models;
using MangaAndLightNovelWebScrape.Websites;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;
using Tsundoku.Helpers;
using Tsundoku.Services;
using System.Globalization;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using Avalonia.Controls;
using Tsundoku.Views;

namespace Tsundoku.ViewModels;

/// <summary>
/// View model for user settings, managing preferences such as currency, memberships, data import/export, and cover refresh.
/// </summary>
public sealed partial class UserSettingsViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    [Reactive] public partial bool IsChangeUsernameButtonEnabled { get; set; }
    [Reactive] public partial bool BooksAMillionMember { get; set; }
    [Reactive] public partial bool KinokuniyaUSAMember { get; set; }
    [Reactive] public partial bool RefreshCovers { get; set; }
    [Reactive] public partial bool GlassmorphismEnabled { get; set; }
    [Reactive] public partial string SelectedCurrency { get; set; } = "$";

    private readonly AddNewSeriesViewModel _addNewSeriesViewModel;
    private readonly ILoadingDialogService _loadingDialogService;
    private readonly BitmapHelper _bitmapHelper;
    private readonly Clients.MangaDex _mangaDex;
    private readonly Clients.AniList _aniList;

    public UserSettingsViewModel(IUserService userService, AddNewSeriesViewModel addNewSeriesViewModel, ILoadingDialogService loadingDialogService, BitmapHelper bitmapHelper, Clients.MangaDex mangaDex, Clients.AniList aniList) : base(userService)
    {
        _addNewSeriesViewModel = addNewSeriesViewModel;
        _loadingDialogService = loadingDialogService;
        _bitmapHelper = bitmapHelper;
        _mangaDex = mangaDex;
        _aniList = aniList;

        // Sync SelectedCurrency from user data
        this.WhenAnyValue(x => x.CurrentUser)
            .Select(user => user?.Currency)
            .DistinctUntilChanged()
            .Where(currency => !string.IsNullOrWhiteSpace(currency))
            .Subscribe(currency => SelectedCurrency = currency!)
            .DisposeWith(_disposables);

        // Write back to user when SelectedCurrency changes (skip initial)
        this.WhenAnyValue(x => x.SelectedCurrency)
            .Skip(1)
            .DistinctUntilChanged()
            .Where(currency => !string.IsNullOrWhiteSpace(currency))
            .Subscribe(currency =>
            {
                UpdateUserCurrency(currency);
                LOGGER.Info("Currency Changed To {Currency}", currency);
            })
            .DisposeWith(_disposables);

        // Sync initial values once CurrentUser is available
        this.WhenAnyValue(x => x.CurrentUser)
            .Where(user => user is not null)
            .Take(1)
            .Subscribe(user =>
            {
                BooksAMillionMember = user.Memberships.TryGetValue(BooksAMillion.TITLE, out bool bam) && bam;
                KinokuniyaUSAMember = user.Memberships.TryGetValue(KinokuniyaUSA.TITLE, out bool kino) && kino;
                RefreshCovers = user.RefreshCovers;
                GlassmorphismEnabled = user.GlassmorphismEnabled;
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.BooksAMillionMember)
            .Skip(1)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(isMember => UpdateMembership(BooksAMillion.TITLE, isMember))
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.KinokuniyaUSAMember)
            .Skip(1)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(isMember => UpdateMembership(KinokuniyaUSA.TITLE, isMember))
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.RefreshCovers)
            .Skip(1)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(value =>
            {
                _userService.UpdateUser(user => user.RefreshCovers = value);
                LOGGER.Debug("Updated RefreshCovers to {Value}", value);
            })
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.GlassmorphismEnabled)
            .Skip(1)
            .DistinctUntilChanged()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(value =>
            {
                _userService.UpdateUser(user => user.GlassmorphismEnabled = value);
                GlassmorphismService.Apply(value);
                LOGGER.Debug("Updated GlassmorphismEnabled to {Value}", value);
            })
            .DisposeWith(_disposables);
    }

    private void UpdateMembership(string websiteTitle, bool isMember)
    {
        _userService.UpdateUser(user => user.Memberships[websiteTitle] = isMember);
        LOGGER.Debug("Updated {site} Membership to {isMember}", websiteTitle, isMember);
    }

    /// <summary>
    /// Updates the current user's currency preference.
    /// </summary>
    /// <param name="newCurrency">The new currency symbol to set.</param>
    public void UpdateUserCurrency(string newCurrency)
    {
        _userService.UpdateUser(user => user.Currency = newCurrency);
    }

    public async Task RepairUserDataAsync()
    {
        await _userService.RepairUserDataAsync();
    }

    /// <summary>
    /// Updates the current user's display name.
    /// </summary>
    /// <param name="newUsername">The new username to set.</param>
    public void UpdateUserName(string newUsername)
    {
        _userService.UpdateUser(user => user.UserName = newUsername);
    }

    /// <summary>
    /// Imports user data from an external JSON file, showing a loading dialog during the operation.
    /// </summary>
    /// <param name="filePath">The path to the JSON file to import.</param>
    /// <param name="owner">The parent window for the loading dialog.</param>
    public async Task ImportUserDataFromJsonAsync(string filePath, Window owner)
    {
        await _loadingDialogService.ShowAsync("Importing User Data from Json", async vm =>
        {
            vm.IsLoadingIndeterminate = true;
            await _userService.ImportUserDataFromJsonAsync(filePath);
        }, owner);
    }

    /// <summary>
    /// Imports series data from Libib CSV export files.
    /// </summary>
    /// <param name="filePaths">The CSV file paths to import from.</param>
    /// <param name="owner">The parent window for the loading dialog.</param>
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
                    (bool success, string message, _) = await _addNewSeriesViewModel.GetSeriesDataAsync(
                        input: entry.Key.Title,
                        bookType: entry.Key.Format,
                        publisher: entry.Key.Publisher,
                        curVolCount: entry.Value,
                        maxVolCount: entry.Value
                    );
                    vm.ProgressValue++;

                    if (success)
                    {
                        LOGGER.Info("Successfully added series {Series} | {Format} | {Publisher} | {Count} from Libib", entry.Key.Title, entry.Key.Format, entry.Key.Publisher, entry.Value);
                    }
                    else
                    {
                        LOGGER.Info("Unable to add series {Series} from Libib", entry.Key.Title);
                    }
                }
            }
        }, owner);
    }

    /// <summary>
    /// Imports series data from Goodreads CSV export files.
    /// </summary>
    /// <param name="filePaths">The CSV file paths to import from.</param>
    /// <param name="owner">The parent window for the loading dialog.</param>
    public async Task ImportGoodreadsDataFromCsv(string[] filePaths, Window owner)
    {
        await _loadingDialogService.ShowAsync("Importing Goodreads Data", async vm =>
        {
            Dictionary<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint>? result = await GoodreadsParser.ExtractUniqueTitles(filePaths);
            if (result is not null)
            {
                vm.ProgressMaximum = result.Count - 1;
                foreach (KeyValuePair<(string Title, SeriesFormat Format, string Publisher, decimal Rating), uint> entry in result)
                {
                    (bool success, string message, _) = await _addNewSeriesViewModel.GetSeriesDataAsync(
                        input: entry.Key.Title,
                        bookType: entry.Key.Format,
                        publisher: entry.Key.Publisher,
                        curVolCount: entry.Value,
                        maxVolCount: entry.Value,
                        rating: entry.Key.Rating
                    );
                    vm.ProgressValue++;

                    if (success)
                    {
                        LOGGER.Info("Successfully added series {Series} | {Format} | {Publisher} | {Count} from Goodreads", entry.Key.Title, entry.Key.Format, entry.Key.Publisher, entry.Value);
                    }
                    else
                    {
                        LOGGER.Info("Unable to add series {Series} from Goodreads", entry.Key.Title);
                    }
                }
            }
        }, owner);
    }

    /// <summary>
    /// Exports the user's collection to a CSV spreadsheet file.
    /// </summary>
    /// <param name="owner">The parent window for the loading dialog.</param>
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

    /// <summary>
    /// Opens a picker dialog for selecting series, then refreshes cover images for the selected series.
    /// </summary>
    /// <param name="owner">The parent window for the picker and loading dialogs.</param>
    public async Task RefreshAllCoversAsync(Window owner)
    {
        SeriesPickerWindow picker = new(_userService.GetUserCollection(), CurrentUser.Language)
        {
            DataContext = this
        };
        await picker.ShowDialog(owner);

        List<Series>? selected = picker.SelectedSeries;
        if (selected is null || selected.Count == 0) return;

        await _loadingDialogService.ShowCancellableAsync("Refreshing Series", async (vm, ct) =>
        {
            vm.ProgressMaximum = selected.Count;
            vm.ProgressValue = 0;

            foreach (Series series in selected)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    vm.StatusText = $"Refreshing {series.Titles[TsundokuLanguage.Romaji]}";

                    Series? refreshed = await Series.CreateNewSeriesCardAsync(
                        _bitmapHelper,
                        _mangaDex,
                        _aniList,
                        series.GetLinkId(),
                        series.Format,
                        series.MaxVolumeCount,
                        series.CurVolumeCount,
                        series.SeriesContainsAdditionalLanagues(),
                        series.Publisher,
                        series.Demographic,
                        series.VolumesRead,
                        series.Rating,
                        series.Value,
                        string.Empty,
                        allowDuplicate: false,
                        isRefresh: true,
                        isCoverImageRefresh: true,
                        coverPath: series.Cover);

                    _userService.RefreshSeries(series, refreshed);
                    LOGGER.Info("Refreshed {Title}", series.Titles[TsundokuLanguage.Romaji]);
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex, "Failed to refresh {Title}", series.Titles[TsundokuLanguage.Romaji]);
                }
                vm.ProgressValue++;
            }

            _userService.SaveUserData();
        }, owner);
    }
}
