using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public partial class PopupWindow : ReactiveWindow<PopupWindowViewModel>
{
    public PopupWindow()
    {
        InitializeComponent();
        Closing += (s, e) => { ViewModel.ResetPopupInfo(); };
    }

    public void SetWindowText(string title, string icon, string infoText)
    {
        ViewModel.SetPopupInfo(title, icon, infoText);
    }
}