# Tsundoku v2.3.0

## Changes

- Smart Shelves: save advanced-filter expressions as named chips above the collection — click to apply, click again to deselect, right-click to rename, or use the × on each chip to delete
- Smart Shelves: the active shelf is now visually highlighted so you can tell at a glance which filter is applied
- Add Series accepts AniList and MangaDex URLs — paste a link to auto-fill the series ID
- Add Series title field has a fixed height and scrolls on overflow instead of growing
- Edit Series header lists every saved title (Romaji, English, Japanese, Native, etc.) on its own row, wrapping cleanly
- Edit Series header now has an icon toolbar: previous/next series navigation, open series link, favorite, refresh, save, mark complete, delete
- Price Analysis: added All Star Comics (AU), Kings Comics (AU), OK Comics (UK), and MangaMart (US); removed SpeedyHen (retired upstream)
- Price Analysis: website list now filters to only sites that support the selected region
- Price Analysis: unreachable sites are now skipped immediately instead of timing out, and per-site failures no longer abort the scrape
- Series cards now fade in a touch more gradually so filter and shelf toggles read as a soft cross-fade instead of a hard repaint
