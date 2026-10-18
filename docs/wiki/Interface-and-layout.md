## Presentation metadata

`WorldMetadata` derives country, activity, and main-character risk presentation from catalogue data. Location and flag mappings are documented in source with links to RuneLite's public protocol definitions. Unknown codes/activities use neutral fallbacks. Fallback icons and the emblem are original vector artwork; supported activities use genuine OSRS skill/boss sprites with provenance in `Assets/Osrs/SOURCES.md`. The observatory is an original generated background. No artwork is fetched at runtime.

### World tiles

Compact world tiles share a template across the dashboard and tracking settings. Tiles are borderless and transparent; inactive tiles fade and recover on hover. Hearts precede world numbers. Favorite, Members, and Free to play headings replace tile membership stars.

Ordinary sections use five balanced columns, read downward then right. Blank activities use the region flag and an em dash; other tiles show an activity sprite or neutral fallback. Hidden reading provenance remains in the tooltip. Thirty ordinary worlds of a single membership occupy six rows; favorites also use five columns. The title text in About is centered independently of its adjacent emblem. Members and Free to play are separated by one 44-DIP tile height. The Show all control reveals the other worlds without probing. Tint/lift and heart animations honor Windows client-area animation preferences, and hover clocks stop when unloaded.

## Navigation and scrolling

Navigation reserves header/footer geometry and uses an overlay deck for settings and worlds. It measures page geometry once and animates opacity and existing RenderTransforms over 440 ms; outgoing content fades for 240 ms and incoming content for 360 ms after an 80 ms overlap delay. Short travel keeps both fades visible. Application and About hide the world list after its fade; returning restores the same card models.

### Show all and Hide all

Show all/Hide all animate a RectangleGeometry clip, opacity, translation, and chevron over 320 ms. The scroll extent changes once, with a separate translation preserving the visual anchor when closing a scrolled catalogue. Neither transition animates Height, Width, Margin, or LayoutTransform.

### Virtualized catalogue

Hidden catalogue cards use VirtualWorldColumnsPanel: only viewport rows plus a small buffer have controls, recycled as the outer page scrolls. Numeric columns, keyboard Tab navigation across unrealized items, and model identities are retained. Hover feedback uses a brief brush tint and lift, with no per-card shader or indefinite pulse. Version guards reject obsolete completion callbacks. The scrollbar stays hidden without disabling scrolling or changing grid width. Pointer exit restores neutral reveal-button ink while preserving its keyboard focus ring. Action feedback expires after five seconds or tab navigation. Gear sparks and rotations honor Windows animation preferences; reduced-motion and reversal paths clear clocks.

### Sticky controls and footer

The footer normally occupies only the 15-DIP timestamp row. When the return control is needed, it reserves an 85-DIP transparent navigation gap until the catalogue closes or tracking is left. Hiding the return button alone does not change geometry. Catalogue closure releases the space at the same boundary as the extent change, before retained translations animate. The same 85-DIP gap separates live and unfolded catalogue worlds; the collapsed catalogue uses a 52-DIP control gap. Back to top lives in the footer, outside the viewer. The transparent sticky Show/Hide toggle clips scrolling content outside its 52-DIP band when pinned to the top or constrained by the bottom edge; threshold/resize geometry changes do not run on animation frames. The footer reserves scan-progress height independently of visibility. Polling does not change navigation state.

## Retained settings reveal

The settings reveal uses a canvas to measure the page content independently of the animated reserved height. An auto-sized content row would otherwise impose its natural minimum on the reveal and make the world grid jump before the animation completes. The reveal clips moving settings content so it cannot overlap live-world labels. Tests sample the retained clip and both content offsets while reserved geometry stays fixed.

## Native window behavior

The fixed prototype retains native caption/minimize behavior for DWM transitions, removes resize/maximize styles, and blocks the corresponding system commands. Its normal bounds fit the current monitor work area. The app installs its native hook after WindowChrome’s hook because HwndSource invokes hooks in reverse registration order. Acceptance checks cover native styles, client bounds, and minimize/restore frames.

## Fixed window and centered content

The working design is 768 × 660 Windows device-independent pixels. These dimensions are provisional pending player review. `FixedWindowLayout` defines the working size and proportion-preserving work-area fit; `MainWindow` uses a uniform Viewbox, locks its normal dimensions, and disables native resizing/maximizing while retaining dragging, minimizing, tray behavior, and saved positions. DPI/work-area/monitor changes refit the window without activating it. Old placement files remain compatible, but their variable sizes and maximized state are no longer applied.

Settings tabs live in the custom title frame with sufficient height for text and hover motion. The silver gear is below the caption controls. Section headers show Favorites, Members, and Free to play without counts. The caption contains only minimize and close buttons; native resize/maximize commands remain blocked.

### Centered columns and stable readings

`WorldColumnsPanel` keeps a stable column pitch and trims the unoccupied tail so the visible list is centered. Numeric readings reserve a fixed width so ordinary ping updates keep their positions. Numeric and failure readings share a fixed 64-DIP field, left-aligned six DIPs after the world number so changing digits cannot move adjacent cards. Longer failure labels trim within the narrow tile; the tooltip and accessible description retain the full reading. Updates and recovery keep the same footprint. Hidden-catalogue measurements are cached independently of viewport recycling.

Windows checks now cover the provisional 768 × 660 layout with 29 member worlds and one free world, occupied-content margins, stable numeric updates, uncounted headings, native title-tab clicks, unclipped labels, a transparent highlighted gear, and disabled size/maximize commands. Existing animation/focus/virtualization/tray/startup checks remain. Preview fixtures simulate proportional smaller work areas; 150–200% render previews and pure DPI-fit tests do not replace user acceptance on an actual Windows display with those scaling settings.

### Application settings layout

Application settings use a centered compact column: health above Refresh worlds, small Refresh/Apply actions, a text-only Latest log action, 120-DIP input fields, and an 11-point text-only Reset to default two DIPs below Apply. The Window & tray controls are removed from the page. Stored window/tray preferences remain compatible; reset still clears them while keeping favorites and placement. About and expanded catalogues scroll in the shorter window. Windows acceptance measures button/input centering, control sizes, reset spacing/function, collapsed-window clearance, and transparent navigation bands.
