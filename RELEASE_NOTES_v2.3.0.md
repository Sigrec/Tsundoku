# Tsundoku v2.3.0

## Changes

- Smart Shelves: save advanced-filter expressions as named chips above the collection — click to apply, click again to deselect, right-click to rename or delete
- Add Series accepts AniList and MangaDex URLs — paste a link to auto-fill the series ID
- Add Series title field has a fixed height and scrolls on overflow instead of growing
- Edit Series header lists every saved title (Romaji, English, Japanese, Native, etc.) on its own row, wrapping cleanly
- Edit Series header now has an icon toolbar: previous/next series navigation, open series link, favorite, refresh, save, mark complete, delete
- Price Analysis: added All Star Comics (AU), Kings Comics (AU), OK Comics (UK), and MangaMart (US); removed SpeedyHen (retired upstream)
- Price Analysis: unreachable sites are now skipped immediately instead of timing out, and per-site failures no longer abort the scrape
- Closing the main window now reliably exits the app when secondary windows are open
