# Changelog

All notable changes to **Tsundoku** are documented here.  
This project adheres to [Semantic Versioning](https://semver.org/) and follows the guidelines from [Keep a Changelog](https://keepachangelog.com).

<details>
<summary>1.x Releases</summary>

### 1.4.0 - 2025-08-10
#### Changed
- Updated **Avalonia** to `11.3.3`.
- Updated **NLog** to `6.0.3`.
- Added search when adding a new series: a list appears on the left; select an item to use add it.
- Added **Goodreads** import.
- During price analysis, other controls are disabled until the scrape completes.

#### Fixed
- ComboBox & ListBox selection styling.
- `Hentai` filter is now selectable.
- Crash when selecting a genre filter (introduced in the last version).
- Changing a series’ demographic now updates percentages correctly.
- Pie chart labels no longer misalign when values change.
- Import no longer crashes on file I/O errors.
- Price analysis window resets correctly if the scrape fails.

### 1.3.1 – 2025-06-27
#### Changed
- Updated all **Avlonia** libs to `11.3.2`
- Updated logging format message

#### Fixed
- Changing rating or volumes read for a series no longer clears/resets the value amount

### 1.3.0 – 2025-06-22
#### Changed
- Updated **NLog** to `6.0.0`
- Added **Hentai** to genre list
- Added **Supervisor** as a valid staff role
- Added support for new MangaDex languages: `Albanian, Belarusian, Bosnian, Galician, Gujarati, Icelandic, Kannada, Latvian, Macedonian, Malayalam, Marathi, Punjabi, Slovenian, Telugu, and Urdu`
- Improved Discord rich presence to show the series the user is editing if clicked, shows the title and format
- Added a loading dialog for long-running operations (e.g., importing or exporting data)
- Removed support for the following currencies: `₣, ₻`
- Added support for the following currencies: `Rp, RM, R$, ₪, ₴, zł, Ft, Kč, kr, lei, ৳, ₮, KM, Br, L, din, ден, ر.س, د.إ, د.ك, Rs`
- Changed maximum value format to `0000000000000000.00`
- App will automatically clean up unused cover images on startup
- Added supports for importing collections from Libib via their csv export

#### Performance Improvements
- Improved performance of the AniList client
- Improved performance of the MangaDex client
- Improved cover-image downloading

#### Fixed
- Series with missing cover images now automatically retrieve a new cover on refresh
- Clicking a button for an already open window now sets the focus to that window
- Fixed currency-symbol placement in collection and series value amounts based on locale
- Extended MangaDex description parser to handle additional edge cases (refresh a series to apply)
- Fixed MangaDex staff-name retrieval to correctly return both native and full names
- When searching by title (without ID), MangaDex and AniList API calls are now ordered by relevance to improve matching for ambiguous titles
- Editing Series window now correctly displays the title in the selected language (instead of always using Romaji)
- Series description now defaults to the original language if an English description is not available for MangaDex entries

### 1.2.0 – 2025-06-14
#### Changed
- Updated **System.Drawing.Common** to `v9.0.6`  
- Updated **System.Linq.Dynamic.Core** to `v1.6.6`  
- Updated **Microsoft.Extensions.Http** to `v9.0.6`  
- Renamed **Value** to **Total Value** in the *Add New Series* window
- Updated **Publisher, Staff, & Description** series card text to be bold

#### Fixed
- Resolved issue where refreshing a series with a changed status would break the UI layout until hovered  
- Fixed bug where changing a cover via URL failed due to folder permission errors  
- Fixed issue where a successful refresh would delete the cover filename  

### 1.1.0 – 2025-06-08
#### Changed
- Updated Avalonia to **v11.3.1**  
- Updated DiscordRichPresence to **v1.3.0.28**  
- Updated DynamicData to **v9.4.1**  
- Updated NLog to **v5.5.0**  
- Updated System.Linq.Dynamic.Core to **v1.6.5**  
- Updated Projektanker.Icons.Avalonia.FontAwesome to **v9.6.2**  
- Centered **Genres** title in the Series Edit Window  
- Added padding to the top of the Series Edit Window buttons  
- Reduced Settings Window max height to **845**  
- Renamed **Mean Score** to **Mean Rating** in the Statistics window.

#### Fixed
- Make series title text copyable by clicking on it.  
- Correct Mean Rating calculation logic.  
- Ensure the log file is created on first run.  
- Enable the Series Edit button to fire properly.  
- Prevent “refresh” from duplicating series cards.  
- Display the app icon at its intended size.  
- Hover title now displays correctly  
- Full series title is copied on click if the series card title text overflows

</details>

<!--
<details>
<summary>1.0.0 – 2025-05-01</summary>

### Added
- Initial release of Tsundoku.
- Basic series lookup (MangaDex + AniList).
- Series card UI with title, cover image, and stats.

</details>
-->

</details>