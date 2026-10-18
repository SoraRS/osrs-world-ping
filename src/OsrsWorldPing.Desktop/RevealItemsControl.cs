using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OsrsWorldPing.Desktop;

public sealed class RevealItemsControl : ItemsControl
{
    private RectangleGeometry _revealClip = new(new Rect());
    // UIElement.Clip invalidates arrange on geometry changes. A retained layout clip avoids that callback per frame.
    public RectangleGeometry RevealClip
    {
        get => _revealClip;
        set { _revealClip = value; InvalidateArrange(); }
    }
    protected override Geometry GetLayoutClip(Size layoutSlotSize) => _revealClip;
}
