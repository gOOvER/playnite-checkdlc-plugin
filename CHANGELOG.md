# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
- **SDK-style Project Conversion**: Modernized `CheckDlc.csproj` to SDK-style (`Microsoft.NET.Sdk` with `net462` and `UseWpf`) enabling fast CLI builds via `dotnet build` and modern C# 10 language features.
- **Direct Integration of `playnite-plugincommon`**: Removed `.gitmodules` and integrated common store and control sources directly into the repository, eliminating git submodule friction.
- **Process Security**: Enforced URI scheme validation and `UseShellExecute` before opening external links to prevent command injection.

### Removed
- **Legacy Package Management**: Removed `.gitmodules`, `packages.config`, and legacy `packages/` folder in favor of native NuGet `PackageReference`.

### Fixed
- **Settings Store Configuration**: Fixed bug in `CheckDlcSettingsViewModel.EndEdit()` where `CheckDlc.EpicApi.StoreSettings` was incorrectly assigned `Settings.SteamStoreSettings` instead of `Settings.EpicStoreSettings`.
- **NullReferenceException in Settings**: Added null checks for `SteamApi`, `EpicApi`, and `GogApi` during settings initialization and save operations when individual store integrations are disabled.
- **Custom Theme Button Hook**: Fixed copy-paste error in `CheckDlc.cs` where `OnCustomThemeButtonClick` listened for `PART_CustomHowLongToBeatButton` instead of `PART_CustomCheckDlcButton`.
- **Price Notification Crash**: Fixed unhandled `NullReferenceException` in background price check task when newly discovered DLC items were not present in previous database records.
- **Nintendo eShop Search Query Typo**: Removed unwanted trailing `'t'` character appended to search queries in `NintendoDlc.cs` (`select?q={term}t&fq=...`).
- **Control DataContext Recursion**: Fixed self-referencing getter recursion in `PluginButton.xaml.cs` and `PluginListDlc.xaml.cs` (`ControlDataContext = (Type)controlDataContext;`).
- **Tagging False Positive**: Fixed `GameDlc.HasAllDlc` evaluating to `true` for games with 0 DLCs, which caused false "100%" tags.
- **Manual AppId Parsing**: Added `uint.TryParse` fallback in manual Steam game assignment dialog to prevent `FormatException`.
- **Memory Leak in UI Elements**: Avoided unmanaged event hook leaks on static database objects from transient WPF controls.

### Changed
- **Price Parsing Performance**: Optimized `Dlc.PriceNumeric` and `PriceBaseNumeric` to eliminate repetitive runtime regex parsing and string allocations on data binding.
- **LINQ Allocations**: Converted redundant enumeration chains in `CheclDlcGameView.xaml.cs` and database lookups to short-circuiting calls.
- **CSV Export**: Replaced quadratic string concatenation in `GetCsvData` with `StringBuilder` and added quote escaping.

---

## [1.3] - 2025-03-24

### Added
- GOG OSS Library support.
- Manual DLC inclusion feature.
- PlayStation Store & Nintendo eShop (Europe) DLC support (experimental).

### Changed
- Updated localizations via Crowdin.
- UI improvements across views.
- Updated CSV data export format.

### Fixed
- Fixed bug where DLC covers were displayed incorrectly or failed to load.

---

## [1.2] - 2024-10-19

### Added
- Ability to manually flag specific DLCs as owned.
- Option to automatically add a tag when 100% of DLCs are owned.
- Export functionality for all owned DLCs to CSV/Excel document.

### Changed
- Updated localizations.
- UI enhancements.

### Fixed
- Fixed owned DLC detection and incorrect store links.

---

## [1.1] - 2024-09-06

### Added
- New elements and integration points for custom themes (see Wiki).

### Changed
- Updated localizations.
- UI improvements.

### Fixed
- Fixed issue with store authentication.

---

## [1.0.1] - 2023-01-02

### Changed
- Updated localizations.

### Fixed
- Minor bug fixes.

---

## [1.0] - 2022-09-24

### Added
- Compatibility exclusively with Playnite 10+.
- Display total number of owned DLCs in the DLC details view.

### Changed
- Updated localizations.

### Fixed
- Miscellaneous bug fixes.

---

## [0.6] - 2022-05-09

### Added
- Origin (EA) store DLC support.
- Notification system when tracked DLC prices change.

### Changed
- Updated localizations.

### Fixed
- Fixed currency handling issues.
- Minor bug fixes.

---

## [0.5] - 2022-02-22

### Changed
- Improved Epic Games Store DLC support.
- UI tweaks and improvements.
- Updated localizations.

### Fixed
- Minor bug fixes.

---

## [0.4.1] - 2022-01-04

### Added
- Settings options for GOG currencies.

### Fixed
- Fixed GOG currency selection bug.
- Fixed issue with free Steam DLC price display.
- Minor bug fixes.

---

## [0.4] - 2021-12-31

### Added
- Filters in DLC view (owned, hidden, free, price limit).
- New configuration options in settings.

### Changed
- UI styling and layout adjustments.
- Updated localizations.

### Fixed
- Minor bug fixes.

---

## [0.3] - 2021-12-22

### Added
- Ignorable terms configuration for import filtering.
- Controls for custom Playnite theme integration.

### Changed
- Updated localizations.

### Fixed
- Minor bug fixes.

---

## [0.2] - 2021-12-18

### Added
- Pricing display for unowned DLCs.
- Epic Games Store DLC integration.
- Dedicated view for unowned free DLCs.

### Changed
- Updated localizations.

### Fixed
- Minor bug fixes.

---

## [0.1.1] - 2021-12-15

### Changed
- Performance improvements during library scanning.
- Updated localizations.

### Fixed
- Minor bug fixes.

---

## [0.1] - 2021-12-11

### Added
- Initial release of CheckDlc for Playnite.
- Basic Steam DLC tracking and metadata retrieval.

---

## [Legacy] - Common Store & Steam Improvements (Submodule & Dependencies)

*The following notes document shared submodule updates and dependencies from commit `805bbe1`:*

### Dependencies
- **SteamKit2**: Upgraded `2.x` → `2.5.0` (latest compatible with .NET Framework 4.6.2).
- **PlayniteSDK**: Upgraded `6.11.0` → `6.16.0`.

### Bug Fixes (`SteamApi.cs` in `playnite-plugincommon`)
- **`GetGameInfos`**: Replaced `uint.Parse(id)` with `uint.TryParse` — invalid IDs no longer throw `FormatException`.
- **`GetAchievementsSchema`**: Added `TryParse` fix; returns empty collection instead of throwing.
- **`GetAchievements`**: Replaced `.Count()` (LINQ enumeration) with `.Count` (O(1) property) on `ObservableCollection`.
- **`RemoveWishlist`**: Added missing `return false` after `sessionid` check failure; removed incorrect `string.Format(UrlWishlistRemove, UserId)`.
- **`GetUsersStats`**: Replaced `ulong.Parse` with `ulong.TryParse`; returns empty list on invalid `UserId`.
- **`CheckIsPublic(AccountInfos)`**: Replaced `ulong.Parse` with `ulong.TryParse` with warn + `false` fallback.
- **`GetScreeshotsPath`**: Replaced `ulong.Parse` with `ulong.TryParse` with early return on invalid `UserId`.
- **`GetWishlistByApi`**: Fixed URL format bug where item object was passed instead of `x.Appid`; added null safety.
- **`GetAccountGamesInfosByWeb`**: Added null guard for `gamesListTemplate` before accessing `GetAttribute`.
- **`GetAchievementsByWeb`**: Replaced query selectors with null-safe access (`?.InnerHtml ?? string.Empty`).
- **`GetCurrentFriendsInfosByWeb`**: Added null-safe access (`?.`) on DOM QuerySelector calls.
- **`GetSteamUsers`**: Replaced `ulong.Parse` with `ulong.TryParse` in `loginusers.vdf` parsing.
- **`ParseDescription`**: Added null guard; returns `string.Empty` on null.
- **`GetAppDetails`**: Converted unbounded recursion into a bounded `while` loop (max 10 attempts).
- **`Login`**: Added null check for `g_rgProfileData` parse result.

### Bug Fixes (`SteamKit.cs` & `GameJoltApi.cs` in `playnite-plugincommon`)
- **WebAPI parameters**: Changed `Dictionary<string, string>` to `Dictionary<string, object>` for SteamKit2 2.5.0 API.
- **`GetAppList`**: Migrated from deprecated `ISteamApps/GetAppList` to `IStoreService/GetAppList/v1` with cursor-based pagination and fallback.
- **`GetUserStatsForGame`**: Upgraded from v1 to v2 endpoint.
- Removed stale `using SuccessStory.Models;` import in `GameJoltApi.cs`.

### New Features (`SteamApi.cs`)
- **`GetAppId` — FuzzySharp fallback**: When no exact name match is found, `Fuzz.Ratio` is used to find candidate with score ≥ 90.
- **`GetGamesDlcsOwned` — API key path**: When `UseAuth = false` and API key is set, `IPlayerService/GetOwnedGames` is used with fallback.

### Code Quality & Build
- Removed dead code: `GetDlcFromSteamDb`, `SetExtensionsAchievementsFromSteamDb`, and unused URL constants.
- Removed redundant intermediate lists and simplified field initializers.
- Fixed `PostBuildEvent` PowerShell invocation quoting in `CheckDlc.csproj`.
