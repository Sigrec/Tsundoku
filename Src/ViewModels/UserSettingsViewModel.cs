using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;
using Tsundoku.Models;
using System.Windows.Input;
using MangaAndLightNovelWebScrape.Websites;
using static Tsundoku.Models.TsundokuLanguageModel;
using Tsundoku.Services;
using System.Collections.ObjectModel;
namespace Tsundoku.ViewModels
{
    public class UserSettingsViewModel : ViewModelBase
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        [Reactive] public string UsernameText { get; set; }
        [Reactive] public bool IsChangeUsernameButtonEnabled { get; set; }
        [Reactive] public int CurrencyIndex { get; set; }
        [Reactive] public bool IndigoMember { get; set; }
        [Reactive] public bool BooksAMillionMember { get; set; }
        [Reactive] public bool KinokuniyaUSAMember { get; set; }
        public ICommand ExportToSpreadSheetAsyncCommand { get; }
        private readonly ISharedSeriesCollectionProvider _sharedSeriesProvider;
        public ReadOnlyObservableCollection<Series> UserCollection { get; }

        public UserSettingsViewModel(IUserService userService, ISharedSeriesCollectionProvider sharedSeriesProvider) : base(userService)
        {
            _sharedSeriesProvider = sharedSeriesProvider ?? throw new ArgumentNullException(nameof(sharedSeriesProvider));
            UserCollection = _sharedSeriesProvider.DynamicUserCollection;
            IndigoMember = CurrentUser.Memberships[Indigo.WEBSITE_TITLE];
            BooksAMillionMember = CurrentUser.Memberships[BooksAMillion.WEBSITE_TITLE];
            KinokuniyaUSAMember = CurrentUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE];

            ExportToSpreadSheetAsyncCommand = ReactiveCommand.CreateFromTask(ExportToSpreadSheetAsync);
            // this.WhenAnyValue(x => x.CurCurrencyInstance).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => CurrencyIndex = Array.IndexOf(AVAILABLE_CURRENCY, Uri.UnescapeDataString(x)));
            // this.WhenAnyValue(x => x.CurCurrencyInstance).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => CurrentUser.Currency = CurCurrencyInstance);
            // this.WhenAnyValue(x => x.UsernameText, x => !string.IsNullOrWhiteSpace(x)).Subscribe(x => IsChangeUsernameButtonEnabled = x);
            this.WhenAnyValue(x => x.IndigoMember).Subscribe(x => CurrentUser.Memberships[Indigo.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.BooksAMillionMember).Subscribe(x => CurrentUser.Memberships[BooksAMillion.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.KinokuniyaUSAMember).Subscribe(x => CurrentUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE] = x);
        }

        public void ImportUserData(string filePath)
        {
            _userService.ImportUserData(filePath);
        }

        private async Task ExportToSpreadSheetAsync()
        {
            await Task.Run(async () =>
            {
                string COLLECTION_FILE = @"TsundokuCollection.csv";
                StringBuilder output = new();
                if (!string.IsNullOrWhiteSpace(CurrentUser.Notes))
                {
                    output.AppendFormat("\"{0}\"", CurrentUser.Notes).AppendLine();
                }
                string[] headers = ["Title", "Staff", "Format", "Status", "Cur Volumes", "Max Volumes", "Demographic", "Value", "Rating", "Volumes Read", "Genres", "Notes"];
                output.AppendLine(string.Join(",", headers));

                foreach (Series curSeries in UserCollection)
                {
                    TsundokuLanguage titleLang = curSeries.Titles.ContainsKey(CurrentUser.Language) ? CurrentUser.Language : TsundokuLanguage.Romaji;
                    TsundokuLanguage staffLang = curSeries.Staff.ContainsKey(CurrentUser.Language) ? CurrentUser.Language : TsundokuLanguage.Romaji;

                    output.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}\n",
                        $"\"{curSeries.Titles[titleLang]}{(!titleLang.Equals(TsundokuLanguage.Romaji) && !curSeries.Titles[titleLang].Equals(curSeries.Titles[TsundokuLanguage.Romaji], StringComparison.OrdinalIgnoreCase) ? $" ({curSeries.Titles[TsundokuLanguage.Romaji]})\"" : "\"")}",
                        $"\"{curSeries.Staff[staffLang]}{(!staffLang.Equals(TsundokuLanguage.Romaji) && !curSeries.Staff[staffLang].Equals(curSeries.Staff[TsundokuLanguage.Romaji], StringComparison.OrdinalIgnoreCase) ? $" ({curSeries.Staff[TsundokuLanguage.Romaji]})\"" : "\"")}",
                        curSeries.Format.ToString(),
                        curSeries.Status.ToString(),
                        curSeries.CurVolumeCount,
                        curSeries.MaxVolumeCount,
                        curSeries.Demographic.ToString(),
                        $"{CurrentUser.Currency}{curSeries.Value}",
                        curSeries.Rating != -1 ? curSeries.Rating : string.Empty,
                        curSeries.VolumesRead,
                        curSeries.Genres != null ? $"\"{string.Join("\n", curSeries.Genres)}\"" : string.Empty,
                        $"\"{curSeries.SeriesNotes}\""
                    );
                }

                try
                {
                    await File.WriteAllTextAsync(COLLECTION_FILE, output.ToString(), Encoding.UTF8);
                    LOGGER.Info($"Exported {CurrentUser.UserName}'s Data To -> TsundokuCollection.csv");
                    await OpenSiteLink(@"TsundokuCollection.csv");
                }
                catch (Exception ex)
                {
                    LOGGER.Warn($"Could not Export {CurrentUser.UserName}'s Data To -> TsundokuCollection.csv \n{ex}");
                }
            });
        }
    }
}
