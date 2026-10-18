using System;
using System.Windows;
using System.Windows.Controls;
using OsrsWorldPing.Core;

namespace OsrsWorldPing.Desktop;

public sealed class WorldColumnsPanel : Panel
{
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(int),
        typeof(WorldColumnsPanel), new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsMeasure), value => (int)value >= 1);
    public int Columns { get => (int)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public double ColumnPitch { get; private set; }
    private double _rowHeight, _occupiedWidth;
    private (object? Context, WorldInfo? World)[] _layoutItems = [];
    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? Columns * 240 : availableSize.Width;
        double pitch = width / Columns;
        var items = InternalChildren.Cast<FrameworkElement>().Select(child =>
            ((object?)child.DataContext, (child.DataContext as WorldCardModel)?.LayoutWorld)).ToArray();
        // Do not recenter on ordinary numeric updates. New membership/catalogue data remeasures the footprint.
        if (pitch != ColumnPitch || !_layoutItems.SequenceEqual(items)) _occupiedWidth = 0;
        _layoutItems = items; ColumnPitch = pitch;
        _rowHeight = 0;
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i];
            child.Measure(new Size(pitch, double.PositiveInfinity));
            _rowHeight = Math.Max(_rowHeight, child.DesiredSize.Height);
            var (_, column) = WorldColumnLayout.Position(i, InternalChildren.Count, Columns);
            // Trim the unused tail of the last occupied column, including when fewer columns are populated.
            // Grow for a longer failure label, but do not shrink/reflow repeatedly on recovery.
            _occupiedWidth = Math.Max(_occupiedWidth, column * pitch + child.DesiredSize.Width);
        }
        return new Size(_occupiedWidth, Math.Ceiling((double)InternalChildren.Count / Columns) * _rowHeight);
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var (row, column) = WorldColumnLayout.Position(i, InternalChildren.Count, Columns);
            InternalChildren[i].Arrange(new Rect(column * ColumnPitch, row * _rowHeight, ColumnPitch, _rowHeight));
        }
        return finalSize;
    }
}
