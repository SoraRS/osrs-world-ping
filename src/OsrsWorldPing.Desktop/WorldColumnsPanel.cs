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
    private double _rowHeight;
    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? Columns * 240 : availableSize.Width;
        double pitch = width / Columns;
        ColumnPitch = pitch;
        double occupiedWidth = 0;
        _rowHeight = 0;
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i];
            child.Measure(new Size(pitch, double.PositiveInfinity));
            _rowHeight = Math.Max(_rowHeight, child.DesiredSize.Height);
            var (_, column) = WorldColumnLayout.Position(i, InternalChildren.Count, Columns);
            // Trim the unused tail of the last occupied column, including when fewer columns are populated.
            // Numeric fields reserve their width. Failure labels can grow and recover without leaving a stale gutter.
            occupiedWidth = Math.Max(occupiedWidth, column * pitch + child.DesiredSize.Width);
        }
        return new Size(occupiedWidth, Math.Ceiling((double)InternalChildren.Count / Columns) * _rowHeight);
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
