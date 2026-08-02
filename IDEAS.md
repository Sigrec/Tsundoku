# Tsundoku — Feature Backlog

A running list of ideas for future versions, grouped by rough effort. Not a commitment — a menu.

Each item lists a **one-line description** and a **rough scope**. Anything marked ❌ is a considered-and-declined idea with the reason preserved so we don't re-litigate it.

---

## 🟢 Quick wins (< 1 day each)

Small, self-contained additions that don't touch data model or shipping surface much.

- **Right-click context menu on series cards** — Copy Title / Copy Link / Edit / Delete in one place. Currently only the card overlay exposes some of these; a right-click menu is the standard Windows discoverability path.
- ✅ **Persist last-active Smart Shelf across sessions** *(shipped)* — `User.LastSelectedShelfId` saved on shelf change; restored on startup via a one-shot `WhenAnyValue(CurrentUser)` subscription. Schema v6.5.
- ✅ **Random picker weighted toward near-complete series** *(shipped)* — toggle in Preferences (`PreferNearlyCompleteSeriesPick`). Weight = `1.0 / remaining` (1-behind → 1.0, 2 → 0.5, 10 → 0.1). Prior pick weight quartered to soften repeats.
- **Random picker recent-picks history** — sidebar showing the last 5–10 rolls; click one to jump back to it. Zero cost, mild delight.
- ✅ **Keyboard shortcuts** *(shipped)* — `Ctrl+N` opens Add Series, `Ctrl+E` opens Edit for the hovered card, `Ctrl+R` refreshes the hovered card. `SeriesCardDisplay` tracks the hovered instance in a static field so window-level KeyDown can target it. Tooltips on Add Series and the card's Edit button mention the shortcuts.
- **Copy full stats to clipboard as a formatted block** — a button in Collection Stats that copies "Mean Rating: 7.4 · Volumes: 812 · Collected: 96%" for pasting into Discord/reddit.
- ✅ **Series card badge for favorites** *(shipped)* — 26px gold-star badge on the top-right of the cover, bound to `Series.IsFavorite`. `IsHitTestVisible="False"` so it doesn't intercept clicks.
- ✅ **Empty-state message on the collection view** *(shipped)* — `IsFilteredEmpty` reactive property + magnifying-glass icon + "No series match this filter" + "Clear filter" button (calls `ClearAllFilters()` which resets search, advanced query, filter enum, publisher, shelf, and FilterBuilder chips).
- **Escape closes any modal** — audit each secondary window; add `Esc → Close` handler if missing (Add Series, Edit Series, Theme Settings, etc.).
- **Copy stat block from Collection Stats card** — one-click "Copy formatted stats" that grabs the whole stats block as clipboard text.

---

## 🟡 Meaningful features (~few days to a week)

Bigger scope — new views, new data model fields, or cross-cutting changes.

- **Wishlist / Want-to-Buy list** — a second collection scope for series you don't own yet. Reuses the existing Series model + all search/filter/Smart Shelves machinery. Pairs with Price Analysis ("show me wishlist items under $X"). Requires: new `IsWishlist` flag on Series or a separate collection, toggle in the main view.
- **Bulk edit mode** — multi-select cards (Ctrl+click) → floating toolbar → set Rating / Publisher / Demographic / Refresh Cover across the selection in one shot. Pain scales with collection size.
- **Custom tags / labels per series** — user-defined free-form tags (e.g. "birthday gift", "borrowed from library", "TODO reread"). Tags become Smart Shelf criteria automatically.
- **Reading-progress chart over time** — extends Collection Stats with "volumes added per month" line chart and "total value over time" chart. Data already aggregated; just new LiveCharts visualizations.
- **Duplicate detector utility** — scan the collection for near-matches (same series stored under two AniList IDs, or imports that doubled up). Surfaces as a Settings utility with per-pair "Keep A / Keep B / Merge" actions.
- **Series completion history / timeline** — record the date each series was marked complete. Show a small timeline in Collection Stats ("Latest completions: X, Y, Z").
- **Publisher/author aggregate stats** — "Top 5 publishers by volume count", "Most collected authors" — bar charts in Collection Stats.
- ✅ **Series notes markdown support** *(shipped)* — `Preview` toggle next to the notes header switches between the `TextBox` and a `SelectableTextBlock` that renders **bold**, *italic*, `code`, [text](url) (URL shown inline in muted gray), and `-`/`*` bullet lists. Custom in-tree `MarkdownRenderer.ApplyTo` — no new dependency.
- ✅ **Multi-currency total-value display** *(shipped)* — Preferences has a "Secondary Currency" dropdown (35 currencies + Off) and a "Refresh Rates" button. Collection Stats shows "≈ {symbol}{amount}" under the primary total when secondary is set. `CurrencyRateService` ships hardcoded USD-base rates for all 35 supported currencies as fallback, and `RefreshAsync()` pulls live rates from `open.er-api.com/v6/latest/USD` (no API key). Schema v6.6 added `User.SecondaryCurrency`.
- **"This day" callout** — small header banner: "You added *Berserk* on this day 2 years ago." Runs off `Series.Id`'s GUID timestamp or a dedicated `AddedDate` field (new).
- **Cover drag-and-drop** — drag an image file onto a card → replace the cover. Currently only via Edit Series URL/file picker.

---

## 🟠 Bigger bets (multi-week)

Real architectural work; consider prioritization carefully.

- **Cloud backup (OneDrive/Dropbox)** — the README's own "if you lose UserData.json your library is gone" warning is the giveaway. OneDrive integration on Windows is one `KnownFolders` call but syncing state, conflict resolution, and merge logic are nontrivial.
- **Mobile companion app** — read-only view of collection over local network. User asked about this earlier; would need a shared JSON schema and either a local WebSocket server or a small Blazor Server companion.
- ✅ **Trash / recycle bin for deleted series** *(shipped)* — Deleting a series now soft-deletes into a `TrashedSeries` list on the user. A dedicated `TrashWindow` (opened from Settings → Trash) lists each entry with its title, format, and deletion date, plus Restore / Delete Forever buttons and an Empty Trash action. Expired entries (>30 days, `UserService.TrashRetentionDays`) are auto-purged on startup. Cover files preserved on trash, deleted on permanent purge. Schema v6.6 covers both this and SecondaryCurrency.
- **Series merge tool** — combine two entries (e.g., two different volumes of the same series accidentally imported as separate entries). Requires field-by-field merge UI.
- ✅ **Undo/redo across edits** *(shipped)* — scoped to the Edit Series window (not global). Two new toolbar buttons: **Revert** (`fa7-rotate-left`) restores every editable field to the snapshot taken when the window opened or the last Save; **Reapply** (`fa7-rotate-right`) is a one-shot redo that brings the reverted changes back. Snapshot lives in `Src/Helpers/SeriesSnapshot.cs` (an immutable record covering Max/Cur/VolumesRead/Value/Rating/Publisher/Format/Status/Demographic/Notes/Genres). Prev/Next navigation resets both endpoints so the actions always target the currently-open series. Global undo across the app was tried and removed — deliberate scope decision.
- **Slideshow / screensaver mode** — full-screen random cover cycler for showing off the collection. Reuses the picker's cover-loading path.
- **Barcode scan via webcam (ISBN lookup)** — needs a camera abstraction + an ISBN → AniList/MangaDex resolver. Rarely used but very cool.

---

## ❌ Considered and declined

Preserved so we don't re-visit.

- **Release calendar (with AniList as data source)** — AniList doesn't expose per-volume release dates for manga; only series-level `startDate`/`endDate` (mostly in the past for tracked series). The calendar would be empty for most users. Would need MangaDex chapter dates or a volume-count-delta detector to be useful, both of which have their own accuracy/effort costs. Revisit if a better data source appears.

---

## 📌 Notes on process

- Order within a section is roughly recommended priority (top = higher value / lower cost).
- Anything that touches `Series` shape needs a `SCHEMA_VERSION` bump in `ViewModelBase.cs` and a migration in `Models/User.cs`.
- Anything user-visible should get a bullet in `Src/Assets/changelog.json` when shipped.
- Keep the picker's dice icon consistent — if we add sibling actions to the toolbar (e.g., Bulk Edit), use complementary icons to avoid visual clutter.
