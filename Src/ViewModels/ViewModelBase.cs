using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        [Reactive] public TsundokuTheme CurrentTheme { get; set; }
        [Reactive] public string CurCurrency { get; set; }
        public static User MainUser { get; set; }

        public static readonly JsonSerializerOptions options = new()
        { 
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        public ViewModelBase()
        {
            
        }

        public static void OpenSiteLink(string link)
        {
            Constants.Logger.Info($"Opening Link {link}");
            try
            {
                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch (Exception other)
            {
                Constants.Logger.Error(other.Message);
            }
        }

    }
}
