using Avalonia.ReactiveUI;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class PopupDialog : ReactiveWindow<PopupDialogViewModel>
{
    public PopupDialog()
    {
        InitializeComponent();

        Closing += (s, e) => { ViewModel.ResetPopupInfo(); };
    }
}