namespace OsrsWorldPing.Core;

public enum ConnectionHealthState { Pending, Healthy, Issues, Failures }

/// <summary>Reports observed reachability separately from catalogue-reported game availability.</summary>
public sealed record ConnectionHealth(ConnectionHealthState State, int Issues, int Failures, string Text, string Detail)
{
    public static ConnectionHealth From(MonitorSnapshot snapshot)
    {
        int failures = snapshot.Worlds.Count(w => w.World is { Population: >= 0 } && w.LatestResult?.Status == ProbeStatus.Unreachable);
        int issues = snapshot.Worlds.Count(w =>
            w.World is null || w.World.Population >= 0 && w.LatestResult is { } result &&
            result.Status != ProbeStatus.Unreachable && (result.Status != ProbeStatus.Success || result.RoundTripMilliseconds is null or < 0));
        int expectedOffline = snapshot.Worlds.Count(w => w.World is { Population: < 0 });
        string detail = $"{expectedOffline} catalogue-reported offline; {issues} unexpected responses; {failures} explicit reachability failures. " +
            "No reply does not establish that a game world is offline. An unreachable response can come from the network path. Open Latest log for the evidence and check times.";
        if (!snapshot.SettingsLoaded || snapshot.IsScanning || snapshot.Worlds.Any(w => w.World is not null && w.LatestResult is null))
            return new(ConnectionHealthState.Pending, issues, failures, snapshot.IsScanning ? "Refreshing worlds" : "Waiting for readings", detail);
        if (snapshot.CatalogueFetchedAt is null || snapshot.Worlds.Count == 0)
            return new(ConnectionHealthState.Pending, issues, failures, "World data unavailable", detail);
        if (failures > 0) return new(ConnectionHealthState.Failures, issues, failures, $"{failures} connection failures", detail);
        if (issues > 0) return new(ConnectionHealthState.Issues, issues, failures, $"{issues} connection issues", detail);
        return new(ConnectionHealthState.Healthy, 0, 0, "OK", detail);
    }
}
