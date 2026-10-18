using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using OsrsWorldPing.Core;
using OsrsWorldPing.Desktop;

namespace OsrsWorldPing.Desktop.Tests;

public sealed partial class DesktopTests
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowEnabled(IntPtr window);
    [Fact]
    public Task TrackingButtonSticksAndBackToTopFadesAtTheSameScrollThreshold() => OnUiThreadAsync(async () =>
    {
        bool previousAnimations = SystemParameters.ClientAreaAnimation;
        var h = Samples(321); var main = new MainWindow(h.Engine); main.Show();
        try
        {
            await SetClientAnimationsAsync(true);
            await UntilAsync(() => main.Model.Tracked.Count == 30); await ResizeAsync(main, 1920, 1040);
            main.Model.ShowSettings = true; await UntilAsync(() => !main.IsNavigating);
            main.Model.ShowAllWorlds = true; await UntilAsync(() => !main.IsRevealingWorlds);
            var scroll = (ScrollViewer)main.FindName("PageScroll");
            var host = (Border)main.FindName("TrackingControlHost");
            var anchor = (Border)main.FindName("TrackingControlAnchor");
            var back = (Button)main.FindName("BackToTopButton");
            var toggle = (ToggleButton)main.FindName("ShowAllButton");
            double threshold = anchor.TranslatePoint(new Point(), scroll).Y;
            Assert.True(threshold > 100); Assert.Equal(Visibility.Hidden, back.Visibility);
            Assert.True(toggle.Focus());
            void KeyOnToggle(Key key) => toggle.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(main), Environment.TickCount, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            KeyOnToggle(Key.PageDown); await UntilAsync(() => scroll.VerticalOffset > 0);
            KeyOnToggle(Key.Home); await UntilAsync(() => scroll.VerticalOffset == 0);
            var pointer = toggle.PointToScreen(new Point(toggle.ActualWidth / 2, toggle.ActualHeight / 2));
            Assert.True(SetCursorPos((int)pointer.X, (int)pointer.Y)); await UntilAsync(() => toggle.IsMouseOver);
            Input[] wheel = [new() { Data = new() { Mouse = new() { Flags = 0x0800, Data = unchecked((uint)-120) } } }];
            Assert.Equal(1u, SendInput(1, wheel, Marshal.SizeOf<Input>()));
            await UntilAsync(() => scroll.VerticalOffset > 0);
            scroll.ScrollToTop(); await UntilAsync(() => scroll.VerticalOffset == 0);
            scroll.ScrollToVerticalOffset(threshold - 10); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Assert.Equal(Visibility.Hidden, back.Visibility);
            scroll.ScrollToVerticalOffset(threshold + 150); await Task.Delay(65);
            Assert.InRange(back.Opacity, .01, .99);
            await UntilAsync(() => back.Opacity == 1);
            Assert.InRange(Math.Abs(host.TranslatePoint(new Point(), scroll).Y), 0, 1);
            scroll.ScrollToBottom(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Assert.InRange(Math.Abs(host.TranslatePoint(new Point(), scroll).Y), 0, 1);
            var hidden = (ItemsControl)main.FindName("HiddenCards");
            double lastCardBottom = Children<WorldTile>(hidden).Max(tile => tile.TranslatePoint(new Point(0, tile.ActualHeight), scroll).Y);
            Assert.True(lastCardBottom <= back.TranslatePoint(new Point(), scroll).Y, "The floating return button must not cover the last row.");
            Preview(main, "tracking-sticky-controls.png");
            await ClickAsync(back); await UntilAsync(() => scroll.VerticalOffset == 0 && back.Visibility == Visibility.Hidden);
            Assert.InRange(Math.Abs(host.TranslatePoint(new Point(), scroll).Y - threshold), 0, 1);
            scroll.ScrollToVerticalOffset(threshold + 150); await UntilAsync(() => back.Opacity == 1);
            scroll.ScrollToVerticalOffset(threshold - 10); await Task.Delay(65);
            Assert.InRange(back.Opacity, .01, .99); Assert.False(back.IsHitTestVisible);
            await UntilAsync(() => back.Visibility == Visibility.Hidden);
            // Closing the catalogue from its bottom keeps the same physical toggle available.
            scroll.ScrollToBottom(); await UntilAsync(() => back.Opacity == 1);
            await ClickAsync(toggle); await UntilAsync(() => !main.IsRevealingWorlds);
            Assert.False(main.Model.ShowAllWorlds); Assert.True(toggle.IsVisible);
            Assert.Equal(Visibility.Hidden, back.Visibility);
            main.Model.ShowAllWorlds = true; await UntilAsync(() => !main.IsRevealingWorlds);
            Motion.SetEnabled(main, false);
            scroll.ScrollToBottom(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Assert.Equal(1, back.Opacity);
            main.Model.SelectedSettingsTab = 1; await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Assert.False(host.IsVisible); Assert.Equal(Visibility.Hidden, back.Visibility);
        }
        finally { SetCursorPos(1, 1); await CloseAsync(main); await h.DisposeAsync(); await SetClientAnimationsAsync(previousAnimations); }
    });

    [Fact]
    public Task CenteredSettingsShowCompactInputsHealthAndAnOwnedSnapshotLog() => OnUiThreadAsync(async () =>
    {
        var h = Samples(80); var main = new MainWindow(h.Engine); main.Show(); Motion.SetEnabled(main, false);
        try
        {
            await UntilAsync(() => main.Model.Tracked.Count == 30); await ResizeAsync(main, 1920, 1040);
            Assert.Equal(16, main.Model.FreeWorldCount); Assert.Equal(5, main.Model.FavoriteWorldCount);
            Assert.True(((FlagGlyph)main.FindName("BestRegionFlag")).TranslatePoint(new Point(), main).X < ((TextBlock)main.FindName("BestWorldNumber")).TranslatePoint(new Point(), main).X);
            Assert.All(Children<WorldTile>(main), tile => Assert.InRange(tile.ActualWidth, 1, 240));
            var ordinary = Children<WorldTile>((ItemsControl)main.FindName("MemberCards")).First();
            var heart = (ToggleButton)ordinary.FindName("FavoriteToggle");
            var outline = Children<System.Windows.Shapes.Path>(heart).Single();
            Assert.Equal(Colors.Transparent, ((SolidColorBrush)outline.Fill).Color); Assert.Equal(1, outline.StrokeThickness);
            main.Model.ShowSettings = true; main.Model.SelectedSettingsTab = 1;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Assert.Null(((Border)main.FindName("ApplicationSheet")).Background); Assert.Null(((Border)main.FindName("AboutSheet")).Background);
            Assert.Empty(Children<Expander>(main));
            var cutoff = (TextBox)main.FindName("CutoffInput"); var interval = (TextBox)main.FindName("IntervalInput");
            Assert.InRange(cutoff.ActualWidth, 1, 180); Assert.Equal(cutoff.ActualWidth, interval.ActualWidth);
            Assert.InRange(Math.Abs(cutoff.TranslatePoint(new Point(), main).X - interval.TranslatePoint(new Point(), main).X), 0, 1);
            Assert.True(interval.TranslatePoint(new Point(), main).Y > cutoff.TranslatePoint(new Point(), main).Y + cutoff.ActualHeight);
            var health = (StackPanel)main.FindName("ConnectionHealth");
            Assert.Equal("OK", main.Model.HealthText);
            Assert.InRange(Math.Abs(health.TranslatePoint(new Point(), main).X + health.ActualWidth / 2 - main.ActualWidth / 2), 0, 10);
            Assert.Equal("Refresh worlds", ((Button)main.FindName("CheckConnectionButton")).Content);
            // ShowDialog runs a nested dispatcher; let the test inspect and close it before the click returns.
            var click = Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                ((Button)main.FindName("LatestLogButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent))));
            await UntilAsync(() => Application.Current.Windows.OfType<DiagnosticsWindow>().Any());
            var log = Application.Current.Windows.OfType<DiagnosticsWindow>().Single();
            Assert.Same(main, log.Owner); Assert.False(IsWindowEnabled(new WindowInteropHelper(main).Handle));
            var table = (DataGrid)log.FindName("DiagnosticTable");
            Assert.True(table.IsReadOnly); Assert.Equal(80, table.Items.Count);
            var rows = table.Items.Cast<DiagnosticRow>().ToArray();
            Assert.Equal(Enumerable.Range(301, 80), rows.Select(row => row.World));
            Assert.Equal("world-301.runescape.com", rows[0].Host);
            Preview(log, "latest-log.png");
            var horizontal = Children<ScrollBar>(table).Single(bar => bar.Orientation == Orientation.Horizontal);
            Assert.InRange(horizontal.ActualHeight, 1, 20); Assert.True(horizontal.ActualWidth > 100);
            var track = (Track)horizontal.Template.FindName("PART_Track", horizontal);
            Assert.Equal(Orientation.Horizontal, track.Orientation); Assert.False(track.IsDirectionReversed);
            double before = horizontal.Value;
            await ClickAsync(track.IncreaseRepeatButton);
            await UntilAsync(() => horizontal.Value > before);
            Preview(log, "latest-log-right-columns.png");
            // A compact dialog must keep the same columns reachable in both directions.
            log.Width = 520; log.Height = 440; log.UpdateLayout();
            Preview(log, "latest-log-compact.png", 2);
            await ClickAsync(track.DecreaseRepeatButton);
            await UntilAsync(() => horizontal.Value == 0);
            h.Probe.Return(new ProbeResult(ProbeStatus.TimedOut, Diagnostic: new("ICMP", "127.0.0.1", "TimedOut")));
            await h.Engine.RecheckAsync(); await UntilAsync(() => main.Model.HealthText == "80 connection issues");
            Assert.All(rows, row => Assert.EndsWith(" ms", row.Reading));
            log.Close(); await click; Assert.True(IsWindowEnabled(new WindowInteropHelper(main).Handle));
            Preview(main, "application-connection-issues.png");
            h.Probe.Return(new ProbeResult(ProbeStatus.Unreachable, Diagnostic: new("ICMP", "127.0.0.1", "DestinationHostUnreachable")));
            await h.Engine.RecheckAsync(); await UntilAsync(() => main.Model.HealthText == "80 connection failures");
            Assert.Equal("#FF9690", main.Model.HealthBrush);
            Preview(main, "application-connection-failures.png");
        }
        finally
        {
            foreach (var log in Application.Current.Windows.OfType<DiagnosticsWindow>().ToArray()) log.Close();
            SetCursorPos(1, 1); await CloseAsync(main); await h.DisposeAsync();
        }
    });
}
