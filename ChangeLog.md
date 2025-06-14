# Changelog

All notable changes to **Tsundoku** are documented here.  
This project adheres to [Semantic Versioning](https://semver.org/) and follows the guidelines from [Keep a Changelog](https://keepachangelog.com).

<details>
<summary>1.x Releases</summary>

### 1.1.1 – 2025-06-14
#### Changed
- Updated **System.Drawing.Common** to `v9.0.6`  
- Updated **System.Linq.Dynamic.Core** to `v1.6.6`  
- Updated **Microsoft.Extensions.Http** to `v9.0.6`  
- Renamed **Value** to **Total Value** in the *Add New Series* window

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