using System.Text.Json;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using OsrsWorldPing.Core;
using OsrsWorldPing.Desktop;

namespace OsrsWorldPing.Desktop.Tests;

public sealed partial class DesktopTests
{
    [Fact]
    public Task ExpandedCatalogueKeepsItsCenteredFootprintAsCardsRecycle() => OnUiThreadAsync(async () =>
    {
        var h = Samples(100); h.Catalogue.Worlds = h.Catalogue.Worlds.Select(world => world with { Activity = "-" }).ToArray();
        var main = new MainWindow(h.Engine); main.Show(); Motion.SetEnabled(main, false);
        try
        {
            await UntilAsync(() => main.Model.Tracked.Count == 30);
            main.Model.ShowSettings = true; main.Model.ShowAllWorlds = true;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); main.UpdateLayout();
            var scroll = (ScrollViewer)main.FindName("PageScroll");
            var cards = (ItemsControl)main.FindName("HiddenCards");
            var panel = Children<VirtualWorldColumnsPanel>(cards).Single();
            Assert.True(panel.ColumnOffset > 20);
            double offset = panel.ColumnOffset;
            foreach (bool bottom in new[] { true, false, true })
            {
                if (bottom) scroll.ScrollToBottom(); else scroll.ScrollToTop();
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); main.UpdateLayout();
                Assert.Equal(offset, panel.ColumnOffset);
                Assert.InRange(Children<WorldTile>(cards).Count(), 1, 65);
            }
            Preview(main, "fixed-expanded-centered.png");
        }
        finally { await CloseAsync(main); await h.DisposeAsync(); }
    });

    [Fact]
    public Task FixedPrototypeCentersOccupiedWorldsCountsAndUnclippedFrameTabs() => OnUiThreadAsync(async () =>
    {
        var h = Samples(30);
        h.Store.Settings = new AppSettings();
        h.Catalogue.Worlds = Enumerable.Range(301, 29).Select(id => new WorldInfo(id, 1, $"world-{id}.runescape.com", "-", 0, 100))
            .Append(new(469, 0, "world-469.runescape.com", "LMS Casual", 0, 100)).ToArray();
        h.Probe.Return(new(ProbeStatus.Success, 20));
        var main = new MainWindow(h.Engine); main.Show(); Motion.SetEnabled(main, false);
        try
        {
            await UntilAsync(() => main.Model.Tracked.Count == 30); main.UpdateLayout();
            Assert.Equal(WindowState.Normal, main.WindowState); Assert.Equal(ResizeMode.CanMinimize, main.ResizeMode);
            Assert.Equal(768, main.Width); Assert.Equal(930, main.Height);
            Assert.Equal(main.MinWidth, main.MaxWidth); Assert.Equal(main.MinHeight, main.MaxHeight);
            var hwnd = new WindowInteropHelper(main).Handle;
            Assert.Equal(0L, GetWindowLongPtr(hwnd, -16).ToInt64() & (0x00040000 | 0x00010000));
            main.WindowState = WindowState.Maximized; Assert.Equal(WindowState.Normal, main.WindowState);
            SystemCommands.MaximizeWindow(main); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Assert.Equal(WindowState.Normal, main.WindowState);
            Assert.Equal("★ Members 29", ((TextBlock)main.FindName("MembersHeader")).Text);
            Assert.Equal("☆ Free to play 1", ((TextBlock)main.FindName("FreeToPlayHeader")).Text);
            Assert.DoesNotContain(Children<TextBlock>(main), text => text.Text == "WORLDS");
            var sections = (StackPanel)main.FindName("WorldSections");
            var sectionBounds = BoundsIn(sections, main);
            Assert.InRange(Math.Abs(sectionBounds.Left - (main.ActualWidth - sectionBounds.Right)), 0, 2);
            var cards = Children<WorldTile>((ItemsControl)main.FindName("MemberCards")).ToArray();
            double inkLeft = cards.Min(tile => BoundsIn(Children<System.Windows.Shapes.Path>((ToggleButton)tile.FindName("FavoriteToggle")).Single(), main).Left);
            double inkRight = cards.Max(tile => BoundsIn((TextBlock)tile.FindName("LatencyValue"), main).Right);
            Assert.InRange(Math.Abs(inkLeft - (main.ActualWidth - inkRight)), 0, 2);
            Assert.True(((ScrollViewer)main.FindName("PageScroll")).ScrollableHeight < 1);
            Preview(main, "fixed-dashboard-768x930.png");
            var positions = cards.Select(tile => tile.TranslatePoint(new Point(), main)).ToArray();
            foreach (long reading in new long[] { 1, 29, 1999, 20 })
            {
                h.Probe.Return(new(ProbeStatus.Success, reading)); await h.Engine.CheckTrackedWorldsAsync();
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                Assert.Equal(positions, cards.Select(tile => tile.TranslatePoint(new Point(), main)));
            }
            main.Model.ShowSettings = true; await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var title = (Grid)main.FindName("TitleBar"); var tabs = (TabControl)main.FindName("SettingsTabs");
            var titleBounds = BoundsIn(title, main);
            var gear = (System.Windows.Shapes.Path)main.FindName("Gear"); var gearBounds = BoundsIn(gear, main);
            Assert.True(gearBounds.Top > titleBounds.Bottom); Assert.True(gearBounds.Left > main.ActualWidth * .8);
            Assert.Equal(0, ((SolidColorBrush)gear.Fill).Color.A);
            Assert.IsType<LinearGradientBrush>(((System.Windows.Shapes.Path)main.FindName("GearInnerHighlight")).Stroke);
            var measuredTabs = new List<Rect>();
            for (int i = 0; i < 3; i++)
            {
                var tab = (TabItem)tabs.Items[i]; var label = Children<TextBlock>(tab).Single();
                var labelBounds = BoundsIn(label, main); measuredTabs.Add(labelBounds);
                Assert.True(labelBounds.Top >= titleBounds.Top && labelBounds.Bottom <= titleBounds.Bottom - 1);
                var at = tab.PointToScreen(new Point(tab.ActualWidth / 2, tab.ActualHeight / 2));
                Assert.True(SetCursorPos((int)at.X, (int)at.Y));
                await UntilAsync(() => tab.IsMouseOver);
                Input[] input = [new() { Data = new() { Mouse = new() { Flags = 0x0002 } } }, new() { Data = new() { Mouse = new() { Flags = 0x0004 } } }];
                Assert.Equal(2u, SendInput(2, input, Marshal.SizeOf<Input>()));
                await Task.Delay(60); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                await UntilAsync(() => tabs.SelectedIndex == i && !main.IsNavigating);
                Preview(main, $"fixed-settings-{i}-768x930.png");
            }
            string directory = Path.Combine(AppContext.BaseDirectory, "ui-preview"); Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(Path.Combine(directory, "fixed-layout-measurements.json"), JsonSerializer.Serialize(new
            { main.ActualWidth, main.ActualHeight, InkLeft = inkLeft, InkRightMargin = main.ActualWidth - inkRight, Tabs = measuredTabs, Gear = gearBounds }));
        }
        finally { SetCursorPos(1, 1); await CloseAsync(main); await h.DisposeAsync(); }
    });
}
