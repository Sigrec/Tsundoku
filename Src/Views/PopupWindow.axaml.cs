using Avalonia.ReactiveUI;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public sealed partial class PopupWindow : ReactiveWindow<PopupWindowViewModel>
{
    public PopupWindow(PopupWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        Closing += (s, e) => { ViewModel.ResetPopupInfo(); };
    }

    public void SetWindowText(string title, string icon, string infoText)
    {
        ViewModel.SetPopupInfo(title, icon, infoText);
    }
}