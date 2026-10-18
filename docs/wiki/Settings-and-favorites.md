Click the **gear beneath the top-right window controls** or press **Ctrl+,**. The settings tabs appear in the title frame. Settings opens inside the same window. **World tracking** keeps the live worlds visible; **Application** and **About** show their settings without the world list. Monitoring continues on every tab. About and the unfolded catalogue can scroll within the shorter window. Click the gear again or press **Esc** to return. Page changes fade and slide smoothly; motion follows Windows' animation preference.

## World tracking

Select **Show all** below the live grid to unfold the rest of the catalogue in world-number order. **Hide all** folds it away with the same fade and slide transition. The toggle stays at the top of the content area once you scroll past it. **Back to top** fades in at that point, sits in a transparent control area above the timestamp, and returns you to the beginning of the page. Both controls have no background and stay clear of world tiles. The gap above the unfolded catalogue matches the return control area. Browsing sends no probes. Inactive worlds fade, with clearer colors on hover; hover for the reading time and whether it came from a scan. The scrollbar stays hidden; wheel, touchpad, and keyboard scrolling remain available.

## Favorites

Use the heart to save up to five worlds, including a world outside the normal cutoff. Favorites use their own slots and never appear twice. Remove a favorite before adding a sixth. Removing its heart restores automatic selection; it may return to the hidden list.

## Application

Set a latency cutoff from **1–2,000 ms** and refresh interval from **5–600 seconds**, then select **Apply settings**. Both values are required. The status report is above the compact action buttons. The compact inputs and actions share a centered column.

## Refresh worlds

In Application, check every world again. The centered status starts orange for unexpected **connection issues**. It turns green **OK** after every affected world is checked again with the same result. Normal live checks and the hidden-world review can confirm those results, as can Refresh worlds. New or changed issues return to orange; explicit unreachable **connection failures** remain red. Green also covers expected successful responses. Catalogue-reported offline worlds are expected; a timeout alone is an issue, not proof of an offline server. Hover the status to distinguish consistent issue responses from successful replies; green does not establish that a game world is online. This response pattern resets when the app exits.

The **Latest log** text button opens a local modal table of the most recent checks, with hostnames, check times, reading sources, and native results. Use **Refresh worlds** after raising the cutoff to find newly eligible worlds immediately.

## Reset to default

Use the small text button just below **Apply settings** to restore the 30 ms cutoff, five-second refresh, and disabled legacy window/tray options. Favorites and saved window placement are kept.

## About

Find the version, description, SoraRS's GitHub and Discord profiles, project/support links, privacy information, signing status, and license.

## Keyboard shortcuts

| Key | Action |
| --- | --- |
| Ctrl+, | Open settings. |
| Esc | Return to worlds. |
| Tab / Shift+Tab | Move between controls with visible keyboard focus. |
| Space / Enter | Activate the focused button or favorite control. |
| Page Up / Page Down, Home / End | Scroll when the page or tracking control has focus. |

## Saved preferences and window position

Favorites, application preferences, and window position are remembered. The working fixed size is 768 × 660 at 100% scaling; older saved widths, heights, and maximized states do not override it. The size remains under review. The progress indicator does not shift the updated timestamp.

Older saved tray or always-on-top preferences remain compatible, although their controls are no longer shown. **Reset to default** clears those preferences while keeping favorites and window position. If an old tray preference hides the app, restore it from the tray menu. Use **Exit** there to quit fully.
