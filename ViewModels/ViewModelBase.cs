using System.Text.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        [Reactive]
        public TsundokuTheme CurrentTheme { get; set; }

        public static readonly JsonSerializerOptions options = new JsonSerializerOptions { 
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        public ViewModelBase()
        {
            
        }

    }
}
