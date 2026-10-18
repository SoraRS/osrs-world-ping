## Build and run

Install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

```sh
dotnet build OsrsWorldPing.slnx --configuration Release -m:1
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

## CI and previews

**Build and test** establishes a 1920×1080 runner desktop so the provisional 768×660 window fits before launching WPF tests. It runs core tests on Linux and Windows, then verifies single-file publishing and startup/clean exit. Proportional smaller-work-area fixtures keep the fixed design intact. The workflow retains the repeated `Process.Refresh()`/window-handle/title wait that fixed startup detection.

The motion acceptance test temporarily enables Windows client-area animations, captures moving gear sparks, exercises reversal and reduced motion, then restores the previous preference in `finally`. It sends the setting-change notification without persisting a profile change.

The same artifact includes `motion-profile.json`: layout-update counts, allocation totals, and UI rendering callback gaps for page transitions and cold/warm catalogue reveals with 321 fixture worlds. The trace avoids screenshot encoding during samples and detaches its rendering listener afterward. These callback gaps are not GPU presentation measurements or proof of physical 240 Hz output. See [animation performance](https://github.com/SoraRS/osrs-world-ping/wiki/Animation-performance).

The **Windows-interface-previews** artifact contains deterministic dashboard and all three settings tabs at baseline, compact, and scaled logical sizes. These are actual WPF renders using fixture data; the running app uses real probes. A **Signed release candidate** workflow reuses these checks, then requires the configured protected environment and SignPath approval. See [signing setup](https://github.com/SoraRS/osrs-world-ping/wiki/Release-signing).

## Related developer guides

- [Monitoring core and persistence](https://github.com/SoraRS/osrs-world-ping/wiki/Monitoring-core)
- [Interface and layout](https://github.com/SoraRS/osrs-world-ping/wiki/Interface-and-layout)
- [Animation performance](https://github.com/SoraRS/osrs-world-ping/wiki/Animation-performance)
- [Windows validation](https://github.com/SoraRS/osrs-world-ping/wiki/Windows-validation)
- [Diagnostics and investigation](https://github.com/SoraRS/osrs-world-ping/wiki/Diagnostics-and-investigation)
- [Release signing](https://github.com/SoraRS/osrs-world-ping/wiki/Release-signing)
- [Maintaining this Wiki](https://github.com/SoraRS/osrs-world-ping/wiki/Maintaining-the-wiki)
