using System.Diagnostics;
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
        public static User? MainUser { get; set; }
        public static bool updatedVersion = false;

        public static readonly JsonSerializerOptions options = new()
        { 
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = true,
        };

        public ViewModelBase()
        {
            
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
