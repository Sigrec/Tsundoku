using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;
using Tsundoku.Models;
using System.Windows.Input;
using MangaAndLightNovelWebScrape.Websites;
namespace Tsundoku.ViewModels
{
    public class UserSettingsViewModel : ViewModelBase
    {
        [Reactive] public string UsernameText { get; set; }
        [Reactive] public bool IsChangeUsernameButtonEnabled { get; set; }
        [Reactive] public int CurrencyIndex { get; set; }
        [Reactive] public bool IndigoMember { get; set; } = MainUser.Memberships[Indigo.WEBSITE_TITLE];
        [Reactive] public bool BarnesAndNobleMember { get; set; } = MainUser.Memberships[BarnesAndNoble.WEBSITE_TITLE];
        [Reactive] public bool BooksAMillionMember { get; set; } = MainUser.Memberships[BooksAMillion.WEBSITE_TITLE];
        [Reactive] public bool KinokuniyaUSAMember { get; set; } = MainUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE];
        public ICommand ExportToSpreadSheetAsyncCommand { get; }

        public UserSettingsViewModel()
        {
            this.CurrentTheme = MainUser.SavedThemes.First(theme => theme.ThemeName.Equals(MainUser.MainTheme));
            CurCurrency = MainUser.Currency;
            ExportToSpreadSheetAsyncCommand = ReactiveCommand.CreateFromTask(ExportToSpreadSheetAsync);

            this.WhenAnyValue(x => x.CurCurrency).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => CurrencyIndex = Array.IndexOf(AVAILABLE_CURRENCY, Uri.UnescapeDataString(x)));
            this.WhenAnyValue(x => x.UsernameText, x => !string.IsNullOrWhiteSpace(x)).Subscribe(x => IsChangeUsernameButtonEnabled = x);
            this.WhenAnyValue(x => x.IndigoMember).Subscribe(x => MainUser.Memberships[Indigo.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.BarnesAndNobleMember).Subscribe(x => MainUser.Memberships[BarnesAndNoble.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.BooksAMillionMember).Subscribe(x => MainUser.Memberships[BooksAMillion.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.KinokuniyaUSAMember).Subscribe(x => MainUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE] = x);
        }

        private static async Task ExportToSpreadSheetAsync()
        {
            await Task.Run(() =>
            {
                string COLLECTION_FILE = @"TsundokuCollection.csv";
                StringBuilder output = new();
                string[] headers = ["Title", "Staff", "Format", "Status", "Cur Volumes", "Max Volumes", "Demographic", "Cost", "Rating", "Volumes Read", "Notes"];
                output.AppendLine(string.Join(",", headers));

                foreach (Series curSeries in MainWindowViewModel.UserCollection)
                {
                    output.AppendLine(string.Join(",", [ 
                        $"\"{(curSeries.Titles.ContainsKey(MainUser.CurLanguage) ? curSeries.Titles[MainUser.CurLanguage] : curSeries.Titles["Romaji"])}{(MainUser.CurLanguage == "Romaji" || !curSeries.Titles.ContainsKey(MainUser.CurLanguage) || (curSeries.Titles.ContainsKey(MainUser.CurLanguage) && curSeries.Titles["Romaji"].Equals(curSeries.Titles[MainUser.CurLanguage], StringComparison.OrdinalIgnoreCase)) ? string.Empty : $" ({curSeries.Titles["Romaji"]})")}\"",
                        $"{(MainUser.CurLanguage == "Romaji" || !curSeries.Staff.ContainsKey(MainUser.CurLanguage) || (curSeries.Staff.ContainsKey(MainUser.CurLanguage) && curSeries.Staff["Romaji"].Equals(curSeries.Staff[MainUser.CurLanguage], StringComparison.OrdinalIgnoreCase)) ? curSeries.Staff["Romaji"] : $"\"{curSeries.Staff[MainUser.CurLanguage]} ({curSeries.Staff["Romaji"]}")})\"", 
                        curSeries.Format.ToString(), 
                        curSeries.Status.ToString(), 
                        curSeries.CurVolumeCount.ToString(), 
                        curSeries.MaxVolumeCount.ToString(), 
                        curSeries.Demographic.ToString(), 
                        $"{MainUser.Currency}{curSeries.Cost}", 
                        curSeries.Rating.ToString(), 
                        curSeries.VolumesRead.ToString(), 
                        curSeries.SeriesNotes ]));
                }

                try
                {
                    File.WriteAllTextAsync(COLLECTION_FILE, output.ToString(), Encoding.UTF8);
                    LOGGER.Info($"Exported {MainUser.UserName}'s Data To -> TsundokuCollection.csv");
                }
                catch (Exception ex)
                {
                    LOGGER.Warn($"Could not Export {MainUser.UserName}'s Data To -> TsundokuCollection.csv \n{ex}");
                }
            });
        }
    }
}
