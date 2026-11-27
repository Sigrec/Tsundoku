using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.Models.Enums;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using MsLogger = Microsoft.Extensions.Logging.ILogger;

namespace Tsundoku.ViewModels;

/// <summary>
/// Holds the main window's multi-card selection and applies one change to every selected series,
/// with a short undo stack.
/// </summary>
public sealed partial class BulkEditViewModel : ReactiveObject, IDisposable
{
    private const int MaxUndoDepth = 5;

    private readonly IUserService _userService;
    private readonly ReadOnlyObservableCollection<Series> _visibleSeries;
    private readonly MsLogger _logger;
    private readonly HashSet<Guid> _selectedIds = [];
    private readonly LinkedList<BulkEditOperation> _undoStack = new();
    private readonly CompositeDisposable _disposables = [];
    private Guid? _rangeAnchorId;

    [Reactive] public partial int SelectedCount { get; set; }
    [Reactive] public partial int HiddenCount { get; set; }
    [Reactive] public partial bool HasSelection { get; set; }
    [Reactive] public partial bool CanUndo { get; set; }
    [Reactive] public partial string SelectionSummary { get; set; } = string.Empty;
    [Reactive] public partial string UndoToolTip { get; set; } = string.Empty;

    public IReadOnlyList<SeriesDemographic> Demographics { get; } = Enum.GetValues<SeriesDemographic>();
    public ImmutableArray<string> Genres { get; } = SERIES_GENRES;

    public BulkEditViewModel(IUserService userService, ISharedSeriesCollectionProvider sharedSeriesProvider, ILogger<BulkEditViewModel> logger)
    {
        _userService = userService;
        _visibleSeries = sharedSeriesProvider.DynamicUserCollection;
        _logger = logger;

        ((INotifyCollectionChanged)_visibleSeries).CollectionChanged += OnVisibleSeriesChanged;
        Disposable.Create(() => ((INotifyCollectionChanged)_visibleSeries).CollectionChanged -= OnVisibleSeriesChanged)
            .DisposeWith(_disposables);

        // A series that leaves the collection (trash, delete, import) can't stay selected.
        _userService.UserCollectionChanges
            .Subscribe(changes =>
            {
                bool removedAny = false;
                foreach (Change<Series, Guid> change in changes)
                {
                    if (change.Reason == ChangeReason.Remove && _selectedIds.Remove(change.Key))
                    {
                        change.Current.IsSelected = false;
                        removedAny = true;
                    }
                }
                if (removedAny)
                {
                    UpdateSelectionState();
                }
            })
            .DisposeWith(_disposables);
    }

    #region Selection

    /// <summary>Adds or removes one series from the selection and makes it the range anchor.</summary>
    public void Toggle(Series series)
    {
        SetSelected(series, !_selectedIds.Contains(series.Id));
        _rangeAnchorId = series.Id;
        UpdateSelectionState();
    }

    /// <summary>
    /// Selects every visible series between the range anchor and <paramref name="series"/>, in the
    /// current sort order. Without an anchor this behaves like <see cref="Toggle"/>.
    /// </summary>
    public void SelectRange(Series series)
    {
        int targetIndex = _visibleSeries.IndexOf(series);
        int anchorIndex = _rangeAnchorId is Guid anchorId ? IndexOfVisible(anchorId) : -1;
        if (targetIndex < 0 || anchorIndex < 0)
        {
            Toggle(series);
            return;
        }

        int start = Math.Min(anchorIndex, targetIndex);
        int end = Math.Max(anchorIndex, targetIndex);
        for (int i = start; i <= end; i++)
        {
            SetSelected(_visibleSeries[i], true);
        }
        UpdateSelectionState();
    }

    /// <summary>Selects every series the current filter, search, or shelf shows.</summary>
    public void SelectAllVisible()
    {
        foreach (Series series in _visibleSeries)
        {
            SetSelected(series, true);
        }
        UpdateSelectionState();
    }

    /// <summary>Clears the selection, including series hidden by the current filter.</summary>
    public void Clear()
    {
        foreach (Series series in ResolveSelected())
        {
            series.IsSelected = false;
        }
        _selectedIds.Clear();
        _rangeAnchorId = null;
        UpdateSelectionState();
    }

    /// <summary>The selected series that are still in the collection, in collection order.</summary>
    public List<Series> GetSelectedSeries() => ResolveSelected();

    private void SetSelected(Series series, bool selected)
    {
        if (selected)
        {
            _selectedIds.Add(series.Id);
        }
        else
        {
            _selectedIds.Remove(series.Id);
        }
        series.IsSelected = selected;
    }

    private int IndexOfVisible(Guid id)
    {
        for (int i = 0; i < _visibleSeries.Count; i++)
        {
            if (_visibleSeries[i].Id == id)
            {
                return i;
            }
        }
        return -1;
    }

    private List<Series> ResolveSelected()
    {
        List<Series> selected = [with(_selectedIds.Count)];
        if (_selectedIds.Count == 0)
        {
            return selected;
        }

        foreach (Series series in _userService.GetUserCollection())
        {
            if (_selectedIds.Contains(series.Id))
            {
                selected.Add(series);
            }
        }
        return selected;
    }

    private void OnVisibleSeriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_selectedIds.Count > 0)
        {
            UpdateSelectionState();
        }
    }

    private void UpdateSelectionState()
    {
        int visibleSelected = 0;
        if (_selectedIds.Count > 0)
        {
            foreach (Series series in _visibleSeries)
            {
                if (series.IsSelected)
                {
                    visibleSelected++;
                }
            }
        }

        SelectedCount = _selectedIds.Count;
        HiddenCount = _selectedIds.Count - visibleSelected;
        HasSelection = _selectedIds.Count > 0;
        SelectionSummary = HiddenCount > 0
            ? $"{SelectedCount} selected ({HiddenCount} hidden)"
            : $"{SelectedCount} selected";
        _logger.SelectionChanged(SelectedCount, HiddenCount);
    }

    #endregion

    #region Edits

    public string SetDemographic(SeriesDemographic demographic) =>
        Apply($"Set demographic to {demographic}", series =>
        {
            if (series.Demographic == demographic) return false;
            series.Demographic = demographic;
            return true;
        });

    public string SetPublisher(string publisher)
    {
        string trimmed = publisher.Trim();
        return trimmed.Length == 0
            ? "Enter a publisher first"
            : Apply($"Set publisher to {trimmed}", series =>
            {
                if (string.Equals(series.Publisher, trimmed, StringComparison.Ordinal)) return false;
                series.Publisher = trimmed;
                return true;
            });
    }

    public string AddGenre(string genreAlias)
    {
        SeriesGenre genre = SeriesGenreModel.Parse(genreAlias);
        return Apply($"Add genre {genreAlias}", series =>
        {
            if (series.Genres.Contains(genre)) return false;
            series.Genres = [.. series.Genres, genre];
            return true;
        });
    }

    public string RemoveGenre(string genreAlias)
    {
        SeriesGenre genre = SeriesGenreModel.Parse(genreAlias);
        return Apply($"Remove genre {genreAlias}", series =>
        {
            if (!series.Genres.Contains(genre)) return false;
            HashSet<SeriesGenre> remaining = [.. series.Genres];
            remaining.Remove(genre);
            series.Genres = remaining;
            return true;
        });
    }

    public string SetFavorite(bool isFavorite) =>
        Apply(isFavorite ? "Add to favorites" : "Remove from favorites", series =>
        {
            if (series.IsFavorite == isFavorite) return false;
            series.IsFavorite = isFavorite;
            return true;
        });

    public string SetRating(string ratingText)
    {
        if (!decimal.TryParse(ratingText.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out decimal rating)
            && !decimal.TryParse(ratingText.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out rating))
        {
            return "Enter a rating from 0 to 10";
        }
        if (rating is < 0m or > 10m)
        {
            return "Enter a rating from 0 to 10";
        }

        rating = decimal.Round(rating, 1);
        return Apply($"Set rating to {rating}", series =>
        {
            if (series.Rating == rating) return false;
            series.Rating = rating;
            return true;
        });
    }

    public string MarkComplete()
    {
        const string Description = "Mark complete";
        return Apply(Description, series =>
        {
            if (series.MaxVolumeCount == 0)
            {
                _logger.BulkEditSeriesSkipped(Description, series.Id, "max volume count is unknown");
                return false;
            }
            if (series.CurVolumeCount == series.MaxVolumeCount) return false;
            series.MarkMainRunComplete();
            return true;
        });
    }

    public string MarkAllRead() =>
        Apply("Mark all collected volumes read", series =>
        {
            if (series.VolumesRead == series.CurVolumeCount) return false;
            series.VolumesRead = series.CurVolumeCount;
            return true;
        });

    /// <summary>Moves every selected series to the trash. Undo restores them.</summary>
    public string Trash()
    {
        List<Series> targets = ResolveSelected();
        if (targets.Count == 0)
        {
            return "Nothing selected";
        }

        List<BulkEditUndoEntry> entries = [with(targets.Count)];
        foreach (Series series in targets)
        {
            entries.Add(new BulkEditUndoEntry(series.Id, SeriesSnapshot.Capture(series), series.IsFavorite));
            series.IsSelected = false;
            _userService.TrashSeries(series);
        }

        _selectedIds.Clear();
        _rangeAnchorId = null;
        PushUndo(new BulkEditOperation($"Move {targets.Count} series to trash", entries, WasTrash: true));
        _userService.SaveUserData();
        UpdateSelectionState();
        _logger.BulkTrashApplied(targets.Count);
        return $"Moved {targets.Count} series to the trash · Ctrl+Z to undo";
    }

    /// <summary>Reverts the most recent bulk edit.</summary>
    public string Undo()
    {
        if (_undoStack.First is not LinkedListNode<BulkEditOperation> node)
        {
            return "Nothing to undo";
        }

        BulkEditOperation operation = node.Value;
        _undoStack.RemoveFirst();

        if (operation.WasTrash)
        {
            foreach (BulkEditUndoEntry entry in operation.Entries)
            {
                _userService.RestoreSeries(entry.SeriesId);
            }
        }
        else
        {
            Dictionary<Guid, Series> byId = [];
            foreach (Series series in _userService.GetUserCollection())
            {
                byId[series.Id] = series;
            }
            foreach (BulkEditUndoEntry entry in operation.Entries)
            {
                if (byId.TryGetValue(entry.SeriesId, out Series? series))
                {
                    entry.Snapshot.ApplyTo(series);
                    series.IsFavorite = entry.IsFavorite;
                }
            }
        }

        _userService.SaveUserData();
        RefreshUndoState();
        _logger.BulkEditUndone(operation.Description, operation.Entries.Count);
        return $"Undid \"{operation.Description}\"";
    }

    private string Apply(string description, Func<Series, bool> change)
    {
        List<Series> targets = ResolveSelected();
        if (targets.Count == 0)
        {
            return "Nothing selected";
        }

        List<BulkEditUndoEntry> entries = [];
        foreach (Series series in targets)
        {
            BulkEditUndoEntry before = new(series.Id, SeriesSnapshot.Capture(series), series.IsFavorite);
            try
            {
                if (change(series))
                {
                    entries.Add(before);
                }
            }
            catch (Exception ex)
            {
                _logger.BulkEditSeriesFailed(description, series.Id, ex);
                before.Snapshot.ApplyTo(series);
                series.IsFavorite = before.IsFavorite;
            }
        }

        _logger.BulkEditApplied(description, entries.Count, targets.Count);
        if (entries.Count == 0)
        {
            return $"{description}: no changes needed";
        }

        PushUndo(new BulkEditOperation(description, entries, WasTrash: false));
        _userService.SaveUserData();

        int unchanged = targets.Count - entries.Count;
        string unchangedText = unchanged > 0 ? $" ({unchanged} unchanged)" : string.Empty;
        return $"{description} on {entries.Count} series{unchangedText} · Ctrl+Z to undo";
    }

    private void PushUndo(BulkEditOperation operation)
    {
        _undoStack.AddFirst(operation);
        while (_undoStack.Count > MaxUndoDepth)
        {
            _undoStack.RemoveLast();
        }
        RefreshUndoState();
    }

    private void RefreshUndoState()
    {
        CanUndo = _undoStack.Count > 0;
        UndoToolTip = _undoStack.First is LinkedListNode<BulkEditOperation> top
            ? $"Undo \"{top.Value.Description}\" (Ctrl+Z)"
            : string.Empty;
    }

    #endregion

    public void Dispose() => _disposables.Dispose();
}
