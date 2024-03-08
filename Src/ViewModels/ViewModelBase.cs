using System.Diagnostics;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        [Reactive] public TsundokuTheme CurrentTheme { get; set; }
        [Reactive] public string CurCurrency { get; set; }
        [Reactive] public string UserName { get; set; }
        [Reactive] public string CurDisplay { get; set; }
        [Reactive] public string CurFilter { get; set; }
        [Reactive] public Region CurRegion { get; set; }
        public static string Filter { get; set; }
        public static User? MainUser { get; set; }
        public static bool updatedVersion = false;
        public static bool newCoverCheck = false;
        public static bool isReloading = false;
        public const string TsundokoCurVersion = "4.0.0.0";
        public const double SCHEMA_VERSION = 4.0;
        public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.66 Safari/537.36";

        public ViewModelBase()
        {
            CurFilter = "None";
        }

        public static void OpenSiteLink(string link)
        {
            LOGGER.Debug($"Opening Link {link}");
            try
            {
                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch (Exception other)
            {
                LOGGER.Error(other.Message);
            }
        }

    }
}
