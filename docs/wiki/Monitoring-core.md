## Monitoring and interfaces

`MonitoringEngine` owns catalogue discovery, live selection, scan coordination, and cancellation. Replaceable `IWorldCatalogueClient`, `ILatencyProbe`, `ISettingsStore`, and `IClock` keep testing deterministic. ICMP includes a two-second deadline and bounded concurrency (32 by default). Live refresh accepts 5–600 seconds; live and hidden-review deadlines are independent, so a five-minute review never causes an early live poll. Live cycles pause during scans. Hidden worlds are checked every five minutes, not on ordinary live ticks. No missed-tick backlog is queued.

Thirty ordinary slots use successful eligible latency, then world number. The core retains session-override compatibility; manual tracking controls are no longer exposed in the UI. Five favorite slots are separate, including unavailable entries. All dashboard projections sort by world number. Card view models are keyed by world ID and updated in place; ordinary readings do not rebuild collections or reset scrolling. The in-window settings view shares the same engine. The live grid appears on the dashboard and World tracking tab; Application and About hide it while monitoring continues.

## Selection and removal

- Thirty ordinary slots and five separate favorite slots; no duplicates.
- Initial selection uses valid latency at or below 30 ms by default, sorted by latency then world number.
- Display projections use ascending world number, reading down columns then right.
- Discovery readings do not count toward removal. A valid reading at or below the cutoff resets the bad-reading streak.
- An unpinned world is removed after `ceil(60 / refreshIntervalSeconds)` consecutive bad readings. Favorites are exempt.
- Monitoring preference changes apply to subsequent checks and reset removal streaks. A scan or review can add newly eligible automatic worlds.
- Session overrides remain a core compatibility feature, reset on exit, and survive rescans. Manually enabled worlds are still subject to removal. The current UI does not expose manual tracking checkboxes.

## Persistence and migration

Settings and the cached catalogue are stored in `%LOCALAPPDATA%\OsrsWorldPing`. A missing/legacy schema marker triggers the one-time 60→30 ms migration; the migrated file is atomically saved immediately. Current-schema settings can deliberately use 60 ms. Session overrides and readings are never serialized. Startup imports missing files from the previous OsrsCheckPing directory, preserving that backup and any newer destination files. Readable old settings/cache remain available if import writes fail. Application resets preserve favorites and window placement.

## Consistent issue responses

`ConnectionResponseHistory` records completed probe operations inside the core. Issue identity includes world ID, supplied host, result category, invalid-success value if any, and diagnostic stage/code. Successful RTTs and resolved-address changes do not define an issue. Track which issue worlds have actually been rechecked across live cycles, hidden reviews, or full scans. Confirm the entire unchanged issue set only after every affected world has fresh evidence; UI snapshot reads and cancelled operations do not count. Changes reset the baseline. Unreachable failures, unavailable favorites, missing data, and incomplete readings cannot be learned as green. `StableIssueResponses` is immutable snapshot metadata held only for the session. `ConnectionHealthState.StableIssues` uses green OK with an explicit consistency explanation and unchanged raw log rows; restart clears it.
