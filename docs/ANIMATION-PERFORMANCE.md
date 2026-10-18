# Animation performance

The 1.4.1 transitions changed settings/canvas heights on every animation frame. That repeatedly invalidated layout around the world grids and scroll viewer. They also attached a DropShadowEffect to every card, including inactive/offscreen cards, and created the full hidden catalogue's control tree on first expansion. A 900 ms navigation sequence with viewport-length travel spent a substantial part of its time outside the visible area.

The Windows baseline at commit `1486fe5b55d660d94158a263f5524af81324fab1` used the unchanged 1.4.1 application plus a profiling test. [Baseline CI](https://github.com/SoraRS/osrs-world-ping/actions/runs/39085562718) uploaded `motion-profile.json` in Windows-interface-previews. The renderer reported hardware tier 2 and the Default process preference.

| Baseline scenario | Layout notifications | Elapsed | Allocations |
| --- | ---: | ---: | ---: |
| Tracking → Application | 39 | 901 ms | 1.21 MB |
| Application → About | 47 | 913 ms | 1.12 MB |
| About → Dashboard | 36 | 932 ms | 0.65 MB |
| Show all, first use, 321 worlds | 24 | 988 ms | 61.74 MB |
| Hide all | 24 | 638 ms | 1.51 MB |
| Show all, warm | 24 | 637 ms | 0.30 MB |

The UI callback gaps typically ranged from 16–28 ms, with 95th percentiles around 30–33 ms on this runner. They indicate uneven UI scheduling there; they do not establish the user's GPU presentation rate. At 240 Hz a display frame is approximately 4.17 ms.

Version 1.4.2 removes per-frame layout changes from navigation/reveal animations, overlays the mutually exclusive page bodies, uses shorter travel and 440/320 ms transitions, removes per-card blur shaders and infinite hover pulses, and virtualizes/recycles hidden catalogue cards. Page and scroll geometry are settled at transition boundaries; rendering properties handle movement between them. The profiling fixture and full-catalogue traversal/keyboard test cover these paths without real network traffic.

No application frame-rate cap, forced software rendering, persistent rendering callback, or high-frequency timer is installed. WPF remains responsible for display scheduling. Its [.NET 10 MediaContext](https://github.com/dotnet/wpf/blob/v10.0.0/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MediaContext.cs) uses the display refresh rate reported by its rendering pipeline. Setting [Timeline.DesiredFrameRate](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.animation.timeline.desiredframerate) is a request, not a presentation guarantee, and does not eliminate expensive layout or rendering.

Microsoft documents [layout invalidation costs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance-layout-and-design) and the need to [detach rendering callbacks when idle](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance-other-recommendations). An [upstream issue](https://github.com/dotnet/wpf/issues/2294) also describes a WPF/windowed variable-refresh interaction. That is a possible system-specific contributor, not a diagnosis of this user's monitor; the application does not change driver or monitor settings.

A CI desktop and callback trace cannot certify 240 FPS. Acceptance on an actual 240 Hz display should record the Windows-selected refresh rate, resolution/scaling, GPU/driver, other displays, and windowed VRR setting. Check frame presentation using a GPU/ETW presentation tool if the optimized build still stutters. Distinguish panel capability, Windows mode, UI callback cadence, and frames actually presented. Compare the same scenes on the same hardware; never treat shortening an animation alone as a frame-rate improvement.
