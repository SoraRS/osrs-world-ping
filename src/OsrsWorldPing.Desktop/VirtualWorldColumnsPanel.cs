using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using OsrsWorldPing.Core;

namespace OsrsWorldPing.Desktop;

// The outer page owns physical scrolling. Realize just its visible rows, retaining numeric column order.
public sealed class VirtualWorldColumnsPanel : VirtualizingPanel
{
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(int),
        typeof(VirtualWorldColumnsPanel), new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsMeasure), value => (int)value >= 1);
    public int Columns { get => (int)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    private const double RowHeight = 44;
    private ScrollViewer? _scroll;
    private bool _arranged;
    private int _focusRequest;
    public VirtualWorldColumnsPanel()
    {
        Loaded += (_, _) =>
        {
            for (DependencyObject? node = VisualTreeHelper.GetParent(this); node is not null; node = VisualTreeHelper.GetParent(node))
                if (node is ScrollViewer viewer) { _scroll = viewer; viewer.ScrollChanged += ScrollChanged; break; }
            InvalidateMeasure();
        };
        Unloaded += (_, _) => { if (_scroll is not null) _scroll.ScrollChanged -= ScrollChanged; _scroll = null; ++_focusRequest; };
    }
    private void ScrollChanged(object sender, ScrollChangedEventArgs args)
    {
        if (args.VerticalChange != 0 || args.ViewportHeightChange != 0 || args.ExtentHeightChange != 0) InvalidateMeasure();
    }
    protected override Size MeasureOverride(Size available)
    {
        var owner = ItemsControl.GetItemsOwner(this);
        if (owner is null) return new Size();
        int count = owner.Items.Count;
        double width = double.IsInfinity(available.Width) ? Columns * 280 : available.Width;
        int firstRow = 0, lastRow = 1;
        if (_arranged && _scroll?.Content is UIElement content)
        {
            double top = TranslatePoint(new Point(), content).Y;
            firstRow = Math.Max(0, (int)Math.Floor((_scroll.VerticalOffset - top) / RowHeight) - 2);
            lastRow = Math.Max(firstRow + 1, (int)Math.Ceiling((_scroll.VerticalOffset + _scroll.ViewportHeight - top) / RowHeight) + 2);
        }
        var wanted = new SortedSet<int>();
        for (int column = 0; column < Columns; column++)
        {
            int rows = count / Columns + (column < count % Columns ? 1 : 0);
            int start = column * (count / Columns) + Math.Min(column, count % Columns);
            for (int row = firstRow; row <= Math.Min(lastRow, rows - 1); row++) wanted.Add(start + row);
        }
        var generator = ItemContainerGenerator;
        for (int child = InternalChildren.Count - 1; child >= 0; child--)
        {
            var view = InternalChildren[child];
            int index = owner.ItemContainerGenerator.IndexFromContainer(view);
            if (index >= 0 && view.IsKeyboardFocusWithin) wanted.Add(index);
            if (wanted.Contains(index)) continue;
            var position = new GeneratorPosition(child, 0);
            if (generator is IRecyclingItemContainerGenerator recycling) recycling.Recycle(position, 1); else generator.Remove(position, 1);
            RemoveInternalChildRange(child, 1);
        }
        foreach (int index in wanted)
        {
            var position = generator.GeneratorPositionFromIndex(index);
            int childIndex = position.Index + (position.Offset == 0 ? 0 : 1);
            using (generator.StartAt(position, GeneratorDirection.Forward, true))
            {
                var child = (UIElement)generator.GenerateNext(out _);
                if (!ReferenceEquals(VisualTreeHelper.GetParent(child), this))
                { InsertInternalChild(childIndex, child); generator.PrepareItemContainer(child); }
                child.Measure(new Size(width / Columns, RowHeight));
            }
        }
        return new Size(width, Math.Ceiling((double)count / Columns) * RowHeight);
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        var owner = ItemsControl.GetItemsOwner(this);
        if (owner is null) return finalSize;
        foreach (UIElement child in InternalChildren)
        {
            int index = owner.ItemContainerGenerator.IndexFromContainer(child);
            if (index < 0) continue;
            var (row, column) = WorldColumnLayout.Position(index, owner.Items.Count, Columns);
            child.Arrange(new Rect(column * finalSize.Width / Columns, row * RowHeight, finalSize.Width / Columns, RowHeight));
        }
        _arranged = true;
        return finalSize;
    }
    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
    {
        base.OnItemsChanged(sender, args);
        ItemContainerGenerator.RemoveAll();
        RemoveInternalChildRange(0, InternalChildren.Count);
        InvalidateMeasure();
    }
    protected override void BringIndexIntoView(int index)
    {
        var owner = ItemsControl.GetItemsOwner(this);
        if (owner is null || index < 0 || index >= owner.Items.Count || _scroll?.Content is not UIElement content) return;
        double top = TranslatePoint(new Point(), content).Y + WorldColumnLayout.Position(index, owner.Items.Count, Columns).Row * RowHeight;
        if (top < _scroll.VerticalOffset) _scroll.ScrollToVerticalOffset(top);
        else if (top + RowHeight > _scroll.VerticalOffset + _scroll.ViewportHeight)
            _scroll.ScrollToVerticalOffset(top + RowHeight - _scroll.ViewportHeight);
        InvalidateMeasure();
    }
    protected override void OnPreviewKeyDown(KeyEventArgs args)
    {
        base.OnPreviewKeyDown(args);
        if (args.Key != Key.Tab || (Keyboard.Modifiers & ~ModifierKeys.Shift) != ModifierKeys.None || args.OriginalSource is not DependencyObject source) return;
        var owner = ItemsControl.GetItemsOwner(this);
        if (owner is null || ItemsControl.ContainerFromElement(owner, source) is not DependencyObject container) return;
        int next = owner.ItemContainerGenerator.IndexFromContainer(container) + (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1);
        if (next < 0 || next >= owner.Items.Count) return;
        args.Handled = true; BringIndexIntoView(next);
        int request = ++_focusRequest;
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            if (request != _focusRequest || !IsVisible) return;
            UpdateLayout();
            if (owner.ItemContainerGenerator.ContainerFromIndex(next) is UIElement view)
                view.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }));
    }
}
