using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Tsundoku.ViewModels;

namespace Tsundoku.Views;

public partial class PopupWindow : ReactiveWindow<PopupWindowViewModel>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
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