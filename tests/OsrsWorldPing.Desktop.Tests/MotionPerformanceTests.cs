using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using OsrsWorldPing.Desktop;

namespace OsrsWorldPing.Desktop.Tests;

public sealed partial class DesktopTests
{
    private sealed record TransitionProfile(string Scenario, double ElapsedMs, int LayoutUpdates, int UiRenderingCallbacks,
        double MedianUiCallbackGapMs, double P95UiCallbackGapMs, double WorstUiCallbackGapMs, long AllocatedBytes);

    [Fact]
    public Task ProfileFullCatalogueTransitionsWithoutScreenshotEncoding() => OnUiThreadAsync(async () =>
    {
        bool previousAnimations = SystemParameters.ClientAreaAnimation;
        var h = Samples(321); var main = new MainWindow(h.Engine); main.Show();
        var profiles = new List<TransitionProfile>();
        try
        {
            await SetClientAnimationsAsync(true);
            await UntilAsync(() => main.Model.Tracked.Count == 30); await ResizeAsync(main, 1920, 1040);
            main.Model.ShowSettings = true; await UntilAsync(() => !main.IsNavigating);
            async Task ProfileAsync(string scenario, Action navigate)
            {
                int layouts = 0, callbacks = 0;
                var gaps = new List<double>();
                long previous = 0;
                TimeSpan previousRenderingTime = TimeSpan.MinValue;
                EventHandler layout = (_, _) => layouts++;
                EventHandler rendering = (_, args) =>
                {
                    var at = ((RenderingEventArgs)args).RenderingTime;
                    if (at == previousRenderingTime) return;
                    previousRenderingTime = at;
                    long now = Stopwatch.GetTimestamp();
                    if (previous != 0) gaps.Add(Stopwatch.GetElapsedTime(previous, now).TotalMilliseconds);
                    previous = now; callbacks++;
                };
                long allocated = GC.GetTotalAllocatedBytes();
                var elapsed = Stopwatch.StartNew();
                main.LayoutUpdated += layout; CompositionTarget.Rendering += rendering;
                try
                {
                    navigate();
                    await Dispatcher.Yield(DispatcherPriority.Loaded);
                    await UntilAsync(() => !main.IsNavigating && !main.IsRevealingWorlds);
                }
                finally { main.LayoutUpdated -= layout; CompositionTarget.Rendering -= rendering; }
                elapsed.Stop(); gaps.Sort();
                double Percentile(double p) => gaps.Count == 0 ? 0 : gaps[(int)Math.Clamp(Math.Ceiling(gaps.Count * p) - 1, 0, gaps.Count - 1)];
                profiles.Add(new(scenario, elapsed.Elapsed.TotalMilliseconds, layouts, callbacks,
                    Percentile(.5), Percentile(.95), Percentile(1), GC.GetTotalAllocatedBytes() - allocated));
                Assert.NotEmpty(gaps);
            }
            await ProfileAsync("tracking-to-application", () => main.Model.SelectedSettingsTab = 1);
            await ProfileAsync("application-to-about", () => main.Model.SelectedSettingsTab = 2);
            await ProfileAsync("about-to-dashboard", () => main.Model.ShowSettings = false);
            main.Model.SelectedSettingsTab = 0; main.Model.ShowSettings = true; await UntilAsync(() => !main.IsNavigating);
            await ProfileAsync("show-all-cold-321-worlds", () => main.Model.ShowAllWorlds = true);
            await ProfileAsync("hide-all-321-worlds", () => main.Model.ShowAllWorlds = false);
            await ProfileAsync("show-all-warm-321-worlds", () => main.Model.ShowAllWorlds = true);
            var scroll = (System.Windows.Controls.ScrollViewer)main.FindName("PageScroll");
            scroll.ScrollToBottom(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            await UntilAsync(() => main.HasReturnControlSpace);
            var worlds = (System.Windows.Controls.StackPanel)main.FindName("WorldList");
            double initialTop = worlds.TranslatePoint(new Point(), scroll).Y;
            await ProfileAsync("hide-all-from-bottom", () =>
            {
                main.Model.ShowAllWorlds = false;
                Assert.InRange(Math.Abs(worlds.TranslatePoint(new Point(), scroll).Y - initialTop), 0, 1);
            });
            Assert.False(main.HasReturnControlSpace);
            Assert.All(profiles, profile => Assert.InRange(profile.LayoutUpdates, 0, 12));
            Assert.True(profiles.Single(p => p.Scenario == "show-all-cold-321-worlds").AllocatedBytes < 20_000_000,
                "First reveal must not allocate the full catalogue's visual tree.");
        }
        finally
        {
            string directory = Path.Combine(AppContext.BaseDirectory, "ui-preview"); Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(Path.Combine(directory, "motion-profile.json"), JsonSerializer.Serialize(new
            {
                Version = main.Model.Version,
                RenderTier = RenderCapability.Tier >> 16,
                ProcessRenderMode = RenderOptions.ProcessRenderMode.ToString(),
                Measurement = "UI rendering callback cadence and layout notifications, not GPU presentation or physical monitor FPS. No screenshot encoding during samples.",
                Profiles = profiles
            }, new JsonSerializerOptions { WriteIndented = true }));
            await CloseAsync(main); await h.DisposeAsync(); await SetClientAnimationsAsync(previousAnimations);
        }
    });
}
