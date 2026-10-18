using System.Windows;
using OsrsWorldPing.Core;

namespace OsrsWorldPing.Desktop;

public sealed record DiagnosticRow(int World, string Expected, string Reading, string Source, string CheckedAt,
    string Host, string Stage, string NativeCode, string Address)
{
    public static DiagnosticRow From(WorldSnapshot world) => new(world.Id,
        world.World is null ? "Unavailable" : world.World.Population < 0 ? "Offline (expected)" : "Listed online",
        world.LatestResult is { Status: ProbeStatus.Success, RoundTripMilliseconds: >= 0 } result ? $"{result.RoundTripMilliseconds} ms" : world.LatestResult?.Status.ToString() ?? "Not checked",
        !world.IsTracked || world.IsScanResult ? "Scan result" : "Live check",
        world.LastChecked?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "—",
        world.World?.Host ?? "—", world.LatestResult?.Diagnostic?.Stage ?? "—",
        world.LatestResult?.Diagnostic?.NativeCode ?? "—", world.LatestResult?.Diagnostic?.Address ?? "—");
}

public partial class DiagnosticsWindow : Window
{
    public DiagnosticsWindow(MonitorSnapshot snapshot, string version)
    {
        InitializeComponent();
        MaxWidth = SystemParameters.WorkArea.Width; MaxHeight = SystemParameters.WorkArea.Height;
        Width = Math.Min(Width, MaxWidth); Height = Math.Min(Height, MaxHeight);
        DataContext = new
        {
            Summary = $"{version} · {ConnectionHealth.From(snapshot).Text}\nCatalogue: {snapshot.CatalogueFetchedAt?.ToLocalTime().ToString("g") ?? "unavailable"} · Snapshot: {snapshot.CapturedAt.ToLocalTime():g}",
            Rows = snapshot.Worlds.OrderBy(w => w.Id).Select(DiagnosticRow.From).ToArray()
        };
    }
}
