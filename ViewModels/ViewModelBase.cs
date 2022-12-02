using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        [Reactive]
        public TsundokuTheme CurrentTheme { get; set; }

        public ViewModelBase()
        {
            
        }

    }
}
