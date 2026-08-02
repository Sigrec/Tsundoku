using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.Services;

namespace Tsundoku.ViewModels;

public sealed partial class TrashViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    public ReadOnlyObservableCollection<TrashedSeries> Trash { get; }
    public int RetentionDays { get; }
    [Reactive] public partial int TrashCount { get; set; }

    public TrashViewModel(IUserService userService) : base(userService)
    {
        Trash = _userService.Trash;
        RetentionDays = _userService.TrashRetentionDays;

        // Keep the header count in sync as items are added/removed.
        if (Trash is System.Collections.Specialized.INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += (_, _) => TrashCount = Trash.Count;
        }
        TrashCount = Trash.Count;
    }

    public void Restore(Guid seriesId) => _userService.RestoreSeries(seriesId);
    public void Purge(Guid seriesId) => _userService.PurgeSeries(seriesId);
    public void EmptyTrash() => _userService.EmptyTrash();
}
