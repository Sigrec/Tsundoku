using ReactiveUI.SourceGenerators;

namespace Tsundoku.ViewModels;

public sealed partial class LoadingDialogViewModel(IUserService userService) : ViewModelBase(userService)
{
    [Reactive] public partial string StatusText { get; set; } = "Loading, Please Wait";
    [Reactive] public partial bool IsLoadingIndeterminate { get; set; } = false;
    [Reactive] public partial uint ProgressValue { get; set; } = 0;
    [Reactive] public partial int ProgressMaximum { get; set; } = 100;
}