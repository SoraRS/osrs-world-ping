using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OsrsWorldPing.Desktop;

public sealed class RevealItemsControl : ItemsControl
{
    private RectangleGeometry _revealClip = new(new Rect());
    private bool _retainRealizedContainers;
    public bool RetainRealizedContainers
    {
        get => _retainRealizedContainers;
        set
        {
            if (_retainRealizedContainers == value) return;
            _retainRealizedContainers = value;
            if (!value) InvalidatePanel(this);
        }
    }
    private static void InvalidatePanel(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is VirtualWorldColumnsPanel panel) panel.InvalidateMeasure(); else InvalidatePanel(child);
        }
    }
    // UIElement.Clip invalidates arrange on geometry changes. A retained layout clip avoids that callback per frame.
    public RectangleGeometry RevealClip
    {
        get => _revealClip;
        set { _revealClip = value; InvalidateArrange(); }
    }
    protected override Geometry GetLayoutClip(Size layoutSlotSize) => _revealClip;
}
