using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;
using System.Text;
using Tsundoku.Models;
using System.Windows.Input;
using MangaLightNovelWebScrape.Websites.America;

namespace Tsundoku.ViewModels
{
    public class UserSettingsViewModel : ViewModelBase
    {
        [Reactive] public string UsernameText { get; set; }
        [Reactive] public bool IsChangeUsernameButtonEnabled { get; set; }
        [Reactive] public int CurrencyIndex { get; set; }
        [Reactive] public bool RightStufAnimeMember { get; set; } = MainUser.Memberships[RightStufAnime.WEBSITE_TITLE];
        [Reactive] public bool BarnesAndNobleMember { get; set; } = MainUser.Memberships[BarnesAndNoble.WEBSITE_TITLE];
        [Reactive] public bool BooksAMillionMember { get; set; } = MainUser.Memberships[BooksAMillion.WEBSITE_TITLE];
        [Reactive] public bool KinokuniyaUSAMember { get; set; } = MainUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE];
        public ICommand ExportToSpreadSheetAsyncCommand { get; }

        public UserSettingsViewModel()
        {
            CurCurrency = MainUser.Currency;
            ExportToSpreadSheetAsyncCommand = ReactiveCommand.CreateFromTask(ExportToSpreadSheetAsync);

            this.WhenAnyValue(x => x.CurCurrency).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => CurrencyIndex = Array.IndexOf(AVAILABLE_CURRENCY, Uri.UnescapeDataString(x)));
            this.WhenAnyValue(x => x.UsernameText, x => !string.IsNullOrWhiteSpace(x)).Subscribe(x => IsChangeUsernameButtonEnabled = x);
            this.WhenAnyValue(x => x.RightStufAnimeMember).Subscribe(x => MainUser.Memberships[RightStufAnime.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.BarnesAndNobleMember).Subscribe(x => MainUser.Memberships[BarnesAndNoble.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.BooksAMillionMember).Subscribe(x => MainUser.Memberships[BooksAMillion.WEBSITE_TITLE] = x);
            this.WhenAnyValue(x => x.KinokuniyaUSAMember).Subscribe(x => MainUser.Memberships[KinokuniyaUSA.WEBSITE_TITLE] = x);
        }

        private static async Task ExportToSpreadSheetAsync()
        {
            await Task.Run(() =>
            {
                string file = @"TsundokuCollection.csv";
                StringBuilder output = new();
                string[] headers = ["Title", "Staff", "Format", "Status", "Cur Volumes", "Max Volumes", "Demographic", "Cost", "Rating", "Volumes Read", "Notes"];
                output.AppendLine(string.Join(",", headers));

                foreach (Series curSeries in MainWindowViewModel.UserCollection)
                {
                    output.AppendLine(string.Join(",", new string[] { 
                        curSeries.Titles.ContainsKey(MainUser.CurLanguage) ? curSeries.Titles[MainUser.CurLanguage] : curSeries.Titles["Romaji"], 
                        curSeries.Staff.ContainsKey(MainUser.CurLanguage) ? curSeries.Staff[MainUser.CurLanguage] : curSeries.Staff["Romaji"], 
                        curSeries.Format, 
                        curSeries.Status, 
                        curSeries.CurVolumeCount.ToString(), 
                        curSeries.MaxVolumeCount.ToString(), 
                        curSeries.Demographic, 
                        $"{MainUser.Currency}{curSeries.Cost}", 
                        curSeries.Rating.ToString(), 
                        $"{curSeries.VolumesRead}", 
                        curSeries.SeriesNotes }));
                }

                try
                {
                    System.IO.File.WriteAllTextAsync(file, output.ToString(), Encoding.UTF8);
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
