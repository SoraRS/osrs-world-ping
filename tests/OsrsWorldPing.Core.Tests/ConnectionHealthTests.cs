using OsrsWorldPing.Core;

namespace OsrsWorldPing.Core.Tests;

public sealed class ConnectionHealthTests
{
    private static MonitorSnapshot Snapshot(params WorldSnapshot[] worlds) => new(new AppSettings(), worlds, true,
        false, worlds.Length, worlds.Length, null, DateTimeOffset.UnixEpoch, null, false, DateTimeOffset.UnixEpoch);
    private static WorldSnapshot World(int id, ProbeStatus? status, int population = 100, long? latency = null) =>
        new(id, new WorldInfo(id, 0, $"supplied-{id}.runescape.com", "-", 0, population), false, false, null, false,
            false, status is { } value ? new ProbeResult(value, value == ProbeStatus.Success ? latency ?? 10 : null) : null, null, true, 0);

    [Fact]
    public void SuccessfulReadingsAndCatalogueOfflineWorldsAreExpected()
    {
        var health = ConnectionHealth.From(Snapshot(World(301, ProbeStatus.Success), World(302, ProbeStatus.TimedOut, -1), World(303, ProbeStatus.Unreachable, -1)));
        Assert.Equal(ConnectionHealthState.Healthy, health.State); Assert.Equal("OK", health.Text);
        Assert.Contains("2 catalogue-reported offline", health.Detail);
    }

    [Theory]
    [InlineData(ProbeStatus.TimedOut)]
    [InlineData(ProbeStatus.DnsError)]
    [InlineData(ProbeStatus.PermissionDenied)]
    [InlineData(ProbeStatus.Unsupported)]
    [InlineData(ProbeStatus.NetworkError)]
    [InlineData(ProbeStatus.Error)]
    public void UnexpectedResponsesAreIssuesWithoutClaimingOffline(ProbeStatus status)
    {
        var health = ConnectionHealth.From(Snapshot(World(301, status), World(302, status)));
        Assert.Equal(ConnectionHealthState.Issues, health.State); Assert.Equal(0, health.Failures);
        Assert.Equal("2 connection issues", health.Text);
    }

    [Fact]
    public void ExplicitReachabilityFailuresTakePrecedenceAndOnlyCountAdvertisedOnlineWorlds()
    {
        var health = ConnectionHealth.From(Snapshot(World(301, ProbeStatus.Unreachable), World(302, ProbeStatus.TimedOut), World(303, ProbeStatus.Unreachable, -1)));
        Assert.Equal(ConnectionHealthState.Failures, health.State); Assert.Equal("1 connection failures", health.Text);
        Assert.Equal(1, health.Issues); Assert.Equal(1, health.Failures);
    }

    [Fact]
    public void WaitingAndUnavailableCataloguesNeverReportOk()
    {
        Assert.Equal(ConnectionHealthState.Pending, ConnectionHealth.From(Snapshot()).State);
        Assert.Equal(ConnectionHealthState.Pending, ConnectionHealth.From(Snapshot(World(301, null))).State);
        Assert.Equal(ConnectionHealthState.Pending, ConnectionHealth.From(Snapshot(World(301, ProbeStatus.Success)) with { IsScanning = true }).State);
        Assert.Equal(ConnectionHealthState.Pending, ConnectionHealth.From(Snapshot() with { CatalogueFetchedAt = null }).State);
    }

    [Fact]
    public void MissingFavoritesAndInvalidSuccessReadingsAreIssues()
    {
        var missing = World(399, null) with { World = null, IsFavorite = true, IsTracked = true };
        var invalid = World(301, ProbeStatus.Success, latency: -1);
        Assert.Equal("2 connection issues", ConnectionHealth.From(Snapshot(missing, invalid)).Text);
    }
}
