Install and open OSRS World Ping on a Windows PC.

## Requirements

- Windows x64.
- Microsoft's [.NET 10 Desktop Runtime (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0). Choose **Desktop Runtime**; the standard or ASP.NET runtime alone is not enough.

## Download the app

Published downloads will appear under [Releases](https://github.com/SoraRS/osrs-world-ping/releases). Until a release is available, use a testing build:

1. Sign in to GitHub and open [Build and test](https://github.com/SoraRS/osrs-world-ping/actions/workflows/build.yml).
2. Choose a successful run for the branch or version you want. Use a run for **main** for the current merged app, or the run linked from a review PR to test that candidate.
3. Scroll to **Artifacts** and download **OsrsWorldPing-win-x64**. The interface previews artifact contains screenshots, not the app.
4. Extract the ZIP into a folder you want to keep and open `OsrsWorldPing.exe`.

No installer or administrator access is normally needed. Check **About** for the app version. [GitHub's artifact download guide](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/download-workflow-artifacts) has more download help.

Testing builds may be unsigned. See [Windows warnings](https://github.com/SoraRS/osrs-world-ping/wiki/Windows-warnings) if Windows shows a publisher warning.

## First launch

The app checks every world once, then shows worlds with suitable response times for your connection. Open the **gear → Application** to see progress, a catalogue notice, or the **Refresh worlds** action. If the list is empty, follow [Troubleshooting](https://github.com/SoraRS/osrs-world-ping/wiki/Troubleshooting).

## Update an older copy

Fully exit the old app first. If it is in the tray, right-click its icon and choose **Exit**. Replace your old download with the new `OsrsWorldPing.exe`, open that file, and update any shortcut pointing to an older copy. Downloading a new build does not replace other copies automatically.

The app was previously named **OSRS Check Ping**. Existing favorites and preferences are imported when possible; the old folder is kept as a backup. See [Privacy and local data](https://github.com/SoraRS/osrs-world-ping/wiki/Privacy) for storage and migration details.
