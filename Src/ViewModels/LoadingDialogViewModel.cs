using ReactiveUI.SourceGenerators;

namespace Tsundoku.ViewModels;

public sealed class LoadingDialogViewModel(IUserService userService) : ViewModelBase(userService)
{
    [Reactive] public string StatusText { get; set; } = "Loading, Please Wait";
    [Reactive] public bool IsLoadingIndeterminate { get; set; } = false;
    [Reactive] public uint ProgressValue { get; set; } = 0;
    [Reactive] public int ProgressMaximum { get; set; } = 100;
}