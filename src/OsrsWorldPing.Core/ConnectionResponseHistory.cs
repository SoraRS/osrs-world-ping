namespace OsrsWorldPing.Core;

// Session-only observations. Called only after a probe operation completes, never by UI refreshes.
internal sealed class ConnectionResponseHistory
{
    private WorldIssueResponse[] _previousIssues = [];
    private readonly HashSet<int> _rechecked = [];
    public IReadOnlyList<WorldIssueResponse> StableIssues { get; private set; } = Array.Empty<WorldIssueResponse>();

    public void RecordCompletedChecks(IReadOnlyList<WorldSnapshot> worlds, IEnumerable<int> checkedIds)
    {
        var issues = ConnectionHealth.CaptureIssues(worlds);
        bool comparable = worlds.Count > 0 && worlds.All(w => w.World is not null && w.LatestResult is not null) &&
            !worlds.Any(w => w.World is { Population: >= 0 } && w.LatestResult?.Status == ProbeStatus.Unreachable);
        if (!comparable || issues.Length == 0 || !_previousIssues.SequenceEqual(issues))
        {
            _previousIssues = comparable ? issues : [];
            _rechecked.Clear(); StableIssues = Array.Empty<WorldIssueResponse>();
            return;
        }
        _rechecked.UnionWith(checkedIds);
        if (issues.All(issue => _rechecked.Contains(issue.WorldId)))
            StableIssues = Array.AsReadOnly(issues);
    }
}
