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

## Preview evidence

The Windows preview artifact also contains desktop-level taskbar and minimize/restore frames. These supplement WPF renders; native window styles, monitor work-area bounds, foreground focus, and user interaction must be verified on Windows. Temporary validation branches may be committed for CI; merging remains subject to maintainer acceptance.
