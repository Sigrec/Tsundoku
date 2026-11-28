# Plan: Budget

## Problem

`Series.Value` is one total per series, and nothing in the model has a date
(`TrashedSeries.DeletedAtUtc` is the only `DateTime`). The app knows what the collection is worth,
but not **when** money was spent, so it can't answer "how much did I spend this month?". A budget
needs that, so this feature starts with a purchase log.

## Goal

- Set a monthly spending target.
- Log purchases quickly.
- See this month's spend against the target, plus a 12-month history.
- See what it would cost to finish the series you're collecting.

## Data model

```
User.Purchases : List<Purchase>
User.MonthlyBudget : decimal?      // null = budgeting off

Purchase
  Guid     Id
  Guid     SeriesId                // may point at a trashed or deleted series; keep the entry
  DateOnly Date
  uint     VolumeCount             // or the volume numbers, once owned-volume tracking exists
  decimal  Amount                  // total for this purchase, in Currency
  string   Currency                // the user's currency when logged
  string?  Store                   // free text with autocomplete from previous entries
  string?  Note
```

- `Purchase` goes in its own file. Schema 6.8 or 6.9, depending on order with owned-volume
  tracking, adds `Purchases: []` and `MonthlyBudget: null`.
- `Series.Value` stays the user-controlled total. Logging a purchase **adds** the amount to `Value`
  by default (a checkbox in the dialog), so the two stay consistent without forcing every value
  change to have a purchase.
- **Currency:** each entry records its currency. Totals convert entries in another currency through
  the existing `ICurrencyRateService`, and are marked approximate when they do.

## Logging purchases

- **Log Purchase dialog:** opened from the card (a menu entry next to Edit), the edit window, or
  bulk edit's selection bar. It has series (prefilled), volumes, amount, date (today), store and
  note. On save it:
  - appends the purchase
  - adds the volumes to the series (+N, or the specific volume numbers)
  - adds the amount to `Value` when that box is checked
- **The card's + button stays one click and free.** It records nothing. Silent purchase records
  without amounts would make the budget wrong without any sign of it.
- An optional setting, "Ask for price when adding a volume", turns + into a small inline flyout
  with an amount box and Skip, for users who want every volume logged.
- **Backfill:** a purchase with a past date covers buying before this feature existed. No
  automatic backfill from `Value`, because there's no date to attach it to.

## Budget view

Add a Budget tab or section to Collection Stats (`Views/CollectionStatsWindow.axaml`, which already
uses LiveChartsCore):

- **This month:** progress bar of spent / budget, the remaining amount, and days left. The bar uses
  the theme's progress color, and switches to the divider or warning style when over.
- **Last 12 months:** bar chart of monthly spend with a budget line.
- **Breakdown:** spend by store and by publisher, this year and all time.
- **Cost to complete:** for each series with missing volumes, missing × average paid per volume
  (`Value / CurVolumeCount`, or the average purchase amount when there is purchase history), plus a
  total. Sorted so you can see which series are cheapest to finish.
- **Purchase history:** a list you can edit or delete, filterable by month or series. It's the
  only place to fix a mistyped amount.

## Alerts

- The main window's notification text (`MainWindowViewModel.NotificationText`) shows
  "Budget: $12 left this month" after a logged purchase, and a warning when the budget is
  exceeded. Nothing pops up and nothing blocks.

## Logging

`BudgetServiceLogging.cs`: `PurchaseLogged` (Information), `PurchaseEdited`, `PurchaseDeleted`,
`BudgetExceeded` (Information, since it's user state rather than an app problem), and
`CurrencyConversionFailed` (Warning, when the total falls back to unconverted).

## Steps

1. `Purchase` model, `User.Purchases` and `MonthlyBudget`, the migration, and a `BudgetService`
   holding the aggregation logic, with unit tests (month boundaries, mixed currencies, deleted
   series).
2. Log Purchase dialog from the card and the edit window.
3. Budget section: this month and the 12-month chart.
4. Purchase history list with edit and delete.
5. Breakdowns, and cost to complete (better once owned-volume tracking lands).
6. The optional "ask for price on +" setting.
7. README and changelog.

## Dependencies and overlap

- **Activity history (feature #1)** and the purchase log are the same kind of data: dated events
  per series. If both are built, build one `CollectionEvent` log with a `Purchase` kind instead of
  two separate lists. This plan's model can become that event type.
- **Owned-volume tracking** makes "cost to complete" exact. Without it, the estimate uses
  `MaxVolumeCount - CurVolumeCount`.

## Effort

Medium. The data and aggregation are simple. The UI (dialog, budget section, history list) is the
work.
