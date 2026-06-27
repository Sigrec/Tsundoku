using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Tsundoku.Models;

/// <summary>
/// A user-named saved filter expression that appears as a one-click chip in the main window.
/// </summary>
public sealed partial class SavedShelf : ReactiveObject
{
    [Reactive] public partial string Name { get; set; } = string.Empty;
    [Reactive] public partial string Query { get; set; } = string.Empty;
    [Reactive] public partial string IconKey { get; set; } = "fa7-solid fa7-bookmark";
    public Guid Id { get; set; } = Guid.NewGuid();
}
