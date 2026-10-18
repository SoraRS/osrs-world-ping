# Development and Windows acceptance

## Build and run

Install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

```sh
dotnet build --configuration Release
dotnet test tests/OsrsWorldPing.Core.Tests --configuration Release
```

On Windows:

```sh
dotnet run --project src/OsrsWorldPing.Desktop
dotnet test tests/OsrsWorldPing.Desktop.Tests --configuration Release
```

Publish the framework-dependent single file:

```sh
dotnet publish src/OsrsWorldPing.Desktop --configuration Release -p:PublishProfile=win-x64 --output artifacts/win-x64
```

The output folder must contain only `OsrsWorldPing.exe`. The target machine needs the .NET 10 Desktop Runtime x64. Builds/tests use no game account or external latency API.

The core targets portable `net10.0`. WPF and UI tests target `net10.0-windows`; Linux can cross-compile them with Windows targeting, but cannot run WPF or validate Windows rendering. A successful Linux build is not evidence of visual acceptance.

## Architecture

`MonitoringEngine` owns catalogue discovery, live selection, scan coordination, and cancellation. Replaceable `IWorldCatalogueClient`, `ILatencyProbe`, `ISettingsStore`, and `IClock` keep testing deterministic. ICMP includes a two-second deadline and bounded concurrency (32 by default). Live refresh accepts 5–600 seconds; live and hidden-review deadlines are independent, so a five-minute review never causes an early live poll. Live cycles pause during scans. Hidden worlds are checked every five minutes, not on ordinary live ticks. No missed-tick backlog is queued.

Thirty ordinary slots use successful eligible latency, then world number. The core retains session-override compatibility; manual tracking controls are no longer exposed in the UI. Five favorite slots are separate, including unavailable entries. All dashboard projections sort by world number. Card view models are keyed by world ID and updated in place; ordinary readings do not rebuild collections or reset scrolling. The in-window settings view shares the same engine. The live grid appears on the dashboard and World tracking tab; Application and About hide it while monitoring continues.

`WorldMetadata` derives country, activity, and main-character risk presentation from catalogue data. Location and flag mappings are documented in source with links to RuneLite's public protocol definitions. Unknown codes/activities use neutral fallbacks. Fallback icons and the emblem are original vector artwork; supported activities use genuine OSRS skill/boss sprites with provenance in `Assets/Osrs/SOURCES.md`. The observatory is an original generated background. No artwork is fetched at runtime. Compact world tiles share a template across the dashboard and tracking settings. Tiles are borderless and transparent; inactive tiles fade and recover on hover. Hearts precede world numbers. Favorite, Members, and Free to play headings replace tile membership stars. Ordinary sections use five balanced columns, read downward then right. Blank activities use the region flag and an em dash; other tiles show an activity sprite or neutral fallback. Hidden reading provenance remains in the tooltip. Thirty ordinary worlds of a single membership occupy six rows; favorites also use five columns. The title text in About is centered independently of its adjacent emblem. Members and Free to play are separated by one 44-DIP tile height. The Show all control reveals the other worlds without probing. Tint/lift and heart animations honor Windows client-area animation preferences, and hover clocks stop when unloaded.

Settings and the cached catalogue are stored in `%LOCALAPPDATA%\OsrsWorldPing`. A missing/legacy schema marker triggers the one-time 60→30 ms migration; the migrated file is atomically saved immediately. Current-schema settings can deliberately use 60 ms. Session overrides and readings are never serialized. Startup imports missing files from the previous OsrsCheckPing directory, preserving that backup and any newer destination files. Readable old settings/cache remain available if import writes fail. Application resets preserve favorites and window placement.

Navigation reserves header/footer geometry and uses an overlay deck for settings and worlds. It measures page geometry once and animates opacity and existing RenderTransforms over 440 ms; outgoing content fades for 240 ms and incoming content for 360 ms after an 80 ms overlap delay. Short travel keeps both fades visible. Application and About hide the world list after its fade; returning restores the same card models. Show all/Hide all animate a RectangleGeometry clip, opacity, translation, and chevron over 320 ms. The scroll extent changes once, with a separate translation preserving the visual anchor when closing a scrolled catalogue. Neither transition animates Height, Width, Margin, or LayoutTransform. Hidden catalogue cards use VirtualWorldColumnsPanel: only viewport rows plus a small buffer have controls, recycled as the outer page scrolls. Numeric columns, keyboard Tab navigation across unrealized items, and model identities are retained. Hover feedback uses a brief brush tint and lift, with no per-card shader or indefinite pulse. Version guards reject obsolete completion callbacks. The scrollbar stays hidden without disabling scrolling or changing grid width. Pointer exit restores neutral reveal-button ink while preserving its keyboard focus ring. Action feedback expires after five seconds or tab navigation. Gear sparks and rotations honor Windows animation preferences; reduced-motion and reversal paths clear clocks. The footer normally occupies only the 15-DIP timestamp row. When the return control is needed, it reserves an 85-DIP transparent navigation gap until the catalogue closes or tracking is left. Hiding the return button alone does not change geometry. Catalogue closure releases the space at the same boundary as the extent change, before retained translations animate. The same 85-DIP gap separates live and unfolded catalogue worlds; the collapsed catalogue uses a 52-DIP control gap. Back to top lives in the footer, outside the viewer. The transparent sticky Show/Hide toggle clips scrolling content outside its 52-DIP band when pinned to the top or constrained by the bottom edge; threshold/resize geometry changes do not run on animation frames. The footer reserves scan-progress height independently of visibility. Polling does not change navigation state.

## CI and previews

**Build and test** establishes a 1920×1080 runner desktop so the provisional 768×660 window fits before launching WPF tests. It runs core tests on Linux and Windows, then verifies single-file publishing and startup/clean exit. Proportional smaller-work-area fixtures keep the fixed design intact. The workflow retains the repeated `Process.Refresh()`/window-handle/title wait that fixed startup detection.

The motion acceptance test temporarily enables Windows client-area animations, captures moving gear sparks, exercises reversal and reduced motion, then restores the previous preference in `finally`. It sends the setting-change notification without persisting a profile change.

The same artifact includes `motion-profile.json`: layout-update counts, allocation totals, and UI rendering callback gaps for page transitions and cold/warm catalogue reveals with 321 fixture worlds. The trace avoids screenshot encoding during samples and detaches its rendering listener afterward. These callback gaps are not GPU presentation measurements or proof of physical 240 Hz output. See [animation performance](ANIMATION-PERFORMANCE.md).

The **Windows-interface-previews** artifact contains deterministic dashboard and all three settings tabs at baseline, compact, and scaled logical sizes. These are actual WPF renders using fixture data; the running app uses real probes. A **Signed release candidate** workflow reuses these checks, then requires the configured protected environment and SignPath approval. See [signing setup](SIGNING.md).

## Windows acceptance gate

Review the actual Windows preview artifact and run these checks before approving a release. Record results against the candidate commit, Windows version, monitor size, scaling, and installed Desktop Runtime in the release review. Generated previews exercise scaled logical layouts; the monitor/DPI checks below still require actual display scaling.

| Check | Expected result |
| --- | --- |
| 768×660 prototype at 100%, five favorites + thirty ordinary worlds | All 35 live tiles fit without scrolling; bottom edge shortly below Show all; separate favorite section; numeric order; centered occupied footprint; uncounted section headings; no clipped controls or malformed outlines. |
| 1366×768, 150% and 200%, mixed-DPI monitor moves | Windows scaling is respected; the full fixed design fits proportionally without changing its aspect ratio or column order; scrolling works; caption controls stay reachable; taskbar is respected. |
| Background window during several live cycles and a hidden review | Foreground window stays active; no flashing, restore, focus changes, or selection. |
| Scrolled dashboard and settings during ping changes | Offset and keyboard focus stay in place; card identities/order stay stable. |
| Ctrl+, / Tab / Shift+Tab / Space / Esc | Settings navigation and controls work with visible keyboard focus; cards have no persistent selected state. |
| Gear hover and Windows animation preference | Two-notch clockwise rotation and reverse; no animated rotation when client-area animations are disabled. |
| Members/free/unknown region/activity and PvP/High Risk/Bounty | Correct section symbols, membership colors, activity/flag fallbacks; warning text/tooltips; separate-profile/safe modes avoid the main-character warning. |
| Online/offline startup, cached catalogue, no cache, exit during scan | Settings always accessible; progress/notices/retry work; exit cancels probes cleanly. |
| Default30, upgraded60, later chosen60; favorite and preference changes | Migration happens once; preferences/pins persist; session overrides reset on restart; five-favorite and thirty-slot limits hold. |
| Minimize/close tray options, restore/settings/exit | Monitoring continues hidden; only deliberate restore activates; Exit fully quits. |
| Fixed startup, drag, disabled resize/maximize commands; disconnected monitor | Startup uses provisional fixed bounds and a saved reachable position; maximize/resize remain disabled; moving/minimizing work; screen changes refit proportionally. |
| Explorer/taskbar/tray icon and signed candidate with runtime | Matching icons; only one EXE required; valid timestamp/signature/hash when approved; responsive launch and clean shutdown. |

Design approval requires reviewing actual Windows output against the current approved feedback specifications, not just successful compilation. Private feedback documents stay outside the Git tree. Signing enrollment and manual Windows acceptance must not be represented as completed by a source change.

## Connection investigation

`ProbeResult` includes optional stage, resolved game-server address, and native error/status details. Injectable `IDnsResolver` and `IIcmpSender` adapters allow deterministic DNS, deadline, permission, cancellation, and destination-error tests. A timeout is presented as **No reply**, never inferred as a world outage or firewall diagnosis. Negative catalogue population is independently noted in the reading tooltip.

The explicit developer command `dotnet run --project tools/OsrsWorldPing.Diagnostics --configuration Release -- artifacts/world-diagnostics.json` downloads the supplied catalogue, makes one bounded probe per host, retries failures twice five seconds apart, and compares persistent failures with native Windows ping. It includes loopback control and per-attempt results. This investigation is separate from application polling and writes only the requested report. Validation-branch PRs run it as an informational CI step; external reachability is not a deterministic test assertion.

The Windows preview artifact also contains desktop-level taskbar and minimize/restore frames. These supplement WPF renders; native window styles, monitor work-area bounds, foreground focus, and user interaction must be verified on Windows. Temporary validation branches may be committed for CI; merging remains subject to maintainer acceptance.

The settings reveal uses a canvas to measure the page content independently of the animated reserved height. An auto-sized content row would otherwise impose its natural minimum on the reveal and make the world grid jump before the animation completes. The reveal clips moving settings content so it cannot overlap live-world labels. Tests sample the retained clip and both content offsets while reserved geometry stays fixed.

The fixed prototype retains native caption/minimize behavior for DWM transitions, removes resize/maximize styles, and blocks the corresponding system commands. Its normal bounds fit the current monitor work area. The app installs its native hook after WindowChrome’s hook because HwndSource invokes hooks in reverse registration order. Acceptance checks cover native styles, client bounds, and minimize/restore frames.

## Fixed-window prototype (1.5.3)

The working design is 768 × 660 Windows device-independent pixels. These dimensions are provisional pending player review. `FixedWindowLayout` defines the working size and proportion-preserving work-area fit; `MainWindow` uses a uniform Viewbox, locks its normal dimensions, and disables native resizing/maximizing while retaining dragging, minimizing, tray behavior, and saved positions. DPI/work-area/monitor changes refit the window without activating it. Old placement files remain compatible, but their variable sizes and maximized state are no longer applied.

Settings tabs live in the custom title frame with sufficient height for text and hover motion. The silver gear is below the caption controls. Section headers show Favorites, Members, and Free to play without counts. The caption contains only minimize and close buttons; native resize/maximize commands remain blocked. `WorldColumnsPanel` keeps a stable column pitch and trims the unoccupied tail so the visible list is centered. Numeric readings reserve a fixed width so ordinary ping updates keep their positions. Numeric and failure readings share a fixed 64-DIP field, left-aligned six DIPs after the world number so changing digits cannot move adjacent cards. Longer failure labels trim within the narrow tile; the tooltip and accessible description retain the full reading. Updates and recovery keep the same footprint. Hidden-catalogue measurements are cached independently of viewport recycling.

Windows checks now cover the provisional 768 × 660 layout with 29 member worlds and one free world, occupied-content margins, stable numeric updates, uncounted headings, native title-tab clicks, unclipped labels, a transparent highlighted gear, and disabled size/maximize commands. Existing animation/focus/virtualization/tray/startup checks remain. Preview fixtures simulate proportional smaller work areas; 150–200% render previews and pure DPI-fit tests do not replace user acceptance on an actual Windows display with those scaling settings.

Application settings use a centered compact column: health above Refresh worlds, small Refresh/Apply actions, a text-only Latest log action, 120-DIP input fields, and an 11-point text-only Reset to default two DIPs below Apply. The Window & tray controls are removed from the page. Stored window/tray preferences remain compatible; reset still clears them while keeping favorites and placement. About and expanded catalogues scroll in the shorter window. Windows acceptance measures button/input centering, control sizes, reset spacing/function, collapsed-window clearance, and transparent navigation bands.

`ConnectionResponseHistory` records completed probe operations inside the core. Issue identity includes world ID, supplied host, result category, invalid-success value if any, and diagnostic stage/code. Successful RTTs and resolved-address changes do not define an issue. Track which issue worlds have actually been rechecked across live cycles, hidden reviews, or full scans. Confirm the entire unchanged issue set only after every affected world has fresh evidence; UI snapshot reads and cancelled operations do not count. Changes reset the baseline. Unreachable failures, unavailable favorites, missing data, and incomplete readings cannot be learned as green. `StableIssueResponses` is immutable snapshot metadata held only for the session. `ConnectionHealthState.StableIssues` uses green OK with an explicit consistency explanation and unchanged raw log rows; restart clears it.
