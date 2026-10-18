Find the symptom below. For setup steps, see [Getting started](https://github.com/SoraRS/osrs-world-ping/wiki/Getting-started).

## I still see the previous interface

Fully exit older copies, download the executable artifact from the intended successful build, and launch the extracted file directly. Check **About** for version 1.5.3 or newer and update your shortcut.

## No ordinary worlds appear

Open **Application** to check progress or a notice. The default 30 ms cutoff may be too low for your location. Raise it, select **Apply settings**, then **Refresh worlds**. You can also favorite a world regardless of ping.

## Some worlds show No reply or Unreachable

Brief disconnections and missed ICMP replies are expected on some connections. Open **Application → Latest log** for the failure breakdown if reporting a problem. The table is a snapshot when opened; select rows and press **Ctrl+C** if you want to share them. [Connection result meanings](https://github.com/SoraRS/osrs-world-ping/wiki/Connection-results) explain the different conditions. A timeout does not prove the world is offline or that ICMP is blocked.

## Everything times out

Check your connection, VPN, firewall, and network rules. Some networks block ICMP even when the game works. Try another network before assuming every world is offline; administrator access is not normally required.

## The world list will not download

The app uses its last saved catalogue when possible. Application shows the notice; settings shows the catalogue's saved time. Connect to the internet and select **Refresh worlds** to retry.

## Windows asks for a runtime

Install the **x64 .NET 10 Desktop Runtime** linked on [Getting started](https://github.com/SoraRS/osrs-world-ping/wiki/Getting-started), then reopen the app.

## Windows says “Don't run” or “unknown publisher”

Current testing builds are unsigned. Confirm that the file came from this project's release or Actions page. Signed releases can be checked under **Properties → Digital Signatures**, using the signature and checksum information accompanying that release. See [Windows warnings and signing status](https://github.com/SoraRS/osrs-world-ping/wiki/Windows-warnings). Signing does not guarantee immediate SmartScreen reputation. Do not disable Windows protection globally.

## The app disappeared

If a tray option is enabled, look in the system tray. Double-click its icon to restore the app, or right-click for **Settings** and **Exit**.

## My old cutoff changed from 60 to 30 ms

The earlier default moves to 30 ms once. Other custom cutoffs are kept. An intentional 60 ms selection saved after that migration remains 60 ms through this update. Old refresh intervals below five seconds are raised to the new minimum.

## Animations feel choppy

Use a current build; check **About** for its version. The app uses render-only transitions and loads only visible catalogue tiles to reduce animation overhead. If it still stutters, include your display resolution, Windows scaling/refresh-rate setting, GPU, and whether you use multiple displays or windowed G-SYNC/FreeSync when [reporting the problem](https://github.com/SoraRS/osrs-world-ping/issues). Windows animation preferences are respected.

## Preferences do not survive a restart

Check Application for a save error. The app needs access to `%LOCALAPPDATA%\OsrsWorldPing`. Readable settings can still work for the current session. Keep the old data folder until the import succeeds. To start completely fresh, exit first and rename both the current and legacy data folders so old preferences are not imported again.
