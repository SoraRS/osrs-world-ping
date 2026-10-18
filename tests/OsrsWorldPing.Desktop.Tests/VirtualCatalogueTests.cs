using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using OsrsWorldPing.Core;
using OsrsWorldPing.Desktop;

namespace OsrsWorldPing.Desktop.Tests;

public sealed partial class DesktopTests
{
    [Fact]
    public Task FullCatalogueRecyclesCardsWithoutLosingWorldsColumnOrderOrKeyboardAccess() => OnUiThreadAsync(async () =>
    {
        var h = Samples(321); var main = new MainWindow(h.Engine); main.Show(); Motion.SetEnabled(main, false);
        try
        {
            await UntilAsync(() => main.Model.Tracked.Count == 30); await ResizeAsync(main, 1920, 1040);
            main.Model.ShowSettings = true; main.Model.ShowAllWorlds = true;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); main.UpdateLayout();
            var scroll = (ScrollViewer)main.FindName("PageScroll");
            var cards = (ItemsControl)main.FindName("HiddenCards");
            Assert.Equal(286, cards.Items.Count);
            int probes = h.Probe.Hosts.Count;
            var seen = new HashSet<int>();
            double extent = scroll.ScrollableHeight;
            for (double offset = 0; offset <= extent + scroll.ViewportHeight; offset += scroll.ViewportHeight / 2)
            {
                scroll.ScrollToVerticalOffset(Math.Min(offset, extent));
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); main.UpdateLayout();
                var tiles = Children<WorldTile>(cards).ToArray();
                Assert.InRange(tiles.Length, 1, 90);
                Assert.Equal(tiles.Length, tiles.Select(t => ((WorldCardModel)t.DataContext).Id).Distinct().Count());
                foreach (var tile in tiles)
                {
                    var model = (WorldCardModel)tile.DataContext; seen.Add(model.Id);
                    int index = main.Model.Hidden.IndexOf(model);
                    var (row, column) = WorldColumnLayout.Position(index, cards.Items.Count, main.ColumnCount);
                    var at = tile.TranslatePoint(new Point(), cards);
                    Assert.InRange(Math.Abs(at.Y - row * 44), 0, 1);
                    Assert.InRange(Math.Abs(at.X - Children<VirtualWorldColumnsPanel>(cards).Single().ColumnOffset - column * cards.ActualWidth / main.ColumnCount), 0, 1);
                }
            }
            Assert.Equal(main.Model.Hidden.Select(w => w.Id), seen.OrderBy(id => id));
            // Tab from the bottom of the first column must reach the unrendered top of the next column.
            scroll.ScrollToBottom(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); main.UpdateLayout();
            int lastInFirstColumn = (int)Math.Ceiling((double)cards.Items.Count / main.ColumnCount) - 1;
            var last = Children<WorldTile>(cards).Single(t => ReferenceEquals(t.DataContext, main.Model.Hidden[lastInFirstColumn]));
            var heart = (ToggleButton)last.FindName("FavoriteToggle");
            Assert.True(heart.Focus());
            heart.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(main), Environment.TickCount, Key.Tab)
            { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            await UntilAsync(() => Keyboard.FocusedElement is ToggleButton next &&
                ReferenceEquals(next.DataContext, main.Model.Hidden[lastInFirstColumn + 1]));
            Assert.True(scroll.VerticalOffset < extent - 100);
            Assert.Equal(probes, h.Probe.Hosts.Count);
            Preview(main, "virtual-catalogue-keyboard.png");
        }
        finally { await CloseAsync(main); await h.DisposeAsync(); }
    });
}
