# Changelog

## [Unreleased]

### Dependencies

- **SteamKit2**: upgraded `2.x` → `2.5.0` (latest compatible with .NET Framework 4.6.2)
- **PlayniteSDK**: upgraded `6.11.0` → `6.16.0`

---

### Bug Fixes

#### `SteamApi.cs`

- **`GetGameInfos`**: replaced `uint.Parse(id)` with `uint.TryParse` — invalid IDs no longer throw `FormatException`
- **`GetAchievementsSchema`**: same `TryParse` fix; returns empty collection instead of throwing
- **`GetAchievements`**: replaced `.Count()` (LINQ enumeration) with `.Count` (O(1) property) on `ObservableCollection`
- **`RemoveWishlist`**: added missing `return false` after `sessionid` check failure — the HTTP POST was previously sent even without a valid session; also removed incorrect `string.Format(UrlWishlistRemove, UserId)` (URL has no `{0}` placeholder)
- **`GetUsersStats`**: replaced `ulong.Parse` with `ulong.TryParse`; returns empty list on invalid `UserId`
- **`CheckIsPublic(AccountInfos)`**: replaced `ulong.Parse` with `ulong.TryParse` with warn + `false` fallback
- **`GetScreeshotsPath`**: replaced `ulong.Parse` with `ulong.TryParse` with early return on invalid `UserId`
- **`GetWishlistByApi`**: fixed URL format bug — `string.Format(UrlSteamGame, x)` was passing the entire wishlist item object instead of `x.Appid`; also added `?.Response?.Items?.ForEach(...)` null safety
- **`GetAccountGamesInfosByWeb`**: added null guard for `gamesListTemplate` before accessing `GetAttribute` — prevents `NullReferenceException` when the page template is missing
- **`GetAchievementsByWeb`**: replaced `el.QuerySelector(...).InnerHtml` (×2) with null-safe `?.InnerHtml ?? string.Empty`
- **`GetCurrentFriendsInfosByWeb`**: added null-safe access (`?.`) on all three DOM QuerySelector calls (`a.selectable_overlay`, `img`, `div.friend_block_content`)
- **`GetSteamUsers`**: replaced `ulong.Parse(user.Name)` with `ulong.TryParse` — malformed Steam IDs in `loginusers.vdf` are now skipped with a warning instead of crashing
- **`ParseDescription`**: added null guard — `null` input no longer throws, returns `string.Empty`
- **`GetAppDetails`**: converted unbounded recursion (up to 11 stack frames, up to 200s blocking) into a bounded `while` loop with max 10 attempts
- **`Login`**: added null check for `g_rgProfileData` parse result before accessing properties

#### `SteamKit.cs`

- **All WebAPI call parameters**: changed `Dictionary<string, string>` to `Dictionary<string, object>` to match SteamKit2 2.5.0 API
- **`GetAppList`**: migrated from deprecated `ISteamApps/GetAppList` to `IStoreService/GetAppList/v1` with cursor-based pagination and fallback to legacy endpoint
- **`GetUserStatsForGame`**: upgraded from v1 to v2 endpoint

#### `GameJoltApi.cs`

- Removed stale `using SuccessStory.Models;` import that caused build failure

---

### New Features

#### `SteamApi.cs`

- **`GetAppId` — FuzzySharp fallback**: when no exact name match is found, `Fuzz.Ratio` is used to find the best candidate with score ≥ 90; the match and score are logged for debugging
- **`GetGamesDlcsOwned` — API key path**: when `UseAuth = false` and an API key is configured, `IPlayerService/GetOwnedGames` is used instead of cookie-based `UserData.RgOwnedApps`; falls back to `UserData` when API call returns no results

---

### Code Quality

- Removed dead code: `GetDlcFromSteamDb`, `SetExtensionsAchievementsFromSteamDb`, and their associated URL constants (`SteamDbDlc`, `SteamDbExtensionAchievements`) — both methods were commented out at all call sites
- Removed redundant intermediate list (`dlcsIdSteamDb`) in `GetGameInfos`
- Simplified `GetGameInfos` field initializers (removed superfluous `?.` after null-guard)

---

### Build

- Fixed `PostBuildEvent` PowerShell invocation — path with spaces now correctly quoted with `-File` and `&quot;`
