# Plan: Bulk edit

## Problem

Every change happens one series at a time: a card's +/− buttons or the edit window. Recategorizing
20 series (a publisher rename, marking a run as Finished, adding a genre, favoriting a shelf) means
20 edit-window round trips.

## Current state

- The main window hosts cards in an `ItemsRepeater` (`Views/MainWindow.axaml:642-659`). It has no
  selection model.
- The only notion of a "current" card is the static hovered card
  (`SeriesCardDisplay.axaml.cs:184-197`), which Ctrl+E and Ctrl+R use.
- `SeriesPickerWindow` is a multi-select `ListBox` of titles, used only by Refresh Series. It maps
  selections back to series **by title string** (`SeriesPickerWindow.axaml.cs:64-89`), so two
  series with the same title collide.

## Goal

Select several cards in the main window and apply one change to all of them, with one-step undo.

## Selection

- `MainWindowViewModel` holds a `HashSet<Guid> SelectedSeriesIds` and an
  `IObservable<int> SelectionCount`.
- `Series` gets a `[JsonIgnore][Reactive] bool IsSelected`, so the card can bind its selected style
  without a lookup. The view model keeps the flag and the set in sync.
- Input:
  - **Ctrl+click** a card toggles it.
  - **Shift+click** selects the range between the last clicked card and this one, in the current
    sort order.
  - **Ctrl+A** selects every card currently visible. Today Ctrl+A goes to the search box, so it
    only applies while the collection pane has focus.
  - **Esc** clears the selection.
  - Plain clicks keep their current behaviour (open link, copy title).
- Selected style: a 3px border in `TsundokuSeriesProgressBarColor` and a small check badge on the
  cover. No new theme keys.
- When a filter, search or shelf hides selected cards, they stay selected. The bar says
  `12 selected (3 hidden)` so nothing changes off-screen without notice.

## Selection bar

A bar docked at the bottom of the main window appears while the selection is non-empty:

```
12 selected (3 hidden)  │ Status ▾ │ Format ▾ │ Demographic ▾ │ Publisher… │ Genres… │ ★ Favorite ▾ │ Rating… │ Mark Complete │ Mark All Read │ Refresh │ 🗑 Trash │ ✕
```

- Dropdowns apply immediately (Status, Format, Demographic, Favorite on/off).
- Publisher and Rating open a small flyout with one input and Apply.
- Genres opens a flyout of genre checkboxes with three states: all selected series have it, some
  have it (indeterminate), none have it. Applying changes only the genres you touched, so
  unrelated genres stay as they are on each series.
- Mark Complete sets cur = max, and skips series with max = 0, reporting how many it skipped.
  Mark All Read sets VolumesRead = CurVolumeCount.
- Refresh reuses the cancellable loading dialog from `UserSettingsViewModel.RefreshAllCoversAsync`
  (:417-434).
- Trash asks for confirmation once for the whole selection, then moves the series to the trash,
  where each can be restored.

## Applying changes and undo

- A new `BulkEditService`:
  `Apply(IReadOnlyCollection<Series>, Action<Series>, string description)`.
  1. Captures a `SeriesSnapshot` (`Helpers/SeriesSnapshot.cs`) for every target.
  2. Applies the change through the existing `Series` mutators, so the current validation rules
     still apply.
  3. Calls `SaveUserData()` once.
  4. Pushes `(description, snapshots)` onto a short undo stack (depth 5).
- After an apply, a toast shows "Set status to Finished on 12 series · **Undo**". Ctrl+Z also undoes
  the last bulk edit while the main window has focus.
- `Genres` is a `HashSet` on a reactive property. Changing it in place raises no change
  notification, so bulk genre edits assign a new set.
- Source-generated logging in `BulkEditServiceLogging.cs`: `BulkEditApplied` (Information),
  `BulkEditUndone` (Information), `BulkEditSeriesSkipped` (Warning, with the reason).

## Also fix

- `SeriesPickerWindow` maps selections by title. Switch it to `Series.Id`, so Refresh Series and
  any future picker-based bulk action stop colliding on duplicate titles.

## Steps

1. `IsSelected`, the selection set, input handling and selected card style.
2. Selection bar, with only Status, Favorite and Trash at first.
3. `BulkEditService`, the undo stack and the toast.
4. The remaining actions: Format, Demographic, Publisher, Genres (three-state), Rating, Mark
   Complete, Mark All Read, Refresh.
5. Change `SeriesPickerWindow` to map by Id.
6. Add the shortcuts to the CONTROLS list in Settings, and update the README.

## Open questions

- Should **Value** be bulk-editable? Setting one total on many series is rarely right. Better to
  leave it out until the budget feature tracks per-purchase amounts.
- Should Ctrl+click become selection, given that plain click on the cover opens the series page?
  The plan keeps plain click as it is, so nothing existing changes.

## Effort

Medium. The selection plumbing on an `ItemsRepeater` is the fiddly part. The actions reuse existing
mutators.
