# Plan: Track which volumes are owned

## Problem

`Series` stores only `CurVolumeCount` (a number). Owning volumes 1–3 and 5 looks the same as
owning 1–4, so the app can't show gaps, build an accurate shopping list, or tell Price Analysis
which volumes are actually missing.

## Goal

Store the specific volume numbers owned, show gaps on the card and in the edit window, and keep
everything that reads `CurVolumeCount` today working unchanged.

## Data model

- Add `Series.OwnedVolumes`: a sorted `List<uint>` of owned volume numbers, serialized as a JSON
  array (`"OwnedVolumes": [1, 2, 3, 5]`). A series rarely has more than ~100 volumes, so a plain
  list is small and readable in `UserData.json`.
- `CurVolumeCount` stays as a property, so filters, sorting, stats, the CSV export and the advanced
  query (`CurVolumes>=`) don't change. It becomes **derived**: every change goes through `Series`
  methods that update `OwnedVolumes` and then set `CurVolumeCount = OwnedVolumes.Count`.
- Add a computed `[JsonIgnore]` property `MissingVolumes`: the numbers in `1..MaxVolumeCount` that
  aren't owned. When `MaxVolumeCount == 0` (unknown or ongoing), only the gaps below the highest
  owned volume count as missing.
- `Series.cs:647-720` holds `Increment`/`Decrement`/`Update*VolumeCount`. They're rewritten over
  `OwnedVolumes`:
  - **+ on the card** adds `max(owned) + 1`. This matches today's behaviour for anyone who owns a
    contiguous run.
  - **− on the card** removes the highest owned volume.
  - New `ToggleVolume(uint)` and `SetOwnedVolumes(IEnumerable<uint>)` for the edit window.
  - `UpdateMaxVolumeCount` may no longer go below the highest owned volume. That replaces today's
    `newMax >= Cur` check.

### Special volumes (18.5, 0, 0.5 …)

- Store volume numbers as `decimal`, not `uint`, so `OwnedVolumes` is `List<decimal>`
  (`[1, 2, …, 18, 18.5, 19]`). `decimal` keeps 18.5 exact, where `double` could drift.
- Add `Series.SpecialVolumes: List<decimal>`: the non-whole numbers this series has, plus 0.
  Users add these by hand, since AniList and MangaDex don't report them. A special that's in
  `SpecialVolumes` but not in `OwnedVolumes` shows as missing.
- **Specials sit outside the main count.** `CurVolumeCount` counts only whole volumes
  `1..MaxVolumeCount`, so `cur <= max`, the progress bar, the Complete filter and the completion
  pulse all keep working. The card reads `20 / 20 +1`, and stats can add an "Owned specials" tile.
- **Complete means the main run.** A missing special doesn't make a series incomplete. It's
  listed separately: `Missing 47, 83 · special 47.5`.
- **Display:** specials are drawn half-width in the card strip and as dashed chips in the edit
  grid, placed right after the volume they follow (18.5 after 18). Volume 0 goes first.
- **Range box:** accepts decimals as single entries (`1-46, 48-100, 18.5`). Typing a new decimal
  there adds it as a special. Ranges stay whole-number only.
- Named extras with no number, like fanbooks, are out of scope.

## Migration (schema 6.8)

- For each series without `OwnedVolumes`, write `[1..CurVolumeCount]`. This assumes existing
  collections are contiguous, which is the only reading today's data allows.
- Imports (Libib, Goodreads, Json) go through the same migration path, so they get the same fill.
- `SeriesSnapshot` (`Helpers/SeriesSnapshot.cs`) adds `OwnedVolumes` (copied), so edit-window
  revert and reapply cover it.

## UI

### Series card (`Controls/SeriesCardDisplay.axaml:293-342`)

- The progress bar becomes a segmented strip, one segment per volume, when `MaxVolumeCount <= 60`.
  Owned segments are filled, missing ones are hollow, so gaps are visible at a glance. Above 60, or
  when max is unknown, keep the solid bar and add small tick marks at gap positions.
- The overlay text stays `8 / 12`. When there are gaps it becomes `8 / 12 · missing 4, 9`, with
  long lists truncated (`missing 4, 9, +3`).
- The completion pulse (`SeriesCardDisplay.axaml.cs:225-251`) fires when `MissingVolumes` becomes
  empty, instead of when the count equals max.

### Edit window (`Views/EditSeriesInfoWindow.axaml`)

- Replace the `CurVolumeMaskedTextBox` with a **volume grid**: a `WrapPanel` of small toggle chips
  numbered 1..max. Click toggles one volume, Shift+click toggles a range, and there are
  "All" / "None" buttons.
- When `MaxVolumeCount` is 0, show chips up to `max(owned) + 5` with a "+" chip to extend.
- `MaxVolumeMaskedTextBox` stays. Raising max adds hollow chips, and lowering it below an owned
  volume is refused with an inline message. Today that failure is only a silent `LOGGER.Warn`.
- Read status stays a count (`VolumesRead`). Volume-level read tracking is a separate follow-up.

### Finding gaps

- A new simple filter, "Has Gaps", in `TsundokuFilterModel`.
- A new advanced-query key `Missing` (numeric: `Missing>0`, `Missing>=2`) in
  `SharedSeriesCollecitonProvider.cs:254-378`, plus a chip in `FilterChipViewModel`.
- **Shopping list:** a "Copy Missing Volumes" quick action that copies text like
  `Frieren — 4, 9` for every series with gaps (or the current shelf), ready to paste into a store
  search or a note.

### Price Analysis (follow-up)

- Highlight results whose volume number is in `MissingVolumes`. This depends on how reliably the
  scraper's entry titles expose a volume number, so it's out of scope for the first version.

## Stats

`NumVolumesCollected` and `CompletionRate` (`CollectionStatsViewModel.cs:318-355`) keep summing
`CurVolumeCount`, so they don't change. Add a "Missing Volumes" tile: the total gaps across the
collection.

## Steps

1. Add the model, the derived `CurVolumeCount`, `MissingVolumes`, the new mutators and the 6.8
   migration. Add unit tests for the migration and the mutators. The tests need no AniList calls.
2. Include `OwnedVolumes` in `SeriesSnapshot`.
3. Card: segmented strip, gap text, completion pulse condition.
4. Edit window: volume grid and max-volume validation message.
5. "Has Gaps" filter, `Missing` query key, Missing Volumes stats tile.
6. Copy Missing Volumes quick action.
7. README and changelog.

## Risks and open questions

- **Box sets** aren't volume numbers. Track them through notes for now.
- **Omnibus and 3-in-1 editions** map one physical book to several volume numbers. The grid can
  cover this by toggling a range, but whether `CurVolumeCount` should then count books or volumes
  needs a decision. The plan counts volume numbers.
- Segment rendering on 500+ cards: draw the strip in one custom control with `Render`, not 60
  `Border`s per card, so scrolling stays smooth.

## Effort

Medium–large. Most of the work is the card strip and the edit-window grid. The model change and
migration are small.
