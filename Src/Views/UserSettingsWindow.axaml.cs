using Avalonia;
using GemBox.Spreadsheet;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Tsundoku.ViewModels;
using Avalonia.Controls;
using System.Diagnostics;

namespace Tsundoku.Views
{
    public partial class SettingsWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public UserSettingsViewModel? UserSettingsVM => DataContext as UserSettingsViewModel;
        public bool IsOpen = false;

        MainWindow CollectionWindow;
        
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new UserSettingsViewModel();
            Opened += (s, e) =>
            {
                CollectionWindow = (MainWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                UserSettingsVM.CurrentTheme = CollectionWindow.CollectionViewModel.CurrentTheme;
                IsOpen ^= true;
            };

            Closing += (s, e) =>
            {
                ((SettingsWindow)s).Hide();
                Topmost = false;
                IsOpen ^= true;
                e.Cancel = true;
            };
        }

        private void OpenCoversFolder(object sender, RoutedEventArgs args)
        {
            Process.Start("explorer.exe", @$"Covers");
        }

        private void ChangeUsername(object sender, RoutedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(UsernameChange.Text))
            {
                (((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Windows[0] as MainWindow).CollectionViewModel.UserName = UsernameChange.Text;
                Logger.Info($"Username Changed To -> {UsernameChange.Text}");
            }
            else
            {
                Logger.Warn("Change Username Field is Missing Input");
            }
        }  

        private void JoinDiscord(object sender, RoutedEventArgs args)
        {
            Logger.Info(@"Opening Issue Repo https://discord.gg/QcZ5jcFPeU");
            try
            {
                Process.Start(new ProcessStartInfo(@"https://discord.gg/QcZ5jcFPeU") { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                Logger.Error(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Logger.Error(other.Message);
            }
        }

        private void ReportBug(object sender, RoutedEventArgs args)
        {
            Logger.Info(@"Opening Issue Repo https://github.com/Sigrec/TsundokuApp/issues/new");
            try
            {
                Process.Start(new ProcessStartInfo(@"https://github.com/Sigrec/TsundokuApp/issues/new") { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                Logger.Error(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Logger.Error(other.Message);
            }
        }

        private void ExportToSpreadsheet(object sender, RoutedEventArgs args)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            // Create new empty workbook.
            ExcelFile workbook = new ExcelFile();

            // Add new sheet and add headers to spreadsheet
            ExcelWorksheet worksheet = workbook.Worksheets.Add("Collection");
            string[] headers = new string[] { "Title", "Format", "Status", "Cur Volumes", "Max Volumes", "Notes", "Staff" };
            worksheet.Rows["1"].Style.Font.Weight = ExcelFont.BoldWeight;
            for (int col = 0; col < headers.Length; col++)
            {
                worksheet.Cells[0, col].Value = headers[col];
            }

            // Add the users collection data to the spreadsheet
            for (int row = 0; row < MainWindowViewModel.Collection.Count; row++)
            {
                Models.Series curSeries = MainWindowViewModel.Collection[row];
                switch(MainWindowViewModel.MainUser.CurLanguage)
                {
                    case "Native":
                        worksheet.Cells[row + 1, 0].Value = curSeries.Titles[2];
                        worksheet.Cells[row + 1, 6].Value = curSeries.Staff[1].Replace(" | ", "\n");
                        break;
                    case "English":
                        worksheet.Cells[row + 1, 0].Value = curSeries.Titles[1];
                        worksheet.Cells[row + 1, 6].Value = curSeries.Staff[0].Replace(" | ", "\n");
                        break;
                    default:
                        worksheet.Cells[row + 1, 0].Value = curSeries.Titles[0];
                        worksheet.Cells[row + 1, 6].Value = curSeries.Staff[0].Replace(" | ", "\n");
                        break;
                }

                switch (curSeries.Status)
                {
                    case "Ongoing":
                        worksheet.Cells[row + 1, 2].Style.FillPattern.SetSolid(SpreadsheetColor.FromArgb(255, 163, 63)); // Orange
                        break;
                    case "Complete":
                        worksheet.Cells[row + 1, 2].Style.FillPattern.SetSolid(SpreadsheetColor.FromArgb(182, 238, 86)); // Green
                        break;
                    case "Cancelled":
                        worksheet.Cells[row + 1, 2].Style.FillPattern.SetSolid(SpreadsheetColor.FromArgb(254, 107, 95)); // Red
                        break;
                    case "Hiatus":
                        worksheet.Cells[row + 1, 2].Style.FillPattern.SetSolid(SpreadsheetColor.FromArgb(250, 218, 94)); // Yellow
                        break;
                    case "Coming Soon":
                        worksheet.Cells[row + 1, 2].Style.FillPattern.SetSolid(SpreadsheetColor.FromArgb(134, 135, 217)); // Blue
                        break;
                }

                worksheet.Cells[row + 1, 1].Value = curSeries.Format;
                worksheet.Cells[row + 1, 2].Value = curSeries.Status;
                worksheet.Cells[row + 1, 3].Value = curSeries.CurVolumeCount;
                worksheet.Cells[row + 1, 4].Value = curSeries.MaxVolumeCount;
                worksheet.Cells[row + 1, 5].Value = curSeries.SeriesNotes;
            }

            // Title
            worksheet.Columns["A"].SetWidth(40, LengthUnit.CharacterWidth);
            worksheet.Columns["A"].Style.WrapText = true;
            worksheet.Columns["A"].Style.VerticalAlignment = VerticalAlignmentStyle.Center;

            // Format
            worksheet.Columns["B"].AutoFit();
            worksheet.Columns["B"].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            worksheet.Columns["B"].Style.VerticalAlignment = VerticalAlignmentStyle.Center;

            // Status
            worksheet.Columns["C"].AutoFit();
            worksheet.Columns["C"].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            worksheet.Columns["C"].Style.VerticalAlignment = VerticalAlignmentStyle.Center;

            // Cur Volumes
            worksheet.Columns["D"].AutoFit();
            worksheet.Columns["D"].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            worksheet.Columns["D"].Style.VerticalAlignment = VerticalAlignmentStyle.Center;

            // Max Volumes
            worksheet.Columns["E"].AutoFit();
            worksheet.Columns["E"].Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            worksheet.Columns["E"].Style.VerticalAlignment = VerticalAlignmentStyle.Center;

            // Notes
            worksheet.Columns["F"].SetWidth(40, LengthUnit.CharacterWidth);
            worksheet.Columns["F"].Style.WrapText = true;
            worksheet.Columns["F"].Style.VerticalAlignment = VerticalAlignmentStyle.Center;

            // Staff
            worksheet.Columns["G"].AutoFit();
            worksheet.Columns["G"].Style.VerticalAlignment = VerticalAlignmentStyle.Center;
    
            workbook.Save(@$"TsundokuCollection.xlsx");
            Logger.Info(@$"Exported {MainWindowViewModel.MainUser.UserName}'s Data To -> TsundokuCollection.xlsx");
        }
    }
}
