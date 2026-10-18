using OsrsWorldPing.Core;

namespace OsrsWorldPing.Core.Tests;

public sealed class ConnectionResponseHistoryTests
{
    private static ConnectionHealth Health(MonitoringEngine engine) => ConnectionHealth.From(engine.GetSnapshot());
    private static readonly ProbeResult Timeout = new(ProbeStatus.TimedOut, Diagnostic: new("ICMP", "127.0.0.1", "TimedOut"));

    [Theory]
    [InlineData(ProbeStatus.TimedOut)]
    [InlineData(ProbeStatus.DnsError)]
    [InlineData(ProbeStatus.PermissionDenied)]
    [InlineData(ProbeStatus.Unsupported)]
    [InlineData(ProbeStatus.NetworkError)]
    [InlineData(ProbeStatus.Error)]
    public async Task ActualRepeatedChecksConfirmIssuesWithoutChangingReadings(ProbeStatus status)
    {
        await using var h = new Harness(); h.Probe.Return(new(status));
        await h.Engine.StartAsync();
        for (int i = 0; i < 5; i++) Assert.Equal(ConnectionHealthState.Issues, Health(h.Engine).State);
        await h.Engine.RecheckAsync();
        var health = Health(h.Engine);
        Assert.Equal(ConnectionHealthState.StableIssues, health.State); Assert.Equal("OK", health.Text);
        Assert.Equal(4, health.Issues); Assert.Equal(0, health.Failures);
        Assert.Contains("4 unchanged issue responses", health.Detail);
        Assert.All(h.Engine.GetSnapshot().Worlds, world => Assert.Equal(status, world.LatestResult!.Status));
    }

    [Fact]
    public async Task GoodLivePingsCannotConfirmUnprobedHiddenIssues()
    {
        await using var h = new Harness();
        string issueHost = h.Catalogue.Worlds[0].Host;
        h.Probe.Handler = (host, _) => Task.FromResult(host == issueHost ? Timeout : new(ProbeStatus.Success, 20));
        await h.Engine.StartAsync();
        for (int i = 0; i < 3; i++) await h.Engine.CheckTrackedWorldsAsync();
        Assert.Equal(ConnectionHealthState.Issues, Health(h.Engine).State);
        await h.Engine.RecheckHiddenWorldsAsync();
        Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
    }

    [Fact]
    public async Task EveryIssueMustBeRecheckedAcrossLiveAndHiddenCycles()
    {
        await using var h = new Harness(); h.Store.Settings = new() { FavoriteWorldIds = [301] };
        h.Probe.Return(Timeout); await h.Engine.StartAsync();
        await h.Engine.CheckTrackedWorldsAsync();
        Assert.Equal(ConnectionHealthState.Issues, Health(h.Engine).State);
        await h.Engine.RecheckHiddenWorldsAsync();
        Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
        Assert.Equal(4, Health(h.Engine).Issues);
    }

    [Fact]
    public async Task ARepeatedLiveFavoriteIssueCanBeConfirmed()
    {
        await using var h = new Harness(); h.Store.Settings = new() { FavoriteWorldIds = [301] };
        string issueHost = h.Catalogue.Worlds[0].Host;
        h.Probe.Handler = (host, _) => Task.FromResult(host == issueHost ? Timeout : new(ProbeStatus.Success, 20));
        await h.Engine.StartAsync(); Assert.Equal(ConnectionHealthState.Issues, Health(h.Engine).State);
        await h.Engine.CheckTrackedWorldsAsync();
        Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
    }

    [Fact]
    public async Task TheSameIssueCountOnDifferentWorldsMustBeConfirmedAgain()
    {
        await using var h = new Harness(); string issueHost = h.Catalogue.Worlds[0].Host;
        h.Probe.Handler = (host, _) => Task.FromResult(host == issueHost ? Timeout : new(ProbeStatus.Success, 20));
        await h.Engine.StartAsync(); await h.Engine.RecheckAsync();
        Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
        issueHost = h.Catalogue.Worlds[1].Host;
        await h.Engine.RecheckAsync(); Assert.Equal(ConnectionHealthState.Issues, Health(h.Engine).State);
        Assert.Equal(1, Health(h.Engine).Issues);
        await h.Engine.RecheckAsync(); Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
    }

    [Theory]
    [InlineData("status")]
    [InlineData("diagnostic")]
    [InlineData("host")]
    public async Task ChangedResponsesOrHostsInvalidateConfirmation(string change)
    {
        await using var h = new Harness(); h.Probe.Return(Timeout);
        await h.Engine.StartAsync(); await h.Engine.RecheckAsync();
        Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
        if (change == "status") h.Probe.Return(new(ProbeStatus.DnsError));
        if (change == "diagnostic") h.Probe.Return(Timeout with { Diagnostic = new("ICMP", "127.0.0.1", "DifferentNativeResult") });
        if (change == "host") h.Catalogue.Worlds = h.Catalogue.Worlds.Select(w => w.Id == 301 ? w with { Host = "replacement.example" } : w).ToArray();
        await h.Engine.RecheckAsync(); Assert.Equal(ConnectionHealthState.Issues, Health(h.Engine).State);
        await h.Engine.RecheckAsync(); Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
    }

    [Fact]
    public async Task SuccessfulPingValuesAndResolvedAddressesCanVary()
    {
        await using var h = new Harness(); long latency = 10; string address = "127.0.0.1";
        string issueHost = h.Catalogue.Worlds[0].Host;
        h.Probe.Handler = (host, _) => Task.FromResult(host == issueHost ? Timeout with { Diagnostic = new("ICMP", address, "TimedOut") } : new(ProbeStatus.Success, latency));
        await h.Engine.StartAsync(); latency = 123; address = "127.0.0.2";
        await h.Engine.RecheckAsync(); Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
    }

    [Fact]
    public async Task AHealthyCheckResetsTheIssuePattern()
    {
        await using var h = new Harness(); h.Probe.Return(Timeout);
        await h.Engine.StartAsync(); await h.Engine.RecheckAsync();
        h.Probe.Return(new(ProbeStatus.Success, 20)); await h.Engine.RecheckAsync();
        Assert.Equal(ConnectionHealthState.Healthy, Health(h.Engine).State);
        h.Probe.Return(Timeout); await h.Engine.RecheckAsync();
        Assert.Equal(ConnectionHealthState.Issues, Health(h.Engine).State);
        await h.Engine.RecheckAsync(); Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
    }

    [Fact]
    public async Task ReachabilityFailuresAndUnavailableFavoritesStayVisible()
    {
        await using var h = new Harness(); h.Probe.Return(new(ProbeStatus.Unreachable));
        await h.Engine.StartAsync(); await h.Engine.RecheckAsync();
        Assert.Equal(ConnectionHealthState.Failures, Health(h.Engine).State);
        Assert.Empty(h.Engine.GetSnapshot().StableIssueResponses);
        await using var missing = new Harness(); missing.Store.Settings = new() { FavoriteWorldIds = [399] };
        await missing.Engine.StartAsync(); await missing.Engine.RecheckAsync();
        Assert.Equal(ConnectionHealthState.Issues, Health(missing.Engine).State);
        Assert.Empty(missing.Engine.GetSnapshot().StableIssueResponses);
    }

    [Fact]
    public async Task CancelledChecksAndCachedCatalogueTimestampsCannotFakeConfirmation()
    {
        await using var h = new Harness(); h.Probe.Return(Timeout); await h.Engine.StartAsync();
        using var stop = new CancellationTokenSource();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        h.Probe.Handler = async (_, token) => { entered.TrySetResult(); await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, token); return Timeout; };
        var scan = h.Engine.RecheckHiddenWorldsAsync(stop.Token); await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        stop.Cancel(); await Assert.ThrowsAnyAsync<OperationCanceledException>(() => scan);
        Assert.Equal(ConnectionHealthState.Issues, Health(h.Engine).State);
        h.Probe.Return(Timeout); h.Catalogue.Error = new IOException("Use cache");
        var fetchedAt = h.Engine.GetSnapshot().CatalogueFetchedAt;
        await h.Engine.RecheckAsync(); Assert.Equal(fetchedAt, h.Engine.GetSnapshot().CatalogueFetchedAt);
        Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
    }

    [Fact]
    public async Task RestartDoesNotRememberConfirmedIssues()
    {
        await using var h = new Harness(); h.Probe.Return(Timeout);
        await h.Engine.StartAsync(); await h.Engine.RecheckAsync();
        Assert.Equal(ConnectionHealthState.StableIssues, Health(h.Engine).State);
        await h.Engine.DisposeAsync();
        await using var restarted = new MonitoringEngine(h.Catalogue, h.Probe, h.Store, h.Clock);
        await restarted.StartAsync(); Assert.Equal(ConnectionHealthState.Issues, Health(restarted).State);
    }
}
